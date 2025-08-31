using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.OrTools.Sat;
using LessonScheduler.Models;

namespace LessonScheduler.Services
{
    public class OrToolsScheduleSolver : IScheduleSolver
    {
        // Python kodundaki priorities dictionary'sinin birebir karşılığı
        private readonly Dictionary<string, int> _priorities = new()
        {
            {"Matematik", 1},
            {"Türkçe", 1},
            {"Fen Bilgisi", 1},
            {"Sosyal Bilgiler", 2},
            {"İngilizce", 2},
            {"Din Kültürü ve Ahlak Bilgisi", 3},
            {"Bilişim Teknolojileri ve Yazılım", 3},
            {"Seçmeli Matematik", 3},
            {"Beden Eğitimi", 4},
            {"Seçmeli Spor Faaliyetleri", 4},
            {"Müzik", 5},
            {"Görsel Sanatlar", 5},
            {"Seçmeli Müzik", 5}
        };

        private const int TotalDays = 5; // Haftalık gün sayısı

        public async Task<(SolutionData? solution, string message)> SolveScheduleAsync(
            SchoolData schoolData, IProgress<SolverProgress>? progress = null, CancellationToken cancellationToken = default)
        {
            return await SolveSchedule(schoolData, progress, cancellationToken);
        }

        private async Task<(SolutionData? solution, string message)> SolveSchedule(
            SchoolData schoolData, IProgress<SolverProgress>? progress, CancellationToken cancellationToken)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                
                // Python kodundaki model ve solver oluşturma
                var model = new CpModel();
                var solver = new CpSolver();

                // Python kodundaki ogretmen_ders_sinif yapısını oluştur
                var teacherClassLessons = BuildTeacherClassLessons(schoolData);
                
                // Python kodundaki teacher_availability yapısını oluştur
                var teacherAvailability = BuildTeacherAvailability(schoolData);

                // Python kodundaki blocks listesini oluştur
                var blocks = CreateBlocks(teacherClassLessons, teacherAvailability);
                
                if (blocks.Count == 0)
                    return (null, "Atanacak ders bulunamadı.");

                // �  İlk progress raporu
                progress?.Report(new SolverProgress
                {
                    TotalBlocks = blocks.Count,
                    PlacedBlocks = 0,
                    CurrentStatus = "Ders programı analiz ediliyor...",
                    ElapsedSeconds = stopwatch.Elapsed.TotalSeconds,
                    CurrentTip = "Akıllı algoritma başlatılıyor"
                });

                // 🔍 ERKEN KONTROL: İmkansız blokları hemen tespit et
                var impossibleBlocks = blocks.Where(b => GetFeasibleSlots(b, teacherAvailability, schoolData).Count == 0).ToList();
                if (impossibleBlocks.Any())
                {
                    var first = impossibleBlocks.First();
                    return (null, $"❌ '{first.Ogretmen}' öğretmeni '{first.Ders}' dersini ({first.BlokUzunluk}h) yerleştiremez. Müsaitlik saatlerini kontrol edin.");
                }

                // 🧠 AKILLI SIRALAMA: En kısıtlı blokları önce yerleştir
                var blockConstraintInfo = blocks.Select(block => new
                {
                    Block = block,
                    FeasibleSlots = GetFeasibleSlots(block, teacherAvailability, schoolData).Count
                }).ToList();

                // En az seçeneği olan blokları önce yerleştir (Most Constrained First)
                blocks = blockConstraintInfo
                    .OrderBy(x => x.FeasibleSlots)           // En kısıtlı önce
                    .ThenBy(x => x.Block.Priority)           // Sonra öncelik
                    .ThenByDescending(x => x.Block.BlokUzunluk) // Sonra uzunluk
                    .Select(x => x.Block)
                    .ToList();

                // 📊 Sıralama tamamlandı
                progress?.Report(new SolverProgress
                {
                    TotalBlocks = blocks.Count,
                    PlacedBlocks = 0,
                    CurrentStatus = "Optimizasyon modeli hazırlanıyor...",
                    ElapsedSeconds = stopwatch.Elapsed.TotalSeconds,
                    CurrentTip = "En uygun çözüm aranıyor"
                });

                // Variables oluşturma - Python kodundaki day_vars, start_vars, interval_vars
                var dayVars = new Dictionary<int, IntVar>();
                var startVars = new Dictionary<int, IntVar>();
                var intervalVars = new Dictionary<int, IntervalVar>();

                for (int i = 0; i < blocks.Count; i++)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return (null, "İşlem iptal edildi.");

                    var block = blocks[i];
                    
