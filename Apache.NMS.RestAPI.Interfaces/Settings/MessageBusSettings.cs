namespace Apache.NMS.RestAPI.Interfaces.Settings
{
    public class MessageBusSettings
    {
        public string ServerUrl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Destination { get; set; }
    }
}