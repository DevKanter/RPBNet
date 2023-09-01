using System.Runtime.CompilerServices;
using RPBCommon.Packet.Packets.Character;
using RPBNet.NetworkBase.Connections;
using RPBNet.NetworkBase.Server;
using RPBUtilities;
using RPBUtilities.Logging;
using static RPBNet.NetworkBase.RPBLoggerType;

namespace RPBNet.NetworkBase.General
{
    public abstract partial class ServerBase<T> where T : class
    {
        private ClientListener<T> _listener;
        protected ServerBase(int port)
        {
            _listener = new ClientListener<T>(port,OnEstablish, OnReceive);
            Log.Write(COMMON_FILE,$"Server on EndPoint: {port} started!",LogLevel.SYSTEM_MESSAGE);
        }
        
        protected abstract void OnEstablish(Connection<T> connection);
        protected abstract void OnReceive(ByteBuffer buffer, Connection<T> connection);

    }


}
