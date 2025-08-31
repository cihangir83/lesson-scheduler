using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using LessonScheduler.Models;
using LessonScheduler.Services;
using LessonScheduler.Utilities;

namespace LessonScheduler.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly IDataService _dataService;
        private readonly IScheduleSolver _scheduleSolver;
        private CancellationTokenSource? _cancellationTokenSource;

        private string _statusMessage = "Hazır";
        private string _qualityReport = "Henüz çözüm oluşturulmadı.";
        private string _solutionSummary = "Henüz çözüm oluşturulmadı.";
        private SchoolData _schoolData = new();
        private bool _isSolverRunning = false;
        


        // Results ViewModels
        public ResultsViewModel ClassResultsViewModel { get; }
        public ResultsViewModel TeacherResultsViewModel { get; }
        
        // Constraints ViewModel
        public ConstraintsViewModel ConstraintsViewModel { get; }
        
        // Assignments ViewModel
        public AssignmentsViewModel AssignmentsViewModel { get; }
        
        // Definitions ViewModel
        public DefinitionsViewModel DefinitionsViewModel { get; }
        
        // Settings ViewModel
        public SettingsViewModel SettingsViewModel { get; }
        
        // Services
        private readonly PdfExportService _pdfExportService;

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public string QualityReport
        {
            get => _qualityReport;
            set => SetProperty(ref _qualityReport, value);
        }

        public string SolutionSummary
        {
            get => _solutionSummary;
            set => SetProperty(ref _solutionSummary, value);
        }

        public SchoolData SchoolData
        {
            get => _schoolData;
            set 
            { 
                if (SetProperty(ref _schoolData, value))
                {
                    // Results ViewModels'lere yeni veriyi aktar
                    ClassResultsViewModel.SchoolData = value;
                    TeacherResultsViewModel.SchoolData = value;
                    
                    // Constraints ViewModel'e yeni veriyi aktar
                    ConstraintsViewModel.UpdateTeachers(value);
                    
                    // Assignments ViewModel'e yeni veriyi aktar
                    AssignmentsViewModel.UpdateSchoolData(value);
                    
                    // Definitions ViewModel'e yeni veriyi aktar
                    DefinitionsViewModel.UpdateSchoolData(value);
                    
                    // Settings ViewModel'e yeni veriyi aktar
                    SettingsViewModel.UpdateSchoolData(value);
                    
                    // Configuration değişikliklerini dinle
                    SettingsViewModel.ConfigurationChanged += OnConfigurationChanged;
                }
            }
        }

        public bool IsSolverRunning
        {
            get => _isSolverRunning;
            set => SetProperty(ref _isSolverRunning, value);
        }

        // Commands
        public ICommand LoadJsonCommand { get; }
        public ICommand SaveJsonCommand { get; }
        public ICommand LoadTestDataCommand { get; }
        public ICommand RunSolverCommand { get; }
        

        
        // PDF Export Commands
        public ICommand ExportClassPdfCommand { get; }
        public ICommand ExportClassPdfSeparateCommand { get; }
        public ICommand ExportTeacherPdfCommand { get; }
        public ICommand ExportTeacherPdfSeparateCommand { get; }


        public MainViewModel()
        {
            _dataService = new JsonDataService();
            _scheduleSolver = new OrToolsScheduleSolver();
            _pdfExportService = new PdfExportService();

            // Results ViewModels'leri initialize et
            ClassResultsViewModel = new ResultsViewModel { Mode = "class" };
            TeacherResultsViewModel = new ResultsViewModel { Mode = "teacher" };
            
            // Constraints ViewModel'i initialize et
            ConstraintsViewModel = new ConstraintsViewModel();
            
            // Assignments ViewModel'i initialize et
            AssignmentsViewModel = new AssignmentsViewModel();
            
            // Definitions ViewModel'i initialize et
            DefinitionsViewModel = new DefinitionsViewModel();

            // Settings ViewModel'i initialize et
            SettingsViewModel = new SettingsViewModel();

            // İlk açılışta boş ama geçerli veri yapısı oluştur
            InitializeDefaultData();

            // Commands
            LoadJsonCommand = new RelayCommand(async () => await LoadJsonAsync(), () => !IsSolverRunning);
            SaveJsonCommand = new RelayCommand(async () => await SaveJsonAsync(), () => !IsSolverRunning);
            LoadTestDataCommand = new RelayCommand(LoadTestData, () => !IsSolverRunning);
            RunSolverCommand = new RelayCommand(async () => await RunSolverAsync(), () => !IsSolverRunning && HasValidData());

            
            // PDF Export Commands
            ExportClassPdfCommand = new RelayCommand(async () => await ExportClassPdfAsync(), () => HasSolution());
            ExportClassPdfSeparateCommand = new RelayCommand(async () => await ExportClassPdfSeparateAsync(), () => HasSolution());
            ExportTeacherPdfCommand = new RelayCommand(async () => await ExportTeacherPdfAsync(), () => HasSolution());
            ExportTeacherPdfSeparateCommand = new RelayCommand(async () => await ExportTeacherPdfSeparateAsync(), () => HasSolution());

        }

        private void InitializeDefaultData()
        {
            // İlk açılışta boş ama geçerli veri yapısı oluştur
            SchoolData = SchoolData.CreateEmpty();
            StatusMessage = "Program hazır. Tanımlamalar sekmesinden başlayabilirsiniz.";
        }

        private void OnConfigurationChanged(object? sender, EventArgs e)
        {
            try
            {
                // Yapılandırma değiştiğinde diğer ViewModellere bildir
                ConstraintsViewModel.UpdateTeachers(SchoolData);
                
                StatusMessage = "Okul ayarları güncellendi. Kısıtlamalar yeni yapılandırmaya göre ayarlandı.";
                
                // Eğer çözüm varsa, yeniden çözülmesi gerektiğini bildir
                if (SchoolData.Solution != null)
                {
                    StatusMessage += " Mevcut çözüm temizlendi, yeniden dağıtım yapmanız önerilir.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Yapılandırma güncellenirken hata oluştu: {ex.Message}";
                MessageBox.Show($"Yapılandırma güncellenirken hata oluştu:\n{ex.Message}", 
                              "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadJsonAsync()
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                    Title = "JSON Dosyası Seç"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    StatusMessage = "JSON dosyası yükleniyor...";
                    
                    var loadedData = await _dataService.LoadFromJsonAsync(openFileDialog.FileName);
                    SchoolData = loadedData;
                    
                    UpdateResultsFromSolution();
                    StatusMessage = "JSON dosyası başarıyla yüklendi.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"JSON yükleme hatası: {ex.Message}";
                MessageBox.Show($"JSON dosyası yüklenirken hata oluştu:\n{ex.Message}", 
                              "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task SaveJsonAsync()
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                    Title = "JSON Dosyası Kaydet",
                    DefaultExt = "json"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    StatusMessage = "JSON dosyası kaydediliyor...";
                    
                    await _dataService.SaveToJsonAsync(SchoolData, saveFileDialog.FileName);
                    StatusMessage = "JSON dosyası başarıyla kaydedildi.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"JSON kaydetme hatası: {ex.Message}";
                MessageBox.Show($"JSON dosyası kaydedilirken hata oluştu:\n{ex.Message}", 
                              "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadTestData()
        {
            try
            {
                StatusMessage = "Test verisi yükleniyor...";
                
                SchoolData = _dataService.LoadTestData();
                UpdateResultsFromSolution();
                StatusMessage = "Test verisi başarıyla yüklendi.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Test verisi yükleme hatası: {ex.Message}";
                MessageBox.Show($"Test verisi yüklenirken hata oluştu:\n{ex.Message}", 
                              "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task RunSolverAsync()
        {
            try
            {
                IsSolverRunning = true;
                _cancellationTokenSource = new CancellationTokenSource();
                StatusMessage = "Ders programı oluşturuluyor...";

                // 🎯 Yeni Progress Dialog
                var progressDialog = new LessonScheduler.Views.Dialogs.SmartProgressDialog();
                progressDialog.Owner = Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
                progressDialog.Show();

                // Progress callback
                var progress = new Progress<SolverProgress>(p =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        progressDialog.UpdateProgress(p);
                    });
                });

                StatusMessage = "Akıllı çözüm algoritması başlatılıyor...";

                var (solution, message) = await _scheduleSolver.SolveScheduleAsync(SchoolData, progress, _cancellationTokenSource.Token);

                // Çözüm kontrolü
                if (solution != null)
                {
                    progressDialog.ShowSuccess("✅ Çözüm Bulundu!", message);
                    await Task.Delay(2000); // 2 saniye başarı göster
                    
                    SchoolData.Solution = solution;
                    UpdateResultsFromSolution();
                    StatusMessage = message;
                }
                else
                {
                    progressDialog.ShowError("❌ Çözüm Bulunamadı", message);
                    await Task.Delay(3000); // 3 saniye hata göster
                    StatusMessage = message;
                }

                try
                {
                    progressDialog.Close();
                }
                catch
                {
                    // Dialog zaten kapatılmışsa sessizce devam et
                }
            }
            catch (OperationCanceledException)
            {
                StatusMessage = "İşlem iptal edildi.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Solver hatası: {ex.Message}";
                MessageBox.Show($"Ders programı oluşturulurken hata oluştu:\n{ex.Message}",
                              "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsSolverRunning = false;
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        private void UpdateResultsFromSolution()
        {
            if (SchoolData.Solution != null)
            {
                // Kalite raporu oluştur
                QualityReport = GenerateQualityReport(SchoolData.Solution);
                
                // Çözüm özeti oluştur
                SolutionSummary = GenerateSolutionSummary(SchoolData.Solution);

                // Results ViewModels'lere çözümü aktar
                ClassResultsViewModel.UpdateSolution(SchoolData.Solution);
                TeacherResultsViewModel.UpdateSolution(SchoolData.Solution);
            }
            else
            {
                QualityReport = "Henüz çözüm oluşturulmadı.";
                SolutionSummary = "Henüz çözüm oluşturulmadı.";

                // Results ViewModels'leri temizle
                ClassResultsViewModel.UpdateSolution(null);
                TeacherResultsViewModel.UpdateSolution(null);
            }
        }

        private string GenerateQualityReport(SolutionData solution)
        {
            // Python kodundaki SolutionQualityEvaluator mantığı
            var teacherWorkloads = new Dictionary<string, Dictionary<int, int>>();
            
            foreach (var blockEntry in solution.Blocks)
            {
                var block = blockEntry.Value;
                
                if (!teacherWorkloads.ContainsKey(block.Ogretmen))
                    teacherWorkloads[block.Ogretmen] = new Dictionary<int, int>();
                
                if (!teacherWorkloads[block.Ogretmen].ContainsKey(block.Day))
                    teacherWorkloads[block.Ogretmen][block.Day] = 0;
                
                teacherWorkloads[block.Ogretmen][block.Day] += block.BlokUzunluk;
            }

            var balanceScores = new List<double>();
            
            foreach (var teacherWorkload in teacherWorkloads)
            {
                var dailyHours = new List<int>();
                for (int day = 0; day < 5; day++)
                {
                    dailyHours.Add(teacherWorkload.Value.GetValueOrDefault(day, 0));
                }

                var avg = dailyHours.Average();
                if (avg == 0) continue;

                var variance = dailyHours.Select(h => Math.Pow(h - avg, 2)).Average();
                balanceScores.Add(1.0 / (1.0 + variance));
            }

            var balance = balanceScores.Any() ? balanceScores.Average() : 0;
            var score = balance * 100;

            return $"ÇÖZÜM KALİTE RAPORU\n" +
                   $"====================\n" +
                   $"Öğretmen Yük Dengesi: {balance:F2}/1.00\n\n" +
                   $"GENEL SKOR: {score:F1}/100\n\n" +
                   $"Çözüm Süresi: {solution.SolutionTime:F2} saniye\n" +
                   $"Toplam Blok Sayısı: {solution.Blocks.Count}";
        }

        private string GenerateSolutionSummary(SolutionData solution)
        {
            var summary = "ÇÖZÜM ÖZETİ\n";
            summary += "============\n\n";

            var days = new[] { "Pazartesi", "Salı", "Çarşamba", "Perşembe", "Cuma" };

            foreach (var blockEntry in solution.Blocks.OrderBy(b => b.Value.Day).ThenBy(b => b.Value.Start))
            {
                var block = blockEntry.Value;
                var dayName = days[block.Day];
                var startHour = block.Start + 1; // 1-based indexing for display
                var endHour = startHour + block.BlokUzunluk - 1;

                summary += $"{block.Sinif} - {block.Ders} ({block.Ogretmen})\n";
                summary += $"  {dayName}, {startHour}. saat";
                if (block.BlokUzunluk > 1)
                    summary += $" - {endHour}. saat ({block.BlokUzunluk} saat)";
                summary += "\n\n";
            }

            return summary;
        }

        private bool HasValidData()
        {
            return SchoolData.Definitions.Lessons.Count > 0 &&
                   SchoolData.Definitions.Teachers.Count > 0 &&
                   SchoolData.Definitions.Classes.Count > 0 &&
                   SchoolData.Assignments.Count > 0;
        }

        private bool HasSolution()
        {
            return SchoolData.Solution != null && SchoolData.Solution.Blocks.Count > 0;
        }

        // Dark tema kaldırıldığı için tema değiştirme işlevi devre dışı bırakıldı
        private void ToggleTheme()
        {
            // No-op
            StatusMessage = $"Tema: {ThemeService.Instance.GetCurrentThemeDisplayName()}";
        }

        private async Task ExportClassPdfAsync()
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "PDF files (*.pdf)|*.pdf",
                    Title = "Sınıf Programları PDF Kaydet",
                    FileName = "Sinif_Programlari.pdf",
                    DefaultExt = "pdf"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    StatusMessage = "Sınıf programları PDF'e aktarılıyor...";
                    
                    await _pdfExportService.ExportClassSchedulesToPdfAsync(SchoolData, saveFileDialog.FileName, false);
                    StatusMessage = "Sınıf programları PDF'e başarıyla aktarıldı.";
                    
                    MessageBox.Show("Sınıf programları PDF dosyasına başarıyla aktarıldı!", 
                                  "PDF Export", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"PDF export hatası: {ex.Message}";
                var detailedError = $"PDF export sırasında hata oluştu:\n\nHata: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}\n\nInner Exception: {ex.InnerException?.Message}";
                MessageBox.Show(detailedError, "PDF Export Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ExportClassPdfSeparateAsync()
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "PDF files (*.pdf)|*.pdf",
                    Title = "Sınıf Programları (Ayrı Dosyalar) PDF Kaydet",
                    FileName = "Sinif_Programlari",
                    DefaultExt = "pdf"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    StatusMessage = "Sınıf programları ayrı PDF dosyalarına aktarılıyor...";
                    
                    await _pdfExportService.ExportClassSchedulesToPdfAsync(SchoolData, saveFileDialog.FileName, true);
                    StatusMessage = "Sınıf programları ayrı PDF dosyalarına başarıyla aktarıldı.";
                    
                    MessageBox.Show("Her sınıf için ayrı PDF dosyası oluşturuldu!", 
                                  "PDF Export", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"PDF export hatası: {ex.Message}";
                MessageBox.Show($"PDF export sırasında hata oluştu:\n{ex.Message}", 
                              "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ExportTeacherPdfAsync()
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "PDF files (*.pdf)|*.pdf",
                    Title = "Öğretmen Programları PDF Kaydet",
                    FileName = "Ogretmen_Programlari.pdf",
                    DefaultExt = "pdf"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    StatusMessage = "Öğretmen programları PDF'e aktarılıyor...";
                    
                    await _pdfExportService.ExportTeacherSchedulesToPdfAsync(SchoolData, saveFileDialog.FileName, false);
                    StatusMessage = "Öğretmen programları PDF'e başarıyla aktarıldı.";
                    
                    MessageBox.Show("Öğretmen programları PDF dosyasına başarıyla aktarıldı!", 
                                  "PDF Export", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"PDF export hatası: {ex.Message}";
                MessageBox.Show($"PDF export sırasında hata oluştu:\n{ex.Message}", 
                              "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ExportTeacherPdfSeparateAsync()
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "PDF files (*.pdf)|*.pdf",
                    Title = "Öğretmen Programları (Ayrı Dosyalar) PDF Kaydet",
                    FileName = "Ogretmen_Programlari",
                    DefaultExt = "pdf"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    StatusMessage = "Öğretmen programları ayrı PDF dosyalarına aktarılıyor...";
                    
                    await _pdfExportService.ExportTeacherSchedulesToPdfAsync(SchoolData, saveFileDialog.FileName, true);
                    StatusMessage = "Öğretmen programları ayrı PDF dosyalarına başarıyla aktarıldı.";
                    
                    MessageBox.Show("Her öğretmen için ayrı PDF dosyası oluşturuldu!", 
                                  "PDF Export", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"PDF export hatası: {ex.Message}";
                MessageBox.Show($"PDF export sırasında hata oluştu:\n{ex.Message}", 
                              "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}