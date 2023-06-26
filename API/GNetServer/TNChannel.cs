//-------------------------------------------------
//                    GNetServer 3
// Copyright © 2012-2018 Tasharen Entertainment Inc
//-------------------------------------------------

using System.IO;
#if UNITY_EDITOR
using UnityEngine;
#endif

namespace GNetServer
{
	/// <summary>
	/// A channel contains one or more players.
	/// All information broadcast by players is visible by others in the same channel.
	/// </summary>

	public class Channel : DataNodeContainer
	{
		/// <summary>
		/// If set, the channel data will be kept in a low-memory footprint format rather than in their de-serialized state.
		/// Low memory footprint also makes the saving process take less time. The downside is that the process of joining and leaving channels is a bit slower.
		/// </summary>

		[System.NonSerialized]
		static public bool lowMemoryFootprint = true;

		/// <summary>
		/// Remote function call entry stored within the channel.
		/// </summary>

		public struct RFC
		{
			// Object ID (24 bytes), RFC ID (8 bytes)
			public uint uid;
			public string functionName;
			public Buffer data;

			public uint objectID { get { return (uid >> 8); } set { uid = ((value << 8) | (uid & 0xFF)); } }
			public uint functionID { get { return (uid & 0xFF); } }

			public void Recycle ()
			{
				if (data != null)
				{
					data.Recycle();
					data = null;
				}
			}

			public void ReplaceData (Buffer data)
			{
				if (this.data != null) this.data.Recycle();
				this.data = data;
			}

			public bool Matches (uint uid, ref string funcName) { return uid == this.uid && funcName == functionName; }

			/// <summary>
			/// Write a complete ForwardToOthers packet to the specified buffer.
			/// </summary>

//			public void WritePacket (int channelID, Buffer buffer)
//			{
//#if !MODDING
//				var writer = buffer.BeginPacket(Packet.ForwardToOthers);
//				writer.Write(0);
//				writer.Write(channelID);
//				writer.Write(uid);
//				if (functionID == 0) writer.Write(functionName);
//				writer.Write(data.buffer, 0, data.size);
//				buffer.EndPacket();
//#endif
//			}
		}

		/// <summary>
		/// Created objects are saved by the channels.
		/// </summary>

		public struct CreatedObject
		{
			public int playerId;
			public uint objectId;
			//1 = persistent, 2 = not persistent
			public byte type;
			public Buffer data;

			public void Recycle () { if (data != null) { data.Recycle(); data = null; } }
		}

		/// <summary>
		/// Channel information class created as a result of retrieving a list of channels.
		/// </summary>

		public struct Info
		{
			public int id;              // Channel's ID
			public ushort players;      // Number of players present
			public ushort limit;        // Player limit
			public bool hasPassword;    // Whether the channel is password-protected or not
			public bool isPersistent;   // Whether the channel is persistent or not
			public string level;        // Name of the loaded level
			public DataNode data;       // Data associated with the channel
		}

		public int id;
		public string password = "";
		public string level = "";
		public bool isPersistent = false;
		public bool isClosed = false;
		public bool isLocked = false;
		public bool isLeaving = false;
		public ushort playerLimit = 65535;
		public GList<Player> players = new GList<Player>();
		public GList<RFC> rfcs = new GList<RFC>();
		public GList<CreatedObject> created = new GList<CreatedObject>();
		public GList<uint> destroyed = new GList<uint>();
		public uint objectCounter = 0xFFFFFF;
		public Player host;

		[System.Obsolete("Rename to 'isPersistent'")]
		public bool persistent { get { return isPersistent; } set { isPersistent = value; } }

		[System.Obsolete("Rename to 'isClosed'")]
		public bool closed { get { return isClosed; } set { isClosed = value; } }

		// Key = Object ID. Value is 'true'. This dictionary is used for a quick lookup checking to see
		// if the object actually exists. It's used to store RFCs. RFCs for objects that don't exist are not stored.
		[System.NonSerialized]
		System.Collections.Generic.Dictionary<uint, bool> mCreatedObjectDictionary = new System.Collections.Generic.Dictionary<uint, bool>();

