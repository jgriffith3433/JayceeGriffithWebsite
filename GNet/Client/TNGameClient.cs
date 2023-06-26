//-------------------------------------------------
//                    GNet 3
// Copyright © 2012-2018 Tasharen Entertainment Inc
//-------------------------------------------------

#define USE_MAX_PACKET_TIME
//#define COUNT_PACKETS
//#define PROFILE_PACKETS

#pragma warning disable 0162

using System;
using System.IO;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEngine;
#endif

namespace GNet
{
    /// <summary>
    /// Client-side logic.
    /// </summary>

    public class GameClient : TNEvents
    {
#if USE_MAX_PACKET_TIME
        // Maximum amount of time to spend processing packets per frame, in milliseconds.
        // Useful for breaking up a stream of packets, ensuring that they get processed across multiple frames.
        const long MaxPacketTime = 20;
#endif
        /// <summary>
        /// Custom packet listeners. You can set these to handle custom packets.
        /// </summary>

        public Dictionary<byte, OnPacket> packetHandlers = new Dictionary<byte, OnPacket>();
        public delegate void OnPacket(Packet response, BinaryReader reader, string endPoint);

        /// <summary>
        /// Whether the game client should be actively processing messages or not.
        /// </summary>

        public bool isActive = true;

        // List of players in a dictionary format for quick lookup
        Dictionary<int, Player> mDictionary = new Dictionary<int, Player>();

        // TCP connection is the primary method of communication with the server.
        public ClientPlayer m_ClientPlayer = null;

        /// <summary>
        /// Current connection stage for the hub.
        /// </summary>

        public NetworkPlayer.Stage hubStage { get { return m_ClientPlayer.hubStage; } }

        /// <summary>
        /// Current connection stage for the game server.
        /// </summary>
        public NetworkPlayer.Stage gameServerStage { get { return m_ClientPlayer.gameServerStage; } }

        // Current time, time when the last ping was sent out, and time when connection was started
        long mTimeDifference = 0;
        long mMyTime = 0;
        long mStartTime = 0;

#if !MODDING
        long mPingTime = 0;
        long mNextReset = 0;
#endif

        // Last ping, and whether we can ping again
        int mPing = 0;

        // Each GetFileList() call can specify its own callback
        Dictionary<string, OnGetFiles> mGetFiles = new Dictionary<string, OnGetFiles>();
        public delegate void OnGetFiles(string path, string[] files);

        // Each LoadFile() call can specify its own callback
        Dictionary<string, OnLoadFile> mLoadFiles = new Dictionary<string, OnLoadFile>();
        public delegate void OnLoadFile(string filename, byte[] data);

        bool mIsAdmin = false;

        // List of channels we are currently in the process of joining
        GList<int> mJoining = new GList<int>();

        // Server configuration data
        DataNode mConfig = new DataNode("Version", Player.version);
        int mDataHash = 0;

        /// <summary>
        /// Whether the player has verified himself as an administrator.
        /// </summary>

        public bool isAdmin { get { return mIsAdmin; } }

        /// <summary>
        /// Set administrator privileges. Note that failing the password test will cause a disconnect.
        /// </summary>

        public void SetAdmin(string pass)
        {
            mIsAdmin = true;
            BeginSend(Packet.RequestVerifyAdmin).Write(pass);
            EndSend();
        }

        public BinaryWriter BeginSend(Packet packet)
        {
            return null;
        }
        public BinaryWriter BeginSend(int packet)
        {
            return null;
        }


        public void EndSend()
        {

        }
        public BinaryWriter EndSend(int channelID)
        {
            return null;
        }

        public void CancelSend()
        {

        }

        /// <summary>
        /// Perform the server configuration hash validation against the current data in memory. Useful for detecting memory modification.
        /// </summary>

        public bool ValidateHash()
        {
            if (mConfig.children.size == 0) return true;
            return mDataHash == mConfig.CalculateHash();
        }

        /// <summary>
        /// Request the server-side validation of the specified property.
        /// </summary>

        public void Validate(string name, object val)
        {
            if (isTryingToConnectToGameServer)
            {
                var writer = BeginSend(Packet.RequestValidate);
                writer.Write(name);
                writer.WriteObject(val);
                EndSend();
            }
        }

        /// <summary>
        /// Channels the player belongs to. Don't modify this list.
        /// </summary>

        public GNet.GList<Channel> channels { get { return m_ClientPlayer.channels; } }

        /// <summary>
        /// Current time on the server in milliseconds.
        /// </summary>

        public long serverTime { get { return mTimeDifference + mMyTime; } }

        /// <summary>
        /// Server's uptime in milliseconds.
        /// </summary>

        public long serverUptime { get { return serverTime - mStartTime; } }

        /// <summary>
        /// Whether the client is currently connected to the hub.
        /// </summary>

        public bool isConnectedToHub { get { return m_ClientPlayer != null && m_ClientPlayer.isConnectedToHub; } }

        /// <summary>
        /// Whether the client is currently connected to the game server.
        /// </summary>

        public bool isConnectedToGameServer { get { return m_ClientPlayer != null && m_ClientPlayer.isConnectedToGameServer; } }

        /// <summary>
        /// Whether we are currently trying to establish a new connection to the hub.
        /// </summary>

        public bool isTryingToConnectToHub { get { return m_ClientPlayer != null && m_ClientPlayer.isTryingToConnectToHub; } }

        /// <summary>
        /// Whether we are currently trying to establish a new connection to the game server
        /// </summary>

        public bool isTryingToConnectToGameServer { get { return m_ClientPlayer != null && m_ClientPlayer.isTryingToConnectToGameServer; } }

        /// <summary>
        /// Whether we are currently in the process of joining a channel.
        /// To find out whether we are joining a specific channel, use the "IsJoiningChannel(id)" function.
        /// </summary>

        public bool isJoiningChannel { get { return mJoining.size != 0; } }

        /// <summary>
        /// Whether the client is currently in a channel.
        /// </summary>

        public bool isInChannel { get { return m_ClientPlayer.channels.size != 0; } }

        /// <summary>
        /// Current ping to the server.
        /// </summary>

        public int ping { get { return isConnectedToHub ? mPing : 0; } }

        public DataNode serverData
        {
            get
            {
                return mConfig;
            }
            set
            {
                if (isAdmin)
                {
                    mConfig = value;
#if !MODDING
                    var writer = BeginSend(Packet.RequestSetServerData);
                    writer.Write("");
                    writer.WriteObject(value);
                    EndSend();
#endif
                }
            }
        }

        /// <summary>
        /// Return the client player.
        /// </summary>

        public ClientPlayer player { get { return m_ClientPlayer; } }

        /// <summary>
        /// The player's unique identifier.
        /// </summary>

        public int playerID { get { return m_ClientPlayer.id; } }

        /// <summary>
        /// Name of this player.
        /// </summary>

        public string playerName
        {
            get
            {
                return m_ClientPlayer.name;
            }
            set
            {
                if (m_ClientPlayer.name != value)
                {
#if !MODDING
                    if (isConnectedToGameServer)
                    {
                        SendPacket(new RequestSetNamePacket(value));
                    }
                    else m_ClientPlayer.name = value;
#else
					mTcp.name = value;
#endif
                }
            }
        }

