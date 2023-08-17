using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using RPBNet.NetworkBase.Connections;
using RPBUtilities;
using static RPBNet.NetworkBase.NetworkConst;
using static RPBNet.NetworkBase.RPBLoggerType;
using static RPBUtilities.Logging.LogLevel;

namespace RPBNet.NetworkBase.General
{
    internal class ClientListener<T> where T:class
    {

        private readonly ManualResetEvent _allDone = new ManualResetEvent(false);
        private readonly Action<Connection<T>> _onConnect;
        private readonly Action<ByteBuffer, Connection<T>> _onReceive;

        public ClientListener(int port, Action<Connection<T>> onConnect, Action<ByteBuffer, Connection<T>> onReceive)
        {
            _onConnect = onConnect;
            _onReceive = onReceive;
            Task.Factory.StartNew(() => StartListening(port));
        }


        public void StartListening(int port)
        {
            // Bind the socket to the local endpoint and listen for incoming connections.  
            try
            {
                var localEndPoint = new IPEndPoint(IPAddress.Any, port);
               
                // Create a TCP/IP socket.  
                var listener = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (true)
                {
                    _allDone.Reset();
                    listener.BeginAccept(AcceptCallback,listener);
                    _allDone.WaitOne();
                }
               
            }
            catch (Exception e)
            {
                RPBLog.Log(COMMON_FILE,$"Connection closed unexpectedly!\n {e}",ERROR);
            }

        }

        public void AcceptCallback(IAsyncResult ar)
        {
            _allDone.Set();
            // Get the socket that handles the client request.  
            var listener = (Socket?) ar.AsyncState;
            var handler = listener?.EndAccept(ar);
            // Create the state object.  
            var connection = new Connection<T>();
            try
            {
                connection.WorkSocket = handler;

                _onConnect(connection);

                handler?.BeginReceive(connection.Buffer, 0, BUFFER_SIZE, 0,
                    ReadCallback, connection);

            }
            catch (Exception e)
            {
                RPBLog.Log(COMMON_FILE, $"Connection[{connection}] Closed!", INFO);
                RPBLog.Log(COMMON_FILE, e.Message, SYSTEM_MESSAGE);
            }

        }

        public void ReadCallback(IAsyncResult ar)
        {
            var connection = ar.AsyncState as Connection<T>;
            var handler = connection?.WorkSocket;
            if (handler is null || connection is null)
            {
                RPBLog.Log(COMMON_FILE, "Error in read callback, socket or connection was null!",ERROR);
                return;
            }
            try
            {            
                // Retrieve the state object and the handler socket  
                // from the asynchronous state object.  

                var bytesRead = handler.EndReceive(ar);
                if (bytesRead <= 0) return;

                var rec = new byte[bytesRead];
                Buffer.BlockCopy(connection.Buffer, 0, rec, 0, bytesRead);
                var b = new ByteBuffer(rec);
                var size = b.Read<ushort>();
                _onReceive(b, connection);

                handler.BeginReceive(connection.Buffer, 0, BUFFER_SIZE, 0,
                    ReadCallback, connection);


            }
            catch (Exception e)
            {
                RPBLog.Log(COMMON_FILE, e.Message, SYSTEM_MESSAGE);
            }


        }

        private void Send(Socket handler, byte[] data)
        {
           // Begin sending the data to the remote device.  
            handler.BeginSend(data, 0, data.Length, 0,
                SendCallback, handler);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                var handler = (Socket?)ar.AsyncState;

                // Complete sending the data to the remote device.  
                handler?.EndSend(ar);
            }
            catch (Exception)
            {
                RPBLog.Log(COMMON_FILE, "Failed to send data!",ERROR);
            }
        }


    }
}
