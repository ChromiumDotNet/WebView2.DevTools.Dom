using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace WebView2.DevTools.Dom.Tests.EmulationTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class PageEmulateCPUThrottlingTests : DevTooolsContextBaseTest
    {
        public PageEmulateCPUThrottlingTests(ITestOutputHelper output) : base(output)
        {
        }

        public async Task ShouldChangeTheCPUThrottlingRateSuccessfully()
        {
            await DevToolsContext.EmulateCPUThrottlingAsync(100);
            await DevToolsContext.EmulateCPUThrottlingAsync();
        }
    }
}
