using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace LessonScheduler.Models
{
    public class SchoolConfiguration
    {
        [JsonProperty("school_name")]
        public string SchoolName { get; set; } = "Okul Adı";

        [JsonProperty("principal_name")]
        public string PrincipalName { get; set; } = "Müdür Adı";

        [JsonProperty("daily_hours")]
        public Dictionary<int, int> DailyHours { get; set; } = new()
        {
            {0, 7}, // Pazartesi
            {1, 7}, // Salı
            {2, 7}, // Çarşamba
            {3, 7}, // Perşembe
            {4, 7}  // Cuma
        };

        [JsonProperty("total_days")]
        public int TotalDays { get; set; } = 5;

        public SchoolConfiguration()
        {
            // Varsayılan değerler constructor'da zaten atanmış
        }

        /// <summary>
        /// Belirtilen gün için saat sayısını döndürür
        /// </summary>
        public int GetHoursForDay(int day)
        {
            return DailyHours.TryGetValue(day, out var hours) ? hours : 7;
        }

        /// <summary>
        /// En fazla saat sayısını döndürür (tablo boyutları için)
        /// </summary>
        public int GetMaxHoursPerDay()
        {
            return DailyHours.Values.Max();
        }

        /// <summary>
        /// Toplam haftalık saat sayısını döndürür
        /// </summary>
        public int GetTotalWeeklyHours()
        {
            return DailyHours.Values.Sum();
        }

        /// <summary>
        /// Yapılandırmanın geçerliliğini kontrol eder
        /// </summary>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(SchoolName))
                throw new DataValidationException("Okul adı boş olamaz.");

            if (SchoolName.Length > 100)
                throw new DataValidationException("Okul adı 100 karakterden uzun olamaz.");

            if (string.IsNullOrWhiteSpace(PrincipalName))
                throw new DataValidationException("Müdür adı boş olamaz.");

            if (PrincipalName.Length > 100)
                throw new DataValidationException("Müdür adı 100 karakterden uzun olamaz.");

            if (TotalDays != 5)
                throw new DataValidationException("Toplam gün sayısı 5 olmalıdır.");

            // Her gün için saat kontrolü
            for (int day = 0; day < TotalDays; day++)
            {
                var hours = GetHoursForDay(day);
                if (hours < 1 || hours > 10)
                    throw new DataValidationException($"{day + 1}. gün için saat sayısı 1-10 arasında olmalıdır.");
            }
        }

        /// <summary>
        /// Varsayılan yapılandırma oluşturur
        /// </summary>
        public static SchoolConfiguration CreateDefault()
        {
            return new SchoolConfiguration();
        }

        /// <summary>
        /// Günlük saat ayarlarını günceller
        /// </summary>
        public void UpdateDailyHours(Dictionary<int, int> newHours)
        {
            foreach (var kvp in newHours)
            {
                if (kvp.Key >= 0 && kvp.Key < TotalDays && kvp.Value >= 1 && kvp.Value <= 10)
                {
                    DailyHours[kvp.Key] = kvp.Value;
                }
            }
        }
    }
}