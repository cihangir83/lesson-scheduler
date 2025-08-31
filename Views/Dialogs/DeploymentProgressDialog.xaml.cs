using System.Windows;
using System.Windows.Media;

namespace LessonScheduler.Views.Dialogs
{
    public partial class DeploymentProgressDialog : Window
    {
        public DeploymentProgressDialog()
        {
            // InitializeComponent will be generated from XAML at build time
            InitializeComponent();
        }

        // Çözüm tamamlandığında çağrılır
        public void SetCompleted(bool success, string message)
        {
            // UI öğelerini ada göre bul
            var titleText = this.FindName("TitleText") as System.Windows.Controls.TextBlock;
            var subText = this.FindName("SubText") as System.Windows.Controls.TextBlock;
            var progressBar = this.FindName("ProgressBar") as System.Windows.Controls.ProgressBar;
            var closeButton = this.FindName("CloseButton") as System.Windows.Controls.Button;

            if (titleText != null) titleText.Text = message;
            if (subText != null) subText.Text = success ? "Dağıtım başarıyla tamamlandı." : "Uygun bir çözüm bulunamadı.";

            if (progressBar != null)
            {
                progressBar.IsIndeterminate = false;
                progressBar.Value = 100;

                // Renkleri başarı/başarısızlığa göre değiştir
                var color = success ? (Color)ColorConverter.ConvertFromString("#4CAF50")
                                    : (Color)ColorConverter.ConvertFromString("#F44336");
                var brush = new SolidColorBrush(color);
                progressBar.Foreground = brush;
            }

            if (closeButton != null) closeButton.Visibility = Visibility.Visible;
            if (titleText != null) titleText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1F2937"));
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}