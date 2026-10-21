using System.Windows;
using SnookerClub.Core;
namespace SnookerClub.Desktop.Views;
public partial class SettingsWindow : Window
{
    private readonly IClubStore store;
    private bool saving;
    public SettingsWindow(IClubStore store)
    {
        InitializeComponent(); this.store = store;
        Loaded += LoadSettings;
        Closing += (_, e) => { if (saving) e.Cancel = true; };
    }
    private async void LoadSettings(object sender, RoutedEventArgs e)
    {
        try
        {
            var settings = await Task.Run(store.GetSettings);
            ClubName.Text = settings.ClubName;
            BilliardsRate.Text = Format(settings.BilliardsRate);
            CenturyRate.Text = Format(settings.CenturyRate);
            RoomRate.Text = Format(settings.RoomRate);
            Form.IsEnabled = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Settings could not be loaded. Close and retry.\n\n{ex.Message}", "Settings unavailable", MessageBoxButton.OK, MessageBoxImage.Error);
            Close();
        }
    }
    private static string Format(long? amount) => amount is null ? "" : Money.Format(amount.Value);
    private async void SaveSettings(object sender, RoutedEventArgs e)
    {
        if (saving) return;
        try
        {
            var settings = new ClubSettings(ClubName.Text, Money.ParseOptionalRate(BilliardsRate.Text),
                Money.ParseOptionalRate(CenturyRate.Text), Money.ParseOptionalRate(RoomRate.Text));
            saving = true; Form.IsEnabled = false;
            await Task.Run(() => store.SaveSettings(settings));
            saving = false;
            DialogResult = true;
        }
        catch (SessionRuleException ex) { Feedback.Text = ex.Message; }
        catch (Exception ex) { Feedback.Text = $"Settings save could not be confirmed. Reopen Settings to check. {ex.Message}"; }
        finally { saving = false; Form.IsEnabled = true; }
    }
}
