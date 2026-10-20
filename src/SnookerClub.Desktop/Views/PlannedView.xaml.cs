using System.Windows;
using System.Windows.Controls;
namespace SnookerClub.Desktop.Views;
public partial class PlannedView : UserControl
{
    public event EventHandler? BackRequested;
    public PlannedView(string title, string subtitle, string status, string description)
    {
        InitializeComponent(); Heading.Text = title; Subtitle.Text = subtitle;
        Status.Text = status; Description.Text = description;
    }
    private void GoBack(object sender, RoutedEventArgs e) => BackRequested?.Invoke(this, EventArgs.Empty);
}
