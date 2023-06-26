////-------------------------------------------------
////                    GNet 3
//// Copyright © 2012-2018 Tasharen Entertainment Inc
////-------------------------------------------------

//using System;
//using System.IO;
//using UnityEngine;
//using System.Collections;

//namespace GNet
//{
//    /// <summary>
//    /// Src-based listener that makes it possible for servers to
//    /// register themselves with a central location for easy lobby by clients.
//    /// </summary>

//    public class SrcLobbyServer : LobbyServer
//    {
//        public SrcLobbyServer()
//        {
//            mSrc = new SrcProtocol("Lobby Server", TNManager.SetHubUrl, false);
//        }
//        // List of servers that's currently being updated
//        protected ServerList mList = new ServerList();
//        protected long mTime = 0;
//        protected SrcProtocol mSrc = null;
//        protected Thread mThread;
//        protected Buffer mBuffer;
//        protected long mStartTime = 0;

//        /// <summary>
//        /// Port used to listen for incoming packets.
//        /// </summary>

//        public override int port { get { return 0; } }

//        /// <summary>
//        /// Whether the server is active.
//        /// </summary>

//        public override bool isActive { get { return (mSrc != null && mSrc.isConnected); } }


//        public override NetworkPlayer GeGNetworkPlayer()
//        {
//            return mSrc;
//        }

//        /// <summary>
//        /// Start listening for incoming UDP packets on the specified listener port.
//        /// </summary>

//        public override bool Start(int listenPort)
//        {
//            Stop();
//            mStartTime = System.DateTime.UtcNow.Ticks / 10000;

//            //Tools.LoadList(banFilePath, mBan);

//#if FORCE_EN_US
//			Tools.SetCurrentCultureToEnUS();
//#endif
//            if (!mSrc.Start()) return false;
//#if STANDALONE
//			Tools.Print("Bans: " + mBan.Count);
//			Tools.Print("UDP Lobby Server started on port " + listenPort + " using interface " + UdpProtocol.defaulGNetworkInterface);
//#endif
//            mThread = Tools.CreateThread(ThreadFunction());
//            mThread.Start();
//            return true;
//        }

//        /// <summary>
//        /// Stop listening for incoming packets.
//        /// </summary>

//        public override void Stop()
//        {
//            if (mThread != null)
//            {
//                mThread.Stop();
//                mThread = null;
//            }

//            if (mSrc != null)
//            {
//                mSrc.DisconnectNow();

//                //Tools.LoadList(banFilePath, mBan);
//            }
//            mList.Clear();
//        }

//        /// <summary>
//        /// Thread that will be processing incoming data.
//        /// </summary>

//        IEnumerator ThreadFunction()
//        {
//            for (; ; )
//            {
//#if !STANDALONE
//                if (TNManager.isPaused)
//                {
//                    yield return new WaitForSeconds(0.5f);
//                    continue;
//                }
//#endif
//                mTime = DateTime.UtcNow.Ticks / 10000;

//                // Cleanup a list of servers by removing expired entries
//                mList.Cleanup(mTime);

//                Buffer buffer;
//                string endPoint;

//                // Process incoming SRC packets
//                if (mSrc != null && mSrc.ReceivePacket(out buffer, out endPoint))
//                {
//                    try
//                    {
//                        ProcessPacket(buffer, endPoint);
//                    }
//                    catch (System.Exception) { }

//                    if (buffer != null)
//                    {
//                        buffer.Recycle();
//                        buffer = null;
//                    }
//                }
//                yield return null;
//            }
//        }

//        /// <summary>
//        /// Process an incoming packet.
//        /// </summary>

//        bool ProcessPacket(Buffer buffer, string fromEndPoint)
//        {
//            //TODO: Handle IP somehow
//            //if (mBan.Count != 0 && mBan.Contains(ip.Address.ToString())) return false;

//            var reader = buffer.BeginReading();
//            var size = reader.ReadInt32();
//            var request = (Packet)reader.ReadByte();


