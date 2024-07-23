using UnityEngine;

namespace Beamable.Common.Models
{
    public sealed class Payload
    {
        [SerializeField] private PayloadType _type;
        [SerializeField] private string _json;

        public Payload(PayloadType type, string json)
        {
            _type = type;
            _json = json;
        }
        
        public PayloadType Type => _type;
        public string Json => _json;
    }
}