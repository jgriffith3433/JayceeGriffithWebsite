using UnityEngine;

namespace GNet
{
    /// <summary>
    /// SignalRCore-based lobby client, designed to communicate with the SrcLobbyServer.
    /// </summary>

    public class TNSrcLobbyClient : LobbyClient
    {
        public int RequestsPerMinute = 3;
        public string HubUrlToConnectTo = GNetConfig.HubUrl;
        public static ushort GameId = 1;
        private ClientPlayer mSrc = null;
        long mNextSend = 0;
        bool mReEnable = false;
        private static TNSrcLobbyClient m_Instance = null;

        public bool IsConnectedToHub
        {
            get { return mSrc.isConnectedToHub; }
        }

        public bool IsConnectingToHub
        {
            get { return mSrc.hubStage == NetworkPlayer.Stage.Connecting; }
        }

        public static TNSrcLobbyClient Instance
        {
            get { return m_Instance; }
        }

        private void Awake()
        {
            if (Application.isPlaying)
            {
                if (m_Instance == null)
                {
                    m_Instance = this;
                    DontDestroyOnLoad(gameObject);
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            mSrc = new ClientPlayer("Lobby Client", HubUrlToConnectTo ?? GNetConfig.HubUrl); ;
            mSrc.ReceiveResponseServerListPacket += OnReceivedServerList;
        }

        protected override void OnDisable()
        {
            isActive = false;
            base.OnDisable();

            try
            {
                if (mSrc != null)
                {
                    mSrc.ReceiveResponseServerListPacket -= OnReceivedServerList;
                    mSrc.StartDisconnectingFromHub();
                    mSrc = null;
                }
                onChange?.Invoke();
            }
            catch (System.Exception)
            {
            }
        }

        void OnReceivedServerList(CommandPacket commandPacket)
        {
            var responseServerListPacket = commandPacket as ResponseServerListPacket;
            long time = System.DateTime.UtcNow.Ticks / 10000;
            knownServers.Clear();
            knownServers.ReadFrom(responseServerListPacket, time);
            //TODO: cleaning up is a good idea, but for now comment out so my list of servers don't keep disappearing
            //knownServers.CleanupAfterTime(time);
        }

        //test for browser payloads
        //public System.Collections.IEnumerator Start()
        //{
        //    yield return new WaitForSeconds(1);
        //    BrowserBridge.m_Instance.ProcessPacket("{\"payloadType\":\"BrowserPastedEventPayload\",\"pastedText\":\"asdf\"}");
        //}

        void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                if (isActive)
                {
                    mReEnable = true;
                    OnDisable();
                }
            }
            else if (mReEnable)
            {
                mReEnable = false;
                //Start();
            }
        }

        public void ForceRequest()
        {
            mNextSend = 0;
        }

        /// <summary>
        /// Keep receiving incoming packets.
        /// </summary>

        void Update()
        {
            bool changed = false;
            long time = System.DateTime.UtcNow.Ticks / 10000;

            if (mSrc != null && !string.IsNullOrEmpty(mSrc.ConnectionId))
            {
                //TODO: Probably a good idea, commenting out for now to show server list
                // Clean up old servers
                //if (knownServers.Cleanup())
                    //changed = true;

                // Trigger the listener callback
                if (changed && onChange != null)
                {
                    onChange();
                }
                else if (mNextSend < time && mSrc != null && mSrc.isConnectedToHub)
                {
                    mNextSend = time + (60 / RequestsPerMinute * 1000);
                    mSrc.SendPacket(new RequestServerListPacket(GameId));
                }
            }
        }

        public void ConnectToHub()
        {
            mSrc.ConnectToHub();
        }

        public void DisconnectFromHub()
        {
            mSrc.StartDisconnectingFromHub();
        }
    }
}
