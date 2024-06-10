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
        private List<RoomHandle> _chatRooms;


        protected async void Start()
        {
            await SetupBeamable();
            
            _chatService = _beamContext.ServiceProvider.GetService<ChatService>();
            _chatService.Subscribe(GetRooms);
            _chatRooms = new List<RoomHandle>();

        }

        private async Task SetupBeamable()
        {
            _beamContext = await BeamContext.Default.Instance;

        }
        
        private void GetRooms(ChatView chatView)
        {
            _chatRooms = chatView.roomHandles;
            DisplayRooms(_chatRooms);
        }
        
        private void DisplayRooms(List<RoomHandle> rooms)
        {
            var count = 1;
            foreach (var room in rooms)
            {
                var roomName = room.Name;
                if (count > 1) // Skip the first element
                {
                    if (count == 2) // Change the name for the second button
                    {
                        roomName = "General";
                    }
                    else
                    {
                        // Increment the count if not the second element
                        count++;
                    }

                    Debug.Log(room.Name);
                    var button = Instantiate(roomButtonPrefab, roomListContent);
                    button.GetComponentInChildren<TextMeshProUGUI>().text = $"{count - 1}. {roomName}";
                    button.onClick.AddListener(() => OnGroupClick(room));
                }
                count++;
            }
        }

        
        private void OnGroupClick(RoomHandle room)
        {
            PlayerPrefs.SetString("SelectedRoomName", room.Name);
            SceneManager.LoadScene("ChatRoom");
        }
    }
