using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using GNetServer;
using ContainerNinja.Services;

namespace ContainerNinja.Hubs
{
    public class GameHub : Hub
    {
        private readonly IGameService m_GameService;

        public GameHub(IGameService gameService)
        {
            m_GameService = gameService;
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            m_GameService.StopAnyUserServer(Context.ConnectionId);
            LeaveAnyUserJoinedServer();
            return base.OnDisconnectedAsync(exception);
        }

        public async Task OnReceiveRequestConnectionIdPacket(RequestConnectionIdPacket requestConnectionIdPacket)
        {
            var receiveResponseConnectionIdPacket = new ResponseConnectionIdPacket(Context.ConnectionId);
            await Clients.Caller.SendAsync(receiveResponseConnectionIdPacket.GetHubTarget(), receiveResponseConnectionIdPacket);
        }

        public async Task OnReceiveRequestNewGameServerPacket(RequestNewGameServerPacket sendRequestNewServerIdPacket)
        {
            m_GameService.StopAnyUserServer(Context.ConnectionId);
            //TODO:
            //if (GameServer.GameId == sendRequestNewServerIdPacket.GameId)
            //{

            //}
            var newGameServer = new GameServer();
            newGameServer.GameServerInfo.Name = sendRequestNewServerIdPacket.Name;
            newGameServer.GameServerInfo.PlayerCount = sendRequestNewServerIdPacket.PlayerCount;
            newGameServer.GameServerInfo.UserId = Context.ConnectionId;
            newGameServer.onShutdown += m_GameService.OnGameServerStopped;
            newGameServer.onPlayerDisconnect += m_GameService.OnUserLeftServer;
            newGameServer.onPlayerConnect += m_GameService.OnUserJoinedGameServer;
            newGameServer.onSendToClient += m_GameService.OnSendToClient;

            m_GameService.AddNewUserGameServer(Context.ConnectionId, newGameServer);



            var responseNewServerIdPacket = new ResponseNewGameServerPacket(newGameServer.GameServerInfo.ServerId);
            await Clients.Caller.SendAsync(responseNewServerIdPacket.GetHubTarget(), responseNewServerIdPacket);
        }

        public async Task OnReceiveRequestStopGameServerPacket(RequestStopGameServerPacket requestStopGameServerPacket)
        {
            var gameServer = m_GameService.GetGameServerByServerId(requestStopGameServerPacket.ServerId);
            if (gameServer != null)
            {
                gameServer.Stop();
                var responseStopGameServerPacket = new ResponseStopGameServerPacket(requestStopGameServerPacket.ServerId);
                await Clients.Caller.SendAsync(responseStopGameServerPacket.GetHubTarget(), responseStopGameServerPacket);
            }
        }

        public async Task OnReceiveRequestServerListPacket(RequestServerListPacket requestServerListPacket)
        {
            var receiveResponseServerListPacket = new ResponseServerListPacket(m_GameService.GetGameServerList());
            await Clients.Caller.SendAsync(receiveResponseServerListPacket.GetHubTarget(), receiveResponseServerListPacket);
        }

        private void LeaveAnyUserJoinedServer()
        {
            var userGameServer = m_GameService.GetJoinedGameServer(Context.ConnectionId);
            if (userGameServer != null)
            {
                userGameServer.LeaveServer(Context.ConnectionId);
            }
        }

        public void OnReceiveRequestJoinServerPacket(RequestJoinServerPacket requestJoinServerPacket)
        {
            LeaveAnyUserJoinedServer();
            var gameServer = m_GameService.GetGameServerByServerId(requestJoinServerPacket.ServerId);
            if (gameServer != null)
            {
                gameServer.JoinServer(Context.ConnectionId, requestJoinServerPacket.Name);
            }
        }

