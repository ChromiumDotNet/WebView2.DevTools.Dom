using System.Threading.Tasks;
using WebView2.DevTools.Dom;
using WebView2.DevTools.Dom.Tests.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace WebView2.DevTools.Dom.Tests.TouchScreenTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class TouchScreenTests : DevTooolsContextBaseTest
    {
        //private readonly DeviceDescriptor _iPhone = Emulation.Devices[DeviceDescriptorName.IPhone6];

        public TouchScreenTests(ITestOutputHelper output) : base(output)
        {
        }

        [WebView2ContextFact(Skip = "Fix me")]
        public Task ShouldTapTheButton()
        {
            throw new System.NotImplementedException();
            //await Page.EmulateAsync(_iPhone);
            //await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/button.html");
            //await Page.TapAsync("button");
            //Assert.Equal("Clicked", await Page.EvaluateExpressionAsync<string>("result"));
        }

        [WebView2ContextFact(Skip = "Fix me")]
        public Task ShouldReportTouches()
        {
            throw new System.NotImplementedException();
            //await Page.EmulateAsync(_iPhone);
            //await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/touches.html");
            //var button = await Page.QuerySelectorAsync("button");
            //await button.TapAsync();
            //Assert.Equal(new string[] {
            //    "Touchstart: 0",
            //    "Touchend: 0"
            //}, await Page.EvaluateExpressionAsync<string[]>("getResult()"));
        }
    }
}
