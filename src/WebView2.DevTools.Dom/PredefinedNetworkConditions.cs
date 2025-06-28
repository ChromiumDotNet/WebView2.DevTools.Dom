using System.Collections.Generic;

namespace WebView2.DevTools.Dom
{
    /// <summary>
    /// Predefined network conditions.
    /// </summary>
    public static class PredefinedNetworkConditions
    {
        private static readonly Dictionary<string, NetworkConditions> Conditions = new Dictionary<string, NetworkConditions>
        {
            [NetworkConditions.Slow3G] = new NetworkConditions
            {
                Download = ((500 * 1000) / 8) * 0.8,
                Upload = ((500 * 1000) / 8) * 0.8,
                Latency = 400 * 5,
            },
            [NetworkConditions.Fast3G] = new NetworkConditions
            {
                Download = ((1.6 * 1000 * 1000) / 8) * 0.9,
                Upload = ((750 * 1000) / 8) * 0.9,
                Latency = 150 * 3.75,
            },
        };

        /// <summary>
        /// Get Network condition
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>network condition</returns>
        public static NetworkConditions GetNetworkCondition(string key)
        {
            return Conditions[key];
        }
    }
}
