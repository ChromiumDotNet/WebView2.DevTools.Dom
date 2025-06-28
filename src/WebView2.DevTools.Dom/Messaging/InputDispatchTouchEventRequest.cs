using WebView2.DevTools.Dom.Input;

namespace WebView2.DevTools.Dom.Messaging
{
    internal class InputDispatchTouchEventRequest
    {
        public string Type { get; internal set; }

        public TouchPoint[] TouchPoints { get; set; }

        public int Modifiers { get; internal set; }
    }
}
