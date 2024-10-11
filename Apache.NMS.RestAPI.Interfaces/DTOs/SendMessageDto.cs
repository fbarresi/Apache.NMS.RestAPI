namespace Apache.NMS.RestAPI.Interfaces.DTOs
{
    public class SendMessageDto
    {
        public string Destination { get; set; }
        public string Payload { get; set; }

        public override string ToString()
        {
            return $"{nameof(Destination)}: {Destination}, {nameof(Payload)}: {Payload}";
        }
    }
}