using System;
using System.Collections;
using System.Collections.Generic;
using BestHTTP.SignalRCore;
using BestHTTP.SignalRCore.Encoders;
using UnityEngine;

namespace GNet
{
    public class ClientPlayer : NetworkPlayer
    {
        public class CommandPacketInvoke
        {
            public CommandPacketInvoke(CommandPacket commandPacket, Action<CommandPacket> action)
            {
                CommandPacket = commandPacket;
                Action = action;
            }

            public CommandPacket CommandPacket;
            public Action<CommandPacket> Action;
        }
        public ClientPlayer(string name, string hubUri) : base(name)
        {
            m_HubUri = hubUri;
        }

        protected Queue<CommandPacketInvoke> mCommandPacketIn = new Queue<CommandPacketInvoke>();
        protected Queue<CommandPacket> mCommandPacketOut = new Queue<CommandPacket>();
        private HubConnection m_HubConnection;
        private Coroutine m_SendPacketsCoroutine;
        private Coroutine m_ReceivePacketsCoroutine;
        private string m_HubUri;

        public bool ConnectToHub()
        {
#if !MODDING
            DisconnectFromHubNow();

            hubStage = Stage.Connecting;
            try
            {
                m_HubConnection = new HubConnection(new Uri(m_HubUri), new MessagePackProtocol());
                m_HubConnection.AuthenticationProvider = new AzureSignalRServiceAuthenticator(m_HubConnection);
                m_HubConnection.OnClosed += OnHubConnectionClosed;
                m_HubConnection.OnError += OnHubError;
                m_HubConnection.OnConnected += OnConnectedToHub;
                m_HubConnection.On("OnReceiveResponseNewGameServerPacket", (ResponseNewGameServerPacket p) => ReceivePacket(p, ReceiveResponseNewGameServerPacket));
                m_HubConnection.On("OnReceiveResponseServerListPacket", (ResponseServerListPacket p) => ReceivePacket(p, ReceiveResponseServerListPacket));
                m_HubConnection.On<ResponseConnectionIdPacket>("OnReceiveResponseConnectionIdPacket", OnReceiveResponseConnectionIdPacket);

                if (m_SendPacketsCoroutine != null)
                {
                    TNManager.instance.StopCoroutine(m_SendPacketsCoroutine);
                    m_SendPacketsCoroutine = null;
                }

                m_SendPacketsCoroutine = TNManager.instance.StartCoroutine(SendPackets());

                if (m_ReceivePacketsCoroutine != null)
                {
                    TNManager.instance.StopCoroutine(m_ReceivePacketsCoroutine);
                    m_ReceivePacketsCoroutine = null;
                }

                m_ReceivePacketsCoroutine = TNManager.instance.StartCoroutine(ReceivePackets());
                m_HubConnection.StartConnect();
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message + ex.StackTrace);
                return false;
            }
            return true;
#endif
        }

        private void OnConnectedToHub(HubConnection hub)
        {
            SendPacket(new RequestConnectionIdPacket());
        }

        private void OnReceiveResponseConnectionIdPacket(ResponseConnectionIdPacket responseConnectionIdPacket)
        {
            hubStage = Stage.Connected;
            ConnectionId = responseConnectionIdPacket.ConnectionId;
            ConnectedToHub?.Invoke(this);
            ReceiveResponseConnectionIdPacket?.Invoke(responseConnectionIdPacket);
        }

        public void StartDisconnectingFromHub()
        {
#if !MODDING
            if (m_HubConnection != null && hubStage != Stage.Disconnected)
            {
                hubStage = Stage.Disconnecting;
                m_HubConnection.StartClose();
            }
            else
            {
                OnHubConnectionClosed(null);
            }
#endif
        }

        public void DisconnectFromHubNow()
        {
            if (m_HubConnection != null && hubStage != Stage.Disconnected)
            {
                m_HubConnection.OnClosed -= OnHubConnectionClosed;
                m_HubConnection.StartClose();
            }
            OnHubConnectionClosed(null);
        }

