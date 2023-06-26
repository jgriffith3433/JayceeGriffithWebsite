//-------------------------------------------------
//                    GNet 3
// Copyright © 2012-2018 Tasharen Entertainment Inc
//-------------------------------------------------

using System.IO;
using UnityEngine;

namespace GNet
{
    /// <summary>
    /// Server list is a helper class containing a list of servers.
    /// </summary>

    public class ServerList
    {
        public class Entry
        {
            public string name;
            public int playerCount;
            public string serverId;

            [System.NonSerialized] public long recordTime;

            public void WriteTo(BinaryWriter writer)
            {
                writer.Write(name);
                writer.Write((ushort)playerCount);
                writer.Write(serverId);
            }

            public void ReadFrom(BinaryReader reader)
            {
                name = reader.ReadString();
                playerCount = reader.ReadUInt16();
                serverId = reader.ReadString();
            }

            public void ReadFrom(GameServerInfo gameServerInfo)
            {
                name = gameServerInfo.Name;
                playerCount = gameServerInfo.PlayerCount;
                serverId = gameServerInfo.ServerId;
            }
        }

        /// <summary>
        /// List of active server entries. Be sure to lock it before using it,
        /// as it can be changed from a different thread.
        /// </summary>

        public GList<Entry> list = new GList<Entry>();

        static int SortByPC(Entry a, Entry b)
        {
            if (b.playerCount == a.playerCount) return a.name.CompareTo(b.name);
            return b.playerCount.CompareTo(a.playerCount);
        }

        static int SortAlphabetic(Entry a, Entry b) { return a.name.CompareTo(b.name); }

        /// <summary>
        /// Sort the server list, arranging it by the number of players.
        /// </summary>

        public void SortByPlayers() { list.Sort(SortByPC); }

        /// <summary>
        /// Sort the server list, arranging entries alphabetically.
        /// </summary>

        public void SortAlphabetic() { list.Sort(SortAlphabetic); }

        /// <summary>
        /// Add a new entry to the list.
        /// </summary>

        public Entry Add(string name, int playerCount, string connectionId, long time)
        {
            lock (list)
            {
                for (int i = 0; i < list.size; ++i)
                {
                    var ent = list.buffer[i];

                    if (ent.serverId.Equals(connectionId))
                    {
                        ent.name = name;
                        ent.playerCount = playerCount;
                        ent.recordTime = time;
                        return ent;
                    }
                }

                var e = new Entry();
                e.name = name;
                e.playerCount = playerCount;
                e.serverId = connectionId;
                e.recordTime = time;
                list.Add(e);
                return e;
            }
        }

        /// <summary>
        /// Add a new entry.
        /// </summary>

        public Entry Add(Entry newEntry, long time)
        {
            lock (list) AddInternal(newEntry, time);
            return newEntry;
        }

        /// <summary>
        /// Remove an existing entry from the list.
        /// </summary>

        public Entry Remove(string deviceId)
        {
            lock (list)
            {
                for (int i = 0; i < list.size; ++i)
                {
                    var ent = list.buffer[i];

                    if (ent.serverId.Equals(deviceId))
                    {
                        list.RemoveAt(i);
                        return ent;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Remove expired entries.
        /// </summary>

        public bool CleanupAfterTime(long time)
        {
            time -= 7000;
            bool changed = false;

            lock (list)
            {
                for (int i = 0; i < list.size;)
                {
                    var ent = list.buffer[i];

                    if (ent.recordTime < time)
                    {
                        changed = true;
                        list.RemoveAt(i);
                        continue;
                    }
                    ++i;
                }
            }
            return changed;
        }

        public bool Cleanup()
        {
            bool changed = false;

            lock (list)
            {
                for (int i = 0; i < list.size;)
                {
                    var ent = list.buffer[i];
                    changed = true;
                    list.RemoveAt(i);
                }
            }
            return changed;
        }

        /// <summary>
        /// Clear the list of servers.
        /// </summary>

        public void Clear() { lock (list) list.Clear(); }

        /// <summary>
        /// Save the list of servers to the specified binary writer.
        /// </summary>

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(TNSrcLobbyClient.GameId);

            lock (list)
            {
                writer.Write((ushort)list.size);
                for (int i = 0; i < list.size; ++i)
                    list.buffer[i].WriteTo(writer);
            }
        }

        public void ReadFrom(ResponseServerListPacket responseServerListPacket, long time)
        {
            lock (list)
            {
                foreach (var server in responseServerListPacket.Servers)
                {
                    if (server.GameId == GameServer.GameId)
                    {
                        var ent = new Entry();
                        ent.ReadFrom(server);
                        AddInternal(ent, time);
                    }
                }
            }
        }

        /// <summary>
        /// Read a list of servers from the binary reader.
        /// </summary>

        public void ReadFrom(BinaryReader reader, long time)
        {
            if (reader.ReadUInt16() == GameServer.GameId)
            {
                lock (list)
                {
                    int count = reader.ReadUInt16();

                    for (int i = 0; i < count; ++i)
                    {
                        var ent = new Entry();
                        ent.ReadFrom(reader);
                        AddInternal(ent, time);
                    }
                }
            }
        }

        /// <summary>
        /// Add a new entry. Not thread-safe.
        /// </summary>

        void AddInternal(Entry newEntry, long time)
        {
            for (int i = 0; i < list.size; ++i)
            {
                var ent = list.buffer[i];

                if (ent.serverId.Equals(newEntry.serverId))
                {
                    ent.name = newEntry.name;
                    ent.playerCount = newEntry.playerCount;
                    ent.recordTime = time;
                    return;
                }
            }
            newEntry.recordTime = time;
            list.Add(newEntry);
        }
    }
}
