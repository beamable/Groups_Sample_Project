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
using UnityEngine.Events;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;
using Update = Unity.VisualScripting.Update;

[System.Serializable]
public class ViewGroups : MonoBehaviour
{
        private BeamContext _beamContext;
        [SerializeField] private TMP_InputField searchInput;
        [SerializeField]
        private TMP_Text groupsListText;
        
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
            groupsListText.text = "Groups:\n";
            int count = 1;
            foreach (var groupView in groups)
            {
                groupsListText.text += $"\n{count}. {groupView.name}\n";
                count++;
            }
        }

        public async void OnSearchValueChanged(string searchText)
        {
            await FetchAndDisplayGroups(searchText);
        }

    }