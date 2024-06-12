using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beamable;
using Beamable.Common.Api.Groups;
using Beamable.Experimental.Api.Chat;
using Beamable.Server.Clients;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EditGroup : MonoBehaviour
{
    private BeamContext _beamContext;
    private Group _group;
    
    private string _groupIdString;

    [SerializeField]
    private TMP_Text groupNameText;
    [SerializeField]
    private TMP_InputField groupNameInput;
    [SerializeField]
    private TMP_InputField groupSloganInput;
    [SerializeField]
    private TMP_InputField groupMotdInput;
    [SerializeField]
    private TMP_Dropdown groupTypeDropdown;
    [SerializeField]
    private TMP_Text resultText;

    private async void Start()
    {
        _beamContext = await BeamContext.Default.Instance;
        
        _groupIdString = PlayerPrefs.GetString("SelectedGroupId", string.Empty);
        if (!string.IsNullOrEmpty(_groupIdString) && long.TryParse(_groupIdString, out var groupId))
        {
            _group = await _beamContext.Api.GroupsService.GetGroup(groupId);
            groupNameText.text = _group.name;
            groupNameInput.text = _group.name;
            groupSloganInput.text = _group.slogan;
            groupMotdInput.text = _group.motd;
            switch (_group.enrollmentType.ToLower())
            {
                case "open":
                    groupTypeDropdown.value = 0;
                    break;
                case "restricted":
                    groupTypeDropdown.value = 1;
                    break;
                case "closed":
                    groupTypeDropdown.value = 2;
                    break;
                default:
                    Debug.LogWarning($"Unknown enrollment type: {_group.enrollmentType}");
                    break;
            }        }
        
    }
    
    public void GoBack()
    {
        SceneManager.LoadScene("GroupDetails");
    }

    public async void SaveGroup()
    {
        if (!string.IsNullOrEmpty(_groupIdString) && long.TryParse(_groupIdString, out var groupId))
        {
            var groupName = groupNameInput.text;
            var groupSlogan = groupSloganInput.text;
            var groupMotd = groupMotdInput.text;
            var groupType = groupTypeDropdown.options[groupTypeDropdown.value].text.ToLower();

            var props = new GroupUpdateProperties
            {
                name = groupName,
                slogan = groupSlogan,
                motd = groupMotd,
                enrollmentType = groupType
            };

            try
            {
                await _beamContext.Api.GroupsService.SetGroupProps(groupId, props);
                resultText.text = "Group updated successfully";
            }
            catch (Exception e)
            {
                resultText.text = "Error updating group";
                Debug.LogError($"Error updating group: {e.Message}");
            }
        }
    }
}
