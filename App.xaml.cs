using System.Windows;
using LessonScheduler.Services;
using LessonScheduler.Views.Dialogs;
using LessonScheduler.Properties;

namespace LessonScheduler
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Keep app alive until MainWindow closes (set first)
            this.ShutdownMode = ShutdownMode.OnMainWindowClose;

            base.OnStartup(e);

            // Theme
            ThemeService.Instance.SetTheme(ThemeType.Light);

            // Create main window and show it
            var main = new LessonScheduler.Views.MainWindow();
            this.MainWindow = main;
            main.Show();

            // HARD-ENFORCE: Show welcome dialog immediately and synchronously on startup,
            // ignoring saved preference for now to ensure visibility.
            try
            {
                var dlg = new InfoDialog
                {
                    Owner = main,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Topmost = true,
                    ShowInTaskbar = false
                };
                // Synchronous, blocking call right after main.Show()
                dlg.ShowDialog();
            }
            catch
            {
                // If dialog fails for any reason, continue to main window
            }
        }
    }
}