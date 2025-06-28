namespace WebView2.DevTools.Dom
{
    /// <summary>
    /// Optional waiting parameters.
    /// </summary>
    /// <seealso cref="WebView2DevToolsContext.WaitForFunctionAsync(string, WaitForFunctionOptions, object[])"/>
    /// <seealso cref="Frame.WaitForFunctionAsync(string, WaitForFunctionOptions, object[])"/>
    /// <seealso cref="WaitForSelectorOptions"/>
    public class WaitForFunctionOptions
    {
        /// <summary>
        /// Maximum time to wait for in milliseconds. Defaults to 30000 (30 seconds). Pass 0 to disable timeout.
        /// The default value can be changed by setting the <see cref="WebView2DevToolsContext.DefaultTimeout"/> property.
        /// </summary>
        public int? Timeout { get; set; }

        /// <summary>
        /// An interval at which the <c>pageFunction</c> is executed. defaults to <see cref="WaitForFunctionPollingOption.Raf"/>
        /// </summary>
        public WaitForFunctionPollingOption Polling { get; set; } = WaitForFunctionPollingOption.Raf;

        /// <summary>
        /// An interval at which the <c>pageFunction</c> is executed. If no value is specified will use <see cref="Polling"/>
        /// </summary>
        public int? PollingInterval { get; set; }
    }
}