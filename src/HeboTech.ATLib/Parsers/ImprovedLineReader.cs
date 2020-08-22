using HeboTech.ATLib.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HeboTech.ATLib.Parsers
{
    public class ImprovedLineReader
    {
        private readonly ICommunicator comm;
        private const int maxAtResponse = 8 * 1024;
        private readonly char[] buffer = new char[maxAtResponse];
        private int headIndex = 0;
        private int tailIndex = -1;

        public ImprovedLineReader(ICommunicator comm)
        {
            this.comm = comm;
        }

        public string ReadLine()
        {
            int eolIndex = -1;

            if (buffer[headIndex] == '\0')
            {
                headIndex = 0;
                buffer[headIndex] = '\0';
                tailIndex = 0;
            }
            else
            {
                // There is data in the buffer from the last read

                // Skip over leading newlines
                while (buffer[headIndex] == '\r' || buffer[headIndex] == '\n')
                    headIndex++;

                eolIndex = FindNextEOL(buffer, headIndex);

                if (eolIndex < 0)
                {
                    int len = strlen(buffer, headIndex);
                    memmove(buffer, 0, buffer, headIndex, len);
                    tailIndex = len;
                    headIndex = 0;
                }
            }

            while (eolIndex < 0)
            {
                if (maxAtResponse - (tailIndex - headIndex) == 0)
                {
                    headIndex = 0;
                    buffer[headIndex] = '\0';
                    tailIndex = 0;
                }

                int count;
                do
                {
                    try
                    {
                        count = comm.Read(buffer, tailIndex, buffer.Length - tailIndex - 1).GetAwaiter().GetResult();
                    }
                    catch (OperationCanceledException oce)
                    {
                        return null;
                    }
                } while (count < 0);

                if (count > 0)
                {
                    buffer[count] = '\0';

                    // Skip over leading newlines
                    while (buffer[headIndex] == '\r' || buffer[headIndex] == '\n')
                        headIndex++;

                    eolIndex = FindNextEOL(buffer, headIndex);
                    tailIndex += count;
                }
                else if (count <= 0)
                {
                    // Read error encountered or EOF reached
                    // count == 0 => EOF reached
                    // count < 0 => Read error
                    return null;
                }
            }

            buffer[eolIndex] = '\0';
            string line = new string(buffer, headIndex, eolIndex - headIndex);
            headIndex = eolIndex + 1;
            return line;
        }

        private int FindNextEOL(char[] input, int startIndex)
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
                return -1;
            }
            else
            {
                return i;
            }
        }

        private int strlen(char[] input, int startIndex)
        {
            for (int i = startIndex; i < input.Length; i++)
            {
                if (input[i] == 0)
                    return i - startIndex;
            }

            return input.Length - startIndex;
        }

        private void memmove(char[] destination, int destOffset, char[] source, int sourceOffset, int count)
        {
            for (int i = sourceOffset; i < count; i++)
            {
                destination[destOffset + i] = source[sourceOffset + i];
            }
        }
    }
}
