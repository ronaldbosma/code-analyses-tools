using System.Text.RegularExpressions;

namespace SolutionProjectChecker
{
    /// <summary>
    /// Class to extract the target framework per project file and write results to CSV.
    /// </summary>
    internal class TargetFrameworkCounter
    {
        private const string rootFolder = @"C:\repos\foo";
        private const string outputFile = @"C:\repos\foo\target-frameworks.csv";
        private const bool lookForGlobalJson = true;

        public static void Run()
        {
            Regex targetFrameworkVersionPattern = new Regex(@"<TargetFrameworkVersion>([A-Za-z0-9\.-]+)<\/TargetFrameworkVersion>");
            Regex targetFrameworkPattern = new Regex(@"<TargetFramework>([A-Za-z0-9\.-]+)<\/TargetFramework>");

            var csprojFiles = Directory.GetFiles(rootFolder, "*.csproj", SearchOption.AllDirectories);
            var vbprojFiles = Directory.GetFiles(rootFolder, "*.vbproj", SearchOption.AllDirectories);

            var projectFiles = csprojFiles.Union(vbprojFiles);
            var targetFrameworks = new Dictionary<string, string>();

            foreach (var projFile in projectFiles)
            {
                using (var reader = new StreamReader(projFile))
                {
                    var targetFrameworkVersion = "";

                    var content = reader.ReadToEnd();
                    var match = targetFrameworkVersionPattern.Match(content);
                    if (!match.Success)
                    {
                        match = targetFrameworkPattern.Match(content);
                    }

                    if (match.Success && match.Groups.Count > 1)
                    {
                        targetFrameworkVersion = match.Groups[1].Value;
                    }

                    if (string.IsNullOrWhiteSpace(targetFrameworkVersion) && lookForGlobalJson)
                    {
                        var directory = Directory.GetParent(projFile)?.FullName;
                        targetFrameworkVersion = GetTargetFrameworkVersionFromGlobalJson(directory);
                    }

                    targetFrameworks.Add(projFile, targetFrameworkVersion);
                }
            }

            using (var writer = new StreamWriter(outputFile))
            {
                writer.WriteLine("Project;Target Framework");
                foreach (var item in targetFrameworks)
                {
                    Console.WriteLine($"{item.Key}: {item.Value}");
                    writer.WriteLine($"{item.Key.Replace(rootFolder, ".")}; {item.Value}");
                }
            }
        }

        private static string GetTargetFrameworkVersionFromGlobalJson(string? directory)
        {
            if (directory == null || directory.Equals(rootFolder, StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            var globalJsonPath = Path.Combine(directory, "global.json");

            if (File.Exists(globalJsonPath))
            {
                using (var reader = new StreamReader(globalJsonPath))
                {
                    var content = reader.ReadToEnd();
                    
                    Regex versionPattern = new Regex(@"""version"": ""([A-Za-z0-9\.-]+)""");
                    var match = versionPattern.Match(content);

                    if (match.Success && match.Groups.Count > 1)
                    {
                        return match.Groups[1].Value;
                    }
                }
            }

            return GetTargetFrameworkVersionFromGlobalJson(Directory.GetParent(directory)?.FullName);
        }
    }
}
