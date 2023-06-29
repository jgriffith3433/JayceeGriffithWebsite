//-------------------------------------------------
//                    GNetServer 3
// Copyright © 2012-2018 Tasharen Entertainment Inc
//-------------------------------------------------

//#define DEBUG_PACKETS

// Must also be defined in TNServerInstance.cs
#define SINGLE_THREADED

#pragma warning disable 0162

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Threading;
using System.Net.Http.Headers;

namespace GNetServer
{
    /// <summary>
    /// Game server logic. Handles new connections, RFCs, and pretty much everything else. Example usage:
    /// GameServer gs = new GameServer();
    /// gs.Start(5127);
    /// </summary>

    public class GameServerInfo
    {
        public GameServerInfo(string serverId, uint gameId)
        {
            ServerId = serverId;
            GameId = gameId;
        }
        public string Name;
        public int PlayerCount;
        public string ServerId;
        public uint GameId;
        public string UserId;
    }
    public class GameServer : FileServer
    {
        public GameServer()
        {
            GameServerInfo = new GameServerInfo(Guid.NewGuid().ToString().Substring(0, 4), GameId);
        }

        ~GameServer()
        {
            Stop();
        }

#if SINGLE_THREADED
        public const bool isMultiThreaded = false;
#else
        public const bool isMultiThreaded = true;
#endif

        public const string defaultAdminPath = "ServerConfig/admin.txt";

        /// <summary>
        /// Path to the admin file. Can generally be left untouched, unless you really want to change it.
        /// </summary>

        public string adminFilePath = defaultAdminPath;

        /// <summary>
        /// You will want to make this a unique value.
        /// </summary>
        public GameServerInfo GameServerInfo;

        static public ushort GameId = 1;

        public delegate void OnCustomPacket(ServerPlayer player, Buffer buffer, BinaryReader reader, byte request);
        public delegate void OnPlayerAction(Player p);
        public delegate void OnShutdown();

        /// <summary>
        /// Any packet not already handled by the server will go to this function for processing.
        /// </summary>

        public OnCustomPacket onCustomPacket;

        /// <summary>
        /// Notification triggered when a player connects and authenticates successfully.
        /// </summary>

        public Action<string, int, string> onPlayerConnect;

        public Action<string, int, string> onPlayerDisconnect;

        public Action<string> onShutdown;

        public Action<NetworkPlayer, CommandPacket> onSendToClient;

        /// <summary>
        /// Give your server a name.
        /// </summary>

        public string name = "Game Server";

        // List of players in a consecutive order for each looping.
        protected GList<ServerPlayer> mPlayerList = new GList<ServerPlayer>();

        // List of all the active channels.
        protected GList<Channel> mChannelList = new GList<Channel>();

        // Dictionary of active channels to make lookup faster
        protected Dictionary<int, Channel> mChannelDict = new Dictionary<int, Channel>();

        // List of admin keywords
        protected HashSet<string> mAdmin = new HashSet<string>();

        // Random number generator.
        protected System.Random mRandom = new System.Random();
        protected int mListenerPort = 0;
        protected long mTime = 0;
        //protected UdpProtocol mUdp = new UdpProtocol("Game Server");
        protected bool mAllowUdp = false;
        protected object mLock = 0;
        protected DataNode mServerData = null;
        protected string mFilename = "world.dat";
        protected long mNextSave = 0;
        protected bool mIsActive = true;
        protected bool mServerDataChanged = false;
        protected long mStartTime = 0;

        /// <summary>
        /// Put the server to sleep or wake it up.
        /// </summary>

        public void Sleep(bool val)
        {
            Channel.lowMemoryFootprint = val;

            lock (mLock)
            {
                foreach (var ch in mChannelList)
                {
                    if (val) ch.Sleep();
                    else ch.Wake();
                }
            }
        }

        /// <summary>
        /// Add a new entry to the list. Returns 'true' if a new entry was added.
        /// </summary>

