using AssetRestApiCore;
using Xunit;

namespace AssetRestApiCoreTest
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
