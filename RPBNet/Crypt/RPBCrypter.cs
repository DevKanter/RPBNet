using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using RPBCommon.Packet;
using RPBNet.NetworkBase;
using RPBNet.NetworkBase.Client;
using RPBNet.NetworkBase.Connections;
using RPBUtilities.Crypt;
using RPBUtilities.Logging;
using static RPBNet.NetworkBase.NetworkConst;

namespace RPBNet.Crypt
{
    public class RPBCrypter
    {
        private ICrypter _crypter;

        public RPBCrypter()
        {
            _crypter = new NoCrypter();
        }

        public byte[] Encrypt(byte[] data)
        {
            return _crypter.Encrypt(data);
        }

        public byte[] Decrypt(byte[] buffer,int size)
        {
            return _crypter.Decrypt(buffer,size);
        }

        public void OnS2C_START_ENC_HANDSHAKE(S2CStartEncHandshake packet,IConnection connection)
        {
            if (packet.Message != "THERE IS NO ENCRYPTION HERE!")
            {
                var errorPacket = new C2SHandshakeFail() {ErrorCode = 0};
                connection.Send(errorPacket);
                return;
            }

            var answerPacket = new C2SReadyEncHandshake();
            connection.Send(answerPacket);
        }

        public void OnC2S_READY_ENC_HANDSHAKE(C2SReadyEncHandshake packet, IConnection connection)
        {
            if (packet.Message != "ARE YOU SURE???")
            {
                var errorPacket = new S2CHandshakeFail() { ErrorCode = 0 };
                connection.Send(errorPacket);
                return;
            }

            var rsa = new RsaCryptor();
            var answerPacket = new S2CShareRsa()
            {
                ServerRsaPublicKey = rsa.GetMyPublicKey()
            };
            connection.Send(answerPacket);

            _crypter = rsa;
        }

        public void OnS2C_SHARE_RSA(S2CShareRsa packet, IConnection connection)
        {
            _crypter = new RsaCryptor();

            var rsa = Unsafe.As<RsaCryptor>(_crypter);
            rsa.SetOtherPublicKey(packet.ServerRsaPublicKey);

            var answerPacket = new C2SShareRsa()
            {
                ClientRsaPublicKey = rsa.GetMyPublicKey()
            };

            connection.Send(answerPacket);
        }

        public void OnC2S_SHARE_RSA(C2SShareRsa packet, IConnection connection)
        {
            var rsa = Unsafe.As<RsaCryptor>(_crypter);
            rsa.SetOtherPublicKey(packet.ClientRsaPublicKey);

            var aes = new AesCryptor();
            var answerPacket = aes.GetAesPacket();

            connection.Send(answerPacket);

            _crypter = aes;
        }

        public void OnS2C_SHARE_AES(S2CShareAES packet, IConnection connection)
        {
            var aes = new AesCryptor(packet.Key,packet.IV);
            _crypter = aes;

            var answerPacket = new C2SShareAESAns()
            {
                Success = true
            };

            connection.Send(answerPacket);
        }

        public void OnC2S_SHARE_AES_SUCCESS(C2SShareAESAns packet, IConnection connection)
        {
            if (packet.Success)
            {
                connection.OnEstablish();
            }
        }
    }



    internal interface ICrypter
    {
        byte[] Encrypt(byte[] data);
        byte[] Decrypt(byte[] buffer,int size);
    }

    internal class NoCrypter : ICrypter
    {
        public byte[] Encrypt(byte[] data)
        {
            return data;
        }

        public byte[] Decrypt(byte[] buffer, int size)
        {
            var result = new byte[size];
            Buffer.BlockCopy(buffer,0,result,0,size);
            return result;
        }
    }

    internal class AesCryptor : ICrypter
    {
        private readonly AES _aes;

        public AesCryptor()
        {
            _aes = new AES();
        }
        public AesCryptor(byte[] key, byte[] iv)
        {
            _aes = new AES(key, iv);
        }

        public S2CShareAES GetAesPacket()
        {
            return new S2CShareAES()
            {
                Key = _aes.Key,
                IV = _aes.IV,
            };
        }
        public byte[] Decrypt(byte[] buffer,int size)
        {
            return _aes.Decrypt(buffer, size);
        }

        public byte[] Encrypt(byte[] data)
        {
            return _aes.Encrypt(data);
        }
    }

    internal class RsaCryptor : ICrypter
    {
        private readonly RSA _myRSA;
        private readonly RSA _otherRSA;
        public RsaCryptor()
        {
            _myRSA = RSA.Create();
            _myRSA.KeySize = 4096;
            _otherRSA = RSA.Create();
            _otherRSA.KeySize = 4096;
        }

        public void SetOtherPublicKey(string publicKeyString)
        {
            var sr = new System.IO.StringReader(publicKeyString);
            //we need a deserializer
            var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
            //get the object back from the stream
            var pubKey = (RSAParameters)xs.Deserialize(sr);
            _otherRSA.ImportParameters(pubKey);
        }

        public string GetMyPublicKey()
        {
            var pubKey = _myRSA.ExportParameters(false);
            //we need some buffer
            var sw = new System.IO.StringWriter();
            //we need a serializer
            var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
            //serialize the key into the stream
            xs.Serialize(sw, pubKey);
            //get the string from the stream
            return sw.ToString();
        }
        public byte[] Encrypt(byte[] data)
        {
            return _otherRSA.Encrypt(data, RSAEncryptionPadding.Pkcs1);
        }

        public byte[] Decrypt(byte[] buffer, int size)
        {
            var bytes = new byte[size];
            Buffer.BlockCopy(buffer, 0, bytes, 0, size);
            return _myRSA.Decrypt(bytes, RSAEncryptionPadding.Pkcs1);
        }
    }

}
