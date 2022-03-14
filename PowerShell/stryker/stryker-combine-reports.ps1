<#
	.SYNOPSIS
		Combines Stryker reports.

	.DESCRIPTION
		Looks for `mutation-report.json` files in the specified root folder and merges them into a single HTML report.
		
	.PARAMETER rootFolder
		The root folder in which to look for `mutation-report.json` files.

	.PARAMETER mutationReportTemplateFilePath
		Path to the HTML report template.

	.EXAMPLE
		.\stryker-combine-reports.ps1 "C:\temp\my-repo" "C:\temp\stryker-mutation-report-template.html"

	.NOTES
		Requires PowerShell Core 7 or newer. We need the -EscapeHandling parameter on ConvertTo-Json to escape HTML-specific characters in the json.
#>

[cmdletbinding()]
param
(
	$rootFolder,
	$mutationReportTemplateFilePath
)


if ($PSVersionTable.PSEdition -ne "Core" -or $PSVersionTable.PSVersion -lt 7)
{
	throw "PowerShell Core 7 of newer required. We need the -EscapeHandling parameter on ConvertTo-Json to escape HTML-specific characters in the json"
}


Write-Verbose "Parameters:"
Write-Verbose "- rootFolder: $rootFolder"
Write-Verbose "- mutationReportTemplateFilePath: $mutationReportTemplateFilePath"


$combinedReportHtmlFile = Join-Path $rootFolder "combined-mutation-report.html"
$combinedReport = $null


$reportFiles = Get-ChildItem -Path $rootFolder -Include "mutation-report.json" -Recurse

if ($reportFiles.Length -eq 0)
{
	Write-Warning "No mutation reports found in: $rootFolder"
}
else
{
	# Use the first report for the base values
	$combinedReport = (ConvertFrom-Json (Get-Content $reportFiles[0].FullName -Raw))
	$combinedReport.projectRoot = $rootFolder
	# Clear the collection of files. When we add the files to the combined report, we prefix the file paths with the project name. And we also want to do that for the first report.
	$combinedReport.files = [PSCustomObject]@{}
	
	foreach ($reportFile in $reportFiles)
	{
		Write-Host "Load report: $($reportFile.FullName)"
		
		$report = ConvertFrom-Json (Get-Content $reportFile.FullName -Raw)
		$projectName = Split-Path $report.projectRoot -Leaf
		
		# Add files from the report to the files from the combined report
		# Add the project name to the file name so the files are grouped per project in the combined report
		$report.files.psobject.Properties | ForEach-Object {
			$combinedReport.files | Add-Member -MemberType $_.MemberType -Name (Join-Path $projectName $_.Name) -Value $_.Value -Force
		}
	}

	# Add combined report JSON to report template and write to .html file
	$reportTemplate = (Get-Content $mutationReportTemplateFilePath -Raw | Out-String)
	
	# We pass in the -EscapeHandling parameter to escape HTML-specific characters
	$json = ConvertTo-Json $combinedReport -Depth 15 -Compress -EscapeHandling EscapeHtml
	$html = $reportTemplate.Replace("##REPORT_JSON##", $json)
	
	Write-Host "Generate combined HTML report: $combinedReportHtmlFile"
	Set-Content -Path $combinedReportHtmlFile -Value $html -Force
}

