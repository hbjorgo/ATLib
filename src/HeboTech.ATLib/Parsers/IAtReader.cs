using System.Threading;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Parsers
{
    public interface IAtReader
    {
        ValueTask<string> ReadAsync(CancellationToken cancellationToken = default);
        void Open();
        void Close();
        /// <summary>
        /// Gets the current number of items available
        /// </summary>
        /// <returns></returns>
        int AvailableItems();
    }
}