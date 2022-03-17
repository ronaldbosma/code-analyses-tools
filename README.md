# Code Analyses Tools

These are some handy tools and scripts I've used in the past to perform code analyses.

>NOTE: The solutions mostly have hardcodes paths to folders and files, so you might need to change these.

## File Splitter

Can split a file in multiple smaller files. For instance it can split a large .sql file in smaller ones, splitting on the keyword `GO`.

## PowerShell

Folder with a few handy PowerShell scripts. See the top comments in the scripts for more details.

## SQL

Folder with Scripts:
- DependencyAnalysis: scripts to perform a dependency analysis between stored procedures and tables.

## Solution Project Checker

Has features
1. Checking which project files in a certain folder are not linked in a specific solution file.
1. Extracting the target frameworks of each project in a folder (and subfolders), and writing the result to a CSV.
1. Extracting the package references of each project in a folder (and subfolders), and writing the result to a CSV. 
   - Will look for `PackageReference` elements in a `(*.csproj|*.vbproj)` file. Can handle package references with the version as an attribute and as an element.
   - Will look for packages in `package.config` files that are in the same folder as a `(*.csproj|*.vbproj)`
   - Will also list all unique combinations of package id and version in a separate CSV.

## StrykerReportMerger

Can merge multiple Stryker JSON report files (mutation-report.json). 
It will:
- Look for all `mutation-report.json` in a specified folder and subfolders.
- Merge the reports that have the same project root.
  This can be the case if you have different test projects that test the same project. 
  It will look for the same files and merge the mutants by taking the mutant with the best status. 
  E.g. `Killed` takes precedence over `Surived`, `Survived` over `NoCoverage`, etc.
- Add the project folder to the file paths in the report so the report is grouped per project/folder.
- Combine the merged reports of the different project roots into one report.
- Export the combined report as a JSON.
- Add the JSON to an [HTML report template](./PowerShell/stryker/stryker-mutation-report-template.html), resulting in the HTML report.