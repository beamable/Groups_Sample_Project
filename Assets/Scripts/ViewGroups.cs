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


        protected void Start()
        {
            SetupBeamable();
        }

        private async void SetupBeamable()
        {
            _beamContext = BeamContext.Default;
            await _beamContext.OnReady;
            
            _beamContext.Api.GroupsService.Subscribe( groupsView =>
            {
                _groupsView = groupsView;
                
                groupsListText.text = "Groups:\n";
                foreach (var groupView in groupsView.Groups)
                {
                    groupsListText.text += $"{groupView.Group.name}\n";

                }
                Debug.Log("GroupsService.Subscribe 1: " + _groupsView.Groups.Count);

            });
        }

    }