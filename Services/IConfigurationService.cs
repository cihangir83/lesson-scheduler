using System.Threading.Tasks;
using LessonScheduler.Models;

namespace LessonScheduler.Services
{
    public interface IConfigurationService
    {
        /// <summary>
        /// Yapılandırmayı dosyadan yükler
        /// </summary>
        Task<SchoolConfiguration> LoadConfigurationAsync(string filePath);

        /// <summary>
        /// Yapılandırmayı dosyaya kaydeder
        /// </summary>
        Task SaveConfigurationAsync(SchoolConfiguration config, string filePath);

        /// <summary>
        /// Varsayılan yapılandırma döndürür
        /// </summary>
        SchoolConfiguration GetDefaultConfiguration();

        /// <summary>
        /// Yapılandırmanın geçerliliğini kontrol eder
        /// </summary>
        void ValidateConfiguration(SchoolConfiguration config);

        /// <summary>
        /// Mevcut JSON dosyasından yapılandırma çıkarır, yoksa varsayılan oluşturur
        /// </summary>
        SchoolConfiguration ExtractOrCreateConfiguration(SchoolData schoolData);
    }
}