using RPBNet.Crypt;
using RPBPacketBase;

namespace RPBNet.NetworkBase.Connections
{
    public interface IConnection
    {
        void Send(RPBPacket packet);

        void OnEstablish();

        RPBCrypter GetCrypter();
    }
}