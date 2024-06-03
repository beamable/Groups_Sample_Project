using System;
using System.Threading.Tasks;
using Beamable;
using TMPro;
using UnityEngine;

public class GroupDetails : MonoBehaviour
{
    private BeamContext _beamContext;

    [SerializeField]
    private TMP_Text groupNameText;

    [SerializeField]
    private TMP_Text groupMembersText;

    private async void Start()
    {
        _beamContext = BeamContext.Default;
        await _beamContext.OnReady;

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
                groupMembersText.text += $"\n{count}. {member.gamerTag}\n";
                count++;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error fetching group details: {e.Message}");
        }
    }
}