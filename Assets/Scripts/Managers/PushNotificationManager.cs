using System;
using System.Threading.Tasks;
using Beamable;
using Beamable.Server.Clients;
using Firebase.Extensions;
using Firebase.Messaging;
using Unity.Notifications.Android;
using UnityEngine;
using Beamable.Common.Utils;
using Firebase;
using Google.Apis.Auth.OAuth2;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

namespace Managers
{
    public class PushNotificationManager : MonoBehaviour
    {
        private const string ChannelID = "notifications_channel";
        private NotificationsServiceClient _notificationsService;
        private BeamContext _beamContext;
        private bool _isAppInFocus;

        private async void Start()
        {
            _notificationsService = new NotificationsServiceClient();

            _beamContext = await BeamContext.Default.Instance;

            InitializeFirebase();
            
#if UNITY_ANDROID
            CreateNotificationChannel();
#endif

        }

        private void OnApplicationFocus(bool hasFocus)
        {
            _isAppInFocus = hasFocus;
        }

        private static void CreateNotificationChannel()
        {
#if UNITY_ANDROID
            var channel = new AndroidNotificationChannel()
            {
                Id = ChannelID,
                Name = "Chat Notifications",
                Importance = Importance.High,
                Description = "Notifications for new chat messages"
            };
            AndroidNotificationCenter.RegisterNotificationChannel(channel);
#endif
        }

        private void InitializeFirebase()
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                var dependencyStatus = task.Result;
                if (dependencyStatus == DependencyStatus.Available)
                {
                    FirebaseMessaging.TokenReceived += OnTokenReceived;
                    FirebaseMessaging.MessageReceived += OnMessageReceived;
                }
                else
                {
                    Debug.LogError($"PushNotificationManager: Could not resolve all Firebase dependencies: {dependencyStatus}");
                }
            });
        }

        private async void OnTokenReceived(object sender, TokenReceivedEventArgs e)
        {
            var fcmToken = e.Token;

            try
            {
                 await _notificationsService.SetPlayerFcmToken(_beamContext.PlayerId, fcmToken);
            }
            catch (Exception ex)
            {
                Debug.LogError($"PushNotificationManager: Error sending FCM token to server - {ex.Message}");
            }
        }

        private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            var notification = e.Message.Notification;
            if (notification == null)
            {
                Debug.LogWarning("PushNotificationManager: Received message has no notification.");
                return;
            }

            var (title, body) = (notification.Title, notification.Body);
            
#if UNITY_ANDROID
            ShowAndroidNotification(title, body);
#endif
        }

#if UNITY_ANDROID
        private static void ShowAndroidNotification(string title, string body)
        {

            var notification = new AndroidNotification
            {
                Title = title,
                Text = body,
                FireTime = DateTime.Now
            };
            AndroidNotificationCenter.SendNotification(notification, ChannelID);
        }
#endif
    }
}
