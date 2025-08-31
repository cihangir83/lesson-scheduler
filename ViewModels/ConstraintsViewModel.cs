using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using LessonScheduler.Models;
using LessonScheduler.Utilities;

namespace LessonScheduler.ViewModels
{
    public class ConstraintsViewModel : ObservableObject
    {
        private readonly string[] _days = { "Pzt", "Sal", "Çrş", "Prş", "Cum" };
        private string[] _hours = { "1", "2", "3", "4", "5", "6", "7" };

        private ObservableCollection<string> _teachers = new();
        private string? _selectedTeacher;
        private ObservableCollection<ConstraintRowViewModel> _constraintRows = new();
        private SchoolData? _schoolData;

        public ObservableCollection<string> Teachers
        {
            get => _teachers;
            set => SetProperty(ref _teachers, value);
        }

        public string? SelectedTeacher
        {
            get => _selectedTeacher;
            set
            {
                if (SetProperty(ref _selectedTeacher, value))
                {
                    UpdateConstraintDisplay();
                }
            }
        }

        public ObservableCollection<ConstraintRowViewModel> ConstraintRows
        {
            get => _constraintRows;
            set => SetProperty(ref _constraintRows, value);
        }

        public string[] Days => _days;
        public string[] Hours => _hours;

        public ICommand CellClickCommand { get; }
        public ICommand ShowTeacherStatusCommand { get; }

        public ConstraintsViewModel()
        {
            CellClickCommand = new RelayCommand<string>(OnCellClick);
            ShowTeacherStatusCommand = new RelayCommand(OnShowTeacherStatus);
        }

        public void UpdateTeachers(SchoolData? schoolData)
        {
            _schoolData = schoolData;
            
            Teachers.Clear();
            
            if (schoolData?.Definitions?.Teachers != null)
            {
                foreach (var teacher in schoolData.Definitions.Teachers.OrderBy(t => t))
                {
                    Teachers.Add(teacher);
                }

                // İlk öğretmeni seç
                if (Teachers.Any())
                {
                    SelectedTeacher = Teachers.First();
                }
            }
        }

        private void UpdateConstraintDisplay()
        {
            ConstraintRows.Clear();

            if (string.IsNullOrEmpty(SelectedTeacher) || _schoolData == null)
            {
                // Boş tablo oluştur - maksimum saat sayısına göre
                var maxHours = _schoolData?.GetMaxHoursPerDay() ?? 7;
                UpdateHoursArray(maxHours);
                
                for (int hour = 0; hour < maxHours; hour++)
                {
                    var row = new ConstraintRowViewModel(_hours[hour], this, _schoolData);
                    ConstraintRows.Add(row);
                }
                return;
            }

            // Maksimum saat sayısını al ve hours array'ini güncelle
            var maxHoursForSchedule = _schoolData.GetMaxHoursPerDay();
            UpdateHoursArray(maxHoursForSchedule);

            // Öğretmenin kısıtlamalarını al
            var constraints = _schoolData.GetConstraintsForTeacher(SelectedTeacher);
            
            for (int hour = 0; hour < maxHoursForSchedule; hour++)
            {
                var row = new ConstraintRowViewModel(_hours[hour], this, _schoolData);
                
                for (int day = 0; day < _days.Length; day++)
                {
                    bool isAvailable = true; // Varsayılan: müsait
                    bool isValidHour = hour < _schoolData.GetHoursForDay(day); // Bu gün için geçerli saat mi?
                    
                    if (constraints != null && constraints.TryGetValue(day.ToString(), out var dayConstraints))
                    {
                        if (hour < dayConstraints.Length)
                        {
                            isAvailable = dayConstraints[hour];
                        }
                    }
                    
                    row.SetCell(day, isAvailable, isValidHour);
                }
                
                ConstraintRows.Add(row);
            }
        }

        private void UpdateHoursArray(int maxHours)
        {
            _hours = new string[maxHours];
            for (int i = 0; i < maxHours; i++)
            {
                _hours[i] = (i + 1).ToString();
            }
        }

        private void OnCellClick(string? parameter)
        {
            if (string.IsNullOrEmpty(parameter) || string.IsNullOrEmpty(SelectedTeacher) || _schoolData == null)
                return;

            // Parameter format: "day,hour" (örn: "0,2")
            var parts = parameter.Split(',');
            if (parts.Length != 2 || !int.TryParse(parts[0], out int day) || !int.TryParse(parts[1], out int hour))
                return;

            var maxHours = _schoolData.GetMaxHoursPerDay();
            if (day < 0 || day >= 5 || hour < 0 || hour >= maxHours)
                return;

            // Bu gün için bu saat geçerli mi kontrol et
            if (hour >= _schoolData.GetHoursForDay(day))
                return; // Geçersiz saat, tıklamayı ignore et

            // Öğretmenin kısıtlamalarını al veya oluştur
            if (!_schoolData.Constraints.ContainsKey(SelectedTeacher))
            {
                _schoolData.AddTeacher(SelectedTeacher);
            }

            var constraints = _schoolData.Constraints[SelectedTeacher];
            var dayKey = day.ToString();
            
            if (!constraints.ContainsKey(dayKey))
            {
                var hoursForDay = _schoolData.GetHoursForDay(day);
                constraints[dayKey] = Enumerable.Repeat(true, hoursForDay).ToArray();
            }

            // Mevcut durumu tersine çevir
            constraints[dayKey][hour] = !constraints[dayKey][hour];

            // UI'ı güncelle
            if (hour < ConstraintRows.Count)
            {
                var isValidHour = hour < _schoolData.GetHoursForDay(day);
                ConstraintRows[hour].SetCell(day, constraints[dayKey][hour], isValidHour);
            }
        }

