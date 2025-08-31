using System;
using System.Windows;
using System.Windows.Media;
using LessonScheduler.Services;

namespace LessonScheduler.Views.Dialogs
{
    public partial class SmartProgressDialog : Window
    {
        public SmartProgressDialog()
        {
            InitializeComponent();
        }

        public void UpdateProgress(SolverProgress progress)
        {
            // Progress bar güncelle
            MainProgressBar.Value = progress.ProgressPercentage;
            PercentageText.Text = $"{progress.ProgressPercentage:F0}%";
            
            // Durum güncelle
            StatusText.Text = progress.CurrentStatus;
            
            // İstatistikler güncelle
            StatsText.Text = $"Yerleştirilen: {progress.PlacedBlocks}/{progress.TotalBlocks} blok";
            TimeText.Text = $"Süre: {progress.ElapsedSeconds:F1}s";
            
            if (progress.EstimatedRemainingSeconds > 0)
            {
                EstimateText.Text = $"Kalan: ~{progress.EstimatedRemainingSeconds:F0}s";
            }
            else
            {
                EstimateText.Text = "Kalan: Hesaplanıyor...";
            }
            
            // İpucu güncelle
            if (!string.IsNullOrEmpty(progress.CurrentTip))
            {
                TipText.Text = $"💡 {progress.CurrentTip}";
            }
            
            // Progress bar rengini duruma göre ayarla
            if (progress.ProgressPercentage >= 100)
            {
                MainProgressBar.Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Yeşil
                StatusText.Text = "🚀 Çözüm algoritması çalışıyor...";
            }
            else if (progress.ProgressPercentage >= 75)
            {
                MainProgressBar.Foreground = new SolidColorBrush(Color.FromRgb(255, 193, 7)); // Sarı
            }
            else
            {
                MainProgressBar.Foreground = new SolidColorBrush(Color.FromRgb(33, 150, 243)); // Mavi
            }
        }

        public void ShowSuccess(string title, string message)
        {
            // Başarı mesajını göster
            ResultBorder.Background = new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Yeşil
            ResultTitle.Text = title;
            ResultMessage.Text = message;
            ResultBorder.Visibility = Visibility.Visible;
            
            // Başlığı güncelle
            TitleText.Text = "🎉 Tamamlandı!";
            TitleText.Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80));
        }

        public void ShowError(string title, string message)
        {
            // Hata mesajını göster
            ResultBorder.Background = new SolidColorBrush(Color.FromRgb(244, 67, 54)); // Kırmızı
            ResultTitle.Text = title;
            ResultMessage.Text = message;
            ResultBorder.Visibility = Visibility.Visible;
            
            // Başlığı güncelle
            TitleText.Text = "⚠️ Tamamlanamadı";
            TitleText.Foreground = new SolidColorBrush(Color.FromRgb(244, 67, 54));
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DialogResult = false;
                Close();
            }
            catch
            {
                // Güvenli kapatma
                Hide();
            }
        }
    }
}