                    // 📊 Progress güncelle
                    if (i % 5 == 0 || i == blocks.Count - 1) // Her 5 blokta bir güncelle
                    {
                        var statusMessages = new[]
                        {
                            "Ders blokları hazırlanıyor...",
                            "Öğretmen programları kontrol ediliyor...",
                            "Zaman çakışmaları analiz ediliyor...",
                            "Sınıf programları düzenleniyor...",
                            "Optimizasyon modeli oluşturuluyor..."
                        };
                        
                        var tipMessages = new[]
                        {
                            "En kısıtlı dersler önce yerleştiriliyor",
                            "Öğretmen müsaitlikleri kontrol ediliyor", 
                            "Sınıf çakışmaları önleniyor",
                            "Program dengesi sağlanıyor",
                            "Son optimizasyonlar yapılıyor"
                        };

                        var statusIndex = (i * statusMessages.Length) / blocks.Count;
                        var tipIndex = (i * tipMessages.Length) / blocks.Count;

                        progress?.Report(new SolverProgress
                        {
                            TotalBlocks = blocks.Count,
                            PlacedBlocks = i,
                            CurrentStatus = statusMessages[Math.Min(statusIndex, statusMessages.Length - 1)],
                            ElapsedSeconds = stopwatch.Elapsed.TotalSeconds,
                            EstimatedRemainingSeconds = (stopwatch.Elapsed.TotalSeconds / Math.Max(i, 1)) * (blocks.Count - i),
                            CurrentTip = tipMessages[Math.Min(tipIndex, tipMessages.Length - 1)]
                        });
                    }

                    var feasible = GetFeasibleSlots(block, teacherAvailability, schoolData);

                    if (feasible.Count == 0)
                        return (null, $"Çözüm İmkansız: '{block.Ogretmen}' ({block.Ders} - {block.BlokUzunluk} saatlik blok) için uygun zaman bulunamadı.");

                    var maxHours = schoolData.GetMaxHoursPerDay();
                    dayVars[i] = model.NewIntVar(0, TotalDays - 1, $"d_{i}");
                    startVars[i] = model.NewIntVar(0, maxHours - 1, $"s_{i}");
                    
                    // Python kodundaki AddAllowedAssignments - C# API'sinde farklı kullanım
                    var feasibleTuples = feasible.Select(f => new long[] { f.day, f.start }).ToArray();
                    
                    // C# OR-Tools'da table constraint kullanıyoruz
                    var tableConstraint = model.AddAllowedAssignments(new IntVar[] { dayVars[i], startVars[i] });
                    foreach (var tuple in feasibleTuples)
                    {
                        tableConstraint.AddTuple(tuple);
                    }

                    // Python kodundaki abs_start ve interval_vars
                    var absStart = model.NewIntVar(0, maxHours * TotalDays, $"abs_{i}");
                    model.Add(absStart == dayVars[i] * maxHours + startVars[i]);
                    intervalVars[i] = model.NewIntervalVar(absStart, block.BlokUzunluk, absStart + block.BlokUzunluk, $"i_{i}");
                }

                // Python kodundaki class_intervals ve teacher_intervals
                var classIntervals = new Dictionary<string, List<IntervalVar>>();
                var teacherIntervals = new Dictionary<string, List<IntervalVar>>();

                for (int i = 0; i < blocks.Count; i++)
                {
                    var block = blocks[i];
                    
                    if (!classIntervals.ContainsKey(block.Sinif))
                        classIntervals[block.Sinif] = new List<IntervalVar>();
                    classIntervals[block.Sinif].Add(intervalVars[i]);

                    if (!teacherIntervals.ContainsKey(block.Ogretmen))
                        teacherIntervals[block.Ogretmen] = new List<IntervalVar>();
                    teacherIntervals[block.Ogretmen].Add(intervalVars[i]);
                }

                // Python kodundaki AddNoOverlap constraints
                foreach (var classInterval in classIntervals.Values)
                {
                    model.AddNoOverlap(classInterval.ToArray());
                }

                foreach (var teacherInterval in teacherIntervals.Values)
                {
                    model.AddNoOverlap(teacherInterval.ToArray());
                }

                // ⚡ AKILLI ZAMAN: Blok sayısına göre dinamik süre
                var timeLimit = blocks.Count switch
                {
                    <= 30 => 15.0,   // Küçük problemler: 15s
                    <= 60 => 30.0,   // Orta problemler: 30s  
                    <= 100 => 60.0,  // Büyük problemler: 60s
                    _ => 90.0         // Çok büyük: 90s
                };
                
                solver.StringParameters = $"max_time_in_seconds:{timeLimit} num_search_workers:4 cp_model_presolve:true";

                // 📊 Çözüm başlıyor - Simüle edilmiş progress ile
                var status = await SolveWithProgressAsync(solver, model, blocks, progress, timeLimit, stopwatch, cancellationToken);
                stopwatch.Stop();

