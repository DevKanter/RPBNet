using System.Net;
using GDCNetwork.NetworkBase.Connections;
using GDCNetwork.NetworkBase.General;

namespace RPBNet.NetworkBase.General
{
    internal abstract class ServerBase<T>
    {
        private ClientListener<T> _listener;
        protected ServerBase(int port)
        {
            _listener = new(port,OnConnect, OnReceive);
            Logger.Instance.Log($"Server on EndPoint: {port} started!",LogType.SUCCESS);
        }
        
        protected abstract void OnConnect(Connection<T> connection);
        protected abstract void OnReceive(ByteBuffer buffer, Connection<T> connection);
    }
}
