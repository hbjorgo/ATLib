using HeboTech.ATLib.Calls;
using System;
using Xunit;

namespace HeboTech.ATLib.Tests.Calls
{
    public class CallDetailsTests
    {
        [Fact]
        internal void Sets_properties()
        {
            CallDetails sut = new(TimeSpan.FromMinutes(7));

            Assert.Equal(TimeSpan.FromMinutes(7), sut.Duration);
        }
    }
}
