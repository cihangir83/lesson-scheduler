using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using LessonScheduler.Models;

namespace LessonScheduler.Views.Dialogs
{
    public partial class AssignmentDialog : Window
    {
        public LessonAssignment? Result { get; private set; }

        public AssignmentDialog(List<string> lessons, List<string> teachers, LessonAssignment? currentAssignment = null)
        {
            InitializeComponent();

            // ComboBox'ları doldur
            LessonComboBox.ItemsSource = lessons;
            TeacherComboBox.ItemsSource = teachers;

            // Mevcut atama varsa form'u doldur
            if (currentAssignment != null)
            {
                LessonComboBox.SelectedItem = currentAssignment.Ders;
                TeacherComboBox.SelectedItem = currentAssignment.Ogretmen;
                TotalHoursTextBox.Text = currentAssignment.ToplamSaat.ToString();
                BlockStructureTextBox.Text = currentAssignment.BlokYapisi;
            }
            else
            {
                // Varsayılan değerler
                if (lessons.Any()) LessonComboBox.SelectedIndex = 0;
                if (teachers.Any()) TeacherComboBox.SelectedIndex = 0;
            }

            // TextBox değişikliklerini dinle
            TotalHoursTextBox.TextChanged += ValidateInput;
            BlockStructureTextBox.TextChanged += ValidateInput;
        }

        private void ValidateInput(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ValidateBlockStructure();
        }

        private bool ValidateBlockStructure()
        {
            ValidationMessage.Visibility = Visibility.Collapsed;

            // Toplam saat kontrolü
            if (!int.TryParse(TotalHoursTextBox.Text, out int totalHours) || totalHours <= 0)
            {
                ShowValidationError("Toplam saat pozitif bir sayı olmalıdır.");
                return false;
            }

            // Blok yapısı kontrolü
            var blockStructureText = BlockStructureTextBox.Text?.Trim();
            if (string.IsNullOrEmpty(blockStructureText))
            {
                ShowValidationError("Blok yapısı boş olamaz.");
                return false;
            }

            try
            {
                var blocks = blockStructureText.Split(',')
                                             .Select(s => s.Trim())
                                             .Select(int.Parse)
                                             .ToList();

                if (blocks.Any(b => b <= 0))
                {
                    ShowValidationError("Blok uzunlukları pozitif olmalıdır.");
                    return false;
                }

                if (blocks.Sum() != totalHours)
                {
                    ShowValidationError($"Blok toplamı ({blocks.Sum()}) toplam saatle ({totalHours}) eşleşmiyor.");
                    return false;
                }

                return true;
            }
            catch (FormatException)
            {
                ShowValidationError("Blok yapısı sadece sayılar ve virgül içermelidir.");
                return false;
            }
        }

        private void ShowValidationError(string message)
        {
            ValidationMessage.Text = message;
            ValidationMessage.Visibility = Visibility.Visible;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // Validasyon
            if (LessonComboBox.SelectedItem == null)
            {
                MessageBox.Show("Lütfen bir ders seçin.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (TeacherComboBox.SelectedItem == null)
            {
                MessageBox.Show("Lütfen bir öğretmen seçin.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ValidateBlockStructure())
            {
                return;
            }

            // Sonuç oluştur
            Result = new LessonAssignment
            {
                Ders = LessonComboBox.SelectedItem.ToString()!,
                Ogretmen = TeacherComboBox.SelectedItem.ToString()!,
                ToplamSaat = int.Parse(TotalHoursTextBox.Text),
                BlokYapisi = BlockStructureTextBox.Text.Trim()
            };

            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}