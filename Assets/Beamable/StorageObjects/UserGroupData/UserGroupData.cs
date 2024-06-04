using Beamable.Common;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Beamable.Server
{
	[StorageObject("UserGroupData")]
	public class UserGroupData : MongoStorageObject
	{
	}

	public class User
	{
		public ObjectId ID;
		public long GamerTag;
		public string Username;
	}

	public static class UserGroupDataExtension
	{
		/// <summary>
		/// Get an authenticated MongoDB instance for UserGroupData
		/// </summary>
		/// <returns></returns>
		public static Promise<IMongoDatabase> UserGroupDataDatabase(this IStorageObjectConnectionProvider provider)
			=> provider.GetDatabase<UserGroupData>();

		/// <summary>
		/// Gets a MongoDB collection from UserGroupData by the requested name, and uses the given mapping class.
		/// If you don't want to pass in a name, consider using <see cref="UserGroupDataCollection{TCollection}()"/>
		/// </summary>
		/// <param name="name">The name of the collection</param>
		/// <typeparam name="TCollection">The type of the mapping class</typeparam>
		/// <returns>When the promise completes, you'll have an authorized collection</returns>
		public static Promise<IMongoCollection<TCollection>> UserGroupDataCollection<TCollection>(
			this IStorageObjectConnectionProvider provider, string name)
			where TCollection : StorageDocument
			=> provider.GetCollection<UserGroupData, TCollection>(name);

		/// <summary>
		/// Gets a MongoDB collection from UserGroupData by the requested name, and uses the given mapping class.
		/// If you want to control the collection name separate from the class name, consider using <see cref="UserGroupDataCollection{TCollection}(string)"/>
		/// </summary>
		/// <param name="name">The name of the collection</param>
		/// <typeparam name="TCollection">The type of the mapping class</typeparam>
		/// <returns>When the promise completes, you'll have an authorized collection</returns>
		public static Promise<IMongoCollection<TCollection>> UserGroupDataCollection<TCollection>(
			this IStorageObjectConnectionProvider provider)
			where TCollection : StorageDocument
			=> provider.GetCollection<UserGroupData, TCollection>();
	}
}
