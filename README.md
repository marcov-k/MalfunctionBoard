# MalfunctionBoard
Custom dashboard application for FRC team 9668 Malfunctionz capable of reading data from NetworkTables and displaying it via a 
.NET MAUI application built using C#.

## Installation
### Windows (x64)
1. Download the MalfunctionBoard-[version]-win .zip file from the most recent GitHub release
2. Extract the downloaded .zip file and run the MalfunctionBoardInstaller.bat batch file - this will move the application into
your drive's ProgramFiles directory and create a desktop shortcut.
### Mac (x64/ARM64)
1. Download the MalfunctionBoard-[version]-mac-[CPU architecture] .pkg file from the most recent GitHub release
(use x64 for Intel chips and ARM64 for Apple Silicon chips respectively)
3. Install the application via the downloaded .pkg installer

## Design and Functionality
MalfunctionBoard has been built to be as easily extensible as possible. Adding new types of displays simply requires 
extending one of the existing DashboardDisplay classes and defining how it should interpret and display the data it 
receives from FRC NetworkTables. The current set of display types is very limited but will be expanded once the core 
functionality of the dashboard is finalized. Contributions to the implementation of new display types is appreciated 
but not required.
