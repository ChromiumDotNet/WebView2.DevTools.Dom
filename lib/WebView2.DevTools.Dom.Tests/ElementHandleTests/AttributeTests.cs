using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit;
using WebView2.DevTools.Dom.Tests.Attributes;

namespace WebView2.DevTools.Dom.Tests.ElementHandleTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class AttributeTests : DevTooolsContextBaseTest
    {
        public AttributeTests(ITestOutputHelper output) : base(output)
        {
        }

        [WebView2ContextFact]
        public async Task ShouldGetAttribute()
        {
            const string expected = "checkbox";

            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/checkbox.html");
            var checkbox = await DevToolsContext.QuerySelectorAsync("#agree");
            var actual = await checkbox.GetAttributeValueAsync<string>("type");

            Assert.Equal(expected, actual);
        }

        [WebView2ContextFact]
        public async Task ShouldSetAttribute()
        {
            const int expected = 1676;

            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/checkbox.html");
            var checkbox = await DevToolsContext.QuerySelectorAsync("#agree");
            await checkbox.SetAttributeValueAsync("data-custom", expected);

            var actual = await checkbox.GetAttributeValueAsync<int>("data-custom");

            Assert.Equal(expected, actual);
        }
    }
}
