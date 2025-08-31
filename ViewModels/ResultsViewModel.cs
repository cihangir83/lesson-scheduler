using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using LessonScheduler.Models;
using LessonScheduler.Utilities;

namespace LessonScheduler.ViewModels
{
    public class ResultsViewModel : ObservableObject
    {
        private readonly string[] _days = { "Pzt", "Sal", "Çrş", "Prş", "Cum" };
        private string[] _hours = { "1", "2", "3", "4", "5", "6", "7" };

        private string _mode = "class"; // "class" or "teacher"
        private ObservableCollection<string> _availableItems = new();
        private string? _selectedItem;
        private ObservableCollection<ScheduleRowViewModel> _scheduleRows = new();
        private SolutionData? _currentSolution;

        public string Mode
        {
            get => _mode;
            set
            {
                if (SetProperty(ref _mode, value))
                {
                    UpdateAvailableItems();
                    UpdateScheduleDisplay();
                }
            }
        }

        public ObservableCollection<string> AvailableItems
        {
            get => _availableItems;
            set => SetProperty(ref _availableItems, value);
        }

        public string? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SetProperty(ref _selectedItem, value))
                {
                    UpdateScheduleDisplay();
                }
            }
        }

        public ObservableCollection<ScheduleRowViewModel> ScheduleRows
        {
            get => _scheduleRows;
            set => SetProperty(ref _scheduleRows, value);
        }

        public string[] Days => _days;
        public string[] Hours => _hours;

        private SchoolData? _schoolData;
        public SchoolData? SchoolData
        {
            get => _schoolData;
            set
            {
                if (SetProperty(ref _schoolData, value))
                {
                    UpdateAvailableItems();
                    UpdateScheduleDisplay();
                }
            }
        }

        public void UpdateSolution(SolutionData? solution)
        {
            _currentSolution = solution;
            UpdateScheduleDisplay();
        }

        private void UpdateAvailableItems()
        {
            AvailableItems.Clear();
            
            if (SchoolData == null) return;

            var items = Mode == "class" 
                ? SchoolData.Definitions.Classes.OrderBy(x => x)
                : SchoolData.Definitions.Teachers.OrderBy(x => x);

            foreach (var item in items)
            {
                AvailableItems.Add(item);
            }

            // İlk öğeyi seç
            if (AvailableItems.Any())
            {
                SelectedItem = AvailableItems.First();
            }
        }

        private void UpdateScheduleDisplay()
        {
            ScheduleRows.Clear();

            // Maksimum saat sayısını al ve hours array'ini güncelle
            var maxHours = SchoolData?.GetMaxHoursPerDay() ?? 7;
            UpdateHoursArray(maxHours);

            if (string.IsNullOrEmpty(SelectedItem) || _currentSolution == null)
            {
                // Boş tablo oluştur
                for (int hour = 0; hour < maxHours; hour++)
                {
                    var row = new ScheduleRowViewModel(_hours[hour], SchoolData);
                    ScheduleRows.Add(row);
                }
                return;
            }

            // Çözümden program oluştur
            var schedule = CreateScheduleGrid();
            
            for (int hour = 0; hour < maxHours; hour++)
            {
                var row = new ScheduleRowViewModel(_hours[hour], SchoolData);
                
                for (int day = 0; day < _days.Length; day++)
                {
                    var content = "";
                    var isValidHour = SchoolData != null && hour < SchoolData.GetHoursForDay(day);
                    
                    if (isValidHour && hour < schedule[day].Length)
                    {
                        content = schedule[day][hour];
                    }
                    else if (!isValidHour)
                    {
                        content = "-"; // Bu gün için bu saat yok
                    }
                    
                    row.SetCell(day, content, isValidHour);
                }
                
                ScheduleRows.Add(row);
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

        private string[][] CreateScheduleGrid()
        {
            var maxHours = SchoolData?.GetMaxHoursPerDay() ?? 7;
            
            // 5 gün x maxHours saat grid oluştur
            var schedule = new string[5][];
            for (int day = 0; day < 5; day++)
            {
                schedule[day] = new string[maxHours];
                for (int hour = 0; hour < maxHours; hour++)
                {
                    schedule[day][hour] = "";
                }
            }

            if (_currentSolution == null || string.IsNullOrEmpty(SelectedItem))
                return schedule;

            // Çözümdeki blokları yerleştir
            foreach (var blockEntry in _currentSolution.Blocks.Values)
            {
                bool shouldInclude = Mode == "class" 
                    ? blockEntry.Sinif == SelectedItem
                    : blockEntry.Ogretmen == SelectedItem;

                if (shouldInclude)
                {
                    for (int offset = 0; offset < blockEntry.BlokUzunluk; offset++)
                    {
                        int hour = blockEntry.Start + offset;
                        if (hour < maxHours) // Güvenlik kontrolü
                        {
                            var cellContent = Mode == "class"
                                ? $"{blockEntry.Ders}\n({blockEntry.Ogretmen})"
                                : $"{blockEntry.Ders}\n({blockEntry.Sinif})";
                            
                            schedule[blockEntry.Day][hour] = cellContent;
                        }
                    }
                }
            }

            return schedule;
        }
    }

    public class ScheduleRowViewModel : ObservableObject
    {
        private readonly string[] _cells = new string[5];
        private readonly bool[] _validHours = new bool[5]; // Bu günler için bu saat geçerli mi?
        private readonly SchoolData? _schoolData;

        public string Hour { get; }

        public string Monday
        {
            get => _cells[0];
            set => SetCell(0, value);
        }

        public string Tuesday
        {
            get => _cells[1];
            set => SetCell(1, value);
        }

        public string Wednesday
        {
            get => _cells[2];
            set => SetCell(2, value);
        }

        public string Thursday
        {
            get => _cells[3];
            set => SetCell(3, value);
        }

        public string Friday
        {
            get => _cells[4];
            set => SetCell(4, value);
        }

        public ScheduleRowViewModel(string hour, SchoolData? schoolData = null)
        {
            Hour = hour;
            _schoolData = schoolData;
            for (int i = 0; i < 5; i++)
            {
                _cells[i] = "";
                _validHours[i] = true;
            }
        }

        public void SetCell(int dayIndex, string value, bool isValidHour = true)
        {
            if (dayIndex >= 0 && dayIndex < 5)
            {
                _cells[dayIndex] = value ?? "";
                _validHours[dayIndex] = isValidHour;
                
                // Property change notification
                switch (dayIndex)
                {
                    case 0: OnPropertyChanged(nameof(Monday)); break;
                    case 1: OnPropertyChanged(nameof(Tuesday)); break;
                    case 2: OnPropertyChanged(nameof(Wednesday)); break;
                    case 3: OnPropertyChanged(nameof(Thursday)); break;
                    case 4: OnPropertyChanged(nameof(Friday)); break;
                }
            }
        }

        // UI için stil property'leri
        public string MondayStyle => !_validHours[0] ? "Gray" : "Normal";
        public string TuesdayStyle => !_validHours[1] ? "Gray" : "Normal";
        public string WednesdayStyle => !_validHours[2] ? "Gray" : "Normal";
        public string ThursdayStyle => !_validHours[3] ? "Gray" : "Normal";
        public string FridayStyle => !_validHours[4] ? "Gray" : "Normal";
    }
}