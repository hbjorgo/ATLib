using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Parsers
{
    public abstract class AtChannel : IDisposable
    {
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
        private bool readerClosed;
        private readonly Task readerTask;

        public event EventHandler<UnsolicitedEventArgs> UnsolicitedEvent;

        private AtCommandType commandType;
        private string responsePrefix;
        private string smsPdu;
        private AtResponse response;
        private bool disposedValue;

        public AtChannel(CancellationToken cancellationToken = default)
        {
            readerTask = Task.Factory.StartNew(ReaderLoopAsync);
        }

        protected abstract ValueTask<string> ReadSingleMessageAsync(CancellationToken cancellationToken = default);
        protected abstract ValueTask<bool> Write(string text, CancellationToken cancellationToken = default);
        protected abstract ValueTask<bool> Write(char[] input, int offset, int count, CancellationToken cancellationToken = default);

        /// <summary>
        /// Send command and get command status
        /// </summary>
        /// <param name="command"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public virtual AtError SendCommand(string command)
        {
            AtError error = SendFullCommand(command, AtCommandType.NO_RESULT, null, null, TimeSpan.Zero, out _);
            return error;
        }

        public virtual AtError SendSingleLineCommand(string command, string responsePrefix, out AtResponse response)
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

        public virtual AtError SendMultilineCommand(string command, string responsePrefix, out AtResponse response)
        {
            AtCommandType commandType = responsePrefix == null ? AtCommandType.MULTILINE_NO_PREFIX : AtCommandType.MULTILINE;
            AtError error = SendFullCommand(command, commandType, responsePrefix, null, TimeSpan.Zero, out response);
            return error;
        }

        public virtual AtError SendSms(string command, string pdu, string responsePrefix, out AtResponse response)
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
        public virtual AtError SendFullCommand(string command, AtCommandType commandType, string responsePrefix, string smsPdu, TimeSpan timeout, out AtResponse response)
        {
            lock (lockObject)
            {
                return SendFullCommandNoLock(command, commandType, responsePrefix, smsPdu, timeout, out response);
            }
        }

        // TODO: Ref
        public virtual AtError SendFullCommandNoLock(string command, AtCommandType commandType, string responsePrefix, string smsPdu, TimeSpan timeout, out AtResponse outResponse)
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

        private AtError WriteLine(string command)
        {
            Write(command);
            Write("\r");

            return AtError.NO_ERROR;
        }

        private void ClearPendingCommand()
        {
            response = null;
            responsePrefix = null;
            smsPdu = null;
        }

        private async Task ReaderLoopAsync()
        {
            while (!readerClosed)
            {
                string line1 = await ReadSingleMessageAsync();
                if (line1 == null)
                {
                    break;
                }

                if (IsSMSUnsolicited(line1))
                {
                    string line2 = await ReadSingleMessageAsync();
                    if (line2 == null)
                    {
                        break;
                    }

                    HandleUnsolicited(line1, line2);
                }
                else
                {
                    ProcessLine(line1);
                }
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
                        case AtCommandType.MULTILINE_NO_PREFIX:
                            AddIntermediate(line);
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
            Write(smsPdu);
            Write("\x1A");
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

        private void HandleUnsolicited(string line1, string line2 = null)
        {
            UnsolicitedEvent?.Invoke(this, new UnsolicitedEventArgs(line1, line2));
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

        public void Close()
        {
            Dispose();
        }

        #region Dispose
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects)

                    if (!readerClosed)
                    {
                        lock (lockObject)
                        {
                            readerClosed = true;
                            Monitor.Pulse(lockObject);
                        }

                        var status = readerTask.Wait(TimeSpan.FromSeconds(5));
                    }
                }

                // Free unmanaged resources (unmanaged objects) and override finalizer
                // Set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~AtChannel()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
