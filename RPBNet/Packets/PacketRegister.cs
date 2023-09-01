using RPBNet.Crypt;
using RPBNet.NetworkBase.Connections;
using RPBNet.NetworkBase.Server;

namespace RPBNet.Packets
{
    internal static partial class PacketRegister
    {
        static partial void _initialize();

        public static void Initialize()
        {
            _initialize();

            PacketParser.AddPacketFactory<S2CStartEncHandshake>();
            PacketParser.AddPacketFactory<C2SReadyEncHandshake>();
            PacketParser.AddPacketFactory<S2CShareRsa>();
            PacketParser.AddPacketFactory<C2SShareRsa>();
            PacketParser.AddPacketFactory<S2CShareAES>();
            PacketParser.AddPacketFactory<C2SShareAESAns>();

            PacketParser.RegisterPacketAction<S2CStartEncHandshake>(OnS2C_START_ENC_HANDSHAKE);
            PacketParser.RegisterPacketAction<C2SReadyEncHandshake>(OnC2S_READY_ENC_HANDSHAKE);
            PacketParser.RegisterPacketAction<S2CShareRsa>(OnS2C_SHARE_RSA);
            PacketParser.RegisterPacketAction<C2SShareRsa>(OnC2S_SHARE_RSA);
            PacketParser.RegisterPacketAction<S2CShareAES>(OnS2C_SHARE_AES);
            PacketParser.RegisterPacketAction<C2SShareAESAns>(OnC2S_SHARE_AES_SUCCESS);
        }

        private static void OnS2C_START_ENC_HANDSHAKE(S2CStartEncHandshake packet, IConnection connection)
        {
            connection.GetCrypter().OnS2C_START_ENC_HANDSHAKE(packet,connection);
        }
        private static void OnC2S_READY_ENC_HANDSHAKE(C2SReadyEncHandshake packet, IConnection connection)
        {
            connection.GetCrypter().OnC2S_READY_ENC_HANDSHAKE(packet, connection);
        }
        private static void OnS2C_SHARE_RSA(S2CShareRsa packet, IConnection connection)
        {
            connection.GetCrypter().OnS2C_SHARE_RSA(packet, connection);
        }
        private static void OnC2S_SHARE_RSA(C2SShareRsa packet, IConnection connection)
        {
            connection.GetCrypter().OnC2S_SHARE_RSA(packet, connection);
        }
        private static void OnS2C_SHARE_AES(S2CShareAES packet, IConnection connection)
        {
            connection.GetCrypter().OnS2C_SHARE_AES(packet, connection);
        }
        private static void OnC2S_SHARE_AES_SUCCESS(C2SShareAESAns packet, IConnection connection)
        {
            connection.GetCrypter().OnC2S_SHARE_AES_SUCCESS(packet, connection);
        }
    }
}
