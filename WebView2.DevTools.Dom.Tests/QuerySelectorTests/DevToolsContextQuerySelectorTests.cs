using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using WebView2.DevTools.Dom.Tests.Attributes;
using Microsoft.Web.WebView2.Core;

namespace WebView2.DevTools.Dom.Tests.QuerySelectorTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class DevToolsContextQuerySelectorTests : DevTooolsContextBaseTest
    {
        public DevToolsContextQuerySelectorTests(ITestOutputHelper output) : base(output)
        {
        }

#pragma warning disable IDE0051 // Remove unused private members
        void Usage(CoreWebView2 coreWebView2)
#pragma warning restore IDE0051 // Remove unused private members
        {
            #region QuerySelector

            // Add using WebView2.DevTools.Dom; to get access to the
            // CreateDevToolsContextAsync extension method

            coreWebView2.NavigationCompleted += async (sender, args) =>
            {
                if(args.IsSuccess)
                {
                    // WebView2DevToolsContext implements IAsyncDisposable and can be Disposed
                    // via await using or await devToolsContext.DisposeAsync();
                    // Only DisposeAsync is supported. It's very important the WebView2DevToolsContext is Disposed
                    // When you have finished. Only create a single instance at a time, reuse an instance rather than
                    // creaeting a new WebView2DevToolsContext. Dispose the old WebView2DevToolsContext instance before
                    // creating a new instance if you need to manage the lifespan manually.
                    // https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-disposeasync#using-async-disposable
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

                    //Change CSS style background colour
                    _ = await element.EvaluateFunctionAsync("e => e.style.backgroundColor = 'yellow'");

                    //Type text in an input field
                    await element.TypeAsync("Welcome to my Website!");

                    //Scroll Element into View (if needed)
                    //Can optional specify a Rect to be scrolled into view, relative to the node's border box,
                    //in CSS pixels. When omitted, center of the node will be used
                    await element.ScrollIntoViewIfNeededAsync();

                    //Click The element
                    await element.ClickAsync();

                    var divElements = await devtoolsContext.QuerySelectorAllAsync("div");

                    foreach (var div in divElements)
                    {
                        // Get a reference to the CSSStyleDeclaration
                        var style = await div.GetStyleAsync();

                        //Set the border to 1px solid red
                        await style.SetPropertyAsync("border", "1px solid red", important: true);

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