        /// <summary>
        /// Get or set the player's data. Read-only. Use SetPlayerData to change the contents.
        /// </summary>

        public DataNode playerData { get { return m_ClientPlayer.dataNode; } set { m_ClientPlayer.dataNode = value; } }

        /// <summary>
        /// Immediately sync the player data. Call if it changing the player's DataNode manually.
        /// </summary>

        public void SyncPlayerData()
        {
#if !MODDING
            var writer = BeginSend(Packet.RequestSetPlayerData);
            writer.Write(m_ClientPlayer.id);
            writer.Write("");
            writer.WriteObject(m_ClientPlayer.dataNode);
            EndSend();
#endif
        }

        /// <summary>
        /// Set the specified value on the player.
        /// </summary>

        public void SetPlayerData(string path, object val)
        {
            var node = m_ClientPlayer.Set(path, val);
#if !MODDING
            if (isConnectedToGameServer)
            {
                var writer = BeginSend(Packet.RequestSetPlayerData);
                writer.Write(m_ClientPlayer.id);
                writer.Write(path);
                writer.WriteObject(val);
                EndSend();
            }
#endif
            if (onSetPlayerData != null)
                onSetPlayerData(m_ClientPlayer, path, node);
        }

        /// <summary>
        /// Whether the client is currently trying to join the specified channel.
        /// </summary>

        public bool IsJoiningChannel(int id) { return mJoining.Contains(id); }

        /// <summary>
        /// Whether the player is currently in the specified channel.
        /// </summary>

