using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Beamable;
using Beamable.Experimental.Api.Chat;
using Beamable.Server.Clients;
using TMPro;
using UnityEngine;

public class ChatRoom : MonoBehaviour
{
    private string _roomName;
    private BeamContext _beamContext;
    private BeamContext _beamContext01;
    
    private ChatService _chatService;
    private ChatService _chatService01;
    
    private RoomHandle _currentRoom;
    private RoomHandle _currentRoom01;

    private UserServiceClient _userService;
    private List<Message> _activeGroupChatMessages;
    private ChatView ChatView { get; set; }
    private ChatView ChatView01 { get; set; }
    
    [SerializeField] 
    private TMP_Text roomNameText;
    [SerializeField]
    private TMP_Text chatLogText;
    [SerializeField]
    private TMP_InputField messageInput;

    private async void Start()
    {
        _beamContext = await BeamContext.Default.Instance;
        _beamContext01 = await BeamContext.ForPlayer("MyPlayer02").Instance;

        _chatService = _beamContext.ServiceProvider.GetService<ChatService>();
        _chatService01 = _beamContext01.ServiceProvider.GetService<ChatService>();
        
        _userService = new UserServiceClient();

        _roomName = PlayerPrefs.GetString("SelectedRoomName", string.Empty);
        roomNameText.text = _roomName;
        
        _chatService = _beamContext.ServiceProvider.GetService<ChatService>();
        _chatService.Subscribe(HandleChatViewUpdate);
        
        _chatService01 = _beamContext01.ServiceProvider.GetService<ChatService>();
        _chatService01.Subscribe(HandleChatViewUpdateForGuest);       
    }
    
    private void HandleChatViewUpdate(ChatView chatView)
    {
        ChatView = chatView;
        _currentRoom = _roomName == "General" ? ChatView.GuildRooms.LastOrDefault() : ChatView.roomHandles.Find(x =>
        {
            Debug.Log(x.Name);
            foreach (var player in x.Players)
            {
                Debug.Log(player);
            }
            return x.Name == _roomName;
        });

        _currentRoom?.Subscribe().Then(_ => HandleGroupChatRoomUpdate(_currentRoom));
    }
    
    private async void HandleChatViewUpdateForGuest(ChatView chatView)
    {
        ChatView01 = chatView;
        var roomInfos = await _chatService01.GetMyRooms();
        if (roomInfos != null)
        {
            foreach (var room in roomInfos)
            {
                Debug.Log("Guest Player Room: " + room.name);
            }
        }
        else
        {
            Debug.LogError("Guest Player Room Infos are null");
        }
        _currentRoom01 = _roomName == "General" ? ChatView01.GuildRooms.LastOrDefault() : ChatView01.roomHandles.Find(x => x.Name == _roomName);
        
        if (_currentRoom01 != null)
        { 
            StartCoroutine(SendGuestMessageAfterDelay(5f, "Hello from guest player!"));
        }
        else
        {
            Debug.LogError("Room not found for the guest player.");
        }

    }
    
    private void HandleGroupChatRoomUpdate(RoomHandle groupChatRoomHandle)
    {
        _activeGroupChatMessages = groupChatRoomHandle.Messages;
        if (_activeGroupChatMessages == null || _activeGroupChatMessages.Count == 0)
            _activeGroupChatMessages = new List<Message>();
        
        groupChatRoomHandle.OnMessageReceived += OnMessageReceived;
        LoadChatHistory();
    }
    
    private async void OnMessageReceived(Message message)
    {
        var username = await _userService.GetPlayerAvatarName(message.gamerTag);
        string roomMessage = $"{username.data}: {message.content}";
        chatLogText.text += $"{roomMessage}\n";
    }
    
    private async void LoadChatHistory()
    {
        chatLogText.text = "";
        foreach (var message in _currentRoom.Messages)
        {
            var username = await _userService.GetPlayerAvatarName(message.gamerTag);

            string roomMessage = $"{username.data}: {message.content}";
            chatLogText.text += $"{roomMessage}\n";
        }
    }
    
    
    public void SendMessage()
    {
        if (_currentRoom != null && !string.IsNullOrEmpty(messageInput.text))
        {
            _currentRoom.SendMessage(messageInput.text);
            messageInput.text = "";
        }
    }
    
    private IEnumerator SendGuestMessageAfterDelay(float delay, string message)
    {
        yield return new WaitForSeconds(delay);

        if (_currentRoom01 != null)
        {
            _currentRoom01.SendMessage(message);
        }
    }
}
