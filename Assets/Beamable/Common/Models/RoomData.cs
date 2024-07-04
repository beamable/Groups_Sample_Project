using System;
using System.Collections.Generic;
using Beamable.Common.Interfaces;
using Beamable.Server;

namespace Beamable.Common.Models
{
    [Serializable]
    public class RoomData : StorageDocument, ISetStorageDocument<RoomData>
    {
        public string roomName;
        public List<long> memberGamerTags = new List<long>();
        public List<MessageData> messages = new List<MessageData>();

        public void Set(RoomData document)
        {
            roomName = document.roomName;
            memberGamerTags = document.memberGamerTags;
            messages = document.messages;
        }
    }
}