using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace LessonScheduler.Models
{
    public class LessonAssignment
    {
        [JsonProperty("ders")]
        public string Ders { get; set; } = string.Empty;

        [JsonProperty("ogretmen")]
        public string Ogretmen { get; set; } = string.Empty;

        [JsonProperty("toplam_saat")]
        public int ToplamSaat { get; set; }

        [JsonProperty("blok_yapisi")]
        public string BlokYapisi { get; set; } = string.Empty;

        /// <summary>
        /// Blok yapısını integer listesine çevirir (örn: "2,2,1" -> [2,2,1])
        /// </summary>
        public List<int> GetBlockStructure()
        {
            if (string.IsNullOrWhiteSpace(BlokYapisi))
                return new List<int>();

            return BlokYapisi.Split(',')
                           .Select(s => s.Trim())
                           .Where(s => int.TryParse(s, out _))
                           .Select(int.Parse)
                           .ToList();
        }

        /// <summary>
        /// Blok yapısının toplam saatinin ToplamSaat ile eşleşip eşleşmediğini kontrol eder
        /// </summary>
        public bool IsBlockStructureValid()
        {
            var blocks = GetBlockStructure();
            return blocks.Sum() == ToplamSaat && blocks.All(b => b > 0);
        }
    }
}