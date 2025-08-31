using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;
using LessonScheduler.Models;

namespace LessonScheduler.Services
{
    public class PdfExportService
    {
        private readonly string[] _days = { "Pazartesi", "Salı", "Çarşamba", "Perşembe", "Cuma" };
        private readonly string[] _hours = { "1. Saat", "2. Saat", "3. Saat", "4. Saat", "5. Saat", "6. Saat", "7. Saat" };

        // Türkçe karakter desteği için font helper
        private Font GetTurkishFont(int size, int style = Font.NORMAL, BaseColor? color = null)
        {
            color = color ?? BaseColor.BLACK;
            
            // En basit çözüm - standart font
            return FontFactory.GetFont(FontFactory.HELVETICA, size, style, color);
        }

        // Türkçe karakterleri PDF'de görüntülenebilir karakterlerle değiştir
        private string FixTurkishChars(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            
            return text
                .Replace("ç", "c")
                .Replace("Ç", "C")
                .Replace("ğ", "g")
                .Replace("Ğ", "G")
                .Replace("ı", "i")
                .Replace("İ", "I")
                .Replace("ö", "o")
                .Replace("Ö", "O")
                .Replace("ş", "s")
                .Replace("Ş", "S")
                .Replace("ü", "u")
                .Replace("Ü", "U");
        }

        public async Task ExportClassSchedulesToPdfAsync(SchoolData schoolData, string filePath, bool separateFiles = false)
        {
            if (schoolData.Solution == null)
                throw new InvalidOperationException("Henüz çözüm oluşturulmadı.");

            if (separateFiles)
            {
                await ExportClassSchedulesSeparatelyAsync(schoolData, filePath);
            }
            else
            {
                await ExportClassSchedulesCombinedAsync(schoolData, filePath);
            }
        }

        public async Task ExportTeacherSchedulesToPdfAsync(SchoolData schoolData, string filePath, bool separateFiles = false)
        {
            if (schoolData.Solution == null)
                throw new InvalidOperationException("Henüz çözüm oluşturulmadı.");

            if (separateFiles)
            {
                await ExportTeacherSchedulesSeparatelyAsync(schoolData, filePath);
            }
            else
            {
                await ExportTeacherSchedulesCombinedAsync(schoolData, filePath);
            }
        }

