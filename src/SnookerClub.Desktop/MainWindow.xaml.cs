using System.Windows;
using SnookerClub.Core;
using SnookerClub.Desktop.Views;
namespace SnookerClub.Desktop;
public partial class MainWindow : Window
{
    private readonly IClubStore store;
    private readonly DashboardView dashboard;
    public MainWindow(IClubStore store)
    {
        InitializeComponent(); this.store = store;
        dashboard = new DashboardView(store);
        PageHost.Content = dashboard;
        Loaded += async (_, _) => { await LoadClubName(); await dashboard.RefreshAsync(); };
    }
    private async Task LoadClubName()
    {
        try
        {
            var settings = await Task.Run(store.GetSettings);
            ClubLabel.Text = settings.ClubName; Title = settings.ClubName;
        }
        catch (Exception ex) { MessageBox.Show(this, $"Club settings could not be loaded.\n\n{ex.Message}", "Settings unavailable"); }
    }
    private void OpenDashboard(object sender, RoutedEventArgs e) => PageHost.Content = dashboard;
    private void OpenInventory(object sender, RoutedEventArgs e) => ShowPlanned("Inventory", "Product catalog and stock", "Coming in M3", "Add products, restock items, and track availability. Stock changes and sales will be saved together.");
    private void OpenAnalysis(object sender, RoutedEventArgs e) => ShowPlanned("Analysis", "Understand your club", "Coming later", "Historical exports are planned for M6. Detailed reports, profit analysis, and optional Python analytics will be planned separately.");
    private async void OpenSettings(object sender, RoutedEventArgs e)
    {
        if (new SettingsWindow(store) { Owner = this }.ShowDialog() == true) await LoadClubName();
    }
    private void ShowPlanned(string title, string subtitle, string status, string description)
    {
        var view = new PlannedView(title, subtitle, status, description);
        view.BackRequested += (_, _) => PageHost.Content = dashboard;
        PageHost.Content = view;
    }
}