		// Channel data is not parsed until it's actually needed, saving memory
		byte[] mSource;
#if !MODDING
		int mSourceSize;
#endif
		/// <summary>
		/// Whether the channel has data that can be saved.
		/// </summary>

		public bool hasData { get { return rfcs.size > 0 || created.size > 0 || destroyed.size > 0 || dataNode != null || mSource != null; } }

		/// <summary>
		/// Whether the channel can be joined.
		/// </summary>

		public bool isOpen { get { return !isClosed && players.size < playerLimit; } }

		/// <summary>
		/// Helper function that returns a new unique ID that's not currently used by any object.
		/// </summary>

		public uint GetUniqueID ()
		{
			for (; ; )
			{
				uint uniqueID = --objectCounter;

				// 1-32767 is reserved for existing scene objects.
				// 32768 - 16777215 is for dynamically created objects.
				if (uniqueID < 32768)
				{
					objectCounter = 0xFFFFFF;
					uniqueID = 0xFFFFFF;
				}

				// Ensure that this object ID is not already in use
				if (!mCreatedObjectDictionary.ContainsKey(uniqueID))
					return uniqueID;
			}
		}

		/// <summary>
		/// Add a new created object to the list. This object's ID must always be above 32767.
		/// </summary>

		public void AddCreatedObject (CreatedObject obj)
		{
			created.Add(obj);
			mCreatedObjectDictionary[obj.objectId] = true;
		}

		/// <summary>
		/// Return a player with the specified ID.
		/// </summary>

		public Player GetPlayer (int pid)
		{
			for (int i = 0; i < players.size; ++i)
			{
				var p = players.buffer[i];
				if (p.id == pid) return p;
			}
			return null;
		}

		/// <summary>
		/// Remove the player with the specified ID.
		/// </summary>

