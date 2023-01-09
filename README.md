# ChristianRaun-HMIDevelopment
To develop a desktop HMI software for client that runs on Windows 10 or above and is capable to handle / process EtherCAT data
# Introduction
 
# Testing
# Architecture
 ![](img/r6.png)
# Requirements
Works with  
```
PROGRAM MAIN 
VAR CONSTANT 
deviceCount : WORD := 2; 
END_VAR 
VAR 
{attribute 'TcLinkTo' := ''}device : ARRAY [1..deviceCount] OF EthercatDevice ; 
counter: WORD; 
END_VAR

FOR counter:=1 TO deviceCount BY 1 DO
device[counter].dummyDataOut:= SHL(device[counter].dir,2*8) OR device[counter].speed ;
END_FOR;
```
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
4. Click "Run" button
5. Click on "Machine Setting" tab
 
# Open Issues
https://github.com/eSpark-Consultants/flexmold/issues/11

# Issue Results
 
 <!-- ![](img/Issue10d.gif)
 ![](img/R1.png)
 ![](img/R2.png)
 ![](img/R3.png)
 ![](img/R4.png) -->
## External Thanks
- [For NLogViewer] https://github.com/erizet/NlogViewer
- [For WPF Panel Control Template] https://www.youtube.com/watch?v=PzP8mw7JUzI&t=1s&ab_channel=Payload


