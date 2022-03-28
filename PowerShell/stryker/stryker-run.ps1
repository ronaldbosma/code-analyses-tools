<#
	.SYNOPSIS
        Run Stryker on all test projects of a solution.
		
    .DESCRIPTION
        Runs Stryker on all combinations of test projects and their project references, for a solution.
		It will:
			- step into the solution directory
			- look for test projects in the root folder and subfolders of the solution
			- for each test project
				- extract all project references 
				- execute Stryker for each combination of the test project and project reference
			- return to the current directory
			
	.PARAMETER solutionFilePath
		The full path of the solution file.
		
	.PARAMETER strykerConfigFile
		The path to the stryker config file to use.
		
	.PARAMETER testProjectFilter
		The filter to use for selecting test projects.
		
	.EXAMPLE
		.\stryker-run.ps1 "C:\temp\my-solution.sln" "C:\temp\stryker-config.json" "*Test*.csproj"
#>

[cmdletbinding()]
param
(
	$solutionFilePath,
	$strykerConfigFile,
	$testProjectFilter = "*Test*.csproj"
)


Write-Verbose "Parameters:"
Write-Verbose "- solutionFilePath: $solutionFilePath"
Write-Verbose "- testProjectFilter: $testProjectFilter"


$solutionFileName = Split-Path $solutionFilePath -Leaf
$solutionFolder = Split-Path $solutionFilePath
$solutionName = [io.path]::GetFileNameWithoutExtension($solutionFilePath)

Write-Host "Variables:"
Write-Host "- Solution: $solutionFileName"
Write-Host "- Project: $solutionName"
Write-Host "- Location: $(Get-Location)"



Push-Location $solutionFolder


$projectReferenceRegex = "[A-Za-z0-9\.-]*.csproj"

# Stryker can only run against one test project. So we loop over all of them.
$testProjects = Get-ChildItem -Path $solutionFolder -Include $testProjectFilter -Recurse
foreach ($testProject in $testProjects)
{
	$testProjectFolder = Split-Path $testProject.FullName
	Push-Location $testProjectFolder
		
	# If a test project has more than one Project Reference, we need to specify which project to mutate.
	# So we search for the project reference in the test project and loop over all of them.
	$matchResults = Select-String -Path $testProject.FullName -Pattern $projectReferenceRegex -AllMatches
	foreach ($matchResult in $matchResults)
	{
		foreach ($match in $matchResult.Matches)
		{
			$projectName = $match.Value
			
			Write-Host "Test Project: $($testProject.Name)"
			Write-Host "Project: $projectName"
			
			Write-Host "dotnet stryker --solution $solutionFilePath --test-project $($testProject.FullName) --project $projectName --config-file $strykerConfigFile"
			
			dotnet stryker --solution $solutionFilePath --test-project $testProject.FullName --project $projectName --config-file $strykerConfigFile
				
			# Write-Host "Press enter to continue"
			# Read-Host
		}
	}
	
	Pop-Location
}

Pop-Location