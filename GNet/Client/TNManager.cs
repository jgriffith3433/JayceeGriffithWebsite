//-------------------------------------------------
//                    GNet 3
// Copyright © 2012-2018 Tasharen Entertainment Inc
//-------------------------------------------------

//#define COUNT_PACKETS

// If you want to see exceptions instead of error messages, comment this out
#define SAFE_EXCEPTIONS

using System.IO;
using UnityEngine;
using System.Reflection;
using UnityTools = GNet.UnityTools;
using System;
using System.Collections;

namespace GNet
{
    /// <summary>
    /// Tasharen Network Manager tailored for Unity.
    /// </summary>

    public class GNetConfig
    {
#if DEBUG
        public static string HubUrl = "http://localhost:5000/hub/game";
#else
        public static string HubUrl = "https://jaycee.margravesenclave.com/hub/game";
        //public static string HubUrl = "https://jayceegriffith.azurewebsites.net/";
#endif
    }

    public class TNManager : MonoBehaviour
    {
        // Will be 'true' at play time unless the application is shutting down. 'false' at edit time.
        static bool isPlaying { get { return !mDestroyed && Application.isPlaying; } }
        [System.NonSerialized] static bool mDestroyed = false;
        [System.NonSerialized] static bool mShuttingDown = false;

        #region Delegates
        /// <summary>
        /// Ping notification.
        /// </summary>

        static public TNEvents.OnPing onPing { get { return isPlaying ? instance.mClient.onPing : null; } set { if (isPlaying) instance.mClient.onPing = value; } }

        /// <summary>
        /// Error notification.
        /// </summary>

        static public TNEvents.OnError onError { get { return isPlaying ? instance.mClient.onError : null; } set { if (isPlaying) instance.mClient.onError = value; } }

        /// <summary>
        /// Connected to hub
        /// </summary>

        static public TNEvents.OnConnectedToHubDel onConnectedToHub { get { return isPlaying ? instance.mClient.onConnectedToHub : null; } set { if (isPlaying) instance.mClient.onConnectedToHub = value; } }

        /// <summary>
        /// Notification sent after the connection terminates for any reason.
        /// </summary>

        static public TNEvents.OnDisconnectedFromHubDel onDisconnectedFromHub { get { return isPlaying ? instance.mClient.onDisconnectedFromHub : null; } set { if (isPlaying) instance.mClient.onDisconnectedFromHub = value; } }

        /// <summary>
        /// Connected to game server
        /// </summary>

        static public TNEvents.OnConnectedToGameServerDel onConnectedToGameServer { get { return isPlaying ? instance.mClient.onConnectedToGameServer : null; } set { if (isPlaying) instance.mClient.onConnectedToGameServer = value; } }

        /// <summary>
        /// Notification sent after the connection terminates for any reason.
        /// </summary>

        static public TNEvents.OnDisconnectedFromGameServerDel onDisconnectedFromGameServer { get { return isPlaying ? instance.mClient.onDisconnectedFromGameServer : null; } set { if (isPlaying) instance.mClient.onDisconnectedFromGameServer = value; } }

        /// <summary>
        /// Notification sent when receiving a new ResponseNewServerIdPacket
        /// </summary>

        static public Action<ResponseNewGameServerPacket> onReceiveResponseNewGameServerPacket { get { return isPlaying ? instance.mClient.onReceiveResponseNewGameServerPacket : null; } set { if (isPlaying) instance.mClient.onReceiveResponseNewGameServerPacket = value; } }

        /// <summary>
        /// Notification sent when attempting to join a channel, indicating a success or failure.
        /// </summary>

        static public TNEvents.OnJoinChannel onJoinChannel { get { return isPlaying ? instance.mClient.onJoinChannel : null; } set { if (isPlaying) instance.mClient.onJoinChannel = value; } }

        /// <summary>
        /// Notification sent when the channel gets updated.
        /// </summary>

        static public TNEvents.OnUpdateChannel onUpdateChannel { get { return isPlaying ? instance.mClient.onUpdateChannel : null; } set { if (isPlaying) instance.mClient.onUpdateChannel = value; } }

        /// <summary>
        /// Notification sent when leaving a channel for any reason, including being disconnected.
        /// </summary>

        static public TNEvents.OnLeaveChannel onLeaveChannel { get { return isPlaying ? instance.mClient.onLeaveChannel : null; } set { if (isPlaying) instance.mClient.onLeaveChannel = value; } }

        /// <summary>
        /// Notification sent when changing levels.
        /// </summary>

        static public TNEvents.OnLoadLevel onLoadLevel { get { return isPlaying ? instance.mClient.onLoadLevel : null; } set { if (isPlaying) instance.mClient.onLoadLevel = value; } }

        /// <summary>
        /// Notification sent when a new player joins the channel.
        /// </summary>

        static public TNEvents.OnPlayerJoin onPlayerJoin { get { return isPlaying ? instance.mClient.onPlayerJoin : null; } set { if (isPlaying) instance.mClient.onPlayerJoin = value; } }

        /// <summary>
        /// Notification sent when a player leaves the channel.
        /// </summary>

        static public TNEvents.OnPlayerLeave onPlayerLeave { get { return isPlaying ? instance.mClient.onPlayerLeave : null; } set { if (isPlaying) instance.mClient.onPlayerLeave = value; } }

        /// <summary>
        /// Notification of some player changing their name.
        /// </summary>

        static public TNEvents.OnRenamePlayer onRenamePlayer { get { return isPlaying ? instance.mClient.onRenamePlayer : null; } set { if (isPlaying) instance.mClient.onRenamePlayer = value; } }

        /// <summary>
        /// Notification sent when the channel's host changes.
        /// </summary>

        static public TNEvents.OnHostChanged onHostChanged { get { return isPlaying ? instance.mClient.onHostChanged : null; } set { if (isPlaying) instance.mClient.onHostChanged = value; } }

        /// <summary>
        /// Notification sent when the server's data gets changed.
        /// </summary>

        static public TNEvents.OnSetServerData onSetServerData { get { return isPlaying ? instance.mClient.onSetServerData : null; } set { if (isPlaying) instance.mClient.onSetServerData = value; } }

        /// <summary>
        /// Notification sent when the channel's data gets changed.
        /// </summary>

        static public TNEvents.OnSetChannelData onSetChannelData { get { return isPlaying ? instance.mClient.onSetChannelData : null; } set { if (isPlaying) instance.mClient.onSetChannelData = value; } }

        /// <summary>
        /// Notification sent when player data gets changed.
        /// </summary>

        static public TNEvents.OnSetPlayerData onSetPlayerData { get { return isPlaying ? instance.mClient.onSetPlayerData : null; } set { if (isPlaying) instance.mClient.onSetPlayerData = value; } }

        /// <summary>
        /// Callback triggered when the player gets verified as an administrator.
        /// </summary>

        static public TNEvents.OnSetAdmin onSetAdmin { get { return isPlaying ? instance.mClient.onSetAdmin : null; } set { if (isPlaying) instance.mClient.onSetAdmin = value; } }

        /// <summary>
        /// Callback triggered when a chat message arrives.
        /// </summary>

        static public TNEvents.OnChatPacket onChat { get { return isPlaying ? instance.mClient.onChat : null; } set { if (isPlaying) instance.mClient.onChat = value; } }
        #endregion

        /// <summary>
        /// Whether the application is currently paused.
        /// </summary>

        static public bool isPaused = false;

        /// <summary>
        /// If set to 'true', the list of custom creation functions will be rebuilt the next time it's accessed.
        /// </summary>

        static public bool rebuildMethodList = true;

        // Cached list of creation functions
        static System.Collections.Generic.Dictionary<int, CachedFunc> mDict0 = new System.Collections.Generic.Dictionary<int, CachedFunc>();
        static System.Collections.Generic.Dictionary<string, CachedFunc> mDict1 = new System.Collections.Generic.Dictionary<string, CachedFunc>();

        // Player list that will contain only the player in it. Here for the same reason as 'mPlayer'.
        static GList<Player> mPlayers;

        // Instance pointer
        static TNManager mInstance;

        public delegate void UpdateDelegate();
        static GList<UpdateDelegate> m_UpdateDelegates = new GList<UpdateDelegate>();

        /// <summary>
        /// Object owner is only valid during object creation. In most cases you will want to use tno.owner.
        /// </summary>

        static internal Player currentObjectOwner = null;

        /// <summary>
        /// List of objects that can be instantiated by the network.
        /// </summary>

        public GameObject[] objects;

        // Network client
#if UNITY_WEBGL
        [System.NonSerialized] GameClient mClient = new GameClient();
#else
        [System.NonSerialized] GameClient mClient = new GameClient();
#endif
        [System.NonSerialized] GList<int> mLoadingLevel = new GList<int>();


