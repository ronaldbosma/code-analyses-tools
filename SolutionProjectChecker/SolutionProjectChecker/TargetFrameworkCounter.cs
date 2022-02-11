using System.Text.RegularExpressions;

namespace SolutionProjectChecker
{
    /// <summary>
    /// Class to extract the target framework per project file and write results to CSV.
    /// </summary>
    internal class TargetFrameworkCounter
    {
        public static void Run()
        {
            const string rootFolder = @"C:\repos\foo";
            const string outputFile = @"C:\repos\foo\target-frameworks.csv";

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
    }
}
