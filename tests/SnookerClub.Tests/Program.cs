using System.Diagnostics;
using System.Reflection;
using Microsoft.Data.Sqlite;
using SnookerClub.Core;
using SnookerClub.Infrastructure;

// A dependency-light executable suite. Each check uses a separate temporary database.
// Run with dotnet run --project tests/SnookerClub.Tests -c Release, not dotnet test.
if (args is ["crash-worker", var workerPath])
{
    var store = new SqliteClubStore(workerPath);
    store.Initialize();
    store.SaveSettings(new("Crash test club", 20000, 1000, 100000));
    store.StartSession(new(Guid.NewGuid(), "table-1", "Crash test customer", GameMode.Century, 1000));
    Console.WriteLine("COMMITTED");
    Console.Out.Flush();
    await Task.Delay(Timeout.Infinite);
    return 0;
}

var tests = new (string Name, Action<TestDatabase> Check)[]
{
    ("Migration seeds eight bookable locations and leaves rates unset", db =>
    {
        Check.Equal(8, db.Store.GetLocations().Count);
        Check.Equal(1, db.Store.GetLocations().Count(x => x.IsPrivateRoom));
        Check.True(db.Store.GetLocations().All(x => !x.Name.Contains("Counter")));
        Check.Equal<long?>(null, db.Store.GetSettings().BilliardsRate);
        Check.Equal(1L, db.Scalar("PRAGMA user_version;"));
        Check.Equal("wal", db.Value("PRAGMA journal_mode;"));
    }),
    ("Migrations can rerun without deleting saved sessions", db =>
    {
        var saved = db.Start(); db.Store.Initialize();
        Check.Equal(saved, db.Store.GetActiveSessions().Single());
        Check.Equal(8, db.Store.GetLocations().Count);
    }),
    ("All game modes persist their customer, rate, UTC time, and start event", db =>
    {
        db.Rates();
        var requests = new[]
        {
            new StartSessionRequest(Guid.NewGuid(), "table-1", "  Test A  ", GameMode.Billiards, 20000),
            new StartSessionRequest(Guid.NewGuid(), "table-2", "Test B", GameMode.Century, 1000),
            new StartSessionRequest(Guid.NewGuid(), "private-room", "Test C", GameMode.PrivateRoom, 100000)
        };
        var sessions = requests.Select(db.Store.StartSession).ToArray();
        var reopened = new SqliteClubStore(db.Path).GetActiveSessions();
        Check.Equal(3, reopened.Count);
        foreach (var saved in sessions)
        {
            Check.Equal(saved, reopened.Single(x => x.Id == saved.Id));
            Check.Equal(TimeSpan.Zero, saved.StartedAt.Offset);
            Check.Equal(2, saved.PlayerCount);
            Check.Equal("session_started", db.Store.GetEvents(saved.Id).Single().Kind);
        }
        Check.Equal("Test A", sessions[0].CustomerName);
    }),
    ("Names are required and limited without inserting anything", db =>
    {
        db.Rates();
        foreach (var name in new[] { " ", new string('a', 121) })
            Check.Throws<SessionRuleException>(() => db.Store.StartSession(db.Request(name: name)));
        Check.Equal(0L, db.Scalar("SELECT count(*) FROM sessions;"));
        Check.Equal(0L, db.Scalar("SELECT count(*) FROM session_events;"));
    }),
    ("Unknown locations and invalid game/location combinations are rejected", db =>
    {
        db.Rates();
        Check.Throws<SessionRuleException>(() => db.Store.StartSession(db.Request(location: "counter")));
        Check.Throws<SessionRuleException>(() => db.Store.StartSession(db.Request(location: "private-room")));
        Check.Throws<SessionRuleException>(() => db.Store.StartSession(db.Request(mode: GameMode.PrivateRoom)));
        Check.Throws<SessionRuleException>(() => db.Store.StartSession(db.Request(mode: (GameMode)99)));
        Check.Throws<SessionRuleException>(() => db.Store.StartSession(db.Request() with { RequestId = Guid.Empty }));
        Check.Equal(0L, db.Scalar("SELECT count(*) FROM sessions;"));
    }),
    ("Missing or changed rates cannot silently start a session", db =>
    {
        Check.Throws<SessionRuleException>(() => db.Store.StartSession(db.Request()));
        db.Rates();
        Check.Throws<SessionRuleException>(() => db.Store.StartSession(db.Request() with { ExpectedRateMinorUnits = 1 }));
        Check.Equal(0, db.Store.GetActiveSessions().Count);
    }),
    ("A second session cannot replace an occupied table", db =>
    {
        var first = db.Start();
        Check.Throws<SessionRuleException>(() => db.Store.StartSession(db.Request(name: "Different customer")));
        Check.Equal(first, db.Store.GetActiveSessions().Single());
        Check.Equal(1, db.Store.GetEvents(first.Id).Count);
    }),
    ("Retrying the same request saves exactly one session and event", db =>
    {
        db.Rates(); var request = db.Request();
        var first = db.Store.StartSession(request);
        Check.Equal(first, db.Store.StartSession(request));
        Check.Equal(1, db.Store.GetActiveSessions().Count);
        Check.Equal(1, db.Store.GetEvents(first.Id).Count);
        Check.Throws<SessionRuleException>(() => db.Store.StartSession(request with { CustomerName = "Changed" }));
    }),
    ("Price changes do not change saved rates, including idempotent retries", db =>
    {
        db.Rates(); var request = db.Request(); var first = db.Store.StartSession(request);
        db.Store.SaveSettings(new("New club label", 21000, 1000, 100000));
        Check.Equal(first, db.Store.StartSession(request));
        Check.Equal(20000L, db.Store.GetActiveSessions().Single().RateMinorUnits);
        var second = db.Store.StartSession(db.Request(location: "table-2") with { ExpectedRateMinorUnits = 21000 });
        Check.Equal(21000L, second.RateMinorUnits);
    }),
    ("Recovery marking preserves sessions and records one pending review event", db =>
    {
        var first = db.Start();
        db.Store.MarkReopenedSessionsForReview(); db.Store.MarkReopenedSessionsForReview();
        var recovered = new SqliteClubStore(db.Path).GetActiveSessions().Single();
        Check.Equal(first with { NeedsRecoveryReview = true }, recovered);
        Check.Equal(2, db.Store.GetEvents(first.Id).Count);
        Check.Equal(1, db.Store.GetEvents(first.Id).Count(x => x.Kind == "recovery_required"));
        Check.Equal(first.StartedAt, db.Store.GetEvents(first.Id).First().RecordedAt);
    }),
    ("Failure to write the start event rolls back the entire session", db =>
    {
        db.Rates();
        db.Execute("CREATE TRIGGER fail_event BEFORE INSERT ON session_events BEGIN SELECT RAISE(ABORT,'Injected failure'); END;");
        Check.Throws<SqliteException>(() => db.Store.StartSession(db.Request()));
        Check.Equal(0L, db.Scalar("SELECT count(*) FROM sessions;"));
        Check.Equal(0L, db.Scalar("SELECT count(*) FROM session_events;"));
    }),
    ("Failed recovery event does not partially mark sessions", db =>
    {
        var first = db.Start();
        db.Execute("CREATE TRIGGER fail_recovery BEFORE INSERT ON session_events WHEN NEW.kind='recovery_required' BEGIN SELECT RAISE(ABORT,'Injected failure'); END;");
        Check.Throws<SqliteException>(() => db.Store.MarkReopenedSessionsForReview());
        Check.Equal(first, db.Store.GetActiveSessions().Single());
        Check.Equal(1, db.Store.GetEvents(first.Id).Count);
    }),
    ("Database enforces occupied-location uniqueness independently", db =>
    {
        db.Start();
        Check.Throws<SqliteException>(() => db.Execute("""
            INSERT INTO sessions(id,request_id,location_id,customer_name,mode,started_at,rate_minor_units)
            VALUES('other','other-request','table-1','Test duplicate','Billiards','2026-10-20T00:00:00Z',20000);
            """));
        Check.Equal(1, db.Store.GetActiveSessions().Count);
    }),
    ("Database enforces game-location matching independently", db =>
    {
        Check.Throws<SqliteException>(() => db.Execute("""
            INSERT INTO sessions(id,request_id,location_id,customer_name,mode,started_at,rate_minor_units)
            VALUES('other','other-request','private-room','Test mismatch','Billiards','2026-10-20T00:00:00Z',20000);
            """));
    }),
    ("A newer schema is refused without resetting records", db =>
    {
        var first = db.Start(); db.Execute("PRAGMA user_version=999;");
        Check.Throws<InvalidOperationException>(db.Store.Initialize);
        Check.Equal(999L, db.Scalar("PRAGMA user_version;"));
        Check.Equal(first, db.Store.GetActiveSessions().Single());
    }),
    ("Invalid settings do not overwrite the previous settings", db =>
    {
        db.Rates(); var original = db.Store.GetSettings();
        Check.Throws<SessionRuleException>(() => db.Store.SaveSettings(original with { CenturyRate = -1 }));
        Check.Throws<SessionRuleException>(() => db.Store.SaveSettings(original with { ClubName = " " }));
        Check.Equal(original, db.Store.GetSettings());
    }),
    ("Money parsing stores exact minor units and rejects silent rounding", _ =>
    {
        Check.Equal<long?>(12345, Money.ParseOptionalRate("123.45"));
        Check.Equal<long?>(0, Money.ParseOptionalRate("0"));
        Check.Equal<long?>(null, Money.ParseOptionalRate(" "));
        foreach (var value in new[] { "-1", "1.234", "NaN", "1,000", "1e3", "99999999999999999999" })
            Check.Throws<SessionRuleException>(() => Money.ParseOptionalRate(value));
        Check.Equal("123.45", Money.Format(12345));
    }),
    ("Customer text is treated as data, not SQL", db =>
    {
        db.Rates(); var name = "O'Brien'); DROP TABLE sessions; --";
        db.Store.StartSession(db.Request(name: name));
        Check.Equal(name, db.Store.GetActiveSessions().Single().CustomerName);
    }),
    ("Concurrent attempts create only one session for a location", db =>
    {
        db.Rates();
        var results = Task.WhenAll(Enumerable.Range(0, 2).Select(_ => Task.Run(() =>
        {
            try { new SqliteClubStore(db.Path).StartSession(db.Request()); return true; }
            catch (SessionRuleException) { return false; }
        }))).GetAwaiter().GetResult();
        Check.Equal(1, results.Count(x => x));
        Check.Equal(1L, db.Scalar("SELECT count(*) FROM session_events;"));
    }),
    ("Killed writer process retains its committed session on restart", db =>
    {
        var info = new ProcessStartInfo(Environment.ProcessPath!) { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
        if (System.IO.Path.GetFileNameWithoutExtension(Environment.ProcessPath) == "dotnet")
            info.ArgumentList.Add(Assembly.GetExecutingAssembly().Location);
        info.ArgumentList.Add("crash-worker"); info.ArgumentList.Add(db.Path);
        using var child = Process.Start(info) ?? throw new Exception("Could not start crash worker.");
        try
        {
            var line = child.StandardOutput.ReadLineAsync().WaitAsync(TimeSpan.FromSeconds(30)).GetAwaiter().GetResult();
            if (line != "COMMITTED") throw new Exception("Worker did not commit: " + child.StandardError.ReadToEnd());
            child.Kill(entireProcessTree: true); child.WaitForExit();
            var restarted = new SqliteClubStore(db.Path); restarted.Initialize(); restarted.MarkReopenedSessionsForReview();
            var session = restarted.GetActiveSessions().Single();
            Check.Equal("Crash test customer", session.CustomerName);
            Check.Equal(GameMode.Century, session.Mode);
            Check.True(session.NeedsRecoveryReview);
            Check.Equal(1, restarted.GetEvents(session.Id).Count(x => x.Kind == "session_started"));
        }
        finally { if (!child.HasExited) { child.Kill(entireProcessTree: true); child.WaitForExit(); } }
    })
};
var failures = 0;
foreach (var (name, check) in tests)
{
    try { using var db = new TestDatabase(); check(db); Console.WriteLine($"PASS {name}"); }
    catch (Exception ex) { failures++; Console.Error.WriteLine($"FAIL {name}\n{ex}"); }
}
Console.WriteLine($"{tests.Length - failures}/{tests.Length} checks passed.");
return failures == 0 ? 0 : 1;

sealed class TestDatabase : IDisposable
{
    private readonly string directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "snooker-tests-" + Guid.NewGuid());
    public string Path => System.IO.Path.Combine(directory, "club.db");
    public SqliteClubStore Store { get; }
    public TestDatabase() { Store = new(Path); Store.Initialize(); }
    public void Rates() => Store.SaveSettings(new("Test club", 20000, 1000, 100000));
    public StartSessionRequest Request(string location = "table-1", string name = "Test customer", GameMode mode = GameMode.Billiards)
        => new(Guid.NewGuid(), location, name, mode, 20000);
    public ActiveSession Start() { Rates(); return Store.StartSession(Request()); }
    public object? Value(string sql)
    {
        using var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = Path, Pooling = false, ForeignKeys = true }.ToString());
        connection.Open(); using var command = connection.CreateCommand(); command.CommandText = sql;
        return command.ExecuteScalar();
    }
    public long Scalar(string sql) => Convert.ToInt64(Value(sql));
    public void Execute(string sql) => Value(sql);
    public void Dispose() => Directory.Delete(directory, recursive: true);
}
static class Check
{
    public static void True(bool actual) { if (!actual) throw new Exception("Expected true."); }
    public static void Equal<T>(T expected, T actual)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual)) throw new Exception($"Expected {expected}, got {actual}.");
    }
    public static void Throws<T>(Action action) where T : Exception
    {
        try { action(); } catch (T) { return; }
        throw new Exception($"Expected {typeof(T).Name}.");
    }
}