		public bool RemovePlayer (int pid)
		{
			for (int i = 0; i < players.size; ++i)
			{
				var p = players.buffer[i];

				if (p.id == pid)
				{
					players.RemoveAt(i);
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Reset the channel to its initial state.
		/// </summary>

		public void Reset ()
		{
			for (int i = 0; i < rfcs.size; ++i) rfcs.buffer[i].Recycle();
			for (int i = 0; i < created.size; ++i) created.buffer[i].Recycle();

			rfcs.Clear();
			created.Clear();
			destroyed.Clear();
			mCreatedObjectDictionary.Clear();
			objectCounter = 0xFFFFFF;
			mSource = null;
		}

		/// <summary>
		/// Remove the specified player from the channel.
		/// </summary>

		public void RemovePlayer (ServerPlayer p, System.Collections.Generic.List<uint> destroyedObjects)
		{
#if !MODDING
			destroyedObjects.Clear();

			if (players.Remove(p))
			{
				// When the host leaves, clear the host (it gets changed in SendLeaveChannel)
				if (p == host) host = null;

				// Remove all of the non-persistent objects that were created by this player
				for (int i = 0; i < created.size;)
				{
					if (created.buffer[i].playerId == p.id)
					{
						if (created.buffer[i].type == 2)
						{
							created.buffer[i].Recycle();
							uint objID = created.buffer[i].objectId;
							created.RemoveAt(i);
							destroyedObjects.Add(objID);
							if (objID >= 32768) mCreatedObjectDictionary.Remove(objID);
							DestroyObjectRFCs(objID);
							continue;
						}

						// The same operation happens on the client as well
						if (players.size != 0) created.buffer[i].playerId = players.buffer[0].id;
					}
					++i;
				}

				// Close the channel if it wasn't persistent
				if ((!isPersistent || playerLimit < 1) && players.size == 0)
				{
					isClosed = true;
					for (int i = 0; i < rfcs.size; ++i) rfcs.buffer[i].Recycle();
					rfcs.Clear();
				}
			}
#endif
		}

		/// <summary>
		/// Export the specified object, writing its RCC and RFCs into the binary writer. Only dynamically created objects can be exported.
		/// </summary>

		public bool ExportObject (uint objID, BinaryWriter writer)
		{
#if !MODDING
			if (objID < 32768) return false;

			if (mCreatedObjectDictionary.ContainsKey(objID))
			{
				for (int i = 0; i < created.size; ++i)
				{
					var co = created.buffer[i];
					if (co.objectId != objID) continue;

					writer.Write(co.data.size);

					if (co.data.size > 0) writer.Write(co.data.buffer, co.data.position, co.data.size);

					var count = 0;

					for (int r = 0; r < rfcs.size; ++r)
					{
						var rfc = rfcs.buffer[r];
						if (rfc.objectID == objID) ++count;
					}

					writer.Write(count);

					if (count != 0)
					{
						for (int r = 0; r < rfcs.size; ++r)
						{
							var rfc = rfcs.buffer[r];
							if (rfc.objectID != objID) continue;

							writer.Write(rfc.uid);
							if (rfc.functionID == 0) writer.Write(rfc.functionName);

							if (rfc.data != null)
							{
								writer.Write(rfc.data.size);
								if (rfc.data.size > 0) writer.Write(rfc.data.buffer, rfc.data.position, rfc.data.size);
							}
							else writer.Write(0);
						}
					}
					return true;
				}
			}
			else if (mForward.size != 0)
			{
				for (int i = 0; i < mForward.size; ++i)
				{
					if (mForward.buffer[i].objectID == objID && mForward.buffer[i].newChannel != null)
						return mForward.buffer[i].newChannel.ExportObject(mForward.buffer[i].newID, writer);
				}
			}
#endif
			return false;
		}

		/// <summary>
		/// Import a previously exported object. Returns its object ID, or '0' if failed.
		/// </summary>

//		public uint ImportObject (int playerId, BinaryReader reader)
//		{
//#if !MODDING
//			// Create a new object and read its RCC data
//			var co = new CreatedObject();
//			co.objectId = GetUniqueID();
//			co.type = 1;
//			co.data = Buffer.Create();
//			var bytes = reader.ReadBytes(reader.ReadInt32());
//			co.data.BeginWriting(false).Write(bytes);
//			co.data.EndWriting();
//			AddCreatedObject(co);
//			SendToAll(new SendResponseCreateObjectPacket(playerId, id, co.objectId, bytes));

//			// Now read all the RFCs
//			var size = reader.ReadInt32();

//			if (size != 0)
//			{
//				for (int i = 0; i < size; ++i)
//				{
//					var rfc = new RFC();
//					rfc.uid = reader.ReadUInt32();
//					rfc.objectID = co.objectId;
//					if (rfc.functionID == 0) rfc.functionName = reader.ReadString();
//					bytes = reader.ReadBytes(reader.ReadInt32());

//					var b = Buffer.Create();
//					b.BeginWriting(false).Write(bytes);
//					b.EndWriting();
//					rfc.data = b;
//					rfcs.Add(rfc);

//					packet = Buffer.Create();
//					rfc.WritePacket(id, packet);
//					SendToAll(packet);
//					packet.Recycle();
//				}
//			}
//			return co.objectId;
//#else
//			return 0;
//#endif
//		}

		/// <summary>
		/// Send the specified packet to all players in the channel.
		/// </summary>

		public void SendToAll (CommandPacket sendCommandPacket)
		{
			for (int i = 0; i < players.size; ++i)
			{
				var p = players.buffer[i] as ServerPlayer;
				p.SendPacket(sendCommandPacket);
			}
		}

		/// <summary>
		/// Change the object's associated player. Only works with dynamically instantiated objects.
		/// </summary>

		public bool ChangeObjectOwner (uint objID, int playerID)
		{
#if !MODDING
			if (objID < 32768) return false;

			if (mCreatedObjectDictionary.ContainsKey(objID))
			{
				for (int i = 0; i < created.size; ++i)
				{
					if (created.buffer[i].objectId == objID)
					{
						created.buffer[i].playerId = playerID;
						return true;
					}
				}
			}
#endif
			return false;
		}

		/// <summary>
		/// Remove an object with the specified unique identifier.
		/// </summary>

		public bool DestroyObject (uint objID)
		{
#if !MODDING
			if (objID < 32768)
			{
				// Static objects have ID below 32768
				if (!destroyed.Contains(objID))
				{
					destroyed.Add(objID);
					DestroyObjectRFCs(objID);
					return true;
				}
			}
			else if (mCreatedObjectDictionary.Remove(objID))
			{
				// Dynamic objects are always a part of the 'created' array and the lookup table
				for (int i = 0; i < created.size; ++i)
				{
					var obj = created.buffer[i];

					if (obj.objectId == objID)
					{
						obj.Recycle();
						created.RemoveAt(i);
						DestroyObjectRFCs(objID);
						return true;
					}
				}
			}
#endif
			return false;
		}

		/// <summary>
		/// Delete the specified remote function call.
		/// </summary>

		public void DestroyObjectRFCs (uint objectID)
		{
#if !MODDING
			for (int i = rfcs.size; i > 0;)
			{
				if (rfcs.buffer[--i].objectID == objectID)
				{
					rfcs.buffer[i].Recycle();
					rfcs.RemoveAt(i);
				}
			}
#endif
		}

#if !MODDING
		struct ForwardRecord
		{
			public uint objectID;
			public Channel newChannel;
			public uint newID;
			public long expiration;
		}

		GList<ForwardRecord> mForward = null;

		void AddForwardRecord (uint objectID, Channel newChannel, uint newID, long expiration)
		{
			var fw = new ForwardRecord();
			fw.objectID = objectID;
			fw.newChannel = newChannel;
			fw.newID = newID;
			fw.expiration = expiration;
			if (mForward == null) mForward = new GList<ForwardRecord>();
			mForward.Add(fw);
		}
#endif

		/// <summary>
		/// Transfer the specified object to another channel, changing its Object ID in the process.
		/// </summary>

		public CreatedObject? TransferObject (uint objectID, Channel newChannel, long time)
		{
#if !MODDING
			if (objectID < 32768)
			{
				Tools.LogError("Transferring objects only works with objects that were instantiated at run-time.");
			}
			else if (mCreatedObjectDictionary.Remove(objectID))
			{
				for (int i = 0; i < created.size; ++i)
				{
					var obj = created.buffer[i];

					if (obj.objectId == objectID)
					{
						// Move the created object over to the other channel
						obj.objectId = newChannel.GetUniqueID();

						// Add a new forward record for 10 seconds so that any packets that arrive for this object will automatically get redirected
						AddForwardRecord(objectID, newChannel, obj.objectId, time + 10000);

						// If the other channel doesn't contain the object's owner, assign a new owner
						bool changeOwner = true;

						for (int b = 0; b < newChannel.players.size; ++b)
						{
							if (newChannel.players.buffer[b].id == obj.playerId)
							{
								changeOwner = false;
								break;
							}
						}

						if (changeOwner) obj.playerId = (newChannel.host != null) ? newChannel.host.id : 0;

						created.RemoveAt(i);
						newChannel.created.Add(obj);
						newChannel.mCreatedObjectDictionary[obj.objectId] = true;

						// Move RFCs over to the other channel
						for (int b = 0; b < rfcs.size;)
						{
							if (rfcs.buffer[b].objectID == objectID)
							{
								rfcs.buffer[b].objectID = obj.objectId;
								newChannel.rfcs.Add(rfcs.buffer[b]);
								rfcs.RemoveAt(b);
							}
							else ++b;
						}
						return obj;
					}
				}
			}
#endif
			return null;
		}

		/// <summary>
		/// Add a new saved remote function call.
		/// </summary>

		public void AddRFC (uint uid, string funcName, Buffer buffer, long time)
		{
#if !MODDING
			if (isClosed || buffer == null) return;
			uint objID = (uid >> 8);

			if (objID < 32768) // Static object ID
			{
				// Ignore objects that were marked as deleted
				if (destroyed.Contains(objID)) return;
			}
			else if (!mCreatedObjectDictionary.ContainsKey(objID))
			{
				if (mForward != null)
				{
					for (int i = 0; i < mForward.size; ++i)
					{
						if (mForward.buffer[i].objectID == objID)
						{
							// Redirect this packet
							mForward.buffer[i].newChannel.AddRFC((mForward.buffer[i].newID << 8) | (uid & 0xFF), funcName, buffer, time);
							return;
						}
						else if (mForward.buffer[i].expiration < time)
						{
							// Expired entry -- remove it
							mForward.RemoveAt(i--);
							if (mForward.size == 0) { mForward = null; break; }
						}
					}
				}
				return; // This object doesn't exist
			}

			var b = Buffer.Create();
			b.BeginWriting(false).Write(buffer.buffer, buffer.position, buffer.size);
			b.EndWriting();

			for (int i = 0; i < rfcs.size; ++i)
			{
				if (rfcs.buffer[i].Matches(uid, ref funcName))
				{
					var r = rfcs.buffer[i];
					r.ReplaceData(b);

					// Move this RFC to the end of the list so that it gets called in correct order on load
					rfcs.RemoveAt(i);
					rfcs.Add(r);
					return;
				}
			}

			var rfc = new RFC();
			rfc.uid = uid;
			rfc.functionName = funcName;
			rfc.data = b;
			rfcs.Add(rfc);
#endif
		}

		/// <summary>
		/// Delete the specified remote function call.
		/// </summary>

		public void DeleteRFC (uint uid, string funcName, long time)
		{
#if !MODDING
			for (int i = 0; i < rfcs.size; ++i)
			{
				if (rfcs.buffer[i].Matches(uid, ref funcName))
				{
					rfcs.buffer[i].Recycle();
					rfcs.RemoveAt(i);
					return;
				}
			}

			if (mForward != null)
			{
				uint objID = (uid >> 8);

				for (int i = 0; i < mForward.size; ++i)
				{
					if (mForward.buffer[i].objectID == objID)
					{
						// Redirect this packet
						mForward.buffer[i].newChannel.DeleteRFC((mForward.buffer[i].newID << 8) | (uid & 0xFF), funcName, time);
						return;
					}
					else if (mForward.buffer[i].expiration < time)
					{
						// Expired entry -- remove it
						mForward.RemoveAt(i--);
						if (mForward.size == 0) { mForward = null; break; }
					}
				}
			}
#endif
		}

		// Cached to reduce memory allocations
		[System.NonSerialized] GList<uint> mCleanedOBJs = new GList<uint>();
		[System.NonSerialized] GList<CreatedObject> mCreatedOBJs = new GList<CreatedObject>();
		[System.NonSerialized] GList<RFC> mCreatedRFCs = new GList<RFC>();

		/// <summary>
		/// Save the channel's data into the specified file.
		/// </summary>

		public void SaveTo (BinaryWriter writer)
		{
#if !MODDING
			if (mSource != null)
			{
				writer.Write(mSource, 0, mSourceSize);
				return;
			}

			writer.Write(Player.version);
			writer.Write(level);
			writer.Write(dataNode);
			writer.Write(objectCounter);
			//writer.Write(password);
			writer.Write(isPersistent);
			writer.Write(playerLimit);

			// Record which objects are temporary and which ones are not
			for (int i = 0; i < created.size; ++i)
			{
				var co = created.buffer[i];

				if (co.type == 1)
				{
					mCreatedOBJs.Add(co);
					mCleanedOBJs.Add(co.objectId);
				}
			}

			// Record all RFCs that don't belong to temporary objects
			for (int i = 0; i < rfcs.size; ++i)
			{
				var objID = rfcs.buffer[i].objectID;

				if (objID < 32768)
				{
					mCreatedRFCs.Add(rfcs.buffer[i]);
				}
				else
				{
					for (int b = 0; b < mCleanedOBJs.size; ++b)
					{
						if (mCleanedOBJs.buffer[b] == objID)
						{
							mCreatedRFCs.Add(rfcs.buffer[i]);
							break;
						}
					}
				}
			}

			writer.Write(mCreatedRFCs.size);

			for (int i = 0; i < mCreatedRFCs.size; ++i)
			{
				var rfc = mCreatedRFCs.buffer[i];
				writer.Write(rfc.uid);
				if (rfc.functionID == 0) writer.Write(rfc.functionName);
				writer.Write(rfc.data.size);
				if (rfc.data.size > 0) writer.Write(rfc.data.buffer, rfc.data.position, rfc.data.size);
			}

			writer.Write(mCreatedOBJs.size);

			for (int i = 0; i < mCreatedOBJs.size; ++i)
			{
				var co = mCreatedOBJs.buffer[i];
				writer.Write(co.playerId);
				writer.Write(co.objectId);
				writer.Write(co.data.size);
				if (co.data.size > 0) writer.Write(co.data.buffer, co.data.position, co.data.size);
			}

			writer.Write(destroyed.size);
			for (int i = 0; i < destroyed.size; ++i) writer.Write(destroyed.buffer[i]);

			mCleanedOBJs.Clear();
			mCreatedOBJs.Clear();
			mCreatedRFCs.Clear();

			writer.Write(isLocked);
#endif
		}

		/// <summary>
		/// Load the channel's data from the specified file.
		/// </summary>

		public bool LoadFrom (BinaryReader reader, bool keepInMemory = false)
		{
#if !MODDING
			var start = reader.BaseStream.Position;
			int version = reader.ReadInt32();

			if (version < 20160207)
			{
#if UNITY_EDITOR
				UnityEngine.Debug.LogWarning("Incompatible data: " + version);
#endif
				return false;
			}

			// Clear all RFCs, just in case
			for (int i = 0; i < rfcs.size; ++i) rfcs.buffer[i].Recycle();

			rfcs.Clear();
			created.Clear();
			destroyed.Clear();
			mCreatedObjectDictionary.Clear();

			level = reader.ReadString();
			dataNode = reader.ReadDataNode();
			objectCounter = reader.ReadUInt32();
			//password = reader.ReadString();
			isPersistent = reader.ReadBoolean();
			playerLimit = reader.ReadUInt16();

			int size = reader.ReadInt32();

			for (int i = 0; i < size; ++i)
			{
				var rfc = new RFC();
				rfc.uid = reader.ReadUInt32();
				if (rfc.functionID == 0) rfc.functionName = reader.ReadString();
				var b = Buffer.Create();
				b.BeginWriting(false).Write(reader.ReadBytes(reader.ReadInt32()));
				b.EndWriting();
				rfc.data = b;
				rfcs.Add(rfc);
			}

			size = reader.ReadInt32();

			for (int i = 0; i < size; ++i)
			{
				var co = new CreatedObject();
				co.playerId = reader.ReadInt32();
				co.objectId = reader.ReadUInt32();
				co.playerId = 0; // The player ID is no longer valid as player IDs reset on reload
				co.type = 1;

				var b = Buffer.Create();
				b.BeginWriting(false).Write(reader.ReadBytes(reader.ReadInt32()));
				b.EndWriting();
				co.data = b;
				AddCreatedObject(co);
			}

			size = reader.ReadInt32();

			for (int i = 0; i < size; ++i)
			{
				uint uid = reader.ReadUInt32();
				if (uid < 32768) destroyed.Add(uid);
			}

			isLocked = reader.ReadBoolean();
			mSource = null;

#if STANDALONE
			if (!keepInMemory && players.size == 0)
			{
				Reset();
				var end = reader.BaseStream.Position;
				reader.BaseStream.Position = start;
				mSourceSize = (int)(end - start);
				mSource = reader.ReadBytes(mSourceSize);
			}
#endif
#endif
			return true;
		}

		/// <summary>
		/// When channels have no players in them, they can be put to sleep in order to reduce the server's memory footprint.
		/// </summary>

		public void Sleep ()
		{
#if !MODDING
#if STANDALONE
			if (lowMemoryFootprint && players.size == 0 && mSource == null)
			{
				var ms = new MemoryStream();
				var writer = new BinaryWriter(ms);
				SaveTo(writer);
				Reset();
				mSourceSize = (int)ms.Position;

				if (mSourceSize > 0)
				{
					mSource = ms.GetBuffer();
					System.Array.Resize(ref mSource, mSourceSize);
				}
			}
#else
			mSourceSize = 0;
#endif
#endif
		}

		/// <summary>
		/// Ensure that the channel's data has been loaded.
		/// </summary>

		public void Wake ()
		{
#if !MODDING
			if (mSource != null)
			{
				var stream = new MemoryStream(mSource);
				var reader = new BinaryReader(stream);
				LoadFrom(reader, true);
				reader.Close();
				mSource = null;
			}
#endif
		}
	}
}