        static public void RegisterUpdate(UpdateDelegate updateDelegate)
        {
            m_UpdateDelegates.Add(updateDelegate);
        }

        static public void UnregisterUpdate(UpdateDelegate updateDelegate)
        {
            if (m_UpdateDelegates.Contains(updateDelegate))
            {
                m_UpdateDelegates.Remove(updateDelegate);
            }
        }

        /// <summary>
        /// Whether the player has verified himself as an administrator.
        /// </summary>

        static public bool isAdmin { get { return (mInstance == null || !mInstance.mClient.isConnectedToGameServer || mInstance.mClient.isAdmin); } }

        /// <summary>
        /// Set administrator privileges. Note that failing the password test will cause a disconnect.
        /// </summary>

        static public void SetAdmin(string passKey) { if (mInstance) mInstance.mClient.SetAdmin(passKey); }

        /// <summary>
        /// Add a new pass key to the admin file.
        /// </summary>

        static public void AddAdmin(string passKey)
        {
#if !MODDING
            if (isAdmin)
            {
                BeginSend(Packet.RequestCreateAdmin).Write(passKey);
                EndSend();
            }
#endif
        }

        /// <summary>
        /// Remove the specified pass key from the admin file.
        /// </summary>

        static public void RemoveAdmin(string passKey)
        {
#if !MODDING
            if (isAdmin)
            {
                BeginSend(Packet.RequestRemoveAdmin).Write(passKey);
                EndSend();
            }
#endif
        }

        /// <summary>
        /// Set a player alias. Player aliases can be used to store useful player-associated data such as Steam usernames,
        /// database IDs, or other unique identifiers. Aliases will show up in GNet's log and can also be banned by on
        /// the server side. When a player is banned, all their aliases are banned as well, so be careful to make sure
        /// that they are indeed unique. All aliases are visible via GNet.Player.aliases list of each player.
        /// </summary>

        static public void SetAlias(string alias)
        {
#if !MODDING
            if (mInstance)
            {
                BeginSend(Packet.RequestSetAlias).Write(alias);
                EndSend();
            }
#endif
        }

        /// <summary>
        /// GNet Client used for communication.
        /// </summary>

        static public GameClient client
        {
            get
            {
                return mInstance != null ? mInstance.mClient : (!mDestroyed && !mShuttingDown ? instance.mClient : null);
            }
        }

        /// <summary>
        /// Whether we're currently connected to the hub.
        /// </summary>

        static public bool isConnectedToHub { get { return mInstance != null && mInstance.mClient.isConnectedToHub; } }

        static public bool isConnectedToGameServer { get { return mInstance != null && mInstance.mClient.isConnectedToGameServer; } }


        /// <summary>
        /// Immediately reset the packet count. Calling this isn't necessary as they get updated once per second anyway.
        /// </summary>

        static public void ResetPacketCount() { if (mInstance != null) mInstance.mClient.ResetPacketCount(); }

        /// <summary>
        /// Whether we are currently in the process of joining a channel.
        /// To find out whether we are joining a specific channel, use the "IsJoiningChannel(id)" function.
        /// </summary>

        static public bool isJoiningChannel { get { return IsJoiningChannel(-1); } }

        /// <summary>
        /// Whether we are currently trying to join the specified channel.
        /// </summary>

        static public IsJoiningChannelFunc IsJoiningChannel = IsJoiningChannelDefault;

        /// <summary>
        /// Default function to check whether the GNet is currently joining a channel.
        /// </summary>

        static public bool IsJoiningChannelDefault(int channelID)
        {
            if (!isConnectedToGameServer) return false;

            if (channelID < 0)
            {
                return (mInstance.mLoadingLevel.size != 0 || mInstance.mClient.isJoiningChannel);
            }
            return mInstance.mLoadingLevel.Contains(channelID) || mInstance.mClient.IsJoiningChannel(channelID);
        }

        public delegate bool IsJoiningChannelFunc(int channelID);

        /// <summary>
        /// Whether we are currently trying to establish a new connection to the hub.
        /// </summary>

        static public bool isTryingToConnectToHub { get { return mInstance != null && mInstance.mClient.isTryingToConnectToHub; } }

        /// <summary>
        /// Whether we are currently trying to establish a new connection to the game server.
        /// </summary>

        static public bool isTringToConnectToGameServer { get { return mInstance != null && mInstance.mClient.isTryingToConnectToGameServer; } }

        /// <summary>
        /// Whether we're currently hosting. Note that this should only be used if the player is in only one channel.
        /// </summary>

        [System.Obsolete("Use IsHosting(channelID)")]
        static public bool isHosting
        {
            get
            {
#if UNITY_EDITOR
                if (channels.size > 1)
                {
                    Debug.LogWarning("Use TNManager.IsHosting(channelID) instead, as the player is in more than one channel.\nUndefined results will occur if you keep using TNManager.isHosting.");
                }
#endif
                return GetHost(lastChannelID) == player;
            }
        }

        /// <summary>
        /// Whether the player is hosting this channel.
        /// </summary>

        static public bool IsHosting(int channelID) { return GetHost(channelID) == player; }

        /// <summary>
        /// Whether we're currently in any channel. To find out if we are in a specific channel, use TNManager.IsInChannel(id).
        /// </summary>

        static public bool isInChannel
        {
            get
            {
                return !isJoiningChannel && mInstance.mClient.isInChannel;
            }
        }

        /// <summary>
        /// You can pause TNManager's message processing if you like.
        /// This happens automatically when a scene is being loaded.
        /// </summary>

        static public bool isActive
        {
            get
            {
                return mInstance != null && mInstance.mClient.isActive;
            }
            set
            {
                if (mInstance != null) mInstance.mClient.isActive = value;
            }
        }

        /// <summary>
        /// Current ping to the server.
        /// </summary>

        static public int ping { get { return mInstance != null ? mInstance.mClient.ping : 0; } }


        /// <summary>
        /// Current time on the server in milliseconds.
        /// </summary>

        static public long serverTime { get { return (mInstance != null) ? mInstance.mClient.serverTime : (System.DateTime.UtcNow.Ticks / 10000); } }

        /// <summary>
        /// Server's uptime in milliseconds.
        /// </summary>

        static public long serverUptime { get { return (mInstance != null) ? mInstance.mClient.serverUptime : (System.DateTime.UtcNow.Ticks / 10000) - mStartTime; } }
        static readonly long mStartTime = (System.DateTime.UtcNow.Ticks / 10000);

        /// <summary>
        /// Time elapsed either since the server has been started up in seconds, or (if not connected) -- time since TNManager was first used.
        /// It's a more precise version of Unity's Time.time, since it takes a mere 3 hours for float precision to start dropping milliseconds.
        /// </summary>

        static public double time { get { return serverUptime * 0.001; } }

        /// <summary>
        /// Player's /played time in seconds. This value is automatically tracked by the server and is saved as a part of the player file.
        /// </summary>

        static public long playedTime
        {
            get
            {
                if (player == null || player.dataNode == null) return 0;
                var server = player.dataNode.GetChild("Server");
                if (server == null) return 0;

                var time = serverTime;
                var played = server.GetChild<long>("playedTime", 0);
                var save = server.GetChild<long>("lastSave", time);
                var elapsed = time - save;
                return (elapsed + played) / 1000;
            }
        }

        /// <summary>
        /// Whether the specified channel is currently locked.
        /// </summary>

        static public bool IsChannelLocked(int channelID)
        {
            if (mInstance != null && mInstance.mClient != null)
            {
                var ch = mInstance.mClient.GetChannel(channelID);
                return (ch != null && ch.isLocked);
            }
            return false;
        }

        /// <summary>
        /// Whether the specified channel is closed.
        /// </summary>

        static public bool IsChannelClosed(int channelID)
        {
            if (mInstance != null && mInstance.mClient != null)
            {
                var ch = mInstance.mClient.GetChannel(channelID);
                return (ch != null && ch.isClosed);
            }
            return false;
        }

        /// <summary>
        /// ID of the channel the player is in. This is not a reliable way of retrieving a channel, and is only meant
        /// to be used from inside RCC functions when the object's channel is required.
        /// </summary>

        [System.NonSerialized]
        static public int lastChannelID = 0;

        /// <summary>
        /// List of channels the player is currently in.
        /// </summary>

        static public GList<Channel> channels
        {
            get
            {
                return mInstance.mClient.channels;
            }
        }

        /// <summary>
        /// Check to see if we are currently in the specified channel.
        /// </summary>

        static public bool IsInChannel(int channelID, bool isNotLeaving = false)
        {
            return mInstance.mClient.IsInChannel(channelID);
        }

        /// <summary>
        /// Check to see if the specified player is present in the chosen channel.
        /// </summary>

