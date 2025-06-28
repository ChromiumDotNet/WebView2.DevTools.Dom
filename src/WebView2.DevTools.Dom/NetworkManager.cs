using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core.DevToolsProtocolExtension;

namespace WebView2.DevTools.Dom
{
    internal class NetworkManager
    {
        private readonly DevToolsProtocolHelper _client;

        private readonly InternalNetworkConditions _emulatedNetworkConditions = new InternalNetworkConditions
        {
            Offline = false,
            Upload = -1,
            Download = -1,
            Latency = 0,
        };

        internal NetworkManager(DevToolsProtocolHelper client)
        {
            _client = client;
        }

        internal async Task EmulateNetworkConditionsAsync(NetworkConditions networkConditions)
        {
            _emulatedNetworkConditions.Upload = networkConditions?.Upload ?? -1;
            _emulatedNetworkConditions.Download = networkConditions?.Download ?? -1;
            _emulatedNetworkConditions.Latency = networkConditions?.Latency ?? 0;
            await UpdateNetworkConditionsAsync().ConfigureAwait(true);
        }

        private Task UpdateNetworkConditionsAsync()
            => _client.Network.EmulateNetworkConditionsAsync(_emulatedNetworkConditions.Offline, _emulatedNetworkConditions.Latency, _emulatedNetworkConditions.Download, _emulatedNetworkConditions.Upload);
    }
}
