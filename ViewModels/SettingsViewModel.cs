using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using LessonScheduler.Models;
using LessonScheduler.Services;
using LessonScheduler.Utilities;

namespace LessonScheduler.ViewModels
{
    public class SettingsViewModel : ObservableObject
    {
        private readonly IConfigurationService _configurationService;
        private SchoolData? _schoolData;
        
        private string _schoolName = "Okul Adı";
        private string _principalName = "Müdür Adı";
        private ObservableCollection<DayHourSetting> _dailyHours = new();
        private bool _hasUnsavedChanges = false;

        public string SchoolName
        {
            get => _schoolName;
            set
            {
                if (SetProperty(ref _schoolName, value))
                {
                    HasUnsavedChanges = true;
                }
            }
        }

        public string PrincipalName
        {
            get => _principalName;
            set
            {
                if (SetProperty(ref _principalName, value))
                {
                    HasUnsavedChanges = true;
                }
            }
        }

        public ObservableCollection<DayHourSetting> DailyHours
        {
            get => _dailyHours;
            set => SetProperty(ref _dailyHours, value);
        }

        public bool HasUnsavedChanges
        {
            get => _hasUnsavedChanges;
            set => SetProperty(ref _hasUnsavedChanges, value);
        }

        // Commands
        public ICommand SaveSettingsCommand { get; }
        public ICommand ResetToDefaultCommand { get; }
        public ICommand ApplyChangesCommand { get; }
        public ICommand CancelChangesCommand { get; }

        public SettingsViewModel()
        {
            _configurationService = new ConfigurationService();
            
            InitializeDailyHours();
            
            // Commands
            SaveSettingsCommand = new RelayCommand(SaveSettings, CanSaveSettings);
            ResetToDefaultCommand = new RelayCommand(ResetToDefault);
            ApplyChangesCommand = new RelayCommand(ApplyChanges, CanApplyChanges);
            CancelChangesCommand = new RelayCommand(CancelChanges, () => HasUnsavedChanges);
        }

        public void UpdateSchoolData(SchoolData? schoolData)
        {
            _schoolData = schoolData;
            LoadFromSchoolData();
        }

