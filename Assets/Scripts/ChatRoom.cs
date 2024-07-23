using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Beamable;
using Beamable.Common.Models;
using Beamable.Experimental.Api.Chat;
using Beamable.Server.Clients;
using TMPro;
using Unity.Notifications.Android;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChatRoom : MonoBehaviour
{
    private string _roomName;
    private BeamContext _beamContext;
    private BeamContext _beamContext01;
    
    private ChatService _chatService;
    
    private RoomHandle _currentRoom;
    private RoomHandle _currentRoom01;
    
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
        _beamContext01 = await BeamContext.ForPlayer("MyPlayer02").Instance;

        await _beamContext.Accounts.OnReady;
        await _beamContext01.Accounts.OnReady;
        
        _roomName = PlayerPrefs.GetString("SelectedRoomName", string.Empty);
        roomNameText.text = _roomName;
        
        _chatService = _beamContext.ServiceProvider.GetService<ChatService>();
        _chatService.Subscribe(HandleChatViewUpdate);
    }

    private void HandleChatViewUpdate(ChatView chatView)
    {
        ChatView = chatView;
        _currentRoom = _roomName == "General"
            ? ChatView.GuildRooms.LastOrDefault()
            : ChatView.roomHandles.Find(x => x.Name == _roomName);

        _currentRoom?.Subscribe().Then(_ => _currentRoom.OnMessageReceived += OnMessageReceived);
    }

    private async void OnMessageReceived(Message message)
    {
        Debug.Log("censored content: " + message.censoredContent );
        Debug.Log("content: " + message.content);
    }

    
    
    
    public void SendMessage()
    {
        if (_currentRoom != null && !string.IsNullOrEmpty(messageInput.text))
        {
            // Create a new ChatFeed object with the sender, message, and timestamp
            var feed = new ChatFeed(
                _beamContext.Accounts.Current.GamerTag,
                messageInput.text,
                DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            );

            // Serialize the ChatFeed object to JSON
            string feedJson = JsonUtility.ToJson(feed);

            // Create a Payload object with the serialized ChatFeed JSON
            var payload = new Payload(PayloadType.Chat, feedJson);

            // Serialize the Payload object to JSON
            string payloadJson = JsonUtility.ToJson(payload);

            // Debug log to verify the serialized payload
            Debug.Log("Payload JSON to send: " + payloadJson);

            // Send the serialized Payload JSON to the current room
            _currentRoom.SendMessage(payloadJson);

            // Clear the message input field
            messageInput.text = "";
        }
    }
    
    public void ForgetRoom()
    {
        if (_currentRoom != null)
        {
            try
            {
                ChatView.roomHandles.Remove(_currentRoom);
                SceneManager.LoadScene("ChatRooms");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error forgetting room: {e.Message}");
            }
        }
    }
}
