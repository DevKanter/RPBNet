using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using RPBCommon.Packet;
using RPBNet.Crypt;
using RPBUtilities;
using RPBUtilities.Crypt;
using RPBUtilities.Logging;
using static RPBNet.NetworkBase.RPBLoggerType;
using static RPBUtilities.Logging.LogLevel;
using RPBPacketBase;
using Timer = System.Timers.Timer;

namespace RPBNet.NetworkBase.Connections
{
    public class Connection<T> : IDisposable, IConnection where T:class
    {
        public ConnectionState State { get; private set; }
        public byte[] Buffer { get; } = new byte[NetworkConst.BUFFER_SIZE];
        public Socket WorkSocket { get; }
        public Guid ID { get; }
        public T User { get; private set; }

        private readonly RPBCrypter _crypter;
        private readonly Action<Connection<T>> _onEstablish;
        private readonly List<Action<Connection<T>>> _onCloseHandlers = new List<Action<Connection<T>>>();

        public Connection(Socket socket, Action<Connection<T>> onEstablish)
        {
            ID = Guid.NewGuid();
            State = ConnectionState.UNDEFINED;
            WorkSocket = socket;
            _onEstablish = onEstablish;
            _crypter = new RPBCrypter();
        }
        public void Send(RPBPacket packet)
        {
            Send(packet.GetData());
        }
        public void Send(byte[] data)
        {
            data = _crypter.Encrypt(data);
            WorkSocket.BeginSend(data, 0, data.Length, 0, _sendCallback, WorkSocket);
        }
        

        public byte[] Decrypt(int size)
        {
            return _crypter.Decrypt(Buffer, size);
        }
        public void OnEstablish()
        {
            _onEstablish(this);
        }

        public RPBCrypter GetCrypter()
        {
            return _crypter;
        }

        public void OnLogin(T user)
        {
            User = user;
            State = ConnectionState.LOGGED_IN;
        }
        private void _sendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                var handler = Unsafe.As<Socket>(ar.AsyncState);

                // Complete sending the data to the remote device.  
                handler?.EndSend(ar);
            }
            catch (Exception)
            {
                Log.Write(COMMON_FILE, "Error sending data",ERROR);
            }
        }

        public void AppendCloseHandler(Action<Connection<T>> onCloseAction)
        {
            _onCloseHandlers.Add(onCloseAction);
        }

        public void Close()
        {
            WorkSocket?.Shutdown(SocketShutdown.Both);
            WorkSocket?.Close();
            Log.Write(COMMON_FILE, $"Connection[{ID}] Closed!", INFO);
            foreach (var onCloseHandler in _onCloseHandlers)
            {
                onCloseHandler(this);
            }
            Dispose();
        }

        public bool Poll()
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
        }
    }
}