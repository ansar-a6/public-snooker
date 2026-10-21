using System.IO;
using System.Windows;
using SnookerClub.Core;
using SnookerClub.Infrastructure;

namespace SnookerClub.Desktop;
public partial class App : Application
{
    private FileStream? instanceLock;
    public IClubStore Store { get; private set; } = null!;
    public string DatabasePath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SnookerClub", "club.db");

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ShutdownMode = ShutdownMode.OnExplicitShutdown;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(DatabasePath)!);
            try { instanceLock = new FileStream(DatabasePath + ".lock", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None); }
            catch (IOException)
            {
                MessageBox.Show("Snooker Club may already be open, or its data folder is unavailable. Close the other instance and check folder access before retrying.", "Cannot open club data");
                Shutdown(1);
                return;
            }
            Store = new SqliteClubStore(DatabasePath);
            await Task.Run(() => { Store.Initialize(); Store.MarkReopenedSessionsForReview(); });
            MainWindow = new MainWindow(Store);
            ShutdownMode = ShutdownMode.OnMainWindowClose;
            MainWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"The local database could not be opened. Existing data has not been reset.\n\n{DatabasePath}\n\n{ex.Message}", "Startup failed", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }
    protected override void OnExit(ExitEventArgs e) { instanceLock?.Dispose(); base.OnExit(e); }
}
