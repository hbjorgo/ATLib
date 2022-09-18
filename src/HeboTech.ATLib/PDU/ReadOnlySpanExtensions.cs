using System;

namespace HeboTech.ATLib.PDU
{
    internal static class ReadOnlySpanExtensions
    {
        public static ReadOnlySpan<T> SliceOnIndex<T>(this ReadOnlySpan<T> span, int start, int end)
        {
            return span.Slice(start, end - start);
        }
    }
}
