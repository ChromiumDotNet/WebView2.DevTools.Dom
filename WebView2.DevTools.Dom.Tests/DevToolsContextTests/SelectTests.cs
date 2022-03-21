using System;
using System.Threading.Tasks;
using WebView2.DevTools.Dom;
using WebView2.DevTools.Dom.Tests.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace WebView2.DevTools.Dom.Tests.DevToolsContextTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class SelectTests : DevTooolsContextBaseTest
    {
        public SelectTests(ITestOutputHelper output) : base(output)
        {
        }

        [WebView2ContextFact]
        public async Task ShouldSelectSingleOption()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/select.html");
            await DevToolsContext.SelectAsync("select", "blue");
            Assert.Equal(new string[] { "blue" }, await DevToolsContext.EvaluateExpressionAsync<string[]>("result.onInput"));
            Assert.Equal(new string[] { "blue" }, await DevToolsContext.EvaluateExpressionAsync<string[]>("result.onChange"));
        }

        [WebView2ContextFact]
        public async Task ShouldSelectOnlyFirstOption()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/select.html");
            await DevToolsContext.SelectAsync("select", "blue", "green", "red");
            Assert.Equal(new string[] { "blue" }, await DevToolsContext.EvaluateExpressionAsync<string[]>("result.onInput"));
            Assert.Equal(new string[] { "blue" }, await DevToolsContext.EvaluateExpressionAsync<string[]>("result.onChange"));
        }

        [WebView2ContextFact]
        public async Task ShouldNotThrowWhenSelectCausesNavigation()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/select.html");
            await DevToolsContext.QuerySelectorAsync("select").EvaluateFunctionAsync("select => select.addEventListener('input', () => window.location = '/empty.html')");
            await Task.WhenAll(
              DevToolsContext.SelectAsync("select", "blue"),
              WebView.CoreWebView2.WaitForNavigationAsync()
            );
            Assert.Contains("empty.html", DevToolsContext.Url);
        }

        [WebView2ContextFact]
        public async Task ShouldSelectMultipleOptions()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/select.html");
            await DevToolsContext.EvaluateExpressionAsync("makeMultiple()");
            await DevToolsContext.SelectAsync("select", "blue", "green", "red");
            Assert.Equal(new string[] { "blue", "green", "red" },
                         await DevToolsContext.EvaluateExpressionAsync<string[]>("result.onInput"));
            Assert.Equal(new string[] { "blue", "green", "red" },
                         await DevToolsContext.EvaluateExpressionAsync<string[]>("result.onChange"));
        }

        [WebView2ContextFact]
        public async Task ShouldRespectEventBubbling()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/select.html");
            await DevToolsContext.SelectAsync("select", "blue");
            Assert.Equal(new string[] { "blue" }, await DevToolsContext.EvaluateExpressionAsync<string[]>("result.onBubblingInput"));
            Assert.Equal(new string[] { "blue" }, await DevToolsContext.EvaluateExpressionAsync<string[]>("result.onBubblingChange"));
        }

        [WebView2ContextFact]
        public async Task ShouldThrowWhenElementIsNotASelect()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/select.html");
            var exception = await Assert.ThrowsAsync<WebView2DevToolsEvaluationFailedException>(async () => await DevToolsContext.SelectAsync("body", ""));
            Assert.Contains("Element is not a <select> element.", exception.Message);
        }

        [WebView2ContextFact]
        public async Task ShouldReturnEmptyArrayOnNoMatchedValues()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/select.html");
            var result = await DevToolsContext.SelectAsync("select", "42", "abc");
            Assert.Empty(result);
        }

        [WebView2ContextFact]
        public async Task ShouldReturnAnArrayOfMatchedValues()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/select.html");
            await DevToolsContext.EvaluateExpressionAsync("makeMultiple()");
            var result = await DevToolsContext.SelectAsync("select", "blue", "black", "magenta");
            Array.Sort(result);
            Assert.Equal(new string[] { "black", "blue", "magenta" }, result);
        }

        [WebView2ContextFact]
        public async Task ShouldReturnAnArrayOfOneElementWhenMultipleIsNotSet()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/select.html");
            Assert.Single(await DevToolsContext.SelectAsync("select", "42", "blue", "black", "magenta"));
        }

        [WebView2ContextFact]
        public async Task ShouldReturnEmptyArrayOnNoValues()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/select.html");
            Assert.Empty(await DevToolsContext.SelectAsync("select"));
        }

        [WebView2ContextFact]
        public async Task ShouldDeselectAllOptionsWhenPassedNoValuesForAMultipleSelect()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/select.html");
            await DevToolsContext.EvaluateExpressionAsync("makeMultiple()");
            await DevToolsContext.SelectAsync("select", "blue", "black", "magenta");
            await DevToolsContext.SelectAsync("select");
            Assert.True(await DevToolsContext.QuerySelectorAsync("select").EvaluateFunctionAsync<bool>(
                "select => Array.from(select.options).every(option => !option.selected)"));
        }

        [WebView2ContextFact]
        public async Task ShouldDeselectAllOptionsWhenPassedNoValuesForASelectWithoutMultiple()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/input/select.html");
            await DevToolsContext.SelectAsync("select", "blue", "black", "magenta");
            await DevToolsContext.SelectAsync("select");
            Assert.True(await DevToolsContext.QuerySelectorAsync("select").EvaluateFunctionAsync<bool>(
                "select => Array.from(select.options).every(option => !option.selected)"));
        }
    }
}
