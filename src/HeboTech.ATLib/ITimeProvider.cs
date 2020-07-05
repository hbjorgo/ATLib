using System;

namespace HeboTech.ATLib
{
    public interface ITimeProvider
    {
        DateTime UtcNow { get; }
    }
}