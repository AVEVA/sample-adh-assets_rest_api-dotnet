using AssetsRestApi;
using Xunit;

namespace AssetsRestApiTest
{
    public class UnitTests
    {
        [Fact]
        public void AssetRestApiCoreUnitTest()
        {
            Assert.True(Program.MainAsync(true).Result);
        }
    }
}
