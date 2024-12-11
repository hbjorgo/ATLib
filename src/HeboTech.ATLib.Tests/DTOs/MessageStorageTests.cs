using HeboTech.ATLib.Dtos;
using System;
using Xunit;

namespace HeboTech.ATLib.Tests.DTOs
{
    public class MessageStorageTests
    {
        [Theory]
        [InlineData("SM")]
        [InlineData("ME")]
        [InlineData("MT")]
        [InlineData("BM")]
        [InlineData("SR")]
        [InlineData("TA")]
        public void TProperties_are_set(string storageName)
        {
            MessageStorage storage = MessageStorage.Parse(storageName);

            Assert.Equal(storageName, storage.Value);
            Assert.Equal(storageName, storage);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("A")]
        [InlineData("AB")]
        public void Throws_on_invalid_name(string storageName)
        {
            Assert.Throws<ArgumentException>(() => MessageStorage.Parse(storageName));
        }
    }
}
