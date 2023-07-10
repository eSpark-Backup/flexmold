# ChristianRaun-HMIDevelopment
To develop a desktop HMI software for client that runs on Windows 10 or above and is capable to handle / process EtherCAT data
# Introduction
 
# Testing
# Architecture
 ![](img/r6.png)
# Installation
Executable can be downloaded from [FCA](./Binaries/FlexMoldV1.0.0.8.rar)
## Prerequisites
- TwincCAT 3.1 
- ![](img/13a.png)
- .Net Runtime 6 (if required)
#### How To Run 
1. Run Application (Flexmold.exe)
2. Select Machine
3. Build TwinCAT Solution and wait
4. Click the "Run" button
5. Click on the "Machine Setting" tab

#### How To Deploy 
1. Build then zip FCA App named : [major#].[minor#].[Issue#].[Issueminor#] eg FCA 1.0.0.4.zip  
2. Install [TwinCat](https://www.beckhoff.com/en-en/support/download-finder/software-and-tools/#)
3. Copy and extract FCA to the deployment machine
4. Open Twincat and generate a new Twincat Solution
5. Add Twincat Project(xml)
6. Scan Hardware (optional)
7. Goto Twincat file menu --> Show Realtime Devices --> Install eth devices
8. Build Project
9. Generate a Trial license when asked
10. run C:\TwinCAT\3.1\System\win8settick.bat with admin previlges
11. save and restart pc
12. Run the FCA app  
# Open Issues

# Issue Results
 ![](img/19.gif)

## External Thanks
- [For NLogViewer] https://github.com/erizet/NlogViewer
- [For WPF Panel Control Template] https://www.youtube.com/watch?v=PzP8mw7JUzI&t=1s&ab_channel=Payload


