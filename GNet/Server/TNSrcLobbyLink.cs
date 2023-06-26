////-------------------------------------------------
////                    GNet 3
//// Copyright © 2012-2018 Tasharen Entertainment Inc
////-------------------------------------------------

//using System;
//using System.Collections;
//using System.IO;
//using UnityEngine;

//namespace GNet
//{
//    /// <summary>
//    /// SignalRCore-based lobby server link. Designed to communicate with a remote SrcLobbyServer.
//    /// You can use this class to register your game server with a remote lobby server.
//    /// </summary>

//    public class SrcLobbyServerLink : LobbyServerLink
//    {
//        SrcProtocol mSrc;
//        string m_ServerConnectionId;
//        long mNextSend = 0;

//        /// <summary>
//        /// Create a new link to a remote lobby server.
//        /// </summary>

//        public SrcLobbyServerLink(GameServer gameServer, string serverConnectionId) : base()
//        {
//            mGameServer = gameServer;
//            m_ServerConnectionId = serverConnectionId;
//        }

//        /// <summary>
//        /// Whether the link is currently active.
//        /// </summary>

//        public override bool isActive
//        {
//            get
//            {
//                if (mSrc != null)
//                {
//                    if (mSrc.isConnected)
//                    {
//                        return true;
//                    }
//                    else
//                    {
//                        if (mSrc.stage == NetworkPlayer.Stage.Identifying)
//                        {
//                            return true;
//                        }
//                    }
//                }
//                return false;
//            }
//        }

//        /// <summary>
//        /// Make sure the socket gets released.
//        /// </summary>

//        ~SrcLobbyServerLink()
//        {
//            if (mSrc != null)
//            {
//                mSrc.DisconnectNow();
//                mSrc = null;
//            }
//        }

//        /// <summary>
//        /// Start the lobby server link.
//        /// </summary>

//        public override void Start()
//        {
//            base.Start();

//#if FORCE_EN_US
//		Tools.SetCurrentCultureToEnUS();
//#endif
//            if (mSrc == null)
//            {
//                mSrc = new SrcProtocol("Lobby Link", TNManager.SetHubUrl, false, true);
//                //TODO: Now we could probably make this a Connect function so it feels more like Tcp
//                mSrc.ConnectedToId = m_ServerConnectionId;
//                mSrc.Start();
//            }
//        }

//        /// <summary>
//        /// Send a server update.
//        /// </summary>

//        public override void SendUpdate()
//        {
//            if (!mShutdown)
//            {
//                mNextSend = 0;

//                if (mThread == null)
//                {
//                    //TODO: Maybe make this coroutine always run instead of creating so much garbage collection
//                    mThread = Tools.CreateThread(ThreadFunction());
//                    mThread.Start();
//                }
//            }
//        }

//        /// <summary>
//        /// Send periodic updates.
//        /// </summary>

//        IEnumerator ThreadFunction()
//        {
//            while (true)
//            {
//#if !STANDALONE
//                if (TNManager.isPaused || string.IsNullOrEmpty(mGameServer.GeGNetworkPlayer().ConnectionId))
//                {
//                    yield return new WaitForSeconds(0.5f);
//                    continue;
//                }
//#endif
//                long time = DateTime.UtcNow.Ticks / 10000;

//                if (mShutdown)
//                {
//                    Buffer buffer = Buffer.Create();
//                    BinaryWriter writer = buffer.BeginPacket(Packet.RequestRemoveServer);
//                    writer.Write(GameServer.gameId);
//                    writer.Write(mGameServer.GeGNetworkPlayer().ConnectionId);
//                    buffer.EndPacket();
//                    mSrc.SendPacket(buffer);
//                    buffer.Recycle();
//                    mThread = null;
//                    break;
//                }

//                if (mNextSend < time && mGameServer != null)
//                {
//                    if (isActive)
//                    {
//                        mNextSend = time + 3000;
//                        Buffer buffer = Buffer.Create();
//                        var writer = buffer.BeginPacket(Packet.RequestAddServer);
//                        writer.Write(GameServer.gameId);
//                        writer.Write(mGameServer.name);
//                        writer.Write((short)mGameServer.playerCount);
//                        writer.Write(mGameServer.GeGNetworkPlayer().ConnectionId);
//                        buffer.EndPacket();
//                        mSrc.SendPacket(buffer);
//                        buffer.Recycle();
//                    }
//                }
//                yield return new WaitForSeconds(0.01f);
//            }
//        }
//    }
//}
