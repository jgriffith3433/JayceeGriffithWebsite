using System;

namespace GNetServer
{
    public class ServerPlayer : NetworkPlayer
    {
        public Action<NetworkPlayer, CommandPacket> SendToClient;

        public ServerPlayer(string name) : base(name)
        {
            this.name = name;
            gameServerStage = Stage.Connected;
        }

        public override void SendPacket(CommandPacket commandPacket)
        {
            SendToClient?.Invoke(this, commandPacket);
        }

        /// <summary>
        /// Assign an ID to this player.
        /// </summary>

        public override void AssignID()
        {
            if (id == 0)
            {
                lock (mLock)
                {
                    id = ++mPlayerCounter;
                }
            }
        }
    }
}
