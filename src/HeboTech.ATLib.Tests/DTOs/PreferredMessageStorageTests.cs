using HeboTech.ATLib.Dtos;
using HeboTech.ATLib.DTOs;
using Xunit;

namespace HeboTech.ATLib.Tests.DTOs
{
    public class PreferredMessageStorageTests
    {
        [Theory]
        [InlineData("SM", 5, 10)]
        [InlineData("ME", 5, 10)]
        [InlineData("MT", 5, 10)]
        [InlineData("BM", 5, 10)]
        [InlineData("SR", 5, 10)]
        [InlineData("TA", 5, 10)]
        public void Test(string expectedStorageName, int expectedStorageMessages, int expectedStorageMessageLocations)
        {
            PreferredMessageStorage sut = new(MessageStorage.Parse(expectedStorageName), expectedStorageMessages, expectedStorageMessageLocations);

            Assert.Equal(expectedStorageName, sut.StorageName);
            Assert.Equal(expectedStorageMessages, sut.StorageMessages);
            Assert.Equal(expectedStorageMessageLocations, sut.StorageMessageLocations);
        }
    }
}
