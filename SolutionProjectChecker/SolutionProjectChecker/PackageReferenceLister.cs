using System.Text.RegularExpressions;

namespace SolutionProjectChecker
{
    /// <summary>
    /// Class to extract the package references per project file and packages.config, and write results to CSV.
    /// Will also list all unique combinations of package id and version in a separate CSV.
    /// </summary>
    internal class PackageReferenceLister
    {
        private const string rootFolder = @"C:\repos\foo";
        private const string outputFileAllPackageReferences = @$"{rootFolder}\package-references-all.csv";
        private const string outputFileUniquePackageReferences = @$"{rootFolder}\package-references-unique.csv";

        // Patterns in *.csproj
        private static readonly Regex packageReferencePattern1 = new (@"<PackageReference Include=""([A-Za-z0-9\.-]+)"" Version=""([A-Za-z0-9\.-]+)"" />");
        private static readonly Regex packageReferencePattern2 = new(@"<PackageReference Include=""([A-Za-z0-9\.-]+)"">[ \n\r]*<Version>([A-Za-z0-9\.-]+)</Version>[ \n\r]*</PackageReference>");

        // Patterns in packages.config
        private static readonly Regex packagesConfigIdPattern = new (@"id=""([A-Za-z0-9\.-]+)""");
        private static readonly Regex packagesConfigVersionPattern = new (@"version=""([A-Za-z0-9\.-]+)""");

        private readonly record struct PackageReference(string SourceFile, Package Package);
        private readonly record struct Package(string Id, string Version);

        public static void Run()
        {
            var csprojFiles = Directory.GetFiles(rootFolder, "*.csproj", SearchOption.AllDirectories);
            var vbprojFiles = Directory.GetFiles(rootFolder, "*.vbproj", SearchOption.AllDirectories);
            var projectFiles = csprojFiles.Union(vbprojFiles);

            var packageReferences = new List<PackageReference>();
            foreach (var projFile in projectFiles)
            {
                packageReferences.AddRange(GetPackageReferencesFromProjectFile(projFile));
                packageReferences.AddRange(GetPackageReferencesFromPackagesConfig(projFile));
            }

            WriteAllPackageReferencesToCsv(packageReferences);
            WriteAllUniquePackagesToCsv(packageReferences);
        }

        private static IEnumerable<PackageReference> GetPackageReferencesFromProjectFile(string projFile)
        {
            var packageReferences = new List<PackageReference>();

            using (var reader = new StreamReader(projFile))
            {
                var content = reader.ReadToEnd();
                packageReferences.AddRange(GetPackageReferencesUsingRegex(projFile, content, packageReferencePattern1));
                packageReferences.AddRange(GetPackageReferencesUsingRegex(projFile, content, packageReferencePattern2));
            }

            return packageReferences;
        }

        private static IEnumerable<PackageReference> GetPackageReferencesUsingRegex(string projFile, string content, Regex regex)
        {
            var packageReferences = new List<PackageReference>();
            
            var matches = regex.Matches(content);
            if (matches != null)
            {
                foreach (var match in matches.ToList())
                {
                    if (match.Success && match.Groups.Count > 2)
                    {
                        var packageId = match.Groups[1].Value;
                        var packageVersion = match.Groups[2].Value;
                        packageReferences.Add(new PackageReference(projFile, new Package(packageId, packageVersion)));
                    }
                }
            }

            return packageReferences;
        }

        private static IEnumerable<PackageReference> GetPackageReferencesFromPackagesConfig(string projFile)
        {
            var packagesConfigPath = Path.Combine(Path.GetDirectoryName(projFile)!, "packages.config");
            var packageReferences = new List<PackageReference>();

            if (File.Exists(packagesConfigPath))
            {
                using (var reader = new StreamReader(packagesConfigPath))
                {
                    var line = reader.ReadLine();
                    while (line != null)
                    {
                        var matchId = packagesConfigIdPattern.Match(line);
                        var matchVersion = packagesConfigVersionPattern.Match(line);

                        if (matchId.Success && matchId.Groups.Count > 1)
                        {
                            var packageId = matchId.Groups[1].Value;
                            var packageVersion = "";

                            if (matchVersion.Success && matchVersion.Groups.Count > 1)
                            {
                                packageVersion = matchVersion.Groups[1].Value;
                            }

                            packageReferences.Add(new PackageReference(packagesConfigPath, new Package(packageId, packageVersion)));
                        }

                        line = reader.ReadLine();
                    }
                }
            }
            
            return packageReferences;
        }

        private static void WriteAllPackageReferencesToCsv(List<PackageReference> packageReferences)
        {
            Console.WriteLine($"Create: {outputFileAllPackageReferences}");
            using (var writer = new StreamWriter(outputFileAllPackageReferences))
            {
                writer.WriteLine("Source;Package Id;Package Version;Multiple Package Versions Found");
                foreach (var item in packageReferences)
                {
                    var multipleVersionsFound = packageReferences.Any(pr => pr.Package.Id == item.Package.Id && pr.Package.Version != item.Package.Version);

                    Console.WriteLine($"{item.SourceFile}: {item.Package.Id} - {item.Package.Version} - {multipleVersionsFound}");
                    writer.WriteLine($"{item.SourceFile.Replace(rootFolder, ".")}; {item.Package.Id}; {item.Package.Version}; {multipleVersionsFound}");
                }
            }
        }

        private static void WriteAllUniquePackagesToCsv(List<PackageReference> packageReferences)
        {
            Console.WriteLine($"Create: {outputFileUniquePackageReferences}");
            using (var writer = new StreamWriter(outputFileUniquePackageReferences))
            {
                var uniquePackageReferences = (from pr in packageReferences
                                               group pr by pr.Package into package
                                               orderby package.Key.Id, package.Key.Version
                                               select package
                                              )
                                              .Distinct();

                writer.WriteLine("Package Id;Package Version;Number of Occurrences;Multiple Package Versions Found");
                foreach (var item in uniquePackageReferences)
                {
                    var package = item.Key;
                    var multipleVersionsFound = packageReferences.Any(pr => pr.Package.Id == package.Id && pr.Package.Version != package.Version);

                    Console.WriteLine($"{package.Id} - {package.Version} ({item.Count()} - {multipleVersionsFound})");
                    writer.WriteLine($"{package.Id}; {package.Version}; {item.Count()}; {multipleVersionsFound}");
                }
            }
        }
    }
}
