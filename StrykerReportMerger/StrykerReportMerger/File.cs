using Newtonsoft.Json;

namespace StrykerReportMerger
{
    internal class File
    {
        [JsonProperty(Order = 0)]
        public string language { get; set; } = null!;

        [JsonProperty(Order = 1)]
        public string source { get; set; } = null!;

        [JsonProperty(Order = 2)]
        public List<Mutant> mutants { get; set; } = null!;

        internal void MergeWith(File otherFile)
        {
            foreach (var otherMutant in otherFile.mutants)
            {
                var mutant = mutants.SingleOrDefault(m =>
                    m.mutatorName == otherMutant.mutatorName &&
                    m.replacement == otherMutant.replacement &&
                    m.location == otherMutant.location);
                
                if (mutant != null)
                {
                    if (mutant.status < otherMutant.status)
                    {
                        mutant.status = otherMutant.status;
                        mutant.statusReason = otherMutant.statusReason;
                    }
                }
                else
                {
                    mutants.Add(otherMutant);
                }
            }
        }
    }
}
