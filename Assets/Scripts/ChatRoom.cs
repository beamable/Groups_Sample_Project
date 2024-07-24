using System;
using System.Collections.Generic;
using System.Linq;
using Beamable;
using Beamable.Common.Models;
using Beamable.Experimental.Api.Chat;
using Beamable.Server.Clients;
using TMPro;
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

    private readonly HashSet<string> _censoredWords = new HashSet<string>
    {
        "fuck",
        "porn",
        // Add more words as needed
    };

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
        Debug.Log("Censored content: " + message.content);
    }

    private string CensorMessage(string message)
    {
        foreach (var word in _censoredWords)
        {
            message = message.Replace(word, new string('*', word.Length), StringComparison.OrdinalIgnoreCase);
        }
        return message;
    }
    
    public void SendMessage()
    {
        if (_currentRoom != null && !string.IsNullOrEmpty(messageInput.text))
        {
            // Censor the message
            string censoredMessage = CensorMessage(messageInput.text);
            var feed = new ChatFeed(
                _beamContext.Accounts.Current.GamerTag,
                censoredMessage,
                DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            );

            string feedJson = JsonUtility.ToJson(feed);
            var payload = new Payload(PayloadType.Chat, feedJson);
            string payloadJson = JsonUtility.ToJson(payload);
            Debug.Log("Payload JSON to send: " + payloadJson);
            _currentRoom.SendMessage(payloadJson);
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
