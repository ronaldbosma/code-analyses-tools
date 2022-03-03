using System.Text.RegularExpressions;

namespace SolutionProjectChecker
{
    /// <summary>
    /// Class to extract the package references per project file and write results to CSV.
    /// Will also list all unique combinations of package id and version in a separate CSV.
    /// </summary>
    internal class PackageReferenceLister
    {
        public static void Run()
        {
            const string rootFolder = @"C:\repos\foo";
            const string outputFileAllPackageReferences = @"C:\repos\foo\package -references-all.csv";
            const string outputFileUniquePackageReferences = @"C:\repos\foo\package-references-unique.csv";

            Regex packageReferencePattern = new Regex(@"<PackageReference Include=""([A-Za-z0-9\.-]+)"" Version=""([A-Za-z0-9\.-]+)"" />");

            var csprojFiles = Directory.GetFiles(rootFolder, "*.csproj", SearchOption.AllDirectories);
            var vbprojFiles = Directory.GetFiles(rootFolder, "*.vbproj", SearchOption.AllDirectories);

            var projectFiles = csprojFiles.Union(vbprojFiles);
            var packageReferences = new List<PackageReference>();

            foreach (var projFile in projectFiles)
            {
                using (var reader = new StreamReader(projFile))
                {
                    var line = reader.ReadLine();
                    while (line != null)
                    {
                        var match = packageReferencePattern.Match(line);

                        if (match.Success && match.Groups.Count > 2)
                        {
                            var packageId = match.Groups[1].Value;
                            var packageVersion = match.Groups[2].Value;
                            packageReferences.Add(new PackageReference(projFile, new Package(packageId, packageVersion)));
                        }

                        line = reader.ReadLine();
                    }
                }
            }

            Console.WriteLine($"Create: {outputFileAllPackageReferences}");
            using (var writer = new StreamWriter(outputFileAllPackageReferences))
            {
                writer.WriteLine("Project;Package Id;Package Version");
                foreach (var item in packageReferences)
                {
                    Console.WriteLine($"{item.ProjectFile}: {item.Package.Id} - {item.Package.Version}");
                    writer.WriteLine($"{item.ProjectFile.Replace(rootFolder, ".")}; {item.Package.Id}; {item.Package.Version}");
                }
            }

            Console.WriteLine($"Create: {outputFileUniquePackageReferences}");
            using (var writer = new StreamWriter(outputFileUniquePackageReferences))
            {
                var uniquePackageReferences = (from pr in packageReferences
                                               group pr by pr.Package into package
                                               orderby package.Key.Id, package.Key.Version
                                               select package
                                              )
                                              .Distinct();

                writer.WriteLine("Package Id;Package Version;Number of Occurrences");
                foreach (var item in uniquePackageReferences)
                {
                    var package = item.Key;

                    Console.WriteLine($"{package.Id} - {package.Version} ({item.Count()})");
                    writer.WriteLine($"{package.Id}; {package.Version}; {item.Count()}");
                }
            }
        }

        private readonly record struct PackageReference(string ProjectFile, Package Package);
        private readonly record struct Package(string Id, string Version);
    }
}
