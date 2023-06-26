////-------------------------------------------------
////                    GNet 3
//// Copyright © 2012-2018 Tasharen Entertainment Inc
////-------------------------------------------------

//// Must also be defined in TNGameServer.cs
//#define SINGLE_THREADED

//using UnityEngine;
//using System.IO;

//namespace GNet
//{
//	/// <summary>
//	/// Tasharen Network server tailored for Unity.
//	/// </summary>

//	public class TNServerInstance : MonoBehaviour
//	{
//		[DoNotObfuscate] public enum Type
//		{
//			Lan,
//			Udp,
//			Tcp,
//			Src,
//		}

//		[DoNotObfuscate] public enum State
//		{
//			Inactive,
//			Starting,
//			Active,
//		}

//		private void Awake()
//		{
//			if (Instance == null)
//			{
//				Instance = this;
//				DontDestroyOnLoad(gameObject);
//			}
//			else
//			{
//				Destroy(gameObject);
//			}
//		}

//		private void Start()
//		{
//			mGame = new GameServer(Type.Src);
//			mLobby = new SrcLobbyServer();
//		}

//		LobbyServer mLobby = null;
//		//UPnP mUp = new UPnP();

//		/// <summary>
//		/// Instance access is internal only as all the functions are static for convenience purposes.
//		/// </summary>

//		static public TNServerInstance Instance;

//		/// <summary>
//		/// Custom packet receiving function, called from a worker thread.
//		/// </summary>

//		static public System.Action onReceivePackets { get { return (Instance != null) ? Instance.mGame.onReceivePackets : null; } set { if (Instance != null) Instance.mGame.onReceivePackets = value; } }

//		/// <summary>
//		/// Path to the admin file.
//		/// </summary>

//		static public string adminPath
//		{
//			get
//			{
//				if (!isActive) return GameServer.defaultAdminPath;
//				return string.IsNullOrEmpty(Instance.mGame.rootDirectory) ? Instance.mGame.adminFilePath : Path.Combine(Instance.mGame.rootDirectory, Instance.mGame.adminFilePath);
//			}
//		}


//		/// <summary>
//		/// Whether the server instance is currently active.
//		/// </summary>

//		static public bool isActive { get { return (Instance != null) && Instance.mGame.isActive; } }

//		/// <summary>
//		/// Port used to listen for incoming TCP connections.
//		/// </summary>

//		static public int listeningPort { get { return 0; } }

//		/// <summary>
//		/// Set your server's name.
//		/// </summary>

//		static public string serverName { get { return (Instance != null) ? Instance.mGame.name : null; } set { if (Instance != null) Instance.mGame.name = value; } }

//		/// <summary>
//		/// How many players are currently connected to the server.
//		/// </summary>

//		static public int playerCount { get { return (Instance != null) ? Instance.mGame.playerCount : 0; } }

//		/// <summary>
//		/// List of connected players.
//		/// </summary>

//		static public List<NetworkPlayer> players { get { return (Instance != null) ? Instance.mGame.players : null; } }

//		/// <summary>
//		/// Active lobby server.
//		/// </summary>

//		static public LobbyServer lobby { get { return (Instance != null) ? Instance.mLobby : null; } }

//		/// <summary>
//		/// Set the root directory for the server instance. Call this before Start().
//		/// </summary>

//		static public void SetRootDirectory (string path)
//		{
//			Instance.mGame.rootDirectory = path;
//		}

//		/// <summary>
//		/// Start a new server.
//		/// </summary>

//		public bool StartGameServer (int tcpPort, int udpPort, int srcPort)
//		{
//			if (mGame.isActive) DisconnectGameServer();
//			if (mGame.Start(tcpPort, udpPort, srcPort))
//			{
//				return true;
//			}

//			DisconnectGameServer();
//			return false;
//		}

//		public void DisconnectLobbyServer()
//		{
//			if (mLobby != null)
//			{
//				mLobby.Stop();
//			}
//		}

//		public void DisconnectGameServer()
//		{
//			if (mGame != null)
//			{
//				mGame.Stop();
//			}
//		}

//		/// <summary>
//		/// Make sure that the servers are stopped when the server instance is destroyed.
//		/// </summary>

//		void OnDestroy ()
//		{
//			DisconnectLobbyServer();
//			DisconnectGameServer();
//			//mUp.WaitForThreads();
//		}

//		[System.Obsolete("Calling this function is no longer necessary. The server will auto-save.")]
//		static public void SaveTo (string fileName) { }
//	}
//}
