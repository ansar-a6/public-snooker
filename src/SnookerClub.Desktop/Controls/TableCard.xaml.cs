using System.Windows;
using System.Windows.Controls;
using SnookerClub.Desktop.Views;
namespace SnookerClub.Desktop.Controls;

public partial class TableCard : UserControl
{
    public static readonly DependencyProperty LocationNameProperty = DependencyProperty.Register(
        nameof(LocationName), typeof(string), typeof(TableCard), new PropertyMetadata("Table"));
    public static readonly DependencyProperty IsPrivateRoomProperty = DependencyProperty.Register(
        nameof(IsPrivateRoom), typeof(bool), typeof(TableCard), new PropertyMetadata(false));
    public static readonly DependencyProperty SubtitleProperty = DependencyProperty.Register(
        nameof(Subtitle), typeof(string), typeof(TableCard), new PropertyMetadata("Billiards / Century"));
    public string LocationName { get => (string)GetValue(LocationNameProperty); set => SetValue(LocationNameProperty, value); }
    public bool IsPrivateRoom { get => (bool)GetValue(IsPrivateRoomProperty); set => SetValue(IsPrivateRoomProperty, value); }
    public string Subtitle { get => (string)GetValue(SubtitleProperty); set => SetValue(SubtitleProperty, value); }
    public TableCard() => InitializeComponent();
    private void OpenLocation(object sender, RoutedEventArgs e) =>
        new StartSessionWindow(LocationName, IsPrivateRoom) { Owner = Window.GetWindow(this) }.ShowDialog();
}
