using System.Collections.Generic;
using Newtonsoft.Json;

namespace LessonScheduler.Models
{
    public class Definitions
    {
        [JsonProperty("lessons")]
        public List<string> Lessons { get; set; } = new();

        [JsonProperty("teachers")]
        public List<string> Teachers { get; set; } = new();

        [JsonProperty("classes")]
        public List<string> Classes { get; set; } = new();
    }
}