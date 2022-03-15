using Newtonsoft.Json;
using StrykerReportMerger;

// Folder in which to look for mutation-report.json (includes subfolders)
const string rootFolder = @$"C:\temp\foo";

// Path to the report template
const string reportTemplatePath = @"C:\temp\stryker-mutation-report-template.html";

// File path for the JSON file with the combined report
const string combinedJsonReportPath = @$"C:\temp\foo\StrykerOutput\combined-mutation-report.json";

// File path for the HTML file with the combined report
const string combinedHtmlReportPath = @$"C:\temp\foo\StrykerOutput\combined-mutation-report.html";


var reportFiles = SelectReportFiles(rootFolder);
var reports = LoadReports(reportFiles);
var projectReports = MergeReportsPerProject(reports);
var combinedReport = CombineReports(projectReports);
var json = ConvertToJson(combinedReport);
WriteToJsonFile(json, combinedJsonReportPath);
CreateHtmlReport(json, reportTemplatePath, combinedHtmlReportPath);



IEnumerable<string> SelectReportFiles(string folderPath)
{
    var reportFiles = Directory.GetFiles(folderPath, "mutation-report.json", SearchOption.AllDirectories);
    if (!reportFiles.Any())
    {
        throw new Exception($"No mutation-report.json files found in {folderPath}");
    }
    return reportFiles;
}


List<Report> LoadReports(IEnumerable<string> reportFiles)
{
    var reports = new List<Report>();
    foreach (var reportFile in reportFiles)
    {
        using var reader = new StreamReader(reportFile);
        var json = reader.ReadToEnd();
        var report = JsonConvert.DeserializeObject<Report>(json) ?? throw new InvalidOperationException("Unable to deserialize Json");

        reports.Add(report);
    }

    return reports;
}


List<Report> MergeReportsPerProject(List<Report> reports)
{
    // Group reports that are for the same project(Root)
    var projects = (from report in reports
                    group report by report.projectRoot into project
                    select project).ToList();


    // Merge reports that are for the same project
    var projectReports = new List<Report>();
    foreach (var project in projects)
    {
        var projectReport = project.First();
        foreach (var report in project.Skip(1))
        {
            projectReport.MergeWith(report);
        }

        // add the project folder to the file paths so the files are grouped by project in the combined result
        projectReport.AddProjectFolderToFilePaths(rootFolder);

        projectReports.Add(projectReport);
    }

    return projectReports;
}


Report CombineReports(List<Report> projectReports)
{
    var combinedReport = projectReports.First();
    foreach (var projectReport in projectReports.Skip(1))
    {
        combinedReport.Combine(projectReport);
    }
    return combinedReport;
}


string ConvertToJson(Report report)
{
    var settings = new JsonSerializerSettings
    {
        StringEscapeHandling = StringEscapeHandling.EscapeHtml
    };
    settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());

    var json = JsonConvert.SerializeObject(combinedReport, Formatting.None, settings);
    return json;
}


void WriteToJsonFile(string json, string outputFilePath)
{
    using var writer = new StreamWriter(outputFilePath);
    writer.Write(json);
}


void CreateHtmlReport(string json, string reportTemplatePath, string outputFilePath)
{
    // Load report template
    using var reader = new StreamReader(reportTemplatePath);
    var reportTemplate = reader.ReadToEnd();

    // Add json to the report template
    var combinedReportHtml = reportTemplate.Replace("##REPORT_JSON##", json);

    // Write combined HTML report
    using var writer = new StreamWriter(combinedHtmlReportPath);
    writer.Write(combinedReportHtml);
}