        private async Task ExportClassSchedulesCombinedAsync(SchoolData schoolData, string filePath)
        {
            await Task.Run(() =>
            {
                using var document = new Document(PageSize.A4.Rotate(), 20, 20, 30, 30);
                using var writer = PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create));
                
                document.Open();

                // Başlık kaldırıldı - direkt sınıf programlarına geç

                // Her sınıf için program
                foreach (var className in schoolData.Definitions.Classes.OrderBy(c => c))
                {
                    AddClassScheduleToDocument(document, schoolData, className);
                    document.NewPage();
                }

                document.Close();
            });
        }

        private async Task ExportClassSchedulesSeparatelyAsync(SchoolData schoolData, string basePath)
        {
            var directory = Path.GetDirectoryName(basePath);
            var baseFileName = Path.GetFileNameWithoutExtension(basePath);

            await Task.Run(() =>
            {
                foreach (var className in schoolData.Definitions.Classes.OrderBy(c => c))
                {
                    var fileName = Path.Combine(directory!, $"{baseFileName}_{className}.pdf");
                    
                    using var document = new Document(PageSize.A4.Rotate(), 20, 20, 30, 30);
                    using var writer = PdfWriter.GetInstance(document, new FileStream(fileName, FileMode.Create));
                    
                    document.Open();
                    AddClassScheduleToDocument(document, schoolData, className);
                    document.Close();
                }
            });
        }

        private async Task ExportTeacherSchedulesCombinedAsync(SchoolData schoolData, string filePath)
        {
            await Task.Run(() =>
            {
                using var document = new Document(PageSize.A4.Rotate(), 20, 20, 30, 30);
                using var writer = PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create));
                
                document.Open();

                // Başlık kaldırıldı - direkt öğretmen programlarına geç

                // Her öğretmen için program
                foreach (var teacherName in schoolData.Definitions.Teachers.OrderBy(t => t))
                {
                    AddTeacherScheduleToDocument(document, schoolData, teacherName);
                    document.NewPage();
                }

                document.Close();
            });
        }

        private async Task ExportTeacherSchedulesSeparatelyAsync(SchoolData schoolData, string basePath)
        {
            var directory = Path.GetDirectoryName(basePath);
            var baseFileName = Path.GetFileNameWithoutExtension(basePath);

            await Task.Run(() =>
            {
                foreach (var teacherName in schoolData.Definitions.Teachers.OrderBy(t => t))
                {
                    var fileName = Path.Combine(directory!, $"{baseFileName}_{teacherName}.pdf");
                    
                    using var document = new Document(PageSize.A4.Rotate(), 20, 20, 30, 30);
                    using var writer = PdfWriter.GetInstance(document, new FileStream(fileName, FileMode.Create));
                    
                    document.Open();
                    AddTeacherScheduleToDocument(document, schoolData, teacherName);
                    document.Close();
                }
            });
        }

        private void AddClassScheduleToDocument(Document document, SchoolData schoolData, string className)
        {
            // Okul adı header
            AddSchoolHeader(document, schoolData);
            
            // Sınıf başlığı - Türkçe karakter desteği
            var classFont = GetTurkishFont(16, Font.BOLD, BaseColor.BLUE);
            var classTitle = new Paragraph($"🎓 {FixTurkishChars(className)} PROGRAMI", classFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 15
            };
            document.Add(classTitle);

            // Program tablosu oluştur
            var schedule = CreateClassScheduleGrid(schoolData, className);
            var table = CreateScheduleTable(schedule, className, true, schoolData);
            document.Add(table);

            // Müdür adı footer
            AddPrincipalFooter(document, schoolData);
            
            // Boşluk
            document.Add(new Paragraph(" ") { SpacingAfter = 20 });
        }

        private void AddTeacherScheduleToDocument(Document document, SchoolData schoolData, string teacherName)
        {
            // Okul adı header
            AddSchoolHeader(document, schoolData);
            
            // Öğretmen başlığı - Türkçe karakter desteği
            var teacherFont = GetTurkishFont(16, Font.BOLD, BaseColor.BLUE);
            var teacherTitle = new Paragraph($"👨‍🏫 {FixTurkishChars(teacherName)} PROGRAMI", teacherFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 15
            };
            document.Add(teacherTitle);

            // Program tablosu oluştur
            var schedule = CreateTeacherScheduleGrid(schoolData, teacherName);
            var table = CreateScheduleTable(schedule, teacherName, false, schoolData);
            document.Add(table);

            // Müdür adı footer
            AddPrincipalFooter(document, schoolData);
            
            // Boşluk
            document.Add(new Paragraph(" ") { SpacingAfter = 20 });
        }

        private string[][] CreateClassScheduleGrid(SchoolData schoolData, string className)
        {
            var maxHours = schoolData.GetMaxHoursPerDay();
            var schedule = new string[maxHours][];
            
            for (int hour = 0; hour < maxHours; hour++)
            {
                schedule[hour] = new string[5];
                for (int day = 0; day < 5; day++)
                {
                    schedule[hour][day] = "";
                }
            }

            if (schoolData.Solution?.Blocks != null)
            {
                foreach (var block in schoolData.Solution.Blocks.Values)
                {
                    if (block.Sinif == className)
                    {
                        for (int offset = 0; offset < block.BlokUzunluk; offset++)
                        {
                            int hour = block.Start + offset;
                            if (hour < maxHours)
                            {
                                schedule[hour][block.Day] = $"{block.Ders}\n({block.Ogretmen})";
                            }
                        }
                    }
                }
            }

            return schedule;
        }

        private string[][] CreateTeacherScheduleGrid(SchoolData schoolData, string teacherName)
        {
            var maxHours = schoolData.GetMaxHoursPerDay();
            var schedule = new string[maxHours][];
            
            for (int hour = 0; hour < maxHours; hour++)
            {
                schedule[hour] = new string[5];
                for (int day = 0; day < 5; day++)
                {
                    schedule[hour][day] = "";
                }
            }

            if (schoolData.Solution?.Blocks != null)
            {
                foreach (var block in schoolData.Solution.Blocks.Values)
                {
                    if (block.Ogretmen == teacherName)
                    {
                        for (int offset = 0; offset < block.BlokUzunluk; offset++)
                        {
                            int hour = block.Start + offset;
                            if (hour < maxHours)
                            {
                                schedule[hour][block.Day] = $"{block.Ders}\n({block.Sinif})";
                            }
                        }
                    }
                }
            }

            return schedule;
        }

        private PdfPTable CreateScheduleTable(string[][] schedule, string title, bool isClass, SchoolData schoolData)
        {
            var table = new PdfPTable(6) // 1 saat + 5 gün
            {
                WidthPercentage = 100,
                SpacingBefore = 10,
                SpacingAfter = 10
            };

            // Kolon genişlikleri
            table.SetWidths(new float[] { 1, 2, 2, 2, 2, 2 });

            // Header font - Türkçe karakter desteği
            var headerFont = GetTurkishFont(12, Font.BOLD, BaseColor.WHITE);
            var cellFont = GetTurkishFont(10, Font.NORMAL, BaseColor.BLACK);

            // Header satırı
            table.AddCell(CreateHeaderCell("⏰ Saat", headerFont));
            foreach (var day in _days)
            {
                table.AddCell(CreateHeaderCell($"📅 {day}", headerFont));
            }

            // Veri satırları - esnek saat yapısını destekle
            var maxHours = schoolData.GetMaxHoursPerDay();
            for (int hour = 0; hour < maxHours; hour++)
            {
                // Saat kolonu - dinamik saat isimleri
                var hourLabel = hour < _hours.Length ? _hours[hour] : $"{hour + 1}. Saat";
                table.AddCell(CreateDataCell(hourLabel, cellFont, true));

                // Gün kolonları
                for (int day = 0; day < 5; day++)
                {
                    var content = "";
                    
                    // Bu gün için bu saat var mı kontrol et
                    if (hour < schoolData.GetHoursForDay(day))
                    {
                        content = schedule[hour][day];
                    }
                    else
                    {
                        // Bu gün için bu saat yok, boş hücre
                        content = "-";
                    }
                    
                    var cell = CreateDataCell(content, cellFont, false);
                    
                    // Eğer bu saat bu gün için yoksa farklı renk
                    if (content == "-")
                    {
                        cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                    }
                    
                    table.AddCell(cell);
                }
            }

            return table;
        }

        private PdfPCell CreateHeaderCell(string text, Font font)
        {
            var cell = new PdfPCell(new Phrase(FixTurkishChars(text), font))
            {
                BackgroundColor = BaseColor.DARK_GRAY,
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                Padding = 8,
                MinimumHeight = 30
            };
            return cell;
        }

        private PdfPCell CreateDataCell(string text, Font font, bool isHour)
        {
            var cell = new PdfPCell(new Phrase(FixTurkishChars(text), font))
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                Padding = 6,
                MinimumHeight = 40
            };

            if (isHour)
            {
                cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            }
            else if (!string.IsNullOrEmpty(text))
            {
                cell.BackgroundColor = new BaseColor(230, 245, 255); // Açık mavi
            }

            return cell;
        }

        private void AddSchoolHeader(Document document, SchoolData schoolData)
        {
            var schoolName = schoolData.Configuration?.SchoolName ?? "Okul Adı";
            var headerFont = GetTurkishFont(20, Font.BOLD, BaseColor.DARK_GRAY);
            var header = new Paragraph($"🏫 {FixTurkishChars(schoolName)}", headerFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 10
            };
            document.Add(header);

            // Tarih bilgisi - Türkçe karakter desteği
            var dateFont = GetTurkishFont(10, Font.NORMAL, BaseColor.GRAY);
            var dateText = new Paragraph($"Oluşturulma Tarihi: {DateTime.Now:dd.MM.yyyy HH:mm}", dateFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 20
            };
            document.Add(dateText);
        }

        private void AddPrincipalFooter(Document document, SchoolData schoolData)
        {
            var principalName = schoolData.Configuration?.PrincipalName ?? "Müdür Adı";
            
            // Footer için boşluk
            // Boşluk ekle
            document.Add(new Paragraph(" ") { SpacingBefore = 30 });
            
            // Müdür imza alanı - Tablo ile düzgün hizalama
            var footerFont = GetTurkishFont(9, Font.NORMAL, BaseColor.BLACK);
            
            var footerTable = new PdfPTable(2);
            footerTable.WidthPercentage = 100;
            footerTable.SetWidths(new float[] { 70, 30 }); // Sol %70, sağ %30
            
            // Sol taraf boş
            var leftCell = new PdfPCell(new Phrase("", footerFont))
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            footerTable.AddCell(leftCell);
            
            // Sağ taraf müdür bilgisi - uzun isimleri otomatik kırp
            var displayName = principalName.Length > 15 ? principalName.Substring(0, 12) + "..." : principalName;
            var rightCell = new PdfPCell(new Phrase($"Müdür\n{FixTurkishChars(displayName)}", footerFont))
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE
            };
            footerTable.AddCell(rightCell);
            
            document.Add(footerTable);
        }
    }
}