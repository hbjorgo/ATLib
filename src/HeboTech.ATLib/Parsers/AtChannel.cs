using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Parsers
{
    public class AtChannel : IAtChannel, IDisposable
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

        private bool debugEnabled;
        private Action<string> debugAction;

        private bool isDisposed;
        private IAtReader atReader;
        private IAtWriter atWriter;
        private CancellationTokenSource cancellationTokenSource;
        private Task readerTask;
        private SemaphoreSlim waitingForCommandResponse;

        private AtCommand currentCommand;
        private AtResponse currentResponse;

        public AtChannel(IAtReader atReader, IAtWriter atWriter)
        {
            this.atReader = atReader;
            this.atWriter = atWriter;
            cancellationTokenSource = new CancellationTokenSource();
            waitingForCommandResponse = new SemaphoreSlim(0, 1);
        }

        public TimeSpan DefaultCommandTimeout { get; set; } = TimeSpan.FromSeconds(5);

        public void Open()
        {
            atReader.Open();
            readerTask = ReaderLoopAsync(cancellationTokenSource.Token);
        }

        public void Close()
        {
            Dispose();
        }

        private bool IsDebugEnabled()
        {
            return debugEnabled;
        }

        public void EnableDebug(Action<string> debugAction)
        {
            this.debugAction = debugAction ?? throw new ArgumentNullException(nameof(debugAction));
            debugEnabled = true;
            debugAction($"##### DEBUG ENABLED #####");
        }

        public void DisableDebug()
        {
            debugEnabled = false;
            debugAction = default;
            debugAction($"##### DEBUG DISABLED #####");
        }

        /// <summary>
        /// Clears all available items
        /// </summary>
        /// <returns></returns>
        public async Task ClearAsync(CancellationToken cancellationToken = default)
        {
            for (int i = 0; i < atReader.AvailableItems(); i++)
            {
                await atReader.ReadAsync(cancellationToken);
            }
        }

        /// <summary>
        /// Send command and get command status
        /// </summary>
        /// <param name="command"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public virtual Task<AtResponse> SendCommand(string command, TimeSpan? timeout = null)
        {
            return SendFullCommandAsync(new AtCommand(AtCommandType.NO_RESULT, command, null, null, timeout ?? DefaultCommandTimeout));
        }

        public virtual async Task<AtResponse> SendSingleLineCommandAsync(string command, string responsePrefix, TimeSpan? timeout = null)
        {
            AtResponse response = await SendFullCommandAsync(new AtCommand(AtCommandType.SINGELLINE, command, responsePrefix, null, timeout ?? DefaultCommandTimeout));

            if (response != null && response.Success && !response.Intermediates.Any())
            {
                // Successful command must have an intermediate response
                throw new InvalidResponseException("Did not get an intermediate response");
            }

            return response;
        }

        public virtual Task<AtResponse> SendMultilineCommand(string command, string responsePrefix, TimeSpan? timeout = null)
        {
            AtCommandType commandType = responsePrefix == null ? AtCommandType.MULTILINE_NO_PREFIX : AtCommandType.MULTILINE;
            return SendFullCommandAsync(new AtCommand(commandType, command, responsePrefix, null, timeout ?? DefaultCommandTimeout));
        }

        public virtual async Task<AtResponse> SendSmsAsync(string command, string pdu, string responsePrefix, TimeSpan? timeout = null)
        {
            AtResponse response = await SendFullCommandAsync(new AtCommand(AtCommandType.SINGELLINE, command, responsePrefix, pdu, timeout ?? DefaultCommandTimeout));

            if (response != null && response.Success && !response.Intermediates.Any())
            {
                // Successful command must have an intermediate response
                throw new InvalidResponseException("Did not get an intermediate response");
            }

            return response;
        }

        /// <summary>
        /// Not re-entrant
        /// </summary>
        /// <param name="command"></param>
        /// <param name="commandType"></param>
        /// <param name="responsePrefix"></param>
        /// <param name="smsPdu"></param>
        /// <param name="timeout"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<AtResponse> SendFullCommandAsync(AtCommand command, CancellationToken cancellationToken = default)
        {
            try
            {
                this.currentCommand = command;
                this.currentResponse = new AtResponse();

                if (debugEnabled)
                    debugAction($"Out: {command.Command}");
                await atWriter.WriteLineAsync(command.Command);

                if (!await waitingForCommandResponse.WaitAsync(command.Timeout, cancellationToken))
                    throw new TimeoutException("Timed out while waiting for command response");

                return currentResponse;
            }
            finally
            {
                this.currentCommand = default;
                this.currentResponse = default;
            }
        }

        private async Task ReaderLoopAsync(CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                string line1;
                try
                {
                    line1 = await atReader.ReadAsync(cancellationToken);
                    if (debugEnabled)
                        debugAction($"In (line1): {line1}");
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                if (line1 == null)
                    break;
                if (line1 == string.Empty)
                    continue;
                if (IsSMSUnsolicited(line1))
                {
                    string line2;
                    try
                    {
                        line2 = await atReader.ReadAsync(cancellationToken);
                        if (debugEnabled)
                            debugAction($"In (line2): {line2}");
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    if (line2 == null)
                        break;
                    HandleUnsolicited(line1, line2);
                }
                else
                    ProcessMessage(line1);
            }
        }

        private void ProcessMessage(string line)
        {
            if (currentResponse == null)
            {
                HandleUnsolicited(line);
            }
            else if (IsFinalResponseSuccess(line))
            {
                currentResponse.Success = true;
                HandleFinalResponse(line);
            }
            else if (IsFinalResponseError(line))
            {
                currentResponse.Success = false;
                HandleFinalResponse(line);
            }
            else if (currentCommand.SmsPdu != null && line == "> ")
            {
                // See eg. TS 27.005 4.3
                // Commands like AT+CMGS have a "> " prompt
                if (debugEnabled)
                    debugAction($"Out: {currentCommand.SmsPdu}");
                atWriter.WriteSmsPduAndCtrlZAsync(currentCommand.SmsPdu);
                currentCommand.SmsPdu = null;
            }
            else
            {
                switch (currentCommand.CommandType)
                {
                    case AtCommandType.NO_RESULT:
                        HandleUnsolicited(line);
                        break;
                    case AtCommandType.NUMERIC:
                        if (!currentResponse.Intermediates.Any() && char.IsDigit(line[0]))
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
                        if (!currentResponse.Intermediates.Any() && line.StartsWith(currentCommand.ResponsePrefix))
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
                        if (line.StartsWith(currentCommand.ResponsePrefix))
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
            currentResponse.Intermediates.Add(line);
        }

        private void HandleFinalResponse(string line)
        {
            currentResponse.FinalResponse = line;
            waitingForCommandResponse.Release();
        }

        private void HandleUnsolicited(string line1, string line2 = null)
        {
            UnsolicitedEvent?.Invoke(this, new UnsolicitedEventArgs(line1, line2));
        }

        private static bool IsFinalResponseSuccess(string line)
        {
            return FinalResponseSuccesses.Any(response => line.StartsWith(response));
        }

        private static bool IsFinalResponseError(string line)
        {
            return FinalResponseErrors.Any(response => line.StartsWith(response));
        }

        private static bool IsSMSUnsolicited(string line)
        {
            return SmsUnsoliciteds.Any(response => line.StartsWith(response));
        }

        public static AtChannel Create(Stream stream)
        {
            return new AtChannel(new AtReader(stream), new AtWriter(stream));
        }

        public static AtChannel Create(Stream inputStream, Stream outputStream)
        {
            return new AtChannel(new AtReader(inputStream), new AtWriter(outputStream));
        }

        #region Dispose
        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    cancellationTokenSource.Cancel();
                    readerTask?.Wait();
                    readerTask?.Dispose();
                    readerTask = null;
                    atReader.Close();
                    atWriter.Close();
                    waitingForCommandResponse.Dispose();
                    waitingForCommandResponse = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
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