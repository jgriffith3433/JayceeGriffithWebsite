////-------------------------------------------------
////                    GNet 3
//// Copyright © 2012-2018 Tasharen Entertainment Inc
////-------------------------------------------------

//using System;
//using System.Collections;
//using UnityEngine;

//namespace GNet
//{
//    /// <summary>
//    /// The game server cannot communicate directly with a lobby server because that server can be TCP or UDP based,
//    /// and may also be hosted either locally or on another computer. And so we use a different class to "link" them
//    /// together -- the LobbyServerLink. This class will link a game server with a local lobby server.
//    /// </summary>

//    public abstract class LobbyServerLink
//    {
//        long mNextSend = 0;

//        protected GameServer mGameServer;
//        protected Thread mThread;

//        // Thread-safe flag indicating that the server should shut down at the first available opportunity
//        protected bool mShutdown = false;

//        /// <summary>
//        /// Whether the link is currently active.
//        /// </summary>

//        public virtual bool isActive { get { return (!mShutdown && mGameServer != null && mGameServer.GeGNetworkPlayer() != null && mGameServer.GeGNetworkPlayer().ConnectionId != null); } }

//        /// <summary>
//        /// Start the lobby server link. Establish a connection, if one is required.
//        /// </summary>

//        public virtual void Start() { mShutdown = false; }

//        /// <summary>
//        /// Stopping the server should be delayed in order for it to be thread-safe.
//        /// </summary>

//        public virtual void Stop()
//        {
//            if (!mShutdown)
//            {
//                mShutdown = true;
//            }
//        }

//        /// <summary>
//        /// Send an update to the lobby server.
//        /// </summary>

//        public abstract void SendUpdate();
//    }
//}