        private void OnHubConnectionClosed(HubConnection obj)
        {
#if !MODDING
            if (hubStage != Stage.Disconnected)
            {
                hubStage = Stage.Disconnected;
                ConnectionId = null;

                m_HubConnection.OnClosed -= OnHubConnectionClosed;
                m_HubConnection.OnError -= OnHubError;
                m_HubConnection.OnConnected -= OnConnectedToHub;
                m_HubConnection.Remove("OnReceiveResponseNewGameServerPacket");
                m_HubConnection.Remove("OnReceiveResponseServerListPacket");
                m_HubConnection.Remove("OnReceiveResponseConnectionIdPacket");
                m_HubConnection = null;
                if (m_SendPacketsCoroutine != null)
                {
                    TNManager.instance.StopCoroutine(m_SendPacketsCoroutine);
                    m_SendPacketsCoroutine = null;
                }
                if (m_ReceivePacketsCoroutine != null)
                {
                    TNManager.instance.StopCoroutine(m_ReceivePacketsCoroutine);
                    m_ReceivePacketsCoroutine = null;
                }
                DisconnectedFromHub?.Invoke(this);
            }
#endif
        }

        private void OnHubError(HubConnection obj, string error)
        {
            DisconnectFromHubNow();
            Debug.Log(error);
        }

