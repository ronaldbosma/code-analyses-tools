# This script looks for all 'Views' folders in the specified root folder.
# It then counts all the *.vb and *.cs files in those 'View' folders. Excluding *.Designer.vb and *.Designer.cs files.


$folder = "C:\repos\foo"

$viewFolders = Get-ChildItem -Path $folder -Include "Views" -Recurse -Directory

$views = @()

foreach ($viewFolder in $viewFolders)
{
	$views += Get-ChildItem -Path $viewFolder.FullName -Include "*.vb" -Exclude "*.Designer.vb" -Recurse
	$views += Get-ChildItem -Path $viewFolder.FullName -Include "*.cs" -Exclude "*.Designer.cs" -Recurse
}

foreach ($view in $views)
{
	Write-Host $view.FullName
}

Write-Host
Write-Host "Number of views: $($view.Length)"