using System;
using System.Threading.Tasks;
using Beamable;
using Beamable.Server.Clients;
using TMPro;
using UnityEngine;

public class GroupDetails : MonoBehaviour
{
    private BeamContext _beamContext;
    private UserGroupServiceClient _userGroupServiceClient;

    [SerializeField]
    private TMP_Text groupNameText;

    [SerializeField]
    private TMP_Text groupMembersText;

    private async void Start()
    {
        _beamContext = BeamContext.Default;
        await _beamContext.OnReady;
        _userGroupServiceClient = new UserGroupServiceClient();

        string groupIdString = PlayerPrefs.GetString("SelectedGroupId", string.Empty);
        if (!string.IsNullOrEmpty(groupIdString) && long.TryParse(groupIdString, out var groupId))
        {
            await DisplayGroupDetails(groupId);
        }
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
                var username = _userGroupServiceClient.GetUsername(member.gamerTag);
                groupMembersText.text += $"\n{count}. {username}\n";
                count++;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error fetching group details: {e.Message}");
        }
    }
}