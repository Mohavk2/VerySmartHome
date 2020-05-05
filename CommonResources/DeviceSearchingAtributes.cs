using System;
using System.Collections.Generic;
using System.Text;

namespace CommonLibrary
{
    public struct DeviceSearchingAtributes
    {
        public string SsdpMessage { get; set; }
        public string DeviceType { get; set; }
        public int MulticastPort { get; set; }
    }
}