        public bool IsInChannel(int channelID)
        {
            if (isConnectedToGameServer)
            {
                if (mJoining.Contains(channelID)) return false;

                for (int i = 0; i < m_ClientPlayer.channels.size; ++i)
                {
                    var ch = m_ClientPlayer.channels.buffer[i];
                    if (ch.id == channelID) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Get the player hosting the specified channel. Only works for the channels the player is in.
        /// </summary>

        public Player GetHost(int channelID)
        {
            if (isConnectedToGameServer)
            {
                for (int i = 0; i < m_ClientPlayer.channels.size; ++i)
                {
                    var ch = m_ClientPlayer.channels.buffer[i];
                    if (ch.id == channelID) return ch.host;
                }
            }
            return null;
        }

        /// <summary>
        /// Retrieve a player by their ID.
        /// </summary>

        public Player GetPlayer(int id, bool createIfMissing = false)
        {
            if (id == m_ClientPlayer.id) return m_ClientPlayer;

            if (isConnectedToGameServer)
            {
                Player player = null;
                mDictionary.TryGetValue(id, out player);

                if (player == null && createIfMissing)
                {
                    player = new Player();
                    player.id = id;
                    mDictionary[id] = player;
                }
                return player;
            }
            return null;
        }

        /// <summary>
        /// Retrieve a player by their name.
        /// </summary>

        public Player GetPlayer(string name)
        {
            foreach (var p in mDictionary)
            {
                if (p.Value.name == name)
                    return p.Value;
            }
            return null;
        }

        /// <summary>
        /// Return a channel with the specified ID.
        /// </summary>

        public Channel GetChannel(int channelID, bool createIfMissing = false)
        {
            for (int i = 0; i < m_ClientPlayer.channels.size; ++i)
            {
                var ch = m_ClientPlayer.channels.buffer[i];
                if (ch.id == channelID) return ch;
            }

            if (createIfMissing)
            {
                var ch = new Channel();
                ch.id = channelID;
                m_ClientPlayer.channels.Add(ch);
                return ch;
            }
            return null;
        }

        /// <summary>
        /// Try to establish a connection with the hub url.
        /// </summary>


        public void ConnectToHub(string hubUrl, string playerName)
        {
#if !MODDING
            //HTTPManager.Logger.Level = BestHTTP.Logger.Loglevels.All;

            if (m_ClientPlayer != null)
            {
                DisconnectFromHubNow();
            }
            m_ClientPlayer = new ClientPlayer(playerName, hubUrl);
            m_ClientPlayer.ConnectedToHub += OnConnectedToHub;
            m_ClientPlayer.DisconnectedFromHub += OnDisconnectedFromHub;
            m_ClientPlayer.ReceiveResponseNewGameServerPacket += OnReceiveResponseNewGameServerPacket;
            m_ClientPlayer.ConnectToHub();
#endif
        }

        private void OnConnectedToHub(ClientPlayer player)
        {
            onConnectedToHub?.Invoke(player);
        }

        private void OnDisconnectedFromHub(ClientPlayer player)
        {
            m_ClientPlayer.ConnectedToHub -= OnConnectedToHub;
            m_ClientPlayer.DisconnectedFromHub -= OnDisconnectedFromHub;
            m_ClientPlayer.ReceiveResponseNewGameServerPacket -= OnReceiveResponseNewGameServerPacket;
            m_ClientPlayer = null;
            onDisconnectedFromHub?.Invoke(player);
        }

        private void OnReceiveResponseNewGameServerPacket(CommandPacket commandPacket)
        {
            var responseNewGameServerPacket = commandPacket as ResponseNewGameServerPacket;
            onReceiveResponseNewGameServerPacket?.Invoke(responseNewGameServerPacket);
        }

        /// <summary>
        /// Start disconnecting from the hub.
        /// </summary>

        public void StartDisconnectingFromHub()
        {
#if !MODDING
            if (m_ClientPlayer.isConnectedToGameServer)
            {
                m_ClientPlayer.DisconnectFromGameServerNow();
            }
            m_ClientPlayer.StartDisconnectingFromHub();
#endif
        }

        /// <summary>
        /// Disconnect from the hub now.
        /// </summary>
#if !MODDING
        public void DisconnectFromHubNow()
        {
            if (m_ClientPlayer.isConnectedToGameServer)
            {
                m_ClientPlayer.DisconnectFromGameServerNow();
            }
            m_ClientPlayer.DisconnectFromHubNow();
        }
#endif

        /// <summary>
        /// Try to establish a connection with the game server id.
        /// </summary>

        public void ConnectToGameServer(string gameServerId)
        {
#if !MODDING
            if (isConnectedToHub)
            {
                if (isConnectedToGameServer)
                {
                    DisconnectFromGameServerNow();
                }
                m_ClientPlayer.ConnectedToGameServer += OnConnectedToGameServer;
                m_ClientPlayer.DisconnectedFromGameServer += OnDisconnectedFromGameServer;
                m_ClientPlayer.ReceiveResponseJoinChannelPacket += OnReceiveResponseJoinChannelPacket;
                m_ClientPlayer.ReceiveJoiningChannelPacket += OnReceiveJoiningChannelPacket;
                m_ClientPlayer.ReceiveUpdateChannelPacket += OnReceiveUpdateChannelPacket;
                m_ClientPlayer.ReceiveResponseLeaveChannelPacket += OnReceiveResponseLeaveChannelPacket;
                m_ClientPlayer.ReceiveLoadLevelPacket += OnReceiveLoadLevelPacket;
                m_ClientPlayer.ReceiveCreateObjectPacket += OnReceiveCreateObjectPacket;
                m_ClientPlayer.ReceiveDestroyObjectsPacket += OnReceiveDestroyObjectsPacket;
                m_ClientPlayer.ReceiveForwardPacket += OnReceiveForwardPacket;
                m_ClientPlayer.ReceivePlayerJoinedChannelPacket += OnReceivePlayerJoinedChannelPacket;
                m_ClientPlayer.ReceivePlayerLeftChannelPacket += OnReceivePlayerLeftChannelPacket;
                m_ClientPlayer.ReceiveResponseDestroyObjectsPacket += OnReceiveResponseDestroyObjectsPacket;
                m_ClientPlayer.ReceiveTransferredObjectPacket += OnReceiveTransferredObjectPacket;
                m_ClientPlayer.ReceiveResponseSetNamePacket += OnReceiveResponseSetNamePacket;
                m_ClientPlayer.ReceivePlayerChangedNamePacket += OnReceivePlayerChangedNamePacket;
                m_ClientPlayer.JoinGameServer(gameServerId);
            }
#endif
        }

        private void OnConnectedToGameServer(ClientPlayer player)
        {
            onConnectedToGameServer?.Invoke(player);
        }

        private void OnDisconnectedFromGameServer(ClientPlayer player)
        {
            if (onLeaveChannel != null)
            {
                while (m_ClientPlayer.channels.size > 0)
                {
                    int index = m_ClientPlayer.channels.size - 1;
                    var ch = m_ClientPlayer.channels.buffer[index];
                    ch.isLeaving = true;
                    m_ClientPlayer.channels.RemoveAt(index);
                    onLeaveChannel(ch.id);
                }
            }

            m_ClientPlayer.channels.Clear();
            mGetChannelsCallbacks.Clear();
            mDictionary.Clear();
            mLoadFiles.Clear();
            mGetFiles.Clear();
            mJoining.Clear();
            m_ClientPlayer.ConnectedToGameServer -= OnConnectedToGameServer;
            m_ClientPlayer.DisconnectedFromGameServer -= OnDisconnectedFromGameServer;
            m_ClientPlayer.ReceiveResponseJoinChannelPacket -= OnReceiveResponseJoinChannelPacket;
            m_ClientPlayer.ReceiveJoiningChannelPacket -= OnReceiveJoiningChannelPacket;
            m_ClientPlayer.ReceiveUpdateChannelPacket -= OnReceiveUpdateChannelPacket;
            m_ClientPlayer.ReceiveResponseLeaveChannelPacket -= OnReceiveResponseLeaveChannelPacket;
            m_ClientPlayer.ReceiveLoadLevelPacket -= OnReceiveLoadLevelPacket;
            m_ClientPlayer.ReceiveCreateObjectPacket -= OnReceiveCreateObjectPacket;
            m_ClientPlayer.ReceiveDestroyObjectsPacket -= OnReceiveDestroyObjectsPacket;
            m_ClientPlayer.ReceiveForwardPacket -= OnReceiveForwardPacket;
            m_ClientPlayer.ReceivePlayerJoinedChannelPacket -= OnReceivePlayerJoinedChannelPacket;
            m_ClientPlayer.ReceivePlayerLeftChannelPacket -= OnReceivePlayerLeftChannelPacket;
            m_ClientPlayer.ReceiveResponseDestroyObjectsPacket -= OnReceiveResponseDestroyObjectsPacket;
            m_ClientPlayer.ReceiveTransferredObjectPacket -= OnReceiveTransferredObjectPacket;
            m_ClientPlayer.ReceiveResponseSetNamePacket -= OnReceiveResponseSetNamePacket;
            m_ClientPlayer.ReceivePlayerChangedNamePacket -= OnReceivePlayerChangedNamePacket;

            mIsAdmin = false;
            mOnExport.Clear();
            mOnImport.Clear();
            mMyTime = 0;

            mConfig = new DataNode("Version", Player.version);
            onDisconnectedFromGameServer?.Invoke(player);
        }

        /// <summary>
        /// Start disconnecting from the game server.
        /// </summary>

        public void StartDisconnectingFromGameServer()
        {
#if !MODDING
            m_ClientPlayer.StartDisconnectingFromGameServer();
#endif
        }

        /// <summary>
        /// Disconnect from the game server now.
        /// </summary>
#if !MODDING
        public void DisconnectFromGameServerNow()
        {
            m_ClientPlayer.DisconnectFromGameServerNow();
        }
#endif

        /// <summary>
        /// Join the specified channel.
        /// </summary>
        /// <param name="channelID">ID of the channel. Every player joining this channel will see one another.</param>
        /// <param name="levelName">Level that will be loaded first.</param>
        /// <param name="persistent">Whether the channel will remain active even when the last player leaves.</param>
        /// <param name="playerLimit">Maximum number of players that can be in this channel at once.</param>
        /// <param name="password">Password for the channel. First player sets the password.</param>

        public void JoinChannel(int channelID, string levelName, bool persistent, int playerLimit, string password)
        {
#if !MODDING
            if (isConnectedToGameServer && !IsInChannel(channelID) && !mJoining.Contains(channelID))
            {
                if (playerLimit > 65535) playerLimit = 65535;
                else if (playerLimit < 0) playerLimit = 0;
                player.SendPacket(new RequestJoinChannelPacket(channelID, password, levelName, persistent, (ushort)playerLimit));

                // Prevent all further packets from going out until the join channel response arrives.
                // This prevents the situation where packets are sent out between LoadLevel / JoinChannel
                // requests and the arrival of the OnJoinChannel/OnLoadLevel responses, which cause RFCs
                // from the previous scene to be executed in the new one.
                mJoining.Add(channelID);
            }
#endif
        }

        /// <summary>
        /// Close the channel the player is in. New players will be prevented from joining.
        /// Once a channel has been closed, it cannot be re-opened.
        /// </summary>

        public bool CloseChannel(int channelID)
        {
#if !MODDING
            if (isConnectedToGameServer && IsInChannel(channelID))
            {
                BeginSend(Packet.RequestCloseChannel).Write(channelID);
                EndSend();
                return true;
            }
#endif
            return false;
        }

        /// <summary>
        /// Leave the current channel.
        /// </summary>

        public bool LeaveChannel(int channelId)
        {
#if !MODDING
            if (isConnectedToGameServer)
            {
                for (int i = 0; i < m_ClientPlayer.channels.size; ++i)
                {
                    var ch = m_ClientPlayer.channels.buffer[i];

                    if (ch.id == channelId)
                    {
                        if (ch.isLeaving) return false;
                        ch.isLeaving = true;
                        m_ClientPlayer.SendPacket(new RequestLeaveChannelPacket(ch.id, player.id));
                        return true;
                    }
                }
            }
#endif
            return false;
        }

        /// <summary>
        /// Leave all channels.
        /// </summary>

        public void LeaveAllChannels()
        {
#if !MODDING
            if (isConnectedToGameServer)
            {
                mJoining.Clear();

                for (int i = m_ClientPlayer.channels.size; i > 0;)
                {
                    var ch = m_ClientPlayer.channels.buffer[--i];

                    if (!ch.isLeaving)
                    {
                        ch.isLeaving = true;
                        m_ClientPlayer.SendPacket(new RequestLeaveChannelPacket(ch.id, player.id));
                    }
                }
            }
#endif
        }

        /// <summary>
        /// Delete the specified channel.
        /// </summary>

        public void DeleteChannel(int id, bool disconnect)
        {
#if !MODDING
            if (isConnectedToGameServer)
            {
                var writer = BeginSend(Packet.RequestDeleteChannel);
                writer.Write(id);
                writer.Write(disconnect);
                EndSend();
            }
#endif
        }

        /// <summary>
        /// Change the maximum number of players that can join the channel the player is currently in.
        /// </summary>

        public void SetPlayerLimit(int channelID, int max)
        {
#if !MODDING
            if (isConnectedToGameServer && IsInChannel(channelID))
            {
                var writer = BeginSend(Packet.RequestSetPlayerLimit);
                writer.Write(channelID);
                writer.Write((ushort)max);
                EndSend();
            }
#endif
        }

        /// <summary>
        /// Switch the current level.
        /// </summary>

        public bool LoadLevel(int channelID, string levelName)
        {
#if !MODDING
            if (isConnectedToGameServer && IsInChannel(channelID))
            {
                var writer = BeginSend(Packet.RequestLoadLevel);
                writer.Write(channelID);
                writer.Write(levelName);
                EndSend();
                return true;
            }
#endif
            return false;
        }

        /// <summary>
        /// Change the hosting player.
        /// </summary>

        public void SetHost(int channelID, Player player)
        {
#if !MODDING
            if (isConnectedToGameServer && GetHost(channelID) == m_ClientPlayer)
            {
                var writer = BeginSend(Packet.RequestSetHost);
                writer.Write(channelID);
                writer.Write(player.id);
                EndSend();
            }
#endif
        }

        /// <summary>
        /// Set the timeout for the player. By default it's 10 seconds. If you know you are about to load a large level,
        /// and it's going to take, say 60 seconds, set this timeout to 120 seconds just to be safe. When the level
        /// finishes loading, change this back to 10 seconds so that dropped connections gets detected correctly.
        /// </summary>

        public void SetTimeout(int seconds)
        {
#if !MODDING
            if (isConnectedToGameServer)
            {
                BeginSend(Packet.RequestSetTimeout).Write(seconds);
                EndSend();
            }
#endif
        }

        /// <summary>
        /// Retrieve a list of files from the server.
        /// </summary>

        public void GetFiles(string path, OnGetFiles callback)
        {
#if !MODDING
            mGetFiles[path] = callback;
            var writer = BeginSend(Packet.RequestGetFileList);
            writer.Write(path);
            EndSend();
#endif
        }

        /// <summary>
        /// Load the specified file from the server.
        /// </summary>

        public void LoadFile(string filename, OnLoadFile callback)
        {
#if !MODDING
            mLoadFiles[filename] = callback;
            var writer = BeginSend(Packet.RequestLoadFile);
            writer.Write(filename);
            EndSend();
#endif
        }

        /// <summary>
        /// Save the specified file on the server.
        /// </summary>

        public void SaveFile(string filename, byte[] data)
        {
#if !MODDING
            if (data != null)
            {
                var writer = BeginSend(Packet.RequestSaveFile);
                writer.Write(filename);
                writer.Write(data.Length);
                writer.Write(data);
            }
            else
            {
                var writer = BeginSend(Packet.RequestDeleteFile);
                writer.Write(filename);
            }
            EndSend();
#endif
        }

        /// <summary>
        /// Delete the specified file on the server.
        /// </summary>

        public void DeleteFile(string filename)
        {
#if !MODDING
            var writer = BeginSend(Packet.RequestDeleteFile);
            writer.Write(filename);
            EndSend();
#endif
        }

        /// <summary>
        /// Send out a chat message.
        /// </summary>

        public void SendChat(string text, Player target = null)
        {
#if !MODDING
            var writer = BeginSend(Packet.RequestSendChat);
            writer.Write(target != null ? target.id : 0);
            writer.Write(text);
            EndSend();
#endif
        }

        public void SendPacket(CommandPacket commandPacket)
        {
            m_ClientPlayer.SendPacket(commandPacket);
        }

        private void OnReceiveResponseJoinChannelPacket(CommandPacket commandPacket)
        {
            var responseJoinChannelPacket = commandPacket as ResponseJoinChannelPacket;
            // mJoining can contain -2 and -1 when joining random channels
            if (!mJoining.Remove(responseJoinChannelPacket.ChannelId))
            {
                for (int i = 0; i < mJoining.size; ++i)
                {
                    int id = mJoining.buffer[i];

                    if (id < 0)
                    {
                        mJoining.RemoveAt(i);
                        break;
                    }
                }
            }
#if DEBUG
            if (!responseJoinChannelPacket.Success)
            {
                UnityEngine.Debug.LogError("ResponseJoinChannel: " + responseJoinChannelPacket.Success + ", " + responseJoinChannelPacket.Message);
            }
#endif
            onJoinChannel?.Invoke(responseJoinChannelPacket.ChannelId, responseJoinChannelPacket.Success, responseJoinChannelPacket.Message);
        }

        private void OnReceiveJoiningChannelPacket(CommandPacket commandPacket)
        {
            var joiningChannelPacket = commandPacket as JoiningChannelPacket;
            var ch = GetChannel(joiningChannelPacket.ChannelId, true);
            foreach (var playerInChannel in joiningChannelPacket.PlayersInChannel)
            {
                var player = GetPlayer(playerInChannel.PlayerId, true);
                player.name = playerInChannel.PlayerName;
                player.dataNode = playerInChannel.DataNode;
                ch.players.Add(player);
            }
        }

        private void OnReceiveLoadLevelPacket(CommandPacket commandPacket)
        {
            var loadLevelPacket = commandPacket as LoadLevelPacket;
            onLoadLevel?.Invoke(loadLevelPacket.ChannelId, loadLevelPacket.LevelName);
        }

        private void OnReceivePlayerJoinedChannelPacket(CommandPacket commandPacket)
        {
            var playerJoinedChannelPacket = commandPacket as PlayerJoinedChannelPacket;
            var ch = GetChannel(playerJoinedChannelPacket.ChannelId);

            if (ch != null)
            {
                var player = GetPlayer(playerJoinedChannelPacket.PlayerId, true);
                player.name = playerJoinedChannelPacket.PlayerName;
                player.dataNode = playerJoinedChannelPacket.PlayerDataNode;
                ch.players.Add(player);
                onPlayerJoin?.Invoke(playerJoinedChannelPacket.ChannelId, player);
            }
        }

        private void OnReceivePlayerLeftChannelPacket(CommandPacket commandPacket)
        {
            var playerLeftChannelPacket = commandPacket as PlayerLeftChannelPacket;
            var ch = GetChannel(playerLeftChannelPacket.ChannelId);

            if (ch != null)
            {
                var player = GetPlayer(playerLeftChannelPacket.PlayerId, false);
                if (player != null)
                {
                    player.name = playerLeftChannelPacket.PlayerName;
                    player.dataNode = playerLeftChannelPacket.PlayerDataNode;
                    if (ch.players.Contains(player))
                    {
                        ch.players.Remove(player);
                        onPlayerLeave?.Invoke(playerLeftChannelPacket.ChannelId, player);
                    }
                }
            }
        }

        private void OnReceiveCreateObjectPacket(CommandPacket commandPacket)
        {
            var createObjectPacket = commandPacket as CreateObjectPacket;
            onCreate?.Invoke(createObjectPacket.ChannelId, createObjectPacket.PlayerId, createObjectPacket.ObjectId, createObjectPacket.ObjectData);
        }

        private void OnReceiveDestroyObjectsPacket(CommandPacket commandPacket)
        {
            var destroyObjectsPacket = commandPacket as DestroyObjectsPacket;
            for (int i = 0; i < destroyObjectsPacket.ObjectsToDestroy.Length; ++i)
            {
                onDestroy?.Invoke(destroyObjectsPacket.ChannelId, destroyObjectsPacket.ObjectsToDestroy[i]);
            }
        }

        private void OnReceiveResponseDestroyObjectsPacket(CommandPacket commandPacket)
        {
            var responseDestroyObjectsPacket = commandPacket as ResponseDestroyObjectsPacket;
            foreach (var objectUid in responseDestroyObjectsPacket.DestroyedObjects)
            {
                onDestroy?.Invoke(responseDestroyObjectsPacket.ChannelId, objectUid);
            }
        }

        private void OnReceiveTransferredObjectPacket(CommandPacket commandPacket)
        {
            var transferredObjectPacket = commandPacket as TransferredObjectPacket;
            onTransfer?.Invoke(transferredObjectPacket.OldChannelId, transferredObjectPacket.NewChannelId, transferredObjectPacket.OldObjectId, transferredObjectPacket.NewObjectId);
        }

        private void OnReceiveResponseSetNamePacket(CommandPacket commandPacket)
        {
            var responseSetNamePacket = commandPacket as ResponseSetNamePacket;
            if (responseSetNamePacket.Success)
            {
                var oldName = m_ClientPlayer.name;
                m_ClientPlayer.name = responseSetNamePacket.Name;
                if (m_ClientPlayer.name != oldName)
                {
                    onRenamePlayer?.Invoke(m_ClientPlayer, oldName);
                }
            }
        }

        private void OnReceivePlayerChangedNamePacket(CommandPacket commandPacket)
        {
            var playerChangedNamePacket = commandPacket as PlayerChangedNamePacket;
            var player = TNManager.GetPlayer(playerChangedNamePacket.PlayerId);
            if (player != null)
            {
                var oldName = player.name;
                player.name = playerChangedNamePacket.Name;
                if (player.name != oldName)
                {
                    onRenamePlayer?.Invoke(player, oldName);
                }
            }
        }

        private void OnReceiveForwardPacket(CommandPacket commandPacket)
        {
            var forwardPacket = commandPacket as ForwardPacket;
            onReceiveForwardPacket?.Invoke(forwardPacket.ChannelId, forwardPacket.Uid, forwardPacket.FunctionName, forwardPacket.Data);
        }

        private void OnReceiveUpdateChannelPacket(CommandPacket commandPacket)
        {
            var updateChannelPacket = commandPacket as UpdateChannelPacket;
            var channel = GetChannel(updateChannelPacket.ChannelId);
            if (channel != null)
            {
                channel.playerLimit = updateChannelPacket.PlayerLimit;
                channel.isPersistent = (updateChannelPacket.Flags & 1) != 0;
                channel.isClosed = (updateChannelPacket.Flags & 2) != 0;
                channel.isLocked = (updateChannelPacket.Flags & 4) != 0;
                onUpdateChannel?.Invoke(channel);
            }
        }

        private void OnReceiveResponseLeaveChannelPacket(CommandPacket commandPacket)
        {
            var responseLeaveChannelPacket = commandPacket as ResponseLeaveChannelPacket;
            for (int i = 0; i < m_ClientPlayer.channels.size; ++i)
            {
                var ch = m_ClientPlayer.channels.buffer[i];

                if (ch.id == responseLeaveChannelPacket.ChannelId)
                {
                    ch.isLeaving = true;
                    m_ClientPlayer.channels.RemoveAt(i);
                    break;
                }
            }

            RebuildPlayerDictionary();
            onLeaveChannel?.Invoke(responseLeaveChannelPacket.ChannelId);
        }

        /// <summary>
        /// Immediately reset the packet count.
        /// </summary>

        public void ResetPacketCount()
        {
#if !MODDING
            mNextReset = mMyTime + 1000;

#if UNITY_EDITOR && COUNT_PACKETS
			var temp = TNObject.lastSentDictionary;
			temp.Clear();
			TNObject.lastSentDictionary = TNObject.sentDictionary;
			TNObject.sentDictionary = temp;
#endif
#endif
        }

#if PROFILE_PACKETS
		[System.NonSerialized] static System.Collections.Generic.Dictionary<int, string> mPacketNames = new Dictionary<int, string>();
#endif

#if !MODDING
        /// <summary>
        /// Process a single incoming packet. Returns whether we should keep processing packets or not.
        /// </summary>

        bool ProcessPacket(Buffer buffer, string endPoint)
        {
            var packetSourceID = 0;
            var reader = buffer.BeginReading();
            if (buffer.size == 0) return true;
            var size = reader.ReadInt32();
            int packetID = reader.ReadByte();
            var response = (Packet)packetID;

#if DEBUG_PACKETS && !STANDALONE
			if (response != Packet.ResponsePing)
				UnityEngine.Debug.Log("Client: " + response + " (" + buffer.size + " bytes) " + ((ip == null) ? "(TCP)" : "(UDP)") + " " + UnityEngine.Time.time);
#endif

#if PROFILE_PACKETS
			string packetName;

			if (!mPacketNames.TryGetValue(packetID, out packetName))
			{
				packetName = response.ToString();
				mPacketNames.Add(packetID, packetName);
			}

			UnityEngine.Profiling.Profiler.BeginSample(packetName);
#endif
            OnPacket callback;

            if (packetHandlers.TryGetValue((byte)response, out callback) && callback != null)
            {
                callback(response, reader, endPoint);
#if PROFILE_PACKETS
				UnityEngine.Profiling.Profiler.EndSample();
#endif
                return true;
            }

            switch (response)
            {
                case Packet.ResponseSendChat:
                    {
                        var player = GetPlayer(reader.ReadInt32());
                        var msg = reader.ReadString();
                        var prv = reader.ReadBoolean();
                        if (onChat != null) onChat(player, msg, prv);
                        break;
                    }
                case Packet.ResponseSetPlayerData:
                    {
                        int pid = reader.ReadInt32();
                        var target = GetPlayer(pid);

                        if (target != null)
                        {
                            var path = reader.ReadString();
                            var node = target.Set(path, reader.ReadObject());
                            if (onSetPlayerData != null) onSetPlayerData(target, path, node);
                        }
                        else UnityEngine.Debug.LogError("Not found: " + pid);
                        break;
                    }
                case Packet.ResponsePing:
                    {
                        int ping = (int)(mMyTime - mPingTime);

                        if (endPoint != null)
                        {
                            if (onPing != null && endPoint != null) onPing((string)endPoint, ping);
                        }
                        else
                        {
                            mPing = ping;
                        }

                        // Trivial time speed hack check
                        /*var expectedTime = reader.ReadInt64();
                        reader.ReadUInt16();
                        var diff = (serverTime - expectedTime) - ping;

                        if ((diff < 0 ? -diff : diff) > 10000)
                        {
    #if W2
                            var s = "Server time is too different: " + diff.ToString("N0") + " milliseconds apart, ping " + ping;
                            GameChat.NotifyAdmins(s);
                            if (onError != null) onError(s);
                            TNManager.Disconnect(1f);
    #else
                            if (onError != null) onError("Server time is too different: " + diff.ToString("N0") + " milliseconds apart, ping " + ping);
                            Disconnect();
    #endif
                            break;
                        }*/
                        break;
                    }
                case Packet.ResponseSetUDP:
                    {
                        //#if !UNITY_WEBPLAYER
                        //                        // The server has a new port for UDP traffic
                        //                        ushort port = reader.ReadUInt16();

                        //                        if (port != 0 && networkProtocol.tcpEndPoint != null)
                        //                        {
                        //                            var ipa = new IPAddress(networkProtocol.tcpEndPoint.Address.GetAddressBytes());
                        //                            mServerUdpEndPoint = new IPEndPoint(ipa, port);

                        //                            // Send the first UDP packet to the server
                        //                            if (mUdp.isActive)
                        //                            {
                        //                                mBuffer = Buffer.Create();
                        //                                mBuffer.BeginPacket(Packet.RequestActivateUDP).Write(playerID);
                        //                                mBuffer.EndPacket();
                        //                                mUdp.Send(mBuffer, mServerUdpEndPoint);
                        //                                mBuffer.Recycle();
                        //                                mBuffer = null;
                        //                            }
                        //                        }
                        //                        else mServerUdpEndPoint = null;
                        //#endif
                        break;
                    }

                //case Packet.ResponsePlayerLeft:
                //    {
                //        int channelID = reader.ReadInt32();
                //        int playerID = reader.ReadInt32();

                //        var ch = GetChannel(channelID);

                //        if (ch != null)
                //        {
                //            Player p = ch.GetPlayer(playerID);
                //            ch.players.Remove(p);
                //            RebuildPlayerDictionary();
                //            if (onPlayerLeave != null) onPlayerLeave(channelID, p);
                //        }
                //        break;
                //    }
                case Packet.ResponseSetHost:
                    {
                        int channelID = reader.ReadInt32();
                        int hostID = reader.ReadInt32();

                        for (int i = 0; i < m_ClientPlayer.channels.size; ++i)
                        {
                            var ch = m_ClientPlayer.channels.buffer[i];

                            if (ch.id == channelID)
                            {
                                ch.host = GetPlayer(hostID);
                                if (onHostChanged != null) onHostChanged(ch);
                                break;
                            }
                        }
                        break;
                    }
                case Packet.ResponseSetChannelData:
                    {
                        int channelID = reader.ReadInt32();
                        Channel ch = GetChannel(channelID);

                        if (ch != null)
                        {
                            string path = reader.ReadString();
                            DataNode node = ch.Set(path, reader.ReadObject());
                            if (onSetChannelData != null) onSetChannelData(ch, path, node);
                        }
                        break;
                    }
                case Packet.ResponseExport:
                    {
                        var requestID = reader.ReadInt32();
                        var byteCount = reader.ReadInt32();
                        var data = (byteCount > 0) ? reader.ReadBytes(byteCount) : null;

                        ExportCallback cb;

                        if (mOnExport.TryGetValue(requestID, out cb))
                        {
                            mOnExport.Remove(requestID);
                            if (cb.callback0 != null) cb.callback0(data);
                            else if (cb.callback1 != null) cb.callback1(data != null ? DecodeExportedObjects(cb.objects, data) : null);
                        }
                        break;
                    }
                case Packet.ResponseImport:
                    {
                        var requestID = reader.ReadInt32();
                        reader.ReadInt32(); // The request already knows what channel it was made in
                        var size2 = reader.ReadInt32();
                        var result = new uint[size2];
                        for (int i = 0; i < size2; ++i) result[i] = reader.ReadUInt32();

                        Action<uint[]> cb;

                        if (mOnImport.TryGetValue(requestID, out cb))
                        {
                            mOnImport.Remove(requestID);
                            if (cb != null) cb(result);
                        }
                        break;
                    }
                case Packet.Error:
                    {
                        string err = reader.ReadString();
                        if (onError != null) onError(err);
                        //if (m_ClientPlayer.stage != ServerPlayer.Stage.Connected && onConnect != null) onConnect(false, err);
                        break;
                    }
                case Packet.Disconnect:
                    {
                        StartDisconnectingFromHub();
                        break;
                    }
                case Packet.ResponseGetFileList:
                    {
                        string filename = reader.ReadString();
                        int size3 = reader.ReadInt32();
                        string[] files = null;

                        if (size3 > 0)
                        {
                            files = new string[size3];
                            for (int i = 0; i < size3; ++i)
                                files[i] = reader.ReadString();
                        }

                        OnGetFiles cb = null;
                        if (mGetFiles.TryGetValue(filename, out cb))
                            mGetFiles.Remove(filename);

                        if (cb != null)
                        {
                            try
                            {
                                cb(filename, files);
                            }
#if UNITY_EDITOR
                            catch (System.Exception ex)
                            {
                                Debug.LogError(ex.Message + ex.StackTrace);
                            }
#else
                            catch (System.Exception) { }
#endif
                        }
                        break;
                    }
                case Packet.ResponseLoadFile:
                    {
                        string filename = reader.ReadString();
                        int size4 = reader.ReadInt32();
                        byte[] data = reader.ReadBytes(size4);
                        OnLoadFile cb = null;

                        if (mLoadFiles.TryGetValue(filename, out cb))
                            mLoadFiles.Remove(filename);

                        if (cb != null)
                        {
                            try
                            {
                                cb(filename, data);
                            }
#if UNITY_EDITOR
                            catch (System.Exception ex)
                            {
                                Debug.LogError(ex.Message + ex.StackTrace);
                            }
#else
                            catch (System.Exception) { }
#endif
                        }
                        break;
                    }
                case Packet.ResponseVerifyAdmin:
                    {
                        int pid = reader.ReadInt32();
                        Player p = GetPlayer(pid);
                        if (p == player) mIsAdmin = true;
                        if (onSetAdmin != null) onSetAdmin(p);
                        break;
                    }
                case Packet.ResponseSetServerData:
                    {
                        if (!ValidateHash())
                        {
#if W2
						Game.MAC("Edited the server configuration in memory");
#else
                            StartDisconnectingFromHub();
#endif
                            break;
                        }

                        var path = reader.ReadString();
                        var obj = reader.ReadObject();

                        if (obj != null)
                        {
                            var node = mConfig.SetHierarchy(path, obj);
                            mDataHash = mConfig.CalculateHash();
                            if (onSetServerData != null) onSetServerData(path, node);
                        }
                        else
                        {
                            var node = mConfig.RemoveHierarchy(path);
                            mDataHash = mConfig.CalculateHash();
                            if (onSetServerData != null) onSetServerData(path, node);
                        }
                        break;
                    }
                case Packet.ResponseConnected:
                    {
                        if (onConnectedToHub != null) onConnectedToHub(null);
                        break;
                    }
                case Packet.ResponseChannelList:
                    {
                        if (mGetChannelsCallbacks.Count != 0)
                        {
                            var cb = mGetChannelsCallbacks.Dequeue();
                            var channels = new GList<Channel.Info>();
                            var count = reader.ReadInt32();

                            for (int i = 0; i < count; ++i)
                            {
                                var info = new Channel.Info();
                                info.id = reader.ReadInt32();
                                info.players = reader.ReadUInt16();
                                info.limit = reader.ReadUInt16();
                                info.hasPassword = reader.ReadBoolean();
                                info.isPersistent = reader.ReadBoolean();
                                info.level = reader.ReadString();
                                info.data = reader.ReadDataNode();
                                channels.Add(info);
                            }

                            if (cb != null) cb(channels);
                        }
                        break;
                    }
                case Packet.ResponseSetOwner:
                    {
                        var channelID = reader.ReadInt32();
                        var objID = reader.ReadUInt32();
                        var playerID = reader.ReadInt32();
                        onChangeOwner(channelID, objID, playerID != 0 ? GetPlayer(playerID) : null);
                        break;
                    }
            }
#if PROFILE_PACKETS
			UnityEngine.Profiling.Profiler.EndSample();
#endif
            return true;
        }
#endif // !MODDING

        /// <summary>
        /// Rebuild the player dictionary from the list of players in all of the channels we're currently in.
        /// </summary>

        void RebuildPlayerDictionary()
        {
            mDictionary.Clear();

            for (int i = 0; i < m_ClientPlayer.channels.size; ++i)
            {
                var ch = m_ClientPlayer.channels.buffer[i];

                for (int b = 0; b < ch.players.size; ++b)
                {
                    var p = ch.players.buffer[b];
                    if (!mDictionary.ContainsKey(p.id)) mDictionary[p.id] = p;
                }
            }
        }

        /// <summary>
        /// Retrieve the specified server option.
        /// </summary>

        public DataNode GetServerData(string key) { return (mConfig != null) ? mConfig.GetHierarchy(key) : null; }

        /// <summary>
        /// Retrieve the specified server option.
        /// </summary>

        public T GetServerData<T>(string key) { return (mConfig != null) ? mConfig.GetHierarchy<T>(key) : default(T); }

        /// <summary>
        /// Retrieve the specified server option.
        /// </summary>

        public T GetServerData<T>(string key, T def) { return (mConfig != null) ? mConfig.GetHierarchy<T>(key, def) : def; }

        /// <summary>
        /// Set the specified server option.
        /// </summary>

        public void SetServerData(DataNode node)
        {
#if !MODDING
            var writer = BeginSend(Packet.RequestSetServerData);
            writer.Write(node.name);
            writer.WriteObject(node);
            EndSend();
#endif
        }

        /// <summary>
        /// Set the specified server option.
        /// </summary>

        public void SetServerData(string key, object val)
        {
#if !MODDING
            if (val != null)
            {
                mConfig.SetHierarchy(key, val);
                mDataHash = mConfig.CalculateHash();
            }
            else
            {
                mConfig.RemoveHierarchy(key);
                mDataHash = mConfig.CalculateHash();
            }

            var writer = BeginSend(Packet.RequestSetServerData);
            writer.Write(key);
            writer.WriteObject(val);
            EndSend();
#endif
        }

        /// <summary>
        /// Set the specified server option.
        /// </summary>

        public void SetChannelData(int channelID, string path, object val)
        {
#if !MODDING
            var ch = GetChannel(channelID);

            if (ch != null && !string.IsNullOrEmpty(path))
            {
                if (!ch.isLocked || isAdmin)
                {
                    DataNode node = ch.dataNode;

                    if (node == null)
                    {
                        if (val == null) return;
                        node = new DataNode("Version", Player.version);
                    }

                    node.SetHierarchy(path, val);

                    var bw = BeginSend(Packet.RequestSetChannelData);
                    bw.Write(channelID);
                    bw.Write(path);
                    bw.WriteObject(val);
                    EndSend();
                }
#if UNITY_EDITOR
                else Debug.LogWarning("Trying to SetChannelData on a locked channel: " + channelID);
#endif
            }
#if UNITY_EDITOR
            else Debug.LogWarning("Calling SetChannelData with invalid parameters: " + channelID + " = " + (ch != null) + ", " + path);
#endif
#endif
        }

        public delegate void OnGetChannels(GList<Channel.Info> list);
        Queue<OnGetChannels> mGetChannelsCallbacks = new Queue<OnGetChannels>();

        /// <summary>
        /// Get a list of channels from the server.
        /// </summary>

        public void GetChannelList(OnGetChannels callback)
        {
#if !MODDING
            mGetChannelsCallbacks.Enqueue(callback);
            BeginSend(Packet.RequestChannelList);
            EndSend();
#endif
        }

#if !MODDING
        int mRequestID = 0;
        Dictionary<int, ExportCallback> mOnExport = new Dictionary<int, ExportCallback>();
        Dictionary<int, Action<uint[]>> mOnImport = new Dictionary<int, Action<uint[]>>();

        struct ExportCallback
        {
            public GList<TNObject> objects;
            public Action<byte[]> callback0;
            public Action<DataNode> callback1;
        }
#endif

        /// <summary>
        /// Export the specified objects from the server. The server will return the byte[] necessary to re-instantiate all of the specified objects and restore their state.
        /// </summary>

        public void ExportObjects(GList<TNObject> list, Action<byte[]> callback)
        {
#if !MODDING
            if (isConnectedToGameServer && list.size > 0)
            {
                var cb = new ExportCallback();
                cb.objects = list;
                cb.callback0 = callback;

                mOnExport.Add(++mRequestID, cb);

                var writer = BeginSend(Packet.RequestExport);
                writer.Write(mRequestID);
                writer.Write(list.size);

                foreach (var obj in list)
                {
                    writer.Write(obj.channelID);
                    writer.Write(obj.uid);
                }

                EndSend();
            }
#endif
        }

        /// <summary>
        /// Export the specified objects from the server. The server will return the DataNode necessary to re-instantiate all of the specified objects and restore their state.
        /// </summary>

        public void ExportObjects(GList<TNObject> list, Action<DataNode> callback)
        {
#if !MODDING
            if (isConnectedToGameServer && list.size > 0)
            {
                var cb = new ExportCallback();
                cb.objects = list;
                cb.callback1 = callback;

                mOnExport.Add(++mRequestID, cb);

                var writer = BeginSend(Packet.RequestExport);
                writer.Write(mRequestID);
                writer.Write(list.size);

                foreach (var obj in list)
                {
                    writer.Write(obj.channelID);
                    writer.Write(obj.uid);
                }

                EndSend();
            }
#endif
        }

        /// <summary>
        /// Import previously exported objects in the specified channel.
        /// </summary>

        public void ImportObjects(int channelID, byte[] data, Action<uint[]> callback = null)
        {
#if !MODDING
            if (isConnectedToGameServer && data != null && data.Length > 0)
            {
                ++mRequestID;
                if (callback != null) mOnImport.Add(mRequestID, callback);

                var writer = BeginSend(Packet.RequestImport);
                writer.Write(mRequestID);
                writer.Write(channelID);
                writer.Write(data);
                EndSend();
            }
#endif
        }

        /// <summary>
        /// Import previously exported objects in the specified channel.
        /// </summary>

        public void ImportObjects(int channelID, DataNode node, Action<uint[]> callback = null)
        {
#if !MODDING
            var data = EncodeExportedObjects(node);
            ImportObjects(channelID, data, callback);
            data.Recycle();
#endif
        }

        /// <summary>
        /// Import previously exported objects in the specified channel.
        /// </summary>

        public void ImportObjects(int channelID, Buffer buffer, Action<uint[]> callback = null)
        {
#if !MODDING
            if (isConnectedToGameServer && buffer != null && buffer.size > 0)
            {
                ++mRequestID;
                if (callback != null) mOnImport.Add(mRequestID, callback);

                var writer = BeginSend(Packet.RequestImport);
                writer.Write(mRequestID);
                writer.Write(channelID);
                writer.Write(buffer.buffer, buffer.position, buffer.size);
                EndSend();
            }
#endif
        }

#if !MODDING
        /// <summary>
        /// When a server exports objects, the result comes as a byte array, which is not very readable or modifiable.
        /// This function is used to convert the byte array into a structured DataNode format, which is much easier to edit.
        /// </summary>

        static DataNode DecodeExportedObjects(GList<TNObject> objects, byte[] bytes)
        {
            var node = new DataNode();
#if W2
			try
#endif
            {
                var buffer = Buffer.Create();
                buffer.BeginWriting(false).Write(bytes);
                var reader = buffer.BeginReading();

                // Number of objects
                var count = reader.ReadInt32();

                for (int i = 0; i < count; ++i)
                {
                    var obj = objects.buffer[i];
                    reader.ReadInt32(); // Size of the data, we don't need it since we're parsing everything
                    var rccID = reader.ReadByte();
                    var funcName = (rccID == 0) ? reader.ReadString() : null;
                    var prefab = reader.ReadString();
                    var args = reader.ReadArray();
                    var func = TNManager.GetRCC(rccID, funcName);

                    var child = (rccID != 0) ? node.AddChild("RCC", rccID) : node.AddChild("RCC", funcName);
                    child.AddChild("prefab", prefab);

                    if (func != null)
                    {
                        var funcPars = func.parameters;
                        var argLength = args.Length;

                        if (funcPars.Length == argLength + 1)
                        {
                            var pn = child.AddChild("Args");
                            for (int b = 0; b < argLength; ++b) pn.AddChild(funcPars[b + 1].Name, args[b]);
                        }
#if UNITY_EDITOR
                        else Debug.LogError("RCC " + rccID + " (" + funcName + ") has a different number of parameters than expected: " + funcPars.Length + " vs " + (args.Length + 1), obj);
#endif
                    }
#if UNITY_EDITOR
                    else Debug.LogError("Unable to find RCC " + rccID + " (" + funcName + ")", obj);
#endif
                    var rfcs = reader.ReadInt32();
                    if (rfcs > 0) child = child.AddChild("RFCs");

                    for (int r = 0; r < rfcs; ++r)
                    {
                        uint objID;
                        byte funcID;
                        TNObject.DecodeUID(reader.ReadUInt32(), out objID, out funcID);
                        funcName = (funcID == 0) ? reader.ReadString() : null;
                        reader.ReadInt32(); // Size of the data, we don't need it since we're parsing everything
                        var array = reader.ReadArray();
                        var funcRef = (funcID == 0) ? obj.FindFunction(funcName) : obj.FindFunction(funcID);

                        if (funcRef != null)
                        {
                            var pc = array.Length;

                            if (funcRef.parameters.Length == pc)
                            {
                                var rfcNode = (funcID == 0) ? child.AddChild("RFC", funcName) : child.AddChild("RFC", funcID);
                                for (int p = 0; p < pc; ++p) rfcNode.AddChild(funcRef.parameters[p].Name, array[p]);
                            }
#if UNITY_EDITOR
                            else Debug.LogError("RFC " + funcID + " (" + funcName + ") has a different number of parameters than expected: " + funcRef.parameters.Length + " vs " + pc, obj);
#endif
                        }
#if UNITY_EDITOR
                        else Debug.LogWarning("RFC " + funcID + " (" + funcName + ") can't be found", obj);
#endif
                    }
                }

                buffer.Recycle();
                return node;
            }
#if W2
			catch (Exception ex)
			{
				TNManager.Log("ERROR: " + ex.Message + "\n" + ex.StackTrace);
				TNManager.SaveFile("Debug/" + TNManager.playerName + "_" + (TNManager.serverUptime / 1000) + ".txt", bytes);
				return node;
			}
#endif
        }

        /// <summary>
        /// The opposite of DecodeExportedObjects, encoding the DataNode-stored data into a binary format that can be sent back to the server.
        /// </summary>

        static Buffer EncodeExportedObjects(DataNode node)
        {
            var buffer = Buffer.Create();
            var writer = buffer.BeginWriting();

            // Number of objects
            writer.Write(node.children.size);

            for (int i = 0; i < node.children.size; ++i)
            {
                var child = node.children.buffer[i];
                var sizePos = buffer.position;

                if (child.value is string)
                {
                    var s = (string)child.value;
                    if (string.IsNullOrEmpty(s)) continue;

                    writer.Write(0); // Size of the RCC's data -- set after writing it
                    writer.Write((byte)0);
                    writer.Write(s);
                }
                else
                {
                    writer.Write(0); // Size of the RCC's data -- set after writing it
                    writer.Write((byte)child.Get<int>());
                }

                writer.Write(child.GetChild<string>("prefab"));

                var args = child.GetChild("Args");
                var argCount = (args != null) ? args.children.size : 0;
                var array = new object[argCount];
                for (int b = 0; b < argCount; ++b) array[b] = args.children.buffer[b].value;
                writer.WriteArray(array);

                // Write down the size of the RCC
                var endPos = buffer.position;
                var size = endPos - sizePos;
                buffer.position = sizePos;
                writer.Write(size - 4);
                buffer.position = endPos;

                var rfcs = child.GetChild("RFCs");
                var rfcCount = (rfcs != null) ? rfcs.children.size : 0;
                writer.Write(rfcCount);

                if (rfcCount > 0)
                {
                    for (int b = 0; b < rfcs.children.size; ++b)
                    {
                        var rfc = rfcs.children.buffer[b];

                        if (rfc.value is string)
                        {
                            var s = (string)rfc.value;
                            if (string.IsNullOrEmpty(s)) continue;
                            writer.Write((uint)0);
                            writer.Write(s);
                        }
                        else writer.Write(TNObject.GetUID(0, (byte)rfc.Get<int>()));

                        array = new object[rfc.children.size];
                        for (int c = 0; c < rfc.children.size; ++c) array[c] = rfc.children.buffer[c].value;

                        var rfcPos = buffer.position;
                        writer.Write(0); // Size of the array -- set after writing the array
                        writer.WriteArray(array);

                        // Write down the size of the RFC
                        endPos = buffer.position;
                        size = endPos - rfcPos;
                        buffer.position = rfcPos;
                        writer.Write(size - 4);
                        buffer.position = endPos;
                    }
                }
            }

            buffer.EndWriting();
            return buffer;
        }
#endif
    }
}
