using System.Text.Json;
using System;
using System.Threading.Tasks;
using WebView2.DevTools.Dom;
using Microsoft.Web.WebView2.Core;
using WebView2.DevTools.Dom.Tests.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace WebView2.DevTools.Dom.Tests.DevToolsContextTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class PageEventsPageErrorTests : DevTooolsContextBaseTest
    {
        public PageEventsPageErrorTests(ITestOutputHelper output) : base(output)
        {
        }

        [WebView2ContextFact(Skip = "Possibly implement this")]
        public Task ShouldFire()
        {
            throw new System.NotImplementedException();
            //string error = null;
            //void EventHandler(object sender, PageErrorEventArgs e)
            //{
            //    error = e.Message;
            //    Page.PageError -= EventHandler;
            //}

            //Page.PageError += EventHandler;

            //var completion = new TaskCompletionSource<JsonElement>();

            //var reciever = WebView.CoreWebView2.GetDevToolsProtocolEventReceiver("Runtime.exceptionThrown");

            //reciever.DevToolsProtocolEventReceived += handler;

            //void handler(object sender, CoreWebView2DevToolsProtocolEventReceivedEventArgs e)
            //{
            //    var d = JsonDocument.Parse(e.ParameterObjectAsJson);
                
            //    if (d.RootElement.GetProperty("MessageId").GetString() != "Runtime.exceptionThrown")
            //    {
            //        return;
            //    }
            //    reciever.DevToolsProtocolEventReceived -= handler;
            //    completion.SetResult(d.RootElement.GetProperty("MessageData").GetString());
            //}

            //reciever.DevToolsProtocolEventReceived += handler;
            //return completion.Task;

            //await Task.WhenAll(
            //    WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/error.html"),
            //    WaitEvent(, )
            //);

            //Assert.Contains("Fancy", error);
        }
    }
}
