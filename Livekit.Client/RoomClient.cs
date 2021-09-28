using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Livekit.Client.Proto;

namespace Livekit.Client
{
    public class RoomClient
    {
        private Grpc.Core.ChannelBase Channel;
        private RoomService.RoomServiceClient ServiceClient;
        private AccessToken Token;

        /// <summary>
        /// Create a new GRPC channel, create a RoomServiceClient to be reused by the class functions
        /// </summary>
        /// <param name="address">Livekit server address (https://my.host)</param>
        /// <param name="apiKey">Server API key</param>
        /// <param name="apiSecret">Server API secret</param>
        public RoomClient(string address, string apiKey, string apiSecret)
        {
            Channel = GrpcChannel.ForAddress(address);
            ServiceClient = new RoomService.RoomServiceClient(Channel);

            Token = new AccessToken(apiKey, apiSecret, new Grant()
            {
                video = new VideoGrant()
                {
                    roomCreate = true,
                    roomList = true,
                    roomAdmin = true
                }
            });
        }

        /// <summary>
        /// Create a new room
        /// </summary>
        /// <param name="name">Room name</param>
        /// <param name="emptyTimeout">Timeout to delete the room if empty</param>
        /// <param name="maxParticipants">Maximum number of participants</param>
        /// <param name="nodeId">The ID of the node to be used for hosting the room</param>
        /// <returns>The created Room details</returns>
        public Room CreateRoom(string name, uint emptyTimeout = 0, uint maxParticipants = 0, string nodeId = "")
        {
            var req = new CreateRoomRequest()
            {
                Name = name,
                EmptyTimeout = emptyTimeout,
                MaxParticipants = maxParticipants,
                NodeId = nodeId,
            };
            return ServiceClient.CreateRoom(req, Token.GetAsHeader(name));
        }

        /// <summary>
        /// List active rooms
        /// </summary>
        /// <returns>Iterateable collection of Rooms</returns>
        public Google.Protobuf.Collections.RepeatedField<Room> ListRooms()
        {
            var req = new ListRoomsRequest();
            return ServiceClient.ListRooms(req, Token.GetAsHeader()).Rooms;
        }

        /// <summary>
        /// Delete the given room
        /// </summary>
        /// <param name="room">Room name</param>
        public void DeleteRoom(string room)
        {
            var req = new DeleteRoomRequest()
            {
                Room = room
            };
            ServiceClient.DeleteRoom(req, Token.GetAsHeader(room));
        }

        /// <summary>
        /// List participants of given room
        /// </summary>
        /// <param name="room">Room name</param>
        /// <returns>Repeated field of ParticipantInfo</returns>
        public Google.Protobuf.Collections.RepeatedField<ParticipantInfo> ListParticipants(string room)
        {
            var req = new ListParticipantsRequest()
            {
                Room = room
            };
            return ServiceClient.ListParticipants(req, Token.GetAsHeader(room)).Participants;
        }

        /// <summary>
        /// Get a participant of the given room
        /// </summary>
        /// <param name="room">Room name</param>
        /// <param name="identity">Identity to get</param>
        /// <returns>ParticipantInfo of the given identity</returns>
        public ParticipantInfo GetParticipant(string room, string identity)
        {
            var req = new RoomParticipantIdentity()
            {
                Room = room,
                Identity = identity
            };
            return ServiceClient.GetParticipant(req, Token.GetAsHeader(room));
        }

        /// <summary>
        /// Remove (kick [not banning]) a participant from the room
        /// </summary>
        /// <param name="room">Room name</param>
        /// <param name="identity">Identity to remove</param>
        public void RemoveParticipant(string room, string identity)
        {
            var req = new RoomParticipantIdentity()
            {
                Room = room,
                Identity = identity
            };
            ServiceClient.RemoveParticipant(req, Token.GetAsHeader(room));
        }

        /// <summary>
        /// Mute/Unmute a track of an identity
        /// </summary>
        /// <param name="room">Room name</param>
        /// <param name="identity">Identity to set</param>
        /// <param name="track">Track SID</param>
        /// <param name="muted">Is the track muted</param>
        /// <returns>TrackInfo of the given track</returns>
        public TrackInfo MutePublishedTrack(string room, string identity, string track, bool muted)
        {
            var req = new MuteRoomTrackRequest()
            {
                Room = room,
                Identity = identity,
                TrackSid = track,
                Muted = muted
            };
            return ServiceClient.MutePublishedTrack(req, Token.GetAsHeader(room)).Track;
        }

        /// <summary>
        /// Update the metadata and permissions of the given identity
        /// </summary>
        /// <param name="room">Room name</param>
        /// <param name="identity">Identity to set</param>
        /// <param name="metadata">Metadata to edit (optional)</param>
        /// <param name="permission">Permission to edit</param>
        /// <returns>ParticipantInfo of the given identity</returns>
        public ParticipantInfo UpdateParticipant(string room, string identity, string metadata = null, ParticipantPermission permission = null)
        {
            var req = new UpdateParticipantRequest()
            {
                Room = room,
                Identity = identity,
                Metadata = metadata,
                Permission = permission
            };
            return ServiceClient.UpdateParticipant(req, Token.GetAsHeader(room));
        }

        /// <summary>
        /// Add or remove Subscriptions of an identity
        /// </summary>
        /// <param name="room">Room name</param>
        /// <param name="identity">Identity to edit</param>
        /// <param name="tracks">Track(s) to add or remove</param>
        /// <param name="subscribe">Add or remove the track(s)</param>
        public void UpdateSubscriptions(string room, string identity, string[] tracks, bool subscribe)
        {
            var req = new UpdateSubscriptionsRequest()
            {
                Room = room,
                Identity = identity,
                Subscribe = subscribe
            };
            req.TrackSids.AddRange(tracks);
            ServiceClient.UpdateSubscriptions(req, Token.GetAsHeader(room));
        }

        /// <summary>
        /// Send data to participants
        /// </summary>
        /// <param name="room">Room name</param>
        /// <param name="kind">Reliable, lossy</param>
        /// <param name="data">Data to send</param>
        /// <param name="sids">Participants to send to, when empty sends to all users</param>
        public void SendData(string room, DataPacket.Types.Kind kind, Google.Protobuf.ByteString data, string[] sids = null)
        {
            var req = new SendDataRequest()
            {
                Room = room,
                Kind = kind,
                Data = data
            };
            req.DestinationSids.AddRange(sids);
            ServiceClient.SendData(req, Token.GetAsHeader(room));
        }

        /// <summary>
        /// Set the metadata string to a room
        /// </summary>
        /// <param name="room">Room name</param>
        /// <param name="metadata">Metadata string</param>
        public void UpdateRoomMetadata(string room, string metadata)
        {
            var req = new UpdateRoomMetadataRequest()
            {
                Room = room,
                Metadata = metadata
            };
            ServiceClient.UpdateRoomMetadata(req, Token.GetAsHeader(room));
        }

        ~RoomClient()
        {
            Channel.ShutdownAsync().Wait();
        }
    }
}
