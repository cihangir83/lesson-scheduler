using System.Collections.Generic;
using Newtonsoft.Json;

namespace LessonScheduler.Models
{
    public class SolutionData
    {
        [JsonProperty("blocks")]
        public Dictionary<int, ScheduleBlock> Blocks { get; set; } = new();

        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;

        [JsonProperty("time")]
        public double SolutionTime { get; set; }

        public SolutionData() { }

        public SolutionData(Dictionary<int, ScheduleBlock> blocks, string message, double solutionTime)
        {
            Blocks = blocks;
            Message = message;
            SolutionTime = solutionTime;
        }
    }
}