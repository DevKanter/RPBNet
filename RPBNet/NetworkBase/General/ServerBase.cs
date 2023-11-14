using RPBNet.NetworkBase.Connections;
using RPBNet.NetworkBase.Server;
using RPBUtilities;
using RPBUtilities.Logging;
using static RPBNet.NetworkBase.RPBLoggerType;

namespace RPBNet.NetworkBase.General
{
    public abstract class ServerBase<T> where T : class
    {
        private ClientListener<T> _listener;

        protected ServerBase(int port)
        {
            _listener = new ClientListener<T>(port, OnEstablish, OnReceive);
            Log.Write(COMMON_FILE, $"Server on EndPoint: {port} started!", LogLevel.SYSTEM_MESSAGE);
        }

        protected abstract void OnEstablish(Connection<T> connection);

        protected void OnReceive(ByteBuffer buffer, Connection<T> connection)
        {
            PacketParser.Parse(buffer, connection, out var action);
            action.Invoke();
        }
    }
}