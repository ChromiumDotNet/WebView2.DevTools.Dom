using System.Threading.Tasks;
using WebView2.DevTools.Dom.Tests.Attributes;
using Xunit.Abstractions;
using Xunit;

namespace WebView2.DevTools.Dom.Tests.ElementHandleTests
{

    [Collection(TestConstants.TestFixtureCollectionName)]
    public class CssStyleTests : DevTooolsContextBaseTest
    {
        public CssStyleTests(ITestOutputHelper output) : base(output)
        {
        }

        [WebView2ContextFact]
        public async Task ShouldWork()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/button.html");
            var button = await DevToolsContext.QuerySelectorAsync("button");

            var style = await button.GetStyleAsync();

            Assert.NotNull(style);
        }

        [WebView2ContextFact]
        public async Task ShouldGetPriorityFalse()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/button.html");
            var button = await DevToolsContext.QuerySelectorAsync("button");

            var style = await button.GetStyleAsync();

            var priority = await style.GetPropertyPriorityAsync("border");

            Assert.False(priority);
        }

        [WebView2ContextFact]
        public async Task ShouldGetPriorityTrue()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/button.html");
            var button = await DevToolsContext.QuerySelectorAsync("button");

            var style = await button.GetStyleAsync();

            await style.SetPropertyAsync("border", "1px solid red", true);

            var val = await style.GetPropertyValueAsync<string>("border");

            var priority = await style.GetPropertyPriorityAsync("border");

            Assert.True(priority);
        }

        [WebView2ContextFact]
        public async Task ShouldGetPropertyNameByIndex()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/button.html");
            var button = await DevToolsContext.QuerySelectorAsync("button");

            var style = await button.GetStyleAsync();

            await style.SetPropertyAsync("border", "1px solid red", true);

            var propertyName = await style.ItemAsync(0);

            Assert.Equal("border-top-width", propertyName);
        }

        [WebView2ContextFact]
        public async Task ShouldRemovePropertyByName()
        {
            const string expectedValue = "1px";

            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/button.html");
            var button = await DevToolsContext.QuerySelectorAsync("button");

            var style = await button.GetStyleAsync();

            await style.SetPropertyAsync("border-top-width", expectedValue, true);
            
            var actualValue = await style.RemovePropertyAsync("border-top-width");

            Assert.Equal(expectedValue, actualValue);
        }
    }
}
