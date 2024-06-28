using System;

namespace Beamable.Common
{
    [Serializable]
    public class FirebaseMessageRequest
    {
        public string token;
        public string title;
        public string body;
    }
}