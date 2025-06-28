using System.Text.Json;

namespace WebView2.DevTools.Dom.Messaging
{
    internal class BindingCalledResponsePayload
    {
        public string Name { get; set; }

        public JsonElement[] Args { get; set; }

        public int Seq { get; set; }

        public JsonElement JsonObject { get; set; }
    }
}
