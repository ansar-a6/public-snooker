using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SnookerClub.Core;

namespace SnookerClub.Desktop.Controls;
public partial class TableCard : UserControl
{
    public string LocationId { get; set; } = "";
    public Location? Location { get; private set; }
    public ActiveSession? Session { get; private set; }
    public event EventHandler? OpenRequested;
    public TableCard() => InitializeComponent();
    public void Display(Location location, ActiveSession? session)
    {
        Location = location;
        Session = session;
        TitleLabel.Text = session?.CustomerName ?? location.Name;
        TitleLabel.ToolTip = TitleLabel.Text;
        StatusLabel.Text = session is null ? "AVAILABLE" : session.NeedsRecoveryReview ? "RECOVERY REVIEW" : "IN USE";
        SubtitleLabel.Text = session is null
            ? location.IsPrivateRoom ? "Private space · Hour blocks" : "Billiards / Century"
            : $"{location.Name} · {(session.Mode == GameMode.PrivateRoom ? "Private Room" : session.Mode)}";
        ActionLabel.Text = session is null ? "Start session  →" : "View session  →";
        var color = session?.Mode switch
        {
            GameMode.Billiards => "#F1AC68", GameMode.Century => "#8CD8A6",
            GameMode.PrivateRoom => "#EDF4F1", _ => "#D27878"
        };
        CardButton.BorderBrush = (Brush)new BrushConverter().ConvertFromString(color)!;
        StatusLabel.Foreground = session?.NeedsRecoveryReview == true ? Brushes.Khaki : CardButton.BorderBrush;
        AutomationProperties.SetName(CardButton, $"{location.Name}, {StatusLabel.Text}, {session?.CustomerName}");
        CardButton.IsEnabled = true;
    }
    private void OpenLocation(object sender, RoutedEventArgs e) => OpenRequested?.Invoke(this, EventArgs.Empty);
    private void QuickEntry(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
        if (Session is not null)
            MessageBox.Show(Window.GetWindow(this), "Quick entry for frames and canteen items arrives in M3. No entries have been added.", "Quick entry — coming in M3");
    }
}
