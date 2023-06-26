using GNetServer;
using System.Collections.Generic;

namespace ContainerNinja.Services
{
    public interface IGameService
    {
        void OnUserJoinedGameServer(string userConnectionId, int playerId, string serverId);

        void OnUserLeftServer(string userConnectionId, int playerId, string serverId);

        void OnGameServerStopped(string serverId);

        void OnSendToClient(NetworkPlayer player, CommandPacket commandPacket);

        void StopAnyUserServer(string userConnectionId);

        void AddNewUserGameServer(string userConnectionId, GameServer newGameServer);

        GameServer GetGameServerByServerId(string serverId);

        GameServer GetJoinedGameServer(string userConnectionId);

        List<GameServerInfo> GetGameServerList();
    }
}
