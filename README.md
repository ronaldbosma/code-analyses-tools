# Code Analyses Tools

These are some handy tools and scripts I've used in the past to perform code analyses.

>NOTE: The solutions mostly have hardcodes paths to folders and files, so you might need to change these.

## File Splitter

Can split a file in multiple smaller files. For instance it can split a large .sql file in smaller ones, splitting on the keyword `GO`.

## Solution Project Checker

Has features
1. Checking which project files in a certain folder are not linked in a specific solution file.
1. Extracting the target frameworks of each project in a folder (and subfolders), and writing the result to a CSV.