//            switch (request)
//            {
//                case Packet.Connect:
//                    {
//                        int theirVer = reader.ReadInt32();

//                        if (theirVer == Player.version)
//                        {
//                            //TODO: Find out if we need to maintain a list of lobby clients and add them to a list on connect. Look at how game server client works
//                            var writer = BeginSend(Packet.Connected);
//                            writer.Write(Player.version);
//                            EndSend(fromEndPoint);
//                        }
//                        break;
//                    }
//                case Packet.RequestID:
//                    {
//                        var writer = BeginSend(Packet.ResponseID);
//                        //TODO: Find out if we need to maintain a list of lobby clients and increment them when requesting a new id. Look at how game server client works
//                        //writer.Write(player.id);
//                        //for now sending id of 0 for lobby clients
//                        writer.Write(0);
//                        writer.Write((Int64)(System.DateTime.UtcNow.Ticks / 10000));
//                        writer.Write(mStartTime);
//                        EndSend(fromEndPoint);
//                        break;
//                    }
//                case Packet.RequestPing:
//                    {
//                        var writer = BeginSend(Packet.ResponsePing);
//                        writer.Write(mTime);
//                        writer.Write((ushort)mList.list.size);
//                        EndSend(fromEndPoint);
//                        break;
//                    }
//                case Packet.RequestAddServer:
//                    {
//                        if (reader.ReadUInt16() != GameServer.gameId) return false;

//                        var ent = new ServerList.Entry();
//                        ent.ReadFrom(reader);

//                        //TODO: make sure users can't change their device id to get around the ban
//                        if (mBan.Count != 0 && (mBan.Contains(ent.connectionId.ToString()) || IsBanned(ent.name))) return false;

//                        //TODO: Handle fallback with IP somehow
//                        //if (ent.externalAddress.Address.Equals(IPAddress.None) ||
//                        //	ent.externalAddress.Address.Equals(IPAddress.IPv6None))
//                        //	ent.externalAddress = ip;

//                        mList.Add(ent, mTime);
//#if STANDALONE
//					Tools.Print(ip + " added a server (" + ent.internalAddress + ", " + ent.externalAddress + ")");
//#endif
//                        return true;
//                    }
//                case Packet.RequestRemoveServer:
//                    {
//                        if (reader.ReadUInt16() != GameServer.gameId) return false;
//                        var deviceId = reader.ReadString();

//                        RemoveServer(deviceId);
//#if STANDALONE
//					Tools.Print(ip + " removed a server (" + internalAddress + ", " + externalAddress + ")");
//#endif
//                        return true;
//                    }
//                case Packet.RequestServerList:
//                    {
//                        if (reader.ReadUInt16() != GameServer.gameId) return false;
//                        mList.WriteTo(BeginSend(Packet.ResponseServerList));
//                        EndSend(fromEndPoint);
//                        return true;
//                    }
//            }
//            return false;
//        }

//        /// <summary>
//        /// Add a new server to the list.
//        /// </summary>

//        public override void AddServer(string name, int playerCount, string connectionId)
//        {
//            mList.Add(name, playerCount, connectionId, mTime);
//        }

//        /// <summary>
//        /// Remove an existing server from the list.
//        /// </summary>

//        public override void RemoveServer(string connectionId)
//        {
//            mList.Remove(connectionId);
//        }

//        /// <summary>
//        /// Start the sending process.
//        /// </summary>

//        BinaryWriter BeginSend(Packet packet)
//        {
//            mBuffer = Buffer.Create();
//            BinaryWriter writer = mBuffer.BeginPacket(packet);
//            return writer;
//        }

//        /// <summary>
//        /// Send the outgoing buffer to the specified remote destination.
//        /// </summary>

//        void EndSend(string toEndPoint = null)
//        {
//            mBuffer.EndPacket();
//            mSrc.SendPacket(mBuffer, toEndPoint);
//            mBuffer.Recycle();
//            mBuffer = null;
//        }
//    }
//}
