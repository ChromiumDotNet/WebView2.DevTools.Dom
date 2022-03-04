# WebView2 DevTools DOM

WebView2 DevTools Dom is a port of [puppeteer-sharp by Darío Kondratiuk](https://github.com/hardkoded/puppeteer-sharp) that has been adapted specifically for use with WebView2.
Direct communication with the CoreWebView2 via the DevTools protocol.
1:1 mapping of WebView2DevToolsContext and CoreWebView2
The primary focus of this project is DOM access/manipulation and Javascript execution/evaluation. Only a subset of the Puppeteer Sharp features
were ported. It maybe possible to port additional features if sufficent user demand.

# Sponsorware

This project has adopted a variant of the [Sponsorware](https://github.com/sponsorware/docs) open source model. To ensure the project maintainer/developer ([@amaitland](https://github.com/amaitland))
can support the project the source will be released under an MIT license when the target of 45 sponsors via [GitHub Sponsors](https://github.com/sponsors/amaitland/)
is reached. Sponsors will get **priority support**. Everyone is free to download and use the Nuget package.

# Prerequisites

 * .Net 4.6.2 or .Net Core 3.1 or greater
 * Microsoft.Web.WebView2.DevToolsProtocolExtension 1.0.824 or greater

# Questions and Support

Sponsors can:

* Ask a question on [Discussions](https://github.com/amaitland/WebView2.DevTools.Dom/discussions).
* File bug reports on [Issues](https://github.com/amaitland/WebView2.DevTools.Dom/issues).

# Usage

## WebView2DevToolsContext

The **WebView2DevToolsContext** class is the main entry point into the library and can be created from a
[CoreWebView2](https://docs.microsoft.com/en-us/dotnet/api/microsoft.web.webview2.core.corewebview2) instance.

```c#
var devtoolsContext = await coreWebView2.CreateDevToolsContextAsync();
```

## DOM Access

Read/write to the DOM
<!-- snippet: QuerySelector -->
<a id='snippet-queryselector'></a>
```cs
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
```
<sup><a href='/lib/WebView2.DevTools.Dom.Tests/QuerySelectorTests/PageQuerySelectorTests.cs#L20-L61' title='Snippet source file'>snippet source</a> | <a href='#snippet-queryselector' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Inject HTML
<!-- snippet: SetContentAsync -->
<a id='snippet-setcontentasync'></a>
```cs
await using var devtoolsContext = await coreWebView2.CreateDevToolsContextAsync();
await devtoolsContext.SetContentAsync("<div>My Receipt</div>");
var result = await devtoolsContext.GetContentAsync();
```
<sup><a href='/lib/WebView2.DevTools.Dom.Tests/DevToolsContextTests/SetContentTests.cs#L22-L27' title='Snippet source file'>snippet source</a> | <a href='#snippet-setcontentasync' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Evaluate Javascript

<!-- snippet: Evaluate -->
<a id='snippet-evaluate'></a>
```cs
await webView2Browser.EnsureCoreWebView2Async();

using var devToolsContext = await webView2Browser.CoreWebView2.CreateDevToolsContextAsync();
await devToolsContext.IgnoreCertificateErrorsAsync(true);
var seven = await devToolsContext.EvaluateExpressionAsync<int>("4 + 3");
var someObject = await devToolsContext.EvaluateFunctionAsync<dynamic>("(value) => ({a: value})", 5);
System.Console.WriteLine(someObject.a);
```
<sup><a href='/lib/WebView2.DevTools.Dom.Tests/QuerySelectorTests/ElementHandleQuerySelectorEvalTests.cs#L19-L28' title='Snippet source file'>snippet source</a> | <a href='#snippet-evaluate' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## NOT YET SUPPORTED
- Drag Interception/Events
- Print to PDF