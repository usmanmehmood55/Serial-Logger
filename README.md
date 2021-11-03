# Serial Logger

### Purpose
Logs serial data from available COM ports for specified time, and writes them into a .csv file.

### Requirements
* For building from source, .NET Core 3.1 SDK
* For running the build, hopefully nothing, as it is self-contained

### Features
* Can automatically detect available COM ports
* Can log multiple values in single line, when seperated by commas
* Always places log file on Desktop for easy access

### To Do
* Add automatic seperator detection for multiple values in a single line
* Add option for timestamps
* Maybe even a serial plotter as good as Arduino IDE's
* Turn into a WPF app