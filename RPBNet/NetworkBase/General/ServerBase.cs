using System.Net;
using RPBNet.NetworkBase.Connections;
using RPBUtilities;
using RPBUtilities.Logging;
using static RPBNet.NetworkBase.RPBLoggerType;

namespace RPBNet.NetworkBase.General
{
    internal abstract class ServerBase<T> where T : class
    {
        private ClientListener<T> _listener;
        protected ServerBase(int port)
        {
            _listener = new ClientListener<T>(port,OnConnect, OnReceive);
            RPBLog.Log(COMMON_FILE,$"Server on EndPoint: {port} started!",LogLevel.SYSTEM_MESSAGE);
        }
        
        protected abstract void OnConnect(Connection<T> connection);
        protected abstract void OnReceive(ByteBuffer buffer, Connection<T> connection);
    }
}
