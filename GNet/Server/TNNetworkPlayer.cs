using System.IO;
using System.Collections.Generic;
using System.Text;
using System;

namespace GNet
{
    public abstract class NetworkPlayer : Player
    {
        public NetworkPlayer(string name) : base(name)
        {

        }

        public string ConnectionId;
        public string GameServerId;

        public GList<Channel> channels = new GList<Channel>();

        public Action<ClientPlayer> ConnectedToHub;
        public Action<ClientPlayer> DisconnectedFromHub;
        public Action<ClientPlayer> ConnectedToGameServer;
        public Action<ClientPlayer> DisconnectedFromGameServer;
        public Action<ResponseConnectionIdPacket> ReceiveResponseConnectionIdPacket;
        public Action<CommandPacket> ReceiveClientDisconnectedPacket;
        public Action<CommandPacket> ReceiveResponseNewGameServerPacket;
        public Action<CommandPacket> ReceiveResponseServerListPacket;
        public Action<CommandPacket> ReceiveResponseJoinServerPacket;
        public Action<CommandPacket> ReceiveResponseLeaveServerPacket;
        public Action<CommandPacket> ReceiveResponseJoinChannelPacket;
        public Action<CommandPacket> ReceiveJoiningChannelPacket;
        public Action<CommandPacket> ReceiveUpdateChannelPacket;
        public Action<CommandPacket> ReceiveResponseLeaveChannelPacket;
        public Action<CommandPacket> ReceiveLoadLevelPacket;
        public Action<CommandPacket> ReceiveUnloadLevelPacket;
        public Action<CommandPacket> ReceiveCreateObjectPacket;
        public Action<CommandPacket> ReceiveDestroyObjectsPacket;
        public Action<CommandPacket> ReceiveForwardPacket;
        public Action<CommandPacket> ReceivePlayerJoinedChannelPacket;
        public Action<CommandPacket> ReceivePlayerLeftChannelPacket;
        public Action<CommandPacket> ReceiveResponseDestroyObjectsPacket;
        public Action<CommandPacket> ReceiveTransferredObjectPacket;
        public Action<CommandPacket> ReceiveResponseSetNamePacket;
        public Action<CommandPacket> ReceivePlayerChangedNamePacket;

        public bool isTryingToConnectToHub { get { return hubStage == Stage.Connecting; } }

        public bool isTryingToConnectToGameServer { get { return gameServerStage == Stage.Connecting; } }

        [DoNotObfuscate]
        public enum Stage
        {
            Connecting,
            Connected,
            Disconnecting,
            Disconnected
        }

        /// <summary>
        /// Current connection stage for hub.
        /// </summary>

        public Stage hubStage = Stage.Disconnected;

        /// <summary>
        /// Current connection stage for game server.
        /// </summary>

        public Stage gameServerStage = Stage.Disconnected;

        /// <summary>
        /// Whether the hub connection to the game server is currently active.
        /// </summary>

        public bool isConnectedToGameServer { get { return gameServerStage == Stage.Connected; } }

        /// <summary>
        /// Whether the connection is to the hub currently active.
        /// </summary>

        public bool isConnectedToHub { get { return hubStage == Stage.Connected; } }

        /// <summary>
        /// Whether the player is in the specified channel.
        /// </summary>

        public bool IsInChannel(int id)
        {
            for (int i = 0; i < channels.size; ++i)
                if (channels.buffer[i].id == id) return true;
            return false;
        }

        /// <summary>
        /// Return the specified channel if the player is currently within it, null otherwise.
        /// </summary>

        public Channel GetChannel(int id)
        {
            for (int i = 0; i < channels.size; ++i)
                if (channels.buffer[i].id == id) return channels.buffer[i];
            return null;
        }

        /// <summary>
        /// Whether this player has authenticated as an administrator.
        /// </summary>

        public bool isAdmin = false;

        /// <summary>
        /// Path where the player's data gets saved, if any.
        /// </summary>

        public string savePath;

        /// <summary>
        /// Next time the player data will be saved.
        /// </summary>

        public bool saveNeeded = false;

        /// <summary>
        /// Type of the saved data.
        /// </summary>

        public DataNode.SaveType saveType = DataNode.SaveType.Binary;


        /// <summary>
        /// Whether the specified player is already known to this one.
        /// </summary>

        public bool IsKnownTo(Player p, Channel ignoreChannel = null)
        {
            for (int i = 0; i < channels.size; ++i)
            {
                var ch = channels.buffer[i];
                if (ch == ignoreChannel) continue;
                if (ch.players.Contains(p)) return true;
            }
            return false;
        }

        public abstract void SendPacket(CommandPacket commandPacket);

        /// <summary>
        /// Assign an ID to this player.
        /// </summary>

        public abstract void AssignID();
    }
}
