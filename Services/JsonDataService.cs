using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using LessonScheduler.Models;

namespace LessonScheduler.Services
{
    public class JsonDataService : IDataService
    {
        private readonly JsonSerializerSettings _jsonSettings;

        public JsonDataService()
        {
            _jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            };
        }

        public async Task<SchoolData> LoadFromJsonAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    throw new FileNotFoundException($"JSON dosyası bulunamadı: {filePath}");

                var jsonContent = await File.ReadAllTextAsync(filePath);
                
                if (string.IsNullOrWhiteSpace(jsonContent))
                    throw new DataValidationException("JSON dosyası boş.");

                var schoolData = JsonConvert.DeserializeObject<SchoolData>(jsonContent, _jsonSettings);
                
                if (schoolData == null)
                    throw new DataValidationException("JSON dosyası geçersiz format içeriyor.");

                // Backward compatibility - eski dosyalar için configuration oluştur
                if (schoolData.Configuration == null)
                {
                    schoolData.MigrateFromOldFormat();
                }

                // Veri doğrulama
                schoolData.ValidateData();

                return schoolData;
            }
            catch (JsonException ex)
            {
                throw new DataValidationException($"JSON format hatası: {ex.Message}", ex);
            }
            catch (Exception ex) when (!(ex is DataValidationException || ex is FileNotFoundException))
            {
                throw new DataValidationException($"JSON yükleme hatası: {ex.Message}", ex);
            }
        }

        public async Task SaveToJsonAsync(SchoolData data, string filePath)
        {
            try
            {
                if (data == null)
                    throw new ArgumentNullException(nameof(data));

                // Veri doğrulama
                data.ValidateData();

                var jsonContent = JsonConvert.SerializeObject(data, _jsonSettings);
                
                // Dizin yoksa oluştur
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                await File.WriteAllTextAsync(filePath, jsonContent);
            }
            catch (Exception ex) when (!(ex is DataValidationException))
            {
                throw new DataValidationException($"JSON kaydetme hatası: {ex.Message}", ex);
            }
        }

        public SchoolData LoadTestData()
        {
            try
            {
                // test5.json dosyasından oku - önce uygulama dizininde ara
                string appDir = AppDomain.CurrentDomain.BaseDirectory;
                string testFilePath = Path.Combine(appDir, "test5.json");
                
                // Eğer uygulama dizininde yoksa, mevcut dizinde ara (geliştirme için)
                if (!File.Exists(testFilePath))
                {
                    testFilePath = "test5.json";
                }
                if (File.Exists(testFilePath))
                {
                    var jsonContent = File.ReadAllText(testFilePath, System.Text.Encoding.UTF8);
                    var schoolData = JsonConvert.DeserializeObject<SchoolData>(jsonContent, _jsonSettings);
                    
                    if (schoolData != null)
                    {
                        // Eski format için configuration oluştur
                        if (schoolData.Configuration == null)
                        {
                            schoolData.MigrateFromOldFormat();
                        }

                        // Eksik sınıflar için atamalar oluştur
                        foreach (var className in schoolData.Definitions.Classes)
                        {
                            if (!schoolData.Assignments.ContainsKey(className))
                            {
                                schoolData.Assignments[className] = new List<LessonAssignment>();
                            }
                        }

                        // Eksik öğretmenler için kısıtlamalar oluştur
                        foreach (var teacherName in schoolData.Definitions.Teachers)
                        {
                            schoolData.AddTeacher(teacherName);
                        }

                        return schoolData;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new DataValidationException($"Test verisi yükleme hatası: {ex.Message}", ex);
            }

            throw new DataValidationException("Test verisi dosyası bulunamadı veya okunamadı.");
        }
    }
}