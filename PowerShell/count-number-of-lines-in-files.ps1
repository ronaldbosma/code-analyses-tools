# With the following script you can get the number of files and total number of lines in the files for a folder (including subfolders).
# Empty lines and lines with a { or } are ignored.
# Set the correct path in the $folder variable and include any file extensions you want to analyse in $extensionsToInclude.

$folder = "<path-to-the-folder>"

$extensionsToInclude = @(
    ".cs",
    ".cshtml",
    ".css",
    ".htm",
    ".html"
    ".js",
    ".razor"
    ".proto",
    ".ps1",
    ".psm1",
    ".sql",
    ".ts"
)


[long]$totalNumberOfFiles = 0
[long]$totalNumberOfLines = 0


# Get the files
$files = Get-ChildItem -Path $folder -Recurse -File


# Create dictionary to track numbers per file type
$fileTypes = $files | Select-Object Extension -Unique | Sort-Object Extension
$numbersPerFileType = @{}
foreach ($fileType in $fileTypes)
{
    $numbersPerFileType[$fileType.Extension]= @{ NumberOfFiles = 0; NumberOfLines = 0 }
}


# Get the numbers per file
foreach ($file in $files)
{
    $filePath = $file.FullName
        
    if ($extensionsToInclude -contains $file.Extension)
    {
        # Get file content but exclude empty lines and lines with a { or }
        $content = Get-Content $filePath | Where-Object { -not([string]::IsNullOrEmpty($_)) -and -not($_ -like "*{*") -and -not($_ -like "*}*") }
        
        $numberOfLines = $content.Length
        
        $numbersPerFileType[$file.Extension].NumberOfFiles++;
        $numbersPerFileType[$file.Extension].NumberOfLines += $numberOfLines;
        
        $totalNumberOfFiles++
        $totalNumberOfLines += $numberOfLines
        
        Write-Host "$filePath, $numberOfLines"
    }
    else
    {
        Write-Host "$filePath is ignored"
    }
}


# Write the numbers per file type
Write-Host
foreach ($key in $numbersPerFileType.Keys)
{
    $fileType = $numbersPerFileType[$key]
    if ($fileType.NumberOfFiles -gt 0)
    {
        Write-Host "FileType = $key;  NumberOfFiles = $($fileType.NumberOfFiles);  NumberOfLines = $($fileType.NumberOfLines)"
    }
}


# Write the total numbers
Write-Host
Write-Host "Total number of files: $totalNumberOfFiles"
Write-Host "Total number of lines: $totalNumberOfLines"