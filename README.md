# ATLib ![Continuous Integration](https://github.com/hbjorgo/ATLib/workflows/Continuous%20Integration/badge.svg?branch=master)
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
IDuplexPipe is an interface in System.IO.Pipelines. Microsoft and ATLib doesn't come with an implementations for this, but Pipelines.Sockets.Unofficial has one that works great!
```
Install it with
```shell
dotnet add package Pipelines.Sockets.Unofficial
```
