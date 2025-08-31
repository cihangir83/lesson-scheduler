using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using LessonScheduler.Models;
using LessonScheduler.Utilities;
using Microsoft.VisualBasic;

namespace LessonScheduler.ViewModels
{
    public class DefinitionsViewModel : ObservableObject
    {
        private ObservableCollection<string> _lessons = new();
        private ObservableCollection<string> _teachers = new();
        private ObservableCollection<string> _classes = new();
        
        private string? _selectedLesson;
        private string? _selectedTeacher;
        private string? _selectedClass;
        
        private SchoolData? _schoolData;

        public ObservableCollection<string> Lessons
        {
            get => _lessons;
            set => SetProperty(ref _lessons, value);
        }

        public ObservableCollection<string> Teachers
        {
            get => _teachers;
            set => SetProperty(ref _teachers, value);
        }

        public ObservableCollection<string> Classes
        {
            get => _classes;
            set => SetProperty(ref _classes, value);
        }

        public string? SelectedLesson
        {
            get => _selectedLesson;
            set => SetProperty(ref _selectedLesson, value);
        }

        public string? SelectedTeacher
        {
            get => _selectedTeacher;
            set => SetProperty(ref _selectedTeacher, value);
        }

        public string? SelectedClass
        {
            get => _selectedClass;
            set => SetProperty(ref _selectedClass, value);
        }

        // Lesson Commands
        public ICommand AddLessonCommand { get; }
        public ICommand EditLessonCommand { get; }
        public ICommand DeleteLessonCommand { get; }

        // Teacher Commands
        public ICommand AddTeacherCommand { get; }
        public ICommand EditTeacherCommand { get; }
        public ICommand DeleteTeacherCommand { get; }

        // Class Commands
        public ICommand AddClassCommand { get; }
        public ICommand EditClassCommand { get; }
        public ICommand DeleteClassCommand { get; }

        public DefinitionsViewModel()
        {
            // Lesson Commands
            AddLessonCommand = new RelayCommand(AddLesson);
            EditLessonCommand = new RelayCommand(EditLesson, () => !string.IsNullOrEmpty(SelectedLesson));
            DeleteLessonCommand = new RelayCommand(DeleteLesson, () => !string.IsNullOrEmpty(SelectedLesson));

            // Teacher Commands
            AddTeacherCommand = new RelayCommand(AddTeacher);
            EditTeacherCommand = new RelayCommand(EditTeacher, () => !string.IsNullOrEmpty(SelectedTeacher));
            DeleteTeacherCommand = new RelayCommand(DeleteTeacher, () => !string.IsNullOrEmpty(SelectedTeacher));

            // Class Commands
            AddClassCommand = new RelayCommand(AddClass);
            EditClassCommand = new RelayCommand(EditClass, () => !string.IsNullOrEmpty(SelectedClass));
            DeleteClassCommand = new RelayCommand(DeleteClass, () => !string.IsNullOrEmpty(SelectedClass));
        }

        public void UpdateSchoolData(SchoolData? schoolData)
        {
            _schoolData = schoolData;
            RefreshAllLists();
        }

        private void RefreshAllLists()
        {
            RefreshLessons();
            RefreshTeachers();
            RefreshClasses();
        }

        private void RefreshLessons()
        {
            var currentSelection = SelectedLesson;
            Lessons.Clear();
            
            if (_schoolData?.Definitions?.Lessons != null)
            {
                foreach (var lesson in _schoolData.Definitions.Lessons.OrderBy(l => l))
                {
                    Lessons.Add(lesson);
                }
            }

            // Seçimi geri yükle
            if (!string.IsNullOrEmpty(currentSelection) && Lessons.Contains(currentSelection))
            {
                SelectedLesson = currentSelection;
            }
        }

        private void RefreshTeachers()
        {
            var currentSelection = SelectedTeacher;
            Teachers.Clear();
            
            if (_schoolData?.Definitions?.Teachers != null)
            {
                foreach (var teacher in _schoolData.Definitions.Teachers.OrderBy(t => t))
                {
                    Teachers.Add(teacher);
                }
            }

            // Seçimi geri yükle
            if (!string.IsNullOrEmpty(currentSelection) && Teachers.Contains(currentSelection))
            {
                SelectedTeacher = currentSelection;
            }
        }

        private void RefreshClasses()
        {
            var currentSelection = SelectedClass;
            Classes.Clear();
            
            if (_schoolData?.Definitions?.Classes != null)
            {
                foreach (var className in _schoolData.Definitions.Classes.OrderBy(c => c))
                {
                    Classes.Add(className);
                }
            }

            // Seçimi geri yükle
            if (!string.IsNullOrEmpty(currentSelection) && Classes.Contains(currentSelection))
            {
                SelectedClass = currentSelection;
            }
        }

