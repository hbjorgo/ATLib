using HeboTech.ATLib.Parsers;
using Xunit;

namespace HeboTech.ATLib.Tests.Parsers
{
    public class AtErrorParsersTests
    {
        [Fact]
        public void CME_error_code_is_parsed()
        {
            AtResponse response = new()
            {
                FinalResponse = "+CME ERROR: 14",
                Success = false
            };

            bool success = AtErrorParsers.TryGetError(response.FinalResponse, out Error error);

            Assert.True(success);
            Assert.Equal(14, error.ErrorCode);
            Assert.Equal("SIM busy", error.ErrorMessage);
        }

        [Fact]
        public void CMS_error_code_is_parsed()
        {
            AtResponse response = new()
            {
                FinalResponse = "+CMS ERROR: 500",
                Success = false
            };

            bool success = AtErrorParsers.TryGetError(response.FinalResponse, out Error error);

            Assert.True(success);
            Assert.Equal(500, error.ErrorCode);
            Assert.Equal("unknown error", error.ErrorMessage);
        }

        [Fact]
        public void Unknown_error_code_returns_null()
        {
            AtResponse response = new()
            {
                FinalResponse = "+CME ERROR: 1337",
                Success = false
            };

            bool success = AtErrorParsers.TryGetError(response.FinalResponse, out Error error);

            Assert.False(success);
            Assert.Null(error);
        }

        [Fact]
        public void Unknown_error_prefix_returns_null()
        {
            AtResponse response = new()
            {
                FinalResponse = "+AAA ERROR: 500",
                Success = false
            };

            bool success = AtErrorParsers.TryGetError(response.FinalResponse, out Error error);

            Assert.False(success);
            Assert.Null(error);
        }
    }
}
