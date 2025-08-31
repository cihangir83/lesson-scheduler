using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using LessonScheduler.Models;
using LessonScheduler.Utilities;
using LessonScheduler.Views.Dialogs;

namespace LessonScheduler.ViewModels
{
    public class AssignmentsViewModel : ObservableObject
    {
        private ObservableCollection<string> _classes = new();
        private string? _selectedClass;
        private ObservableCollection<LessonAssignmentViewModel> _assignments = new();
        private LessonAssignmentViewModel? _selectedAssignment;
        private SchoolData? _schoolData;

        public ObservableCollection<string> Classes
        {
            get => _classes;
            set => SetProperty(ref _classes, value);
        }

        public string? SelectedClass
        {
            get => _selectedClass;
            set
            {
                if (SetProperty(ref _selectedClass, value))
                {
                    UpdateAssignmentsDisplay();
                }
            }
        }

        public ObservableCollection<LessonAssignmentViewModel> Assignments
        {
            get => _assignments;
            set => SetProperty(ref _assignments, value);
        }

        public LessonAssignmentViewModel? SelectedAssignment
        {
            get => _selectedAssignment;
            set => SetProperty(ref _selectedAssignment, value);
        }

        // Commands
        public ICommand AddAssignmentCommand { get; }
        public ICommand EditAssignmentCommand { get; }
        public ICommand DeleteAssignmentCommand { get; }

        public AssignmentsViewModel()
        {
            AddAssignmentCommand = new RelayCommand(AddAssignment, CanAddAssignment);
            EditAssignmentCommand = new RelayCommand(EditAssignment, CanEditAssignment);
            DeleteAssignmentCommand = new RelayCommand(DeleteAssignment, CanDeleteAssignment);
        }

        public void UpdateSchoolData(SchoolData? schoolData)
        {
            _schoolData = schoolData;
            
            Classes.Clear();
            
            if (schoolData?.Definitions?.Classes != null)
            {
                foreach (var className in schoolData.Definitions.Classes.OrderBy(c => c))
                {
                    Classes.Add(className);
                }

                // İlk sınıfı seç
                if (Classes.Any())
                {
                    SelectedClass = Classes.First();
                }
            }
        }

        private void UpdateAssignmentsDisplay()
        {
            Assignments.Clear();

            if (string.IsNullOrEmpty(SelectedClass) || _schoolData == null)
                return;

            var classAssignments = _schoolData.GetAssignmentsForClass(SelectedClass);
            
            foreach (var assignment in classAssignments)
            {
                Assignments.Add(new LessonAssignmentViewModel(assignment));
            }
        }

        private bool CanAddAssignment()
        {
            return !string.IsNullOrEmpty(SelectedClass) && 
                   _schoolData?.Definitions?.Lessons?.Any() == true &&
                   _schoolData?.Definitions?.Teachers?.Any() == true;
        }

        private void AddAssignment()
        {
            if (!CanAddAssignment()) return;

            var dialog = new AssignmentDialog(
                _schoolData!.Definitions.Lessons,
                _schoolData.Definitions.Teachers)
            {
                Owner = Application.Current.MainWindow
            };

            if (dialog.ShowDialog() == true && dialog.Result != null)
            {
                // SchoolData'ya ekle
                if (!_schoolData.Assignments.ContainsKey(SelectedClass!))
                {
                    _schoolData.Assignments[SelectedClass!] = new List<LessonAssignment>();
                }

                _schoolData.Assignments[SelectedClass!].Add(dialog.Result);

                // UI'ı güncelle
                UpdateAssignmentsDisplay();
            }
        }

        private bool CanEditAssignment()
        {
            return SelectedAssignment != null && !string.IsNullOrEmpty(SelectedClass);
        }

        private void EditAssignment()
        {
            if (!CanEditAssignment()) return;

            var dialog = new AssignmentDialog(
                _schoolData!.Definitions.Lessons,
                _schoolData.Definitions.Teachers,
                SelectedAssignment!.Assignment)
            {
                Owner = Application.Current.MainWindow
            };

            if (dialog.ShowDialog() == true && dialog.Result != null)
            {
                // SchoolData'da güncelle - mevcut assignment nesnesinin özelliklerini güncelle
                var originalAssignment = SelectedAssignment.Assignment;
                originalAssignment.Ders = dialog.Result.Ders;
                originalAssignment.Ogretmen = dialog.Result.Ogretmen;
                originalAssignment.ToplamSaat = dialog.Result.ToplamSaat;
                originalAssignment.BlokYapisi = dialog.Result.BlokYapisi;
                
                // UI'ı güncelle - hem ViewModel'i refresh et hem de display'i güncelle
                SelectedAssignment.RefreshProperties();
                UpdateAssignmentsDisplay();
            }
        }

        private bool CanDeleteAssignment()
        {
            return SelectedAssignment != null && !string.IsNullOrEmpty(SelectedClass);
        }

        private void DeleteAssignment()
        {
            if (!CanDeleteAssignment()) return;

            var result = MessageBox.Show(
                $"'{SelectedAssignment!.Ders}' dersinin atamasını silmek istediğinizden emin misiniz?",
                "Silme Onayı",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // SchoolData'dan sil
                _schoolData!.Assignments[SelectedClass!].Remove(SelectedAssignment.Assignment);

                // UI'ı güncelle
                UpdateAssignmentsDisplay();
            }
        }
    }

    public class LessonAssignmentViewModel : ObservableObject
    {
        public LessonAssignment Assignment { get; }

        public string Ders => Assignment.Ders;
        public string Ogretmen => Assignment.Ogretmen;
        public int ToplamSaat => Assignment.ToplamSaat;
        public string BlokYapisi => Assignment.BlokYapisi;

        // UI için formatlanmış özellikler
        public string ToplamSaatText => $"{ToplamSaat} saat";
        public string BlokYapisiText => $"Bloklar: {BlokYapisi}";

        public LessonAssignmentViewModel(LessonAssignment assignment)
        {
            Assignment = assignment;
        }

        // UI güncellemesi için tüm property'leri yeniden bildir
        public void RefreshProperties()
        {
            OnPropertyChanged(nameof(Ders));
            OnPropertyChanged(nameof(Ogretmen));
            OnPropertyChanged(nameof(ToplamSaat));
            OnPropertyChanged(nameof(BlokYapisi));
            OnPropertyChanged(nameof(ToplamSaatText));
            OnPropertyChanged(nameof(BlokYapisiText));
        }
    }
}