using System.Windows;
using System.Windows.Controls;
using SnookerClub.Core;
using SnookerClub.Desktop.Controls;

namespace SnookerClub.Desktop.Views;
public partial class DashboardView : UserControl
{
    private readonly IClubStore store;
    private readonly TableCard[] cards;
    public DashboardView(IClubStore store)
    {
        InitializeComponent();
        this.store = store;
        cards = Floor.Children.OfType<TableCard>().ToArray();
        foreach (var card in cards) card.OpenRequested += OpenLocation;
    }
    public async Task RefreshAsync()
    {
        IsEnabled = false;
        try
        {
            var data = await Task.Run(() => (Locations: store.GetLocations(), Sessions: store.GetActiveSessions()));
            foreach (var card in cards)
                card.Display(data.Locations.Single(x => x.Id == card.LocationId), data.Sessions.SingleOrDefault(x => x.LocationId == card.LocationId));
            Summary.Text = $"{data.Sessions.Count} in use · {8 - data.Sessions.Count} available";
            IsEnabled = true;
        }
        catch (Exception ex)
        {
            Summary.Text = "Saved sessions could not be loaded. Restart the application to retry.";
            MessageBox.Show(Window.GetWindow(this), $"Table actions are disabled because saved occupancy could not be loaded.\n\n{ex.Message}", "Cannot load sessions", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    private async void OpenLocation(object? sender, EventArgs e)
    {
        if (sender is not TableCard { Location: not null } card) return;
        if (card.Session is not null)
            new SessionDetailsWindow(store, card.Location, card.Session) { Owner = Window.GetWindow(this) }.ShowDialog();
        else
            new StartSessionWindow(store, card.Location) { Owner = Window.GetWindow(this) }.ShowDialog();
        await RefreshAsync();
    }
}