        public void OnReceiveRequestLeaveServerPacket(RequestLeaveServerPacket requestLeaveServerPacket)
        {
            var gameServer = m_GameService.GetGameServerByServerId(requestLeaveServerPacket.ServerId);
            if (gameServer != null)
            {
                gameServer.LeaveServer(Context.ConnectionId);
            }
        }

        public void OnReceiveRequestJoinChannelPacket(RequestJoinChannelPacket requestJoinChannelPacket)
        {
            var gameServer = m_GameService.GetJoinedGameServer(Context.ConnectionId);
            if (gameServer != null)
            {
                gameServer.JoinChannel(Context.ConnectionId, requestJoinChannelPacket.ChannelId, requestJoinChannelPacket.Password, requestJoinChannelPacket.LevelName, requestJoinChannelPacket.Persistent, 255);// requestJoinChannelPacket.PlayerLimit);
            }
        }

        public void OnReceiveRequestLeaveChannelPacket(RequestLeaveChannelPacket requestLeaveChannelPacket)
        {
            var gameServer = m_GameService.GetJoinedGameServer(Context.ConnectionId);
            if (gameServer != null)
            {
                gameServer.LeaveChannel(Context.ConnectionId, requestLeaveChannelPacket.ChannelId);
            }
        }

        public void OnReceiveForwardPacket(ForwardPacket forwardPacket)
        {
            var gameServer = m_GameService.GetJoinedGameServer(Context.ConnectionId);
            if (gameServer != null)
            {
                gameServer.SendForwardPacket(Context.ConnectionId, forwardPacket.ChannelId, forwardPacket.Uid, forwardPacket.FunctionName, (ForwardType)forwardPacket.ForwardType, forwardPacket.Data);
            }
        }

        public void OnReceiveForwardToPlayerPacket(ForwardToPlayerPacket forwardToPlayerPacket)
        {
            var gameServer = m_GameService.GetJoinedGameServer(Context.ConnectionId);
            if (gameServer != null)
            {
                gameServer.SendForwardToPlayerPacket(forwardToPlayerPacket.ChannelId, forwardToPlayerPacket.Uid, forwardToPlayerPacket.FromPlayerId, forwardToPlayerPacket.ToPlayerId, forwardToPlayerPacket.FunctionName, ForwardType.None, forwardToPlayerPacket.Data);
            }
        }

        public void OnReceiveRequestDestroyObject(RequestDestroyObject requestDestroyObject)
        {
            var gameServer = m_GameService.GetJoinedGameServer(Context.ConnectionId);
            if (gameServer != null)
            {
                gameServer.DestroyObject(Context.ConnectionId, requestDestroyObject.ChannelId, requestDestroyObject.Uid);
            }
        }

        public void OnReceiveRequestCreateObject(RequestCreateObject requestCreateObject)
        {
            var gameServer = m_GameService.GetJoinedGameServer(Context.ConnectionId);
            if (gameServer != null)
            {
                gameServer.CreateObject(Context.ConnectionId, requestCreateObject.ChannelId, requestCreateObject.PlayerId, requestCreateObject.Persistent, requestCreateObject.ObjsData);
            }
        }

        public void OnReceiveRequestSetNamePacket(RequestSetNamePacket requestSetNamePacket)
        {
            var gameServer = m_GameService.GetJoinedGameServer(Context.ConnectionId);
            if (gameServer != null)
            {
                gameServer.SetName(Context.ConnectionId, requestSetNamePacket.Name);
            }
        }

        public void OnReceiveRequestTransferObjectPacket(RequestTransferObjectPacket requestTransferObjectPacket)
        {
            var gameServer = m_GameService.GetJoinedGameServer(Context.ConnectionId);
            if (gameServer != null)
            {
                gameServer.TransferObject(Context.ConnectionId, requestTransferObjectPacket.PlayerId, requestTransferObjectPacket.OldChannelId, requestTransferObjectPacket.NewChannelId, requestTransferObjectPacket.OldObjectId);
            }
        }
    }
}