        static public bool IsPlayerInChannel(Player p, int channelID)
        {
            if (IsInChannel(channelID))
            {
                if (p == player) return true;
                var channel = GetChannel(channelID);
                for (int i = 0; i < channel.players.size; ++i) if (channel.players.buffer[i] == p) return true;
            }
            return false;
        }

        /// <summary>
        /// Check to see if the specified player is present in the chosen channel.
        /// </summary>

        static public bool IsPlayerInChannel(int playerID, int channelID)
        {
            if (IsInChannel(channelID))
            {
                if (playerID == TNManager.playerID) return true;
                var channel = GetChannel(channelID);
                for (int i = 0; i < channel.players.size; ++i) if (channel.players.buffer[i].id == playerID) return true;
            }
            return false;
        }

        /// <summary>
        /// Get the player hosting the specified channel. Only works for the channels the player is in.
        /// </summary>

        static public Player GetHost(int channelID)
        {
            if (!isConnectedToGameServer) return player;
            return mInstance.mClient.GetHost(channelID);
        }

        /// <summary>
        /// The player's unique identifier.
        /// </summary>

        static public int playerID { get { return mInstance.mClient.playerID; } }

        /// <summary>
        /// Get or set the player's name as everyone sees him on the network.
        /// </summary>

        static public string playerName
        {
            get
            {
                return mInstance.mClient.playerName;
            }
            set
            {
                mInstance.mClient.playerName = value;
            }
        }

        /// <summary>
        /// Get or set the player's data, synchronizing it with the server.
        /// To change the data, use TNManager.SetPlayerData instead of changing the content directly.
        /// </summary>

        static public DataNode playerData
        {
            get
            {
                return mInstance.mClient.playerData;
            }
            set
            {
                if (isConnectedToGameServer) mInstance.mClient.SetPlayerData("", value);
            }
        }

        /// <summary>
        /// List of other players in the same channel as the client. This list does not include TNManager.player.
        /// </summary>

        [Obsolete("Use TNManager.GetPlayers(channelID)")]
        static public GList<Player> players { get { return GetPlayers(lastChannelID); } }

        /// <summary>
        /// Get a list of players under the specified channel.
        /// This will only work for channels the player has joined.
        /// The returned list will not include TNManager.player.
        /// </summary>

        static public GList<Player> GetPlayers(int channelID)
        {
            if (isConnectedToGameServer)
            {
                Channel ch = mInstance.mClient.GetChannel(channelID);
                if (ch != null) return ch.players;
            }

            if (mPlayers == null) mPlayers = new GList<Player>();
            return mPlayers;
        }

        /// <summary>
        /// Get the network player.
        /// </summary>

        static public Player player { get { return mInstance.mClient.player; } }

        /// <summary>
        /// Ensure that we have a TNManager to work with.
        /// </summary>

        public static TNManager instance
        {
            get
            {
#if UNITY_EDITOR
                if (!Application.isPlaying) return mInstance;
#endif
                if (mInstance == null)
                {
                    if (mShuttingDown) return null;
                    GameObject go = new GameObject("Network Manager");
                    mInstance = go.AddComponent<TNManager>();
                    mDestroyed = false;
                }
                return mInstance;
            }
        }

        /// <summary>
        /// Server configuration is set by administrators.
        /// In most cases you should use GetServerData and SetServerData functions instead.
        /// </summary>

        static public DataNode serverData
        {
            get
            {
                return ((mInstance != null) ? mInstance.mClient.serverData : null) ?? mDummyNode;
            }
            set
            {
                if (mInstance != null)
                    mInstance.mClient.serverData = value;
            }
        }

        static DataNode mDummyNode = new DataNode("Version", Player.version);

        /// <summary>
        /// Retrieve the specified server option.
        /// </summary>

        static public DataNode GetServerData(string key) { return (mInstance != null) ? mInstance.mClient.GetServerData(key) : null; }

        /// <summary>
        /// Retrieve the specified server option.
        /// </summary>

        static public T GetServerData<T>(string key) { return (mInstance != null) ? mInstance.mClient.GetServerData<T>(key) : default(T); }

        /// <summary>
        /// Retrieve the specified server option.
        /// </summary>

        static public T GetServerData<T>(string key, T def) { return (mInstance != null) ? mInstance.mClient.GetServerData<T>(key, def) : def; }

        /// <summary>
        /// Set the specified server option using key = value notation.
        /// </summary>

        static public void SetServerData(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                string[] parts = text.Split(new char[] { '=' }, 2);

                if (parts.Length == 2)
                {
                    string key = parts[0].Trim();
                    string val = parts[1].Trim();
                    DataNode node = new DataNode(key, val);
                    if (node.ResolveValue()) SetServerData(node.name, node.value);
                }
                else Debug.LogWarning("Invalid syntax [" + text + "]. Expected [key = value].");
            }
        }

        /// <summary>
        /// Set the specified server option.
        /// </summary>

        static public void SetServerData(DataNode node) { if (mInstance != null && isAdmin) mInstance.mClient.SetServerData(node); }

        /// <summary>
        /// Set the specified server option.
        /// </summary>

        static public void SetServerData(string key, object val) { if (mInstance != null && isAdmin) mInstance.mClient.SetServerData(key, val); }

        /// <summary>
        /// Return a channel with the specified ID. This will only work as long as the player is in this channel.
        /// </summary>

        static public Channel GetChannel(int channelID) { return isConnectedToGameServer ? mInstance.mClient.GetChannel(channelID, false) : null; }

        /// <summary>
        /// Convenience method: Retrieve the specified channel option.
        /// </summary>

        [System.Obsolete("Use GetChannelData(channelID, key) instead")]
        static public DataNode GetChannelData(string key)
        {
            if (!isConnectedToGameServer) return null;
            var ch = GetChannel(lastChannelID);
            return (ch != null) ? ch.Get(key) : null;
        }

        /// <summary>
        /// Convenience method: Retrieve the specified channel option.
        /// </summary>

        [System.Obsolete("Use GetChannelData(channelID, key) instead")]
        static public T GetChannelData<T>(string key)
        {
            if (!isConnectedToGameServer) return default(T);
            var ch = GetChannel(lastChannelID);
            return (ch != null) ? ch.Get<T>(key) : default(T);
        }

        /// <summary>
        /// Convenience method: Retrieve the specified channel option.
        /// </summary>

        [System.Obsolete("Use GetChannelData(channelID, key) instead")]
        static public T GetChannelData<T>(string key, T def)
        {
            if (!isConnectedToGameServer) return def;
            var ch = GetChannel(lastChannelID);
            return (ch != null) ? ch.Get<T>(key) : def;
        }

        /// <summary>
        /// Convenience method: Retrieve the specified channel option.
        /// </summary>

        static public DataNode GetChannelData(int channelID, string key)
        {
            if (!isConnectedToGameServer) return null;
            var ch = GetChannel(channelID);
            return (ch != null) ? ch.Get(key) : null;
        }

        /// <summary>
        /// Convenience method: Retrieve the specified channel option.
        /// </summary>

        static public T GetChannelData<T>(int channelID, string key)
        {
            if (!isConnectedToGameServer) return default(T);
            var ch = GetChannel(channelID);
            return (ch != null) ? ch.Get<T>(key) : default(T);
        }

        /// <summary>
        /// Convenience method: Retrieve the specified channel option.
        /// </summary>

        static public T GetChannelData<T>(int channelID, string key, T def)
        {
            if (!isConnectedToGameServer) return def;
            var ch = GetChannel(channelID);
            return (ch != null) ? ch.Get<T>(key, def) : def;
        }

        /// <summary>
        /// Set the specified channel option.
        /// </summary>

        [System.Obsolete("Use SetChannelData(channelID, key, value) instead")]
        static public void SetChannelData(string key, object val) { if (isConnectedToGameServer && isInChannel) mInstance.mClient.SetChannelData(lastChannelID, key, val); }

        /// <summary>
        /// Set the specified channel option.
        /// </summary>

        static public void SetChannelData(int channelID, string key, object val) { if (isConnectedToGameServer) mInstance.mClient.SetChannelData(channelID, key, val); }

        /// <summary>
        /// Get the player associated with the specified ID.
        /// </summary>

        static public Player GetPlayer(int id)
        {
            if (id == 0) return null;
            if (id == playerID) return player;
            if (isConnectedToGameServer) return mInstance.mClient.GetPlayer(id);
            return null;
        }

        /// <summary>
        /// Get the player associated with the specified name.
        /// </summary>

        static public Player GetPlayer(string name)
        {
            if (name == playerName) return player;
            if (isConnectedToGameServer) return mInstance.mClient.GetPlayer(name);
            return null;
        }

        /// <summary>
        /// Find the player associated with the specified partial name. This function is not case sensitive, so calling it with "Ren" will return "Aren Mook" as a possible choice.
        /// </summary>

