using System;

namespace Beamable.Common.Models
{
    [Serializable]
    public class MessageData
    {
        public long senderGamerTag;
        public string content;
        public DateTime timestamp;
    }
}