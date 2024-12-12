using HeboTech.ATLib.Messaging;
using Xunit;

namespace HeboTech.ATLib.Tests.Messaging
{
    public class CodingGroupTests
    {
        [Theory]
        [InlineData(CodingGroup.GeneralDataCoding, 0)]
        [InlineData(CodingGroup.MessageMarkedForAutomaticDeletion, 1)]
        [InlineData(CodingGroup.Reserved, 2)]
        [InlineData(CodingGroup.MessageWaitingInfo_DiscardMessage, 3)]
        [InlineData(CodingGroup.MessageWaitingInfo_StoreMessage, 4)]
        [InlineData(CodingGroup.DataCoding_MessageClass, 5)]
        internal void Has_correct_values(CodingGroup codingGroup, int expectedValue)
        {
            Assert.Equal(expectedValue, (int)codingGroup);
        }
    }
}
