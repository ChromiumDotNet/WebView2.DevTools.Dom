using System.Collections.Generic;
using Microsoft.Web.WebView2.Core.DevToolsProtocolExtension;

namespace WebView2.DevTools.Dom.Messaging
{
    internal class RuntimeCallFunctionOnRequest
    {
        public string FunctionDeclaration { get; set; }

        public string ObjectId { get; set; }

        public IEnumerable<Runtime.CallArgument> Arguments { get; set; }

        public bool ReturnByValue { get; set; }

        public bool UserGesture { get; set; }

        public bool AwaitPromise { get; set; }

        public int? ExecutionContextId { get; set; }
    }
}
