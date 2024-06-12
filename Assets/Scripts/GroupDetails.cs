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

    private string _groupIdString;
    
    [SerializeField]
    private TMP_Text groupNameText;
    [SerializeField]
    private TMP_Text groupMembersText;
    [SerializeField] 
    private TMP_InputField chatNameInput;
    [SerializeField] 
    private Button startChatButton;
    [SerializeField] 
    private Button disbandGroupButton;


    private async void Start()
    {
        _beamContext = await BeamContext.Default.Instance;
        _beamContext01 = await BeamContext.ForPlayer("MyPlayer02").Instance;
        await _beamContext01.Accounts.OnReady;
        
        _userService = new UserServiceClient();
        startChatButton.interactable = false;
        _chatService = _beamContext.ServiceProvider.GetService<ChatService>();
        
        SetupUIListeners();

         _groupIdString = PlayerPrefs.GetString("SelectedGroupId", string.Empty);
        if (!string.IsNullOrEmpty(_groupIdString) && long.TryParse(_groupIdString, out var groupId))
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
            if (group != null)
            {
                groupNameText.text = group.name;
                groupMembersText.text = "Members:\n";

                if (group.members != null)
                {
                    var count = 1;
                    foreach (var member in group.members)
                    {
                        var username = await _userService.GetPlayerAvatarName(member.gamerTag);
                        groupMembersText.text += $"\n{count}. {username.data}\n";
                        count++;
                    }
                }

                if ( group.canDisband)
                {
                    disbandGroupButton.gameObject.SetActive(true);
                }
            }
            else
            {
                Debug.LogError("Group details are null.");
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
            Debug.LogError(_beamContext.PlayerId + " " + guestPlayerId);
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

    public async void DisbandGroupTrigger()
    {
        if (!string.IsNullOrEmpty(_groupIdString) && long.TryParse(_groupIdString, out var groupId))
        {
            await DisbandGroup(groupId);
        }
    }

    private async Task DisbandGroup(long groupId)
    {
        try
        {
            var group = await _beamContext.Api.GroupsService.GetGroup(groupId);
            if (group.canDisband)
            {
                await _beamContext.Api.GroupsService.DisbandGroup(groupId);
                Debug.Log("Group disbanded successfully");
                SceneManager.LoadScene("CreateGroup");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}