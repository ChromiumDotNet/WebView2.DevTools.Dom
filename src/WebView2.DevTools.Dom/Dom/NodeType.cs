using System.Runtime.Serialization;

namespace WebView2.DevTools.Dom
{
    /// <summary>
    /// Contains an enumeration of various node types.
    /// </summary>
    public enum NodeType : byte
    {
        /// <summary>
        /// A standard node element.
        /// </summary>
        Element = 1,
        /// <summary>
        /// An attribute node.
        /// </summary>
        Attribute = 2,
        /// <summary>
        /// A text node.
        /// </summary>
        Text = 3,
        /// <summary>
        /// A CData text node.
        /// </summary>
        CharacterData = 4,
        /// <summary>
        /// An entity reference node.
        /// </summary>
        EntityReference = 5,
        /// <summary>
        /// An entity node.
        /// </summary>
        Entity = 6,
        /// <summary>
        /// A processing instruction node.
        /// </summary>
        ProcessingInstruction = 7,
        /// <summary>
        /// A comment node.
        /// </summary>
        Comment = 8,
        /// <summary>
        /// A document node.
        /// </summary>
        Document = 9,
        /// <summary>
        /// A document type node.
        /// </summary>
        DocumentType = 10,
        /// <summary>
        /// A document (fragment mode) node.
        /// </summary>
        DocumentFragment = 11,
        /// <summary>
        /// A notation node.
        /// </summary>
        Notation = 12
    }
}
