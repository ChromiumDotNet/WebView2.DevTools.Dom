using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace WebView2.DevTools.Dom.Tests.DevToolsContextTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class SetJavaScriptEnabledTests : DevTooolsContextBaseTest
    {
        public SetJavaScriptEnabledTests(ITestOutputHelper output) : base(output)
        {
        }

        public async Task ShouldWork()
        {
            await DevToolsContext.SetJavaScriptEnabledAsync(false);
            await WebView.CoreWebView2.NavigateToAsync("data:text/html, <script>var something = 'forbidden'</script>");

            var exception = await Assert.ThrowsAnyAsync<Exception>(async () => await DevToolsContext.EvaluateExpressionAsync("something"));
            Assert.Contains("something is not defined", exception.Message);

            await DevToolsContext.SetJavaScriptEnabledAsync(true);
            await WebView.CoreWebView2.NavigateToAsync("data:text/html, <script>var something = 'forbidden'</script>");
            Assert.Equal("forbidden", await DevToolsContext.EvaluateExpressionAsync<string>("something"));
        }
    }
}
