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
        
        [SerializeField]
        private TMP_Text groupsListText;
        
        private GroupsView _groupsView = null;


        protected async void Start()
        {
            await SetupBeamable();
        }

        private async Task SetupBeamable()
        {
            _beamContext = BeamContext.Default;
            await _beamContext.OnReady;
            
            _beamContext.Api.GroupsService.Subscribe(async groupsView =>
            {
                _groupsView = groupsView;
                
                await FetchAndDisplayGroups();
                Debug.Log("GroupsService.Subscribe 1: " + _groupsView.Groups.Count);

            });
        }
        
        private async Task FetchAndDisplayGroups()
        {
            try
            {
                // Perform a group search to get all available groups
                var groupSearchResponse = await _beamContext.Api.GroupsService.Search("", new List<string> { "open", "closed", "restricted" });

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



    }