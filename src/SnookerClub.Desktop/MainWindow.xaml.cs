using System.Windows;
using SnookerClub.Desktop.Views;
namespace SnookerClub.Desktop;
public partial class MainWindow : Window
{
    private readonly DashboardView dashboard = new();
    public MainWindow() { InitializeComponent(); PageHost.Content = dashboard; }
    private void OpenDashboard(object sender, RoutedEventArgs e) => PageHost.Content = dashboard;
    private void OpenInventory(object sender, RoutedEventArgs e) => ShowPlanned("Inventory", "Product catalog and stock", "Coming in M3", "Add products, restock items, and track availability. Stock changes and sales will be saved together once persistence is implemented.");
    private void OpenAnalysis(object sender, RoutedEventArgs e) => ShowPlanned("Analysis", "Understand your club", "Coming later", "Historical exports are planned for M6. Detailed reports, profit analysis, and optional Python analytics will be planned separately.");
    private void OpenSettings(object sender, RoutedEventArgs e) => ShowPlanned("Settings", "Club details and game rates", "Coming in a later milestone", "Configure the club name, currency, price per frame, price per minute, and Private Room hourly rate. No production prices have been set.");
    private void ShowPlanned(string title, string subtitle, string status, string description)
    {
        var view = new PlannedView(title, subtitle, status, description);
        view.BackRequested += (_, _) => PageHost.Content = dashboard;
        PageHost.Content = view;
    }
}
