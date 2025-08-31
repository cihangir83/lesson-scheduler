using LessonScheduler.Utilities;

namespace LessonScheduler.Models
{
    public class DayHourSetting : ObservableObject
    {
        private int _hours;

        public string DayName { get; set; } = string.Empty;
        public int DayIndex { get; set; }
        
        public int Hours
        {
            get => _hours;
            set
            {
                if (value >= MinHours && value <= MaxHours)
                {
                    SetProperty(ref _hours, value);
                }
            }
        }

        public int MinHours { get; set; } = 1;
        public int MaxHours { get; set; } = 10;

        public DayHourSetting()
        {
            _hours = 7; // Varsayılan değer
        }

        public DayHourSetting(string dayName, int dayIndex, int hours)
        {
            DayName = dayName;
            DayIndex = dayIndex;
            _hours = hours;
        }
    }
}