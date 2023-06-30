using Microsoft.AspNetCore.SignalR;
using GNetServer;
using ContainerNinja.Contracts.Services;

namespace ContainerNinja.Core.Services
{
    public class GameHub : Hub
    {
        private readonly IGameService m_GameService;

        public GameHub(IGameService gameService)
        {
            m_GameService = gameService;
        }

        public override Task OnConnectedAsync()
        {
            var count = m_GameService.Users.Count + 1;
            var userName = "User " + count.ToString();
            while (string.IsNullOrEmpty(m_GameService.GetUserIdByUserName(userName)) == false)
            {
                count++;
                userName = "User " + count.ToString();
            }
            m_GameService.AddUser(Context.ConnectionId, userName);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            m_GameService.RemoveUser(Context.ConnectionId);
            m_GameService.StopAnyUserServer(Context.ConnectionId);
            LeaveAnyUserJoinedServer();
            m_GameService.RemoveUser(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }

        public async Task OnReceiveRequestConnectionIdPacket(RequestConnectionIdPacket requestConnectionIdPacket)
        {
            var receiveResponseConnectionIdPacket = new ResponseConnectionIdPacket(Context.ConnectionId);
            await Clients.Caller.SendAsync(receiveResponseConnectionIdPacket.GetHubTarget(), receiveResponseConnectionIdPacket);
        }

        public async Task OnReceiveRequestNewGameServerPacket(RequestNewGameServerPacket sendRequestNewServerIdPacket)
        {
            var newGameServerInfo = new GameServerInfo(Guid.NewGuid().ToString().Substring(0, 4), GameServer.GameId);
            newGameServerInfo.Name = sendRequestNewServerIdPacket.Name;
            newGameServerInfo.PlayerCount = sendRequestNewServerIdPacket.PlayerCount;
            newGameServerInfo.UserId = Context.ConnectionId;

            m_GameService.StopAnyUserServer(Context.ConnectionId);
            m_GameService.AddNewUserGameServer(Context.ConnectionId, newGameServerInfo);
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
                var userName = m_GameService.Users[Context.ConnectionId];
                gameServer.JoinServer(Context.ConnectionId, userName);
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
                gameServer.JoinChannel(Context.ConnectionId, requestJoinChannelPacket.ChannelId, requestJoinChannelPacket.Password, requestJoinChannelPacket.LevelName, requestJoinChannelPacket.Persistent, 255, requestJoinChannelPacket.Additive);// requestJoinChannelPacket.PlayerLimit);
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
