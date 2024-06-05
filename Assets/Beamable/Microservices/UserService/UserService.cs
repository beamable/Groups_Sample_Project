using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beamable.Common;
using Beamable.Common.Models;
using Beamable.Common.Utils;
using Beamable.Mongo;
using Beamable.Server;
using MongoDB.Driver;
using UnityEngine;

namespace Beamable.Microservices
{
	[Microservice("UserService")]
	public class UserService : Microservice
	{
		[ClientCallable]
		public async Promise<Response<string>> GetPlayerAvatarName(long gamerTag)
		{
			try
			{
				var playerData = await Storage.GetByFieldName<PlayerData, long>("gamerTag", gamerTag);
				if (playerData != null && !string.IsNullOrEmpty(playerData.avatarName))
				{
					return new Response<string>(playerData.avatarName);
				}
				else
				{
					return new Response<string>("Avatar name not found");
				}
			}
			catch (Exception e)
			{
				BeamableLogger.LogError(e);
				return new Response<string>(e.Message);
			}
		}
		
		[ClientCallable]
		public async Promise<Response<bool>> SetPlayerAvatarName(long gamerTag, string avatarName)
		{
			try
			{
				var existingPlayerData = await Storage.GetByFieldName<PlayerData, string>("avatarName", avatarName);
				if (existingPlayerData != null)
				{
					return new Response<bool>(false, "Avatar name already exists");
				}

				await Storage.Create<UserGroupData, PlayerData>(new PlayerData
				{
					gamerTag = gamerTag, 
					avatarName = avatarName
				});
				return new Response<bool>(true);
			}
			catch (Exception e)
			{
				BeamableLogger.LogError(e);
				return new Response<bool>(false, "Error setting avatar name");
			}
		}
		
		[ClientCallable]
		public string Test(long lala, string lalala)
		{
			Debug.Log("Testt");
			return "test";

		}
	}
}
