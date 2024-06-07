using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beamable;
using Beamable.Common;
using Beamable.Experimental.Api.Chat;
using Beamable.Server.Clients;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class ChatRoom : MonoBehaviour
{
    private string _roomName;
    private BeamContext _beamContext;
    private ChatService _chatService;
    private RoomHandle _currentRoom;
    private UserServiceClient _userService;
    private List<Message> _activeGroupChatMessages;
    private ChatView ChatView { get; set; }
    
    [SerializeField] 
    private TMP_Text roomNameText;
    [SerializeField]
    private TMP_Text chatLogText;
    [SerializeField]
    private TMP_InputField messageInput;

    private async void Start()
    {
        _beamContext = await BeamContext.Default.Instance;
        _chatService = _beamContext.ServiceProvider.GetService<ChatService>();
        _userService = new UserServiceClient();

        _roomName = PlayerPrefs.GetString("SelectedRoomName", string.Empty);
        roomNameText.text = _roomName;
        
        _chatService = _beamContext.ServiceProvider.GetService<ChatService>();
        _chatService.Subscribe(HandleChatViewUpdate);
    }
    
    private void HandleChatViewUpdate(ChatView chatView)
    {
        ChatView = chatView;
        _currentRoom = ChatView.roomHandles.Find(x => x.Name == _roomName);
        
        _currentRoom?.Subscribe().Then(_ => HandleGroupChatRoomUpdate(_currentRoom));
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
        Debug.Log($"Message received: {roomMessage}");
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
    

}
