# DLL-Version-Tracker
A WPF C# Application to track a .NET project's libraries, use the average values in the version sequence to determine which libraries are not up to date. (Excluding third-party libraries)

## Scenario
The Deploy process contains a Build procedure that runs on a solution. After running the procedure it was discovered that not all the projects were rebuilt successfully.
The version numbers of the DLL files and/or .EXE files of the newly rebuilt projects have been updated, however,  the projects’ that were not rebuilt correctly output version numbers have not been updated correctly.
One of the problems encountered is that the Release Build will reference and package all the DLL’s, of which some are invalid.
What makes this harder to track is that there are third party libraries in the release folder, which can have any version number and should not be directly controlled by the developers – these should be excluded.

## Task
Write a WPF application to 
1. check a specified directory for libraries 
and 
2. track their DLL versions.
List any outliers and cater for exclusions. 
3. Use the average values in the version sequence to determine which libraries are not up to date.

4. Display the results, focussing on the erroneous, potentially outdated libraries.

Ensure the app is user friendly – UX, error handling, etc.

Preferred environment:
IDE: Visual Studio
Language: C#
