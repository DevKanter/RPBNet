﻿using System;
using System.Runtime.CompilerServices;
using RPBCommon.Packet;
using RPBCommon.Packet.Packets;
using RPBNet.Packets;
using RPBPacketBase;
using RPBUtilities;
using static RPBNet.Crypt.CryptProtocol;


namespace RPBNet.Crypt
{
    [Packet(255, (byte)S2C_START_ENC_HANDSHAKE)]
    public class S2CStartEncHandshake : RPBPacket
    {
        private static readonly int _id = BitConverter.ToInt32(new byte[] {  255, (byte)S2C_START_ENC_HANDSHAKE, 0, 0 }, 0);
        public override int PacketId => _id;
        public string Message = "THERE IS NO ENCRYPTION HERE!";

        public override int GetSize()
        {
            return Message.Length + 4;
        }

        public override void Serialize(ByteBuffer buffer)
        {
            buffer.Write(Message);
        }

        public override T Deserialize<T>(ByteBuffer buffer)
        {
            return Unsafe.As<T>(new S2CStartEncHandshake()
            {
                Message = buffer.ReadString()
            });
        }
    }

    [Packet(255, (byte)C2S_READY_ENC_HANDSHAKE)]
    public class C2SReadyEncHandshake : RPBPacket
    {
        private static readonly int _id = BitConverter.ToInt32(new byte[] { 255, (byte)C2S_READY_ENC_HANDSHAKE, 0, 0 }, 0);
        public override int PacketId => _id;

        public string Message = "ARE YOU SURE???";

        public override int GetSize()
        {
            return Message.Length + 4;
        }

        public override void Serialize(ByteBuffer buffer)
        {
            buffer.Write(Message);
        }

        public override T Deserialize<T>(ByteBuffer buffer)
        {
            return Unsafe.As<T>(new C2SReadyEncHandshake()
            {
                Message = buffer.ReadString()
            });
        }
    }


    [Packet(255, (byte)S2C_SHARE_RSA)]
    public class S2CShareRsa : RPBPacket
    {
        private static readonly int _id = BitConverter.ToInt32(new byte[] { 255, (byte)S2C_SHARE_RSA, 0, 0 }, 0);
        public override int PacketId => _id;

        public string ServerRsaPublicKey;

        public override int GetSize()
        {
            return ServerRsaPublicKey.Length + 4;
        }

        public override void Serialize(ByteBuffer buffer)
        {
            buffer.Write(ServerRsaPublicKey);
        }

        public override T Deserialize<T>(ByteBuffer buffer)
        {
            return Unsafe.As<T>(new S2CShareRsa()
            {
                ServerRsaPublicKey = buffer.ReadString()
            });
        }
    }

    [Packet(255, (byte)C2S_SHARE_RSA)]
    public class C2SShareRsa : RPBPacket
    {
        private static readonly int _id = BitConverter.ToInt32(new byte[] { 255, (byte)C2S_SHARE_RSA, 0, 0 }, 0);
        public override int PacketId => _id;

        public string ClientRsaPublicKey;

        public override int GetSize()
        {
            return ClientRsaPublicKey.Length + 4;
        }

        public override void Serialize(ByteBuffer buffer)
        {
            buffer.Write(ClientRsaPublicKey);
        }

        public override T Deserialize<T>(ByteBuffer buffer)
        {
            return Unsafe.As<T>(new S2CShareRsa()
            {
                ServerRsaPublicKey = buffer.ReadString()
            });
        }
    }

    [Packet(255,(byte)S2C_SHARE_AES)]
    public class S2CShareAES : RPBPacket
    {
        private static readonly int _id = BitConverter.ToInt32(new byte[] { 255, (byte)S2C_SHARE_AES, 0, 0 }, 0);
        public override int PacketId => _id;

        public byte[] Key;
        public byte[] IV;

        public override int GetSize()
        {
            return Key.Length +4 + IV.Length+4;
        }

        public override void Serialize(ByteBuffer buffer)
        {
            buffer.Write(Key);
            buffer.Write(IV);
        }

        public override T Deserialize<T>(ByteBuffer buffer)
        {
            return Unsafe.As<T>(new S2CShareAES()
            {
                Key =  buffer.ReadBytes(),
                IV = buffer.ReadBytes()
            });
        }
    }

    [Packet(255, (byte)C2S_SHARE_AES_SUCCESS)]
    public class C2SShareAESAns : RPBPacket
    {
        private static readonly int _id = BitConverter.ToInt32(new byte[] { 255, (byte)C2S_SHARE_AES_SUCCESS, 0, 0 }, 0);
        public override int PacketId => _id;

        public bool Success;

        public override int GetSize()
        {
            return sizeof(bool);
        }

        public override void Serialize(ByteBuffer buffer)
        {
            buffer.Write(Success);
        }

        public override T Deserialize<T>(ByteBuffer buffer)
        {
            return Unsafe.As<T>(new C2SShareAESAns()
            {
                Success = buffer.Read<bool>()
            });
        }
    }

    [Packet(255, (byte)S2C_HANDSHAKE_FAIL)]
    public class S2CHandshakeFail : RPBPacket
    {
        private static readonly int _id = BitConverter.ToInt32(new byte[] { 255, (byte)S2C_HANDSHAKE_FAIL, 0, 0 }, 0);
        public override int PacketId => _id;

        public int ErrorCode;

        public override int GetSize()
        {
            return sizeof(int);
        }

        public override void Serialize(ByteBuffer buffer)
        {
            buffer.Write(ErrorCode);
        }

        public override T Deserialize<T>(ByteBuffer buffer)
        {
            return Unsafe.As<T>(new S2CHandshakeFail()
            {
                ErrorCode = buffer.Read<int>()
            });
        }
    }

    [Packet(255, (byte)C2S_HANDSHAKE_FAIL)]
    public class C2SHandshakeFail : RPBPacket
    {
        private static readonly int _id = BitConverter.ToInt32(new byte[] { 255, (byte)C2S_HANDSHAKE_FAIL, 0, 0 }, 0);
        public override int PacketId => _id;

        public int ErrorCode;

        public override int GetSize()
        {
            return sizeof(int);
        }

        public override void Serialize(ByteBuffer buffer)
        {
            buffer.Write(ErrorCode);
        }

        public override T Deserialize<T>(ByteBuffer buffer)
        {
            return Unsafe.As<T>(new C2SHandshakeFail()
            {
                ErrorCode = buffer.Read<int>()
            });
        }
    }
}

