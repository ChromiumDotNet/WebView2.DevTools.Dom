using System.Runtime.Serialization;

namespace WebView2.DevTools.Dom.Media
{
    /// <summary>
    /// Media type.
    /// </summary>
    public enum MediaType
    {
        /// <summary>
        /// Media Print.
        /// </summary>
        Print,
        /// <summary>
        /// Media Screen.
        /// </summary>
        Screen,
        /// <summary>
        /// No media set
        /// </summary>
        [EnumMember(Value = "")]
        None
    }
}
