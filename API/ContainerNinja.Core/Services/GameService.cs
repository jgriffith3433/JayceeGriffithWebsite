using GNetServer;
using ContainerNinja.Contracts.Services;
using Microsoft.AspNetCore.SignalR;
using ContainerNinja.Core.Services;

namespace ContainerNinja.Services
{
    public class GameService : IGameService
    {
        private readonly IHubContext<GameHub> _hubContext;

        public GameService(IHubContext<GameHub> hubContext)
        {
            _hubContext = hubContext;
        }

        private Dictionary<string, string> m_UserGameServers = new Dictionary<string, string>();

        private Dictionary<string, string> m_UserJoinedGameServers = new Dictionary<string, string>();

        private Dictionary<string, GameServer> m_GameServers = new Dictionary<string, GameServer>();

        public Dictionary<string, string> Users { get; private set; } = new Dictionary<string, string>();

        public void OnUserJoinedGameServer(string userConnectionId, int playerId, string serverId)
        {
            if (!m_UserJoinedGameServers.ContainsKey(userConnectionId))
            {
                m_UserJoinedGameServers.Add(userConnectionId, serverId);
            }
            var userClient = _hubContext.Clients.Client(userConnectionId);

            if (userClient != null)
            {
                var responseJoinServerPacket = new ResponseJoinServerPacket(serverId, playerId, true);
                userClient.SendAsync(responseJoinServerPacket.GetHubTarget(), responseJoinServerPacket);
            }
        }

        public void OnUserLeftServer(string userConnectionId, int playerId, string serverId)
        {
            if (m_UserJoinedGameServers.ContainsKey(userConnectionId))
            {
                m_UserJoinedGameServers.Remove(userConnectionId);
            }

            var userClient = _hubContext.Clients.Client(userConnectionId);
            if (userClient != null)
            {
                var responseLeaveServerPacket = new ResponseLeaveServerPacket(serverId, playerId, true);
                userClient.SendAsync(responseLeaveServerPacket.GetHubTarget(), responseLeaveServerPacket);
            }
        }

        public void OnGameServerStopped(string serverId)
        {
            if (m_GameServers.ContainsKey(serverId))
            {
                m_GameServers.Remove(serverId);
            }
            var newUserGameServers = new Dictionary<string, string>();
            foreach (var userGameServerKvp in m_UserGameServers)
            {
                if (userGameServerKvp.Value != serverId)
                {
                    newUserGameServers.Add(userGameServerKvp.Key, userGameServerKvp.Value);
                }
            }
            m_UserGameServers = newUserGameServers;
            var newUserJoinedGameServers = new Dictionary<string, string>();
            foreach (var userJoinedGameServerKvp in m_UserJoinedGameServers)
            {
                if (userJoinedGameServerKvp.Value != serverId)
                {
                    newUserJoinedGameServers.Add(userJoinedGameServerKvp.Key, userJoinedGameServerKvp.Value);
                }
            }
            m_UserJoinedGameServers = newUserJoinedGameServers;
        }

        public void OnSendToClient(NetworkPlayer player, CommandPacket commandPacket)
        {
            var userClient = _hubContext.Clients.Client(player.ConnectionId);
            if (userClient != null)
            {
                userClient.SendAsync(commandPacket.GetHubTarget(), commandPacket);
            }
        }

        public void StopAnyUserServer(string userConnectionId)
        {
            if (m_UserGameServers.ContainsKey(userConnectionId))
            {
                if (m_GameServers.ContainsKey(m_UserGameServers[userConnectionId]))
                {
                    m_GameServers[m_UserGameServers[userConnectionId]].Stop();
                }
                else
                {
                    var newUserGameServers = new Dictionary<string, string>();
                    foreach (var userGameServerKvp in m_UserGameServers)
                    {
                        if (userGameServerKvp.Value != m_UserGameServers[userConnectionId])
                        {
                            newUserGameServers.Add(userGameServerKvp.Key, userGameServerKvp.Value);
                        }
                    }
                    m_UserGameServers = newUserGameServers;
                    var newUserJoinedGameServers = new Dictionary<string, string>();
                    foreach (var userJoinedGameServerKvp in m_UserJoinedGameServers)
                    {
                        if (userJoinedGameServerKvp.Value != m_UserGameServers[userConnectionId])
                        {
                            newUserJoinedGameServers.Add(userJoinedGameServerKvp.Key, userJoinedGameServerKvp.Value);
                        }
                    }
                    m_UserJoinedGameServers = newUserJoinedGameServers;
                }
                m_UserGameServers.Remove(userConnectionId);
            }
        }

        public async void AddNewUserGameServer(string userConnectionId, GameServerInfo newGameServerInfo)
        {
            var newGameServer = new GameServer();
            newGameServer.GameServerInfo = newGameServerInfo;
            newGameServer.onShutdown += OnGameServerStopped;
            newGameServer.onPlayerDisconnect += OnUserLeftServer;
            newGameServer.onPlayerConnect += OnUserJoinedGameServer;
            newGameServer.onSendToClient += OnSendToClient;

            m_UserGameServers.Add(userConnectionId, newGameServer.GameServerInfo.ServerId);
            m_GameServers.Add(newGameServer.GameServerInfo.ServerId, newGameServer);

            var responseNewServerIdPacket = new ResponseNewGameServerPacket();
            responseNewServerIdPacket.ServerId = newGameServer.GameServerInfo.ServerId;
            await _hubContext.Clients.Client(userConnectionId).SendAsync(responseNewServerIdPacket.GetHubTarget(), responseNewServerIdPacket);
        }

        public GameServer GetGameServerByServerId(string serverId)
        {
            if (m_GameServers.ContainsKey(serverId))
            {
                return m_GameServers[serverId];
            }
            return null;
        }

        public GameServer GetGameServerByServerName(string serverName)
        {
            foreach(var serverKvp in m_GameServers)
            {
                if (serverKvp.Value.GameServerInfo.Name.ToLower() == serverName.ToLower())
                {
                    return serverKvp.Value;
                }
            }
            return null;
        }

        public void AddUser(string userConnectionId, string userName)
        {
            if (!Users.ContainsKey(userConnectionId))
            {
                Users.Add(userConnectionId, userName);
            }
        }

        public void RemoveUser(string userConnectionId)
        {
            if (Users.ContainsKey(userConnectionId))
            {
                Users.Remove(userConnectionId);
            }
        }

        public string GetUserIdByUserName(string userName)
        {
            foreach (var userKvp in Users)
            {
                if (userKvp.Value.ToLower() == userName.ToLower())
                {
                    return userKvp.Key;
                }
            }
            return null;
        }

        public GameServer GetJoinedGameServer(string userConnectionId)
        {
            if (m_UserJoinedGameServers.ContainsKey(userConnectionId))
            {
                if (m_GameServers.ContainsKey(m_UserJoinedGameServers[userConnectionId]))
                {
                    return m_GameServers[m_UserJoinedGameServers[userConnectionId]];
                }
            }
            return null;
        }

        public List<GameServerInfo> GetGameServerList()
        {
            var servers = new List<GameServerInfo>();
            foreach (var gameServerKvp in m_GameServers)
            {
                servers.Add(gameServerKvp.Value.GameServerInfo);
            }
            return servers;
        }
    }
}
