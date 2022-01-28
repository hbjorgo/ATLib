using System.Threading;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Parsers
{
    public interface IAtReader
    {
        ValueTask<string> ReadAsync(CancellationToken cancellationToken = default);
        void Open();
        void Close();
    }
}