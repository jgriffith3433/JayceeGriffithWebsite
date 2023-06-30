using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine.LowLevel;

namespace GNet
{
    public static class CommandPacketExtensions
    {
        public static string GetHubTarget(this CommandPacket sendCommandPacket)
        {
            return "OnReceive" + sendCommandPacket.GetType().Name;
        }
    }

    public abstract class CommandPacket
    {
    }

    public class ClientDisconnectedPacket : CommandPacket
    {
        public ClientDisconnectedPacket() { }
        public ClientDisconnectedPacket(string clientConnectionId)
        {
            ClientConnectionId = clientConnectionId;
        }

        public string ClientConnectionId;
    }

    public class RequestNewGameServerPacket : CommandPacket
    {
        public RequestNewGameServerPacket() { }
        public RequestNewGameServerPacket(ushort gameId, string name, int playerCount)
        {
            GameId = gameId;
            Name = name;
            PlayerCount = playerCount;
        }
        public ushort GameId;
        public string Name;
        public int PlayerCount;
    }

    public class ResponseNewGameServerPacket : CommandPacket
    {
        public ResponseNewGameServerPacket() { }
        public ResponseNewGameServerPacket(string serverId)
        {
            ServerId = serverId;
        }

        public string ServerId;
    }

    public class RequestStopGameServerPacket : CommandPacket
    {
        public RequestStopGameServerPacket() { }
        public RequestStopGameServerPacket(string serverId)
        {
            ServerId = serverId;
        }
        public string ServerId;
    }

    public class ResponseStopGameServerPacket : CommandPacket
    {
        public ResponseStopGameServerPacket() { }
        public ResponseStopGameServerPacket(string serverId)
        {
            ServerId = serverId;
        }

        public string ServerId;
    }

    public class RequestConnectionIdPacket : CommandPacket
    {
        public RequestConnectionIdPacket() { }
    }

    public class ResponseConnectionIdPacket : CommandPacket
    {
        public ResponseConnectionIdPacket() { }
        public ResponseConnectionIdPacket(string connectionId)
        {
            ConnectionId = connectionId;
        }

        public string ConnectionId;
    }

    public class RequestServerListPacket : CommandPacket
    {
        public RequestServerListPacket() { }
        public RequestServerListPacket(ushort gameId)
        {
            GameId = gameId;
        }
        public ushort GameId;
    }

    public class ResponseServerListPacket : CommandPacket
    {
        public ResponseServerListPacket() { }
        public ResponseServerListPacket(List<GameServerInfo> servers)
        {
            Servers = servers;
        }

        public List<GameServerInfo> Servers;
    }

    public class RequestVerifyAdminPacket : CommandPacket
    {
        public RequestVerifyAdminPacket() { }
    }

    public class RequestJoinServerPacket : CommandPacket
    {
        public RequestJoinServerPacket() { }
        public RequestJoinServerPacket(string serverId)
        {
            ServerId = serverId;
        }
        public string ServerId;
    }

    public class ResponseJoinServerPacket : CommandPacket
    {
        public ResponseJoinServerPacket() { }
        public ResponseJoinServerPacket(string serverId, int playerId, bool success, string name)
        {
            ServerId = serverId;
            PlayerId = playerId;
            Success = success;
            Name = name;
        }
        public string ServerId;
        public int PlayerId;
        public bool Success;
        public string Name;
    }

    public class RequestLeaveServerPacket : CommandPacket
    {
        public RequestLeaveServerPacket() { }
        public RequestLeaveServerPacket(string serverId, int playerId)
        {
            ServerId = serverId;
            PlayerId = playerId;
        }
        public string ServerId;
        public int PlayerId;
    }

    public class ResponseLeaveServerPacket : CommandPacket
    {
        public ResponseLeaveServerPacket() { }
        public ResponseLeaveServerPacket(string serverId, int playerId, bool success)
        {
            ServerId = serverId;
            PlayerId = playerId;
            Success = success;
        }
        public string ServerId;
        public int PlayerId;
        public bool Success;
    }

    public class ResponseSetHostPacket : CommandPacket
    {
        public ResponseSetHostPacket() { }
        public ResponseSetHostPacket(int channelId, int playerId)
        {
            ChannelId = channelId;
            PlayerId = playerId;
        }

        public int ChannelId;
        public int PlayerId;
    }

    public class RequestDestroyObject : CommandPacket
    {
        public RequestDestroyObject() { }
        public RequestDestroyObject(int channelId, uint uid)
        {
            ChannelId = channelId;
            Uid = uid;
        }

        public int ChannelId;
        public uint Uid;
    }

    public class DestroyObjectsPacket : CommandPacket
    {
        public DestroyObjectsPacket() { }

        public DestroyObjectsPacket(int channelId, uint[] objectsToDestroy)
        {
            ChannelId = channelId;
            ObjectsToDestroy = objectsToDestroy;
        }

