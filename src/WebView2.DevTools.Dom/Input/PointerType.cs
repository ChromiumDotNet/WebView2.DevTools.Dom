using System.Runtime.Serialization;

namespace WebView2.DevTools.Dom.Input
{
    internal enum PointerType
    {
        [EnumMember(Value = "mouse")]
        Mouse,
        [EnumMember(Value = "pen")]
        Pen,
    }
}
