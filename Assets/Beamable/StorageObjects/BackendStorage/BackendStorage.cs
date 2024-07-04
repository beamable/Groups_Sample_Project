using Beamable.Common;
using MongoDB.Driver;

namespace Beamable.Server
{
	[StorageObject("BackendStorage")]
	public class BackendStorage : MongoStorageObject
	{
	}

	public static class BackendStorageExtension
	{
		/// <summary>
		/// Get an authenticated MongoDB instance for BackendStorage
		/// </summary>
		/// <returns></returns>
		public static Promise<IMongoDatabase> BackendStorageDatabase(this IStorageObjectConnectionProvider provider)
			=> provider.GetDatabase<BackendStorage>();

		/// <summary>
		/// Gets a MongoDB collection from BackendStorage by the requested name, and uses the given mapping class.
		/// If you don't want to pass in a name, consider using <see cref="BackendStorageCollection{TCollection}()"/>
		/// </summary>
		/// <param name="name">The name of the collection</param>
		/// <typeparam name="TCollection">The type of the mapping class</typeparam>
		/// <returns>When the promise completes, you'll have an authorized collection</returns>
		public static Promise<IMongoCollection<TCollection>> BackendStorageCollection<TCollection>(
			this IStorageObjectConnectionProvider provider, string name)
			where TCollection : StorageDocument
			=> provider.GetCollection<BackendStorage, TCollection>(name);

		/// <summary>
		/// Gets a MongoDB collection from BackendStorage by the requested name, and uses the given mapping class.
		/// If you want to control the collection name separate from the class name, consider using <see cref="BackendStorageCollection{TCollection}(string)"/>
		/// </summary>
		/// <param name="name">The name of the collection</param>
		/// <typeparam name="TCollection">The type of the mapping class</typeparam>
		/// <returns>When the promise completes, you'll have an authorized collection</returns>
		public static Promise<IMongoCollection<TCollection>> BackendStorageCollection<TCollection>(
			this IStorageObjectConnectionProvider provider)
			where TCollection : StorageDocument
			=> provider.GetCollection<BackendStorage, TCollection>();
	}
}
