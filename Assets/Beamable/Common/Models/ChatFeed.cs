using UnityEngine;

namespace Beamable.Common.Models
{
    public class ChatFeed
    {
        [SerializeField] private long _sender;
        [SerializeField] private string _message;
        [SerializeField] private string _dateTimeUtc;
        
        public ChatFeed(long sender, string message, string dateTimeUtc)
        {
            _sender = sender;
            _message = message;
            _dateTimeUtc = dateTimeUtc;
        }
        
        public long Sender => _sender;
        public string Message => _message;
        public string Time => _dateTimeUtc;
    }
}