        public int ChannelId;
        public uint[] ObjectsToDestroy;
    }

    public class ResponseDestroyObjectsPacket : CommandPacket
    {
        public ResponseDestroyObjectsPacket() { }
        public ResponseDestroyObjectsPacket(int channelId, int playerId, List<uint> destroyedObjects)
        {
            ChannelId = channelId;
            PlayerId = playerId;
            DestroyedObjects = destroyedObjects;
        }

        public int ChannelId;
        public int PlayerId;
        public List<uint> DestroyedObjects;
    }

    public class RequestJoinChannelPacket : CommandPacket
    {
        public RequestJoinChannelPacket() { }

        public RequestJoinChannelPacket(int channelId, string password, string levelName, bool persistent, ushort playerLimit, bool additive)
        {
            ChannelId = channelId;
            Password = password;
            LevelName = levelName;
            Persistent = persistent;
            //PlayerLimit = playerLimit;
            Additive = additive;
        }

        public int ChannelId;
        public string Password;
        public string LevelName;
        public bool Persistent;
        public bool Additive;
    }

    public class ResponseJoinChannelPacket : CommandPacket
    {
        public ResponseJoinChannelPacket() { }

        public ResponseJoinChannelPacket(int channelId, bool success = true, string message = null)
        {
            ChannelId = channelId;
            Success = success;
            Message = message;
        }

        public int ChannelId;
        public bool Success;
        public string Message;
    }

    public class RequestLeaveChannelPacket : CommandPacket
    {
        public RequestLeaveChannelPacket() { }

        public RequestLeaveChannelPacket(int channelId, int playerId)
        {
            ChannelId = channelId;
        }

        public int ChannelId;
    }

    public class ResponseLeaveChannelPacket : CommandPacket
    {
        public ResponseLeaveChannelPacket() { }

        public ResponseLeaveChannelPacket(int channelId, bool success = true, string message = null)
        {
            ChannelId = channelId;
            Success = success;
            Message = message;
        }

        public int ChannelId;
        public bool Success;
        public string Message;
    }

    public class PlayerJoinedChannelPacket : CommandPacket
    {
        public PlayerJoinedChannelPacket() { }

        public PlayerJoinedChannelPacket(int channelId, int playerId, string playerName, DataNode playerDataNode)
        {
            ChannelId = channelId;
            PlayerId = playerId;
            PlayerName = playerName;
            PlayerDataNode = playerDataNode;
        }

        public int ChannelId;
        public int PlayerId;
        public string PlayerName;
        public DataNode PlayerDataNode;
    }

    public class JoiningChannelPacket : CommandPacket
    {
        public class PlayerInChannel
        {
            public PlayerInChannel() { }

            public PlayerInChannel(int playerId, string playerName, DataNode dataNode)
            {
                PlayerId = playerId;
                PlayerName = playerName;
                DataNode = dataNode;
            }

            public int PlayerId;
            public string PlayerName;
            public DataNode DataNode;
        }

        public JoiningChannelPacket() { }

        public JoiningChannelPacket(int channelId, List<PlayerInChannel> playersInChannel, int hostPlayerId, DataNode channelDataNode)
        {
            ChannelId = channelId;
            PlayersInChannel = playersInChannel;
            HostPlayerId = hostPlayerId;
            ChannelDataNode = channelDataNode;
        }

        public int ChannelId;
        public List<PlayerInChannel> PlayersInChannel;
        public int HostPlayerId;
        public DataNode ChannelDataNode;
    }

    public class PlayerLeftChannelPacket : CommandPacket
    {
        public PlayerLeftChannelPacket() { }

        public PlayerLeftChannelPacket(int channelId, int playerId, string playerName, DataNode playerDataNode)
        {
            ChannelId = channelId;
            PlayerId = playerId;
            PlayerName = playerName;
            PlayerDataNode = playerDataNode;
        }

        public int ChannelId;
        public int PlayerId;
        public string PlayerName;
        public DataNode PlayerDataNode;
    }

    public class LoadLevelPacket : CommandPacket
    {
        public LoadLevelPacket() { }

        public LoadLevelPacket(int channelId, string levelName, bool additive)
        {
            ChannelId = channelId;
            LevelName = levelName;
            Additive = additive;
        }

        public int ChannelId;
        public string LevelName;
        public bool Additive;
    }

    public class UnloadLevelPacket : CommandPacket
    {
        public UnloadLevelPacket() { }

        public UnloadLevelPacket(string levelName)
        {
            LevelName = levelName;
        }

        public string LevelName;
    }

    public class RequestCreateObject : CommandPacket
    {
        public RequestCreateObject() { }
        public RequestCreateObject(int channelId, int playerId, bool persistent, byte[] objsData)
        {
            ChannelId = channelId;
            PlayerId = playerId;
            Persistent = persistent;
            ObjsData = objsData;
        }