        public void OnCellClicked(int day, int hour)
        {
            OnCellClick($"{day},{hour}");
        }

        private void OnShowTeacherStatus()
        {
            if (_schoolData == null) return;

            var statusWindow = new Views.Dialogs.TeacherStatusDialog(_schoolData);
            statusWindow.ShowDialog();
        }
    }

    public class ConstraintRowViewModel : ObservableObject
    {
        private readonly ConstraintsViewModel _parent;
        private readonly SchoolData? _schoolData;
        private readonly bool[] _cells = new bool[5];
        private readonly bool[] _validHours = new bool[5]; // Bu günler için bu saat geçerli mi?

        public string Hour { get; }

        public bool Monday
        {
            get => _cells[0];
            set => SetCell(0, value);
        }

        public bool Tuesday
        {
            get => _cells[1];
            set => SetCell(1, value);
        }

        public bool Wednesday
        {
            get => _cells[2];
            set => SetCell(2, value);
        }

        public bool Thursday
        {
            get => _cells[3];
            set => SetCell(3, value);
        }

        public bool Friday
        {
            get => _cells[4];
            set => SetCell(4, value);
        }

        // UI için renk property'leri
        public string MondayColor => !_validHours[0] ? "Gray" : (Monday ? "LightGreen" : "LightCoral");
        public string TuesdayColor => !_validHours[1] ? "Gray" : (Tuesday ? "LightGreen" : "LightCoral");
        public string WednesdayColor => !_validHours[2] ? "Gray" : (Wednesday ? "LightGreen" : "LightCoral");
        public string ThursdayColor => !_validHours[3] ? "Gray" : (Thursday ? "LightGreen" : "LightCoral");
        public string FridayColor => !_validHours[4] ? "Gray" : (Friday ? "LightGreen" : "LightCoral");

        // UI için metin property'leri
        public string MondayText => !_validHours[0] ? "-" : (Monday ? "Müsait" : "Müsait Değil");
        public string TuesdayText => !_validHours[1] ? "-" : (Tuesday ? "Müsait" : "Müsait Değil");
        public string WednesdayText => !_validHours[2] ? "-" : (Wednesday ? "Müsait" : "Müsait Değil");
        public string ThursdayText => !_validHours[3] ? "-" : (Thursday ? "Müsait" : "Müsait Değil");
        public string FridayText => !_validHours[4] ? "-" : (Friday ? "Müsait" : "Müsait Değil");

        // Click command'ları
        public ICommand MondayClickCommand { get; }
        public ICommand TuesdayClickCommand { get; }
        public ICommand WednesdayClickCommand { get; }
        public ICommand ThursdayClickCommand { get; }
        public ICommand FridayClickCommand { get; }

        public ConstraintRowViewModel(string hour, ConstraintsViewModel parent, SchoolData? schoolData)
        {
            Hour = hour;
            _parent = parent;
            _schoolData = schoolData;
            
            // Varsayılan olarak tüm saatler müsait ve geçerli
            for (int i = 0; i < 5; i++)
            {
                _cells[i] = true;
                _validHours[i] = true;
            }

            // Click command'larını oluştur
            MondayClickCommand = new RelayCommand(() => _parent.OnCellClicked(0, int.Parse(hour) - 1));
            TuesdayClickCommand = new RelayCommand(() => _parent.OnCellClicked(1, int.Parse(hour) - 1));
            WednesdayClickCommand = new RelayCommand(() => _parent.OnCellClicked(2, int.Parse(hour) - 1));
            ThursdayClickCommand = new RelayCommand(() => _parent.OnCellClicked(3, int.Parse(hour) - 1));
            FridayClickCommand = new RelayCommand(() => _parent.OnCellClicked(4, int.Parse(hour) - 1));
        }

        public void SetCell(int dayIndex, bool value, bool isValidHour = true)
        {
            if (dayIndex >= 0 && dayIndex < 5)
            {
                _cells[dayIndex] = value;
                _validHours[dayIndex] = isValidHour;
                
                // Property change notification
                switch (dayIndex)
                {
                    case 0: 
                        OnPropertyChanged(nameof(Monday)); 
                        OnPropertyChanged(nameof(MondayColor));
                        OnPropertyChanged(nameof(MondayText));
                        break;
                    case 1: 
                        OnPropertyChanged(nameof(Tuesday)); 
                        OnPropertyChanged(nameof(TuesdayColor));
                        OnPropertyChanged(nameof(TuesdayText));
                        break;
                    case 2: 
                        OnPropertyChanged(nameof(Wednesday)); 
                        OnPropertyChanged(nameof(WednesdayColor));
                        OnPropertyChanged(nameof(WednesdayText));
                        break;
                    case 3: 
                        OnPropertyChanged(nameof(Thursday)); 
                        OnPropertyChanged(nameof(ThursdayColor));
                        OnPropertyChanged(nameof(ThursdayText));
                        break;
                    case 4: 
                        OnPropertyChanged(nameof(Friday)); 
                        OnPropertyChanged(nameof(FridayColor));
                        OnPropertyChanged(nameof(FridayText));
                        break;
                }
            }
        }
    }
}