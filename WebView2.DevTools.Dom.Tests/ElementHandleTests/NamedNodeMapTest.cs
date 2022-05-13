using System.Threading.Tasks;
using WebView2.DevTools.Dom.Tests.Attributes;
using Xunit.Abstractions;
using Xunit;

namespace WebView2.DevTools.Dom.Tests.ElementHandleTests
{

    [Collection(TestConstants.TestFixtureCollectionName)]
    public class NamedNodeMapTest : DevTooolsContextBaseTest
    {
        public NamedNodeMapTest(ITestOutputHelper output) : base(output)
        {
        }

        [WebView2ContextFact]
        public async Task ShouldWork()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/dataattributes.html");
            var namedNodeMap = await DevToolsContext.QuerySelectorAsync<HtmlElement>("h1").AndThen(x => x.GetAttributesAsync());

            Assert.NotNull(namedNodeMap);

            var data = await namedNodeMap.ToArrayAsync();

            Assert.NotNull(data);
            Assert.NotEmpty(data);

            Assert.Equal("data-testing", await data[0].GetNameAsync());
            Assert.Equal("Test1", await data[0].GetValueAsync());
        }
    }
}
