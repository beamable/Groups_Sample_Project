using System;
using Beamable.Common.Interfaces;
using Beamable.Server;

namespace Beamable.Common.Models
{
    [Serializable]
    public class InviteData : StorageDocument, ISetStorageDocument<InviteData>
    {
        public long gamerTag;
        public long groupId;

        public void Set(InviteData document)
        {
            gamerTag = document.gamerTag;
            groupId = document.groupId;
        }
    }
}