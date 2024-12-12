using System.Threading;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Parsing
{
    internal interface IAtWriter
    {
        Task WriteLineAsync(string command, CancellationToken cancellationToken = default);
        Task WriteSmsPduAndCtrlZAsync(string smsPdu, CancellationToken cancellationToken = default);
        void Close();
    }
}