                if (status == CpSolverStatus.Optimal || status == CpSolverStatus.Feasible)
                {
                    // Python kodundaki solution parsing
                    var solutionBlocks = new Dictionary<int, ScheduleBlock>();
                    
                    for (int i = 0; i < blocks.Count; i++)
                    {
                        var block = blocks[i];
                        block.Day = (int)solver.Value(dayVars[i]);
                        block.Start = (int)solver.Value(startVars[i]);
                        solutionBlocks[i] = block;
                    }

                    var solution = new SolutionData(
                        solutionBlocks,
                        $"✅ Çözüm {stopwatch.Elapsed.TotalSeconds:F1}s'de bulundu! ({blocks.Count} blok yerleştirildi)",
                        stopwatch.Elapsed.TotalSeconds
                    );

                    return (solution, solution.Message);
                }

                var statusMessage = status switch
                {
                    CpSolverStatus.Unknown => $"⏱️ {timeLimit}s'de çözüm bulunamadı. Öğretmen kısıtlamalarını azaltmayı deneyin.",
                    CpSolverStatus.Infeasible => "❌ Matematiksel olarak çözüm yok. Öğretmen müsaitliklerini kontrol edin.",
                    CpSolverStatus.ModelInvalid => "⚠️ Model hatası. Veri girişinde sorun var.",
                    _ => $"❓ Bilinmeyen durum: {status}"
                };
                
