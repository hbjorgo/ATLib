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

        private readonly object lockObject = new object();
        private readonly Task readerTask;
        private readonly PipeReader reader;
        private readonly Stream outputStream;
        private readonly CancellationToken cancellationToken;

        public event EventHandler<UnsolicitedEventArgs> UnsolicitedEvent;

        private AtCommandType commandType;
        private string responsePrefix;
        private string smsPdu;
        private AtResponse response;
        private bool disposedValue;

        public AtChannel(Stream stream, CancellationToken cancellationToken = default)
        {
            outputStream = stream;
            this.cancellationToken = cancellationToken;
            reader = PipeReader.Create(stream);
            readerTask = Task.Factory.StartNew(ReaderLoopAsync);
        }

        public AtChannel(Stream input, Stream output, CancellationToken cancellationToken = default)
        {
            outputStream = output;
            this.cancellationToken = cancellationToken;
            reader = PipeReader.Create(input);
            readerTask = Task.Factory.StartNew(ReaderLoopAsync);
        }

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

            while (response.FinalResponse == null && !cancellationToken.IsCancellationRequested)
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

            if (cancellationToken.IsCancellationRequested)
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
            while (!cancellationToken.IsCancellationRequested)
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
            Monitor.Pulse(lockObject);
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

        protected async ValueTask<string> ReadSingleMessageAsync(CancellationToken cancellationToken = default)
        {
            while (true)
            {
                ReadResult result = await reader.ReadAsync(cancellationToken);
                ReadOnlySequence<byte> buffer = result.Buffer;

                // In the event that no message is parsed successfully, mark consumed
                // as nothing and examined as the entire buffer.
                SequencePosition consumed = buffer.Start;
                SequencePosition examined = buffer.End;

                try
                {
                    if (TryReadLine(ref buffer, out string message))
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
                    if (result.IsCompleted)
                    {
                        if (buffer.Length > 0)
                        {
                            // The message is incomplete and there's no more data to process.
                            throw new InvalidDataException("Incomplete message.");
                        }

                        break;
                    }
                }
                finally
                {
                    reader.AdvanceTo(consumed, examined);
                }
            }

            return null;
        }

        private static bool TryReadLine(ref ReadOnlySequence<byte> buffer, out string line)
        {
            // Get the index of EOL and SMS prompt
            long eolIndex = FindIndexOf(buffer, eolSequence.AsSpan());
            long smsPromptIndex = FindIndexOf(buffer, smsPromptSequence.AsSpan());

            // Read the first occurence of either EOL or SMS prompt
            if ((eolIndex >= 0 && smsPromptIndex < 0) || (eolIndex >= 0 && eolIndex < smsPromptIndex))
                return ReadLine(ref buffer, out line);
            else if ((smsPromptIndex >= 0 && eolIndex < 0) || (smsPromptIndex >= 0 && smsPromptIndex < eolIndex))
                return ReadSmsPrompt(ref buffer, out line);
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

        private static bool ReadLine(ref ReadOnlySequence<byte> buffer, out string line)
        {
            SequenceReader<byte> sequenceReader = new SequenceReader<byte>(buffer);
            while (sequenceReader.TryReadTo(out ReadOnlySequence<byte> slice, eolSequence.AsSpan(), advancePastDelimiter: true))
            {
                string temp = Encoding.ASCII.GetString(slice.ToArray());
                buffer = buffer.Slice(sequenceReader.Position);
                if (!string.IsNullOrEmpty(temp))
                {
                    line = temp;
                    return true;
                }
            }
            line = default;
            return false;
        }

        private static bool ReadSmsPrompt(ref ReadOnlySequence<byte> buffer, out string line)
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

        protected Task Write(string text, CancellationToken cancellationToken = default)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            return outputStream.WriteAsync(buffer, 0, buffer.Length, cancellationToken);
        }

        #region Dispose
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects)

                    lock (lockObject)
                    {
                        Monitor.Pulse(lockObject);
                    }

                    readerTask.Wait(TimeSpan.FromSeconds(5));
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
