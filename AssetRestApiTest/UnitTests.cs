using AssetsRestApi;
using Xunit;

namespace AssetsRestApiTests
{
    public class UnitTests
    {
        [Fact]
        public void AssetRestApiUnitTest()
        {
            Assert.True(Program.MainAsync(true).Result);
        }
    }
}
