using Newtonsoft.Json;

namespace StrykerReportMerger
{
    internal class Mutant
    {
        [JsonProperty(Order = 0)]
        public string id { get; set; } = null!;

        [JsonProperty(Order = 1)]
        public string mutatorName { get; set; } = null!;

        [JsonProperty(Order = 2)]
        public string replacement { get; set; } = null!;

        [JsonProperty(Order = 3)]
        public Location location { get; set; } = null!;

        [JsonProperty(Order = 4)]
        public MutantStatus status { get; set; }

        [JsonProperty(Order = 5)]
        public string? statusReason { get; set; }

        [JsonProperty(Order = 6)]
        public bool @static { get; set; }
    }
}
