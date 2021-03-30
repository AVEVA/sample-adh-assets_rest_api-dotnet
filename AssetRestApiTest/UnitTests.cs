using AssetsRestApi;
using Xunit;

namespace AssetsRestApiTest
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