        static bool AddUnique(GList<string> list, string s)
        {
            if (!string.IsNullOrEmpty(s) && !list.Contains(s))
            {
                list.Add(s);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Add a new entry to the list. Returns 'true' if a new entry was added.
        /// </summary>

        static bool AddUnique(HashSet<string> hash, string s)
        {
            if (!string.IsNullOrEmpty(s) && !hash.Contains(s))
            {
                hash.Add(s);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Whether the server is currently actively serving players.
        /// </summary>

        public bool isActive { get { return mIsActive; } }

        /// <summary>
        /// How many players are currently connected to the server.
        /// </summary>

        public int playerCount { get { return isActive ? mPlayerList.Count : 0; } }

        /// <summary>
        /// List of connected players.
        /// </summary>

        public GList<ServerPlayer> players { get { return isActive ? mPlayerList : null; } }


        /// <summary>
        /// Load the admin list from the external file.
        /// </summary>

        //public void LoadAdminList()
        //{
        //    Tools.Print("Admins: " + (Tools.LoadList(string.IsNullOrEmpty(rootDirectory) ? adminFilePath : Path.Combine(rootDirectory, adminFilePath), mAdmin) ? mAdmin.Count.ToString() : "file not found"));
        //}

        ///// <summary>
        ///// Save the admin list back to the external file.
        ///// </summary>

        //public void SaveAdminList() { Tools.SaveList(string.IsNullOrEmpty(rootDirectory) ? adminFilePath : Path.Combine(rootDirectory, adminFilePath), mAdmin); }

        /// <summary>
        /// Start listening to incoming connections on the specified port.
        /// </summary>

//        public bool Start(int tcpPort = 0, int udpPort = 0, int srcPort = -1)
//        {
//#if !MODDING
//            mStartTime = System.DateTime.UtcNow.Ticks / 10000;

//            Stop();

//#if FORCE_EN_US
//			Tools.SetCurrentCultureToEnUS();
//#endif
//            //LoadBanList();
//            //LoadAdminList();

//            var remove = new GList<string>();

//            // Banning by IPs is only good as a temporary measure
//            foreach (var ban in mBan)
//            {
//                //IPAddress ip;
//                //if (IPAddress.TryParse(ban, out ip)) remove.Add(ban);
//            }

//            foreach (var rem in remove) mBan.Remove(rem);

//#if STANDALONE
//			Tools.Print("Game server started on port " + tcpPort + " using protocol version " + Player.version);
//#endif
//            //if (udpPort > 0)
//            //{
//            //    // Twice just in case the first try falls on a taken port
//            //    if (NetworkPlayer.defaultListenerInterface.AddressFamily == AddressFamily.InterNetworkV6 &&
//            //        UdpProtocol.defaulGNetServerworkInterface.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
//            //    {
//            //        if (!mUdp.Start(udpPort, IPAddress.IPv6Any))
//            //        {
//            //            Tools.LogError("Unable to listen to UDP port " + udpPort, null);
//            //            Stop();
//            //            return false;
//            //        }
//            //    }
//            //    else if (!mUdp.Start(udpPort))
//            //    {
//            //        Tools.LogError("Unable to listen to UDP port " + udpPort, null);
//            //        Stop();
//            //        return false;
//            //    }
//            //}

//            mAllowUdp = (udpPort > 0);
//            mIsActive = true;
//#endif
//            return true;
//        }

        public void LeaveServer(string userConnectionId)
        {
            var player = GetPlayerByConnectionId(userConnectionId);
            if (player != null)
            {
                RemovePlayer(player);
            }
        }

        public void JoinServer(string userConnectionId, string name)
        {
            var player = GetPlayerByConnectionId(userConnectionId);
            if (player == null)
            {
                AddPlayer(userConnectionId, name);
            }
        }

        public void JoinChannel(string userConnectionId, int channelId, string password, string levelName, bool persistent, ushort playerLimit, bool additive)
        {
            var player = GetPlayerByConnectionId(userConnectionId);
            if (player != null)
            {
                SendJoinChannel(player, playerLimit, persistent, password, channelId, levelName, additive);
            }
        }

        public void LeaveChannel(string userConnectionId, int channelId)
        {
            var player = GetPlayerByConnectionId(userConnectionId);
            if (player != null)
            {
                SendLeaveChannel(player, player.GetChannel(channelId), true);
            }
        }

        public void SendForwardPacket(string userConnectionId, int channelId, uint uid, string functionName, ForwardType forwardType, byte[] data)
        {
            var player = GetPlayerByConnectionId(userConnectionId);
            var channel = GetChannel(channelId);
            switch (forwardType)
            {
                case ForwardType.All:
                case ForwardType.AllSaved:
                    SendPacket(new ForwardPacket(channelId, uid, functionName, forwardType, data), channelId, null);
                    break;
                case ForwardType.Others:
                case ForwardType.OthersSaved:
                    SendPacket(new ForwardPacket(channelId, uid, functionName, forwardType, data), channelId, player);
                    break;
                case ForwardType.Host:
                    break;
                case ForwardType.Broadcast:
                    break;
                case ForwardType.Admin:
                    break;
                default:
                    break;
            }
            switch (forwardType)
            {
                case ForwardType.AllSaved:
                case ForwardType.OthersSaved:
                    if (!channel.isLocked || player.isAdmin)
                    {
                        var othersSavedBuffer = Buffer.Create();
                        othersSavedBuffer.BeginWriting(false).Write(data, 0, data.Length);
                        othersSavedBuffer.EndWriting();
                        channel.AddRFC(uid, functionName, othersSavedBuffer, mTime);
                        othersSavedBuffer.Clear();
                    }
                    break;
            }
        }

        public void SendForwardToPlayerPacket(int channelId, uint uid, int fromPlayerId, int toPlayerId, string functionName, ForwardType forwardType, byte[] data)
        {
            var player = GetPlayerById(toPlayerId);
            player.SendPacket(new ForwardPacket(channelId, uid, functionName, forwardType, data));
        }

        public void DestroyObject(string userConnectionId, int channelId, uint objectId)
        {
            var ch = GetChannel(channelId);
            var player = GetPlayerByConnectionId(userConnectionId);
            if (ch != null && (!ch.isLocked || (player != null && player.isAdmin)) && ch.DestroyObject(objectId))
            {
                SendPacket(new DestroyObjectsPacket(channelId, new uint[1] { objectId }), ch, null);
            }
        }

        public void CreateObject(string userConnectionId, int channelId, int playerId, bool persistent, byte[] objsData)
        {
            var ch = GetChannel(channelId);
            var player = GetPlayerByConnectionId(userConnectionId);
            if (ch != null && (!ch.isClosed || (player != null && player.isAdmin)))
            {
                // Exploit: echoed packet of another player
                if (playerId != player.id)
                {
                    //TODO: LogError in context of server player
                    //player.LogError("Tried to echo a create packet (" + playerId + " vs " + player.id + ")", null);
                    RemovePlayer(player);
                    return;
                }

                var uniqueID = ch.GetUniqueID();
                var obj = new Channel.CreatedObject();
                obj.playerId = player.id;
                obj.objectId = uniqueID;
                obj.type = persistent ? (byte)1 : (byte)2;
                var b = Buffer.Create();
                b.BeginWriting().Write(objsData, 0, objsData.Length);
                b.EndWriting();
                obj.data = b;
                ch.AddCreatedObject(obj);
                SendPacket(new CreateObjectPacket(obj.playerId, ch.id, obj.objectId, obj.data.buffer), ch, null);
            }
        }

        public void SetName(string userConnectionId, string name)
        {
            var player = GetPlayerByConnectionId(userConnectionId);
            if (player != null)
            {
                player.name = name;
                if (mBan.Contains(player.name))
                {
                    //player.Log("FAILED a ban check: " + player.name);
                    player.SendPacket(new ResponseSetNamePacket(false, player.name));
                    RemovePlayer(player);
                    return;
                }
                player.SendPacket(new ResponseSetNamePacket(true, player.name));
                SendPacketToAllPlayersChannels(new PlayerChangedNamePacket(player.id, player.name), player);
            }
        }

        public void TransferObject(string userConnectionId, int playerId, int oldChannelId, int newChannelId, uint oldObjectId)
        {
            var player = GetPlayerByConnectionId(userConnectionId);
            if (player != null)
            {
                bool isNew;
                var from = player.GetChannel(oldChannelId);
                var to = CreateChannel(newChannelId, out isNew);

                if (from != null && to != null && from != to)
                {
                    var obj = from.TransferObject(oldObjectId, to, mTime);

                    if (obj != null)
                    {
                        // Notify players in the old channel
                        for (int i = 0; i < from.players.size; ++i)
                        {
                            var p = (NetworkPlayer)from.players.buffer[i];

                            if (to.players.Contains(p))
                            {
                                // The player is also present in the other channel -- inform them of the transfer
                                p.SendPacket(new TransferredObjectPacket(player.id, from.id, to.id, oldObjectId, obj.Value.objectId));
                            }
                            else
                            {
                                // The player is not present in the other channel -- delete this object
                                p.SendPacket(new ResponseDestroyObjectsPacket(from.id, player.id, new List<uint>(1) { oldObjectId }));
                            }
                        }

                        var val = obj.Value;
                        var createObjectPacket = new CreateObjectPacket(val.playerId, to.id, val.objectId, val.data.buffer);

                        // Notify players in the new channel
                        for (int i = 0; i < to.players.size; ++i)
                        {
                            var p = (NetworkPlayer)to.players.buffer[i];

                            if (!from.players.Contains(p))
                            {
                                // Send all buffered RFCs associated with this object
                                for (int b = 0; b < to.rfcs.size; ++b)
                                {
                                    if (to.rfcs.buffer[b].objectID == val.objectId)
                                    {
                                        p.SendPacket(new ForwardPacket(to.id, to.rfcs.buffer[b].uid, to.rfcs.buffer[b].functionName, ForwardType.All, to.rfcs.buffer[b].data.buffer));
                                    }
                                }
                                p.SendPacket(createObjectPacket);
                            }
                        }
                    }
                }
            }
        }




        /// <summary>
        /// Stop listening to incoming connections and disconnect all players.
        /// </summary>

        public void Stop()
        {
#if !MODDING
            Save();

            onShutdown?.Invoke(GameServerInfo.ServerId);

            mAllowUdp = false;

            // Remove all connected players and clear the list of channels
            for (int i = mPlayerList.size; i > 0;) RemovePlayer(mPlayerList.buffer[--i]);
            mPlayerList.Clear();
            mChannelList.Clear();
            mChannelDict.Clear();
            mSavedFiles.Clear();
            mBan.Clear();
            mAdmin.Clear();

            // Player counter should be reset
            Player.ResetPlayerCounter();
            mIsActive = false;
            mServerData = null;
            mListenerPort = 0;
#endif
        }

        /// <summary>
        /// Current player whos packet is being processed.
        /// </summary>

        static public volatile ServerPlayer currentPlayer;

#if !MODDING
        protected void ThreadFunction()
        {
            for (; ; )
            {
                //lock (mLock)
                //{
                //    mTime = DateTime.UtcNow.Ticks / 10000;

                //    ReceiveCommandPacket receiveCommandPacket;

                //    if (mSrc.IngestPacket(out receiveCommandPacket))
                //    {
                //        currentPlayer = GetPlayerByConnectionId(receiveCommandPacket.FromConnectionId);

                //        if (currentPlayer != null)
                //        {
                //            if (!currentPlayer.udpIsUsable) currentPlayer.udpIsUsable = true;

                //            try
                //            {
                //                ProcessPlayerPacket(receiveCommandPacket, currentPlayer);
                //            }
                //            catch (System.Exception ex)
                //            {
                //                Tools.LogError(ex.Message, ex.StackTrace, true);
                //                RemovePlayer(currentPlayer);
                //            }
                //        }
                //        else
                //        {
                //            Packet request = Packet.Empty;

                //            try
                //            {
                //                BinaryReader reader = buffer.BeginReading();
                //                var size = reader.ReadInt32();
                //                request = (Packet)reader.ReadByte();

                //                if (request == Packet.Connect)
                //                {
                //                    int theirVer = reader.ReadInt32();

                //                    if (theirVer == Player.version)
                //                    {
                //                        currentPlayer = AddServerPlayer(fromEndPoint);

                //                        var writer = currentPlayer.BeginSend(Packet.Connected);
                //                        writer.Write(Player.version);
                //                        currentPlayer.EndSend();
                //                        mPlayerList.Add(currentPlayer);
                //                    }
                //                    else
                //                    {
                //                        //TODO: ERROR?
                //                    }
                //                }
                //                else if (request == Packet.RequestPing)
                //                {
                //                    var writer = BeginSend(Packet.ResponsePing);
                //                    writer.Write(mTime);
                //                    writer.Write((ushort)playerCount);
                //                    EndSend(fromEndPoint);
                //                }
                //            }
                //            catch (System.Exception ex)
                //            {
                //                if (currentPlayer != null) currentPlayer.LogError(ex.Message, ex.StackTrace);
                //                else Tools.LogError(ex.Message, ex.StackTrace);
                //                RemovePlayer(currentPlayer);
                //            }
                //        }

                //        currentPlayer = null;
                //    }

                //    // Process player connections next
                //    for (int i = mPlayerList.size - 1; i >= 0; i--)
                //    {
                //        var player = mPlayerList.buffer[i];
                //        if (player != null)
                //        {
                //            // Remove disconnected players
                //            if (!player.isConnected && player.stage != NetworkPlayer.Stage.Connecting && player.stage != NetworkPlayer.Stage.Identifying && player.stage != NetworkPlayer.Stage.Verifying)
                //            {
                //                RemovePlayer(player);
                //                continue;
                //            }

                //            Process up to 100 packets at a time
                //            for (int b = 0; b < 100 && player.ReceivePacket(out buffer, out fromEndPoint); ++b)
                //            {
                //                if (buffer.size > 0)
                //                {
                //                    ProcessPlayerPacket(buffer, player);
                //                }
                //                buffer.Recycle();
                //            }
                //        }
                //        else
                //        {
                //            //TODO: Deal with null somehow
                //        }

                //        //TODO: Get this working during local testing
                //        // Time out -- disconnect this player
                //        //if (player.stage == NetworkPlayer.Stage.Connected)
                //        //{
                //        //    // If the player doesn't send any packets in a while, disconnect him
                //        //    if (player.timeoutTime > 0 && player.lastReceivedTime + player.timeoutTime < mTime)
                //        //    {
                //        //        RemovePlayer(player);
                //        //        continue;
                //        //    }
                //        //}
                //        //else if (player.lastReceivedTime + 2000 < mTime)
                //        //{
                //        //    RemovePlayer(player);
                //        //    continue;
                //        //}
                //    }

                //    // Save periodically
                //    if (mNextSave != 0 && mNextSave < mTime) Save();
                //}
            }
        }
#endif

        /// <summary>
        /// Add a new player entry.
        /// </summary>

        public ServerPlayer AddPlayer(string connectionId, string name)
        {
            var player = new ServerPlayer(name);
            player.AssignID();
            player.ConnectionId = connectionId;
            mPlayerList.Add(player);
            onPlayerConnect?.Invoke(player.ConnectionId, player.id, GameServerInfo.ServerId);
            player.SendToClient += onSendToClient;
            return player;
        }

        public ServerPlayer GetPlayerByName(string name)
        {
            // Exact name match
            for (int i = 0; i < mPlayerList.size; ++i)
            {
                if (mPlayerList.buffer[i].name == name)
                    return mPlayerList.buffer[i];
            }

            // Partial name match
            for (int i = 0; i < mPlayerList.size; ++i)
            {
                if (mPlayerList.buffer[i].name.IndexOf(name, StringComparison.CurrentCultureIgnoreCase) != -1)
                    return mPlayerList.buffer[i];
            }

            // Alias match
            for (int i = 0; i < mPlayerList.size; ++i)
            {
                ServerPlayer p = mPlayerList.buffer[i];
                if (p.HasAlias(name)) return p;
            }
            return null;
        }

        public ServerPlayer GetPlayerById(int id)
        {
            for (int i = 0; i < mPlayerList.size; ++i)
            {
                var p = mPlayerList.buffer[i];
                if (p.id == id) return p;
            }
            return null;
        }


        public ServerPlayer GetPlayerByConnectionId(string connectionId)
        {
            for (int i = 0; i < mPlayerList.size; ++i)
            {
                if (mPlayerList.buffer[i].ConnectionId == connectionId)
                {
                    return mPlayerList.buffer[i];
                }
            }
            return null;
        }

        public void RemovePlayer(ServerPlayer np)
        {
#if !MODDING
            if (mServerData == null || mServerData.GetChild<bool>("save", true)) SavePlayer(np);

            while (np.channels.size > 0)
            {
                var ch = np.channels.buffer[0];
                if (ch != null) SendLeaveChannel(np, ch, false);
                else np.channels.RemoveAt(0);
            }

            if (mPlayerList.Contains(np))
            {
                mPlayerList.Remove(np);
            }

            np.SendToClient -= onSendToClient;

            onPlayerDisconnect?.Invoke(np.ConnectionId, np.id, GameServerInfo.ServerId);
#endif
        }

        /// <summary>
        /// Create a new channel (or return an existing one).
        /// </summary>

        protected Channel CreateChannel(int channelID, out bool isNew)
        {
#if !MODDING
            Channel channel;

            if (mChannelDict.TryGetValue(channelID, out channel))
            {
                isNew = false;
                return channel;
            }

            channel = new Channel();
            channel.id = channelID;
            mChannelList.Add(channel);
            mChannelDict[channelID] = channel;
            isNew = true;
            return channel;
#else
			isNew = false;
			return null;
#endif
        }

        /// <summary>
        /// Check to see if the specified channel exists.
        /// </summary>

        protected bool ChannelExists(int id) { return mChannelDict.ContainsKey(id); }

        protected Channel GetChannel(int id)
        {
            if (ChannelExists(id))
            {
                return mChannelDict[id];
            }
            return null;
        }

        /// <summary>
        /// Send the outgoing buffer to all players in the specified channel.
        /// </summary>

        protected void SendPacket(CommandPacket commandPacket, Channel channel, NetworkPlayer exclude)
        {
#if !MODDING
            for (int i = 0; i < channel.players.size; ++i)
            {
                var player = (NetworkPlayer)channel.players.buffer[i];

                if (player != exclude)
                {
                    player.SendPacket(commandPacket);
                }
            }
#endif
        }

        protected void SendPacket(CommandPacket commandPacket, int channelId, NetworkPlayer exclude)
        {
#if !MODDING
            var channel = GetChannel(channelId);
            if (channel != null)
            {
                SendPacket(commandPacket, channel, exclude);
            }
#endif
        }

        protected void SendPacketToAllPlayersChannels(CommandPacket commandPacket, NetworkPlayer player)
        {
#if !MODDING
            foreach(var ch in player.channels)
            {
                ch.SendToAll(commandPacket);
            }
#endif
        }

#if !MODDING
        protected HashSet<ServerPlayer> mSentList = new HashSet<ServerPlayer>();
#endif

        /// <summary>
        /// Send the specified buffer to all players in the same channel as the source.
        /// </summary>

        protected void SendToOthers(CommandPacket commandPacket, ServerPlayer source, ServerPlayer exclude)
        {
#if !MODDING
            for (int b = 0; b < source.channels.size; ++b)
            {
                var ch = source.channels.buffer[b];

                for (int i = 0; i < ch.players.size; ++i)
                {
                    var p = (ServerPlayer)ch.players.buffer[i];

                    if (p != exclude && !mSentList.Contains(p))
                    {
                        mSentList.Add(p);
                        p.SendPacket(commandPacket);
                    }
                }
            }

            mSentList.Clear();
#endif
        }

        /// <summary>
        /// Have the specified player assume control of the channel.
        /// </summary>

        protected void SendSetHost(Channel ch, ServerPlayer player)
        {
#if !MODDING
            if (ch != null && ch.host != player)
            {
                ch.host = player;
                SendPacket(new ResponseSetHostPacket(ch.id, player.id), ch, null);
            }
#endif
        }

        // Temporary buffer used in SendLeaveChannel below
        protected List<uint> mTemp = new List<uint>();

        /// <summary>
        /// Leave the channel the player is in.
        /// </summary>

        protected void SendLeaveChannel(ServerPlayer player, Channel ch, bool notify)
        {
#if !MODDING
            if (ch == null) return;

            // Remove this player from the channel
            ch.RemovePlayer(player, mTemp);

            if (player.channels.Remove(ch))
            {
                // Are there other players left?
                if (ch.players.size > 0)
                {
                    // Inform the other players that the player's objects should be destroyed
                    if (mTemp.Count > 0)
                    {
                        SendPacket(new ResponseDestroyObjectsPacket(ch.id, player.id, mTemp), ch, null);
                    }

                    // If this player was the host, choose a new host
                    if (ch.host == null) SendSetHost(ch, (ServerPlayer)ch.players.buffer[0]);

                    // Inform everyone of this player leaving the channel
                    SendPacket(new PlayerLeftChannelPacket(ch.id, player.id, string.IsNullOrEmpty(player.name) ? "Guest" : player.name, player.dataNode), ch, null);
                }
                else if (!ch.isPersistent)
                {
                    // No other players left -- delete this channel
                    mChannelDict.Remove(ch.id);
                    mChannelList.Remove(ch);
                }

                // Notify the player that they have left the channel
                if (notify && player.isConnectedToGameServer)
                {
                    player.SendPacket(new ResponseLeaveChannelPacket(ch.id));
                }

                if (!string.IsNullOrEmpty(ch.level))
                {
                    player.SendPacket(new UnloadLevelPacket(ch.level));
                }
            }

            // Put the channel to sleep after all players leave
            if (ch.players.size == 0) ch.Sleep();
#endif
        }

        /// <summary>
        /// Handles joining the specified channel.
        /// </summary>

        //        protected void SendJoinChannel(NetworkPlayer player, int channelID, string pass, string levelName, bool persist, ushort playerLimit)
        //        {
        //#if !MODDING
        //            // Join a random existing channel or create a new one
        //            if (channelID == -2)
        //            {
        //                bool randomLevel = string.IsNullOrEmpty(levelName);
        //                channelID = -1;

        //                for (int i = 0; i < mChannelList.size; ++i)
        //                {
        //                    Channel ch = mChannelList.buffer[i];

        //                    if (ch.isOpen && (randomLevel || levelName.Equals(ch.level)) &&
        //                        (string.IsNullOrEmpty(ch.password) || (ch.password == pass)))
        //                    {
        //                        channelID = ch.id;
        //                        break;
        //                    }
        //                }
        //            }

        //            // Join a random new channel
        //            if (channelID == -1)
        //            {
        //                channelID = 10001 + mRandom.Next(100000000);

        //                for (int i = 0; i < 1000; ++i)
        //                {
        //                    if (!ChannelExists(channelID)) break;
        //                    channelID = 10001 + mRandom.Next(100000000);
        //                }
        //            }

        //            if (player.channels.size == 0 || !player.IsInChannel(channelID))
        //            {
        //                bool isNew;
        //                Channel channel = CreateChannel(channelID, out isNew);

        //                if (channel == null || !channel.isOpen)
        //                {
        //                    BinaryWriter writer = player.BeginSend(Packet.ResponseJoinChannel);
        //                    writer.Write(channelID);
        //                    writer.Write(false);
        //                    writer.Write("The requested channel is closed");
        //                    player.EndSend();
        //                }
        //                else if (isNew)
        //                {
        //                    channel.password = pass;
        //                    channel.isPersistent = persist;
        //                    channel.level = levelName;
        //                    channel.playerLimit = playerLimit;

        //                    SendJoinChannel(player, channel, levelName);
        //                }
        //                else if (string.IsNullOrEmpty(channel.password) || (channel.password == pass))
        //                {
        //                    SendJoinChannel(player, channel, channel.level);
        //                }
        //                else
        //                {
        //                    BinaryWriter writer = player.BeginSend(Packet.ResponseJoinChannel);
        //                    writer.Write(channelID);
        //                    writer.Write(false);
        //                    writer.Write("Wrong password");
        //                    player.EndSend();
        //                }
        //            }
        //#endif
        //        }

        /// <summary>
        /// Join the specified channel.
        /// </summary>

        protected void SendJoinChannel(NetworkPlayer player, ushort playerLimit, bool persistent, string password, int channelId, string requestedLevelName, bool additive)
        {
#if !MODDING
            // Join a random existing channel or create a new one
            if (channelId == -2)
            {
                bool randomLevel = string.IsNullOrEmpty(requestedLevelName);
                channelId = -1;

                for (int i = 0; i < mChannelList.size; ++i)
                {
                    Channel ch = mChannelList.buffer[i];

                    if (ch.isOpen && (randomLevel || requestedLevelName.Equals(ch.level)))// && (string.IsNullOrEmpty(ch.password) || (ch.password == password)))
                    {
                        channelId = ch.id;
                        break;
                    }
                }
            }

            // Join a random new channel
            if (channelId == -1)
            {
                channelId = 10001 + mRandom.Next(100000000);

                for (int i = 0; i < 1000; ++i)
                {
                    if (!ChannelExists(channelId)) break;
                    channelId = 10001 + mRandom.Next(100000000);
                }
            }

            if (player.channels.size == 0 || !player.IsInChannel(channelId))
            {
                bool isNew;
                Channel channel = CreateChannel(channelId, out isNew);
                if (channel == null || !channel.isOpen)
                {
                    player.SendPacket(new ResponseJoinChannelPacket(channelId, false, "The requested channel is closed"));
                    return;
                }
                else if (isNew)
                {
                    //channel.password = password;
                    channel.isPersistent = persistent;
                    channel.level = requestedLevelName;
                    channel.playerLimit = playerLimit;
                }
                //else if (!string.IsNullOrEmpty(channel.password) && (channel.password != password))
                //{
                //    player.SendPacket(new ResponseJoinChannelPacket(channelId, false, "Wrong password"));
                //    return;
                //}

                // Load the channel's data into memory
                channel.Wake();

                // Set the player's channel
                player.channels.Add(channel);

                // Tell the player who else is in the channel
                var playersInChannel = new List<JoiningChannelPacket.PlayerInChannel>();

                for (int i = 0; i < channel.players.size; ++i)
                {
                    var tp = channel.players.buffer[i];
                    playersInChannel.Add(new JoiningChannelPacket.PlayerInChannel(tp.id, tp.name, tp.dataNode));
                }
                if (channel.host == null)
                {
                    channel.host = player;
                }
                player.SendPacket(new JoiningChannelPacket(channel.id, playersInChannel, channel.host.id, channel.dataNode));

                if (!string.IsNullOrEmpty(requestedLevelName) && !string.IsNullOrEmpty(channel.level))
                {
                    //TODO: Find out what to do with requestedLevelName
                    player.SendPacket(new LoadLevelPacket(channel.id, channel.level, additive));
                }

                // Send the list of objects that have been created
                for (int i = 0; i < channel.created.size; ++i)
                {
                    var obj = channel.created.buffer[i];

                    bool isPresent = false;

                    for (int b = 0; b < channel.players.size; ++b)
                    {
                        if (channel.players.buffer[b].id == obj.playerId)
                        {
                            isPresent = true;
                            break;
                        }
                    }

                    // If the previous owner is not present, transfer ownership to the host
                    if (!isPresent)
                    {
                        obj.playerId = channel.host.id;
                        channel.created.buffer[i] = obj;
                    }

                    player.SendPacket(new CreateObjectPacket(obj.playerId, channel.id, obj.objectId, obj.data.buffer));
                }

                // Send the list of objects that have been destroyed
                if (channel.destroyed.size != 0)
                {
                    player.SendPacket(new DestroyObjectsPacket(channel.id, channel.destroyed.buffer));
                }

                // Send all buffered RFCs to the new player
                for (int i = 0; i < channel.rfcs.size; ++i)
                {
                    player.SendPacket(new ForwardPacket(channel.id, channel.rfcs.buffer[i].uid, channel.rfcs.buffer[i].functionName, ForwardType.All, channel.rfcs.buffer[i].data.buffer));
                }

                // Send the channel properties
                var flags = channel.isPersistent ? 1 : 0;
                if (channel.isClosed) flags |= 2;
                if (channel.isLocked) flags |= 4;
                //if (!string.IsNullOrEmpty(channel.password)) flags |= 8;
                player.SendPacket(new UpdateChannelPacket(channel.id, channel.playerLimit, (ushort)flags));

                // The join process is now complete
                player.SendPacket(new ResponseJoinChannelPacket(channel.id));

#if UNITY_EDITOR
                    if (buffer.size > 100000)
                    {
                        //Tools.WriteFile("dump.txt", buffer.buffer, false, false, buffer.size);
                        Debug.Log("Packet size: " + buffer.size.ToString("N0"));
                    }
#endif

                // Inform the channel that a new player is joining
                for (int i = 0; i < channel.players.size; ++i)
                {
                    var p = (NetworkPlayer)channel.players.buffer[i];
                    p.SendPacket(new PlayerJoinedChannelPacket(channel.id, player.id, string.IsNullOrEmpty(player.name) ? "Guest" : player.name, player.dataNode));
                }

                // Add this player to the channel now that the joining process is complete
                channel.players.Add(player);
            }
#endif
        }

        /// <summary>
        /// Write the channel's properties into the chosen buffer.
        /// </summary>

        //static void WriteChannelDescriptor(Channel ch, Buffer buffer)
        //{
        //    var writer = buffer.BeginPacket(Packet.ResponseUpdateChannel);

        //    writer.Write(ch.id);
        //    writer.Write(ch.playerLimit);

        //    int val = ch.isPersistent ? 1 : 0;
        //    if (ch.isClosed) val |= 2;
        //    if (ch.isLocked) val |= 4;
        //    if (!string.IsNullOrEmpty(ch.password)) val |= 8;
        //    writer.Write((ushort)val);
        //}

        /// <summary>
        /// Extra verification steps, if necessary.
        /// </summary>

        protected virtual bool Verify(BinaryReader reader) { return true; }

#if !MODDING
        /// <summary>
        /// Receive and process a single incoming packet.
        /// Returns 'true' if a packet was received, 'false' otherwise.
        /// </summary>

        //        protected bool ProcessPlayerPacket(ReceiveCommandPacket receiveCommandPacket, NetworkPlayer player)
        //        {
        //            // Save every 5 minutes
        //            if (mNextSave == 0) mNextSave = mTime + 300000;
        //            var reader = buffer.BeginReading();
        //            var size = reader.ReadInt32();
        //            var requestByte = reader.ReadByte();
        //            var request = (Packet)requestByte;
        //            // If the player has not yet been identified, the packet must be an ID request
        //            if (player.stage == NetworkPlayer.Stage.Identifying)
        //            {
        //                if (request == Packet.RequestID)
        //                {
        //                    player.name = reader.ReadString();
        //                    player.dataNode = reader.ReadDataNode();
        //                    player.stage = NetworkPlayer.Stage.Connected;

        //#if DEBUG_PACKETS && !STANDALONE
        //					UnityEngine.Debug.Log("Protocol verified");
        //#endif
        //                    //TODO: Set admin
        //                    //player.isAdmin = (player.custom == null) && (player.address == null || player.address == "0.0.0.0:0" || player.address.StartsWith("127.0.0.1:"));

        //                    if (!mBan.Contains(player.name) || player.isAdmin)
        //                    {
        //                        player.AssignID();
        //#if STANDALONE || UNITY_EDITOR
        //                        Tools.Log(player.name + " (" + player.ConnectionId + "): Connected [" + player.id + "]");
        //#endif
        //                        mPlayerDict.Add(player.id, player);
        //                        var writer = player.BeginSend(Packet.ResponseID);
        //                        writer.Write(player.id);
        //                        writer.Write((Int64)(System.DateTime.UtcNow.Ticks / 10000));
        //                        writer.Write(mStartTime);
        //                        player.EndSend();

        //                        if (mServerData != null)
        //                        {
        //                            writer = player.BeginSend(Packet.ResponseSetServerData);
        //                            writer.Write("");
        //                            writer.WriteObject(mServerData);
        //                            player.EndSend();
        //                        }

        //                        if (player.isAdmin)
        //                        {
        //                            player.BeginSend(Packet.ResponseVerifyAdmin).Write(player.id);
        //                            player.EndSend();
        //                        }

        //                        // Connection has now been established and the player should be notified of such
        //                        player.BeginSend(Packet.ResponseConnected);
        //                        player.EndSend();

        //                        if (onPlayerConnect != null) onPlayerConnect(player);
        //                        return true;
        //                    }
        //                    else
        //                    {
        //                        player.Log("User is banned");
        //                        RemovePlayer(player);
        //                        return false;
        //                    }
        //                }

        //                player.BeginSend(Packet.ResponseID).Write(0);
        //                player.EndSend();

        //                Tools.Print(player.ConnectionId + " has failed the verification step");
        //                RemovePlayer(player);
        //                return false;
        //            }

        //#if DEBUG_PACKETS && !STANDALONE
        //#if UNITY_EDITOR
        //			if (request != Packet.RequestPing && request != Packet.ResponsePing && request != Packet.ForwardToOthersSaved)
        //				UnityEngine.Debug.Log(player.name + ": " + request + " (" + buffer.size.ToString("N0") + " bytes)");
        //#elif !SINGLE_THREADED
        //			if (request != Packet.RequestPing && request != Packet.ResponsePing)
        //				Tools.Print(player.name + ": " + request + " (" + buffer.size.ToString("N0") + " bytes)");
        //#endif
        //#endif
        //            switch (request)
        //            {
        //                case Packet.Empty:
        //                    {
        //                        break;
        //                    }
        //                case Packet.Error:
        //                    {
        //                        player.LogError(reader.ReadString());
        //                        break;
        //                    }
        //                case Packet.Disconnect:
        //                    {
        //                        RemovePlayer(player);
        //                        break;
        //                    }
        //                case Packet.RequestPing:
        //                    {
        //                        // Respond with a ping back
        //                        var writer = player.BeginSend(Packet.ResponsePing);
        //                        writer.Write(mTime);
        //                        writer.Write((ushort)playerCount);
        //                        player.EndSend();
        //                        break;
        //                    }
        //                case Packet.RequestSendChat:
        //                    {
        //                        var pid = reader.ReadInt32();
        //                        var txt = reader.ReadString();

        //                        if (pid != 0)
        //                        {
        //                            var p = GetPlayer(pid);
        //                            if (p != null)
        //                            {
        //                                var writer = p.BeginSend(Packet.ResponseSendChat);
        //                                writer.Write(player.id);
        //                                writer.Write(txt);
        //                                writer.Write(pid != 0);
        //                                p.EndSend();
        //                            }
        //                        }
        //                        else
        //                        {
        //                            var writer = BeginSend(Packet.ResponseSendChat);
        //                            writer.Write(player.id);
        //                            writer.Write(txt);
        //                            writer.Write(pid != 0);

        //                            EndSendToOthers(player, null);
        //                        }
        //                        return true;
        //                    }
        //                case Packet.RequestSetUDP:
        //                    {
        //                        //int port = reader.ReadUInt16();

        //                        //if (port != 0 && mSrc.isConnected && player.tcpEndPoint != null)
        //                        //{
        //                        //    SetPlayerUdpEndPoint(player, new IPEndPoint(player.tcpEndPoint.Address, port));
        //                        //}
        //                        //else SetPlayerUdpEndPoint(player, null);

        //                        //// Let the player know if we are hosting an active UDP connection
        //                        //ushort udp = mSrc.isActive ? (ushort)mSrc.listeningPort : (ushort)0;
        //                        //player.BeginSend(Packet.ResponseSetUDP).Write(udp);
        //                        //player.EndSend();

        //                        //// Send an empty packet to the target player to open up UDP for communication
        //                        //if (player.udpEndPoint != null) mSrc.SendEmptyPacket(player.udpEndPoint);
        //                        //break;
        //                        break;
        //                    }
        //                case Packet.RequestActivateUDP:
        //                    {
        //                        //player.udpIsUsable = true;
        //                        //if (player.udpEndPoint != null) mSrc.SendEmptyPacket(player.udpEndPoint);
        //                        //break;
        //                        break;
        //                    }
        //                case Packet.RequestSetPlayerData:
        //                    {
        //                        // 4 bytes for the size, 1 byte for the ID
        //                        int origin = buffer.position - 5;

        //                        // Set the local data
        //                        int playerID = reader.ReadInt32();
        //                        string str = reader.ReadString();
        //                        object obj = reader.ReadObject();

        //                        if (player.id == playerID)
        //                        {
        //                            // The rare case of setting the entire data node: for example player loads a local save file
        //                            if (string.IsNullOrEmpty(str))
        //                            {
        //                                // It's not possible to set the entire player data to nothing
        //                                if (obj == null) break;

        //                                if (obj is DataNode)
        //                                {
        //                                    // Players can't change the "Server" node, so simply remove it
        //                                    var node = obj as DataNode;
        //                                    node.RemoveChild("Server");

        //                                    // Add our existing "Server" node
        //                                    if (player.dataNode != null)
        //                                    {
        //                                        var ours = player.dataNode.GetChild("Server");
        //                                        if (ours != null) node.children.Add(ours);
        //                                    }

        //                                    player.dataNode = node;
        //                                    player.saveNeeded = true;

        //                                    var buff = Buffer.Create();
        //                                    var writer = buff.BeginPacket(Packet.ResponseSetPlayerData);
        //                                    writer.Write(player.id);
        //                                    writer.Write("");
        //                                    writer.WriteObject(player.dataNode);
        //                                    buff.EndPacket();

        //                                    SendToOthers(buff, player, player);
        //                                    buff.Recycle();
        //                                    break;
        //                                }

        //                                player.Set(str, obj);
        //                                player.saveNeeded = true;
        //                            }
        //                            else if (!str.StartsWith("Server"))
        //                            {
        //                                player.Set(str, obj);
        //                                player.saveNeeded = true;
        //                            }
        //                            else break; // Silently ignore attempts to set server data

        //                            // Change the packet type to a response before sending it as-is
        //                            buffer.buffer[origin + 4] = (byte)Packet.ResponseSetPlayerData;
        //                            buffer.position = origin;

        //                            // Forward the packet to everyone that knows this player
        //                            for (int i = 0; i < mPlayerList.size; ++i)
        //                            {
        //                                NetworkPlayer tp = mPlayerList.buffer[i];
        //                                if (tp != player && tp.IsKnownTo(player)) tp.SendPacket(buffer);
        //                            }
        //                        }
        //                        else player.LogError("Players should only set their own data. Ignoring.", null, false);
        //                        break;
        //                    }
        //                case Packet.RequestSetPlayerSave:
        //                    {
        //                        var path = reader.ReadString();
        //                        var type = (DataNode.SaveType)reader.ReadByte();
        //                        var hash = reader.ReadInt();
        //#if W2
        //					var expected = "Players/" + player.aliases.buffer[0] + ".player";

        //					if (player.aliases == null || player.aliases.size == 0 || path != expected)
        //					{
        //						player.LogError("Player requested a save that doesn't match the alias: " + path + " vs " + expected);
        //						RemovePlayer(player);
        //						return false;
        //					}
        //#endif
        //                        // Load and set the player's data from the specified file
        //                        //TODO: Can't read from disk
        //                        //player.dataNode = DataNode.Read(string.IsNullOrEmpty(rootDirectory) ? path : Path.Combine(rootDirectory, path));

        //                        // The player data must be valid at this point
        //                        if (player.dataNode == null)
        //                        {
        //                            player.dataNode = new DataNode("Version", Player.version);
        //                            if (hash != 0) player.dataNode.AddChild("hash", hash);
        //                        }
        //                        else
        //                        {
        //                            player.saveNeeded = false;
        //                            var existing = player.dataNode.GetChild("hash", 0);

        //                            if (hash != existing)
        //                            {
        //                                if (existing == 0 || (mServerData != null && mServerData.GetChild<bool>("ignoreHashChecks", false))
        //#if !STANDALONE
        //                                || TNServerInstance.isActive
        //#endif
        //                            )
        //                                {
        //                                    player.dataNode.SetChild("hash", hash);
        //                                    player.saveNeeded = true;
        //                                }
        //                                else
        //                                {
        //                                    player.LogError("Player file hash mismatch: " + hash + " vs " + existing);
        //                                    player.SendError("Player file hash mismatch");
        //                                    RemovePlayer(player);
        //                                    return false;
        //                                }
        //                            }
        //                        }

        //                        if (player.savePath != path)
        //                        {
        //                            // This handling will prevent the new player from connecting
        //                            if (string.IsNullOrEmpty(player.savePath))
        //                            {
        //                                // First time setting the save file -- make sure it's unique and remove the already connected player that shares the same save file
        //                                for (int i = 0; i < mPlayerList.size; ++i)
        //                                {
        //                                    var existing = mPlayerList.buffer[i];
        //                                    if (existing == player) continue;

        //                                    if (existing.savePath == path)
        //                                    {
        //                                        player.LogError("Already connected (" + existing.ConnectionId + ")");
        //                                        player.SendError("Already connected");
        //                                        RemovePlayer(player);
        //                                        return false;
        //                                    }
        //                                }
        //                            }
        //                            else
        //                            {
        //                                // Changing the save file -- make sure it doesn't conflict with any existing player
        //                                for (int i = 0; i < mPlayerList.size; ++i)
        //                                {
        //                                    var existing = mPlayerList.buffer[i];
        //                                    if (existing == player) continue;

        //                                    if (existing.savePath == path)
        //                                    {
        //                                        player.LogError("Already connected (" + existing.ConnectionId + ")");
        //                                        player.SendError("Already connected");
        //                                        RemovePlayer(player);
        //                                        return false;
        //                                    }
        //                                }

        //                                // Delete the previous save
        //                                //Tools.DeleteFile(player.savePath);
        //                            }

        //                            // This handling would kick the existing player off
        //                            /*if (string.IsNullOrEmpty(player.savePath))
        //                            {
        //                                // First time setting the save file -- make sure it's unique and remove the already connected player that shares the same save file
        //                                for (int i = 0; i < mPlayerList.size; ++i)
        //                                {
        //                                    var existing = mPlayerList.buffer[i];
        //                                    if (existing == player) continue;

        //                                    if (existing.savePath == path)
        //                                    {
        //                                        existing.LogError("Connected from another location");
        //                                        existing.SendError("Connected from another location");
        //                                        RemovePlayer(existing);
        //                                        break;
        //                                    }
        //                                }
        //                            }
        //                            else
        //                            {
        //                                // Changing the save file -- make sure it doesn't conflict with any existing player
        //                                for (int i = 0; i < mPlayerList.size; ++i)
        //                                {
        //                                    var existing = mPlayerList.buffer[i];
        //                                    if (existing == player) continue;

        //                                    if (existing.savePath == path)
        //                                    {
        //                                        player.LogError("Already connected");
        //                                        player.SendError("Already connected");
        //                                        RemovePlayer(player);
        //                                        return false;
        //                                    }
        //                                }

        //                                // Delete the previous save
        //                                Tools.DeleteFile(player.savePath);
        //                            }*/
        //                        }

        //                        player.savePath = path;
        //                        player.saveType = type;

        //                        // We want to record the player's login time so that we can automatically keep track of that player's /played time
        //                        player.dataNode.GetChild("Server", true).SetChild("lastSave", mTime);

        //                        var buff = Buffer.Create();
        //                        var writer = buff.BeginPacket(Packet.ResponseSetPlayerData);
        //                        writer.Write(player.id);
        //                        writer.Write("");
        //                        writer.WriteObject(player.dataNode);
        //                        buff.EndPacket();

        //                        player.SendPacket(buff);
        //                        SendToOthers(buff, player, player);
        //                        buff.Recycle();
        //                        break;
        //                    }
        //                case Packet.RequestSaveFile:
        //                    {
        //                        try
        //                        {
        //                            string fileName = reader.ReadString();
        //                            byte[] data = reader.ReadBytes(reader.ReadInt32());

        //                            if (!string.IsNullOrEmpty(fileName))
        //                            {
        //                                if (data == null || data.Length == 0)
        //                                {
        //                                    //if (DeleteFile(fileName))
        //                                    //    player.Log("Deleted " + fileName);
        //                                }
        //                                //else if (SaveFile(fileName, data))
        //                                //{
        //                                //    player.Log("Saved " + fileName + " (" + (data != null ? data.Length.ToString("N0") : "0") + " bytes)");
        //                                //}
        //                                else player.LogError("Unable to save " + fileName);
        //                            }
        //                        }
        //                        catch (Exception ex)
        //                        {
        //                            player.LogError(ex.Message, ex.StackTrace);
        //                            RemovePlayer(player);
        //                        }
        //                        break;
        //                    }
        //                case Packet.RequestLoadFile:
        //                    {
        //                        //string fn = reader.ReadString();
        //                        //byte[] data = LoadFile(fn);

        //                        //BinaryWriter writer = BeginSend(Packet.ResponseLoadFile);
        //                        //writer.Write(fn);

        //                        //if (data != null)
        //                        //{
        //                        //    writer.Write(data.Length);
        //                        //    writer.Write(data);
        //                        //}
        //                        //else writer.Write(0);

        //                        //EndSend(player);
        //                        break;
        //                    }
        //                case Packet.RequestDeleteFile:
        //                    {
        //                        string fileName = reader.ReadString();

        //                        if (!string.IsNullOrEmpty(fileName))
        //                        {
        //                            //if (DeleteFile(fileName))
        //                            //    player.Log("Deleted " + fileName);
        //                        }
        //                        break;
        //                    }
        //                case Packet.RequestNoDelay:
        //                    {
        //                        player.noDelay = reader.ReadBoolean();
        //                        break;
        //                    }
        //                case Packet.RequestChannelList:
        //                    {
        //                        BinaryWriter writer = player.BeginSend(Packet.ResponseChannelList);

        //                        int count = 0;
        //                        for (int i = 0; i < mChannelList.size; ++i)
        //                            if (!mChannelList.buffer[i].isClosed) ++count;

        //                        writer.Write(count);

        //                        for (int i = 0; i < mChannelList.size; ++i)
        //                        {
        //                            Channel ch = mChannelList.buffer[i];

        //                            if (!ch.isClosed)
        //                            {
        //                                writer.Write(ch.id);
        //                                writer.Write((ushort)ch.players.size);
        //                                writer.Write(ch.playerLimit);
        //                                writer.Write(!string.IsNullOrEmpty(ch.password));
        //                                writer.Write(ch.isPersistent);
        //                                writer.Write(ch.level);
        //                                writer.Write(ch.dataNode);
        //                            }
        //                        }
        //                        player.EndSend();
        //                        break;
        //                    }
        //                case Packet.ServerLog:
        //                    {
        //#if UNITY_EDITOR
        //                        var s = reader.ReadString();
        //                        Debug.Log(s);
        //#else
        //                        var s = reader.ReadString();
        //                        player.Log(s);
        //#endif
        //                        break;
        //                    }
        //                case Packet.RequestSetTimeout:
        //                    {
        //                        // The passed value is in seconds, but the stored value is in milliseconds (to avoid a math operation)
        //                        player.timeoutTime = reader.ReadInt32() * 1000;
        //                        break;
        //                    }
        //                case Packet.ForwardToPlayer:
        //                    {
        //                        // Forward this packet to the specified player
        //                        int origin = buffer.position - 5;

        //                        if (reader.ReadInt32() == player.id) // Validate the packet's source
        //                        {
        //                            var target = GetPlayer(reader.ReadInt32());

        //                            if (target != null && target.isConnected)
        //                            {
        //                                buffer.position = origin;
        //                                target.SendPacket(buffer);
        //                            }
        //                        }
        //                        break;
        //                    }
        //                case Packet.ForwardByName:
        //                    {
        //                        int origin = buffer.position - 5;

        //                        if (reader.ReadInt32() == player.id) // Validate the packet's source
        //                        {
        //                            string name = reader.ReadString();
        //                            NetworkPlayer target = GetPlayer(name);

        //                            if (target != null && target.isConnected)
        //                            {
        //                                buffer.position = origin;
        //                                target.SendPacket(buffer);
        //                            }
        //                            //else if (reliable)
        //                            //{
        //                            //    BeginSend(Packet.ForwardTargetNotFound).Write(name);
        //                            //    EndSend(player);
        //                            //}
        //                        }
        //                        break;
        //                    }
        //                case Packet.BroadcastAdmin:
        //                case Packet.Broadcast:
        //                    {
        //                        // 4 bytes for the size, 1 byte for the ID
        //                        int origin = buffer.position - 5;

        //                        //Tools.Print("Broadcast: " + player.name + ", " + player.address);

        //                        if (!player.isAdmin)
        //                        {
        //                            if (player.nextBroadcast < mTime)
        //                            {
        //                                player.nextBroadcast = mTime + 500;
        //                                player.broadcastCount = 0;
        //                            }
        //                            else if (++player.broadcastCount > 5)
        //                            {
        //                                player.Log("SPAM filter trigger!");
        //                                RemovePlayer(player);
        //                                break;
        //                            }
        //                            else if (player.broadcastCount > 2)
        //                            {
        //                                player.Log("Possible spam!");
        //                            }
        //                        }

        //                        int playerID = reader.ReadInt32();

        //                        // Exploit: echoed packet of another player
        //                        if (playerID != player.id)
        //                        {
        //                            player.LogError("Tried to echo a broadcast packet (" + playerID + " vs " + player.id + ")", null);
        //                            RemovePlayer(player);
        //                            break;
        //                        }

        //                        buffer.position = origin;

        //                        // Forward the packet to everyone connected to the server
        //                        for (int i = 0; i < mPlayerList.size; ++i)
        //                        {
        //                            NetworkPlayer tp = mPlayerList.buffer[i];
        //                            if (!tp.isConnected) continue;
        //                            if (request == Packet.BroadcastAdmin && !tp.isAdmin) continue;

        //                            tp.SendPacket(buffer);
        //                        }
        //                        break;
        //                    }
        //                case Packet.RequestVerifyAdmin:
        //                    {
        //                        string pass = reader.ReadString();

        //                        //if (mAdmin.Count == 0) { mAdmin.Add(pass); SaveAdminList(); }

        //                        if (!string.IsNullOrEmpty(pass) && mAdmin.Contains(pass))
        //                        {
        //                            if (!player.isAdmin)
        //                            {
        //                                player.isAdmin = true;
        //                                player.Log("Admin verified");
        //                                player.BeginSend(Packet.ResponseVerifyAdmin).Write(player.id);
        //                                player.EndSend();
        //                            }
        //                        }
        //                        else
        //                        {
        //                            player.LogError("Tried to authenticate as admin and failed (" + pass + ")");
        //                            RemovePlayer(player);
        //                        }
        //                        break;
        //                    }
        //                case Packet.RequestCreateAdmin:
        //                    {
        //                        string s = reader.ReadString();

        //                        if (player.isAdmin)
        //                        {
        //                            if (!mAdmin.Contains(s)) mAdmin.Add(s);
        //                            player.Log("Added an admin (" + s + ")");
        //                            //SaveAdminList();
        //                        }
        //                        else
        //                        {
        //                            player.LogError("Tried to add an admin (" + s + ") and failed");
        //                            RemovePlayer(player);
        //                        }
        //                        break;
        //                    }
        //                case Packet.RequestRemoveAdmin:
        //                    {
        //                        string s = reader.ReadString();

        //                        // First administrator can't be removed
        //                        if (player.isAdmin)
        //                        {
        //                            if (mAdmin.Remove(s))
        //                            {
        //                                player.Log("Removed an admin (" + s + ")");
        //                                //SaveAdminList();
        //                            }
        //                        }
        //                        else
        //                        {
        //                            player.LogError("Tried to remove an admin (" + s + ") without authorization", null);
        //                            RemovePlayer(player);
        //                        }
        //                        break;
        //                    }
        //                case Packet.RequestSetAlias:
        //                    {
        //                        string s = reader.ReadString();
        //                        if (!SetAlias(player, s)) break;

        //                        if (mAdmin.Contains(s))
        //                        {
        //                            player.isAdmin = true;
        //                            player.Log("Admin verified");
        //                            player.BeginSend(Packet.ResponseVerifyAdmin).Write(player.id);
        //                            player.EndSend();
        //                        }

        //                        if (mServerData != null)
        //                        {
        //                            int max = mServerData.GetChild<int>("maxAlias", int.MaxValue);
        //                            int aliasCount = (player.aliases == null ? 0 : player.aliases.size);

        //                            if (aliasCount > max)
        //                            {
        //                                player.Log("Player has " + aliasCount + "/" + max + " aliases");
        //                                RemovePlayer(player);
        //                                return false;
        //                            }
        //                        }
        //                        break;
        //                    }
        //                case Packet.RequestUnban:
        //                    {
        //                        string s = reader.ReadString();

        //                        if (player.isAdmin)
        //                        {
        //                            mBan.Remove(s);
        //                            //SaveBanList();
        //                            player.Log("Removed an banned keyword (" + s + ")");
        //                        }
        //                        else
        //                        {
        //                            player.LogError("Tried to unban (" + s + ") without authorization", null);
        //                            RemovePlayer(player);
        //                        }
        //                        break;
        //                    }
        //                case Packet.RequestSetBanList:
        //                    {
        //                        string s = reader.ReadString();

        //                        if (player.isAdmin)
        //                        {
        //                            if (!string.IsNullOrEmpty(s))
        //                            {
        //                                string[] lines = s.Split('\n');
        //                                mBan.Clear();
        //                                for (int i = 0; i < lines.Length; ++i) mBan.Add(lines[i]);
        //                            }
        //                            else mBan.Clear();
        //                        }
        //                        else
        //                        {
        //                            player.LogError("Tried to set the ban list without authorization", null);
        //                            RemovePlayer(player);
        //                        }
        //                        break;
        //                    }
        //                case Packet.RequestReloadServerConfig:
        //                    {
        //                        if (player.isAdmin)
        //                        {
        //                            //LoadBanList();
        //                            //LoadAdminList();
        //                            LoadConfig();

        //                            if (mServerData == null) mServerData = new DataNode("Version", Player.version);

        //                            Buffer buff = Buffer.Create();
        //                            var writer = buff.BeginPacket(Packet.ResponseSetServerData);
        //                            writer.Write("");
        //                            writer.WriteObject(mServerData);
        //                            buff.EndPacket();

        //                            // Forward the packet to everyone connected to the server
        //                            for (int i = 0; i < mPlayerList.size; ++i)
        //                            {
        //                                NetworkPlayer tp = mPlayerList.buffer[i];
        //                                tp.SendPacket(buff);
        //                            }
        //                            buff.Recycle();
        //                        }
        //                        else
        //                        {
        //                            player.LogError("Tried to request reloaded server data without authorization", null);
        //                            RemovePlayer(player);
        //                        }
        //                        break;
        //                    }
        //                case Packet.RequestSetServerData:
        //                    {
        //                        // Only administrators can set the server data to prevent sensitive data corruption
        //                        if (player.isAdmin)
        //                        {
        //                            if (mServerData == null) mServerData = new DataNode("Version", Player.version);

        //                            // 4 bytes for size, 1 byte for ID
        //                            int origin = buffer.position - 5;

        //                            // Change the local configuration
        //                            mServerData.SetHierarchy(reader.ReadString(), reader.ReadObject());
        //                            mServerDataChanged = true;

        //                            // Change the packet type to a response before sending it as-is
        //                            buffer.buffer[origin + 4] = (byte)Packet.ResponseSetServerData;
        //                            buffer.position = origin;

        //                            // Forward the packet to everyone connected to the server
        //                            for (int i = 0; i < mPlayerList.size; ++i) mPlayerList.buffer[i].SendPacket(buffer);
        //                        }
        //                        else
        //                        {
        //                            player.LogError("Tried to set the server data without authorization", null);
        //                            RemovePlayer(player);
        //                        }
        //                        break;
        //                    }
        //                case Packet.RequestKick:
        //                    {
        //                        int channelID = reader.ReadInt32();
        //                        int id = reader.ReadInt32();
        //                        string s = (id != 0) ? null : reader.ReadString();
        //                        NetworkPlayer other = (id != 0) ? GetPlayer(id) : GetPlayer(s);

        //                        if (channelID != -1)
        //                        {
        //                            Channel ch;

        //                            if (mChannelDict.TryGetValue(channelID, out ch) && ch != null && ch.host == player)
        //                            {
        //                                player.Log(player.name + " kicked " + other.name + " (" + other.ConnectionId + ") from channel " + channelID);
        //                                SendLeaveChannel(other, ch, true);
        //                            }
        //                        }
        //                        else if (player.isAdmin)
        //                        {
        //                            player.Log(player.name + " kicked " + other.name + " (" + other.ConnectionId + ")");
        //                            RemovePlayer(other);
        //                        }
        //                        break;
        //                    }
        //                case Packet.RequestBan:
        //                    {
        //                        int id = reader.ReadInt32();
        //                        string s = (id != 0) ? null : reader.ReadString();
        //                        NetworkPlayer other = (id != 0) ? GetPlayer(id) : GetPlayer(s);
        //                        bool playerBan = (other == player && mServerData != null && mServerData.GetChild<bool>("playersCanBan"));

        //                        if (player.isAdmin || playerBan)
        //                        {
        //                            if (other != null)
        //                            {
        //                                Ban(player, other);
        //                            }
        //                            else if (id == 0)
        //                            {
        //                                player.Log("BANNED " + s);
        //                                //string banText = "// [" + s + "] banned by [" + player.name + "]- " + (player.aliases != null &&
        //                                //    player.aliases.size > 0 ? player.aliases.buffer[0] : player.address);
        //                                //AddUnique(mBan, banText);
        //                                AddUnique(mBan, s);
        //                                //SaveBanList();
        //                            }
        //                        }
        //                        else if (other == player)
        //                        {
        //                            // Self-ban
        //                            Ban(player, other);
        //                        }
        //                        else if (!playerBan)
        //                        {
        //                            // Do nothing
        //                        }
        //                        else
        //                        {
        //                            player.LogError("Tried to ban " + (other != null ? other.name : s) + " without authorization", null);
        //                            RemovePlayer(player);
        //                        }
        //                        break;
        //                    }
        //                case Packet.RequestGetFileList:
        //                    {
        //                        //var original = reader.ReadString();
        //                        //var path = Tools.FindDirectory(original, player.isAdmin);

        //                        //var writer = player.BeginSend(Packet.ResponseGetFileList);
        //                        //writer.Write(original);

        //                        //if (!string.IsNullOrEmpty(path))
        //                        //{
        //                        //    var files = Tools.GetFiles(path);
        //                        //    writer.Write(files.Length);
        //                        //    for (int i = 0, imax = files.Length; i < imax; ++i)
        //                        //        writer.Write(files[i]);
        //                        //}
        //                        //else writer.Write(0);

        //                        //player.EndSend();
        //                        break;
        //                    }
        //                case Packet.RequestLockChannel:
        //                    {
        //                        int channelID = reader.ReadInt32();
        //                        Channel ch = null;
        //                        mChannelDict.TryGetValue(channelID, out ch);
        //                        bool locked = reader.ReadBoolean();

        //                        if (ch != null)
        //                        {
        //                            if (!ch.isPersistent || player.isAdmin)
        //                            {
        //                                ch.isLocked = locked;
        //                                var b = Buffer.Create();
        //                                WriteChannelDescriptor(ch, b);
        //                                ch.SendToAll(b);
        //                            }
        //                            else
        //                            {
        //                                player.LogError("RequestLockChannel(" + ch.id + ", " + locked + ") without authorization", null);
        //                                RemovePlayer(player);
        //                            }
        //                        }
        //                        break;
        //                    }
        //                case Packet.RequestHTTPGet:
        //                    {
        //                        if (player.stage == NetworkPlayer.Stage.WebBrowser)
        //                        {
        //                            // string requestText = reader.ReadString();
        //                            // Example of an HTTP request:
        //                            // GET / HTTP/1.1
        //                            // Host: 127.0.0.1:5127
        //                            // Connection: keep-alive
        //                            // User-Agent: Chrome/47.0.2526.80

        //                            StringBuilder sb = new StringBuilder();

        //                            // Server name
        //                            sb.Append("Name: ");
        //                            sb.AppendLine(name);

        //                            // Number of connected clients
        //                            sb.Append("Clients: ");
        //                            sb.AppendLine(playerCount.ToString());

        //                            // Detailed list of clients
        //                            for (int i = 0, count = 0; i < mPlayerList.size; ++i)
        //                            {
        //                                if (mPlayerList.buffer[i].stage == NetworkPlayer.Stage.Connected)
        //                                {
        //                                    sb.Append(++count);
        //                                    sb.Append(" ");
        //                                    sb.AppendLine(mPlayerList.buffer[i].name);
        //                                }
        //                            }

        //                            // Create the header indicating that the connection should be severed after receiving the data
        //                            string text = sb.ToString();
        //                            sb = new StringBuilder();
        //                            sb.AppendLine("HTTP/1.1 200 OK");
        //                            sb.AppendLine("Server: GNetServer 3");
        //                            sb.AppendLine("Content-Length: " + Encoding.UTF8.GetByteCount(text));
        //                            sb.AppendLine("Content-Type: text/plain; charset=utf-8");
        //                            sb.AppendLine("Connection: Closed\n");
        //                            sb.Append(text);

        //                            // Send the response
        //                            mBuffer = Buffer.Create();
        //                            BinaryWriter bw = mBuffer.BeginWriting(false);
        //                            bw.Write(Encoding.ASCII.GetBytes(sb.ToString()));
        //                            player.SendPacket(mBuffer);
        //                            mBuffer.Recycle();
        //                            mBuffer = null;
        //                        }
        //                        break;
        //                    }
        //                case Packet.RequestRenameServer:
        //                    {
        //                        name = reader.ReadString();
        //                        break;
        //                    }
        //                case Packet.RequestSetOwner:
        //                    {
        //                        var channelID = reader.ReadInt32();
        //                        var objID = reader.ReadUInt32();
        //                        var playerID = reader.ReadInt32();

        //                        Channel ch;

        //                        if (mChannelDict.TryGetValue(channelID, out ch) && ch != null && ch.ChangeObjectOwner(objID, playerID))
        //                        {
        //                            var writer = BeginSend(Packet.ResponseSetOwner);
        //                            writer.Write(channelID);
        //                            writer.Write(objID);
        //                            writer.Write(playerID);
        //                            EndSend(ch, null);
        //                        }
        //                        break;
        //                    }
        //                case Packet.RequestExport:
        //                    {
        //                        var requestID = reader.ReadInt32();
        //                        int count = reader.ReadInt32();

        //                        if (count > 0)
        //                        {
        //                            var temp = Buffer.Create();
        //                            var tempWriter = temp.BeginWriting();
        //                            tempWriter.Write(0);
        //                            int actual = 0;

        //                            for (int i = 0; i < count; ++i)
        //                            {
        //                                Channel ch;
        //                                var channelID = reader.ReadInt32();
        //                                var objID = reader.ReadUInt32();
        //                                if (!mChannelDict.TryGetValue(channelID, out ch) || ch == null) continue;

        //                                if (ch.ExportObject(objID, tempWriter)) ++actual;
        //#if UNITY_EDITOR
        //                                else Debug.LogWarning("Unable to export " + channelID + " " + objID);
        //#endif
        //                            }

        //                            var end = temp.position;
        //                            tempWriter.Seek(0, SeekOrigin.Begin);
        //                            tempWriter.Write(actual);
        //                            tempWriter.Seek(end, SeekOrigin.Begin);

        //                            temp.BeginReading();
        //                            var writer = player.BeginSend(Packet.ResponseExport);
        //                            writer.Write(requestID);
        //                            writer.Write(temp.size);
        //                            if (temp.size > 0) writer.Write(temp.buffer, 0, temp.size);
        //                            player.EndSend();
        //                            temp.Recycle();
        //                        }
        //                        break;
        //                    }
        //                case Packet.RequestImport:
        //                    {
        //                        var requestID = reader.ReadInt32();
        //                        var channelID = reader.ReadInt32();
        //                        int count = reader.ReadInt32();

        //                        if (count > 0)
        //                        {
        //                            bool isNew;
        //                            var ch = CreateChannel(channelID, out isNew);
        //                            var writer = player.BeginSend(Packet.ResponseImport);
        //                            writer.Write(requestID);
        //                            writer.Write(channelID);
        //                            writer.Write(count);
        //                            for (int i = 0; i < count; ++i) writer.Write(ch.ImportObject(player.id, reader));
        //                            player.EndSend();
        //                        }
        //                        break;
        //                    }
        //                case Packet.RequestValidate:
        //                    {
        //                        var propName = reader.ReadString();
        //                        var propValue = reader.ReadObject();

        //                        // Admins don't have to validate anything
        //                        if (player.isAdmin) break;

        //                        DataNode existing = null;

        //                        if (mServerData != null)
        //                        {
        //                            existing = mServerData.GetChild(propName);

        //                            // If the value matches, there is nothing that needs to be done
        //                            if (existing == null || existing.value == null) { if (propValue == null) break; }
        //                            else if (existing.value.Equals(propValue)) break;
        //                        }

        //                        // This point being reached means the values don't match
        //                        player.LogError("RequestValidate fail: " + propName + " = " + (existing != null && existing.value != null ? existing.value.ToString() : "null") + ", not " + propValue, null);
        //                        RemovePlayer(player);
        //                        return false;
        //                    }
        //                default:
        //                    {
        //                        if (player.channels.size != 0 && requestByte < (int)Packet.UserPacket)
        //                        {
        //                            // Other packets can only be processed while in a channel
        //                            if (request >= Packet.ForwardToAll && request < Packet.ForwardToPlayer)
        //                            {
        //                                ProcessForwardPacket(player, buffer, reader, request);
        //                            }
        //                            else
        //                            {
        //                                ProcessChannelPacket(player, buffer, reader, request);
        //                            }
        //                        }
        //                        else if (onCustomPacket != null)
        //                        {
        //                            onCustomPacket(player, buffer, reader, requestByte);
        //                        }
        //                        break;
        //                    }
        //            }
        //            return true;
        //        }

#endif

        /// <summary>
        /// Set an alias and check it against the ban list.
        /// </summary>

        protected bool SetAlias(ServerPlayer player, string s)
        {
#if !MODDING
            if (mBan.Contains(s))
            {
                Console.WriteLine("FAILED a ban check: " + player.id + ", " + s);
                RemovePlayer(player);
                return false;
            }
            else
            {
                // Steam ID validation
                if (s.Length == 17 && s.StartsWith("7656"))
                {
                    long sid;

                    if (long.TryParse(s, out sid))
                    {
                        if (sid > 76561199999999999)
                        {
                            Console.WriteLine("FAILED a ban check: " + player.id + ", " + s);
                            return false;
                        }
                    }
                }
#if STANDALONE
				Console.WriteLine("Passed a ban check: " + s);
#endif
                player.AddAlias(s);
                return true;
            }
#else
			return false;
#endif
        }

        /// <summary>
        /// Ban the specified player.
        /// </summary>

        protected void Ban(ServerPlayer requestingPlayer, ServerPlayer bannedPlayer)
        {
#if !MODDING
            var info = "BANNED " + bannedPlayer.name + " (" + (bannedPlayer.aliases != null &&
                    bannedPlayer.aliases.size > 0 ? bannedPlayer.aliases.buffer[0] : bannedPlayer.ConnectionId) + ")";
            Console.WriteLine(info);
            // Just to show the name of the player
            string banText = "// [" + bannedPlayer.name + "]";

            if (requestingPlayer != null && requestingPlayer != bannedPlayer)
            {
                banText += " banned by [" + requestingPlayer.name + "]- " + (bannedPlayer.aliases != null &&
                    requestingPlayer.aliases.size > 0 ? requestingPlayer.aliases.buffer[0] : requestingPlayer.ConnectionId);
            }
            else banText += " banned by the server";

            AddUnique(mBan, banText);
            //AddUnique(mBan, bannedPlayer.tcpEndPoint.Address.ToString());

            if (bannedPlayer.aliases != null)
                for (int i = 0; i < bannedPlayer.aliases.size; ++i)
                    AddUnique(mBan, bannedPlayer.aliases.buffer[i]);

            RemovePlayer(bannedPlayer);
            //SaveBanList();
#endif
        }

#if !MODDING
        /// <summary>
        /// Process a packet that's meant to be forwarded.
        /// </summary>

        //protected void ProcessForwardPacket(NetworkPlayer player, Buffer buffer, BinaryReader reader, Packet request)
        //{
        //    // 4 bytes for packet size, 1 byte for packet ID
        //    int origin = buffer.position - 5;
        //    int playerID = reader.ReadInt32();
        //    int channelID = reader.ReadInt32();

        //    // Exploit: echoed packet of another player
        //    if (playerID != player.id)
        //    {
        //        player.LogError("Tried to echo a " + request + " packet (" + playerID + " vs " + player.id + ")", null);
        //        RemovePlayer(player);
        //        return;
        //    }

        //    // The channel must exist
        //    Channel ch;
        //    mChannelDict.TryGetValue(channelID, out ch);
        //    if (ch == null) return;

        //    if (request == Packet.ForwardToHost)
        //    {
        //        var host = (NetworkPlayer)ch.host;
        //        if (host == null) return;
        //        buffer.position = origin;
        //        host.SendPacket(buffer);
        //    }
        //    else
        //    {
        //        // We want to exclude the player if the request was to forward to others
        //        

        //        buffer.position = origin;

        //        // Forward the packet to everyone except the sender
        //        for (int i = 0; i < ch.players.size; ++i)
        //        {
        //            var tp = (NetworkPlayer)ch.players.buffer[i];

        //            if (tp != exclude)
        //            {
        //                tp.SendPacket(buffer);
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// Process a packet from the player.
        /// </summary>

        //protected void ProcessChannelPacket(NetworkPlayer player, Buffer buffer, BinaryReader reader, Packet request)
        //{
        //    switch (request)
        //    {

        //        
        //        case Packet.RequestLoadLevel:
        //            {
        //                var ch = player.GetChannel(reader.ReadInt32());
        //                var lvl = reader.ReadString();

        //                // Change the currently loaded level
        //                if (ch.host == player && ch != null && (!ch.isLocked || player.isAdmin))
        //                {
        //                    ch.Reset();
        //                    ch.level = lvl;

        //                    var writer = BeginSend(Packet.ResponseLoadLevel);
        //                    writer.Write(ch.id);
        //                    writer.Write(string.IsNullOrEmpty(ch.level) ? "" : ch.level);
        //                    EndSend(ch, null);
        //                }
        //                break;
        //            }
        //        case Packet.RequestSetHost:
        //            {
        //                var ch = player.GetChannel(reader.ReadInt32());
        //                var pid = reader.ReadInt32();

        //                // Transfer the host state from one player to another
        //                if (ch != null && ch.host == player)
        //                {
        //                    NetworkPlayer newHost = GetPlayer(pid);
        //                    if (newHost != null && newHost.IsInChannel(ch.id))
        //                        SendSetHost(ch, newHost);
        //                }
        //                break;
        //            }
        //        case Packet.RequestLeaveChannel:
        //            {
        //                var ch = player.GetChannel(reader.ReadInt32());
        //                if (ch != null) SendLeaveChannel(player, ch, true);
        //                break;
        //            }
        //        case Packet.RequestCloseChannel:
        //            {
        //                Channel ch;
        //                var id = reader.ReadInt32();

        //                if (mChannelDict.TryGetValue(id, out ch) && ch != null && !ch.isClosed)
        //                {
        //                    if (!ch.isPersistent && ch.players.Contains(player))
        //                    {
        //                        ch.isClosed = true;
        //                        var b = Buffer.Create();
        //                        WriteChannelDescriptor(ch, b);
        //                        ch.SendToAll(b);
        //                    }
        //                    else if (player.isAdmin)
        //                    {
        //                        player.Log("Closing channel " + ch.id);
        //                        ch.isPersistent = false;
        //                        ch.isClosed = true;

        //                        var b = Buffer.Create();
        //                        WriteChannelDescriptor(ch, b);
        //                        ch.SendToAll(b);
        //                    }
        //                    else
        //                    {
        //                        player.LogError("Tried to call a close channel " + ch.id + " while not authorized", null);
        //                        RemovePlayer(player);
        //                    }
        //                }
        //                break;
        //            }
        //        case Packet.RequestDeleteChannel:
        //            {
        //                int id = reader.ReadInt32();
        //                bool dc = reader.ReadBoolean();

        //                if (player.isAdmin)
        //                {
        //                    Channel ch;

        //                    if (mChannelDict.TryGetValue(id, out ch))
        //                    {
        //                        for (int b = ch.players.size; b > 0;)
        //                        {
        //                            var p = (NetworkPlayer)ch.players.buffer[--b];

        //                            if (p != null)
        //                            {
        //                                if (dc) RemovePlayer(p);
        //                                else SendLeaveChannel(p, ch, true);
        //                            }
        //                        }

        //                        ch.isPersistent = false;
        //                        ch.isClosed = true;
        //                        ch.Reset();

        //                        mChannelDict.Remove(id);
        //                        mChannelList.Remove(ch);
        //                    }
        //                }
        //                else
        //                {
        //                    player.LogError("Tried to call a delete a channel #" + id + " while not authorized", null);
        //                    RemovePlayer(player);
        //                }
        //                break;
        //            }
        //        case Packet.RequestSetPlayerLimit:
        //            {
        //                Channel ch;
        //                mChannelDict.TryGetValue(reader.ReadInt32(), out ch);
        //                ushort limit = reader.ReadUInt16();

        //                if (ch != null)
        //                {
        //                    if (player.isAdmin || mServerData == null ||
        //                        (ch.host == player && (!ch.isPersistent || mServerData.GetChild<bool>("hostCanSetPlayerLimit", true))))
        //                    {
        //                        ch.playerLimit = limit;
        //                        var b = Buffer.Create();
        //                        WriteChannelDescriptor(ch, b);
        //                        ch.SendToAll(b);
        //                    }
        //                }
        //                break;
        //            }
        //        case Packet.RequestRemoveRFC:
        //            {
        //                var ch = player.GetChannel(reader.ReadInt32());
        //                var id = reader.ReadUInt32();
        //                var funcName = ((id & 0xFF) == 0) ? reader.ReadString() : null;
        //                if (ch != null && (!ch.isLocked || player.isAdmin)) ch.DeleteRFC(id, funcName, mTime);
        //                break;
        //            }
        //        case Packet.RequestSetChannelData:
        //            {
        //                // 4 bytes for the size, 1 byte for the ID
        //                int origin = buffer.position - 5;

        //                bool isNew;
        //                var ch = CreateChannel(reader.ReadInt32(), out isNew);

        //                if (ch != null && (!ch.isLocked || player.isAdmin))
        //                {
        //                    if (ch.players.size == 0) ch.isPersistent = true;
        //                    if (ch.dataNode == null) ch.dataNode = new DataNode("Version", Player.version);

        //                    // Set the local data
        //                    ch.dataNode.SetHierarchy(reader.ReadString(), reader.ReadObject());

        //                    // Change the packet type to a response before sending it as-is
        //                    buffer.buffer[origin + 4] = (byte)Packet.ResponseSetChannelData;
        //                    buffer.position = origin;

        //                    // Forward the packet to everyone in this channel
        //                    for (int i = 0; i < ch.players.size; ++i)
        //                    {
        //                        var tp = ch.players.buffer[i] as NetworkPlayer;
        //                        if (tp != null) tp.SendPacket(buffer);
        //                    }
        //                }
        //                break;
        //            }
        //    }
        //}
#endif

#if !UNITY_WEBPLAYER && !UNITY_FLASH
        // Cached to reduce memory allocation
        protected MemoryStream mWriteStream = null;
        protected BinaryWriter mWriter = null;
        protected bool mWriting = false;
#endif
        /// <summary>
        /// Save the server's current state into the file that was loaded previously with Load().
        /// </summary>

        public void Save()
        {
#if !MODDING
            mNextSave = 0;

#if !UNITY_WEBPLAYER && !UNITY_FLASH
            if (mWriting || !isActive || string.IsNullOrEmpty(mFilename)) return;
            if (mServerData != null && !mServerData.GetChild<bool>("save", true)) return;
#if STANDALONE
			var timer = System.Diagnostics.Stopwatch.StartNew();
#endif
            mWriting = true;
            //SaveBanList();
            //SaveAdminList();

            if (mWriteStream == null)
            {
                mWriteStream = new MemoryStream();
                mWriter = new BinaryWriter(mWriteStream);
            }
            else
            {
                mWriter.Seek(0, SeekOrigin.Begin);
                mWriteStream.SetLength(0);
            }

            long length = 0;

            lock (mLock)
            {
                mWriter.Write(0);
                int count = 0;

                for (int i = 0; i < mChannelList.size; ++i)
                {
                    var ch = mChannelList.buffer[i];

                    if (!ch.isClosed && ch.isPersistent && ch.hasData)
                    {
                        mWriter.Write(ch.id);
                        ch.SaveTo(mWriter);
                        ++count;
                    }
                }

                if (count > 0)
                {
                    length = mWriteStream.Position;
                    mWriteStream.Seek(0, SeekOrigin.Begin);
                    mWriter.Write(count);
                }
            }

            //Tools.WriteFile(mFilename, mWriteStream);

            // Save the server configuration data
            if (mServerDataChanged && mServerData != null && !string.IsNullOrEmpty(mFilename))
            {
                mServerDataChanged = false;
                //try { mServerData.Write(mFilename + ".config", DataNode.SaveType.Text, true); }
                //catch (Exception) { }
            }

            // Save the player data
            for (int i = 0; i < mPlayerList.size; ++i) SavePlayer(mPlayerList.buffer[i]);

#if STANDALONE
			var elapsed = timer.ElapsedMilliseconds;
			Console.WriteLine("Saving took " + elapsed + " ms");
#endif
            mWriting = false;
#endif
#endif
        }

        /// <summary>
        /// Save this player's data.
        /// </summary>

        protected void SavePlayer(ServerPlayer player)
        {
#if !MODDING
            if (player == null || !player.saveNeeded || string.IsNullOrEmpty(player.savePath)) return;
            player.saveNeeded = false;

            if (player.dataNode == null || player.dataNode.children.size == 0)
            {
                //if (DeleteFile(player.savePath))
                //    player.Log("Deleted " + player.savePath);
            }
            else
            {
                // Automatically keep track of how long the player has been playing
                var server = player.dataNode.GetChild("Server", true);
                var played = server.GetChild("playedTime", true);
                var save = server.GetChild("lastSave", true);
                var elapsed = mTime - save.Get<long>(mTime);

                // Update the values
                played.value = played.Get<long>() + elapsed;
                save.value = mTime;

                // Save to file
                byte[] bytes = player.dataNode.ToArray(player.saveType);
                //if (!SaveFile(player.savePath, bytes)) player.LogError("Unable to save " + player.savePath);
            }
#endif
        }

        /// <summary>
        /// Load the server's human-readable data.
        /// </summary>

        public void LoadConfig()
        {
#if !MODDING
            if (!string.IsNullOrEmpty(mFilename))
            {
                try
                {
                    //var data = Tools.ReadFile(mFilename + ".config");
                    //mServerData = DataNode.Read(data, DataNode.SaveType.Text);
                }
                catch (Exception) { mServerData = null; }
                mServerDataChanged = false;
            }
#endif
        }

        //[System.Obsolete("Use Load() instead")]
        //public bool LoadFrom(string fileName) { return Load(fileName); }

        /// <summary>
        /// Load a previously saved server from the specified file.
        /// </summary>

        //        public bool Load(string fileName)
        //        {
        //            if (!string.IsNullOrEmpty(rootDirectory)) fileName = Path.Combine(rootDirectory, fileName);

        //            mFilename = fileName;
        //            mNextSave = 0;

        //#if UNITY_WEBPLAYER || UNITY_FLASH || MODDING
        //			// There is no file access in the web player.
        //			return false;
        //#else
        //            LoadConfig();

        //            var bytes = Tools.ReadFile(fileName);
        //            if (bytes == null) return false;

        //            var stream = new MemoryStream(bytes);

        //            lock (mLock)
        //            {
        //                try
        //                {
        //                    var reader = new BinaryReader(stream);

        //                    int channels = reader.ReadInt32();

        //                    for (int i = 0; i < channels; ++i)
        //                    {
        //                        int chID = reader.ReadInt32();
        //                        bool isNew;
        //                        var ch = CreateChannel(chID, out isNew);
        //                        if (isNew) ch.LoadFrom(reader, false);
        //                    }
        //                }
        //                catch (System.Exception ex)
        //                {
        //                    Tools.LogError("Loading from " + fileName + ": " + ex.Message, ex.StackTrace);
        //                    return false;
        //                }
        //            }

        //            GNetServer.Buffer.ReleaseUnusedMemory();
        //            return true;
        //#endif
        //        }

        /// <summary>
        /// Add the specified keyword to the ban list.
        /// </summary>

        public override void Ban(string keyword)
        {
            var other = GetPlayerByName(keyword);
            if (other != null) Ban(null, other);
            else base.Ban(keyword);
        }

        /// <summary>
        /// Add this keyword to the admin list.
        /// </summary>

        public void AddAdmin(string keyword) { if (!mAdmin.Contains(keyword)) mAdmin.Add(keyword); }

        /// <summary>
        /// Remove this keyword from the admin list.
        /// </summary>

        public void RemoveAdmin(string keyword) { mAdmin.Remove(keyword); }

        /// <summary>
        /// Kick the specified player from the server.
        /// </summary>

        public void KickByConnectionId(string connectionId)
        {
            var player = GetPlayerByConnectionId(connectionId);
            if (player != null)
            {
                RemovePlayer(player);
            }
        }
    }
}
