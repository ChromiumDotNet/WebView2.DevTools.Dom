using System.Threading.Tasks;
using WebView2.DevTools.Dom.Tests.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace WebView2.DevTools.Dom.Tests.JSHandleTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class GetPropertiesTests : DevTooolsContextBaseTest
    {
        public GetPropertiesTests(ITestOutputHelper output) : base(output)
        {
        }

        [WebView2ContextFact]
        public async Task ShouldWork()
        {
            var aHandle = await DevToolsContext.EvaluateExpressionHandleAsync(@"({
              foo: 'bar'
            })");
            var properties = await aHandle.GetPropertiesAsync();
            properties.TryGetValue("foo", out var foo);
            Assert.NotNull(foo);
            Assert.Equal("bar", await foo.GetValueAsync<string>());
        }

        [WebView2ContextFact]
        public async Task ShouldWorkForNullableIntEmptyString()
        {
            var aHandle = await DevToolsContext.EvaluateExpressionHandleAsync(@"({
              foo: 'bar',
              prop2: ''
            })");
            var properties = await aHandle.GetPropertiesAsync();
            properties.TryGetValue("prop2", out var foo);
            Assert.NotNull(foo);
            Assert.Null(await foo.GetValueAsync<int?>());
        }

        [WebView2ContextFact]
        public async Task ShouldWorkForNullableInt()
        {
            var aHandle = await DevToolsContext.EvaluateExpressionHandleAsync(@"({
              foo: 'bar',
              prop2: '123'
            })");
            var properties = await aHandle.GetPropertiesAsync();
            properties.TryGetValue("prop2", out var foo);
            Assert.NotNull(foo);
            Assert.Equal(123, await foo.GetValueAsync<int?>());
        }

        [WebView2ContextFact]
        public async Task ShouldForIntWork()
        {
            var aHandle = await DevToolsContext.EvaluateExpressionHandleAsync(@"({
              foo: 'bar',
              prop2: '123'
            })");
            var properties = await aHandle.GetPropertiesAsync();
            properties.TryGetValue("prop2", out var foo);
            Assert.NotNull(foo);
            Assert.Equal(123, await foo.GetValueAsync<int>());
        }

        [WebView2ContextFact]
        public async Task ShouldReturnEvenNonOwnProperties()
        {
            var aHandle = await DevToolsContext.EvaluateFunctionHandleAsync(@"() => {
              class A {
                constructor() {
                  this.a = '1';
                }
              }
              class B extends A {
                constructor() {
                  super();
                  this.b = '2';
                }
              }
              return new B();
            }");
            var properties = await aHandle.GetPropertiesAsync();
            Assert.Equal("1", await properties["a"].GetValueAsync<string>());
            Assert.Equal("2", await properties["b"].GetValueAsync<string>());
        }
    }
}
