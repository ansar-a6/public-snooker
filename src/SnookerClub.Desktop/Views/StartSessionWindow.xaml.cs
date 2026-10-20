using System.Windows;
namespace SnookerClub.Desktop.Views;
public partial class StartSessionWindow : Window
{
    public StartSessionWindow(string locationName, bool isPrivateRoom)
    {
        InitializeComponent(); Heading.Text = $"Start · {locationName}";
        StandardOptions.Visibility = isPrivateRoom ? Visibility.Collapsed : Visibility.Visible;
        RoomDetails.Visibility = isPrivateRoom ? Visibility.Visible : Visibility.Collapsed;
        Loaded += (_, _) => CustomerName.Focus();
    }
}
