using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beamable;
using Beamable.Api.Groups;
using Beamable.Common.Api;
using Beamable.Common.Api.Groups;
using UnityEngine;
using Beamable.Experimental.Api.Chat;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.Events;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Update = Unity.VisualScripting.Update;

[System.Serializable]
public class ViewGroups : MonoBehaviour
{
        private BeamContext _beamContext;

        [SerializeField] 
        private TMP_InputField searchInput;
        [SerializeField]
        private TMP_Text groupsListText;
        [SerializeField]
        private Button groupButtonPrefab;
        [SerializeField]
        private Transform groupsListContent;
        
        private GroupsView _groupsView = null;


        protected async void Start()
        {
            await SetupBeamable();
            searchInput.onValueChanged.AddListener(OnSearchValueChanged);
        }

        private async Task SetupBeamable()
        {
            _beamContext = BeamContext.Default;
            await _beamContext.OnReady;
            
            _beamContext.Api.GroupsService.Subscribe(async groupsView =>
            {
                _groupsView = groupsView;
                
                await FetchAndDisplayGroups("");
                Debug.Log("GroupsService.Subscribe 1: " + _groupsView.Groups.Count);

            });
        }
        
        private async Task FetchAndDisplayGroups(string searchQuery)
        {
            try
            {
                var groupSearchResponse = await _beamContext.Api.GroupsService.Search(searchQuery, new List<string> { "open", "closed" });
                DisplayGroups(groupSearchResponse.groups);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error fetching groups: {e.Message}");
            }
        }
        
        private void DisplayGroups(List<Group> groups)
        {
            var count = 1;
            foreach (var group in groups)
            {
                var button = Instantiate(groupButtonPrefab, groupsListContent);
                button.GetComponentInChildren<TextMeshProUGUI>().text = $"{count}. {group.name}";
                button.onClick.AddListener(() => OnGroupClick(group));
                count++;
            }
        }

        public async void OnSearchValueChanged(string searchText)
        {
            await FetchAndDisplayGroups(searchText);
        }
        
        private void OnGroupClick(Group group)
        {
            PlayerPrefs.SetString("SelectedGroupId", group.id.ToString());
            SceneManager.LoadScene("GroupDetails");
        }

    }