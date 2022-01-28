using System;
using System.Collections.Generic;
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

        private bool isDisposed;
        private IAtReader atReader;
        private IAtWriter atWriter;
        private CancellationTokenSource cancellationTokenSource;
        private Task readerTask;
        private SemaphoreSlim waitingForCommandResponse;

        private AtCommand currentCommand;
        private InternalResponse currentResponse;

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
            readerTask = Task.Factory.StartNew(() => ReaderLoopAsync(cancellationTokenSource.Token), TaskCreationOptions.LongRunning);
        }

        public void Close()
        {
            Dispose();
        }

        public virtual async Task<AtResponse> SendCommand(string command, TimeSpan? timeout = null)
        {
            InternalResponse internalResponse = await SendFullCommandAsync(new AtCommand(AtCommandType.NO_RESULT, command, null, null, timeout ?? DefaultCommandTimeout));
            return new AtResponse(internalResponse.Success, internalResponse.FinalResponse);
        }

        public virtual async Task<AtSingleLineResponse> SendSingleLineCommandAsync(string command, string responsePrefix, TimeSpan? timeout = null)
        {
            InternalResponse internalResponse = await SendFullCommandAsync(new AtCommand(AtCommandType.SINGELLINE, command, responsePrefix, null, timeout ?? DefaultCommandTimeout));
            return new AtSingleLineResponse(internalResponse.Success, internalResponse.FinalResponse, internalResponse.Intermediates.First());
        }

        public virtual async Task<AtMultiLineResponse> SendMultilineCommand(string command, string responsePrefix, TimeSpan? timeout = null)
        {
            AtCommandType commandType = responsePrefix == null ? AtCommandType.MULTILINE_NO_PREFIX : AtCommandType.MULTILINE;
            InternalResponse internalResponse = await SendFullCommandAsync(new AtCommand(commandType, command, responsePrefix, null, timeout ?? DefaultCommandTimeout));
            return new AtMultiLineResponse(internalResponse.Success, internalResponse.FinalResponse, internalResponse.Intermediates);
        }

        public virtual async Task<AtMultiLineResponse> SendSmsAsync(string command, string pdu, string responsePrefix, TimeSpan? timeout = null)
        {
            InternalResponse internalResponse = await SendFullCommandAsync(new AtCommand(AtCommandType.SINGELLINE, command, responsePrefix, pdu, timeout ?? DefaultCommandTimeout));
            return new AtMultiLineResponse(internalResponse.Success, internalResponse.FinalResponse, internalResponse.Intermediates);
        }

        private async Task<InternalResponse> SendFullCommandAsync(AtCommand command, CancellationToken cancellationToken = default)
        {
            try
            {
                this.currentCommand = command;
                this.currentResponse = new InternalResponse();

                await atWriter.WriteLineAsync(command.Command);

                if (!await waitingForCommandResponse.WaitAsync(command.Timeout, cancellationToken))
                    throw new TimeoutException("Timed out while waiting for command response");

                if ((command.CommandType == AtCommandType.SINGELLINE || command.CommandType == AtCommandType.MULTILINE || command.CommandType == AtCommandType.MULTILINE_NO_PREFIX)
                    && currentResponse.Success
                    && !currentResponse.Intermediates.Any())
                {
                    // Successful command must have an intermediate response
                    throw new InvalidResponseException("Did not get an intermediate response");
                }

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

        private class InternalResponse
        {
            public bool Success { get; set; }
            public string FinalResponse { get; set; }
            public List<string> Intermediates { get; set; } = new List<string>();

            public override string ToString()
            {
                return $"Success: {Success}, FinalResponse: {FinalResponse}, Intermediates: {Intermediates.Count}";
            }
        }
    }
}