using System;
using System.Threading.Tasks;
using Beamable.Server;
using MongoDB.Driver;
using UnityEngine;

namespace Beamable.Microservices
{
	[Microservice("UserGroupService")]
	public class UserGroupService : Microservice
	{
		[ClientCallable]
		public string Test()
		{
			Debug.Log("Testt");
			return "Test";
		}
		
		[ClientCallable]
		public async Task SaveUser(long gamerTag, string username)
		
		{
			Debug.Log("Before try");

			try
			{
				Debug.Log("In SaveUser");
				var db = await Storage.GetDatabase<UserGroupData>();
				Debug.Log("instantiated db");

				var collection = db.GetCollection<User>("data");
				Debug.Log("instantiated collection");

				await collection.InsertOneAsync(new User()
				{
					GamerTag = gamerTag,
					Username = username
				});
				Debug.Log("inserted user");

			}
			catch (Exception e)
			{
				System.Console.WriteLine(e.Message);
				throw;
			}

		}

		[ClientCallable]
		public async Task<string> GetUsername(long gamerTag)
		{
			try
			{
				var db = await Storage.GetDatabase<UserGroupData>();
				var collection = db.GetCollection<User>("data");
				var filter = Builders<User>.Filter.Eq("GamerTag", gamerTag);
				var user = await collection.Find(filter).FirstOrDefaultAsync();
				return user.Username;
			}
			catch (Exception e)
			{
				System.Console.WriteLine(e.Message);
				throw;
			}

		}
	}
}
