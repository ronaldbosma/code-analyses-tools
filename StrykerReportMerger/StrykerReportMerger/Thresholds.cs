using Newtonsoft.Json;

namespace StrykerReportMerger
{
    internal class Thresholds
    {
        [JsonProperty(Order = 1)]
        public int high { get; set; }

        [JsonProperty(Order = 2)]
        public int low { get; set; }
    }
}
