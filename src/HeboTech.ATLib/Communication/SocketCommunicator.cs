using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Communication
{
    public class SocketCommunicator : ICommunicator
    {
        BinaryWriter bw;
        BinaryReader br;

        public SocketCommunicator()
        {
            Socket clientSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            clientSocket.Connect(new IPEndPoint(IPAddress.Loopback, 8888));

            NetworkStream ns = new NetworkStream(clientSocket);
            bw = new BinaryWriter(ns);
            br = new BinaryReader(ns);

            Write("Connected...");
        }

        public ValueTask<int> Read(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
        {
            int bytes = br.Read(buffer, offset, count);
            return new ValueTask<int>(bytes);
        }

        public ValueTask Write(string input, CancellationToken cancellationToken = default)
        {
            Console.WriteLine(input);
            return new ValueTask();
        }

        public ValueTask Write(byte[] input, int offset, int count, CancellationToken cancellationToken = default)
        {
            Console.WriteLine(input.Select(b => (char)b).ToArray(), offset, count);
            return new ValueTask();
        }
    }
}
