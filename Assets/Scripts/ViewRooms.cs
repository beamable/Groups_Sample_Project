using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beamable;
using Beamable.Common.Api.Groups;
using Beamable.Experimental.Api.Chat;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ViewRooms: MonoBehaviour
    {
        private BeamContext _beamContext;
        
        [SerializeField]
        private Button roomButtonPrefab;
        [SerializeField]
        private Transform roomListContent;
        
        private ChatService _chatService;
        private List<RoomInfo> _chatRooms;


        protected async void Start()
        {
            await SetupBeamable();
            
            _chatService = _beamContext.ServiceProvider.GetService<ChatService>();
            _chatService.Subscribe(GetRooms);
            _chatRooms = new List<RoomInfo>();

        }

        private async Task SetupBeamable()
        {
            _beamContext = await BeamContext.Default.Instance;
        }
        
        private async void GetRooms(ChatView chatView)
        {
            _chatRooms = await _chatService.GetMyRooms();
            DisplayRooms(_chatRooms);
        }
        
        private void DisplayRooms(List<RoomInfo> rooms)
        {
            // Clear existing buttons
            foreach (Transform child in roomListContent)
            {
                Destroy(child.gameObject);
            }

            if (rooms.Count == 0)
            {
                // Display a message indicating no rooms are available
                var messageText = Instantiate(roomButtonPrefab, roomListContent);
                messageText.GetComponentInChildren<TextMeshProUGUI>().text = "No rooms available";
                messageText.interactable = false; // Optionally disable interaction
                return;
            }

            var count = 1;
            foreach (var room in rooms)
            {
                var roomName = room.name;
                if (count > 1) // Skip the first element
                {
                    if (count == 2) // Change the name for the second button
                    {
                        roomName = "General";
                    }

                    var button = Instantiate(roomButtonPrefab, roomListContent);
                    button.GetComponentInChildren<TextMeshProUGUI>().text = $"{count - 1}. {roomName}";
                    button.onClick.AddListener(() => OnGroupClick(roomName));
                }
                count++;
            }
        }


        
        private void OnGroupClick(string roomName)
        {
            PlayerPrefs.SetString("SelectedRoomName", roomName);
            SceneManager.LoadScene("ChatRoom");
        }
    }