<#
	.SYNOPSIS
        Analyse .NET (Core) solution with SonarQube
		
	.DESCRIPTION
        Performs an analysis of a solution with SonarQube.
		It will:
			- step into the solution folder
			- restore NuGet packages
			- start the SonqarQube analysis
			- build the solution
			- run the tests and collect code coverage
			- analyse the results with SonarQube
			- return to the current folder
			
	.PARAMETER solutionFilePath
		The full path of the solution file.
		
	.PARAMETER sonarHostUrl
		The url to the SonarQube host. E.g. http://localhost:9000
		
	.PARAMETER token
		The token to use when connecting to SonarQube.
		
	.EXAMPLE
		.\sonarqube-perform-analysis-with-dotnet.ps1 "C:\temp\my-solution.sln" "http://localhost:9000" "*****"
		
	.NOTES
		Requires dotnet-sonarscanner to be installed as global dotnet tool.
		Use the following command to install:
			dotnet tool install --global dotnet-sonarscanner
		  
		If code coverage doesn't work, you might have to add a reference to the coverlet.msbuild package in your test projects
#>

param($solutionFilePath, $sonarHostUrl, $token)


$solutionFileName = Split-Path $solutionFilePath -Leaf
$solutionFolder = Split-Path $solutionFilePath
$solutionName = [io.path]::GetFileNameWithoutExtension($solutionFilePath)

Write-Host "Solution: $solutionFileName"
Write-Host "Project: $solutionName"
Write-Host "Location: $(Get-Location)"


Push-Location $solutionFolder


Write-Host "NuGet Restore"
dotnet restore $solutionFileName


Write-Host "Start analysis"
dotnet sonarscanner begin /k:$solutionName `
    /d:sonar.host.url=$sonarHostUrl `
    /d:sonar.login=$token `
    /d:sonar.cs.opencover.reportsPaths="$solutionFolder\TestResults\**\*.opencover.xml" `
    /d:sonar.cs.vstest.reportsPaths="$solutionFolder\TestResults\**\*.trx" `
    /d:sonar.coverage.exclusions="**Tests*.cs"


Write-Host "Build solution"
dotnet build $solutionFileName --configuration Release --force --no-incremental --no-restore


# Coverlet doesn't seem to merge the results, so we execute `dotnet test` for each test project individually
$testProjects = Get-ChildItem -Path $solutionFolder -Include "*Test*.csproj" -Recurse
foreach ($testProject in $testProjects)
{
	$projectName = [io.path]::GetFileNameWithoutExtension($testProject.FullName)
	Write-Host "Test $projectName"
	dotnet test $testProject.FullName `
		/p:CollectCoverage=true `
		/p:CoverletOutputFormat=opencover `
		/p:CoverletOutput="$solutionFolder/TestResults/$projectName.opencover.xml" `
		--configuration Release `
		--no-build `
		--no-restore `
		--logger trx `
		--results-directory "$solutionFolder\TestResults"
	
	# You can add a test filter to the `dotnet test` command to exclude certain tests. See examples below.
		#--filter "TestCategory!=Integration" `
	    #--filter "FullyQualifiedName!~My.Solution.And.Project.Namespace" `
}


Write-Host "Finish analysis"
dotnet sonarscanner end /d:sonar.login=$token


Pop-Location