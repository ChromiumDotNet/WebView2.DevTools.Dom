using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit;
using WebView2.DevTools.Dom.Tests.Attributes;
using Microsoft.Web.WebView2.Core;

namespace WebView2.DevTools.Dom.Tests.DevToolsContextTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class SetContentTests : DevTooolsContextBaseTest
    {
        const string ExpectedOutput = "<html><head></head><body><div>hello</div></body></html>";

        public SetContentTests(ITestOutputHelper output) : base(output)
        {
        }

#pragma warning disable IDE0051 // Remove unused private members
        async Task Usage(CoreWebView2 coreWebView2)
#pragma warning restore IDE0051 // Remove unused private members
        {
            #region SetContentAsync
            await using var devtoolsContext = await coreWebView2.CreateDevToolsContextAsync();
            await devtoolsContext.SetContentAsync("<div>My Receipt</div>");
            var result = await devtoolsContext.GetContentAsync();

            #endregion
        }

        [WebView2ContextFact]
        public async Task ShouldWork()
        {
            await DevToolsContext.SetContentAsync("<div>hello</div>");
            var result = await DevToolsContext.GetContentAsync();

            Assert.Equal(ExpectedOutput, result);
        }

        [WebView2ContextFact]
        public async Task ShouldWorkWithDoctype()
        {
            const string doctype = "<!DOCTYPE html>";

            await DevToolsContext.SetContentAsync($"{doctype}<div>hello</div>");
            var result = await DevToolsContext.GetContentAsync();

            Assert.Equal($"{doctype}{ExpectedOutput}", result);
        }

        [WebView2ContextFact]
        public async Task ShouldWorkWithHtml4Doctype()
        {
            const string doctype = "<!DOCTYPE html PUBLIC \" -//W3C//DTD HTML 4.01//EN\" " +
                "\"http://www.w3.org/TR/html4/strict.dtd\">";

            await DevToolsContext.SetContentAsync($"{doctype}<div>hello</div>");
            var result = await DevToolsContext.GetContentAsync();

            Assert.Equal($"{doctype}{ExpectedOutput}", result);
        }

        [WebView2ContextFact]
        public async Task ShouldWorkFastEnough()
        {
            for (var i = 0; i < 20; ++i)
            {
                await DevToolsContext.SetContentAsync("<div>yo</div>");
            }
        }

        [WebView2ContextFact]
        public async Task ShouldWorkWithTrickyContent()
        {
            await DevToolsContext.SetContentAsync("<div>hello world</div>\x7F");
            Assert.Equal("hello world", await DevToolsContext.QuerySelectorAsync("div").EvaluateFunctionAsync<string>("div => div.textContent"));
        }

        [WebView2ContextFact]
        public async Task ShouldWorkWithAccents()
        {
            await DevToolsContext.SetContentAsync("<div>aberración</div>");
            Assert.Equal("aberración", await DevToolsContext.QuerySelectorAsync("div").EvaluateFunctionAsync<string>("div => div.textContent"));
        }

        [WebView2ContextFact]
        public async Task ShouldWorkWithEmojis()
        {
            await DevToolsContext.SetContentAsync("<div>🐥</div>");
            Assert.Equal("🐥", await DevToolsContext.QuerySelectorAsync("div").EvaluateFunctionAsync<string>("div => div.textContent"));
        }

        [WebView2ContextFact]
        public async Task ShouldWorkWithNewline()
        {
            await DevToolsContext.SetContentAsync("<div>\n</div>");
            Assert.Equal("\n", await DevToolsContext.QuerySelectorAsync("div").EvaluateFunctionAsync<string>("div => div.textContent"));
        }
    }
}