        private void InitializeDailyHours()
        {
            var dayNames = new[] { "Pazartesi", "Salı", "Çarşamba", "Perşembe", "Cuma" };
            
            DailyHours.Clear();
            for (int i = 0; i < 5; i++)
            {
                var setting = new DayHourSetting(dayNames[i], i, 7);
                setting.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(DayHourSetting.Hours))
                    {
                        HasUnsavedChanges = true;
                    }
                };
                DailyHours.Add(setting);
            }
        }

        private void LoadFromSchoolData()
        {
            if (_schoolData?.Configuration != null)
            {
                SchoolName = _schoolData.Configuration.SchoolName;
                PrincipalName = _schoolData.Configuration.PrincipalName;
                
                // Günlük saat ayarlarını yükle
                for (int i = 0; i < DailyHours.Count; i++)
                {
                    DailyHours[i].Hours = _schoolData.Configuration.GetHoursForDay(i);
                }
            }
            else
            {
                // Varsayılan değerleri yükle
                var defaultConfig = _configurationService.GetDefaultConfiguration();
                SchoolName = defaultConfig.SchoolName;
                PrincipalName = defaultConfig.PrincipalName;
                
                for (int i = 0; i < DailyHours.Count; i++)
                {
                    DailyHours[i].Hours = defaultConfig.GetHoursForDay(i);
                }
            }
            
            HasUnsavedChanges = false;
        }

        private void SaveSettings()
        {
            try
            {
                ApplyChanges();
                
                // Başarı mesajı (ApplyChanges'da zaten gösterildi, burada kısa mesaj)
                MessageBox.Show("Ayarlar başarıyla kaydedildi ve uygulandı!", "Başarılı", 
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (DataValidationException)
            {
                // Validation hataları zaten ApplyChanges'da gösterildi
                // Tekrar göstermeye gerek yok
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ayarlar kaydedilirken beklenmeyen hata oluştu:\n{ex.Message}", 
                              "Kritik Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanSaveSettings()
        {
            return HasUnsavedChanges && IsValidConfiguration();
        }

        private void ResetToDefault()
        {
            var result = MessageBox.Show(
                "Tüm ayarları varsayılan değerlere sıfırlamak istediğinizden emin misiniz?",
                "Varsayılana Sıfırla",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var defaultConfig = _configurationService.GetDefaultConfiguration();
                
                SchoolName = defaultConfig.SchoolName;
                PrincipalName = defaultConfig.PrincipalName;
                
                for (int i = 0; i < DailyHours.Count; i++)
                {
                    DailyHours[i].Hours = defaultConfig.GetHoursForDay(i);
                }
                
                HasUnsavedChanges = true;
            }
        }

        private void ApplyChanges()
        {
            if (_schoolData == null)
                return;

            try
            {
                // Validation
                ValidateSettings();
                
                // Yapılandırmayı güncelle
                if (_schoolData.Configuration == null)
                {
                    _schoolData.Configuration = new SchoolConfiguration();
                }

                var oldSchoolName = _schoolData.Configuration.SchoolName;
                var oldPrincipalName = _schoolData.Configuration.PrincipalName;
                var oldHours = _schoolData.Configuration.DailyHours.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                _schoolData.Configuration.SchoolName = SchoolName.Trim();
                _schoolData.Configuration.PrincipalName = PrincipalName.Trim();
                
                // Günlük saat ayarlarını güncelle
                var newHours = DailyHours.ToDictionary(d => d.DayIndex, d => d.Hours);
                _schoolData.Configuration.UpdateDailyHours(newHours);
                
                // Değişiklik var mı kontrol et
                var hasScheduleChanges = !oldHours.SequenceEqual(_schoolData.Configuration.DailyHours);
                
                // Kısıtlamaları güncelle
                _schoolData.OnConfigurationChanged();
                
                HasUnsavedChanges = false;
                
                // Başarı bildirimi
                if (hasScheduleChanges)
                {
                    MessageBox.Show(
                        "Ayarlar başarıyla uygulandı!\n\n" +
                        "⚠️ Ders saati yapısı değiştiği için:\n" +
                        "• Öğretmen kısıtlamaları güncellendi\n" +
                        "• Mevcut çözüm temizlendi\n" +
                        "• Yeniden dağıtım yapmanız önerilir",
                        "Ayarlar Uygulandı",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                
                // Configuration changed event'ini tetikle
                OnConfigurationChanged();
            }
            catch (DataValidationException ex)
            {
                MessageBox.Show($"Ayar doğrulama hatası:\n{ex.Message}", 
                              "Geçersiz Ayar", MessageBoxButton.OK, MessageBoxImage.Warning);
                throw;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ayarlar uygulanırken beklenmeyen hata oluştu:\n{ex.Message}", 
                              "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                throw new DataValidationException($"Ayarlar uygulanırken hata oluştu: {ex.Message}", ex);
            }
        }

        private bool CanApplyChanges()
        {
            return HasUnsavedChanges && IsValidConfiguration();
        }

        private void CancelChanges()
        {
            var result = MessageBox.Show(
                "Kaydedilmemiş değişiklikler var. İptal etmek istediğinizden emin misiniz?",
                "Değişiklikleri İptal Et",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                LoadFromSchoolData();
            }
        }

        private bool IsValidConfiguration()
        {
            try
            {
                ValidateSettings();
                return true;
            }
            catch (DataValidationException)
            {
                return false;
            }
        }

        private void ValidateSettings()
        {
            var errors = new List<string>();

            // Okul adı kontrolü
            if (string.IsNullOrWhiteSpace(SchoolName))
                errors.Add("• Okul adı boş olamaz.");
            else if (SchoolName.Trim().Length > 100)
                errors.Add("• Okul adı 100 karakterden uzun olamaz.");

            // Müdür adı kontrolü
            if (string.IsNullOrWhiteSpace(PrincipalName))
                errors.Add("• Müdür adı boş olamaz.");
            else if (PrincipalName.Trim().Length > 100)
                errors.Add("• Müdür adı 100 karakterden uzun olamaz.");

            // Günlük saat kontrolü
            foreach (var dayHour in DailyHours)
            {
                if (dayHour.Hours < 1 || dayHour.Hours > 10)
                    errors.Add($"• {dayHour.DayName} için saat sayısı 1-10 arasında olmalıdır. (Şu an: {dayHour.Hours})");
            }

            // Toplam haftalık saat kontrolü
            var totalWeeklyHours = DailyHours.Sum(d => d.Hours);
            if (totalWeeklyHours < 5)
                errors.Add("• Toplam haftalık saat sayısı en az 5 olmalıdır.");
            else if (totalWeeklyHours > 50)
                errors.Add("• Toplam haftalık saat sayısı 50'den fazla olamaz.");

            if (errors.Any())
            {
                var errorMessage = "Ayarlarda şu hatalar bulundu:\n\n" + string.Join("\n", errors);
                throw new DataValidationException(errorMessage);
            }
        }

        /// <summary>
        /// Ayarlar değiştiğinde diğer ViewModellere bildirim gönderir
        /// </summary>
        public event EventHandler? ConfigurationChanged;

        protected virtual void OnConfigurationChanged()
        {
            ConfigurationChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}