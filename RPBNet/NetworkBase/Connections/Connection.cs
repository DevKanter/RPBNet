using System.Dynamic;
using System.Net.Sockets;
using System.Timers;
using RPBNet.NetworkBase;
using SunPackets;
using Utilities.Logging;
using Timer = System.Timers.Timer;

namespace GDCNetwork.NetworkBase.Connections
{
    public class Connection<T> : IDisposable
    {
        public ConnectionState State { get; private set; }
        public byte[] Buffer { get; } = new byte[NetworkConst.BUFFER_SIZE];
        public Socket? WorkSocket { get; set; }= null;
        public Guid ID { get; }
        public T? User { get; private set; }

        private readonly Timer _timer;
        private readonly List<Action<Connection<T>>> _onCloseHandlers = new();

        public Connection()
        {
            ID = Guid.NewGuid();
            State = ConnectionState.UNDEFINED;
            _timer = new(1000);
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
                Logger.Instance.Log("Error sending data");
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
            Logger.Instance.Log($"Connection[{ID}] Closed!");
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