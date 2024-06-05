using System;
using System.Threading.Tasks;
using Beamable;
using Beamable.Common.Api;
using Beamable.Common.Api.Groups;
using Beamable.Server.Clients;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class CreateGroups : MonoBehaviour
{
        private BeamContext _beamContext;
        /*
        This example is made with guest players
        */
        private BeamContext _beamContext01;
        private UserServiceClient _userService;

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
        
        private GroupsView _groupsView = null;
        private GroupsView _groupsView01 = null;
        
        private TaskCompletionSource<bool> _groupsView01Ready = new TaskCompletionSource<bool>();
        private TaskCompletionSource<bool> _groupsViewReady = new TaskCompletionSource<bool>();
        private bool _groupsView01ReadySet = false;
        private bool _groupsViewReadySet = false;
        
        protected async void  Start()
        {
            await SetupBeamable();
            SetupUIListeners();
            await CreateGuestsGroup();

            _userService = new UserServiceClient();

            createGroupButton.interactable = false;
        }

        private async Task SetupBeamable()
        {
            _beamContext01 = BeamContext.ForPlayer("MyPlayer01");
            await _beamContext01.OnReady;
            
            _beamContext01.Api.GroupsService.Subscribe(groupsView =>
            {
                _groupsView01 = groupsView;
                Debug.Log("GroupsService01.Subscribe 1: " + _groupsView01.Groups.Count);
                if (!_groupsView01ReadySet)
                {
                    _groupsView01Ready.SetResult(true);
                    _groupsView01ReadySet = true;
                }
            });
            

            
            _beamContext = BeamContext.Default;
            await _beamContext.OnReady;
            await _beamContext.Accounts.OnReady;

            _beamContext.Api.GroupsService.Subscribe(groupsView =>
            {
                _groupsView = groupsView;
                Debug.Log("GroupsService.Subscribe 1: " + _groupsView.Groups.Count);
                if (!_groupsViewReadySet)
                {
                    _groupsViewReady.SetResult(true);
                    _groupsViewReadySet = true;
                }
            });
            await Task.WhenAll(_groupsView01Ready.Task, _groupsViewReady.Task);
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


        private async Task CreateGroupAsync(string groupName, string groupTag, string groupType, int minMembers,
            int maxMembers)
        {
            try
            {
                if (_userService == null)
                {
                    Debug.LogError("_userService is not initialized.");
                    return;
                }
                var account = _beamContext.Accounts.Current;
                
                var groupCreateRequest = new GroupCreateRequest(groupName, groupTag, groupType, minMembers, maxMembers);
                await _beamContext.Api.GroupsService.CreateGroup(groupCreateRequest);

                
                await _userService.SetPlayerAvatarName(account.GamerTag, usernameInput.text);
                

                infoText.text = "Group created successfully!";
            }
            catch (Exception e)
            {
                Debug.LogError("error " + e.Message);
                Console.WriteLine(e.Message);
                throw;
            }

        }

        public async void CreateGroup()
        {
            /*
            await LeaveGroups();
            */
            
            var generatedTag = GenerateTag(groupNameInput.text);
            var type = GetDropdownValue();
            await CreateGroupAsync(groupNameInput.text, generatedTag, type, int.Parse(minMembersInput.text), int.Parse(maxMembersInput.text));
        
        }
        
        public async Task<EmptyResponse> LeaveGroups()
        {

            // Leave any existing groups
            foreach(var group in _groupsView.Groups)
            {
                
                var result = await _beamContext.Api.GroupsService.LeaveGroup(group.Group.id);
            }
            await Task.Delay(500); 

            // HACK: Force refresh here (0.10.1)
            // Wait (arbitrary milliseconds) for refresh to complete 
            _beamContext.Api.GroupsService.Subscribable.ForceRefresh();
            await Task.Delay(300); 
            
            
            return new EmptyResponse();
        }
        
        public async Task<EmptyResponse> LeaveGroupsGuest()
        {
                // Leave any existing groups
                foreach (var group in _groupsView01.Groups)
                {
                    var result = await _beamContext01.Api.GroupsService.LeaveGroup(group.Group.id);
                }
            // HACK: Force refresh here (0.10.1)
            // Wait (arbitrary milliseconds) for refresh to complete 
            _beamContext01.Api.GroupsService.Subscribable.ForceRefresh();

            await Task.Delay(300); 

            
            return new EmptyResponse();
        }
        
        private void CheckFields(string value)
        {
            var allFieldsCompleted = !string.IsNullOrEmpty(groupNameInput.text) &&
                                     !string.IsNullOrEmpty(minMembersInput.text) &&
                                     !string.IsNullOrEmpty(maxMembersInput.text);

            createGroupButton.interactable = allFieldsCompleted;
        }
        
        private async Task CreateGuestsGroup()
        {
            if (_beamContext01 == null)
            {
                Debug.LogError("beamContext01 is not initialized.");
                return;
            }
            
            try
            {
                await LeaveGroupsGuest();
                
                var groupCreateRequest = new GroupCreateRequest("groupName1", "tag", "open", 0, 30);
                await _beamContext01.Api.GroupsService.CreateGroup(groupCreateRequest);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }
      
    }