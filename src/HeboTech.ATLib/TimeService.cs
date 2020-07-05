using System;

namespace HeboTech.ATLib
{
    public static class TimeService
    {
        private static ITimeProvider s_provider;

        public static DateTime UtcNow => s_provider.UtcNow;

        public static void SetProvider(ITimeProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            if (provider.UtcNow.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException("Time must be in UTC format");
            }

            s_provider = provider;
        }
    }
}