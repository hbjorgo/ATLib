using HeboTech.ATLib.Parsers;
using System.Threading.Tasks;
using Xunit;

namespace HeboTech.ATLib.Tests.Parsers
{
    public class AtErrorParsersTests
    {
        [Fact]
        public async Task Cme_error_code_is_parsed()
        {
            AtResponse response = new()
            {
                FinalResponse = "+CME ERROR: 14",
                Success = false
            };

            Error error = AtErrorParsers.GetError(response.FinalResponse);

            Assert.Equal(14, error.ErrorCode);
            Assert.Equal("SIM busy", error.ErrorMessage);
        }
    }
}
