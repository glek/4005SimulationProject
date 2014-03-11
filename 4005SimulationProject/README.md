# SYSC 4005/5001 - Winter 2014 Term Project #

## Requirements ##

* .NET Framework 4.0 or Mono 4.0
* Mono Build Platform Toolset or Microsft Build Platform Toolset

## Optional Requirements ##

* MonoDevelop or Xamarin Studio or Microsoft Visual Studio (C#)

## Compiling ##

If you have Visual Studio/MonoDevelop/Xamarin Studio, skip to step 2.

1. Download your desired IDE
2. Load 4005SimulationProject.csproj
3. Perform a clean and build

## Running ##

Double clicking 4005SimulationProject (located in the bin\<Configuration> folder) will
run the program with the default settings (loading simulation.xml, full event logging).

To run the program with different settings, run it from the command line.

1. Open your terminal/shell/command prompt
2. Navigate to the bin\<Configuration> folder
3. Execute 4005SimulationProject.exe with the desired command line arguments

Valid command line usage:
4005SimulationProject [-noeventlogging] [simulationfile]
-noeventlogger: Disables full event logging
simulationfile: Optional parameter of the simulation definition file to load, defaults to simulation.xml