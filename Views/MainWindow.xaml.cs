using System.Windows;
using LessonScheduler.ViewModels;

namespace LessonScheduler.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        private void OpenSettingsTab(object sender, RoutedEventArgs e)
        {
            // Settings sekmesini aktif yap (ilk sekme)
            MainTabControl.SelectedIndex = 0;
        }
    }
}