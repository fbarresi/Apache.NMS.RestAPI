using System;

namespace Apache.NMS.RestAPI.Interfaces.DTOs
{
    public class RequestMessageDto : SendMessageDto
    {
        public TimeSpan Timeout { get; set; }
        public ReplyTo ReplyToType { get; set; }
        public string ReplyTo { get; set; }

        public override string ToString()
        {
            return
                $"{base.ToString()}, {nameof(Timeout)}: {Timeout}, {nameof(ReplyToType)}: {ReplyToType}, {nameof(ReplyTo)}: {ReplyTo}";
        }
    }
}