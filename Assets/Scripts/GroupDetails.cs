using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beamable;
using Beamable.Experimental.Api.Chat;
using Beamable.Server.Clients;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GroupDetails : MonoBehaviour
{
    private BeamContext _beamContext;
    private BeamContext _beamContext01;

    private UserServiceClient _userService;
    private ChatService _chatService;
    
    [SerializeField]
    private TMP_Text groupNameText;
    [SerializeField]
    private TMP_Text groupMembersText;
    [SerializeField] 
    private TMP_InputField chatNameInput;
    [SerializeField] 
    private Button startChatButton;

    private async void Start()
    {
        _beamContext = await BeamContext.Default.Instance;
        _beamContext01 = await BeamContext.ForPlayer("MyPlayer02").Instance;
        await _beamContext01.Accounts.OnReady;
        
        _userService = new UserServiceClient();
        startChatButton.interactable = false;
        _chatService = _beamContext.ServiceProvider.GetService<ChatService>();
        
        SetupUIListeners();

        string groupIdString = PlayerPrefs.GetString("SelectedGroupId", string.Empty);
        if (!string.IsNullOrEmpty(groupIdString) && long.TryParse(groupIdString, out var groupId))
        {
            await DisplayGroupDetails(groupId);
        }
    }
    
    private void SetupUIListeners()
    {
        chatNameInput.onValueChanged.AddListener(CheckFields);
    }

    private async Task DisplayGroupDetails(long groupId)
    {
        try
        {
            var group = await _beamContext.Api.GroupsService.GetGroup(groupId);
            var count = 1;
            
            groupNameText.text = group.name;
            groupMembersText.text = "Members:\n";

            foreach (var member in group.members)
            {
                var username = await _userService.GetPlayerAvatarName(member.gamerTag);
                
                groupMembersText.text += $"\n{count}. {username.data}\n";
                count++;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error fetching group details: {e.Message}");
        }
    }
    
    private void CheckFields(string value)
    {
        var allFieldsCompleted = !string.IsNullOrEmpty(chatNameInput.text);

        startChatButton.interactable = allFieldsCompleted;
    }
    
    private async Task StartChat(string roomName)
    {
        try
        {
            var guestPlayerId = _beamContext01.Accounts.Current;
            await _chatService.CreateRoom(roomName, false, new List<long> { _beamContext.PlayerId, guestPlayerId.GamerTag });
            PlayerPrefs.SetString("SelectedRoomName", roomName);
            SceneManager.LoadScene("ChatRoom"); 
        }
        catch (Exception e)
        {
            Debug.LogError($"Error creating chat room: {e.Message}");
        }
    }

    public async void StartChatButton()
    {
        var roomName = chatNameInput.text;
        await StartChat(roomName);

    }
}