using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using LessonScheduler.Models;

namespace LessonScheduler.Views.Dialogs
{
    public partial class TeacherStatusDialog : Window
    {
        public TeacherStatusDialog(SchoolData schoolData)
        {
            InitializeComponent();
            LoadTeacherStatus(schoolData);
        }

        private void LoadTeacherStatus(SchoolData schoolData)
        {
            var teacherStatusList = new List<TeacherStatusViewModel>();

            foreach (var teacher in schoolData.Definitions.Teachers.OrderBy(t => t))
            {
                var statusViewModel = new TeacherStatusViewModel
                {
                    TeacherName = teacher
                };

                // Öğretmenin toplam saatlerini hesapla
                var totalHours = CalculateTeacherTotalHours(schoolData, teacher);
                statusViewModel.TotalHours = totalHours;

                // Öğretmenin kısıtlamalarını al
                var constraints = schoolData.GetConstraintsForTeacher(teacher);
                var (availableHours, blockedHours) = CalculateAvailabilityStats(schoolData, constraints);
                
                statusViewModel.AvailableHours = availableHours;
                statusViewModel.BlockedHours = blockedHours;

                // Mini schedule oluştur
                statusViewModel.ScheduleRows = CreateMiniSchedule(schoolData, constraints);

                teacherStatusList.Add(statusViewModel);
            }

            TeacherStatusList.ItemsSource = teacherStatusList;
        }

        private int CalculateTeacherTotalHours(SchoolData schoolData, string teacher)
        {
            int totalHours = 0;

            foreach (var classAssignments in schoolData.Assignments)
            {
                foreach (var assignment in classAssignments.Value)
                {
                    if (assignment.Ogretmen == teacher)
                    {
                        totalHours += assignment.ToplamSaat;
                    }
                }
            }

            return totalHours;
        }

        private (int available, int blocked) CalculateAvailabilityStats(SchoolData schoolData, Dictionary<string, bool[]>? constraints)
        {
            int availableCount = 0;
            int blockedCount = 0;

            for (int day = 0; day < 5; day++) // Pazartesi-Cuma
            {
                var hoursForDay = schoolData.GetHoursForDay(day);
                
                if (constraints != null && constraints.TryGetValue(day.ToString(), out var dayConstraints))
                {
                    for (int hour = 0; hour < hoursForDay; hour++)
                    {
                        if (hour < dayConstraints.Length)
                        {
                            if (dayConstraints[hour])
                                availableCount++;
                            else
                                blockedCount++;
                        }
                        else
                        {
                            availableCount++; // Varsayılan: müsait
                        }
                    }
                }
                else
                {
                    availableCount += hoursForDay; // Kısıtlama yoksa tümü müsait
                }
            }

            return (availableCount, blockedCount);
        }

        private List<MiniScheduleRowViewModel> CreateMiniSchedule(SchoolData schoolData, Dictionary<string, bool[]>? constraints)
        {
            var rows = new List<MiniScheduleRowViewModel>();
            var maxHours = schoolData.GetMaxHoursPerDay();

            for (int hour = 0; hour < maxHours; hour++)
            {
                var row = new MiniScheduleRowViewModel
                {
                    Hour = (hour + 1).ToString()
                };

                for (int day = 0; day < 5; day++)
                {
                    string color = "Gray"; // Varsayılan: geçersiz saat
                    var hoursForDay = schoolData.GetHoursForDay(day);
                    
                    if (hour < hoursForDay) // Bu gün için geçerli saat
                    {
                        bool isAvailable = true; // Varsayılan: müsait
                        
                        if (constraints != null && constraints.TryGetValue(day.ToString(), out var dayConstraints))
                        {
                            if (hour < dayConstraints.Length)
                            {
                                isAvailable = dayConstraints[hour];
                            }
                        }
                        
                        color = isAvailable ? "LightGreen" : "LightCoral";
                    }

                    switch (day)
                    {
                        case 0: row.MondayColor = color; break;
                        case 1: row.TuesdayColor = color; break;
                        case 2: row.WednesdayColor = color; break;
                        case 3: row.ThursdayColor = color; break;
                        case 4: row.FridayColor = color; break;
                    }
                }

                rows.Add(row);
            }

            return rows;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public class TeacherStatusViewModel
    {
        public string TeacherName { get; set; } = "";
        public int TotalHours { get; set; }
        public int AvailableHours { get; set; }
        public int BlockedHours { get; set; }
        public List<MiniScheduleRowViewModel> ScheduleRows { get; set; } = new();
    }

    public class MiniScheduleRowViewModel
    {
        public string Hour { get; set; } = "";
        public string MondayColor { get; set; } = "Gray";
        public string TuesdayColor { get; set; } = "Gray";
        public string WednesdayColor { get; set; } = "Gray";
        public string ThursdayColor { get; set; } = "Gray";
        public string FridayColor { get; set; } = "Gray";
    }
}