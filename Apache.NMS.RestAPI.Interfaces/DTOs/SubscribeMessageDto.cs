using System;

namespace Apache.NMS.RestAPI.Interfaces.DTOs
{
    public class SubscribeMessageDto
    {
        public string Destination { get; set; }
        public int NumberOfEvents { get; set; }
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    }
}