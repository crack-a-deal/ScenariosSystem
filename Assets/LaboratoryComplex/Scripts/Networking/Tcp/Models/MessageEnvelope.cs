using System;

namespace Networking.Request
{
    public class MessageEnvelope
    {
        public Guid RequestId { get; set; }
        public MessageType Type { get; set; }
        public string Contract { get; set; }
        public string Payload { get; set; }
    }
}
