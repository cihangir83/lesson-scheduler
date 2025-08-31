using System;
using System.Threading;
using System.Threading.Tasks;
using LessonScheduler.Models;

namespace LessonScheduler.Services
{
    public class SolverProgress
    {
        public int TotalBlocks { get; set; }
        public int PlacedBlocks { get; set; }
        public double ProgressPercentage => TotalBlocks > 0 ? (double)PlacedBlocks / TotalBlocks * 100 : 0;
        public string CurrentStatus { get; set; } = "";
        public double ElapsedSeconds { get; set; }
        public double EstimatedRemainingSeconds { get; set; }
        public string CurrentTip { get; set; } = "";
    }

    public interface IScheduleSolver
    {
        /// <summary>
        /// Okul verilerine göre ders programı çözümü oluşturur
        /// </summary>
        /// <param name="schoolData">Okul verileri</param>
        /// <param name="progress">İlerleme raporu</param>
        /// <param name="cancellationToken">İptal token'ı</param>
        /// <returns>Çözüm verisi ve mesaj</returns>
        Task<(SolutionData? solution, string message)> SolveScheduleAsync(
            SchoolData schoolData,
            IProgress<SolverProgress>? progress = null,
            CancellationToken cancellationToken = default);
    }
}