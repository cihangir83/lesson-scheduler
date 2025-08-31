using System.Threading.Tasks;
using LessonScheduler.Models;

namespace LessonScheduler.Services
{
    public interface IDataService
    {
        /// <summary>
        /// JSON dosyasından okul verilerini yükler
        /// </summary>
        Task<SchoolData> LoadFromJsonAsync(string filePath);

        /// <summary>
        /// Okul verilerini JSON dosyasına kaydeder
        /// </summary>
        Task SaveToJsonAsync(SchoolData data, string filePath);

        /// <summary>
        /// Test verilerini yükler
        /// </summary>
        SchoolData LoadTestData();
    }
}