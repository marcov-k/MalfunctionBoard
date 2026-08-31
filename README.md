# MalfunctionBoard
Custom dashboard application for FRC team 9668 Malfunctionz capable of reading data from NetworkTables and displaying it via a 
.NET MAUI application built using C#.

## Installation
### Desktop Application
#### Windows (x64)
1. Download the MalfunctionBoard-[version]-win.exe installer file from the most recent GitHub release (named 'MalfunctionBoard vx.x.xx')
2. Run the downloaded installer and follow the prompts to install the application
#### Mac (x64/ARM64) (as of now these builds remain uncertified and unnotarized)
1. Download the MalfunctionBoard-[version]-mac-[CPU architecture] .pkg file from the most recent GitHub release (named 'MalfunctionBoard vx.x.xx')
(use x64 for Intel chips and ARM64 for Apple Silicon chips respectively)
3. Install the application via the downloaded .pkg installer
### WPILib Package
1. In Visual Studio Code use the Command Center to run WPILib: Manage Vendor Libraries > Install new libraries (online) and enter the following URL: https://raw.githubusercontent.com/marcov-k/MalfunctionBoard/main/wpilib-nt/MalfunctionBoardNT.json
2. In your Java code, import com.malfunctionz.malfunctionboard.nt.MalfunctionBoard and com.malfunctionz.malfunctionboard.nt.datatypes.*
3. Create a new instance of the MalfunctionBoard class with your target NetworkTable name and use its public methods to write data to the table as needed

## Design and Functionality
MalfunctionBoard has been built to be as easily extensible as possible. Adding new types of displays simply requires 
extending one of the existing DashboardDisplay classes and defining how it should interpret and display the data it 
receives from FRC NetworkTables. The current set of display types is very limited but will be expanded once the core 
functionality of the dashboard is finalized. Contributions to the implementation of new display types is appreciated 
but not required.
