using System.Threading.Tasks;
using WebView2.DevTools.Dom.Tests.Attributes;
using Xunit;
using Xunit.Abstractions;

namespace WebView2.DevTools.Dom.Tests.InputTests
{
    [Collection(TestConstants.TestFixtureCollectionName)]
    public class FileChooserOpenedTests : DevTooolsContextBaseTest
    {
        const int RenderDelay = 1000;
        public FileChooserOpenedTests(ITestOutputHelper output) : base(output)
        {
        }

        [WebView2ContextFact]
        public async Task ShouldWorkForSingleFilePick()
        {
            await DevToolsContext.SetInterceptFileChooserDialogAsync(true);
            await DevToolsContext.SetContentAsync("<input type=file>");
            await WebView.CoreWebView2.WaitForRenderIdle();

            var evt = await Assert.RaisesAsync<FileChooserOpenedEventArgs>(
                    x => DevToolsContext.FileChooserOpened += x,
                    y => DevToolsContext.FileChooserOpened -= y,
                    async () => {
                        await DevToolsContext.ClickAsync("input");
                        // Generate a frame to allow time for the event to fire
                        await DevToolsContext.EvaluateFunctionAsync("new Promise(x => requestAnimationFrame(() => requestAnimationFrame(x)))");
                    });

            Assert.False(evt.Arguments.Multiple);
        }

        [WebView2ContextFact]
        public async Task ShouldWorkForMultiple()
        {
            await DevToolsContext.SetInterceptFileChooserDialogAsync(true);
            await DevToolsContext.SetContentAsync("<input type=file multiple>");
            await WebView.CoreWebView2.WaitForRenderIdle();

            var evt = await Assert.RaisesAsync<FileChooserOpenedEventArgs>(
                    x => DevToolsContext.FileChooserOpened += x,
                    y => DevToolsContext.FileChooserOpened -= y,
                    async () => {
                        await DevToolsContext.ClickAsync("input");
                        // Generate a frame to allow time for the event to fire
                        await DevToolsContext.EvaluateFunctionAsync("new Promise(x => requestAnimationFrame(() => requestAnimationFrame(x)))");
                    });

            Assert.True(evt.Arguments.Multiple);
        }

        [WebView2ContextFact]
        public async Task ShouldWorkForWebkitDirectory()
        {
            await DevToolsContext.SetInterceptFileChooserDialogAsync(true);
            await DevToolsContext.SetContentAsync("<input type=file multiple webkitdirectory>");
            await WebView.CoreWebView2.WaitForRenderIdle();

            var evt = await Assert.RaisesAsync<FileChooserOpenedEventArgs>(
                    x => DevToolsContext.FileChooserOpened += x,
                    y => DevToolsContext.FileChooserOpened -= y,
                    async () => {
                        await DevToolsContext.ClickAsync("input");
                        // Generate a frame to allow time for the event to fire
                        await DevToolsContext.EvaluateFunctionAsync("new Promise(x => requestAnimationFrame(() => requestAnimationFrame(x)))");
                    });

            Assert.True(evt.Arguments.Multiple);
        }

        [WebView2ContextFact]
        public async Task ShouldAcceptSingleFile()
        {
            await DevToolsContext.SetInterceptFileChooserDialogAsync(true);
            await DevToolsContext.SetContentAsync("<input type=file oninput='javascript:console.timeStamp()'>");
            await WebView.CoreWebView2.WaitForRenderIdle();

            var evt = await Assert.RaisesAsync<FileChooserOpenedEventArgs>(
                    x => DevToolsContext.FileChooserOpened += x,
                    y => DevToolsContext.FileChooserOpened -= y,
                    async () => {
                        await DevToolsContext.ClickAsync("input");
                        // Generate a frame to allow time for the event to fire
                        await DevToolsContext.EvaluateFunctionAsync("new Promise(x => requestAnimationFrame(() => requestAnimationFrame(x)))");
                    });

            var element = evt.Arguments.Element;

            await element.UploadFileAsync(TestConstants.FileToUpload);

            var files = await element.GetFilesAsync().ToArrayAsync();
            var fileName = await files[0].GetNameAsync();

            Assert.NotEmpty(files);
            Assert.Equal("file-to-upload.txt", fileName);
        }