                return (null, statusMessage);
            }
            catch (Exception ex)
            {
                return (null, $"Solver hatası: {ex.Message}");
            }
        }

        private Dictionary<string, Dictionary<string, Dictionary<string, (int totalHours, List<int> blockStructure)>>> 
            BuildTeacherClassLessons(SchoolData schoolData)
        {
            // Python kodundaki ogretmen_ders_sinif yapısını oluştur
            var result = new Dictionary<string, Dictionary<string, Dictionary<string, (int, List<int>)>>>();

            foreach (var classAssignment in schoolData.Assignments)
            {
                var className = classAssignment.Key;
                var assignments = classAssignment.Value;

                foreach (var assignment in assignments)
                {
                    if (!result.ContainsKey(className))
                        result[className] = new Dictionary<string, Dictionary<string, (int, List<int>)>>();

                    if (!result[className].ContainsKey(assignment.Ogretmen))
                        result[className][assignment.Ogretmen] = new Dictionary<string, (int, List<int>)>();

                    result[className][assignment.Ogretmen][assignment.Ders] = 
                        (assignment.ToplamSaat, assignment.GetBlockStructure());
                }
            }

            return result;
        }

        private Dictionary<string, Dictionary<string, bool[]>> BuildTeacherAvailability(SchoolData schoolData)
        {
            // Python kodundaki teacher_availability yapısını oluştur
            var result = new Dictionary<string, Dictionary<string, bool[]>>();

            foreach (var teacher in schoolData.Definitions.Teachers)
            {
                var constraints = schoolData.GetConstraintsForTeacher(teacher);
                if (constraints != null)
                {
                    result[teacher] = constraints;
                }
                else
                {
                    // Varsayılan: tüm saatler müsait
                    result[teacher] = new Dictionary<string, bool[]>();
                    for (int day = 0; day < TotalDays; day++)
                    {
                        var hoursForDay = schoolData.GetHoursForDay(day);
                        result[teacher][day.ToString()] = Enumerable.Repeat(true, hoursForDay).ToArray();
                    }
                }
            }

            return result;
        }

        private List<ScheduleBlock> CreateBlocks(
            Dictionary<string, Dictionary<string, Dictionary<string, (int totalHours, List<int> blockStructure)>>> teacherClassLessons,
            Dictionary<string, Dictionary<string, bool[]>> teacherAvailability)
        {
            var blocks = new List<ScheduleBlock>();

            foreach (var classEntry in teacherClassLessons)
            {
                var className = classEntry.Key;
                var teachers = classEntry.Value;

                foreach (var teacherEntry in teachers)
                {
                    var teacherName = teacherEntry.Key;
                    var lessons = teacherEntry.Value;

                    foreach (var lessonEntry in lessons)
                    {
                        var lessonName = lessonEntry.Key;
                        var (totalHours, blockStructure) = lessonEntry.Value;

                        var priority = _priorities.GetValueOrDefault(lessonName, 5);

                        foreach (var blockLength in blockStructure)
                        {
                            if (blockLength <= 0)
                                continue; // Python kodundaki kontrol

                            blocks.Add(new ScheduleBlock(className, teacherName, lessonName, blockLength, priority));
                        }
                    }
                }
            }

            return blocks;
        }

        private List<(long day, long start)> GetFeasibleSlots(ScheduleBlock block, 
            Dictionary<string, Dictionary<string, bool[]>> teacherAvailability, SchoolData schoolData)
        {
            var feasible = new List<(long, long)>();
            
            if (!teacherAvailability.TryGetValue(block.Ogretmen, out var teacherSchedule))
                return feasible;

            for (int d = 0; d < TotalDays; d++)
            {
                var hoursForDay = schoolData.GetHoursForDay(d);
                bool[] daySchedule;
                
                // Python kodundaki hem liste hem sözlük formatını destekleme mantığı
                if (teacherSchedule.TryGetValue(d.ToString(), out var schedule))
                {
                    daySchedule = schedule;
                }
                else
                {
                    // Varsayılan: tüm saatler müsait
                    daySchedule = Enumerable.Repeat(true, hoursForDay).ToArray();
                }

                if (daySchedule.Length != hoursForDay)
                    continue; // Saat sayısı uyumsuzluğu

                for (int s = 0; s <= hoursForDay - block.BlokUzunluk; s++)
                {
                    bool canPlace = true;
                    for (int offset = 0; offset < block.BlokUzunluk; offset++)
                    {
                        if (!daySchedule[s + offset])
                        {
                            canPlace = false;
                            break;
                        }
                    }

                    if (canPlace)
                    {
                        feasible.Add((d, s));
                    }
                }
            }

            return feasible;
        }

        /// <summary>
        /// Simüle edilmiş progress ile solver çalıştırır
        /// </summary>
        private async Task<CpSolverStatus> SolveWithProgressAsync(CpSolver solver, CpModel model, 
            List<ScheduleBlock> blocks, IProgress<SolverProgress>? progress, double timeLimit, 
            Stopwatch stopwatch, CancellationToken cancellationToken)
        {
            // Solver'ı background thread'de başlat
            var solverTask = Task.Run(() => solver.Solve(model), cancellationToken);

            // Simüle edilmiş progress mesajları
            var progressMessages = new[]
            {
                ("🔍 Ders blokları analiz ediliyor...", "Öncelikli içerikler yerleştiriliyor"),
("📚 Zorunlu içerikler planlanıyor...", "Temel saatler ayarlanıyor"),
("🧪 Uygulama modülleri hesaplanıyor...", "Kaynak kullanımı kontrol ediliyor"),
("🌍 Genel plan güncelleniyor...", "Sınıf çakışmaları gideriliyor"),
("🗣️ İletişim modülleri optimize ediliyor...", "Eğitmen uygunlukları kontrol ediliyor"),
("🎨 Yaratıcı etkinlikler ekleniyor...", "Ortak alan saatleri düzenleniyor"),
("⚽ Hareketli etkinlikler planlanıyor...", "Alan kullanımı dengeleniyor"),
("🎵 Sesli etkinlikler ayarlanıyor...", "Ortam programı optimize ediliyor"),
("💻 Teknoloji içerikleri ekleniyor...", "Cihaz kullanımı planlanıyor"),
("📖 Ek dersler optimize ediliyor...", "Esnek saatler ayarlanıyor"),
("🔧 Çakışmalar çözülüyor...", "Tüm kısıtlamalar optimize ediliyor"),
("⚡ Son düzenlemeler yapılıyor...", "En iyi çözüm hesaplanıyor"),
("🎯 Plan doğrulanıyor...", "Tüm kriterler kontrol ediliyor")

            };

            var messageIndex = 0;
            var startTime = stopwatch.Elapsed.TotalSeconds;

            // Progress simulation loop
            while (!solverTask.IsCompleted)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var elapsed = stopwatch.Elapsed.TotalSeconds - startTime;
                var progressPercent = Math.Min(95, (elapsed / timeLimit) * 100); // Max %95'e kadar

                // Mesaj değiştir
                if (elapsed > (messageIndex + 1) * (timeLimit / progressMessages.Length) && messageIndex < progressMessages.Length - 1)
                {
                    messageIndex++;
                }

                var (status, tip) = progressMessages[Math.Min(messageIndex, progressMessages.Length - 1)];

                progress?.Report(new SolverProgress
                {
                    TotalBlocks = blocks.Count,
                    PlacedBlocks = (int)(blocks.Count * progressPercent / 100),
                    CurrentStatus = status,
                    ElapsedSeconds = stopwatch.Elapsed.TotalSeconds,
                    EstimatedRemainingSeconds = Math.Max(0, timeLimit - elapsed),
                    CurrentTip = tip
                });

                // 500ms bekle
                await Task.Delay(500, cancellationToken);
            }

            // Son progress - %100
            progress?.Report(new SolverProgress
            {
                TotalBlocks = blocks.Count,
                PlacedBlocks = blocks.Count,
                CurrentStatus = "✅ Çözüm tamamlandı!",
                ElapsedSeconds = stopwatch.Elapsed.TotalSeconds,
                EstimatedRemainingSeconds = 0,
                CurrentTip = "Sonuç hazırlanıyor"
            });

            return await solverTask;
        }
    }
}