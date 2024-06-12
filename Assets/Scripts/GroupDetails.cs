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
    private bool _isLeader;
    
    [SerializeField]
    private TMP_Text groupNameText;
    [SerializeField] 
    private TMP_Text groupSloganText;
    [SerializeField]
    private TMP_Text groupMotdText;
    [SerializeField]
    private TMP_Text groupMembersText;
    [SerializeField] 
    private TMP_InputField chatNameInput;
    [SerializeField] 
    private Button startChatButton;
    [SerializeField] 
    private Button disbandGroupButton;
    [SerializeField] 
    private Button editGroupButton;
    [SerializeField] 
    private GameObject createRoom;
    [SerializeField]
    private GameObject memberItemPrefab;
    [SerializeField]
    private Transform groupMembersList;
    


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
                groupSloganText.text = group.slogan;
                groupMotdText.text = group.motd;
                
                _isLeader = group.canDisband;

                if (group.members != null)
                {
                    foreach (Transform child in groupMembersList)
                    {
                        Destroy(child.gameObject);
                    }
                    
                    foreach (var member in group.members)
                    {
                        var username = await _userService.GetPlayerAvatarName(member.gamerTag);
                        AddMemberItem(username.data, group.id, member.gamerTag);
                    }
                }

                if (_isLeader)
                {
                    disbandGroupButton.gameObject.SetActive(true);
                    editGroupButton.gameObject.SetActive(true);
                    createRoom.gameObject.SetActive(true);
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
    
    private void AddMemberItem(string username, long groupId, long gamerTag)
    {
        var memberItem = Instantiate(memberItemPrefab, groupMembersList);
        var text = memberItem.GetComponentInChildren<TMP_Text>();
        text.text = username;

        var kickButton = memberItem.transform.Find("KickButton").GetComponent<Button>();
        kickButton.gameObject.SetActive(_isLeader && gamerTag != _beamContext.PlayerId);
        kickButton.onClick.AddListener(async () => await KickMember(groupId, gamerTag));
    }

    private async Task KickMember(long groupId, long gamerTag)
    {
        try
        {
            var response = await _beamContext.Api.GroupsService.Kick(groupId, gamerTag);
            if (response != null)
            {
                Debug.Log("Member kicked successfully");
                await DisplayGroupDetails(groupId); // Refresh the group details
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error kicking member: {e.Message}");
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
    
    public void LoadEditGroupScene()
    {
        SceneManager.LoadScene("EditGroup");
    }
}