        // Lesson Methods
        private void AddLesson()
        {
            var input = Interaction.InputBox("Yeni ders adını girin:", "Ders Ekle", "");
            
            if (!string.IsNullOrWhiteSpace(input))
            {
                input = input.Trim();
                
                if (_schoolData?.Definitions?.Lessons?.Contains(input) == true)
                {
                    MessageBox.Show("Bu ders zaten mevcut.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _schoolData?.Definitions?.Lessons?.Add(input);
                RefreshLessons();
                SelectedLesson = input;
            }
        }

        private void EditLesson()
        {
            if (string.IsNullOrEmpty(SelectedLesson) || _schoolData?.Definitions?.Lessons == null)
                return;

            var input = Interaction.InputBox("Ders adını düzenleyin:", "Ders Düzenle", SelectedLesson);
            
            if (!string.IsNullOrWhiteSpace(input))
            {
                input = input.Trim();
                
                if (input == SelectedLesson)
                    return; // Değişiklik yok

                if (_schoolData.Definitions.Lessons.Contains(input))
                {
                    MessageBox.Show("Bu ders adı zaten mevcut.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var index = _schoolData.Definitions.Lessons.IndexOf(SelectedLesson);
                if (index >= 0)
                {
                    _schoolData.Definitions.Lessons[index] = input;
                    RefreshLessons();
                    SelectedLesson = input;
                }
            }
        }

        private void DeleteLesson()
        {
            if (string.IsNullOrEmpty(SelectedLesson) || _schoolData?.Definitions?.Lessons == null)
                return;

            var result = MessageBox.Show(
                $"'{SelectedLesson}' dersini silmek istediğinizden emin misiniz?",
                "Silme Onayı",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _schoolData.Definitions.Lessons.Remove(SelectedLesson);
                RefreshLessons();
                SelectedLesson = null;
            }
        }

        // Teacher Methods
        private void AddTeacher()
        {
            var input = Interaction.InputBox("Yeni öğretmen adını girin:", "Öğretmen Ekle", "");
            
            if (!string.IsNullOrWhiteSpace(input))
            {
                input = input.Trim();
                
                if (_schoolData?.Definitions?.Teachers?.Contains(input) == true)
                {
                    MessageBox.Show("Bu öğretmen zaten mevcut.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _schoolData?.Definitions?.Teachers?.Add(input);
                _schoolData?.AddTeacher(input); // Kısıtlamalar için varsayılan değerler oluştur
                RefreshTeachers();
                SelectedTeacher = input;
            }
        }

        private void EditTeacher()
        {
            if (string.IsNullOrEmpty(SelectedTeacher) || _schoolData?.Definitions?.Teachers == null)
                return;

            var input = Interaction.InputBox("Öğretmen adını düzenleyin:", "Öğretmen Düzenle", SelectedTeacher);
            
            if (!string.IsNullOrWhiteSpace(input))
            {
                input = input.Trim();
                
                if (input == SelectedTeacher)
                    return; // Değişiklik yok

                if (_schoolData.Definitions.Teachers.Contains(input))
                {
                    MessageBox.Show("Bu öğretmen adı zaten mevcut.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var index = _schoolData.Definitions.Teachers.IndexOf(SelectedTeacher);
                if (index >= 0)
                {
                    // Kısıtlamalarda da güncelle
                    if (_schoolData.Constraints.ContainsKey(SelectedTeacher))
                    {
                        var constraints = _schoolData.Constraints[SelectedTeacher];
                        _schoolData.Constraints.Remove(SelectedTeacher);
                        _schoolData.Constraints[input] = constraints;
                    }

                    _schoolData.Definitions.Teachers[index] = input;
                    RefreshTeachers();
                    SelectedTeacher = input;
                }
            }
        }

        private void DeleteTeacher()
        {
            if (string.IsNullOrEmpty(SelectedTeacher) || _schoolData?.Definitions?.Teachers == null)
                return;

            var result = MessageBox.Show(
                $"'{SelectedTeacher}' öğretmenini silmek istediğinizden emin misiniz?",
                "Silme Onayı",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _schoolData.Definitions.Teachers.Remove(SelectedTeacher);
                
                // Kısıtlamalardan da sil
                _schoolData.Constraints.Remove(SelectedTeacher);
                
                RefreshTeachers();
                SelectedTeacher = null;
            }
        }

        // Class Methods
        private void AddClass()
        {
            var input = Interaction.InputBox("Yeni sınıf adını girin:", "Sınıf Ekle", "");
            
            if (!string.IsNullOrWhiteSpace(input))
            {
                input = input.Trim();
                
                if (_schoolData?.Definitions?.Classes?.Contains(input) == true)
                {
                    MessageBox.Show("Bu sınıf zaten mevcut.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _schoolData?.Definitions?.Classes?.Add(input);
                _schoolData?.AddClass(input); // Ders atamaları için boş liste oluştur
                RefreshClasses();
                SelectedClass = input;
            }
        }

        private void EditClass()
        {
            if (string.IsNullOrEmpty(SelectedClass) || _schoolData?.Definitions?.Classes == null)
                return;

            var input = Interaction.InputBox("Sınıf adını düzenleyin:", "Sınıf Düzenle", SelectedClass);
            
            if (!string.IsNullOrWhiteSpace(input))
            {
                input = input.Trim();
                
                if (input == SelectedClass)
                    return; // Değişiklik yok

                if (_schoolData.Definitions.Classes.Contains(input))
                {
                    MessageBox.Show("Bu sınıf adı zaten mevcut.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var index = _schoolData.Definitions.Classes.IndexOf(SelectedClass);
                if (index >= 0)
                {
                    // Ders atamalarında da güncelle
                    if (_schoolData.Assignments.ContainsKey(SelectedClass))
                    {
                        var assignments = _schoolData.Assignments[SelectedClass];
                        _schoolData.Assignments.Remove(SelectedClass);
                        _schoolData.Assignments[input] = assignments;
                    }

                    _schoolData.Definitions.Classes[index] = input;
                    RefreshClasses();
                    SelectedClass = input;
                }
            }
        }

        private void DeleteClass()
        {
            if (string.IsNullOrEmpty(SelectedClass) || _schoolData?.Definitions?.Classes == null)
                return;

            var result = MessageBox.Show(
                $"'{SelectedClass}' sınıfını silmek istediğinizden emin misiniz?",
                "Silme Onayı",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _schoolData.Definitions.Classes.Remove(SelectedClass);
                
                // Ders atamalarından da sil
                _schoolData.Assignments.Remove(SelectedClass);
                
                RefreshClasses();
                SelectedClass = null;
            }
        }
    }
}