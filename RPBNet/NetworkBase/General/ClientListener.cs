using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using RPBNet.Crypt;
using RPBNet.NetworkBase.Connections;
using RPBUtilities;
using RPBUtilities.Logging;
using static RPBNet.NetworkBase.NetworkConst;
using static RPBNet.NetworkBase.RPBLoggerType;
using static RPBUtilities.Logging.LogLevel;

namespace RPBNet.NetworkBase.General
{
    internal class ClientListener<T> where T:class
    {

        private readonly ManualResetEvent _allDone = new ManualResetEvent(false);
        private readonly Action<Connection<T>> _onEstablish;
        private readonly Action<ByteBuffer, Connection<T>> _onReceive;
        private readonly Dictionary<Guid, Connection<T>> _activeConnections;

        public ClientListener(int port, Action<Connection<T>> onEstablish, Action<ByteBuffer, Connection<T>> onReceive)
        {
            _onEstablish = onEstablish;
            _onReceive = onReceive;
            _activeConnections = new Dictionary<Guid, Connection<T>>();
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
                Log.Write(COMMON_FILE,$"Connection closed unexpectedly!\n {e}",ERROR);
            }

        }

        public void AcceptCallback(IAsyncResult ar)
        {
            _allDone.Set();
            // Get the socket that handles the client request.  
            var listener = Unsafe.As<Socket>(ar.AsyncState);
            var handler = listener?.EndAccept(ar);
            // Create the state object.  
            var connection = new Connection<T>(handler,_onEstablish);
            try
            {
                var packet = new S2CStartEncHandshake();
                connection.Send(packet);

                handler?.BeginReceive(connection.Buffer, 0, BUFFER_SIZE, 0,
                    ReadCallback, connection);

            }
            catch (Exception e)
            {
                connection.Close();
                Log.Write(COMMON_FILE, $"Connection[{connection}] Closed!", INFO);
                Log.Write(COMMON_FILE, e.Message, SYSTEM_MESSAGE);
            }

        }

        public void ReadCallback(IAsyncResult ar)
        {
            var connection = Unsafe.As<Connection<T>>(ar.AsyncState);
            var handler = connection?.WorkSocket;
            if (handler is null || connection is null)
            {
                Log.Write(COMMON_FILE, "Error in read callback, socket or connection was null!",ERROR);
                return;
            }
            try
            {            
                // Retrieve the state object and the handler socket  
                // from the asynchronous state object.  

                var bytesRead = handler.EndReceive(ar);
                if (bytesRead <= 0) return;

                var b = new ByteBuffer(connection.Decrypt(bytesRead));
                
                _onReceive(b, connection);

                handler.BeginReceive(connection.Buffer, 0, BUFFER_SIZE, 0,
                    ReadCallback, connection);


            }
            catch (Exception e)
            {
                Log.Write(COMMON_FILE, e.Message, SYSTEM_MESSAGE);
            }


        }
    }
}
