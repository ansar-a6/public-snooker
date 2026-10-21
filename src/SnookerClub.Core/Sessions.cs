using System.Globalization;

namespace SnookerClub.Core;

public enum GameMode { Billiards, Century, PrivateRoom }
public sealed record Location(string Id, string Name, bool IsPrivateRoom);
public sealed record ActiveSession(Guid Id, string LocationId, string CustomerName, GameMode Mode,
    DateTimeOffset StartedAt, long RateMinorUnits, bool NeedsRecoveryReview, int PlayerCount = 2);
public sealed record StartSessionRequest(Guid RequestId, string LocationId, string CustomerName,
    GameMode Mode, long ExpectedRateMinorUnits);
public sealed record SessionEvent(Guid Id, Guid SessionId, string Kind, DateTimeOffset RecordedAt);
public sealed record ClubSettings(string ClubName, long? BilliardsRate, long? CenturyRate, long? RoomRate)
{
    public long? RateFor(GameMode mode) => mode switch
    {
        GameMode.Billiards => BilliardsRate,
        GameMode.Century => CenturyRate,
        GameMode.PrivateRoom => RoomRate,
        _ => throw new SessionRuleException("Choose a valid game type.")
    };
}

public sealed class SessionRuleException(string message) : Exception(message);

public static class SessionRules
{
    public static string Validate(StartSessionRequest request, Location location)
    {
        if (request.RequestId == Guid.Empty) throw new SessionRuleException("A session request identifier is required.");
        var name = request.CustomerName?.Trim() ?? "";
        if (name.Length is < 1 or > 120) throw new SessionRuleException("Enter a customer name of 1–120 characters.");
        if (!Enum.IsDefined(request.Mode)) throw new SessionRuleException("Choose a valid game type.");
        if (location.IsPrivateRoom != (request.Mode == GameMode.PrivateRoom))
            throw new SessionRuleException("The selected game does not match this location.");
        return name;
    }
}

public static class Money
{
    public static long? ParseOptionalRate(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        if (!decimal.TryParse(text.Trim(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var value)
            || value < 0 || value > long.MaxValue / 100m || decimal.Truncate(value * 100m) != value * 100m)
            throw new SessionRuleException("Rates must be non-negative amounts with at most two decimal places. Use a dot for decimals.");
        return (long)(value * 100m);
    }
    public static string Format(long amount) => (amount / 100m).ToString("0.00", CultureInfo.InvariantCulture);
    public static string Unit(GameMode mode) => mode switch
    {
        GameMode.Billiards => "frame", GameMode.Century => "minute", _ => "one-hour block"
    };
}

// Synchronous storage contract: the desktop runs database work off its UI thread.
public interface IClubStore
{
    void Initialize();
    IReadOnlyList<Location> GetLocations();
    ClubSettings GetSettings();
    void SaveSettings(ClubSettings settings);
    IReadOnlyList<ActiveSession> GetActiveSessions();
    ActiveSession StartSession(StartSessionRequest request);
    void MarkReopenedSessionsForReview();
    IReadOnlyList<SessionEvent> GetEvents(Guid sessionId);
}
