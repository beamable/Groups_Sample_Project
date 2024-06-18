using System;
using System.Threading.Tasks;
using Beamable;
using Beamable.Common.Api.Groups;
using Beamable.Server.Clients;
using UnityEngine;

namespace Managers
{
    public class PlayerGroupManager
    {
        private BeamContext _beamContext;
        private GroupsView _groupsView;
        private UserServiceClient _userService;
        private TaskCompletionSource<bool> _groupsViewInitialized;
        private bool _isSubscribed;

        public PlayerGroupManager(BeamContext beamContext)
        {
            _beamContext = beamContext;
            _userService = new UserServiceClient();
            _groupsViewInitialized = new TaskCompletionSource<bool>();
        }

        public async Task Initialize()
        {
            if (_isSubscribed) return;
            
            await _beamContext.Accounts.OnReady;
            _beamContext.Api.GroupsService.Subscribe(groupsView =>
            {
                _groupsView = groupsView;
                _groupsViewInitialized.TrySetResult(true);
            });

            _isSubscribed = true;
        }

        public async Task<long> CreateGroup(string groupName, string groupTag, string groupType, int minMembers, int maxMembers, string username)
        {
            await _groupsViewInitialized.Task; // Wait for _groupsView to be initialized
            await LeaveGroups();
            
            var account = _beamContext.Accounts.Current;
            var groupCreateRequest = new GroupCreateRequest(groupName, groupTag, groupType, minMembers, maxMembers);
            var groupResponse = await _beamContext.Api.GroupsService.CreateGroup(groupCreateRequest);
            await _userService.SetPlayerAvatarName(account.GamerTag, username);
            
            Debug.Log("New group created: " + groupName);
            return groupResponse.group.id;
        }

        private async Task LeaveGroups()
        {
            if (_groupsView == null)
            {
                Debug.LogError("_groupsView is not initialized.");
                return;
            }
            
            foreach (var group in _groupsView.Groups)
            {
                await _beamContext.Api.GroupsService.LeaveGroup(group.Group.id);
            }
            _beamContext.Api.GroupsService.Subscribable.ForceRefresh();
            await Task.Delay(300);
        }
        
        public async Task LeaveGroup(long groupId)
        {
            try
            {
                var response = await _beamContext.Api.GroupsService.LeaveGroup(groupId);
                if (response != null)
                {
                    Debug.Log("Left group successfully");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error leaving group: {e.Message}");
            }
        }

        public async Task JoinGroup(long groupId, string username)
        {
            await _groupsViewInitialized.Task; // Wait for _groupsView to be initialized
            await LeaveGroups();
            
            var account = _beamContext.Accounts.Current;
            await _beamContext.Api.GroupsService.JoinGroup(groupId);
            await _userService.SetPlayerAvatarName(account.GamerTag, username);

            Debug.Log("Joined group: " + groupId);
        }
        
        public async Task<Group> GetGroup(long groupId)
        {
            try
            {
                var group = await _beamContext.Api.GroupsService.GetGroup(groupId);
                Debug.Log($"Group details retrieved: {group.name}");
                return group;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error retrieving group details: {e.Message}");
                throw;
            }
        }

        public async Task<bool> KickMember(long groupId, long gamerTag)
        {
            try
            {
                var response = await _beamContext.Api.GroupsService.Kick(groupId, gamerTag);
                if (response != null)
                {
                    Debug.Log("Member kicked successfully");
                    return true;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error kicking member: {e.Message}");
            }
            return false;
        }

        public async Task<bool> SetLeader(long groupId, long gamerTag)
        {
            try
            {
                var response = await _beamContext.Api.GroupsService.SetRole(groupId, gamerTag, "leader");
                if (response != null)
                {
                    Debug.Log("Member set as leader successfully");
                    return true;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error setting leader: {e.Message}");
            }
            return false;
        }

        public async Task<bool> DisbandGroup(long groupId)
        {
            try
            {
                var group = await _beamContext.Api.GroupsService.GetGroup(groupId);
                if (group.canDisband)
                {
                    await _beamContext.Api.GroupsService.DisbandGroup(groupId);
                    Debug.Log("Group disbanded successfully");
                    return true;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error disbanding group: {e.Message}");
            }
            return false;
        }

        public async Task<bool> UpdateGroupProperties(long groupId, GroupUpdateProperties props)
        {
            try
            {
                await _beamContext.Api.GroupsService.SetGroupProps(groupId, props);
                Debug.Log("Group properties updated successfully");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error updating group properties: {e.Message}");
            }
            return false;
        }
    }
}
