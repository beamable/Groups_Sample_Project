using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beamable.Common;
using Beamable.Common.Models;
using Beamable.Common.Utils;
using Beamable.Mongo;
using Beamable.Server;
using UnityEngine;

namespace Beamable.Microservices
{
    [Microservice("BackendService")]
    public class BackendService : Microservice
    {
        private const string GeneralRoomName = "General";

        public async Task SendNotification(List<long> playerIds, string context, object payload)
        {
            var jsonPayload = JsonUtility.ToJson(payload);
            Debug.Log($"Sending notification with payload: {jsonPayload}"); // Log the payload
            await Services.Notifications.NotifyPlayer(playerIds, context, jsonPayload);
        }
        
        [ClientCallable]
        public async Promise<Response<bool>> JoinRoom(long gamerTag, string roomName)
        {
            try
            {
                var roomData = await Storage.GetByFieldName<RoomData, string>("roomName", roomName);
                if (roomData == null)
                {
                    roomData = new RoomData { roomName = roomName };
                    roomData.memberGamerTags.Add(gamerTag);
                    await Storage.Create<BackendStorage, RoomData>(roomData);
                }
                else
                {
                    if (!roomData.memberGamerTags.Contains(gamerTag))
                    {
                        roomData.memberGamerTags.Add(gamerTag);
                        await Storage.Update(roomData.Id, roomData);
                    }
                }

                return new Response<bool>(true);
            }
            catch (Exception e)
            {
                BeamableLogger.LogError(e);
                return new Response<bool>(false, "Error joining room");
            }
        }

        [ClientCallable]
        public async Promise<Response<List<string>>> GetUserRooms(long gamerTag)
        {
            try
            {
                var allRooms = await Storage.GetAll<RoomData>();
                var userRooms = new List<string>();

                foreach (var roomData in allRooms)
                {
                    if (roomData.memberGamerTags.Contains(gamerTag))
                    {
                        userRooms.Add(roomData.roomName);
                    }
                }

                return new Response<List<string>>(userRooms);
            }
            catch (Exception e)
            {
                BeamableLogger.LogError(e);
                return new Response<List<string>>(null, "Error fetching user rooms");
            }
        }
        
        [ClientCallable]
        public async Promise<Response<bool>> LeaveRoom(long gamerTag, string roomName)
        {
            try
            {
                var roomData = await Storage.GetByFieldName<RoomData, string>("roomName", roomName);
                if (roomData != null && roomData.memberGamerTags.Contains(gamerTag))
                {
                    roomData.memberGamerTags.Remove(gamerTag);
                    if (roomData.memberGamerTags.Count == 0)
                    {
                        await Storage.Delete<BackendStorage, RoomData>(roomData.Id);
                    }
                    else
                    {
                        await Storage.Update(roomData.Id, roomData);
                    }
                }

                return new Response<bool>(true);
            }
            catch (Exception e)
            {
                BeamableLogger.LogError(e);
                return new Response<bool>(false, "Error leaving room");
            }
        }

        [ClientCallable]
        public async Promise<Response<bool>> SendMessage(long gamerTag, string roomName, string message)
        {
            try
            {
                var roomData = await Storage.GetByFieldName<RoomData, string>("roomName", roomName);
                if (roomData == null || !roomData.memberGamerTags.Contains(gamerTag))
                {
                    return new Response<bool>(false, "Room not found or user not a member");
                }

                var messageData = new MessageData
                {
                    senderGamerTag = gamerTag,
                    content = message,
                    timestamp = DateTime.UtcNow
                };

                roomData.messages.Add(messageData);
                await Storage.Update(roomData.Id, roomData);

                // Send a notification to all members of the room
                await SendNotification(roomData.memberGamerTags, roomName, messageData);

                return new Response<bool>(true);
            }
            catch (Exception e)
            {
                BeamableLogger.LogError(e);
                return new Response<bool>(false, "Error sending message");
            }
        }

        [ClientCallable]
        public async Promise<Response<List<MessageData>>> GetRoomHistory(string roomName)
        {
            try
            {
                var roomData = await Storage.GetByFieldName<RoomData, string>("roomName", roomName);
                if (roomData == null)
                {
                    return new Response<List<MessageData>>(null, "Room not found");
                }

                return new Response<List<MessageData>>(roomData.messages);
            }
            catch (Exception e)
            {
                BeamableLogger.LogError(e);
                return new Response<List<MessageData>>(null, "Error retrieving room history");
            }
        }
        
        [ClientCallable]
        public async Task EnsureGeneralRoomExists()
        {
            try
            {
                var existingRoom = await Storage.GetByFieldName<RoomData, string>("roomName", GeneralRoomName);
                if (existingRoom == null)
                {
                    var newGeneralRoom = new RoomData { roomName = GeneralRoomName };
                    await Storage.Create<BackendStorage, RoomData>(newGeneralRoom);
                    Debug.Log("General Room created.");
                }
                else
                {
                    Debug.Log("General Room already exists.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error ensuring General Room exists: {e.Message}");
            }
        }
    }
}
