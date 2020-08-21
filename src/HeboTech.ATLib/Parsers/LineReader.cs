using HeboTech.ATLib.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HeboTech.ATLib.Parsers
{
    public class LineReader
    {
        private readonly ICommunicator comm;
        private const int maxAtResponse = 8 * 1024;
        private byte[] atBuffer = new byte[maxAtResponse];
        private int atBufferIndex = 0;

        public LineReader(ICommunicator comm)
        {
            this.comm = comm;
        }

        public string ReadLine()
        {
            int readIndex = -1;
            int eolIndex = -1;

            if (atBuffer[atBufferIndex] == '\0')
            {
                // Empty buffer
                atBufferIndex = 0;
                atBuffer[atBufferIndex] = (byte)'\0';
                readIndex = 0;
            }
            else
            {
                // There's data in the buffer from the last read

                // Skip over leading newlines
                while (atBuffer[atBufferIndex] == '\r' || atBuffer[atBufferIndex] == '\n')
                    atBufferIndex++;

                eolIndex = FindNextEOL(atBuffer, atBufferIndex);
                if (eolIndex < 0)
                {
                    // A partial line. Move it up and prepare to read more
                    int terminationIndex = (new string(atBuffer.Select(b => (char)b).ToArray(), atBufferIndex, atBuffer.Length - atBufferIndex)).IndexOf('\0');
                    Array.Copy(atBuffer, atBufferIndex, atBuffer, 0, terminationIndex - atBufferIndex);
                    readIndex = atBufferIndex;
                    atBufferIndex = 0;
                }
                // Otherwise, there is a complete line that will be returned the while() loop below
            }

            while (eolIndex < 0)
            {
                if (maxAtResponse - readIndex == 0)
                {
                    Console.WriteLine("Input line exceeded buffer");
                    // Ditch buffer and start over again
                    atBufferIndex = 0;
                    atBuffer[atBufferIndex] = (byte)'\0';
                    readIndex = 0;
                }

                int count;
                do
                {
                    count = comm.Read(atBuffer, readIndex, maxAtResponse - (readIndex - atBufferIndex)).GetAwaiter().GetResult();
                } while (count < 0);

                if (count > 0)
                {
                    atBuffer[readIndex + count] = (byte)'\0';

                    // Skip over leading newlines
                    while (atBuffer[atBufferIndex] == '\r' || atBuffer[atBufferIndex] == '\n')
                        atBufferIndex++;

                    eolIndex = FindNextEOL(atBuffer, atBufferIndex);
                    readIndex += count;
                }
                else if (count <= 0)
                {
                    // Read error encountered or EOF reached
                    if (count == 0)
                        Console.WriteLine("EOF reached");
                    else
                        Console.WriteLine("Read error");
                    return null;
                }
            }

            // A full line in the buffer. Place a \0 over the \r and return
            int ret = atBufferIndex;
            atBuffer[eolIndex] = (byte)'\0';
            atBufferIndex = eolIndex + 1; // This will always be <= readIndex, and there will be a \0 at readIndex

            return Encoding.UTF8.GetString(atBuffer, ret, eolIndex - ret);
        }

        private int FindNextEOL(byte[] input, int startIndex)
        {
            if (input[startIndex] == '>' && input[startIndex + 1] == ' ' && input[startIndex + 2] == '\0')
            {
                // SMS prompt character... not \r terminated
                return startIndex + 2;
            }

            // Find next newline
            int i = startIndex;
            while (input[i] != '\0' && input[i] != '\r' && input[i] != '\n')
            {
                i++;
            }

            if (input[i] == '\0')
            {
                return -1; // Result is switched from ref. impl.
            }
            else
            {
                return i;
            }
        }
    }
}