        static public Player FindPlayer(string name, int channelID = 0)
        {
            var exact = GetPlayer(name);
            if (exact != null) return exact;

            var list = GetPlayers(channelID);

            for (int i = 0; i < list.size; ++i)
            {
                var p = list.buffer[i];
                if (p.name.IndexOf(name, System.StringComparison.CurrentCultureIgnoreCase) != -1) return p;
            }
            return null;
        }

        /// <summary>
        /// Get our player's data.
        /// </summary>

        static public DataNode GetPlayerData(string path) { return player.Get(path); }

        /// <summary>
        /// Convenience method: Get the specified value from our player.
        /// </summary>

        static public T GetPlayerData<T>(string path) { return player.Get<T>(path); }

        /// <summary>
        /// Convenience method: Get the specified value from our player.
        /// </summary>

        static public T GetPlayerData<T>(string path, T defaultVal) { return player.Get<T>(path, defaultVal); }

        /// <summary>
        /// Set the specified value on our player.
        /// </summary>

        static public void SetPlayerData(string path, object val)
        {
            if (isConnectedToGameServer)
            {
                mInstance.mClient.SetPlayerData(path, val);
            }
            else if (val != null)
            {
                // This forces the Serialize/Deserialize to be called, same as playing online would.
                // This ensures the same behaviour, with the Deserialize always called, online and offline.
                if (val is IBinarySerializable)
                {
                    var o = (val as IBinarySerializable);
                    var buffer = Buffer.Create();
                    o.Serialize(buffer.BeginWriting());
                    o = (IBinarySerializable)val.GetType().Create();
                    o.Deserialize(buffer.BeginReading());
                    player.Set(path, o);
                    buffer.Recycle();
                }
                else if (val is IDataNodeSerializable)
                {
                    var o = (val as IDataNodeSerializable);
                    var dn = new DataNode("DN");
                    o.Serialize(dn);
                    o = (IDataNodeSerializable)val.GetType().Create();
                    o.Deserialize(dn);
                    player.Set(path, o);
                }
                else player.Set(path, val);
            }
            else player.Set(path, val);
        }

        /// <summary>
        /// Set the specified value on our player using key = value notation.
        /// </summary>

