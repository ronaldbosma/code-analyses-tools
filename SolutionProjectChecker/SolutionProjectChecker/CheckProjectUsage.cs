using System.Text.RegularExpressions;

namespace SolutionProjectChecker
{
    /// <summary>
    /// Checks which project files in a folder are not linked in the specified solution.
    /// </summary>
    internal class CheckProjectUsage
    {
        public static void Run()
        {
            const string rootFolder = @"C:\repos\foo";
            const string solutionFile = @"C:\repos\foo\foo.sln";
            Regex projectNamePattern = new Regex(@"[.A-Za-z0-9_-]*\.(csproj|vbproj|sqlproj)");


            var projectsInSolutionFile = new HashSet<string>();

            using var reader = new StreamReader(solutionFile);
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var match = projectNamePattern.Match(line!);

                if (!string.IsNullOrWhiteSpace(match.Value) && !projectsInSolutionFile.Contains(match.Value))
                {
                    projectsInSolutionFile.Add(match.Value);
                }
            }


            var csprojFiles = Directory.GetFiles(rootFolder, "*.csproj", SearchOption.AllDirectories);
            var vbprojFiles = Directory.GetFiles(rootFolder, "*.vbproj", SearchOption.AllDirectories);
            var sqlprojFiles = Directory.GetFiles(rootFolder, "*.sqlproj", SearchOption.AllDirectories);

            var projFiles = csprojFiles.Union(vbprojFiles).Union(sqlprojFiles);
            var projectsNotInSolutionFile = projFiles.Where(p => !projectsInSolutionFile.Contains(Path.GetFileName(p)))
                                                     .OrderBy(p => p);

            foreach (var p in projectsNotInSolutionFile)
            {
                Console.WriteLine(p);
            }
        }
    }
}
