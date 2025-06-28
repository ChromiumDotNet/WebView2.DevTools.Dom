using System;
using System.Threading.Tasks;

namespace WebView2.DevTools.Dom
{
    internal interface IWaitTask
    {
        Task Rerun();

        void Terminate(Exception exception);
    }
}
