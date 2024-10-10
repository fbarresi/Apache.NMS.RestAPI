namespace Apache.NMS.RestAPI.Interfaces.Settings
{
    public class MessageBusSettings
    {
        public string ServerUrl { get; set; } = "ems:tcp://EMSAP-RBG01.germany.osram-os.com:7222";
        public string Username { get; set; } = "admin";
        public string Password { get; set; } = "";
        public string Destination { get; set; } = "queue://abc";
    }
}