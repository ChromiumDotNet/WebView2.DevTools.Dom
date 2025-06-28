using System.Runtime.Serialization;

namespace WebView2.DevTools.Dom
{
    /// <summary>
    /// An enumeration with possible values on how to compare boundary points.
    /// </summary>
    public enum RangeType : byte
    {
        /// <summary>
        /// From the start to the start (periodic).
        /// </summary>
        [EnumMember(Value = "START_TO_START")]
        StartToStart = 0,
        /// <summary>
        /// From the start to the end (non-periodic).
        /// </summary>
        [EnumMember(Value = "START_TO_END")]
        StartToEnd = 1,
        /// <summary>
        /// From the end to the end (periodic).
        /// </summary>
        [EnumMember(Value = "END_TO_END")]
        EndToEnd = 2,
        /// <summary>
        /// From the end to the start (non-periodic).
        /// </summary>
        [EnumMember(Value = "END_TO_START")]
        EndToStart = 3
    }
}
