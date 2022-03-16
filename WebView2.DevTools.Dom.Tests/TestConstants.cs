using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;

namespace WebView2.DevTools.Dom.Tests
{
public static class TestConstants
{
        public const string TestFixtureCollectionName = "DevToolsContextFixture collection";
        public const int DebuggerAttachedTestTimeout = 300_000;
        public const int DefaultTestTimeout = 30_000;
        public const int DefaultDevToolsTimeout = 10_000;
        public const string ServerUrl = "http://devtools.test";
        public const string ServerDefaultUrl = "http://devtools.test/index.html";
        public const string HttpsPrefix = "https://devtools.test";
        public const string AboutBlank = "about:blank";
        public static readonly string CrossProcessHttpPrefix = "http://devtools2.test";
        public static readonly string EmptyPage = $"{ServerUrl}/empty.html";
        public static readonly string CrossProcessUrl = "http://devtools2.test";

        public static ILoggerFactory LoggerFactory { get; private set; }
        public static string FileToUpload => Path.Combine(AppContext.BaseDirectory, "Assets", "file-to-upload.txt");

        public static readonly IEnumerable<string> NestedFramesDumpResult = new List<string>()
        {
            "http://devtools.test/frames/nested-frames.html",
            "    http://devtools.test/frames/two-frames.html (2frames)",
            "        http://devtools.test/frames/frame.html (uno)",
            "        http://devtools.test/frames/frame.html (dos)",
            "    http://devtools.test/frames/frame.html (aframe)"
        };
    }
}
