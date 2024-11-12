using System;
using System.Threading.Tasks;
using Beamable;
using Beamable.Common.Api.Groups;
using Beamable.Common.Utils;
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
            Debug.Log("Initializing PlayerGroupManager...");
            _beamContext = beamContext;
            _userService = new UserServiceClient();
            _groupsViewInitialized = new TaskCompletionSource<bool>();
        }

        public async Task Initialize()
        {
            if (_isSubscribed)
            {
                Debug.Log("Already subscribed, skipping Initialize.");
                return;
            }

            Debug.Log("Waiting for BeamContext account readiness...");
            await _beamContext.Accounts.OnReady;

            Debug.Log("Subscribing to GroupsService...");
            _beamContext.Api.GroupsService.Subscribe(groupsView =>
            {
                _groupsView = groupsView;
                _groupsViewInitialized.TrySetResult(true);
                Debug.Log("GroupsView subscribed and initialized.");
            });

            _isSubscribed = true;
            Debug.Log("Initialization complete.");
        }

        public async Task<Response<long>> CreateGroup(string groupName, string groupTag, string groupType, int minMembers, int maxMembers, string username)
        {
            Debug.Log("Starting CreateGroup process...");
            await _groupsViewInitialized.Task;
            Debug.Log("_groupsView initialized.");

            Debug.Log("Leaving existing groups...");
            await LeaveGroups();
            Debug.Log("Left existing groups.");

            var account = _beamContext.Accounts.Current;
            Debug.Log($"Current account obtained: {account?.GamerTag}");

            if (!string.IsNullOrEmpty(username))
            {
                Debug.Log($"Setting player avatar name: {username}");
                var response = await _userService.SetPlayerAvatarName(account.GamerTag, username);
                
                if (!string.IsNullOrEmpty(response.errorMessage))
                {
                    Debug.LogError($"Error setting avatar name: {response.errorMessage}");
                    return new Response<long>(default, response.errorMessage);
                }
            }

            Debug.Log("Creating new group...");
            var groupCreateRequest = new GroupCreateRequest(groupName, groupTag, groupType, minMembers, maxMembers);
            var groupResponse = await _beamContext.Api.GroupsService.CreateGroup(groupCreateRequest);

            Debug.Log($"New group created successfully: {groupName}, ID: {groupResponse.group.id}");
            return new Response<long>(groupResponse.group.id);
        }

        private async Task LeaveGroups()
        {
            if (_groupsView == null)
            {
                Debug.LogError("_groupsView is not initialized, cannot leave groups.");
                return;
            }

            Debug.Log("Leaving all joined groups...");
            foreach (var group in _groupsView.Groups)
            {
                Debug.Log($"Leaving group ID: {group.Group.id}");
                await _beamContext.Api.GroupsService.LeaveGroup(group.Group.id);
            }
            Debug.Log("All groups left.");

            _beamContext.Api.GroupsService.Subscribable.ForceRefresh();
            Debug.Log("Forced groups refresh.");
            await Task.Delay(300); // Allowing refresh to propagate
        }
        
        public async Task LeaveGroup(long groupId)
        {
            try
            {
                Debug.Log($"Attempting to leave group ID: {groupId}");
                var response = await _beamContext.Api.GroupsService.LeaveGroup(groupId);

                if (response != null)
                {
                    Debug.Log("Left group successfully.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error leaving group: {e.Message}");
            }
        }

        public async Task JoinGroup(long groupId, string username = null)
        {
            Debug.Log("Starting JoinGroup process...");
            await _groupsViewInitialized.Task;
            Debug.Log("_groupsView initialized for JoinGroup.");

            Debug.Log("Leaving existing groups before joining a new group...");
            await LeaveGroups();

            var account = _beamContext.Accounts.Current;
            Debug.Log($"Current account obtained for joining: {account?.GamerTag}");
            await _beamContext.Api.GroupsService.JoinGroup(groupId);

            if (!string.IsNullOrEmpty(username))
            {
                Debug.Log($"Setting player avatar name after joining: {username}");
                await _userService.SetPlayerAvatarName(account.GamerTag, username);
                Debug.Log($"Avatar name set to {username} for {account.GamerTag}");
            }

            Debug.Log($"Joined group successfully: {groupId}");
        }

        public async Task<Group> GetGroup(long groupId)
        {
            try
            {
                Debug.Log($"Retrieving group details for ID: {groupId}");
                var group = await _beamContext.Api.GroupsService.GetGroup(groupId);
                Debug.Log($"Group details retrieved for ID: {groupId}");
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
