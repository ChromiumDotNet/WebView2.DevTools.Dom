using System;
using System.Runtime.Serialization;

namespace WebView2.DevTools.Dom
{
    /// <summary>
    /// Enumeration of possible document position values.
    /// </summary>
    [Flags]
    public enum DocumentPositions : byte
    {
        /// <summary>
        /// It is the same node.
        /// </summary>
        Same = 0,
        /// <summary>
        /// There is no relation.
        /// </summary>
        [EnumMember(Value = "DOCUMENT_POSITION_DISCONNECTED")]
        Disconnected = 0x01,
        /// <summary>
        /// The node preceeds the other element.
        /// </summary>
        [EnumMember(Value = "DOCUMENT_POSITION_PRECEDING")]
        Preceding = 0x02,
        /// <summary>
        /// The node follows the other element.
        /// </summary>
        [EnumMember(Value = "DOCUMENT_POSITION_FOLLOWING")]
        Following = 0x04,
        /// <summary>
        /// The node contains the other element.
        /// </summary>
        [EnumMember(Value = "DOCUMENT_POSITION_CONTAINS")]
        Contains = 0x08,
        /// <summary>
        /// The node is contained in the other element.
        /// </summary>
        [EnumMember(Value = "DOCUMENT_POSITION_CONTAINED_BY")]
        ContainedBy = 0x10,
        /// <summary>
        /// The relation is implementation specific.
        /// </summary>
        [EnumMember(Value = "DOCUMENT_POSITION_IMPLEMENTATION_SPECIFIC")]
        ImplementationSpecific = 0x20
    }
}