        public int ChannelId;
        public int PlayerId;
        public bool Persistent;
        public byte[] ObjsData;
    }

    public class CreateObjectPacket : CommandPacket
    {
        public CreateObjectPacket() { }

        public CreateObjectPacket(int playerId, int channelId, uint objectId, byte[] objectData)
        {
            PlayerId = playerId;
            ChannelId = channelId;
            ObjectId = objectId;
            ObjectData = objectData;
        }

        public int PlayerId;
        public int ChannelId;
        public uint ObjectId;
        public byte[] ObjectData;
    }

    public class ForwardPacket : CommandPacket
    {
        public ForwardPacket() { }

        public ForwardPacket(int channelId, uint uid, string functionName, ForwardType forwardType, byte[] data)
        {
            ChannelId = channelId;
            Uid = uid;
            FunctionName = functionName;
            ForwardType = (int)forwardType;
            Data = data;
        }

        public int ChannelId;
        public uint Uid;
        public string FunctionName;
        public int ForwardType;
        public byte[] Data;

        public uint GetObjectID()
        {
            return (Uid >> 8);
        }

        public void SetObjectId(uint value)
        {
            Uid = (value << 8) | (Uid & 0xFF);
        }
        public uint GetFunctionID()
        {
            return (Uid & 0xFF);
        }
    }

    public class ForwardToPlayerPacket : CommandPacket
    {
        public ForwardToPlayerPacket() { }

        public ForwardToPlayerPacket(int channelId, uint uid, int fromPlayerId, int toPlayerId, string functionName, byte[] data)
        {
            ChannelId = channelId;
            Uid = uid;
            FunctionName = functionName;
            Data = data;
            FromPlayerId = fromPlayerId;
            ToPlayerId = toPlayerId;
        }

        public int ChannelId;
        public int FromPlayerId;
        public int ToPlayerId;
        public uint Uid;
        public string FunctionName;
        public byte[] Data;

        //TODO: Make sure these properties aren't being serialized
        public uint objectID { get { return (Uid >> 8); } set { Uid = ((value << 8) | (Uid & 0xFF)); } }
        public uint functionID { get { return (Uid & 0xFF); } }
    }

    public class UpdateChannelPacket : CommandPacket
    {
        public UpdateChannelPacket() { }

        public UpdateChannelPacket(int channelId, ushort playerLimit, ushort flags)
        {
            ChannelId = channelId;
            PlayerLimit = playerLimit;
            Flags = flags;
        }

        public int ChannelId;
        public ushort PlayerLimit;
        public ushort Flags;
    }

    public class RequestSetNamePacket : CommandPacket
    {
        public RequestSetNamePacket() { }

        public RequestSetNamePacket(string name)
        {
            Name = name;
        }

        public string Name;
    }

    public class ResponseSetNamePacket : CommandPacket
    {
        public ResponseSetNamePacket() { }

        public ResponseSetNamePacket(bool success, string name)
        {
            Success = success;
            Name = name;
        }

        public bool Success;
        public string Name;
    }

    public class PlayerChangedNamePacket : CommandPacket
    {
        public PlayerChangedNamePacket() { }

        public PlayerChangedNamePacket(int playerId, string name)
        {
            PlayerId = playerId;
            Name = name;
        }

        public int PlayerId;
        public string Name;
    }

    /// <summary>
    /// Notification that the specified object has been transferred to another channel.
    /// This notification is only sent to players that are in both channels.
    /// int32: ID of the player that sent the request packet.
    /// int32: Old channel ID.
    /// int32: New channel ID.
    /// uint32: Old object ID.
    /// uint32: New object ID.
    /// </summary>
    public class RequestTransferObjectPacket : CommandPacket
    {
        public RequestTransferObjectPacket() { }

        public RequestTransferObjectPacket(int playerId, int oldChannelId, int newChannelId, uint oldObjectId)
        {
            PlayerId = playerId;
            OldChannelId = oldChannelId;
            NewChannelId = newChannelId;
            OldObjectId = oldObjectId;
        }

        public int PlayerId;
        public int OldChannelId;
        public int NewChannelId;
        public uint OldObjectId;
    }

    public class TransferredObjectPacket : CommandPacket
    {
        public TransferredObjectPacket() { }

        public TransferredObjectPacket(int playerId, int oldChannelId, int newChannelId, uint oldObjectId, uint newObjectId)
        {
            PlayerId = playerId;
            OldChannelId = oldChannelId;
            NewChannelId = newChannelId;
            OldObjectId = oldObjectId;
            NewObjectId = newObjectId;
        }

        public int PlayerId;
        public int OldChannelId;
        public int NewChannelId;
        public uint OldObjectId;
        public uint NewObjectId;
    }

}
