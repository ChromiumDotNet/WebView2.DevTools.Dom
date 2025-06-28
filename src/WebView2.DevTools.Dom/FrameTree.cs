using System.Collections.Generic;
using Microsoft.Web.WebView2.Core.DevToolsProtocolExtension;

namespace WebView2.DevTools.Dom
{
    internal class FrameTree
    {
        internal FrameTree() => Childs = new List<FrameTree>();

        internal FrameTree(Page.FrameTree frameTree)
        {
            Frame = frameTree.Frame;

            Childs = new List<FrameTree>();
            LoadChildren(this, frameTree);
        }

        internal Page.Frame Frame { get; set; }

        internal List<FrameTree> Childs { get; set; }

        private void LoadChildren(FrameTree frame, Page.FrameTree frameTree)
        {
            var childFrames = frameTree.ChildFrames;

            if (childFrames != null)
            {
                foreach (var item in childFrames)
                {
                    var newFrame = new FrameTree(item);

                    LoadChildren(newFrame, item);
                    frame.Childs.Add(newFrame);
                }
            }
        }
    }
}
