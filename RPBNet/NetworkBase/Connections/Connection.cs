using System;
using System.Collections.Generic;
using System.Net.Sockets;
using RPBUtilities.Logging;
using static RPBNet.NetworkBase.RPBLoggerType;
using static RPBUtilities.Logging.LogLevel;
using Timer = System.Timers.Timer;

namespace RPBNet.NetworkBase.Connections
{
    public class Connection<T> : IDisposable where T:class
    {
        public ConnectionState State { get; private set; }
        public byte[] Buffer { get; } = new byte[NetworkConst.BUFFER_SIZE];
        public Socket? WorkSocket { get; set; }= null;
        public Guid ID { get; }
        public T? User { get; private set; }

        private readonly Timer _timer = new Timer(1000);
        private readonly List<Action<Connection<T>>> _onCloseHandlers = new List<Action<Connection<T>>>();

        public Connection()
        {
            ID = Guid.NewGuid();
            State = ConnectionState.UNDEFINED;
            _timer.AutoReset = true;
            _timer.Elapsed += delegate { ConnectionCheck(); };
            _timer.Start();
        }

        internal void Send(byte[] data)
        {
            WorkSocket!.BeginSend(data, 0, data.Length, 0, SendCallback, WorkSocket);
        }
        public void Establish()
        {
            State = ConnectionState.ESTABLISHED;
        }
        public void OnLogin(T user)
        {
            User = user;
            State = ConnectionState.LOGGED_IN;
        }
        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                var handler = (Socket?) ar.AsyncState;

                // Complete sending the data to the remote device.  
                handler?.EndSend(ar);
            }
            catch (Exception)
            {
                RPBLog.Log(COMMON_FILE, "Error sending data",ERROR);
            }
        }

        public void AppendCloseHandler(Action<Connection<T>> onCloseAction)
        {
            _onCloseHandlers.Add(onCloseAction);
        }

        private void ConnectionCheck()
        {
            if (IsConnected()) return;

            WorkSocket?.Shutdown(SocketShutdown.Both);
            WorkSocket?.Close();
            _timer.Stop();
            RPBLog.Log(COMMON_FILE, $"Connection[{ID}] Closed!",INFO);
            foreach (var onCloseHandler in _onCloseHandlers)
            {
                onCloseHandler(this);
                Dispose();
            }
        }

        private bool IsConnected()
        {
            try
            {
                return WorkSocket != null && !(WorkSocket.Poll(1, SelectMode.SelectRead) && WorkSocket.Available == 0);
            }
            catch (SocketException)
            {
                return false;
            }
        }

        public void Dispose()
        {
            WorkSocket?.Dispose();
            _timer.Dispose();
        }
    }
}