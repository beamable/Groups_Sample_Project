using System;
using System.Linq;
using Beamable.Common;
using Beamable.Common.Models;
using Beamable.Common.Utils;
using Beamable.Integrations.Firebase;
using Beamable.Mongo;
using Beamable.Server;
using UnityEngine;

namespace Beamable.Microservices
{
	[Microservice("NotificationsService")]
	public class NotificationsService : Microservice
	{
		[ConfigureServices]
		public static void Configure(IServiceBuilder builder)
		{
			try
			{
				builder.Builder.AddSingleton<FirebaseService>();
			}
			catch (Exception e)
			{
				BeamableLogger.LogException(e);
				throw;
			}
		}

		[InitializeServices]
		public static async Promise Init(IServiceInitializer init)
		{
			try
			{
				var firebase = init.GetService<FirebaseService>();
				await firebase.Init();
			}
			catch (Exception e)
			{
				BeamableLogger.LogException(e);
				throw;
			}
		}
		
		private async Promise<Response<FirebaseMessageResponse>> SendMessageToDevice(FirebaseMessageRequest request)
		{
			try
			{
				var firebaseService = Provider.GetService<FirebaseService>();
				var sendMessageResult = await firebaseService.SendMessage(request.token, request.title, request.body);
				return new Response<FirebaseMessageResponse>(new FirebaseMessageResponse
				{
					name = sendMessageResult.name, 
					errorMessage = sendMessageResult.errorMessage
				});
			}
			catch (Exception e)
			{
				BeamableLogger.LogException(e);
				return new Response<FirebaseMessageResponse>(null, "Failed to send nudge to player");
			}
		}
		
		[ClientCallable]
		public async Promise<Response<bool>> SendNotification(long fromGamerTag, string toPlayerAvatarName)
		{
			var errorResponse = new Response<bool>(false, "Failed to send chat notification");
		
			try
			{
				var fromPlayerData = await Storage.GetByFieldName<PlayerData, long>("gamerTag", fromGamerTag);
		
				var allPlayersData = await Storage.GetAll<PlayerData>();
		
				if (fromPlayerData == null)
				{
					return errorResponse;
				}

				if (allPlayersData.Count == 0)
				{
					return errorResponse;
				}
		
				var fromPlayerAvatarName = fromPlayerData.avatarName;
				var allPlayerFcmToken = allPlayersData.Select(data => data.fcmToken).ToList();
		
				if (string.IsNullOrEmpty(fromPlayerAvatarName))
				{
					return errorResponse;
				}

				if (allPlayerFcmToken.Count == 0)
				{
					return errorResponse;
				}
		
				foreach (var playerData in allPlayersData)
				{
					var sendMessageResponse = await SendMessageToDevice(new FirebaseMessageRequest
					{
						title = "New Message",
						body = $"{fromPlayerAvatarName} sent a message",
						token = playerData.fcmToken,
					});
					
					var sendMessageResponseErrorMessage = sendMessageResponse.data.errorMessage;
				}
		
				return new Response<bool>(true);
			}
			catch (Exception e)
			{
				return errorResponse;
			}
		}

		
		[ClientCallable]
		public async Promise<Response<bool>> SetPlayerFcmToken(long gamerTag, string fcmToken)
		{
			try
			{
				var existingPlayerData = await Storage.GetByFieldName<PlayerData, long>("gamerTag", gamerTag);
				if (existingPlayerData == null)
				{
					// Create a new player data record
					await Storage.Create<UserGroupData, PlayerData>(new PlayerData
					{
						gamerTag = gamerTag,
						fcmToken = fcmToken
					});
				}
				else
				{
					// Update the existing player data with the new fcmToken
					existingPlayerData.fcmToken = fcmToken;
					await Storage.Update(existingPlayerData.Id, existingPlayerData);
				}

				return new Response<bool>(true);
			}
			catch (Exception e)
			{
				BeamableLogger.LogError(e);
				return new Response<bool>(false, "Error setting fcm token");
			}
		}

	}
}
