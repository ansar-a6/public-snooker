using System.Windows;
using SnookerClub.Core;
namespace SnookerClub.Desktop.Views;
public partial class SessionDetailsWindow : Window
{
    public SessionDetailsWindow(IClubStore store, Location location, ActiveSession session)
    {
        InitializeComponent();
        Customer.Text = session.CustomerName;
        LocationLabel.Text = $"{location.Name} · {(session.Mode == GameMode.PrivateRoom ? "Private Room" : session.Mode)} · In use";
        Started.Text = $"Started: {session.StartedAt.ToLocalTime():g}";
        Rate.Text = $"Saved rate: Rs {Money.Format(session.RateMinorUnits)} per {Money.Unit(session.Mode)}";
        GameInfo.Text = session.Mode switch
        {
            GameMode.Billiards => "Recorded frames: 0 · Frame entry arrives in M3.",
            GameMode.Century => "Charged per minute · Time billing is not implemented yet.",
            _ => "Initial one-hour session saved · Countdown and continuation arrive in M5."
        };
        Recovery.Text = session.NeedsRecoveryReview
            ? "Restored session — recovery review required. The interruption duration is unknown. No downtime has been included or excluded from a bill. Manual review arrives in M6."
            : "Session saved locally.";
        Loaded += async (_, _) =>
        {
            try
            {
                var events = await Task.Run(() => store.GetEvents(session.Id));
                Events.ItemsSource = events.Select(x => $"{x.RecordedAt.ToLocalTime():g} · {(x.Kind == "session_started" ? "Session started" : "Reopened — recovery review required")}");
            }
            catch (Exception ex) { Recovery.Text = $"Activity could not be loaded. No records have been changed. {ex.Message}"; }
        };
    }
}
