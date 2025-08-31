using System.Windows;
using LessonScheduler.Properties;

namespace LessonScheduler.Views.Dialogs
{
    // Ensure namespace/class exactly matches x:Class in XAML: LessonScheduler.Views.Dialogs.InfoDialog
    public partial class InfoDialog : Window
    {
        public InfoDialog()
        {
            // This method is auto-generated at build time from InfoDialog.xaml
            this.InitializeComponent();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            var dontShowAgain = (this.FindName("DontShowAgainCheck") as System.Windows.Controls.CheckBox)?.IsChecked == true;
            if (dontShowAgain)
            {
                Settings.Default.ShowInfoOnStartup = false;
                Settings.Default.Save();
            }

            this.DialogResult = true;
        }
    }
}