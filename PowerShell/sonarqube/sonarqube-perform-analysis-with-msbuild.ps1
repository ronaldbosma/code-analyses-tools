<#
	.SYNOPSIS
        Analyse .NET (Framework) solution with SonarQube.
		
	.DESCRIPTION
        Performs an analysis of a solution with SonarQube. Uses  `msbuild.exe` and `vstest.console.exe`.
		It will:
			- step into the solution folder
			- restore NuGet packages
			- start the SonqarQube analysis
			- build the solution
			- run the tests
			- analyse the results with SonarQube
			- return to the current folder
			
	.PARAMETER solutionFilePath
		The full path of the solution file.
		
	.PARAMETER sonarHostUrl
		The url to the SonarQube host. E.g. http://localhost:9000
		
	.PARAMETER token
		The token to use when connecting to SonarQube.

	.PARAMETER sonarScannerPath
		Path to the `SonarScanner.MSBuild.dll` of the sonnar scanner to use.
		Can be downloaded from: https://docs.sonarqube.org/latest/analysis/scan/sonarscanner-for-msbuild/
	
	.PARAMETER msbuildExePath
		Path to `msbuild.exe`. Used to build the solution.
	
	.PARAMETER vstestExePath
		Path to `vstest.console.exe`. Used to test the solution.

	.PARAMETER testAssemblyFilter
		The test assembly filter used for selecting which tests to execute.

	.EXAMPLE
		.\perform-sonarqube-analysis-with-msbuild.ps1 "C:\temp\my-solution.sln" "http://localhost:9000" "*****"
#>

[cmdletbinding()]
param
(
	$solutionFilePath,
	$sonarHostUrl = "http://localhost:9000",
	$token,
	$sonarScannerPath = "C:\Temp\sonar-scanners\sonar-scanner-msbuild-5.5.3.43281-net5.0\SonarScanner.MSBuild.dll",
	$msbuildExePath = "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\amd64\MSBuild.exe",
	$vstestExePath = "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe",
	$testAssemblyFilter = "**\bin\Debug\**\*Test*.dll"
)

Write-Verbose "Parameters:"
Write-Verbose "- solutionFilePath: $solutionFilePath"
Write-Verbose "- sonarHostUrl: $sonarHostUrl"
Write-Verbose "- token: *****"
Write-Verbose "- sonarScannerPath: $sonarScannerPath"
Write-Verbose "- msbuildExePath: $msbuildExePath"
Write-Verbose "- vstestExePath: $vstestExePath"
Write-Verbose "- testAssemblyFilter: $testAssemblyFilter"


$solutionFileName = Split-Path $solutionFilePath -Leaf
$solutionFolder = Split-Path $solutionFilePath
$solutionName = [io.path]::GetFileNameWithoutExtension($solutionFilePath)

Write-Host "Variables:"
Write-Host "- Solution: $solutionFileName"
Write-Host "- Project: $solutionName"
Write-Host "- Location: $(Get-Location)"


Push-Location $solutionFolder


Write-Host "NuGet Restore"
nuget.exe restore


Write-Host "Start analysis"
dotnet $sonarScannerPath begin /k:$solutionName /d:sonar.login=$token


Write-Host "Build solution"
& $msbuildExePath $solutionFileName /t:Rebuild


Write-Host "Run tests"
& $vstestExePath $testAssemblyFilter /collect:"Code Coverage"


Write-Host "Finish analysis"
dotnet $sonarScannerPath end /d:sonar.login=$token


Pop-Location