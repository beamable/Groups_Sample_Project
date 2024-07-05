using Beamable;
using Beamable.Common.Api.Notifications;
using Beamable.Common.Models;
using Beamable.Installer.SmallerJSON;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Managers
{
    public class InvitationManager : MonoBehaviour
    {
        private BeamContext _beamContext;
        private PlayerGroupManager _groupManager;

        [SerializeField]
        private GameObject invitePopup;

        private TMP_Text _inviteMessage;
        private Button _acceptButton;
        private Button _declineButton;

        private async void Start()
        {
            InitializePopupComponents();
            SetupInvitationListener();

            _groupManager = new PlayerGroupManager(_beamContext);
            await _groupManager.Initialize();
        }

        private void InitializePopupComponents()
        {
            _inviteMessage = invitePopup.GetComponentInChildren<TMP_Text>();
            var buttons = invitePopup.GetComponentsInChildren<Button>();
            foreach (var button in buttons)
            {
                if (button.name == "AcceptButton")
                {
                    _acceptButton = button;
                }
                else if (button.name == "DeclineButton")
                {
                    _declineButton = button;
                }
            }

            if (_inviteMessage == null || _acceptButton == null || _declineButton == null)
            {
                Debug.LogError("Failed to find all components in invitePopup.");
            }
        }

        private async void SetupInvitationListener()
        {
            _beamContext = await BeamContext.Default.Instance;
            _beamContext.Api.NotificationService.Subscribe("GroupInvite", HandleInvitation);
            Debug.Log("Subscribed to GroupInvite notifications.");
        }

        private async void HandleInvitation(object payload)
        {
            Debug.Log("Received payload: " + JsonUtility.FromJson<InviteData>(payload.ToString()));

            try
            {
                // Try to parse the payload as a JSON string
                if (payload is string jsonPayload)
                {
                    Debug.Log("JsonPayload " + jsonPayload);
                    var inviteData = JsonUtility.FromJson<InviteData>(jsonPayload);
                    Debug.Log($"Parsed InviteData: gamerTag={inviteData.gamerTag}, groupId={inviteData.groupId}");

                    var group = await _groupManager.GetGroup(inviteData.groupId);
                    _inviteMessage.text = $"You've been invited to join {group.name}";
                    invitePopup.SetActive(true);

                    _acceptButton.onClick.RemoveAllListeners();
                    _declineButton.onClick.RemoveAllListeners();

                    _acceptButton.onClick.AddListener(() => AcceptInvite(inviteData.groupId));
                    _declineButton.onClick.AddListener(DeclineInvite);
                }
                else
                {
                    Debug.LogError("Payload is not a valid JSON string.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to parse GroupInvite payload: {ex.Message}");
            }
        }

        private async void AcceptInvite(long groupId)
        {
            invitePopup.SetActive(false);
            await _groupManager.JoinGroup(groupId);
        }

        private void DeclineInvite()
        {
            invitePopup.SetActive(false);
        }
    }
}
