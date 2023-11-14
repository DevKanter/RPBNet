using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RPBNet.NetworkBase.Connections;
using RPBPacketBase;
using RPBUtilities;

namespace RPBNet.NetworkBase.Server
{
    public static class PacketParser
    {
        private static readonly Dictionary<int, Func<ByteBuffer, RPBPacket>> _packetFactory =
            new Dictionary<int, Func<ByteBuffer, RPBPacket>>();

        private static readonly Dictionary<int, Action<RPBPacket, IConnection>> _actions =
            new Dictionary<int, Action<RPBPacket, IConnection>>();

        public static void AddPacketFactory<T>() where T : RPBPacket, new()
        {
            var obj = Activator.CreateInstance<T>();
            _packetFactory.Add(obj.PacketId, buffer => obj.Deserialize<T>(buffer));
        }

        public static void RegisterPacketAction<T>(Action<T, IConnection> action) where T : RPBPacket
        {
            var obj = Activator.CreateInstance<T>();
            _actions.Add(obj.PacketId, Unsafe.As<Action<RPBPacket, IConnection>>(action));
        }

        public static bool Parse(ByteBuffer buffer, IConnection connection, out Action action)
        {
            var id = buffer.Read<int>();
            var packet = _packetFactory[id](buffer);
            if (!_actions.TryGetValue(id, out var internalAction))
            {
                action = () => { };
                return false;
            }

            action = () => { internalAction(packet, connection); };
            return true;
        }
    }
}