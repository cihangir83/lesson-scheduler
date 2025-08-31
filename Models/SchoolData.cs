using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace LessonScheduler.Models
{
    public class SchoolData
    {
        [JsonProperty("definitions")]
        public Definitions Definitions { get; set; } = new();

        [JsonProperty("assignments")]
        public Dictionary<string, List<LessonAssignment>> Assignments { get; set; } = new();

        [JsonProperty("constraints")]
        public Dictionary<string, Dictionary<string, bool[]>> Constraints { get; set; } = new();

        [JsonProperty("solution")]
        public SolutionData? Solution { get; set; }

        [JsonProperty("configuration")]
        public SchoolConfiguration Configuration { get; set; } = new();

        public SchoolData()
        {
            Definitions = new Definitions();
            Assignments = new Dictionary<string, List<LessonAssignment>>();
            Constraints = new Dictionary<string, Dictionary<string, bool[]>>();
            Configuration = new SchoolConfiguration();
        }

        /// <summary>
        /// Belirtilen sınıf için ders atamalarını döndürür
        /// </summary>
        public List<LessonAssignment> GetAssignmentsForClass(string className)
        {
            return Assignments.TryGetValue(className, out var assignments) 
                ? assignments 
                : new List<LessonAssignment>();
        }

        /// <summary>
        /// Belirtilen öğretmen için kısıtlamaları döndürür
        /// </summary>
        public Dictionary<string, bool[]>? GetConstraintsForTeacher(string teacherName)
        {
            return Constraints.TryGetValue(teacherName, out var constraints) 
                ? constraints 
                : null;
        }

        /// <summary>
        /// Yeni bir sınıf ekler ve boş atama listesi oluşturur
        /// </summary>
        public void AddClass(string className)
        {
            if (!Assignments.ContainsKey(className))
            {
                Assignments[className] = new List<LessonAssignment>();
            }
        }

        /// <summary>
        /// Yeni bir öğretmen ekler ve varsayılan kısıtlamaları oluşturur (tüm saatler müsait)
        /// </summary>
        public void AddTeacher(string teacherName)
        {
            if (!Constraints.ContainsKey(teacherName))
            {
                var defaultConstraints = new Dictionary<string, bool[]>();
                for (int day = 0; day < Configuration.TotalDays; day++)
                {
                    var hoursForDay = Configuration.GetHoursForDay(day);
                    defaultConstraints[day.ToString()] = Enumerable.Repeat(true, hoursForDay).ToArray();
                }
                Constraints[teacherName] = defaultConstraints;
            }
        }

        /// <summary>
        /// Belirtilen gün için saat sayısını döndürür
        /// </summary>
        public int GetHoursForDay(int day)
        {
            return Configuration.GetHoursForDay(day);
        }

        /// <summary>
        /// En fazla saat sayısını döndürür
        /// </summary>
        public int GetMaxHoursPerDay()
        {
            return Configuration.GetMaxHoursPerDay();
        }

        /// <summary>
        /// Yapılandırma değiştiğinde kısıtlamaları günceller
        /// </summary>
        public void UpdateConstraintsForNewSchedule()
        {
            foreach (var teacherName in Definitions.Teachers)
            {
                if (Constraints.ContainsKey(teacherName))
                {
                    var existingConstraints = Constraints[teacherName];
                    var updatedConstraints = new Dictionary<string, bool[]>();

                    for (int day = 0; day < Configuration.TotalDays; day++)
                    {
                        var newHoursForDay = Configuration.GetHoursForDay(day);
                        var dayKey = day.ToString();

                        if (existingConstraints.ContainsKey(dayKey))
                        {
                            var existingDayConstraints = existingConstraints[dayKey];
                            var newDayConstraints = new bool[newHoursForDay];

                            // Mevcut kısıtlamaları koru, eksik olanları true yap
                            for (int hour = 0; hour < newHoursForDay; hour++)
                            {
                                if (hour < existingDayConstraints.Length)
                                {
                                    newDayConstraints[hour] = existingDayConstraints[hour];
                                }
                                else
                                {
                                    newDayConstraints[hour] = true; // Yeni saatler varsayılan olarak müsait
                                }
                            }
                            updatedConstraints[dayKey] = newDayConstraints;
                        }
                        else
                        {
                            // Gün yoksa varsayılan olarak tüm saatler müsait
                            updatedConstraints[dayKey] = Enumerable.Repeat(true, newHoursForDay).ToArray();
                        }
                    }

                    Constraints[teacherName] = updatedConstraints;
                }
                else
                {
                    // Öğretmen kısıtlaması yoksa yeni oluştur
                    AddTeacher(teacherName);
                }
            }
        }

        /// <summary>
        /// Eski format JSON dosyalarını yeni formata dönüştürür
        /// </summary>
        public void MigrateFromOldFormat()
        {
            // Eğer configuration yoksa varsayılan oluştur
            if (Configuration == null)
            {
                Configuration = new SchoolConfiguration();
            }

            // Mevcut kısıtlamaları yeni yapıya uyarla
            UpdateConstraintsForNewSchedule();
        }

        /// <summary>
        /// Yapılandırma güncellendiğinde çağrılır
        /// </summary>
        public void OnConfigurationChanged()
        {
            // Kısıtlamaları güncelle
            UpdateConstraintsForNewSchedule();
            
            // Çözümü temizle (yeni yapılandırmaya göre yeniden çözülmesi gerekir)
            Solution = null;
        }

        /// <summary>
        /// Boş ama geçerli bir veri yapısı oluşturur
        /// </summary>
        public static SchoolData CreateEmpty()
        {
            var schoolData = new SchoolData();
            
            // Boş listeler zaten constructor'da oluşturuluyor
            // Sadece yapılandırmanın doğru olduğundan emin olalım
            schoolData.Configuration = SchoolConfiguration.CreateDefault();
            
            return schoolData;
        }

        /// <summary>
        /// Tüm veri tutarlılığını kontrol eder
        /// </summary>
        public void ValidateData()
        {
            ValidateConfiguration();
            ValidateDefinitions();
            ValidateAssignments();
            ValidateConstraints();
        }

        /// <summary>
        /// Yapılandırmanın geçerliliğini kontrol eder
        /// </summary>
        private void ValidateConfiguration()
        {
            if (Configuration == null)
            {
                Configuration = new SchoolConfiguration();
            }
            else
            {
                Configuration.Validate();
            }
        }

        /// <summary>
        /// Tanımlamaların geçerliliğini kontrol eder
        /// </summary>
        private void ValidateDefinitions()
        {
            if (Definitions.Lessons.Count == 0)
                throw new DataValidationException("En az bir ders tanımlanmalıdır.");

            if (Definitions.Teachers.Count == 0)
                throw new DataValidationException("En az bir öğretmen tanımlanmalıdır.");

            if (Definitions.Classes.Count == 0)
                throw new DataValidationException("En az bir sınıf tanımlanmalıdır.");

            // Duplicate kontrolü
            if (Definitions.Lessons.Count != Definitions.Lessons.Distinct().Count())
                throw new DataValidationException("Ders listesinde tekrarlanan öğeler var.");

            if (Definitions.Teachers.Count != Definitions.Teachers.Distinct().Count())
                throw new DataValidationException("Öğretmen listesinde tekrarlanan öğeler var.");

            if (Definitions.Classes.Count != Definitions.Classes.Distinct().Count())
                throw new DataValidationException("Sınıf listesinde tekrarlanan öğeler var.");
        }

        /// <summary>
        /// Ders atamalarının geçerliliğini kontrol eder
        /// </summary>
        private void ValidateAssignments()
        {
            foreach (var classAssignments in Assignments)
            {
                var className = classAssignments.Key;
                var assignments = classAssignments.Value;

                // Sınıfın tanımlamalar listesinde olup olmadığını kontrol et
                if (!Definitions.Classes.Contains(className))
                    throw new DataValidationException($"'{className}' sınıfı tanımlamalar listesinde bulunamadı.");

                foreach (var assignment in assignments)
                {
                    // Ders kontrolü
                    if (!Definitions.Lessons.Contains(assignment.Ders))
                        throw new DataValidationException($"'{assignment.Ders}' dersi tanımlamalar listesinde bulunamadı.");

                    // Öğretmen kontrolü
                    if (!Definitions.Teachers.Contains(assignment.Ogretmen))
                        throw new DataValidationException($"'{assignment.Ogretmen}' öğretmeni tanımlamalar listesinde bulunamadı.");

                    // Blok yapısı kontrolü
                    if (!assignment.IsBlockStructureValid())
                        throw new DataValidationException($"'{className}' sınıfındaki '{assignment.Ders}' dersi için blok yapısı geçersiz. Blok toplamı ({assignment.GetBlockStructure().Sum()}) toplam saatle ({assignment.ToplamSaat}) eşleşmiyor.");

                    // Pozitif saat kontrolü
                    if (assignment.ToplamSaat <= 0)
                        throw new DataValidationException($"'{className}' sınıfındaki '{assignment.Ders}' dersi için toplam saat pozitif olmalıdır.");
                }
            }
        }

        /// <summary>
        /// Öğretmen kısıtlamalarının geçerliliğini kontrol eder
        /// </summary>
        private void ValidateConstraints()
        {
            foreach (var teacherConstraints in Constraints)
            {
                var teacherName = teacherConstraints.Key;
                var constraints = teacherConstraints.Value;

                // Öğretmenin tanımlamalar listesinde olup olmadığını kontrol et
                if (!Definitions.Teachers.Contains(teacherName))
                    throw new DataValidationException($"'{teacherName}' öğretmeni tanımlamalar listesinde bulunamadı.");

                // Kısıtlama formatını kontrol et
                for (int day = 0; day < Configuration.TotalDays; day++)
                {
                    var dayKey = day.ToString();
                    if (constraints.ContainsKey(dayKey))
                    {
                        var expectedHours = Configuration.GetHoursForDay(day);
                        if (constraints[dayKey].Length != expectedHours)
                            throw new DataValidationException($"'{teacherName}' öğretmeni için {day + 1}. günün kısıtlama verisi {expectedHours} saat olmalıdır.");
                    }
                }
            }
        }
    }
}