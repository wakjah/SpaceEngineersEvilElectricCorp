using System;
using System.Collections.Generic;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Utils;

namespace EvilElectricCorpMod.Networking
{


    /// <summary>
    /// Simple network communication example.
    /// 
    /// Always send to server as clients can't send to eachother directly.
    /// Then decide in the packet if it should be relayed to everyone else (except sender and server of course).
    /// 
    /// Security note:
    ///  SenderId is not reliable and can be altered by sender to claim they're someone else (like an admin).
    ///  If you need senderId to be secure, a more complicated process is required involving sending
    ///   every player a unique random ID and they sending that ID would confirm their identity.
    /// </summary>
    public class Transceiver<T>
    {
        public readonly ushort ChannelId;

        private List<IMyPlayer> _tempPlayers = null;
        private readonly Func<T, bool> _action;

        /// <summary>
        /// <paramref name="channelId"/> must be unique from all other mods that also use network packets.
        /// </summary>
        public Transceiver(ushort channelId, Func<T, bool> action)
        {
            ChannelId = channelId;
            _action = action;
        }

        /// <summary>
        /// Register packet monitoring, not necessary if you don't want the local machine to handle incomming packets.
        /// </summary>
        public void Register()
        {
            MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(ChannelId, ReceivedPacket);
        }

        /// <summary>
        /// This must be called on world unload if you called <see cref="Register"/>.
        /// </summary>
        public void Unregister()
        {
            MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(ChannelId, ReceivedPacket);
        }

        private void ReceivedPacket(ushort comId, byte[] rawData, ulong playerId, bool relible) // executed when a packet is received on this machine
        {
            try
            {
                var packet = MyAPIGateway.Utilities.SerializeFromBinary<T>(rawData);

                HandlePacket(packet, playerId, rawData);
            }
            catch (Exception e)
            {
                // Handle packet receive errors however you prefer, this is with logging. Remove try-catch to allow it to crash the game.
                // If another mod uses the same channel as your mod, this will throw errors being unable to deserialize their stuff.
                // In that case, one of you must change the channelId and NOT ignoring the error as it can noticeably impact performance.

                MyLog.Default.WriteLineAndConsole($"{e.Message}\n{e.StackTrace}");

                if (MyAPIGateway.Session?.Player != null)
                    MyAPIGateway.Utilities.ShowNotification($"[ERROR: {GetType().FullName}: {e.Message} | Send SpaceEngineers.Log to mod author]", 10000, MyFontEnum.Red);
            }
        }

        private void HandlePacket(T packet, ulong playerId, byte[] rawData = null)
        {
            var relay = _action.Invoke(packet);

            if (relay)
                RelayToClients(packet, playerId, rawData);
        }

        /// <summary>
        /// Send a packet to the server.
        /// Works from clients and server.
        /// </summary>
        public void SendToServer(T packet)
        {
            if (MyAPIGateway.Multiplayer.IsServer)
            {
                HandlePacket(packet, MyAPIGateway.Multiplayer.ServerId);
                return;
            }

            var bytes = MyAPIGateway.Utilities.SerializeToBinary(packet);

            MyAPIGateway.Multiplayer.SendMessageToServer(ChannelId, bytes);
        }

        /// <summary>
        /// Send a packet to a specific player.
        /// Only works server side.
        /// </summary>
        public void SendToPlayer(T packet, ulong steamId)
        {
            if (!MyAPIGateway.Multiplayer.IsServer)
                return;

            var bytes = MyAPIGateway.Utilities.SerializeToBinary(packet);

            MyAPIGateway.Multiplayer.SendMessageTo(ChannelId, bytes, steamId);
        }

        /// <summary>
        /// Send a packet to all players.
        /// Only works server side.
        /// </summary>
        public void SendToAllPlayers(T packet)
        {
            Broadcast(packet, false);
        }

        /// <summary>
        /// Send a packet to all connected.
        /// Only works server side.
        /// </summary>
        public void Broadcast(T packet, bool includeServer = true)
        {
            if (!MyAPIGateway.Multiplayer.IsServer)
                return;

            var bytes = MyAPIGateway.Utilities.SerializeToBinary(packet);

            if (_tempPlayers == null)
                _tempPlayers = new List<IMyPlayer>(MyAPIGateway.Session.SessionSettings.MaxPlayers);
            else
                _tempPlayers.Clear();

            MyAPIGateway.Players.GetPlayers(_tempPlayers);

            foreach (var p in _tempPlayers)
            {
                if (p.IsBot)
                    continue;

                if (p.SteamUserId == MyAPIGateway.Multiplayer.ServerId && !includeServer)
                    continue;

                MyAPIGateway.Multiplayer.SendMessageTo(ChannelId, bytes, p.SteamUserId);
            }
        }

        /// <summary>
        /// Sends packet (or supplied bytes) to all players except server player and supplied packet's sender.
        /// Only works server side.
        /// </summary>
        public void RelayToClients(T packet, ulong senderId, byte[] rawData = null)
        {
            if (!MyAPIGateway.Multiplayer.IsServer)
                return;

            if (_tempPlayers == null)
                _tempPlayers = new List<IMyPlayer>(MyAPIGateway.Session.SessionSettings.MaxPlayers);
            else
                _tempPlayers.Clear();

            MyAPIGateway.Players.GetPlayers(_tempPlayers);

            foreach (var p in _tempPlayers)
            {
                if (p.IsBot)
                    continue;

                if (p.SteamUserId == MyAPIGateway.Multiplayer.ServerId)
                    continue;

                if (p.SteamUserId == senderId)
                    continue;

                if (rawData == null)
                    rawData = MyAPIGateway.Utilities.SerializeToBinary(packet);

                MyAPIGateway.Multiplayer.SendMessageTo(ChannelId, rawData, p.SteamUserId);
            }

            _tempPlayers.Clear();
        }
    }
}
