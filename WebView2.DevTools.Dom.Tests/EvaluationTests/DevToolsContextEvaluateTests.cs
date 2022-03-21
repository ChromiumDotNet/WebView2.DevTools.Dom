using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using WebView2.DevTools.Dom;
using System.Numerics;
using WebView2.DevTools.Dom.Tests.Attributes;
using System.Text.Json;
using System;

namespace WebView2.DevTools.Dom.Tests.DevToolsContextTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class EvaluateTests : DevTooolsContextBaseTest
    {
        public EvaluateTests(ITestOutputHelper output) : base(output)
        {
        }

        [WebView2ContextTheory]
        [InlineData("() => 7 * 3", 21)] //ShouldWork
        [InlineData("() => Promise.resolve(8 * 7)", 56)] //ShouldAwaitPromise
        public async Task BasicIntFunctionEvaluationTest(string script, object expected)
        {
            var result = await DevToolsContext.EvaluateFunctionAsync<int>(script);
            Assert.Equal(expected, result);
        }

        [WebView2ContextFact]
        public async Task ShouldTransferBigInt()
        {
            var result = await DevToolsContext.EvaluateFunctionAsync<BigInteger>("a => a", new BigInteger(42));
            Assert.Equal(new BigInteger(42), result);
        }

        [WebView2ContextTheory]
        [InlineData(double.NaN)] //ShouldTransferNaN
        [InlineData(-0)] //ShouldTransferNegative0
        [InlineData(double.PositiveInfinity)] //ShouldTransferInfinity
        [InlineData(double.NegativeInfinity)] //ShouldTransferNegativeInfinty
        public async Task BasicTransferTest(object transferObject)
        {
            var result = await DevToolsContext.EvaluateFunctionAsync<object>("a => a", transferObject);
            Assert.Equal(transferObject, result);
        }

        [WebView2ContextFact]
        public async Task ShouldTransferArrays()
        {
            var result = await DevToolsContext.EvaluateFunctionAsync<int[]>("a => a", new int[] { 1, 2, 3 });
            Assert.Equal(new int[] { 1, 2, 3 }, result);
        }

        [WebView2ContextFact]
        public async Task ShouldTransferArraysAsArraysNotObjects()
        {
            var result = await DevToolsContext.EvaluateFunctionAsync<bool>("a => Array.isArray(a)", new int[] { 1, 2, 3 });
            Assert.True(result);
        }

        [WebView2ContextFact]
        public async Task ShouldModifyGlobalEnvironment()
        {
            await DevToolsContext.EvaluateFunctionAsync("() => window.globalVar = 123");
            Assert.Equal(123, await DevToolsContext.EvaluateFunctionAsync<int>("() => window.globalVar"));
        }

        [WebView2ContextFact]
        public async Task ShouldEvaluateInThePageContext()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/global-var.html");
            Assert.Equal(123, await DevToolsContext.EvaluateFunctionAsync<int>("() => window.globalVar"));
        }

        [WebView2ContextFact]
        public async Task ShouldReturnUndefinedForObjectsWithSymbols()
            => Assert.Null(await DevToolsContext.EvaluateFunctionAsync<object>("() => [Symbol('foo4')]"));

        [WebView2ContextFact]
        public async Task ShouldWorkWithUnicodeChars()
            => Assert.Equal(42, await DevToolsContext.EvaluateFunctionAsync<int>("a => a['中文字符']", new Dictionary<string, int> { ["中文字符"] = 42 }));

        [WebView2ContextFact(Skip = "Crashes WebView2")]
        public async Task ShouldThrowWhenEvaluationTriggersReload()
        {
            var exception = await Assert.ThrowsAsync<WebView2DevToolsEvaluationFailedException>(() =>
            {
                return DevToolsContext.EvaluateFunctionAsync(@"() => {
                    location.reload();
                    return new Promise(() => {});
                }");
            });

            Assert.Contains("Protocol error", exception.Message);
        }

        [WebView2ContextFact]
        public async Task ShouldWorkRightAfterFrameNavigated()
        {
            Task<int> frameEvaluation = null;

            DevToolsContext.FrameNavigated += (_, e) =>
            {
                frameEvaluation = e.Frame.EvaluateFunctionAsync<int>("() => 6 * 7");
            };

            await WebView.CoreWebView2.NavigateToAsync(TestConstants.EmptyPage);
            Assert.Equal(42, await frameEvaluation);
        }

        [WebView2ContextFact]
        public async Task ShouldWorkFromInsideAnExposedFunction()
        {
            await DevToolsContext.ExposeFunctionAsync("callController", async (int a, int b) =>
            {
                return await DevToolsContext.EvaluateFunctionAsync<int>("(a, b) => a * b", a, b);
            });
            var result = await DevToolsContext.EvaluateFunctionAsync<int>(@"async function() {
                return await callController(9, 3);
            }");
            Assert.Equal(27, result);
        }

        [WebView2ContextFact]
        public async Task ShouldRejectPromiseWithExeption()
        {
            var exception = await Assert.ThrowsAsync<WebView2DevToolsEvaluationFailedException>(() =>
            {
                return DevToolsContext.EvaluateFunctionAsync("() => not_existing_object.property");
            });

            Assert.Contains("not_existing_object", exception.Message);
        }

        [WebView2ContextFact]
        public async Task ShouldSupportThrownStringsAsErrorMessages()
        {
            var exception = await Assert.ThrowsAsync<WebView2DevToolsEvaluationFailedException>(
                () => DevToolsContext.EvaluateExpressionAsync("throw 'qwerty'"));
            Assert.Contains("qwerty", exception.Message);
        }

        [WebView2ContextFact]
        public async Task ShouldSupportThrownNumbersAsErrorMessages()
        {
            var exception = await Assert.ThrowsAsync<WebView2DevToolsEvaluationFailedException>(
                            () => DevToolsContext.EvaluateExpressionAsync("throw 100500"));
            Assert.Contains("100500", exception.Message);
        }

        [WebView2ContextFact]
        public async Task SouldReturnComplexObjects()
        {
            dynamic obj = new
            {
                stringValue = "bar!",
                boolValTrue = true,
                boolValFalse = false,
                numVal = 123,
                arrVal = new[] { 1, 2, 3, }
            };
            var result = await DevToolsContext.EvaluateFunctionAsync<object>("a => a", obj);

            Assert.Equal("bar!", result.stringValue);
            Assert.Equal(true, result.boolValTrue);
            Assert.Equal(false, result.boolValFalse);
            Assert.Equal(123, result.numVal);

            var arrVal = (object[])result.arrVal;

            Assert.Equal(1d, arrVal[0]);
            Assert.Equal(2d, arrVal[1]);
            Assert.Equal(3d, arrVal[2]);
        }

        [WebView2ContextFact]
        public async Task ShouldReturnBigInt()
        {
            var result = await DevToolsContext.EvaluateFunctionAsync<object>("() => BigInt(42)");
            Assert.Equal(new BigInteger(42), result);
        }

        [WebView2ContextTheory]
        [InlineData("() => NaN", double.NaN)] //ShouldReturnNaN
        [InlineData("() => -0", -0)] //ShouldReturnNegative0
        [InlineData("() => Infinity", double.PositiveInfinity)] //ShouldReturnInfinity
        [InlineData("() => -Infinity", double.NegativeInfinity)] //ShouldReturnNegativeInfinty
        public async Task BasicEvaluationTest(string script, object expected)
        {
            var result = await DevToolsContext.EvaluateFunctionAsync<object>(script);
            Assert.Equal(expected, result);
        }

        [WebView2ContextFact]
        public async Task ShouldAcceptNullAsOneOfMultipleParameters()
        {
            var result = await DevToolsContext.EvaluateFunctionAsync<bool>(
                "(a, b) => Object.is(a, null) && Object.is(b, 'foo')",
                null,
                "foo");
            Assert.True(result);
        }

        [WebView2ContextFact]
        public async Task ShouldReturnNullForNonSerializableObjects()
            => Assert.Null(await DevToolsContext.EvaluateFunctionAsync<object>("() => window"));

        [WebView2ContextFact]
        public async Task ShouldFailForCircularObject()
        {
            var result = await DevToolsContext.EvaluateFunctionAsync<object>(@"() => {
                const a = {};
                const b = {a};
                a.b = b;
                return a;
            }");

            Assert.Null(result);
        }

        [WebView2ContextFact]
        public async Task ShouldBeAbleToThrowATrickyError()
        {
            var windowHandle = await DevToolsContext.EvaluateFunctionHandleAsync("() => window");
            WebView2DevToolsContextException exception = await Assert.ThrowsAsync<WebView2DevToolsContextException>(() => windowHandle.JsonValueAsync());
            var errorText = exception.Message;

            exception = await Assert.ThrowsAsync<WebView2DevToolsEvaluationFailedException>(() => DevToolsContext.EvaluateFunctionAsync(@"errorText =>
            {
                throw new Error(errorText);
            }", errorText));
            Assert.Contains(errorText, exception.Message);
        }

        [WebView2ContextTheory]
        [InlineData("1 + 2;", 3)]
        [InlineData("1 + 5;", 6)]
        [InlineData("2 + 5\n// do some math!'", 7)]
        public async Task BasicIntExressionEvaluationTest(string script, object expected)
        {
            var result = await DevToolsContext.EvaluateExpressionAsync<int>(script);
            Assert.Equal(expected, result);
        }

        [WebView2ContextFact]
        public async Task ShouldAcceptElementHandleAsAnArgument()
        {
            await DevToolsContext.SetContentAsync("<section>42</section>");
            var element = await DevToolsContext.QuerySelectorAsync("section");
            var text = await DevToolsContext.EvaluateFunctionAsync<string>("e => e.textContent", element);
            Assert.Equal("42", text);
        }

        [WebView2ContextFact]
        public async Task ShouldThrowIfUnderlyingElementWasDisposed()
        {
            await DevToolsContext.SetContentAsync("<section>39</section>");
            var element = await DevToolsContext.QuerySelectorAsync("section");
            Assert.NotNull(element);
            await element.DisposeAsync();
            var exception = await Assert.ThrowsAsync<WebView2DevToolsEvaluationFailedException>(()
                => DevToolsContext.EvaluateFunctionAsync<string>("e => e.textContent", element));
            Assert.Contains("HtmlElement is disposed", exception.Message);
        }

        [WebView2ContextFact()]
        public async Task ShouldThrowIfElementHandlesAreFromOtherFrames()
        {
            const string expected = "RemoteObjects can be evaluated only in the context they were created!";

            await FrameUtils.AttachFrameAsync(DevToolsContext, "frame1", TestConstants.EmptyPage);
            var bodyHandle = await DevToolsContext.FirstChildFrame().QuerySelectorAsync("body");
            var exception = await Assert.ThrowsAsync<WebView2DevToolsEvaluationFailedException>(()
                => DevToolsContext.EvaluateFunctionAsync<string>("body => body.innerHTML", bodyHandle));

            Assert.Contains(expected, exception.Message);
        }

        [WebView2ContextFact]
        public async Task ShouldSimulateAUserGesture()
            => Assert.True(await DevToolsContext.EvaluateFunctionAsync<bool>(@"() => {
                document.body.appendChild(document.createTextNode('test'));
                document.execCommand('selectAll');
                return document.execCommand('copy');
            }"));

        [WebView2ContextFact(Skip = "Unable to get meaningful error https://github.com/MicrosoftEdge/WebView2Feedback/issues/1609")]
        public async Task ShouldThrowANiceErrorAfterANavigation()
        {
            var executionContext = await DevToolsContext.MainFrame.GetExecutionContextAsync();

            await Task.WhenAll(
                WebView.CoreWebView2.WaitForNavigationAsync(),
                executionContext.EvaluateFunctionAsync("() => window.location.reload()")
            );
            var ex = await Assert.ThrowsAsync<WebView2DevToolsEvaluationFailedException>(() =>
            {
                return executionContext.EvaluateFunctionAsync("() => null");
            });
            Assert.Contains("navigation", ex.Message);
        }

        [WebView2ContextFact]
        public async Task ShouldNotThrowAnErrorWhenEvaluationDoesANavigation()
        {
            await WebView.CoreWebView2.NavigateToAsync(TestConstants.ServerUrl + "/one-style.html");
            var result = await DevToolsContext.EvaluateFunctionAsync<int[]>(@"() =>
            {
                window.location = '/empty.html';
                return [42];
            }");
            Assert.Equal(new[] { 42 }, result);
        }

        [WebView2ContextFact]
        public async Task ShouldTransfer100MbOfDataFromPage()
        {
            var a = await DevToolsContext.EvaluateFunctionAsync<string>("() => Array(100 * 1024 * 1024 + 1).join('a')");
            Assert.Equal(100 * 1024 * 1024, a.Length);
        }

        [WebView2ContextFact]
        public async Task ShouldThrowErrorWithDetailedInformationOnExceptionInsidePromise()
        {
            var exception = await Assert.ThrowsAsync<WebView2DevToolsEvaluationFailedException>(() =>
                DevToolsContext.EvaluateFunctionAsync(
                    @"() => new Promise(() => {
                        throw new Error('Error in promise');
                    })"));
            Assert.Contains("Error in promise", exception.Message);
        }

        [WebView2ContextFact(Skip = "TODO: Fix this")]
        public Task ShouldWorkWithDifferentSerializerSettings()
        {
            throw new NotImplementedException();

            //var result = await Page.EvaluateFunctionAsync<ComplexObjectTestClass>("() => { return { foo: 'bar' }}");
            //Assert.Equal("bar", result.Foo);

            //result = (await Page.EvaluateFunctionAsync<JsonElement>("() => { return { Foo: 'bar' }}"))
            //    .ToObject<ComplexObjectTestClass>();
            //Assert.Equal("bar", result.Foo);

            //result = await Page.EvaluateExpressionAsync<ComplexObjectTestClass>("var obj = { foo: 'bar' }; obj;");
            //Assert.Equal("bar", result.Foo);

            //result = (await Page.EvaluateExpressionAsync<JsonElement>("var obj = { Foo: 'bar' }; obj;"))
            //    .ToObject<ComplexObjectTestClass>();
            //Assert.Equal("bar", result.Foo);
        }

        [WebView2ContextFact]
        public async Task ShouldProperlySerializeNullFields()
        {
            var result = await DevToolsContext.EvaluateFunctionAsync<Dictionary<string, object>>("() => ({a: null})");
            Assert.True(result.ContainsKey("a"));
            Assert.Null(result["a"]);
        }

        [WebView2ContextFact]
        public async Task ShouldAcceptObjectHandleAsAnArgument()
        {
            await DevToolsContext.SetContentAsync("<section>42</section>");
            var element = await DevToolsContext.QuerySelectorAsync("section");
            var text = await DevToolsContext.EvaluateFunctionAsync<string>("(e) => e.textContent", element);
            Assert.Equal("42", text);
        }

        [WebView2ContextFact(Skip = "TODO: Fix me")]
        public Task ShouldWorkWithoutGenerics()
        {
            throw new System.NotImplementedException();
            //Assert.NotNull(await Page.EvaluateExpressionAsync("var obj = {}; obj;"));
            //Assert.NotNull(await Page.EvaluateExpressionAsync("[]"));
            //Assert.NotNull(await Page.EvaluateExpressionAsync("''"));

            //var objectPopulated = await Page.EvaluateExpressionAsync("var obj = {a:1}; obj;");
            //Assert.NotNull(objectPopulated);
            //Assert.Equal(1, objectPopulated["a"]);

            //var arrayPopulated = await Page.EvaluateExpressionAsync("[1]");
            //Assert.IsType<JArray>(arrayPopulated);
            //Assert.Equal(1, ((JArray)arrayPopulated)[0]);

            //Assert.Equal("1", await Page.EvaluateExpressionAsync("'1'"));
            //Assert.Equal(1, await Page.EvaluateExpressionAsync("1"));
            //Assert.Equal(11111111, await Page.EvaluateExpressionAsync("11111111"));
            //Assert.Equal(11111111111111, await Page.EvaluateExpressionAsync("11111111111111"));
            //Assert.Equal(1.1, await Page.EvaluateExpressionAsync("1.1"));
        }

        public class ComplexObjectTestClass
        {
            public string Foo { get; set; }
        }
    }
}
