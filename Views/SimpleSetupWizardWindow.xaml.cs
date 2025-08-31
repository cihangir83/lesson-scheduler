using System.Windows;

namespace LessonScheduler.Views
{
    public partial class SimpleSetupWizardWindow : Window
    {
        public SimpleSetupWizardWindow()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Kurulum sihirbazı tamamlandı!\nArtık ana programı kullanmaya başlayabilirsiniz.", 
                           "Tamamlandı", MessageBoxButton.OK, MessageBoxImage.Information);
            DialogResult = true;
            Close();
        }

        private void SkipButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Kurulum sihirbazını atlamak istediğinizden emin misiniz?", 
                                        "Sihirbazı Atla", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                DialogResult = true;
                Close();
            }
        }
    }
}