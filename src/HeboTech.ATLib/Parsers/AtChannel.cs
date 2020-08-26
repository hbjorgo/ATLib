using HeboTech.ATLib.Communication;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Parsers
{
    public class AtChannel
    {
        public enum AtCommandType
        {
            NO_RESULT, // No intermediate response expected
            NUMERIC, // A single intermediate response starting with a 0-9
            SINGELLINE, // A single intermediate response starting with a prefix
            MULTILINE // Multiple line intermediate response starting with a prefix
        }

        public enum AtError
        {
            NO_ERROR,
            INVALID_RESPONSE,
            COMMAND_PENDING,
            TIMEOUT,
            CHANNEL_CLOSED
        }

        private readonly string[] FinalResponseErrors = new string[]
        {
            "ERROR",
            "+CMS ERROR:",
            "+CME ERROR:",
            "NO CARRIER",
            "NO ANSWER",
            "NO DIALTONE"
        };

        private readonly string[] FinalResponseSuccesses = new string[]
        {
            "OK",
            "CONNECT"
        };

        private readonly string[] SmsUnsoliciteds = new string[]
        {
            "+CMT:",
            "+CDS:",
            "+CBM:"
        };

        private readonly object lockObject = new object();
        private readonly ICommunicator comm;
        private readonly AtLineReader lineReader;
        private readonly Thread readerThread;
        private bool readerClosed;

        public Action<string, string> UnsolicitedHandler { get; set; }

        private AtCommandType commandType;
        private string responsePrefix;
        private string smsPdu;
        private AtResponse response;

        public AtChannel(ICommunicator comm)
        {
            this.comm = comm;
            this.lineReader = new AtLineReader(comm);

            readerThread = new Thread(new ThreadStart(ReaderLoop));
            readerThread.Start();
        }

        public AtError SendCommand(string command, out AtResponse response)
        {
            AtError error = SendFullCommand(command, AtCommandType.NO_RESULT, null, null, TimeSpan.Zero, out response);
            return error;
        }

        public AtError SendSingleLineCommand(string command, string responsePrefix, out AtResponse response)
        {
            AtError error = SendFullCommand(command, AtCommandType.SINGELLINE, responsePrefix, null, TimeSpan.Zero, out response);

            if (error == AtError.NO_ERROR && response != null && response.Success && !response.Intermediates.Any())
            {
                // Successful command must have an intermediate response
                response = null;
                return AtError.INVALID_RESPONSE;
            }

            return error;
        }

        public AtError SendMultilineCommand(string command, string responsePrefix, out AtResponse response)
        {
            AtError error = SendFullCommand(command, AtCommandType.MULTILINE, responsePrefix, null, TimeSpan.Zero, out response);
            return error;
        }

        public AtError SendSms(string command, string pdu, string responsePrefix, out AtResponse response)
        {
            var error = SendFullCommand(command, AtCommandType.SINGELLINE, responsePrefix, pdu, TimeSpan.Zero, out response);

            if (error == AtError.NO_ERROR && response != null && response.Success && !response.Intermediates.Any())
            {
                // Successful command must have an intermediate response
                response = null;
                return AtError.INVALID_RESPONSE;
            }

            return error;
        }

        // TODO: Ref
        public AtError SendFullCommand(string command, AtCommandType commandType, string responsePrefix, string smsPdu, TimeSpan timeout, out AtResponse response)
        {
            lock (lockObject)
            {
                return SendFullCommandNoLock(command, commandType, responsePrefix, smsPdu, timeout, out response);
            }
        }

        // TODO: Ref
        public AtError SendFullCommandNoLock(string command, AtCommandType commandType, string responsePrefix, string smsPdu, TimeSpan timeout, out AtResponse outResponse)
        {
            if (response != null)
            {
                outResponse = null;
                return OnError(AtError.COMMAND_PENDING);
            }

            AtError writeError = WriteLine(command);
            if (writeError != AtError.NO_ERROR)
            {
                outResponse = null;
                return OnError(writeError);
            }

            this.commandType = commandType;
            this.responsePrefix = responsePrefix;
            this.smsPdu = smsPdu;
            this.response = new AtResponse();

            while (response.FinalResponse == null && !readerClosed)
            {
                if (!Monitor.Wait(lockObject, TimeSpan.FromSeconds(600)))
                {
                    outResponse = null;
                    return OnError(AtError.TIMEOUT);
                }
            }

            // TODO: Ref

            outResponse = response;
            response = null;

            if (readerClosed)
            {
                ClearPendingCommand();
                return AtError.CHANNEL_CLOSED;
            }

            return AtError.NO_ERROR;

            AtError OnError(AtError error)
            {
                ClearPendingCommand();
                return error;
            }
        }

        private ValueTask<AtError> ReadLine(CancellationToken token)
        {
            throw new NotImplementedException();
        }

        private AtError WriteLine(string command)
        {
            comm.Write(command);
            comm.Write("\r");

            return AtError.NO_ERROR;
        }

        private void ClearPendingCommand()
        {
            response = null;
            responsePrefix = null;
            smsPdu = null;
        }

        private void ReaderLoop()
        {
            while (!readerClosed)
            {
                string line1 = lineReader.ReadLine();
                if (line1 == null)
                {
                    break;
                }

                if (IsSMSUnsolicited(line1))
                {
                    string line2 = lineReader.ReadLine();
                    if (line2 == null)
                    {
                        break;
                    }

                    UnsolicitedHandler?.Invoke(line1, line2);
                }
                else
                {
                    ProcessLine(line1);
                }

                Thread.Yield();
            }
        }

        private void ProcessLine(string line)
        {
            lock (lockObject)
            {
                if (response == null)
                {
                    HandleUnsolicited(line);
                }
                else if (IsFinalResponseSuccess(line))
                {
                    response.Success = true;
                    HandleFinalResponse(line);
                }
                else if (IsFinalResponseError(line))
                {
                    response.Success = false;
                    HandleFinalResponse(line);
                }
                else if (smsPdu != null && line == "> ")
                {
                    // See eg. TS 27.005 4.3
                    // Commands like AT+CMGS have a "> " prompt
                    WriteCtrlZ(smsPdu);
                    smsPdu = null;
                }
                else
                {
                    switch (commandType)
                    {
                        case AtCommandType.NO_RESULT:
                            HandleUnsolicited(line);
                            break;
                        case AtCommandType.NUMERIC:
                            if (!response.Intermediates.Any() && char.IsDigit(line[0]))
                            {
                                AddIntermediate(line);
                            }
                            else
                            {
                                // Either we already have an intermediate response or the line doesn't begin with a digit
                                HandleUnsolicited(line);
                            }
                            break;
                        case AtCommandType.SINGELLINE:
                            if (!response.Intermediates.Any() && line.StartsWith(responsePrefix))
                            {
                                AddIntermediate(line);
                            }
                            else
                            {
                                // We already have an intermediate response
                                HandleUnsolicited(line);
                            }
                            break;
                        case AtCommandType.MULTILINE:
                            if (line.StartsWith(responsePrefix))
                            {
                                AddIntermediate(line);
                            }
                            else
                            {
                                HandleUnsolicited(line);
                            }
                            break;
                        default:
                            // This should never be reached
                            //TODO: Log error or something
                            HandleUnsolicited(line);
                            break;
                    }
                }
            }
        }

        private void AddIntermediate(string line)
        {
            response.Intermediates.Add(line);
        }

        private void WriteCtrlZ(string smsPdu)
        {
            comm.Write(smsPdu);
            comm.Write("\x1A");
        }

        private bool IsFinalResponseError(string line)
        {
            foreach (string response in FinalResponseErrors)
            {
                if (line.StartsWith(response))
                {
                    return true;
                }
            }
            return false;
        }

        private void HandleFinalResponse(string line)
        {
            response.FinalResponse = line;
            Monitor.Pulse(lockObject);
        }

        private bool IsFinalResponseSuccess(string line)
        {
            foreach (string response in FinalResponseSuccesses)
            {
                if (line.StartsWith(response))
                {
                    return true;
                }
            }
            return false;
        }

        private void HandleUnsolicited(string line)
        {
            UnsolicitedHandler?.Invoke(line, null);
        }

        private bool IsSMSUnsolicited(string line)
        {
            foreach (var response in SmsUnsoliciteds)
            {
                if (line.StartsWith(response))
                {
                    return true;
                }
            }
            return false;
        }

        public void Start()
        {

        }

        public void Close()
        {
            if (!readerClosed)
            {
                lock (lockObject)
                {
                    lineReader.Close();
                    readerClosed = true;
                    Monitor.Pulse(lockObject);
                }

                var status = readerThread.Join(TimeSpan.FromSeconds(5));
            }
        }
    }
}
