<#
	With this script you can analyse multiple solutions and add them to one SonarQube project.
	This can be usefull when there is code duplication across solutions.
	Only builds solutions. Does not execute tests.
#>

$sonarHostUrl = "http://localhost:9000" 
$token = "*****"
$sonarQubeProjectKey = "<the-sonarqube-project-key>"

$solutions = @(
	"C:\repos\my-solution1\my-solution1.sln",
	"C:\repos\my-solution2\my-solution2.sln",
	"C:\repos\my-solution3\my-solution3.sln"
)

$testResultsPath = Join-Path (Get-Location) "TestResults"


Write-Host "Start analysis"
dotnet sonarscanner begin /k:$sonarQubeProjectKey `
	/d:sonar.host.url=$sonarHostUrl `
	/d:sonar.login=$token `
	/d:sonar.cs.opencover.reportsPaths="$testResultsPath\**\*.opencover.xml" `
	/d:sonar.cs.vstest.reportsPaths="$testResultsPath\**\*.trx" `
	/d:sonar.coverage.exclusions="**Tests*.cs"



foreach ($solutionFilePath in $solutions)
{
	$solutionFileName = Split-Path $solutionFilePath -Leaf
	$solutionFolder = Split-Path $solutionFilePath
	$solutionName = [io.path]::GetFileNameWithoutExtension($solutionFilePath)

	Write-Host "Solution: $solutionFileName"
	Write-Host "Project: $solutionName"
	Write-Host "Location: $(Get-Location)"
	
	Write-Host "NuGet Restore"
	dotnet restore $solutionFilePath

	Write-Host "Build solution"
	dotnet build $solutionFilePath --configuration Release --force --no-incremental --no-restore
}



Write-Host "Finish analysis"
dotnet sonarscanner end /d:sonar.login=$token