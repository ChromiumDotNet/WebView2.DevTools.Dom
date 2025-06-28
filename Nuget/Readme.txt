# WebView2 DevTools DOM

WebView2 DevTools Dom is a port of [puppeteer-sharp by Darío Kondratiuk](https://github.com/hardkoded/puppeteer-sharp) that has been adapted specifically for use with WebView2.
- Direct communication with the CoreWebView2 via the DevTools protocol (no need to open a Remote Debugging Port).
- 1:1 mapping of WebView2DevToolsContext and CoreWebView2 (create a single WebView2DevToolsContext per CoreWebView2 instance)
- The primary focus of this project is DOM access/manipulation and Javascript execution/evaluation.
- Only a **subset** of the Puppeteer Sharp features were ported (It maybe possible to port additional features if sufficent user demand).

# Sponsorware

This project has adopted a variant of the [Sponsorware](https://github.com/sponsorware/docs) open source model. To ensure the project maintainer/developer ([@amaitland](https://github.com/amaitland))
can support the project the source will be released under an MIT license when the target of 25 sponsors signup to the [WebView2 DevTools Dom Supporter](https://github.com/sponsors/amaitland/)
tier here on GitHub. Sponsors will get **priority support**. **Everyone** is free to download and use the Nuget package.

# Prerequisites

 * .Net 4.6.2 or .Net Core 3.1 or greater
 * Microsoft.Web.WebView2.DevToolsProtocolExtension 1.0.824 or greater

# Questions and Support

Sponsors can:

* Ask a question on [Discussions](https://github.com/amaitland/WebView2.DevTools.Dom/discussions).
* File bug reports on [Issues](https://github.com/amaitland/WebView2.DevTools.Dom/issues).

# Usage

Checkout https://github.com/ChromiumDotNet/WebView2.DevTools.Dom#readme for more details