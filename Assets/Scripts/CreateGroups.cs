using System;
using System.Threading.Tasks;
using Beamable;
using Beamable.Common.Api;
using Beamable.Common.Api.Groups;
using Beamable.Server.Clients;
using Managers;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class CreateGroups : MonoBehaviour
{
        //This example is made with guest players
        private PlayerGroupManager _mainPlayer;
        private PlayerGroupManager _guestPlayer0;
        private PlayerGroupManager _guestPlayer1;

        [SerializeField] 
        private TMP_InputField usernameInput;
        [SerializeField] 
        private TMP_Dropdown groupTypeDropdown;
        [SerializeField]
        private TMP_InputField groupNameInput;
        [SerializeField]
        private TMP_InputField minMembersInput;
        [SerializeField]
        private TMP_InputField maxMembersInput;
        [SerializeField] 
        private Button createGroupButton;
        [SerializeField]
        private TMP_Text infoText;
        
        protected async void  Start()
        {
            await SetupBeamable();
            SetupUIListeners();
            
            await _guestPlayer0.CreateGroup("groupName2", "tag", "open", 0, 30, "GuestPlayer00");
            
            createGroupButton.interactable = false;
        }

        private async Task SetupBeamable()
        {
            var beamContext = await BeamContext.Default.Instance;
            _mainPlayer = new PlayerGroupManager(beamContext);
            await _mainPlayer.Initialize();
            
            var beamContext01 = await BeamContext.ForPlayer("MyPlayer01").Instance;
            _guestPlayer0 = new PlayerGroupManager(beamContext01);
            await _guestPlayer0.Initialize();
            
            var beamContext02 = await BeamContext.ForPlayer("MyPlayer02").Instance;
            _guestPlayer1 = new PlayerGroupManager(beamContext02);
            await _guestPlayer1.Initialize();
        }

        private void SetupUIListeners()
        {
            groupNameInput.onValueChanged.AddListener(CheckFields);
            minMembersInput.onValueChanged.AddListener(CheckFields);
            maxMembersInput.onValueChanged.AddListener(CheckFields);
        }

        public string GetDropdownValue()
        {
            var dropdownValue = groupTypeDropdown.value;
            var selectedOption = groupTypeDropdown.options[dropdownValue].text;
            return selectedOption;
        }
        
        private string GenerateTag(string groupName)
        {
            var words = groupName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var tag = "";

            for (int i = 0; i < Math.Min(words.Length, 3); i++)
            {
                tag += words[i][0]; 
            }

            // If the tag is less than 3 characters, pad it with the first characters of the group name
            while (tag.Length < 3 && groupName.Length > 0)
            {
                tag += groupName[0]; 
                groupName = groupName.Substring(1); 
            }

            return tag.ToUpper();
        }

        public async void CreateGroup()
        {            
            var generatedTag = GenerateTag(groupNameInput.text);
            var type = GetDropdownValue();

            var groupId = await _mainPlayer.CreateGroup(groupNameInput.text, generatedTag, type, int.Parse(minMembersInput.text),
                int.Parse(maxMembersInput.text), usernameInput.text);
            
            await _guestPlayer1.JoinGroup(groupId, "GuestPlayer01");
            
            infoText.text = "Group created successfully!";
        }
        
        private void CheckFields(string value)
        {
            var allFieldsCompleted = !string.IsNullOrEmpty(groupNameInput.text) &&
                                     !string.IsNullOrEmpty(minMembersInput.text) &&
                                     !string.IsNullOrEmpty(maxMembersInput.text);

            createGroupButton.interactable = allFieldsCompleted;
        }
      
    }