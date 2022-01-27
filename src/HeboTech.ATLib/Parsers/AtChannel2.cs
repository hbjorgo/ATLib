using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Parsers
{
    public class AtChannel2
    {
        private static readonly string[] FinalResponseErrors = new string[]
        {
            "ERROR",
            "+CMS ERROR:",
            "+CME ERROR:",
            "NO CARRIER",
            "NO ANSWER",
            "NO DIALTONE"
        };

        private static readonly string[] FinalResponseSuccesses = new string[]
        {
            "OK",
            "CONNECT"
        };

        private static readonly string[] SmsUnsoliciteds = new string[]
        {
            "+CMT:",
            "+CDS:",
            "+CBM:"
        };

        public event EventHandler<UnsolicitedEventArgs> UnsolicitedEvent;

        private bool isDisposed;
        private readonly IAtReader atReader;
        private readonly IAtWriter atWriter;
        private readonly CancellationTokenSource cancellationTokenSource;
        private Task readerTask;
        private AutoResetEvent commandInProgress;

        private TimeSpan currentCommandTimeout;
        private AtCommandType commandType;
        private string responsePrefix;
        private string smsPdu;
        private AtResponse response;

        public AtChannel2(IAtReader atReader, IAtWriter atWriter)
        {
            this.atReader = atReader;
            this.atWriter = atWriter;
            cancellationTokenSource = new CancellationTokenSource();
            commandInProgress = new AutoResetEvent(false);
        }

        public void Start()
        {
            atReader.Start();
            readerTask = Task.Factory.StartNew(() => ReaderLoopAsync(cancellationTokenSource.Token), TaskCreationOptions.LongRunning);
        }

        public void Stop()
        {
            cancellationTokenSource.Cancel();
            atReader.Stop();
            Task.WaitAll(readerTask);
        }

        public bool IsBusy { get; private set; }

        /// <summary>
        /// Send command and get command status
        /// </summary>
        /// <param name="command"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public virtual Task<AtResponse> SendCommand(string command)
        {
            return SendFullCommandAsync(command, AtCommandType.NO_RESULT, null, null, TimeSpan.FromSeconds(5));
        }

        public virtual async Task<AtResponse> SendSingleLineCommandAsync(string command, string responsePrefix)
        {
            AtResponse response = await SendFullCommandAsync(command, AtCommandType.SINGELLINE, responsePrefix, null, TimeSpan.FromSeconds(10));

            if (response != null && response.Success && !response.Intermediates.Any())
            {
                // Successful command must have an intermediate response
                throw new InvalidResponseException("Did not get an intermediate response");
            }

            return response;
        }

        public virtual Task<AtResponse> SendMultilineCommand(string command, string responsePrefix)
        {
            AtCommandType commandType = responsePrefix == null ? AtCommandType.MULTILINE_NO_PREFIX : AtCommandType.MULTILINE;
            return SendFullCommandAsync(command, commandType, responsePrefix, null, TimeSpan.FromSeconds(5));
        }

        public virtual async Task<AtResponse> SendSmsAsync(string command, string pdu, string responsePrefix)
        {
            AtResponse response = await SendFullCommandAsync(command, AtCommandType.SINGELLINE, responsePrefix, pdu, TimeSpan.FromSeconds(5));

            if (response != null && response.Success && !response.Intermediates.Any())
            {
                // Successful command must have an intermediate response
                throw new InvalidResponseException("Did not get an intermediate response");
            }

            return response;
        }

        /// <summary>
        /// Not thread-safe.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="commandType"></param>
        /// <param name="responsePrefix"></param>
        /// <param name="smsPdu"></param>
        /// <param name="timeout"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<AtResponse> SendFullCommandAsync(string command, AtCommandType commandType, string responsePrefix, string smsPdu, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            IsBusy = true;

            this.currentCommandTimeout = timeout;
            this.commandType = commandType;
            this.responsePrefix = responsePrefix;
            this.smsPdu = smsPdu;
            this.response = new AtResponse();

            await atWriter.WriteLineAsync(command);

            if (!commandInProgress.WaitOne(currentCommandTimeout))
                throw new TimeoutException("Timed out while waiting for command response");

            AtResponse retVal = response;
            this.response = default;
            IsBusy = false;
            return retVal;
        }

        private async Task ReaderLoopAsync(CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                string line1 = await atReader.ReadAsync(cancellationToken);
                if (line1 == null)
                {
                    break;
                }
                if (line1 == string.Empty)
                {
                    continue;
                }

                if (IsSMSUnsolicited(line1))
                {
                    string line2 = await atReader.ReadAsync(cancellationToken);
                    if (line2 == null)
                    {
                        break;
                    }

                    HandleUnsolicited(line1, line2);
                }
                else
                {
                    ProcessMessage(line1);
                }
            }
        }

        private void ProcessMessage(string line)
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
                atWriter.WriteSmsPduAndCtrlZAsync(smsPdu);
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

        private void AddIntermediate(string line)
        {
            response.Intermediates.Add(line);
        }

        private static bool IsFinalResponseError(string line)
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
            commandInProgress.Set();
        }

        private static bool IsFinalResponseSuccess(string line)
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

        private static bool IsSMSUnsolicited(string line)
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

        #region Dispose
        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects)
                    Stop();
                    cancellationTokenSource.Dispose();
                    commandInProgress.Dispose();
                }

                // Free unmanaged resources (unmanaged objects) and override finalizer
                // Set large fields to null

                isDisposed = true;
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