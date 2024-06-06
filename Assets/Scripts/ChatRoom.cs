using System.Collections;
using System.Collections.Generic;
using Beamable;
using Beamable.Common;
using Beamable.Experimental.Api.Chat;
using TMPro;
using UnityEngine;

public class ChatRoom : MonoBehaviour
{
    
    private Promise<BeamContext> _beamContext;
    private ChatService _chatService;
    private RoomHandle _currentRoom;

    [SerializeField] 
    private TMP_Text roomNameText;

    void Start()
    {
        _beamContext = BeamContext.Default.Instance;
        
        var roomName = PlayerPrefs.GetString("SelectedRoomName", string.Empty);
        roomNameText.text = roomName;
    }

}
