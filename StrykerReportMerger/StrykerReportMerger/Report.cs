using Newtonsoft.Json;

namespace StrykerReportMerger
{
    internal class Report
    {
        [JsonProperty(Order = 0)]
        public string schemaVersion { get; set; } = null!;

        [JsonProperty(Order = 1)]
        public Thresholds thresholds { get; set; } = null!;

        [JsonProperty(Order = 2)]
        public string projectRoot { get; set; } = null!;
        
        [JsonProperty(Order = 3)]
        public Dictionary<string, File> files { get; set;} = null!;

        internal void MergeWith(Report otherReport)
        {
            foreach (var otherFile in otherReport.files)
            {
                if (files.ContainsKey(otherFile.Key))
                {
                    files[otherFile.Key].MergeWith(otherFile.Value);
                }
                else
                {
                    files.Add(otherFile.Key, otherFile.Value);
                }
            }
        }

        internal void AddProjectFolderToFilePaths(string rootFolder)
        {
            var projectFolder = Path.GetRelativePath(rootFolder, projectRoot);

            files = files.ToDictionary(
                f => Path.Combine(projectFolder, f.Key),
                f => f.Value
            );
        }

        internal void Combine(Report otherReport)
        {
            foreach (var otherFile in otherReport.files)
            {
                files.Add(otherFile.Key, otherFile.Value);
            }
        }
    }
}