        [WebView2ContextFact]
        public async Task ShouldBeAbleToReadSelectedFile()
        {
            const string expected = "contents of the file";
            await DevToolsContext.SetInterceptFileChooserDialogAsync(true);
            await DevToolsContext.SetContentAsync("<input type=file>");
            await WebView.CoreWebView2.WaitForRenderIdle();

            var evt = await Assert.RaisesAsync<FileChooserOpenedEventArgs>(
                    x => DevToolsContext.FileChooserOpened += x,
                    y => DevToolsContext.FileChooserOpened -= y,
                    async () => {
                        await DevToolsContext.ClickAsync("input");
                        // Generate a frame to allow time for the event to fire
                        await DevToolsContext.EvaluateFunctionAsync("new Promise(x => requestAnimationFrame(() => requestAnimationFrame(x)))");
                    });

            var element = evt.Arguments.Element;

            await element.UploadFileAsync(TestConstants.FileToUpload);

            var files = await element.GetFilesAsync().ToArrayAsync();

            var actual = await files[0].TextAsync();            

            Assert.Equal(expected, actual);
        }

        [WebView2ContextFact]
        public async Task ShouldBeAbleToResetSelectedFilesWithEmptyFileList()
        {
            await DevToolsContext.SetInterceptFileChooserDialogAsync(true);
            await DevToolsContext.SetContentAsync("<input type=file>");
            await WebView.CoreWebView2.WaitForRenderIdle();

            var evt = await Assert.RaisesAsync<FileChooserOpenedEventArgs>(
                    x => DevToolsContext.FileChooserOpened += x,
                    y => DevToolsContext.FileChooserOpened -= y,
                    async () => {
                        await DevToolsContext.ClickAsync("input");
                        // Generate a frame to allow time for the event to fire
                        await DevToolsContext.EvaluateFunctionAsync("new Promise(x => requestAnimationFrame(() => requestAnimationFrame(x)))");
                    });

            var element = evt.Arguments.Element;

            await element.UploadFileAsync(TestConstants.FileToUpload);

            var filesLength = await element.GetFilesAsync().GetLengthAsync();

            Assert.Equal(1, filesLength);

            await element.UploadFileAsync();

            filesLength = await element.GetFilesAsync().GetLengthAsync();

            Assert.Equal(0, filesLength);
        }

        [WebView2ContextFact]
        public async Task ShouldNotAcceptMultipleFilesForSingleFileInput()
        {
            await DevToolsContext.SetInterceptFileChooserDialogAsync(true);
            await DevToolsContext.SetContentAsync("<input type=file>");
            await WebView.CoreWebView2.WaitForRenderIdle();

            var evt = await Assert.RaisesAsync<FileChooserOpenedEventArgs>(
                    x => DevToolsContext.FileChooserOpened += x,
                    y => DevToolsContext.FileChooserOpened -= y,
                    async () => {
                        await DevToolsContext.ClickAsync("input");
                        // Generate a frame to allow time for the event to fire
                        await DevToolsContext.EvaluateFunctionAsync("new Promise(x => requestAnimationFrame(() => requestAnimationFrame(x)))");
                    });

            var element = evt.Arguments.Element;

            await Assert.ThrowsAsync<WebView2DevToolsContextException>(() => element.UploadFileAsync(
                "./assets/file-to-upload.txt",
                "./assets/pptr.png"));
        }

        [WebView2ContextFact]
        public async Task ShouldFailForNonExistentFiles()
        {
            await DevToolsContext.SetInterceptFileChooserDialogAsync(true);
            await DevToolsContext.SetContentAsync("<input type=file>");
            await WebView.CoreWebView2.WaitForRenderIdle();

            var evt = await Assert.RaisesAsync<FileChooserOpenedEventArgs>(
                    x => DevToolsContext.FileChooserOpened += x,
                    y => DevToolsContext.FileChooserOpened -= y,
                    async () => {
                        await DevToolsContext.ClickAsync("input");
                        // Generate a frame to allow time for the event to fire
                        await DevToolsContext.EvaluateFunctionAsync("new Promise(x => requestAnimationFrame(() => requestAnimationFrame(x)))");
                    });

            var element = evt.Arguments.Element;

            await Assert.ThrowsAsync<WebView2DevToolsContextException>(() => element.UploadFileAsync("file-does-not-exist.txt"));
        }
    }
}
