using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using LessonScheduler.Models;

namespace LessonScheduler.Services
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly JsonSerializerSettings _jsonSettings;

        public ConfigurationService()
        {
            _jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore
            };
        }

        public async Task<SchoolConfiguration> LoadConfigurationAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return GetDefaultConfiguration();
                }

                var jsonContent = await File.ReadAllTextAsync(filePath);
                
                if (string.IsNullOrWhiteSpace(jsonContent))
                {
                    return GetDefaultConfiguration();
                }

                var config = JsonConvert.DeserializeObject<SchoolConfiguration>(jsonContent, _jsonSettings);
                
                if (config == null)
                {
                    return GetDefaultConfiguration();
                }

                ValidateConfiguration(config);
                return config;
            }
            catch (JsonException)
            {
                // JSON parse hatası durumunda varsayılan yapılandırma döndür
                return GetDefaultConfiguration();
            }
            catch (DataValidationException)
            {
                // Validation hatası durumunda varsayılan yapılandırma döndür
                return GetDefaultConfiguration();
            }
            catch (Exception ex)
            {
                throw new DataValidationException($"Yapılandırma yükleme hatası: {ex.Message}", ex);
            }
        }

        public async Task SaveConfigurationAsync(SchoolConfiguration config, string filePath)
        {
            try
            {
                if (config == null)
                    throw new ArgumentNullException(nameof(config));

                ValidateConfiguration(config);

                var jsonContent = JsonConvert.SerializeObject(config, _jsonSettings);
                
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
                throw new DataValidationException($"Yapılandırma kaydetme hatası: {ex.Message}", ex);
            }
        }

        public SchoolConfiguration GetDefaultConfiguration()
        {
            return SchoolConfiguration.CreateDefault();
        }

        public void ValidateConfiguration(SchoolConfiguration config)
        {
            if (config == null)
                throw new DataValidationException("Yapılandırma null olamaz.");

            config.Validate();
        }

        public SchoolConfiguration ExtractOrCreateConfiguration(SchoolData schoolData)
        {
            if (schoolData?.Configuration != null)
            {
                try
                {
                    ValidateConfiguration(schoolData.Configuration);
                    return schoolData.Configuration;
                }
                catch (DataValidationException)
                {
                    // Geçersiz yapılandırma varsa varsayılan oluştur
                    var defaultConfig = GetDefaultConfiguration();
                    schoolData.Configuration = defaultConfig;
                    return defaultConfig;
                }
            }

            // Yapılandırma yoksa varsayılan oluştur
            var newConfig = GetDefaultConfiguration();
            if (schoolData != null)
            {
                schoolData.Configuration = newConfig;
            }
            return newConfig;
        }

        /// <summary>
        /// SchoolData'dan yapılandırmayı çıkarır ve gerekirse günceller
        /// </summary>
        public void EnsureConfigurationExists(SchoolData schoolData)
        {
            if (schoolData == null)
                return;

            if (schoolData.Configuration == null)
            {
                schoolData.Configuration = GetDefaultConfiguration();
            }
            else
            {
                try
                {
                    ValidateConfiguration(schoolData.Configuration);
                }
                catch (DataValidationException)
                {
                    // Geçersiz yapılandırma varsa varsayılan ile değiştir
                    schoolData.Configuration = GetDefaultConfiguration();
                }
            }

            // Kısıtlamaları yeni yapılandırmaya göre güncelle
            schoolData.UpdateConstraintsForNewSchedule();
        }

        /// <summary>
        /// Eski format JSON dosyalarını yeni formata dönüştürür
        /// </summary>
        public void MigrateOldConfiguration(SchoolData schoolData)
        {
            if (schoolData?.Configuration == null)
            {
                schoolData.Configuration = GetDefaultConfiguration();
                schoolData.UpdateConstraintsForNewSchedule();
            }
        }
    }
}