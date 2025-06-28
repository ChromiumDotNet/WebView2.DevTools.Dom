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
            var button = await DevToolsContext.QuerySelectorAsync<HtmlButtonElement>("button");

            var style = await button.GetStyleAsync();

            Assert.NotNull(style);
        }

        [WebView2ContextFact]
        public async Task ShouldGetPriorityFalse()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/button.html");
            var button = await DevToolsContext.QuerySelectorAsync<HtmlButtonElement>("button");

            var style = await button.GetStyleAsync();

            var priority = await style.GetPropertyPriorityAsync("border");

            Assert.False(priority);
        }

        [WebView2ContextFact]
        public async Task ShouldGetPriorityTrue()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/button.html");
            var button = await DevToolsContext.QuerySelectorAsync<HtmlButtonElement>("button");

            var style = await button.GetStyleAsync();

            await style.SetPropertyAsync("border", "1px solid red", true);

            var priority = await style.GetPropertyPriorityAsync("border");

            Assert.True(priority);
        }

        [WebView2ContextFact]
        public async Task ShouldGetPropertyAsString()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/button.html");
            var button = await DevToolsContext.QuerySelectorAsync<HtmlButtonElement>("button");

            var style = await button.GetStyleAsync();

            await style.SetPropertyAsync("border", "1px solid red", true);

            var actual = await style.GetPropertyValueAsync<string>("border-width");

            Assert.Equal("1px", actual);
        }

        [WebView2ContextFact]
        public async Task ShouldGetPropertyAsInt()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/button.html");
            var button = await DevToolsContext.QuerySelectorAsync<HtmlButtonElement>("button");

            var style = await button.GetStyleAsync();

            await style.SetPropertyAsync("z-index", 10);

            var actual = await style.GetPropertyValueAsync<int>("z-index");

            Assert.Equal(10, actual);
        }

        [InlineData("auto")]
        [InlineData("1")]
        [WebView2ContextTheory]
        public async Task ShouldGetPropertyZIndexAsObject(object expected)
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/button.html");
            var button = await DevToolsContext.QuerySelectorAsync<HtmlButtonElement>("button");

            var style = await button.GetStyleAsync();

            await style.SetPropertyAsync("z-index", expected);

            var actual = await style.GetPropertyValueAsync<object>("z-index");

            Assert.Equal(expected, actual);
        }

        [WebView2ContextFact]
        public async Task ShouldGetPropertyNameByIndex()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/button.html");
            var button = await DevToolsContext.QuerySelectorAsync<HtmlButtonElement>("button");

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
            var button = await DevToolsContext.QuerySelectorAsync<HtmlButtonElement>("button");

            var style = await button.GetStyleAsync();

            await style.SetPropertyAsync("border-top-width", expectedValue, true);
            
            var actualValue = await style.RemovePropertyAsync("border-top-width");

            Assert.Equal(expectedValue, actualValue);
        }
    }
}
