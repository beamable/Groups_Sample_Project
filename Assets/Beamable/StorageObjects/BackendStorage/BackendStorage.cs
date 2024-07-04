using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Beamable.Common;
using Beamable.Common.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Beamable.Server
{
    [StorageObject("BackendStorage")]
    public class BackendStorage : MongoStorageObject
    {
    }

    public static class BackendStorageExtension
    {
        public static Promise<IMongoDatabase> RoomDataDatabase(this IStorageObjectConnectionProvider provider)
            => provider.GetDatabase<BackendStorage>();

        public static Promise<IMongoCollection<TCollection>> RoomDataCollection<TCollection>(
            this IStorageObjectConnectionProvider provider, string name)
            where TCollection : StorageDocument
            => provider.GetCollection<BackendStorage, TCollection>(name);

        public static Promise<IMongoCollection<TCollection>> RoomDataCollection<TCollection>(
            this IStorageObjectConnectionProvider provider)
            where TCollection : StorageDocument
            => provider.GetCollection<BackendStorage, TCollection>();

        public static async Promise<bool> Update<T>(this IStorageObjectConnectionProvider provider, string id,
            T updatedData) where T : StorageDocument, ISetStorageDocument<T>
        {
            var collection = await provider.GetCollection<BackendStorage, T>();
            var documentToUpdate = await provider.GetById<T>(id);
            if (documentToUpdate == null)
                return false;

            documentToUpdate.Set(updatedData);
            var result = await collection.ReplaceOneAsync(provider.GetFilterById<T>(id), documentToUpdate);
            return result.ModifiedCount > 0;
        }

        public static async Promise<List<T>> GetAll<T>(this IStorageObjectConnectionProvider provider)
            where T : StorageDocument
        {
            var collection = await provider.GetCollection<BackendStorage, T>();
            return collection.Find(data => true).ToList();
        }

        public static async Promise<T> GetById<T>(this IStorageObjectConnectionProvider provider, string id)
            where T : StorageDocument
        {
            var collection = await provider.GetCollection<BackendStorage, T>();
            var search = await collection.FindAsync(provider.GetFilterById<T>(id));
            return search.FirstOrDefault();
        }

        public static async Promise<T> GetByFieldName<T, TValue>(this IStorageObjectConnectionProvider provider,
            string field, TValue value) where T : StorageDocument
        {
            var collection = await provider.GetCollection<BackendStorage, T>();
            var search = await collection.FindAsync(provider.GetFilterByField<T, TValue>(field, value));
            return search.FirstOrDefault();
        }

        public static async Promise<List<T>> GetAllByFieldName<T, TValue>(
            this IStorageObjectConnectionProvider provider, Expression<Func<T, TValue>> field,
            IEnumerable<TValue> values) where T : StorageDocument
        {
            var collection = await provider.GetCollection<BackendStorage, T>();
            var search = await collection.FindAsync(provider.GetAllFilterByField(field, values));
            return search.ToList();
        }

        private static FilterDefinition<T> GetFilterById<T>(this IStorageObjectConnectionProvider provider, string id)
            where T : StorageDocument
            => Builders<T>.Filter.Eq("_id", new ObjectId(id));

        private static FilterDefinition<T> GetFilterByField<T, TValue>(this IStorageObjectConnectionProvider provider,
            string field, TValue value) where T : StorageDocument
            => Builders<T>.Filter.Eq(field, value);

        private static FilterDefinition<T> GetAllFilterByField<T, TValue>(
            this IStorageObjectConnectionProvider provider, Expression<Func<T, TValue>> field,
            IEnumerable<TValue> values) where T : StorageDocument
            => Builders<T>.Filter.In(field, values);
    }
}