        public bool JoinGameServer(string gameServerId)
        {
#if !MODDING
            DisconnectFromGameServerNow();
            try
            {
                gameServerStage = Stage.Connecting;
                m_HubConnection.On<ResponseJoinServerPacket>("OnReceiveResponseJoinServerPacket", OnReceiveResponseJoinServerPacket);
                m_HubConnection.On<ResponseLeaveServerPacket>("OnReceiveResponseLeaveServerPacket", OnReceiveResponseLeaveServerPacket);
                //m_HubConnection.On("OnReceiveClientDisconnectedPacket", ReceiveClientDisconnectedPacket);
                //m_HubConnection.On("OnReceiveResponseJoinChannelPacket", ReceiveResponseJoinChannelPacket);
                //m_HubConnection.On("OnReceiveJoiningChannelPacket", ReceiveJoiningChannelPacket);
                //m_HubConnection.On("OnReceiveUpdateChannelPacket", ReceiveUpdateChannelPacket);
                //m_HubConnection.On("OnReceiveResponseLeaveChannelPacket", ReceiveResponseLeaveChannelPacket);
                //m_HubConnection.On("OnReceiveLoadLevelPacket", ReceiveLoadLevelPacket);
                //m_HubConnection.On("OnReceiveCreateObjectPacket", ReceiveCreateObjectPacket);
                //m_HubConnection.On("OnReceiveDestroyObjectsPacket", ReceiveDestroyObjectsPacket);
                //m_HubConnection.On("OnReceiveForwardPacket", ReceiveForwardPacket);
                //m_HubConnection.On("OnReceiveResponseDestroyObjectsPacket", ReceiveResponseDestroyObjectsPacket);
                //m_HubConnection.On("OnReceiveTransferredObjectPacket", ReceiveTransferredObjectPacket);
                //m_HubConnection.On("OnReceiveResponseSetNamePacket", ReceiveResponseSetNamePacket);
                //m_HubConnection.On("OnReceivePlayerChangedNamePacket", ReceivePlayerChangedNamePacket);
                m_HubConnection.On("OnReceiveClientDisconnectedPacket", (ClientDisconnectedPacket p) => ReceivePacket(p, ReceiveClientDisconnectedPacket));
                m_HubConnection.On("OnReceiveResponseJoinChannelPacket", (ResponseJoinChannelPacket p) => ReceivePacket(p, ReceiveResponseJoinChannelPacket));
                m_HubConnection.On("OnReceiveJoiningChannelPacket", (JoiningChannelPacket p) => ReceivePacket(p, ReceiveJoiningChannelPacket));
                m_HubConnection.On("OnReceiveUpdateChannelPacket", (UpdateChannelPacket p) => ReceivePacket(p, ReceiveUpdateChannelPacket));
                m_HubConnection.On("OnReceiveResponseLeaveChannelPacket", (ResponseLeaveChannelPacket p) => ReceivePacket(p, ReceiveResponseLeaveChannelPacket));
                m_HubConnection.On("OnReceiveLoadLevelPacket", (LoadLevelPacket p) => ReceivePacket(p, ReceiveLoadLevelPacket));
                m_HubConnection.On("OnReceiveCreateObjectPacket", (CreateObjectPacket p) => ReceivePacket(p, ReceiveCreateObjectPacket));
                m_HubConnection.On("OnReceiveDestroyObjectsPacket", (DestroyObjectsPacket p) => ReceivePacket(p, ReceiveDestroyObjectsPacket));
                m_HubConnection.On("OnReceiveForwardPacket", (ForwardPacket p) => ReceivePacket(p, ReceiveForwardPacket));
                m_HubConnection.On("OnReceivePlayerJoinedChannelPacket", (PlayerJoinedChannelPacket p) => ReceivePacket(p, ReceivePlayerJoinedChannelPacket));
                m_HubConnection.On("OnReceivePlayerLeftChannelPacket", (PlayerLeftChannelPacket p) => ReceivePacket(p, ReceivePlayerLeftChannelPacket));
                m_HubConnection.On("OnReceiveResponseDestroyObjectsPacket", (ResponseDestroyObjectsPacket p) => ReceivePacket(p, ReceiveResponseDestroyObjectsPacket));
                m_HubConnection.On("OnReceiveTransferredObjectPacket", (TransferredObjectPacket p) => ReceivePacket(p, ReceiveTransferredObjectPacket));
                m_HubConnection.On("OnReceiveResponseSetNamePacket", (ResponseSetNamePacket p) => ReceivePacket(p, ReceiveResponseSetNamePacket));
                m_HubConnection.On("OnReceivePlayerChangedNamePacket", (PlayerChangedNamePacket p) => ReceivePacket(p, ReceivePlayerChangedNamePacket));
                SendPacket(new RequestJoinServerPacket(gameServerId, name));
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message + ex.StackTrace);
                return false;
            }
            return true;
#endif
        }

        private void ReceivePacket(CommandPacket commandPacket, Action<CommandPacket> action)
        {
            lock (mCommandPacketIn)
            {
                mCommandPacketIn.Enqueue(new CommandPacketInvoke(commandPacket, action));
            }
        }

        private void OnReceiveResponseJoinServerPacket(ResponseJoinServerPacket responseJoinServerPacket)
        {
            gameServerStage = Stage.Connected;
            GameServerId = responseJoinServerPacket.ServerId;
            id = responseJoinServerPacket.PlayerId;
            ConnectedToGameServer?.Invoke(this);
            ReceiveResponseJoinServerPacket?.Invoke(responseJoinServerPacket);
        }

        public void StartDisconnectingFromGameServer()
        {
            if (m_HubConnection != null && gameServerStage != Stage.Disconnected && !string.IsNullOrEmpty(GameServerId))
            {
                SendPacket(new RequestLeaveServerPacket(GameServerId, id));
            }
            else
            {
                OnGameServerConnectionClosed();
            }
        }

        public void DisconnectFromGameServerNow()
        {
            if (m_HubConnection != null && gameServerStage != Stage.Disconnected && !string.IsNullOrEmpty(GameServerId))
            {
                m_HubConnection.Remove("OnReceiveResponseLeaveServerPacket");
                SendPacket(new RequestLeaveServerPacket(GameServerId, id));
            }
            OnGameServerConnectionClosed();
        }

        private void OnReceiveResponseLeaveServerPacket(ResponseLeaveServerPacket responseLeaveServerPacket)
        {
            ReceiveResponseLeaveServerPacket?.Invoke(responseLeaveServerPacket);
            OnGameServerConnectionClosed();
        }

        private void OnGameServerConnectionClosed()
        {
#if !MODDING
            if (gameServerStage != Stage.Disconnected)
            {
                gameServerStage = Stage.Disconnected;
                id = 0;
                GameServerId = null;

                m_HubConnection.Remove("OnReceiveResponseJoinServerPacket");
                m_HubConnection.Remove("OnReceiveResponseLeaveServerPacket");
                m_HubConnection.Remove("OnReceiveClientDisconnectedPacket");
                m_HubConnection.Remove("OnReceiveResponseJoinChannelPacket");
                m_HubConnection.Remove("OnReceiveJoiningChannelPacket");
                m_HubConnection.Remove("OnReceiveResponseLeaveChannelPacket");
                m_HubConnection.Remove("OnReceiveUpdateChannelPacket");
                m_HubConnection.Remove("OnReceiveLoadLevelPacket");
                m_HubConnection.Remove("OnReceiveCreateObjectPacket");
                m_HubConnection.Remove("OnReceiveDestroyObjectsPacket");
                m_HubConnection.Remove("OnReceiveForwardPacket");
                m_HubConnection.Remove("OnReceivePlayerJoinedChannelPacket");
                m_HubConnection.Remove("OnReceiveResponseDestroyObjectsPacket");
                m_HubConnection.Remove("OnReceiveTransferredObjectPacket");
                m_HubConnection.Remove("OnReceiveResponseSetNamePacket");
                m_HubConnection.Remove("OnReceivePlayerChangedNamePacket");

                DisconnectedFromGameServer?.Invoke(this);
            }
#endif
        }

        private IEnumerator SendPackets()
        {
            while (hubStage != Stage.Disconnected)
            {
                if (mCommandPacketOut.Count > 0)
                {
                    lock (mCommandPacketOut)
                    {
#if !MODDING
                        if (m_HubConnection != null)
                        {

                            var sentPackets = 0;
                            var maxPacketsToSend = 10;
                            while (mCommandPacketOut.Count > 0 && sentPackets < maxPacketsToSend)
                            {
                                var commandPacket = mCommandPacketOut.Dequeue();
                                m_HubConnection.Send(commandPacket.GetHubTarget(), commandPacket);
                                sentPackets++;
                            }
                        }
#endif
                    }
                }
                yield return null;
            }
        }

        private IEnumerator ReceivePackets()
        {
            while (hubStage != Stage.Disconnected)
            {
                if (mCommandPacketIn.Count > 0)
                {
                    lock (mCommandPacketIn)
                    {
#if !MODDING
                        var receivedPackets = 0;
                        var maxPacketsToReceive = 10;
                        while (mCommandPacketIn.Count > 0 && receivedPackets < maxPacketsToReceive)
                        {
                            var commandPacketInvoke = mCommandPacketIn.Dequeue();
                            if (commandPacketInvoke != null && commandPacketInvoke.Action != null)
                            {
                                commandPacketInvoke.Action(commandPacketInvoke.CommandPacket);
                            }
                            receivedPackets++;
#endif
                        }

                    }
                }
                yield return null;
            }
        }

        public override void SendPacket(CommandPacket commandPacket)
        {
            lock (mCommandPacketOut)
            {
                mCommandPacketOut.Enqueue(commandPacket);
            }
        }

        public override void AssignID()
        {
            throw new Exception("Can not assign Id on the client");
        }

        ~ClientPlayer()
        {
            DisconnectFromGameServerNow();
            DisconnectFromHubNow();
        }
    }

    public sealed class AzureSignalRServiceAuthenticator : IAuthenticationProvider
    {
        /// <summary>
        /// No pre-auth step required for this type of authentication
        /// </summary>
        public bool IsPreAuthRequired { get { return false; } }

#pragma warning disable 0067
        /// <summary>
        /// Not used event as IsPreAuthRequired is false
        /// </summary>
        public event OnAuthenticationSuccededDelegate OnAuthenticationSucceded;

        /// <summary>
        /// Not used event as IsPreAuthRequired is false
        /// </summary>
        public event OnAuthenticationFailedDelegate OnAuthenticationFailed;

#pragma warning restore 0067

        private HubConnection _connection;

        public AzureSignalRServiceAuthenticator(HubConnection connection)
        {
            this._connection = connection;
        }

        /// <summary>
        /// Not used as IsPreAuthRequired is false
        /// </summary>
        public void StartAuthentication()
        { }

        /// <summary>
        /// Prepares the request by adding two headers to it
        /// </summary>
        public void PrepareRequest(BestHTTP.HTTPRequest request)
        {
            if (this._connection.NegotiationResult == null)
                return;

            // Add Authorization header to http requests, add access_token param to the uri otherwise
            if (BestHTTP.Connections.HTTPProtocolFactory.GetProtocolFromUri(request.CurrentUri) == BestHTTP.Connections.SupportedProtocols.HTTP)
                request.SetHeader("Authorization", "Bearer " + this._connection.NegotiationResult.AccessToken);
            else
                request.Uri = PrepareUriImpl(request.Uri);
        }

        public Uri PrepareUri(Uri uri)
        {
            if (uri.Query.StartsWith("??"))
            {
                UriBuilder builder = new UriBuilder(uri);
                builder.Query = builder.Query.Substring(2);

                return builder.Uri;
            }

            return uri;
        }

        private Uri PrepareUriImpl(Uri uri)
        {
            string query = string.IsNullOrEmpty(uri.Query) ? "" : uri.Query + "&";
            UriBuilder uriBuilder = new UriBuilder(uri.Scheme, uri.Host, uri.Port, uri.AbsolutePath, query + "access_token=" + this._connection.NegotiationResult.AccessToken);
            return uriBuilder.Uri;
        }
    }

}
