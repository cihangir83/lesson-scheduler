using Newtonsoft.Json;

namespace LessonScheduler.Models
{
    public class ScheduleBlock
    {
        [JsonProperty("sinif")]
        public string Sinif { get; set; } = string.Empty;

        [JsonProperty("ogretmen")]
        public string Ogretmen { get; set; } = string.Empty;

        [JsonProperty("ders")]
        public string Ders { get; set; } = string.Empty;

        [JsonProperty("blok_uzunluk")]
        public int BlokUzunluk { get; set; }

        [JsonProperty("priority")]
        public int Priority { get; set; }

        [JsonProperty("day")]
        public int Day { get; set; }

        [JsonProperty("start")]
        public int Start { get; set; }

        public ScheduleBlock() { }

        public ScheduleBlock(string sinif, string ogretmen, string ders, int blokUzunluk, int priority)
        {
            Sinif = sinif;
            Ogretmen = ogretmen;
            Ders = ders;
            BlokUzunluk = blokUzunluk;
            Priority = priority;
        }
    }
}