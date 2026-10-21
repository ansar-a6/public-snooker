using System.Globalization;
using Microsoft.Data.Sqlite;
using SnookerClub.Core;

namespace SnookerClub.Infrastructure;

public sealed class SqliteClubStore : IClubStore
{
    private readonly string databasePath;
    private readonly TimeProvider clock;
    public SqliteClubStore(string databasePath, TimeProvider? clock = null)
    {
        this.databasePath = Path.GetFullPath(databasePath);
        this.clock = clock ?? TimeProvider.System;
    }

    private SqliteConnection Open()
    {
        var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = databasePath, ForeignKeys = true, Pooling = false, DefaultTimeout = 5
        }.ToString());
        try
        {
            connection.Open();
            using var command = Command(connection, null, "PRAGMA synchronous=FULL;");
            command.ExecuteNonQuery();
            return connection;
        }
        catch { connection.Dispose(); throw; }
    }

    private static SqliteCommand Command(SqliteConnection connection, SqliteTransaction? transaction,
        string sql, params (string Name, object? Value)[] parameters)
    {
        var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        foreach (var (name, value) in parameters) command.Parameters.AddWithValue(name, value ?? DBNull.Value);
        return command;
    }

    public void Initialize()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);
        using var connection = Open();
        using var versionQuery = Command(connection, null, "PRAGMA user_version;");
        var version = Convert.ToInt32(versionQuery.ExecuteScalar());
        if (version > 1) throw new InvalidOperationException("This database was created by a newer app. Install that version; existing data has not been reset.");
        using (var wal = Command(connection, null, "PRAGMA journal_mode=WAL;")) wal.ExecuteScalar();
        if (version == 1) return;
        using var transaction = connection.BeginTransaction();
        using var migration = Command(connection, transaction, """
            CREATE TABLE locations (
                id TEXT PRIMARY KEY, name TEXT NOT NULL,
                is_private INTEGER NOT NULL CHECK(is_private IN (0,1))
            );
            CREATE TABLE settings (
                id INTEGER PRIMARY KEY CHECK(id=1), club_name TEXT NOT NULL CHECK(length(trim(club_name)) BETWEEN 1 AND 120),
                billiards_rate INTEGER CHECK(billiards_rate>=0),
                century_rate INTEGER CHECK(century_rate>=0), room_rate INTEGER CHECK(room_rate>=0)
            );
            INSERT INTO settings(id,club_name) VALUES(1,'Snooker Club');
            CREATE TABLE sessions (
                id TEXT PRIMARY KEY, request_id TEXT NOT NULL UNIQUE,
                location_id TEXT NOT NULL REFERENCES locations(id),
                customer_name TEXT NOT NULL CHECK(length(trim(customer_name)) BETWEEN 1 AND 120),
                mode TEXT NOT NULL CHECK(mode IN ('Billiards','Century','PrivateRoom')),
                started_at TEXT NOT NULL, rate_minor_units INTEGER NOT NULL CHECK(rate_minor_units>=0),
                player_count INTEGER NOT NULL DEFAULT 2 CHECK(player_count=2),
                status TEXT NOT NULL DEFAULT 'ACTIVE' CHECK(status IN ('ACTIVE','ENDED_REVIEW','ENDED_UNPAID','CLOSED')),
                needs_recovery INTEGER NOT NULL DEFAULT 0 CHECK(needs_recovery IN (0,1))
            );
            CREATE UNIQUE INDEX one_occupied_session ON sessions(location_id)
                WHERE status IN ('ACTIVE','ENDED_REVIEW');
            CREATE TRIGGER valid_session_location BEFORE INSERT ON sessions
            WHEN (SELECT is_private FROM locations WHERE id=NEW.location_id) != (NEW.mode='PrivateRoom')
            BEGIN SELECT RAISE(ABORT,'Game mode does not match location'); END;
            CREATE TABLE session_events (
                id TEXT PRIMARY KEY, session_id TEXT NOT NULL REFERENCES sessions(id),
                kind TEXT NOT NULL, recorded_at TEXT NOT NULL
            );
            CREATE INDEX events_by_session ON session_events(session_id, recorded_at);
            PRAGMA user_version=1;
            """);
        migration.ExecuteNonQuery();
        for (var number = 1; number <= 7; number++)
        {
            using var seed = Command(connection, transaction,
                "INSERT INTO locations VALUES($id,$name,0);", ("$id", $"table-{number}"), ("$name", $"Table {number}"));
            seed.ExecuteNonQuery();
        }
        using (var room = Command(connection, transaction, "INSERT INTO locations VALUES('private-room','Private Room',1);"))
            room.ExecuteNonQuery();
        transaction.Commit();
    }

    public IReadOnlyList<Location> GetLocations()
    {
        using var connection = Open();
        using var command = Command(connection, null, "SELECT id,name,is_private FROM locations ORDER BY id;");
        using var reader = command.ExecuteReader();
        var locations = new List<Location>();
        while (reader.Read()) locations.Add(new(reader.GetString(0), reader.GetString(1), reader.GetBoolean(2)));
        return locations;
    }

    public ClubSettings GetSettings()
    {
        using var connection = Open();
        return ReadSettings(connection, null);
    }
    private static ClubSettings ReadSettings(SqliteConnection connection, SqliteTransaction? transaction)
    {
        using var command = Command(connection, transaction, "SELECT club_name,billiards_rate,century_rate,room_rate FROM settings WHERE id=1;");
        using var reader = command.ExecuteReader();
        if (!reader.Read()) throw new InvalidOperationException("Club settings are missing. The database has not been reset.");
        return new(reader.GetString(0), reader.IsDBNull(1) ? null : reader.GetInt64(1),
            reader.IsDBNull(2) ? null : reader.GetInt64(2), reader.IsDBNull(3) ? null : reader.GetInt64(3));
    }
    public void SaveSettings(ClubSettings settings)
    {
        var name = settings.ClubName.Trim();
        if (name.Length is < 1 or > 120) throw new SessionRuleException("Enter a club name of 1–120 characters.");
        if (settings.BilliardsRate < 0 || settings.CenturyRate < 0 || settings.RoomRate < 0)
            throw new SessionRuleException("Rates cannot be negative.");
        using var connection = Open();
        using var command = Command(connection, null, """
            UPDATE settings SET club_name=$name,billiards_rate=$b,century_rate=$c,room_rate=$r WHERE id=1;
            """, ("$name", name), ("$b", settings.BilliardsRate), ("$c", settings.CenturyRate), ("$r", settings.RoomRate));
        if (command.ExecuteNonQuery() != 1) throw new InvalidOperationException("Settings could not be saved.");
    }

    private const string SessionColumns = "id,location_id,customer_name,mode,started_at,rate_minor_units,needs_recovery,player_count";
    private static ActiveSession ReadSession(SqliteDataReader reader) => new(Guid.Parse(reader.GetString(0)),
        reader.GetString(1), reader.GetString(2), Enum.Parse<GameMode>(reader.GetString(3)),
        DateTimeOffset.Parse(reader.GetString(4), CultureInfo.InvariantCulture), reader.GetInt64(5), reader.GetBoolean(6), reader.GetInt32(7));

    public IReadOnlyList<ActiveSession> GetActiveSessions()
    {
        using var connection = Open();
        using var command = Command(connection, null, $"SELECT {SessionColumns} FROM sessions WHERE status='ACTIVE' ORDER BY started_at,id;");
        using var reader = command.ExecuteReader();
        var sessions = new List<ActiveSession>();
        while (reader.Read()) sessions.Add(ReadSession(reader));
        return sessions;
    }

    public ActiveSession StartSession(StartSessionRequest request)
    {
        using var connection = Open();
        using var transaction = connection.BeginTransaction();
        Location location;
        using (var query = Command(connection, transaction, "SELECT id,name,is_private FROM locations WHERE id=$id;", ("$id", request.LocationId)))
        using (var reader = query.ExecuteReader())
        {
            if (!reader.Read()) throw new SessionRuleException("Choose an existing playing table or the Private Room.");
            location = new(reader.GetString(0), reader.GetString(1), reader.GetBoolean(2));
        }
        var customerName = SessionRules.Validate(request, location);
        using (var retry = Command(connection, transaction, $"SELECT {SessionColumns} FROM sessions WHERE request_id=$request;", ("$request", request.RequestId.ToString())))
        using (var reader = retry.ExecuteReader())
        {
            if (reader.Read())
            {
                var existing = ReadSession(reader);
                if (existing.LocationId != request.LocationId || existing.CustomerName != customerName
                    || existing.Mode != request.Mode || existing.RateMinorUnits != request.ExpectedRateMinorUnits)
                    throw new SessionRuleException("This request was already saved with different details. Reopen the table to inspect it.");
                return existing;
            }
        }
        using (var occupied = Command(connection, transaction,
            "SELECT COUNT(*) FROM sessions WHERE location_id=$id AND status IN ('ACTIVE','ENDED_REVIEW');", ("$id", location.Id)))
            if (Convert.ToInt64(occupied.ExecuteScalar()) != 0) throw new SessionRuleException("This location is already occupied. Open its existing session.");
        var rate = ReadSettings(connection, transaction).RateFor(request.Mode)
            ?? throw new SessionRuleException("Set this game's rate in Settings before starting a session.");
        if (rate != request.ExpectedRateMinorUnits) throw new SessionRuleException("The rate changed. Reopen the form to review the current rate.");
        var session = new ActiveSession(Guid.NewGuid(), location.Id, customerName, request.Mode, clock.GetUtcNow(), rate, false);
        using var insert = Command(connection, transaction, """
            INSERT INTO sessions(id,request_id,location_id,customer_name,mode,started_at,rate_minor_units)
            VALUES($id,$request,$location,$name,$mode,$started,$rate);
            """, ("$id", session.Id.ToString()), ("$request", request.RequestId.ToString()), ("$location", location.Id),
            ("$name", customerName), ("$mode", session.Mode.ToString()), ("$started", Stamp(session.StartedAt)), ("$rate", rate));
        insert.ExecuteNonQuery();
        AddEvent(connection, transaction, session.Id, "session_started", session.StartedAt);
        transaction.Commit();
        return session;
    }

    public void MarkReopenedSessionsForReview()
    {
        using var connection = Open();
        using var transaction = connection.BeginTransaction();
        var ids = new List<Guid>();
        using (var query = Command(connection, transaction, "SELECT id FROM sessions WHERE status='ACTIVE' AND needs_recovery=0;"))
        using (var reader = query.ExecuteReader())
            while (reader.Read()) ids.Add(Guid.Parse(reader.GetString(0)));
        var now = clock.GetUtcNow();
        foreach (var id in ids) AddEvent(connection, transaction, id, "recovery_required", now);
        using var update = Command(connection, transaction, "UPDATE sessions SET needs_recovery=1 WHERE status='ACTIVE' AND needs_recovery=0;");
        update.ExecuteNonQuery();
        transaction.Commit();
    }
    private static string Stamp(DateTimeOffset time) => time.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);
    private static void AddEvent(SqliteConnection connection, SqliteTransaction transaction, Guid sessionId, string kind, DateTimeOffset time)
    {
        using var command = Command(connection, transaction,
            "INSERT INTO session_events VALUES($id,$session,$kind,$time);", ("$id", Guid.NewGuid().ToString()),
            ("$session", sessionId.ToString()), ("$kind", kind), ("$time", Stamp(time)));
        command.ExecuteNonQuery();
    }
    public IReadOnlyList<SessionEvent> GetEvents(Guid sessionId)
    {
        using var connection = Open();
        using var command = Command(connection, null, "SELECT id,kind,recorded_at FROM session_events WHERE session_id=$id ORDER BY recorded_at,rowid;", ("$id", sessionId.ToString()));
        using var reader = command.ExecuteReader();
        var events = new List<SessionEvent>();
        while (reader.Read()) events.Add(new(Guid.Parse(reader.GetString(0)), sessionId, reader.GetString(1),
            DateTimeOffset.Parse(reader.GetString(2), CultureInfo.InvariantCulture)));
        return events;
    }
}
