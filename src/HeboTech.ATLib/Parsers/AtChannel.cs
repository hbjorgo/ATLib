using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Parsers
{
    public class AtChannel : IDisposable
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

        private static readonly byte[] eolSequence = new byte[] { (byte)'\r', (byte)'\n' };
        private static readonly byte[] smsPromptSequence = new byte[] { (byte)'>', (byte)' ' };

        public event EventHandler<UnsolicitedEventArgs> UnsolicitedEvent;

        private readonly object lockObject = new object();
        private Task readerTask;
        private PipeReader reader;
        private Stream outputStream;
        private CancellationTokenSource internalCancellationTokenSource;
        private CancellationToken internalCancellationToken;
        private SemaphoreSlim semaphore;
        protected SemaphoreSlim readLoopStartSemaphore;
        private bool running;

        private AtCommandType commandType;
        private string responsePrefix;
        private string smsPdu;
        private AtResponse response;
        private bool disposedValue;

        public AtChannel(Stream stream)
            : this(stream, stream, null)
        {
        }

        public AtChannel(Stream inputStream, Stream outputStream)
            : this(inputStream, outputStream, null)
        {
        }

        protected AtChannel(Stream inputStream, Stream outputStream, SemaphoreSlim readLoopStartSemaphore)
        {
            if (inputStream == null)
                throw new ArgumentNullException(nameof(inputStream));
            if (outputStream == null)
                throw new ArgumentNullException(nameof(outputStream));

            if (!inputStream.CanRead)
                throw new ArgumentException($"{nameof(inputStream)} is closed");
            if (!outputStream.CanWrite)
                throw new ArgumentException($"{nameof(outputStream)} is closed");

            internalCancellationTokenSource = new CancellationTokenSource();
            internalCancellationToken = internalCancellationTokenSource.Token;
            semaphore = new SemaphoreSlim(0, 1);
            this.outputStream = outputStream;
            reader = PipeReader.Create(inputStream);
            this.readLoopStartSemaphore = readLoopStartSemaphore;

            running = true;
            readerTask = Task.Run(async () =>
            {
                if (readLoopStartSemaphore != null)
                    await readLoopStartSemaphore.WaitAsync(internalCancellationToken).ConfigureAwait(false);
                await ReaderLoopAsync(internalCancellationToken).ConfigureAwait(false);
            }, internalCancellationToken);
        }

        /// <summary>
        /// Send command and get command status
        /// </summary>
        /// <param name="command"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public virtual Task<(AtError error, AtResponse response)> SendCommand(string command)
        {
            return SendFullCommandAsync(command, AtCommandType.NO_RESULT, null, null, TimeSpan.Zero);
        }

        public virtual async Task<(AtError error, AtResponse response)> SendSingleLineCommandAsync(string command, string responsePrefix)
        {
            (AtError error, AtResponse response) = await SendFullCommandAsync(command, AtCommandType.SINGELLINE, responsePrefix, null, TimeSpan.Zero);

            if (error == AtError.NO_ERROR && response != null && response.Success && !response.Intermediates.Any())
            {
                // Successful command must have an intermediate response
                return (AtError.INVALID_RESPONSE, default);
            }

            return (error, response);
        }

        public virtual Task<(AtError error, AtResponse response)> SendMultilineCommand(string command, string responsePrefix)
        {
            AtCommandType commandType = responsePrefix == null ? AtCommandType.MULTILINE_NO_PREFIX : AtCommandType.MULTILINE;
            return SendFullCommandAsync(command, commandType, responsePrefix, null, TimeSpan.Zero);
        }

        public virtual async Task<(AtError error, AtResponse response)> SendSmsAsync(string command, string pdu, string responsePrefix)
        {
            (AtError error, AtResponse response) = await SendFullCommandAsync(command, AtCommandType.SINGELLINE, responsePrefix, pdu, TimeSpan.Zero);

            if (error == AtError.NO_ERROR && response != null && response.Success && !response.Intermediates.Any())
            {
                // Successful command must have an intermediate response
                return (AtError.INVALID_RESPONSE, default);
            }

            return (error, response);
        }

        // TODO: Ref
        public virtual async Task<(AtError error, AtResponse response)> SendFullCommandAsync(string command, AtCommandType commandType, string responsePrefix, string smsPdu, TimeSpan timeout)
        {
            lock (lockObject)
            {
                if (response != null)
                {
                    return OnError(AtError.COMMAND_PENDING);
                }

                this.commandType = commandType;
                this.responsePrefix = responsePrefix;
                this.smsPdu = smsPdu;
                this.response = new AtResponse();
            }

            AtError writeError = WriteLine(command);
            if (writeError != AtError.NO_ERROR)
            {
                return OnError(writeError);
            }

            try
            {
                if (!await semaphore.WaitAsync(10_000, internalCancellationToken))
                {
                    return OnError(AtError.TIMEOUT);
                }
            }
            catch (Exception ex) when (ex is OperationCanceledException || ex is ObjectDisposedException)
            {
                return OnError(AtError.CHANNEL_CLOSED);
            }

            // TODO: Ref

            AtResponse retVal = response;
            this.response = default;

            return (AtError.NO_ERROR, retVal);

            (AtError, AtResponse) OnError(AtError error)
            {
                response = default;
                responsePrefix = default;
                smsPdu = default;
                return (error, default);
            }
        }

        private AtError WriteLine(string command)
        {
            Write(command);
            Write("\r");

            return AtError.NO_ERROR;
        }

        private async Task ReaderLoopAsync(CancellationToken cancellationToken = default)
        {
            while (running)
            {
                if (cancellationToken.IsCancellationRequested)
                    cancellationToken.ThrowIfCancellationRequested();

                string line1 = await ReadSingleMessageAsync(cancellationToken);
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
                    string line2 = await ReadSingleMessageAsync(cancellationToken);
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

        private void AddIntermediate(string line)
        {
            response.Intermediates.Add(line);
        }

        private void WriteCtrlZ(string smsPdu)
        {
            Write(smsPdu);
            Write("\x1A");
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
            semaphore.Release();
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

        protected async Task<string> ReadSingleMessageAsync(CancellationToken cancellationToken = default)
        {
            while (running)
            {
                ReadResult result;
                try
                {
                    result = await reader.ReadAsync(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                if (!result.IsCanceled)
                {
                    ReadOnlySequence<byte> buffer = result.Buffer;

                    // In the event that no message is parsed successfully, mark consumed
                    // as nothing and examined as the entire buffer.
                    SequencePosition consumed = buffer.Start;
                    SequencePosition examined = buffer.End;

                    try
                    {
                        if (TryReadMessage(ref buffer, out string message))
                        {
                            // A single message was successfully parsed so mark the start as the
                            // parsed buffer as consumed. TryParseMessage trims the buffer to
                            // point to the data after the message was parsed.
                            consumed = buffer.Start;

                            // Examined is marked the same as consumed here, so the next call
                            // to ReadSingleMessageAsync will process the next message if there's
                            // one.
                            examined = consumed;

                            return message;
                        }

                        // There's no more data to be processed.
                        //if (result.IsCompleted)
                        //{
                        //    if (buffer.Length > 0)
                        //    {
                        //        // The message is incomplete and there's no more data to process.
                        //        throw new InvalidDataException("Incomplete message.");
                        //    }

                        //    break;
                        //}
                    }
                    finally
                    {
                        reader.AdvanceTo(consumed, examined);
                    }
                }
            }

            return null;
        }

        private static bool TryReadMessage(ref ReadOnlySequence<byte> buffer, out string line)
        {
            // Get the index of EOL and SMS prompt
            long eolIndex = FindIndexOf(buffer, eolSequence.AsSpan());
            long smsPromptIndex = FindIndexOf(buffer, smsPromptSequence.AsSpan());

            // Read the first occurence of either EOL or SMS prompt
            if ((eolIndex >= 0 && smsPromptIndex < 0) || (eolIndex >= 0 && eolIndex < smsPromptIndex))
                return TryReadLine(ref buffer, out line);
            else if ((smsPromptIndex >= 0 && eolIndex < 0) || (smsPromptIndex >= 0 && smsPromptIndex < eolIndex))
                return TryReadSmsPrompt(ref buffer, out line);
            else
            {
                line = default;
                return false;
            }
        }

        /// <summary>
        /// Get the index of the first occurence of the segment
        /// </summary>
        /// <param name="buffer">The buffer to search in</param>
        /// <param name="data">The segment to find</param>
        /// <returns>Returns the index of the segment, or -1 if not found</returns>
        private static long FindIndexOf(in ReadOnlySequence<byte> buffer, ReadOnlySpan<byte> data)
        {
            long position = 0;

            foreach (ReadOnlyMemory<byte> segment in buffer)
            {
                ReadOnlySpan<byte> span = segment.Span;
                var index = span.IndexOf(data);
                if (index != -1)
                {
                    return position + index;
                }

                position += span.Length;
            }

            return -1;
        }

        private static bool TryReadLine(ref ReadOnlySequence<byte> buffer, out string line)
        {
            SequenceReader<byte> sequenceReader = new SequenceReader<byte>(buffer);
            if (sequenceReader.TryReadTo(out ReadOnlySequence<byte> slice, eolSequence.AsSpan(), advancePastDelimiter: true))
            {
                string temp = Encoding.ASCII.GetString(slice.ToArray());
                buffer = buffer.Slice(sequenceReader.Position);
                line = temp;
                return true;
            }
            line = default;
            return false;
        }

        private static bool TryReadSmsPrompt(ref ReadOnlySequence<byte> buffer, out string line)
        {
            SequenceReader<byte> sequenceReader = new SequenceReader<byte>(buffer);
            if (sequenceReader.TryReadTo(out _, smsPromptSequence.AsSpan(), advancePastDelimiter: true))
            {
                line = Encoding.ASCII.GetString(smsPromptSequence);
                buffer = buffer.Slice(sequenceReader.Position);
                return true;
            }
            line = default;
            return false;
        }

        protected void Write(string text)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            outputStream.Write(buffer, 0, buffer.Length);
            outputStream.Flush();
        }

        #region Dispose
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects)
                    outputStream.Flush();
                    running = false;
                    reader.CancelPendingRead();
                    reader.Complete();
                    internalCancellationTokenSource.Cancel();
                    readerTask.Wait(TimeSpan.FromSeconds(1));

                    semaphore.Dispose();
                    readLoopStartSemaphore?.Dispose();
                    internalCancellationTokenSource.Dispose();
                    reader = null;
                    outputStream = null;
                    semaphore = null;
                    readLoopStartSemaphore = null;
                    internalCancellationTokenSource = null;
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
