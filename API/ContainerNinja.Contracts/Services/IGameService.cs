using GNetServer;

namespace ContainerNinja.Contracts.Services
{
    public interface IGameService
    {
        Dictionary<string, string> Users { get; }

        void OnUserJoinedGameServer(string userConnectionId, int playerId, string serverId, string name);

        void OnUserLeftServer(string userConnectionId, int playerId, string serverId);

        void OnGameServerStopped(string serverId);

        void OnSendToClient(NetworkPlayer player, CommandPacket commandPacket);

        void StopAnyUserServer(string userConnectionId);

        void AddNewUserGameServer(string userConnectionId, GameServerInfo newGameServerInfo);

        GameServer GetGameServerByServerId(string serverId);
        
        GameServer GetGameServerByServerName(string serverName);

        void AddUser(string userConnectionId, string userName);

        void RemoveUser(string userConnectionId);

        string GetUserIdByUserName(string userName);

        GameServer GetJoinedGameServer(string userConnectionId);

        List<GameServerInfo> GetGameServerList();
    }
}
