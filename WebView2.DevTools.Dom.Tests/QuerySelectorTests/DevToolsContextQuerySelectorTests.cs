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
                    // https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-disposeasync#using-async-disposable
                    await using var devToolsContext = await coreWebView2.CreateDevToolsContextAsync();

                    // Get element by Id
                    // https://developer.mozilla.org/en-US/docs/Web/API/Document/querySelector
                    var element = await devToolsContext.QuerySelectorAsync("#myElementId");

                    //Strongly typed element types
                    //Only a subset of element types have been added so far, use HtmlElement as a generic type for all others
                    var htmlDivElement = await devToolsContext.QuerySelectorAsync<HtmlDivElement>("#myDivElementId");
                    var htmlSpanElement = await devToolsContext.QuerySelectorAsync<HtmlSpanElement>("#mySpanElementId");
                    var htmlSelectElement = await devToolsContext.QuerySelectorAsync<HtmlSelectElement>("#mySelectElementId");
                    var htmlInputElement = await devToolsContext.QuerySelectorAsync<HtmlInputElement>("#myInputElementId");
                    var htmlFormElement = await devToolsContext.QuerySelectorAsync<HtmlFormElement>("#myFormElementId");
                    var htmlAnchorElement = await devToolsContext.QuerySelectorAsync<HtmlAnchorElement>("#myAnchorElementId");
                    var htmlImageElement = await devToolsContext.QuerySelectorAsync<HtmlImageElement>("#myImageElementId");
                    var htmlTextAreaElement = await devToolsContext.QuerySelectorAsync<HtmlImageElement>("#myTextAreaElementId");
                    var htmlButtonElement = await devToolsContext.QuerySelectorAsync<HtmlButtonElement>("#myButtonElementId");
                    var htmlParagraphElement = await devToolsContext.QuerySelectorAsync<HtmlParagraphElement>("#myParagraphElementId");
                    var htmlTableElement = await devToolsContext.QuerySelectorAsync<HtmlTableElement>("#myTableElementId");

                    // Get a custom attribute value
                    var customAttribute = await element.GetAttributeAsync<string>("data-customAttribute");

                    //Set innerText property for the element
                    await element.SetPropertyValueAsync("innerText", "Welcome!");

                    await element.SetInnerTextAsync("Welcome 2!");

                    //Get innerText property for the element
                    var innerText = await element.GetInnerTextAsync();
                    //Can also be acessed via calling GetPropertyValueAsync
                    //Can use this method to get any property that isn't currently mapped
                    innerText = await element.GetPropertyValueAsync<string>("innerText");

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

                    //Event Handler
                    //Expose a function to javascript, functions persist across navigations
                    //So only need to do this once
                    await devToolsContext.ExposeFunctionAsync("jsAlertButtonClick", () =>
                    {
                        _ = devToolsContext.EvaluateExpressionAsync("window.alert('Hello! You invoked window.alert()');");
                    });

                    var jsAlertButton = await devToolsContext.QuerySelectorAsync("#jsAlertButton");

                    //Write up the click event listner to call our exposed function
                    _ = jsAlertButton.AddEventListenerAsync("click", "jsAlertButtonClick");

                    //Get a collection of HtmlElements
                    var divElements = await devToolsContext.QuerySelectorAllAsync<HtmlDivElement>("div");

                    foreach (var div in divElements)
                    {
                        // Get a reference to the CSSStyleDeclaration
                        var style = await div.GetStyleAsync();

                        //Set the border to 1px solid red
                        await style.SetPropertyAsync("border", "1px solid red", important: true);

                        await div.SetAttributeAsync("data-customAttribute", "123");
                        await div.SetInnerTextAsync("Updated Div innerText");
                    }

                    //Using standard array
                    var tableRows = await htmlTableElement.GetRowsAsync().ToArrayAsync();

                    foreach(var row in tableRows)
                    {
                        var cells = await row.GetCellsAsync().ToArrayAsync();
                        foreach(var cell in cells)
                        {
                            var newDiv = await devToolsContext.CreateHtmlElementAsync<HtmlDivElement>("div");
                            await newDiv.SetInnerTextAsync("New Div Added!");
                            await cell.AppendChildAsync(newDiv);
                        }
                    }

                    //Get a reference to the HtmlCollection and use async enumerable
                    //Requires Net Core 3.1 or higher
                    var tableRowsHtmlCollection = await htmlTableElement.GetRowsAsync();

                    await foreach (var row in tableRowsHtmlCollection)
                    {
                        var cells = await row.GetCellsAsync();
                        await foreach (var cell in cells)
                        {
                            var newDiv = await devToolsContext.CreateHtmlElementAsync<HtmlDivElement>("div");
                            await newDiv.SetInnerTextAsync("New Div Added!");
                            await cell.AppendChildAsync(newDiv);
                        }
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
