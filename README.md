# WebView2 DevTools DOM

WebView2 DevTools Dom is a port of [puppeteer-sharp by Darío Kondratiuk](https://github.com/hardkoded/puppeteer-sharp) that has been adapted specifically for use with WebView2.
- Direct communication with the CoreWebView2 via the DevTools protocol (no need to open a Remote Debugging Port).
- 1:1 mapping of WebView2DevToolsContext and CoreWebView2 (create a single WebView2DevToolsContext per CoreWebView2 instance)
- The primary focus of this project is DOM access/manipulation and Javascript execution/evaluation.
- Only a **subset** of the Puppeteer Sharp features were ported (It maybe possible to port additional features if sufficent user demand).

# Sponsorware

This project has adopted a variant of the [Sponsorware](https://github.com/sponsorware/docs) open source model. To ensure the project maintainer/developer ([@amaitland](https://github.com/amaitland))
can support the project the source will be released under an MIT license when the target of 25 sponsors signup to the [WebView2 DevTools Dom Supporter](https://github.com/sponsors/amaitland/)
tier here on GitHub. Sponsors will get **priority support** and automatically gain access to the project source. **Everyone** is free to download and use the Nuget package.

The xUnit tests are avaliable as part of this repository. 

# Prerequisites

 * .Net 4.6.2 or .Net Core 3.1 or greater
 * [Microsoft.Web.WebView2.DevToolsProtocolExtension](https://www.nuget.org/packages/Microsoft.Web.WebView2.DevToolsProtocolExtension/) 1.0.824 or greater

# Questions and Support

Sponsors can:

* Ask a question on [Discussions](https://github.com/ChromiumDotNet/WebView2.DevTools.Dom/discussions).
* File bug reports on [Issues](https://github.com/ChromiumDotNet/WebView2.DevTools.Dom/issues).

# Usage

## WebView2DevToolsContext

The **WebView2DevToolsContext** class is the main entry point into the library and can be created from a
[CoreWebView2](https://docs.microsoft.com/en-us/dotnet/api/microsoft.web.webview2.core.corewebview2) instance.
Only a **single** WebView2DevToolsContext should exist at any given time, when you are finished them make sure you
dispose via DisposeAsync.

```c#
// Add using WebView2.DevTools.Dom; to get access to the
// CreateDevToolsContextAsync extension method
var devtoolsContext = await coreWebView2.CreateDevToolsContextAsync();

// Manually dispose of context (only DisposeAsync is supported as the whole API is async)
await devToolsContext.DisposeAsync();
```

```c#
// Dispose automatically via await using
// https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-disposeasync#using-async-disposable
await using var devtoolsContext = await coreWebView2.CreateDevToolsContextAsync();
```

## DOM Access

Read/write to the DOM
<!-- snippet: QuerySelector -->
<a id='snippet-queryselector'></a>
```cs
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
        var divElements = await devToolsContext.QuerySelectorAllAsync("div");

        foreach (var div in divElements)
        {
            // Get a reference to the CSSStyleDeclaration
            var style = await div.GetStyleAsync();

            //Set the border to 1px solid red
            await style.SetPropertyAsync("border", "1px solid red", important: true);

            await div.SetAttributeAsync("data-customAttribute", "123");
            await div.SetInnerTextAsync("Updated Div innerText");
        }
    }
};
```
<sup><a href='/WebView2.DevTools.Dom.Tests/QuerySelectorTests/DevToolsContextQuerySelectorTests.cs#L20-L112' title='Snippet source file'>snippet source</a> | <a href='#snippet-queryselector' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Inject HTML
<!-- snippet: SetContentAsync -->
<a id='snippet-setcontentasync'></a>
```cs
// Add using WebView2.DevTools.Dom; to get access to the
// CreateDevToolsContextAsync extension method

// WebView2DevToolsContext implements IAsyncDisposable and can be Disposed
// via await using or await devToolsContext.DisposeAsync();
// Only DisposeAsync is supported. It's very important the WebView2DevToolsContext is Disposed
// When you have finished. Only create a single instance at a time, reuse an instance rather than
// creaeting a new WebView2DevToolsContext. Dispose the old WebView2DevToolsContext instance before
// creating a new instance if you need to manage the lifespan manually.
// https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-disposeasync#using-async-disposable
await using var devtoolsContext = await coreWebView2.CreateDevToolsContextAsync();
await devtoolsContext.SetContentAsync("<div>My Receipt</div>");
var result = await devtoolsContext.GetContentAsync();
```
<sup><a href='/WebView2.DevTools.Dom.Tests/DevToolsContextTests/SetContentTests.cs#L22-L37' title='Snippet source file'>snippet source</a> | <a href='#snippet-setcontentasync' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Evaluate Javascript

<!-- snippet: Evaluate -->
<a id='snippet-evaluate'></a>
```cs
// Add using WebView2.DevTools.Dom; to get access to the
// CreateDevToolsContextAsync extension method

await webView2Browser.EnsureCoreWebView2Async();

// WebView2DevToolsContext implements IAsyncDisposable and can be Disposed
// via await using or await devToolsContext.DisposeAsync();
// https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-disposeasync#using-async-disposable
await using var devToolsContext = await webView2Browser.CoreWebView2.CreateDevToolsContextAsync();
await devToolsContext.IgnoreCertificateErrorsAsync(true);
var seven = await devToolsContext.EvaluateExpressionAsync<int>("4 + 3");
// Can evaluate a function that returns a Promise
var fourtyTwo = await devToolsContext.EvaluateFunctionAsync<int>("() => Promise.resolve(42)");
// Pass in arguments to a function, including references to HtmlElements and JavascriptHandles
var someObject = await devToolsContext.EvaluateFunctionAsync<dynamic>("(value) => ({a: value})", 5);
System.Console.WriteLine(someObject.a);
```
<sup><a href='/WebView2.DevTools.Dom.Tests/QuerySelectorTests/ElementHandleQuerySelectorEvalTests.cs#L17-L35' title='Snippet source file'>snippet source</a> | <a href='#snippet-evaluate' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## NOT YET SUPPORTED
- Drag Interception/Events
- Print to PDF
