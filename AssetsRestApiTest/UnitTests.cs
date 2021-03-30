using AssetsRestApi;
using Xunit;

namespace AssetsRestApiTests
{
    public class UnitTests
    {
        [Fact]
        public void AssetsRestApiUnitTest()
        {
            Assert.True(Program.MainAsync(true).Result);
        }
    }
}
