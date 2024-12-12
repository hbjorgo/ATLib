using System.Threading;
using System.Threading.Tasks;

namespace HeboTech.ATLib.Parsing
{
    internal interface IAtReader
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