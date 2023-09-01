using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using RPBCommon;
using RPBCommon.Packet;
using RPBNet.Crypt;
using RPBNet.NetworkBase.Connections;
using RPBNet.NetworkBase.Server;
using RPBPacketBase;
using RPBUtilities;
using RPBUtilities.calc;
using RPBUtilities.Crypt;
using SMG1Common;
using static SMG1Common.RCClientConnect;
using static RPBNet.NetworkBase.NetworkConst;

namespace RPBNet.NetworkBase.Client
{

    public class RPBClient: IConnection
    {
        protected TcpClient TcpClient;
        private readonly byte[] _buffer = new byte[BUFFER_SIZE];
        private readonly RPBCrypter _crypter = new RPBCrypter();

        public RCClientConnect TryConnect(string serverIPAddress,int serverPort)
        {
            if (!IPAddress.TryParse(serverIPAddress, out var ipAddress)) return ERROR_PARSE_IP_ADDRESS;

            if (Calc.OutOfRange(serverPort, IPEndPoint.MinPort, IPEndPoint.MaxPort)) return ERROR_PARSE_PORT;

            var endPoint = new IPEndPoint(ipAddress, serverPort);
            _connect(endPoint);
            return BEGIN_CONNECTING;
        }

        private void _connect(IPEndPoint endPoint)
        {
            TcpClient = new TcpClient();
            TcpClient.Connect(endPoint);

            var t = new Thread(_beginAccept);
            t.Start();
        }

        private void _beginAccept()
        {
            if (TcpClient == null) return;
            var stream = TcpClient.GetStream();
            while (true)
            {
                if (!stream.DataAvailable) continue;
                var size = stream.Read(_buffer, 0, BUFFER_SIZE);
                var data = _crypter.Decrypt(_buffer,size);
                OnReceive(data);

            }
        }

        protected virtual void OnReceive(byte[] data)
        {
            PacketParser.Parse(new ByteBuffer(data),this, out var action);
            action();
        }

        public void Send(RPBPacket packet)
        {
            TcpClient.Client.Send(_crypter.Encrypt(packet.GetData()));
        }

        public void OnEstablish()
        {
            throw new NotImplementedException();
        }

        public RPBCrypter GetCrypter()
        {
            return _crypter;
        }
    }

    public class RPBUnityClient : RPBClient
    {
        private readonly Queue<Action> _actionQueue = new Queue<Action>();


        protected override void OnReceive(byte[] data)
        {
            PacketParser.Parse(new ByteBuffer(data),this, out var action);
            _actionQueue.Enqueue(action);
        }

        public void Update()
        {
            if (_actionQueue.Count > 0)
            {
                _actionQueue.Dequeue().Invoke();
            }
        }
    }
}
