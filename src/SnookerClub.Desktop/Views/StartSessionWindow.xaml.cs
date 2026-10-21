using System.ComponentModel;
using System.Windows;
using SnookerClub.Core;

namespace SnookerClub.Desktop.Views;
public partial class StartSessionWindow : Window
{
    private readonly IClubStore store;
    private readonly Location location;
    private readonly Guid requestId = Guid.NewGuid();
    private ClubSettings? settings;
    private bool saving;
    private GameMode SelectedMode => location.IsPrivateRoom ? GameMode.PrivateRoom : Century.IsChecked == true ? GameMode.Century : GameMode.Billiards;
    public StartSessionWindow(IClubStore store, Location location)
    {
        this.store = store; this.location = location;
        InitializeComponent();
        Heading.Text = $"Start · {location.Name}";
        StandardOptions.Visibility = location.IsPrivateRoom ? Visibility.Collapsed : Visibility.Visible;
        RoomDetails.Visibility = location.IsPrivateRoom ? Visibility.Visible : Visibility.Collapsed;
        Loaded += LoadRates;
        Closing += (_, e) => { if (saving) e.Cancel = true; };
    }
    private async void LoadRates(object sender, RoutedEventArgs e)
    {
        try { settings = await Task.Run(store.GetSettings); UpdateRate(); CustomerName.Focus(); }
        catch (Exception ex) { Feedback.Text = $"Rates could not be loaded. Close and retry. {ex.Message}"; }
    }
    private void GameChanged(object sender, RoutedEventArgs e) { if (IsInitialized) UpdateRate(); }
    private void UpdateRate()
    {
        var rate = settings?.RateFor(SelectedMode);
        RateLabel.Text = rate is null ? "Set this game's rate in Settings before starting." : $"Rs {Money.Format(rate.Value)} per {Money.Unit(SelectedMode)}";
        StartButton.IsEnabled = rate is not null;
    }
    private async void StartSession(object sender, RoutedEventArgs e)
    {
        if (saving || settings?.RateFor(SelectedMode) is not long rate) return;
        var request = new StartSessionRequest(requestId, location.Id, CustomerName.Text, SelectedMode, rate);
        saving = true;
        Form.IsEnabled = false;
        Feedback.Text = "Saving session…";
        try
        {
            await Task.Run(() => store.StartSession(request));
            saving = false;
            DialogResult = true;
        }
        catch (SessionRuleException ex) { Feedback.Text = ex.Message; }
        catch (Exception ex) { Feedback.Text = $"The save could not be confirmed. Retry with the same details or close this form to reload the table. {ex.Message}"; }
        finally { saving = false; Form.IsEnabled = true; }
    }
}
