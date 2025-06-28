using System;
using System.Collections.Generic;
using WebView2.DevTools.Dom.Mobile;

namespace WebView2.DevTools.Dom
{
    /// <summary>
    /// Device Emulation
    /// </summary>
    public static class Emulate
    {
        private static readonly Lazy<IReadOnlyDictionary<DeviceName, DeviceDescriptor>> _devices
            = new Lazy<IReadOnlyDictionary<DeviceName, DeviceDescriptor>>(() => DeviceDescriptors.ToReadOnly());

        /// <summary>
        /// Returns a <see cref="DeviceDescriptor"/>  for the given name
        /// </summary>
        /// <param name="name">Device Name</param>
        /// <returns>Device Device Descriptor</returns>
        /// <example>
        /// <code>
        ///<![CDATA[
        /// var iPhone = Emulate.Device(DeviceDescriptorName.IPhone6);
        /// await devToolsContext.EmulateAsync(iPhone);
        /// ]]>
        /// </code>
        /// </example>
        public static DeviceDescriptor Device(DeviceName name)
        {
            return _devices.Value[name];
        }
    }
}
