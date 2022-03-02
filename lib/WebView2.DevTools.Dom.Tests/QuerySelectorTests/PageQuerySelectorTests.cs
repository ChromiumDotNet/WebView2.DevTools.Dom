using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using WebView2.DevTools.Dom.Tests.Attributes;
using Microsoft.Web.WebView2.Core;

namespace WebView2.DevTools.Dom.Tests.QuerySelectorTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class PageQuerySelectorTests : DevTooolsContextBaseTest
    {
        public PageQuerySelectorTests(ITestOutputHelper output) : base(output)
        {
        }

#pragma warning disable IDE0051 // Remove unused private members
        void Usage(CoreWebView2 coreWebView2)
#pragma warning restore IDE0051 // Remove unused private members
        {
            #region QuerySelector

            // Add using WebView2.DevTools.Dom to get access to the
            // CreateDevToolsContextAsync extension method

            coreWebView2.NavigationCompleted += async (sender, args) =>
            {
                if(args.IsSuccess)
                {
                    await using var devtoolsContext = await coreWebView2.CreateDevToolsContextAsync();

                    // Get element by Id
                    // https://developer.mozilla.org/en-US/docs/Web/API/Document/querySelector
                    var element = await devtoolsContext.QuerySelectorAsync("#myElementId");

                    // Get a custom attribute value
                    var customAttribute = await element.GetAttributeValueAsync<string>("data-customAttribute");

                    //Set innerText property for the element
                    await element.SetPropertyValueAsync("innerText", "Welcome!");

                    //Get innerText property for the element
                    var innerText = await element.GetPropertyValueAsync<string>("innerText");

                    //Get all child elements
                    var childElements = await element.QuerySelectorAllAsync("div");

                    //Click The element
                    await element.ClickAsync();

                    var divElements = await devtoolsContext.QuerySelectorAllAsync("div");

                    foreach (var div in divElements)
                    {
                        var style = await div.GetAttributeValueAsync<string>("style");
                        await div.SetAttributeValueAsync("data-customAttribute", "123");
                        await div.SetPropertyValueAsync("innerText", "Updated Div innerText");
                    }
                }
            };            

            #endregion
        }

        [WebView2ContextFact]
        public async Task ShouldQueryExistingElement()
        {
            await DevToolsContext.SetContentAsync("<section>test</section>");
            var element = await DevToolsContext.QuerySelectorAsync("section");
            Assert.NotNull(element);
        }

        [WebView2ContextFact]
        public async Task ShouldReturnNullForNonExistingElement()
        {
            var element = await DevToolsContext.QuerySelectorAsync("non-existing-element");
            Assert.Null(element);
        }
    }
}