        static public void SetPlayerData(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                string[] parts = text.Split(new char[] { '=' }, 2);

                if (parts.Length == 2)
                {
                    string key = parts[0].Trim();
                    string val = parts[1].Trim();
                    DataNode node = new DataNode(key, val);
                    if (node.ResolveValue()) SetPlayerData(node.name, node.value);
                }
                else Debug.LogWarning("Invalid syntax [" + text + "]. Expected [key = value].");
            }
        }

        /// <summary>
        /// Get a list of channels from the server.
        /// </summary>

        static public void GetChannelList(GameClient.OnGetChannels callback) { if (mInstance != null) mInstance.mClient.GetChannelList(callback); }

        /// <summary>
        /// Set the following function to handle this type of packets.
        /// </summary>

        static public void SetPacketHandler(byte packetID, GameClient.OnPacket callback)
        {
            if (!mShuttingDown && !mDestroyed && Application.isPlaying)
                instance.mClient.packetHandlers[packetID] = callback;
        }

        /// <summary>
        /// Set the following function to handle this type of packets.
        /// </summary>

        static public void SetPacketHandler(Packet packet, GameClient.OnPacket callback)
        {
            if (!mShuttingDown && !mDestroyed && Application.isPlaying)
                instance.mClient.packetHandlers[(byte)packet] = callback;
        }

        /// <summary>
        /// Connect to the hub
        /// </summary>

        static public void ConnectToHub(string hubUrl, string gamerTag)
        {
#if !MODDING
            //TODO: Probably make this always true so browsers throttle less
            Application.runInBackground = true;
            mInstance.mClient.ConnectToHub(hubUrl ?? GNetConfig.HubUrl, gamerTag);
#endif
        }

        /// <summary>
        /// Start disconnecting from the hub
        /// </summary>

        static public void StartDisconnectingFromHub()
        {
            if (mInstance != null)
            {
                mInstance.mClient.StartDisconnectingFromHub();
            }
        }

        /// <summary>
        /// Start disconnecting from the hub after a specified delay in seconds.
        /// </summary>

        static public void StartDisconnectingFromHubAfterTime(float delay)
        {
            if (mInstance != null)
            {
                mInstance.Invoke("StartDisconnectingFromHub", delay);
            }
        }

        /// <summary>
        /// Log an error, complete with a stack trace, and disconnect from the hub
        /// </summary>

        static public void DisconnectFromHubWithException(string text)
        {
            try
            {
                throw new Exception(text);
            }
            catch (Exception ex)
            {
                Log("ERROR: " + ex.Message + "\n" + ex.StackTrace);
                StartDisconnectingFromHubAfterTime(1f);
            }
        }



        /// <summary>
        /// Connect to the game server
        /// </summary>

        static public void ConnectToGameServer(string gameServerId)
        {
#if !MODDING
            mInstance.mClient.ConnectToGameServer(gameServerId);
#endif
        }

        /// <summary>
        /// Start disconnecting from the game server
        /// </summary>

        static public void StartDisconnectingFromGameServer()
        {
            if (mInstance != null)
            {
                mInstance.mClient.StartDisconnectingFromGameServer();
            }
        }

        /// <summary>
        /// Start disconnecting from the game server after a specified delay in seconds.
        /// </summary>

        static public void StartDisconnectingFromGameServerAfterTime(float delay)
        {
            if (mInstance != null)
            {
                mInstance.Invoke("StartDisconnectingFromGameServer", delay);
            }
        }

        /// <summary>
        /// Log an error, complete with a stack trace, and disconnect from the hub
        /// </summary>

        static public void DisconnectFromGameServerWithException(string text)
        {
            try
            {
                throw new Exception(text);
            }
            catch (Exception ex)
            {
                Log("ERROR: " + ex.Message + "\n" + ex.StackTrace);
                StartDisconnectingFromGameServerAfterTime(1f);
            }
        }

        static public void RequestNewGameServer()
        {
            mInstance.mClient.player.SendPacket(new RequestNewGameServerPacket(GameServer.GameId, "New Game Server", 0));
        }

        static public void RequestStopGameServer(string gameServerId)
        {
            mInstance.mClient.player.SendPacket(new RequestStopGameServerPacket(gameServerId));
        }

        /// <summary>
        /// Join the specified channel.
        /// </summary>
        /// <param name="channelID">ID of the channel. Every player joining this channel will see one another.</param>
        /// <param name="persistent">Whether the channel will remain active even when the last player leaves.</param>

        static public void JoinChannel(int channelID, bool persistent = false, bool leaveCurrentChannel = false)
        {
            JoinChannel(channelID, null, persistent, 65535, null, leaveCurrentChannel);
        }

        /// <summary>
        /// Load the chosen scene. This delegate is called when it's time to load the specified scene. Don't try to call it yourself. Use TNManager.LoadLevel instead.
        /// </summary>

        static public LoadSceneFunc onLoadScene = delegate (string levelName)
        {
            if (!string.IsNullOrEmpty(levelName))
            {
#if UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
				Application.LoadLevel(levelName);
#else
                UnityEngine.SceneManagement.SceneManager.LoadScene(levelName);
#endif
            }
        };

        /// <summary>
        /// Load the chosen scene asynchronously. This delegate is called when it's time to load the specified scene. Don't try to call it yourself. Use TNManager.LoadLevel instead.
        /// </summary>

        static public LoadSceneAsyncFunc onLoadSceneAsync = delegate (string levelName)
        {
            if (!string.IsNullOrEmpty(levelName))
            {
#if UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
				return Application.LoadLevelAsync(levelName);
#else
                return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(levelName);
#endif
            }
            return null;
        };

        public delegate void LoadSceneFunc(string levelName);
        public delegate AsyncOperation LoadSceneAsyncFunc(string levelName);

        /// <summary>
        /// Join the specified channel. This channel will be marked as persistent, meaning it will
        /// stay open even when the last player leaves, unless explicitly closed first.
        /// </summary>
        /// <param name="channelID">ID of the channel. Every player joining this channel will see one another.</param>
        /// <param name="levelName">Level that will be loaded first.</param>

        static public void JoinChannel(int channelID, string levelName, bool leaveCurrentChannel = true)
        {
            if (!IsInChannel(channelID))
            {
                if (leaveCurrentChannel) LeaveAllChannels();
                mInstance.mClient.JoinChannel(channelID, levelName, false, 65535, null);
            }
        }

        /// <summary>
        /// Join the specified channel.
        /// </summary>
        /// <param name="channelID">ID of the channel. Every player joining this channel will see one another.</param>
        /// <param name="levelName">Level that will be loaded first.</param>
        /// <param name="persistent">Whether the channel will remain active even when the last player leaves.</param>
        /// <param name="playerLimit">Maximum number of players that can be in this channel at once.</param>
        /// <param name="password">Password for the channel. First player sets the password.</param>

        static public void JoinChannel(int channelID, string levelName, bool persistent, int playerLimit, string password, bool leaveCurrentChannel = true)
        {
            if (!IsInChannel(channelID))
            {
                if (leaveCurrentChannel) LeaveAllChannels();
                mInstance.mClient.JoinChannel(channelID, levelName, persistent, playerLimit, password);
            }
        }

        /// <summary>
        /// Join a random open game channel or create a new one. Guaranteed to load the specified level.
        /// </summary>
        /// <param name="levelName">Level that will be loaded first.</param>
        /// <param name="persistent">Whether the channel will remain active even when the last player leaves.</param>
        /// <param name="playerLimit">Maximum number of players that can be in this channel at once.</param>
        /// <param name="password">Password for the channel. First player sets the password.</param>

        static public void JoinRandomChannel(string levelName, bool persistent, int playerLimit, string password, bool leaveCurrentChannel = true)
        {
            if (leaveCurrentChannel) LeaveAllChannels();
            if (isConnectedToGameServer) mInstance.mClient.JoinChannel(-2, levelName, persistent, playerLimit, password);
            else JoinChannel(UnityEngine.Random.Range(1000, 100000), levelName, persistent, playerLimit, password, false);
        }

        /// <summary>
        /// Create a new channel.
        /// </summary>
        /// <param name="levelName">Level that will be loaded first.</param>
        /// <param name="persistent">Whether the channel will remain active even when the last player leaves.</param>
        /// <param name="playerLimit">Maximum number of players that can be in this channel at once.</param>
        /// <param name="password">Password for the channel. First player sets the password.</param>

        static public void CreateChannel(string levelName, bool persistent, int playerLimit, string password, bool leaveCurrentChannel = true)
        {
            if (leaveCurrentChannel) LeaveAllChannels();
            if (isConnectedToGameServer) mInstance.mClient.JoinChannel(-1, levelName, persistent, playerLimit, password);
            else JoinChannel(UnityEngine.Random.Range(1000, 100000), levelName, persistent, playerLimit, password, false);
        }

        /// <summary>
        /// GNet 3.0 onwards makes it possible to join more than one channel at once.
        /// When the player is in more than one channel, commands need to specify which channel they are directed towards.
        /// </summary>

        [System.Diagnostics.DebuggerHidden]
        [System.Diagnostics.DebuggerStepThrough]
        static void ChannelCheck()
        {
            if (channels.size > 1)
            {
                Debug.LogWarning("Currently in more than one channel! Specify which channel you want to work with.");
            }
        }

        /// <summary>
        /// Close the channel the player is in. New players will be prevented from joining.
        /// Once a channel has been closed, it cannot be re-opened.
        /// </summary>

        [System.Obsolete("Use CloseChannel(id)")]
        static public void CloseChannel() { CloseChannel(lastChannelID); }

        /// <summary>
        /// Close the channel the player is in. New players will be prevented from joining.
        /// Once a channel has been closed, it cannot be re-opened.
        /// </summary>

        static public void CloseChannel(int channelID) { if (mInstance != null) mInstance.mClient.CloseChannel(channelID); }

        /// <summary>
        /// Leave all of the channels we're currently in.
        /// </summary>

        static public void LeaveAllChannels()
        {
            if (mInstance != null) mInstance.mClient.LeaveAllChannels();
        }

        /// <summary>
        /// Leave the channel we're in.
        /// </summary>

        static public void LeaveChannel() { LeaveChannel(lastChannelID); }

        /// <summary>
        /// Leave the channel we're in.
        /// </summary>

        static public void LeaveChannel(int channelID)
        {
            mInstance.mClient.LeaveChannel(channelID);
        }

        /// <summary>
        /// Delete the specified channel.
        /// </summary>

        static public void DeleteChannel(int id, bool disconnect) { if (mInstance != null) mInstance.mClient.DeleteChannel(id, disconnect); }

        /// <summary>
        /// Change the maximum number of players that can join the channel the player is currently in.
        /// </summary>

        [System.Obsolete("Use SetPlayerLimit(channel, max)")]
        static public void SetPlayerLimit(int max) { SetPlayerLimit(lastChannelID, max); }

        /// <summary>
        /// Change the maximum number of players that can join the channel the player is currently in.
        /// </summary>

        static public void SetPlayerLimit(int channelID, int max) { if (mInstance != null) mInstance.mClient.SetPlayerLimit(channelID, max); }

        [System.Obsolete("Use TNManager.LoadScene(channel, name) instead")]
        static public void LoadLevel(string levelName) { LoadScene(lastChannelID, levelName); }

        [System.Obsolete("Function renamed to TNManager.LoadScene(channel, name) for clarity")]
        static public void LoadLevel(int channelID, string levelName) { LoadScene(channelID, levelName); }

        /// <summary>
        /// Load the specified scene on all clients.
        /// </summary>

        static public void LoadScene(int channelID, string levelName) { if (!mInstance.mClient.LoadLevel(channelID, levelName)) onLoadScene(levelName); }

        /// <summary>
        /// Save the specified file on the server.
        /// </summary>

        //static public void SaveFile (string filename, byte[] data)
        //{
        //	if (isConnected)
        //	{
        //		mInstance.mClient.SaveFile(filename, data);
        //	}
        //	else
        //	{
        //		try
        //		{
        //			Tools.WriteFile(filename, data);
        //		}
        //		catch (System.Exception ex)
        //		{
        //			Debug.LogError(ex.Message + " (" + filename + ")");
        //		}
        //	}
        //}

        ///// <summary>
        ///// Load the specified file residing on the server.
        ///// </summary>

        //static public void LoadFile (string filename, GameClient.OnLoadFile callback)
        //{
        //	if (callback != null)
        //	{
        //		if (isConnected)
        //		{
        //			mInstance.mClient.LoadFile(filename, callback);
        //		}
        //		else callback(filename, Tools.ReadFile(filename));
        //	}
        //}

        /// <summary>
        /// Specify where the player's data should be saved. You only need to call this function once and GNet
        /// will automatically save the player file for you every time you use TNManager.SetPlayerData afterwards.
        /// </summary>

        //		static public void SetPlayerSave (string filename, DataNode.SaveType type = DataNode.SaveType.Binary, int hash = 0)
        //		{
        //#if !MODDING
        //			if (isConnected)
        //			{
        //				var writer = BeginSend(Packet.RequestSetPlayerSave);
        //				writer.Write(filename);
        //				writer.Write((byte)type);
        //				writer.Write(hash);
        //				EndSend();
        //			}
        //			else
        //			{
        //				playerData = DataNode.Read(filename);
        //				if (onSetPlayerData != null) onSetPlayerData(player, "", playerData);
        //			}
        //#endif
        //		}

        //		/// <summary>
        //		/// Delete the specified file on the server.
        //		/// </summary>

        //		static public void DeleteFile (string filename)
        //		{
        //#if !MODDING
        //			if (isConnected)
        //			{
        //				mInstance.mClient.DeleteFile(filename);
        //			}
        //			else
        //			{
        //				try
        //				{
        //					Tools.DeleteFile(filename);
        //				}
        //				catch (System.Exception ex)
        //				{
        //					Debug.LogError(ex.Message + " (" + filename + ")");
        //				}
        //			}
        //#endif
        //		}

        /// <summary>
        /// Change the hosting player.
        /// </summary>

        static public void SetHost(Player player) { SetHost(lastChannelID, player); }

        /// <summary>
        /// Change the hosting player.
        /// </summary>

        static public void SetHost(int channelID, Player player)
        {
            if (mInstance != null) mInstance.mClient.SetHost(channelID, player);
        }

        /// <summary>
        /// Set the timeout for the player. By default it's 10 seconds. If you know you are about to load a large level,
        /// and it's going to take, say 60 seconds, set this timeout to 120 seconds just to be safe. When the level
        /// finishes loading, change this back to 10 seconds so that dropped connections gets detected correctly.
        /// </summary>

        static public void SetTimeout(int seconds)
        {
            if (mInstance != null) mInstance.mClient.SetTimeout(seconds);
        }

        /// <summary>
        /// Lock the channel the player is currently in.
        /// </summary>

        [System.Obsolete("Use LockChannel(id, locked)")]
        static public void LockChannel(bool locked) { LockChannel(lastChannelID, locked); }

        /// <summary>
        /// Lock the specified channel, preventing all future persistent RFCs from being saved.
        /// </summary>

        static public void LockChannel(int channelID, bool locked)
        {
#if !MODDING
            if (mInstance != null && isAdmin)
            {
                var writer = BeginSend(Packet.RequestLockChannel);
                writer.Write(channelID);
                writer.Write(locked);
                EndSend(channelID);
            }
#endif
        }

        [System.Obsolete("You need to specify the channel ID as the first parameter")]
        static public void Instantiate(int rccID, string path, bool persistent, params object[] objs)
        {
            Instantiate(lastChannelID, rccID, null, path, persistent, objs);
        }

        [System.Obsolete("You need to specify the channel ID as the first parameter")]
        static public void Instantiate(string funcName, string path, bool persistent, params object[] objs)
        {
            Instantiate(lastChannelID, 0, funcName, path, persistent, objs);
        }

        /// <summary>
        /// Create a packet that will send a custom object creation call.
        /// Instantiate a new game object in the specified channel on all connected players.
        /// </summary>

        static public void Instantiate(int channelID, int rccID, string path, bool persistent, params object[] objs)
        {
            Instantiate(channelID, rccID, null, path, persistent, objs);
        }

        /// <summary>
        /// Create a packet that will send a custom object creation call.
        /// Instantiate a new game object in the specified channel on all connected players.
        /// </summary>

        static public void Instantiate(int channelID, string funcName, string path, bool persistent, params object[] objs)
        {
            Instantiate(channelID, 0, funcName, path, persistent, objs);
        }

        /// <summary>
        /// TNObject's ID, set right before calling the RCC function -- just in case you want to use it as a random seed.
        /// </summary>

        [System.NonSerialized]
        static public uint currentRccObjectID = 0;

        /// <summary>
        /// Create a packet that will send a custom object creation call.
        /// Instantiate a new game object in the specified channel on all connected players.
        /// </summary>

        static internal void Instantiate(int channelID, int rccID, string funcName, string path, bool persistent, params object[] objs)
        {
            if (path == null) path = "";

            lastChannelID = channelID;
            var go = UnityTools.LoadPrefab(path) ?? UnityTools.GetDummyObject();

            if (go != null && instance != null)
            {
                var func = GetRCC(rccID, funcName);

                if (func == null)
                {
                    Debug.LogError("Unable to locate RCC " + rccID + " " + funcName);
                }
#if !MODDING
                else if (isConnectedToGameServer)
                {
                    if (IsJoiningChannel(channelID))
                    {
#if UNITY_EDITOR
                        Debug.LogWarning("Trying to create an object while switching scenes. Call will be ignored.");
#endif
                        return;
                    }

                    if (IsChannelLocked(channelID))
                    {
#if UNITY_EDITOR
                        Debug.LogWarning("Trying to create an object in a locked channel. Call will be ignored.");
#endif
                        return;
                    }

                    if (!IsInChannel(channelID))
                    {
#if UNITY_EDITOR
                        Debug.LogWarning("Must join the channel first before calling instantiating objects.");
#endif
                        return;
                    }
                    var ms = new MemoryStream();
                    using (var bw = new BinaryWriter(ms))
                    {
                        if (rccID > 0 && rccID < 256)
                        {
                            bw.Write((byte)rccID);
                        }
                        else
                        {
                            bw.Write((byte)0);
                            bw.Write(funcName);
                        }
                        bw.Write(path);
                        bw.WriteArray(objs);
                    }
                    mInstance.mClient.SendPacket(new RequestCreateObject(channelID, playerID, persistent, ms.GetBuffer()));
                }
#endif
                else
                {
                    // Offline mode
                    currentRccObjectID = TNObject.GetUniqueID(true);
                    objs = BinaryExtensions.CombineArrays(go, objs);
                    go = func.Execute(objs) as GameObject;
                    UnityTools.Clear(objs);

                    if (go != null)
                    {
                        var tno = go.GetComponent<TNObject>();
                        if (tno == null) tno = go.AddComponent<TNObject>();
                        tno.uid = currentRccObjectID;
                        tno.channelID = channelID;
                        go.SetActive(true);
                        tno.Register();
                    }
                }
            }
#if UNITY_EDITOR
            else Debug.LogError("Unable to load " + path);
#endif
        }

        /// <summary>
        /// Get the specified RCC.
        /// </summary>

        static public CachedFunc GetRCC(int rccID, string funcName)
        {
            CachedFunc func = null;

            if (rccID > 0 && rccID < 256 && !mDict0.TryGetValue(rccID, out func))
            {
                CacheRFCs();

                if (!mDict0.TryGetValue(rccID, out func))
                {
                    mDict0[rccID] = null;
#if UNITY_EDITOR
                    Debug.LogError("RCC(" + rccID + ")  was not found");
#endif
                }
            }

            if (func == null)
            {
                if (funcName != null)
                {
                    if (!mDict1.TryGetValue(funcName, out func))
                    {
                        CacheRFCs();

                        if (!mDict1.TryGetValue(funcName, out func))
                        {
                            mDict1[funcName] = null;
#if UNITY_EDITOR
                            Debug.LogError("RCC(" + funcName + ") was not found");
#endif
                        }
                    }
                }
            }
            return func;
        }

        /// <summary>
        /// Automatically find and cache RFCs on all known MonoBehaviours.
        /// </summary>

        static void CacheRFCs()
        {
            var mb = typeof(MonoBehaviour);
            var types = TypeExtensions.GetTypes();

            for (int i = 0; i < types.size; ++i)
            {
                var t = types.buffer[i];
                if (t.type.IsSubclassOf(mb)) AddRCCs(t.type);
            }
        }

        /// <summary>
        /// Add a new Remote Creation Call.
        /// </summary>

        static public void AddRCCs<T>() { AddRCCs(typeof(T)); }

        /// <summary>
        /// Add a new Remote Creation Call.
        /// </summary>

        static public void AddRCCs(Type type)
        {
            var cache = type.GetCache().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

            for (int b = 0, bmax = cache.Count; b < bmax; ++b)
            {
                var ci = cache.buffer[b];

                if (ci.method.IsDefined(typeof(RCC), true))
                {
                    RCC tnc = (RCC)ci.method.GetCustomAttributes(typeof(RCC), true)[0];

                    if (ci.method.ReturnType == typeof(GameObject))
                    {
                        CachedFunc ent = new CachedFunc();
                        ent.mi = ci.method;

                        if (tnc.id > 0 && tnc.id < 256) mDict0[tnc.id] = ent;
                        else mDict1[ci.name] = ent;
                    }
                    else Debug.LogError("RCC(" + tnc.id + ") function [" + ci.name + "] must return an instantiated GameObject");
                }
            }
        }

        /// <summary>
        /// Built-in Remote Creation Call.
        /// </summary>

        [RCC(1)]
        static GameObject OnCreate1(GameObject go)
        {
            go = Instantiate(go) as GameObject;
            go.SetActive(true);
            return go;
        }

        /// <summary>
        /// Built-in Remote Creation Call.
        /// </summary>

        [RCC(2)]
        static GameObject OnCreate2(GameObject go, Vector3 pos, Quaternion rot)
        {
            go = Instantiate(go, pos, rot) as GameObject;
            go.SetActive(true);
            return go;
        }

        /// <summary>
        /// Built-in Remote Creation Call.
        /// </summary>

        [RCC(3)]
        static GameObject OnCreate3(GameObject go, Vector3 pos, Quaternion rot, Vector3 velocity, Vector3 angularVelocity)
        {
            go = UnityTools.Instantiate(go, pos, rot, velocity, angularVelocity);
            go.SetActive(true);
            return go;
        }

        /// <summary>
        /// Write a server log entry.
        /// </summary>

        static public void Log(string text)
        {
#if UNITY_EDITOR
            Debug.Log(text);
#endif
            if (isConnectedToGameServer)
            {
                BeginSend(Packet.ServerLog).Write(text);
                EndSend();
            }
        }

        static public void SendPacket(CommandPacket commandPacket)
        {
            instance.mClient.SendPacket(commandPacket);
        }

        /// <summary>
        /// Begin sending a new packet to the server.
        /// </summary>

        static public BinaryWriter BeginSend(Packet type) { return instance.mClient.BeginSend(type); }

        /// <summary>
        /// Send the outgoing buffer. This should only be used for generic packets going straight to the server.
        /// Packets that are going to a channel should use EndSend(channelID, reliable) function instead.
        /// </summary>

        static public void EndSend() { mInstance.mClient.EndSend(); }

        /// <summary>
        /// Send the outgoing buffer.
        /// </summary>

        static public void EndSend(int channelID)
        {
            if (!IsJoiningChannel(channelID))
            {
                mInstance.mClient.EndSend(channelID);
            }
            else
            {
                mInstance.mClient.CancelSend();
#if UNITY_EDITOR
                Debug.LogWarning("Trying to send a packet while joining a channel. Ignored.");
#endif
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Close channel")]
        void ForceCloseChannel()
        {
            mInstance.mClient.BeginSend(Packet.RequestCloseChannel).Write(lastChannelID);
            mInstance.mClient.EndSend();
        }
#endif

        /// <summary>
        /// Broadcast the packet to everyone on the LAN.
        /// </summary>

        static public void EndSendToLAN(int port) { mInstance.mClient.EndSend(port); }

        /// <summary>
        /// Write the specified data into a local cache file belonging to connected server.
        /// </summary>

        static public bool WriteCache(string path, byte[] data, bool inMyDocuments = false)
        {
            //if (isConnected)
            //{
            //	IPEndPoint ip = tcpEndPoint;
            //	string addr = (ip != null) ? (ip.Address + ":" + ip.Port) : "127.0.0.1:5127";
            //	int code = addr.GetHashCode();
            //	if (code < 0) code = -code;
            //	return Tools.WriteFile("Temp/" + code + "/" + path, data, inMyDocuments, false);
            //}
            return false;
        }

        /// <summary>
        /// Read the specified file from the cache belonging to the connected server.
        /// </summary>

        static public byte[] ReadCache(string path)
        {
            //if (isConnected)
            //{
            //	IPEndPoint ip = tcpEndPoint;
            //	string addr = (ip != null) ? (ip.Address + ":" + ip.Port) : "127.0.0.1:5127";
            //	int code = addr.GetHashCode();
            //	if (code < 0) code = -code;
            //	return Tools.ReadFile("Temp/" + code + "/" + path);
            //}
            return null;
        }

        /// <summary>
        /// Export the specified objects from the server. The server will return the byte[] necessary to re-instantiate all of the specified objects and restore their state.
        /// </summary>

        static public void ExportObjects(GList<TNObject> list, Action<byte[]> callback) { if (mInstance != null) mInstance.mClient.ExportObjects(list, callback); }

        /// <summary>
        /// Export the specified objects from the server. The server will return the DataNode necessary to re-instantiate all of the specified objects and restore their state.
        /// </summary>

        static public void ExportObjects(GList<TNObject> list, Action<DataNode> callback) { if (mInstance != null) mInstance.mClient.ExportObjects(list, callback); }

        /// <summary>
        /// Import previously exported objects in the specified channel. The optional callback will contain the object IDs of instantiated objects.
        /// The callback will be called only after all objects have been instantiated and their RFCs have been called on all clients.
        /// </summary>

        static public void ImportObjects(int channelID, byte[] data, Action<uint[]> callback = null) { if (mInstance != null) mInstance.mClient.ImportObjects(channelID, data, callback); }

        /// <summary>
        /// Import previously exported objects in the specified channel. The optional callback will contain the object IDs of instantiated objects.
        /// The callback will be called only after all objects have been instantiated and their RFCs have been called on all clients.
        /// </summary>

        static public void ImportObjects(int channelID, Buffer data, Action<uint[]> callback = null) { if (mInstance != null) mInstance.mClient.ImportObjects(channelID, data, callback); }

        /// <summary>
        /// Import previously exported objects in the specified channel. The optional callback will contain the object IDs of instantiated objects.
        /// The callback will be called only after all objects have been instantiated and their RFCs have been called on all clients.
        /// </summary>

        static public void ImportObjects(int channelID, DataNode node, Action<uint[]> callback = null) { if (mInstance != null) mInstance.mClient.ImportObjects(channelID, node, callback); }

        #region MonoBehaviour and helper functions -- it's unlikely that you will need to modify these

        /// <summary>
        /// Ensure that there is only one instance of this class present.
        /// </summary>

        private void Awake()
        {
            if (mInstance != null)
            {
                UnityEngine.Object.Destroy(gameObject);
            }
            else
            {
                mInstance = this;
                rebuildMethodList = true;
                DontDestroyOnLoad(gameObject);
#if FORCE_EN_US
				Tools.SetCurrentCultureToEnUS();
#endif
                AddRCCs<TNManager>();
                SetDefaultCallbacks();

                //#if UNITY_EDITOR
                //				List<IPAddress> ips = GNet.Tools.localAddresses;

                //				if (ips != null && ips.size > 0)
                //				{
                //					var ipv6 = (GNet.Tools.localAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6);
                //					string text = "[GNet] Local IPs: " + ips.size;

                //					for (int i = 0; i < ips.size; ++i)
                //					{
                //						var ip = ips.buffer[i];
                //						text += "\n  " + (i + 1) + ": " + ips.buffer[i];

                //						if (ip == GNet.Tools.localAddress)
                //						{
                //							text += " (LAN)";
                //						}
                //						else if (ipv6 && ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                //						{
                //							if (ip == GNet.Tools.externalAddress)
                //								text += " (WAN)";
                //						}
                //					}
                //					Debug.Log(text);
                //				}
                //#endif
            }
        }

        private void Update()
        {
            foreach(var updateDelegate in m_UpdateDelegates)
            {
                updateDelegate?.Invoke();
            }
        }

        /// <summary>
        /// Set the built-in delegates.
        /// </summary>

        void SetDefaultCallbacks()
        {
            mClient.onDisconnectedFromGameServer = delegate (ClientPlayer clientPlayer) { mLoadingLevel.Clear(); };
            mClient.onJoinChannel = delegate (int channelID, bool success, string message) { lastChannelID = channelID; };
            mClient.onLeaveChannel = delegate (int channelID)
            {
                UnityEngine.Profiling.Profiler.BeginSample("CleanupChannelObjects");
                TNObject.CleanupChannelObjects(channelID);
                UnityEngine.Profiling.Profiler.EndSample();

                if (lastChannelID == channelID)
                {
                    var chs = channels;

                    for (int i = 0; i < chs.size; ++i)
                    {
                        var ch = chs.buffer[i];

                        if (ch.id != lastChannelID)
                        {
                            lastChannelID = ch.id;
                            return;
                        }
                    }
                    lastChannelID = 0;
                }
            };

            mClient.onLoadLevel = delegate (int channelID, string levelName)
            {
                lastChannelID = channelID;
                TNObject.CleanupChannelObjects(channelID);

                if (!string.IsNullOrEmpty(levelName))
                {
                    mLoadingLevel.Add(channelID);
                    StartCoroutine("LoadLevelCoroutine", new System.Collections.Generic.KeyValuePair<int, string>(channelID, levelName));
                }
            };

            mClient.onCreate = OnCreateObject;
            mClient.onDestroy = OnDestroyObject;
            mClient.onTransfer = OnTransferObject;
            mClient.onChangeOwner = OnChangeOwner;
            mClient.onReceiveForwardPacket = OnReceiveForwardPacket;
            mClient.onPlayerLeave = TNObject.OnPlayerLeave;
        }

        /// <summary>
        /// Make sure we disconnect on exit.
        /// </summary>

        void OnDestroy()
        {
            if (mInstance == this)
            {
                mDestroyed = true;
                if (isTryingToConnectToHub) mClient.DisconnectFromHubNow();
                mInstance = null;
            }
        }

        /// <summary>
        /// Find the index of the specified game object.
        /// </summary>

        static public int IndexOf(GameObject go)
        {
            if (go != null && mInstance != null && mInstance.objects != null)
            {
                for (int i = 0, imax = mInstance.objects.Length; i < imax; ++i)
                    if (mInstance.objects[i] == go) return i;

                Debug.LogError("[GNet] The game object was not found in the TNManager's list of objects. Did you forget to add it?", go);
            }
            return -1;
        }

        /// <summary>
        /// Notification of a new object being created.
        /// </summary>

        void OnCreateObject(int channelID, int creator, uint objectID, byte[] objectData)
        {
            var reader = new BinaryReader(new MemoryStream(objectData));
            currentObjectOwner = GetPlayer(creator) ?? GetHost(channelID);

            lastChannelID = channelID;
            var rccID = reader.ReadByte();
            var funcName = (rccID == 0) ? reader.ReadString() : null;
            var func = GetRCC(rccID, funcName);

            // Load the object from the resources folder
            var prefab = reader.ReadString();
            var go = UnityTools.LoadPrefab(prefab);

            if (go == null)
            {
                go = UnityTools.GetDummyObject();

#if UNITY_EDITOR
                if (!string.IsNullOrEmpty(prefab)) Debug.LogError("[GNet] Unable to find prefab \"" + prefab + "\". Make sure it's in the Resources folder.");
#else
                if (!string.IsNullOrEmpty(prefab)) Debug.LogError("[GNet] Unable to find prefab \"" + prefab + "\"");
#endif
            }

            currentRccObjectID = objectID;

            if (func != null)
            {
                // Custom creation function
                var objs = reader.ReadArray(go);
                go = func.Execute(objs) as GameObject;
                UnityTools.Clear(objs);
            }
            // Fallback to a very basic function
            else go = OnCreate1(go);

            if (go != null)
            {
                // Network objects should only be destroyed when leaving their channel
                var t = go.transform;
                if (t.parent == null) DontDestroyOnLoad(go);

                // If an object ID was requested, assign it to the TNObject
                if (objectID != 0)
                {
                    var obj = go.GetComponent<TNObject>();
                    if (obj == null) obj = go.AddComponent<TNObject>();
                    obj.channelID = channelID;
                    obj.uid = objectID;
                    go.SetActive(true);
                    obj.Register();
                }
                else
                {
#if UNITY_EDITOR
                    Debug.LogWarning("Object ID is 0. Intentional?", go);
#endif
                    go.SetActive(true);
                }
            }

            currentObjectOwner = null;
        }

        /// <summary>
        /// Notification of a network object being destroyed.
        /// </summary>

        void OnDestroyObject(int channelID, uint objID)
        {
            var obj = TNObject.Find(channelID, objID);
            if (obj) obj.OnDestroyPacket();
        }

        /// <summary>
        /// Notification of a network object being transferred to another channel.
        /// </summary>

        void OnTransferObject(int oldChannelID, int newChannelID, uint oldObjectID, uint newObjectID)
        {
            if (IsInChannel(oldChannelID))
            {
                var obj = TNObject.Find(oldChannelID, oldObjectID);
                if (obj) obj.FinalizeTransfer(newChannelID, newObjectID);
#if UNITY_EDITOR
                else Debug.LogWarning("Unable to find TNO #" + oldObjectID + " in channel " + oldChannelID);
#endif
            }
        }

        /// <summary>
        /// Notification of the object's owner being changed.
        /// </summary>

        void OnChangeOwner(int channelID, uint objectID, Player p)
        {
            var obj = TNObject.Find(channelID, objectID);
            if (obj != null) obj.OnChangeOwnerPacket(p);
        }

        void OnApplicationQuit() { mShuttingDown = true; }

        /// <summary>
        /// If custom functionality is needed, all unrecognized packets will arrive here.
        /// </summary>

        void OnReceiveForwardPacket(int channelId, uint uid, string functionName, byte[] data)
        {
            uint objID;
            byte funcID;
            TNObject.DecodeUID(uid, out objID, out funcID);
            var binaryReader = new BinaryReader(new MemoryStream(data));
            var arr = binaryReader.ReadArray();

            if (funcID == 0)
            {
#if SAFE_EXCEPTIONS
                try
#endif
                {
                    TNObject.FindAndExecute(channelId, objID, functionName, arr);
                }
#if SAFE_EXCEPTIONS
                catch (Exception ex)
                {
                    Debug.LogError(objID + " " + funcID + " " + functionName + "\n" + ex.Message + "\n" + ex.StackTrace);
                }
#endif
            }
            else TNObject.FindAndExecute(channelId, objID, funcID, arr);
        }

        #endregion

        /// <summary>
        /// Load level coroutine handling asynchronous loading of levels.
        /// </summary>

        System.Collections.IEnumerator LoadLevelCoroutine(System.Collections.Generic.KeyValuePair<int, string> pair)
        {
            yield return null;

            loadLevelOperation = onLoadSceneAsync(pair.Value);
            loadLevelOperation.allowSceneActivation = false;

            while (loadLevelOperation.progress < 0.9f)
                yield return null;

            loadLevelOperation.allowSceneActivation = true;
            yield return loadLevelOperation;

            loadLevelOperation = null;
            mLoadingLevel.Remove(pair.Key);
        }

        /// <summary>
        /// When a level is being loaded, this value will contain the async coroutine for the LoadLevel operation.
        /// You can yield on it if you need.
        /// </summary>

        static public AsyncOperation loadLevelOperation = null;

        void OnApplicationPause(bool paused) { isPaused = paused; }

        /// <summary>
        /// Send a chat message to everyone on the server. If you want custom parameters or options, such as sending packets to a specific group of players,
        /// it's best to make a custom RFC on a persistent "global chat" game object instead.
        /// </summary>

        static public void SendChat(string text) { if (mInstance != null) mInstance.mClient.SendChat(text); }

        [System.Obsolete("Renamed to SendChat")]
        static public void SendGlobalChat(string text) { if (mInstance != null) mInstance.mClient.SendChat(text); }

        /// <summary>
        /// Send a private chat message.
        /// </summary>

        static public void SendPM(Player p, string text) { if (mInstance != null) mInstance.mClient.SendChat(text, p); }

        [System.Obsolete("Use TNManager.playerData")]
        static public DataNode playerDataNode { get { return playerData; } }

        static public int packetSourceId;

        //[Obsolete("Use TNManager.serverData instead")]
        //static public DataNode serverOptions { get { return serverData; } }

        [System.Obsolete("Use TNManager.SetServerData instead")]
        static public void SetServerOption(string text) { SetServerData(text); }

        [Obsolete("Use TNManager.SetServerData instead")]
        static public void SetServerOption(string key, object val) { SetServerData(key, val); }

        [Obsolete("Use TNManager.SetServerData(key, value) instead")]
        static public void SetServerOption(DataNode node) { SetServerData(node.name, node.value); }

        [Obsolete("Use TNManager.SetChannelData(key, value) instead")]
        static public void SetChannelOption(DataNode node) { SetChannelData(node.name, node.value); }

        //[Obsolete("Use TNManager.packetSourceIP or TNManager.packetSourceID instead")]
        //static public IPEndPoint packetSource { get { return (mInstance != null) ? mInstance.mClient.packetSourceIP : null; } }

        [Obsolete("It's now possible to be in more than one channel at once. Use TNManager.IsChannelLocked(channelID) instead.")]
        static public bool isChannelLocked { get { return IsChannelLocked(lastChannelID); } }

        [Obsolete("Use TNManager.GetChannelData(id, data) and TNManager.SetChannelData(id, data) instead")]
        static public string channelData
        {
            get
            {
                Channel ch = GetChannel(lastChannelID);
                return (ch != null) ? ch.Get<string>("channelData") : null;
            }
            set
            {
                SetChannelData("channelData", value);
            }
        }

        [Obsolete("All TNObjects have channel IDs associated with them -- use them instead.")]
        static public int channelID { get { return lastChannelID; } }

        [Obsolete("It's now possible to be in more than one channel at once. Use TNManager.GetHost(channelID) instead.")]
        static public int hostID { get { return GetHost(lastChannelID).id; } }

        [Obsolete("You should create a custom RCC and use TNManager.Instantiate instead of using this function")]
        static internal void Create(string path, bool persistent = true) { Instantiate(lastChannelID, 1, null, path, persistent); }

        [Obsolete("You should create a custom RCC and use TNManager.Instantiate instead of using this function")]
        static internal void Create(string path, Vector3 pos, Quaternion rot, bool persistent = true) { Instantiate(lastChannelID, 2, null, path, persistent, pos, rot); }

        [Obsolete("You should create a custom RCC and use TNManager.Instantiate instead of using this function")]
        static internal void Create(string path, Vector3 pos, Quaternion rot, Vector3 vel, Vector3 angVel, bool persistent = true) { Instantiate(lastChannelID, 3, null, path, persistent, pos, rot, vel, angVel); }

        [Obsolete("You should create a custom RCC and use TNManager.Instantiate instead of using this function")]
        static internal void CreateEx(int rccID, bool persistent, string path, params object[] objs) { Instantiate(lastChannelID, rccID, path, persistent, objs); }

        //		[Obsolete("You need to specify a channel ID to send the packet to: TNManager.EndSend(channelID);")]
        //		static public void EndSend ()
        //		{
        //			if (!IsJoiningChannel(lastChannelID))
        //			{
        //				if (channels.size > 1)
        //					Debug.LogWarning("You need to specify which channel this packet should be going to");
        //				mInstance.mClient.EndSend(lastChannelID);
        //			}
        //			else
        //			{
        //				mInstance.mClient.CancelSend();
        //#if UNITY_EDITOR
        //				Debug.LogWarning("Trying to send a packet while joining a channel. Ignored.");
        //#endif
        //			}
        //		}

        [System.Obsolete("Use TNManager.GetServerData instead")]
        static public DataNode GetServerOption(string key) { return (mInstance != null) ? mInstance.mClient.GetServerData(key) : null; }

        [System.Obsolete("Use TNManager.GetServerData instead")]
        static public T GetServerOption<T>(string key) { return (mInstance != null) ? mInstance.mClient.GetServerData<T>(key) : default(T); }

        [System.Obsolete("Use TNManager.GetServerData instead")]
        static public T GetServerOption<T>(string key, T def) { return (mInstance != null) ? mInstance.mClient.GetServerData<T>(key, def) : def; }

        [System.Obsolete("Use gameObject.DestroySelf() instead")]
        static public void Destroy(GameObject go) { go.DestroySelf(); }
    }
}
