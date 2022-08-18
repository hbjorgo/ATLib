using System;

namespace HeboTech.ATLib.Helpers
{

    public static class CollectionsExtensions
    {
        public static T[] Sub<T>(this T[] self, int start, int end)
        {
            var length = end - start;
            T[] result = new T[length];
            Array.Copy(self, start, result, 0, length);
            return result;
        }
        public static ReadOnlySpan<T> Sub<T>(this ReadOnlySpan<T> self, int start, int end)
        {
            return self.Slice(start, end - start);
        }

        public static ReadOnlySpan<T> Sub<T>(this ReadOnlySpan<T> self, int start) => self.Sub(start, self.Length);
    }
    
}