# ATLib
![Continuous Integration](https://github.com/hbjorgo/ATLib/workflows/Continuous%20Integration/badge.svg?branch=master) [![NuGet Version](https://img.shields.io/nuget/vpre/HeboTech.ATLib.svg?style=flat)](https://www.nuget.org/packages/HeboTech.ATLib/)

ATLib is a C# AT command library that abstracts away the commands and makes it easy to communicate with modems.

Hayes command set (commonly known as AT commands) is a command set commonly used in modems. Read more about it at [Wikipedia](https://en.wikipedia.org/wiki/Hayes_command_set)

ATLib is just in the beginning. Currently only a few commands are implemented, but more will come.

## Usage
Install as NuGet package
```shell
dotnet add package HeboTech.ATLib
```

Using a serial port to communicate with a modem is easy:
```csharp
// Create and connect to serial port
SerialPort serialPort = new SerialPort(); //Use an overload to pass in your settings
serialPort.Open();

// Pipe the serial port stream through a duplex pipe
IDuplexPipe duplexPipe = StreamConnection.GetDuplex(serialPort.BaseStream); // See note below

// Pass in the pipe to the communicator and start communicating!
ICommunicator<string> comm = new Communicator(duplexPipe);

// Send a ping to the modem
var initializeResult = await comm.InitializeAsync();
// Get battery status
var batteryStatus = await comm.GetBatteryStatusAsync();
```

```
Note:
IDuplexPipe is an interface in System.IO.Pipelines.
Microsoft and ATLib doesn't come with an implementations for this, but Pipelines.Sockets.Unofficial has one that works great!
Install it with
dotnet add package Pipelines.Sockets.Unofficial
```
## Supported commands:

:heavy_check_mark: = Done   :wrench: = In progress
### V.25TER
| Command         | Numeric          |    Text          | Description | Notes |
|-----------------|:----------------:|:----------------:|-------------|-------|
| ATE[n]          |:heavy_check_mark:|:heavy_check_mark:|             |       |
| ATV[format]     |:heavy_check_mark:|:heavy_check_mark:|             |       |

### 3GPP TS 27.005

| Command         | Numeric          |    Text          | Description | Notes |
|-----------------|:----------------:|:----------------:|-------------|-------|
| AT+CMGF?        |:heavy_check_mark:|:heavy_check_mark:|             |       |
| AT+CMGF=[mode]  |:heavy_check_mark:|:heavy_check_mark:|             |       |
| AT+CMGS=[da]    |:wrench:          |:wrench:          |             |       |


### 3GPP TS 27.005

| Command         | Numeric          |    Text          | Description | Notes |
|-----------------|:----------------:|:----------------:|-------------|-------|
| AT+CBC          |:heavy_check_mark:|:heavy_check_mark:|             |       |
| AT+CSQ          |:heavy_check_mark:|:heavy_check_mark:|             |       |
| AT+CPIN?        |:heavy_check_mark:|:heavy_check_mark:|             |       |
