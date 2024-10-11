using System.Collections.Generic;

namespace Apache.NMS.RestAPI.Interfaces.Settings
{
    public class BusManagerSettings
    {
        public List<MessageBusSessionSettings> BusSettings { get; set; }
        public Dictionary<string, string> Destinations { get; set; }
    }
}