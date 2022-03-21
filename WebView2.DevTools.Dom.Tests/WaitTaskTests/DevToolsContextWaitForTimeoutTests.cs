using System;
using System.Threading.Tasks;
using WebView2.DevTools.Dom.Tests.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace WebView2.DevTools.Dom.Tests.WaitForTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class DevToolsContextWaitForTimeoutTests : DevTooolsContextBaseTest
    {
        public DevToolsContextWaitForTimeoutTests(ITestOutputHelper output) : base(output)
        {
        }

        [WebView2ContextFact]
        public async Task WaitsForTheGivenTimeoutBeforeResolving()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.EmptyPage);
            var startTime = DateTime.Now;
            await DevToolsContext.WaitForTimeoutAsync(1000);
            Assert.True((DateTime.Now - startTime).TotalMilliseconds > 700);
            Assert.True((DateTime.Now - startTime).TotalMilliseconds < 1300);
        }
    }
}
