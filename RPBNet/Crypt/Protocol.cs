using System;
using System.Collections.Generic;
using System.Text;
// ReSharper disable InconsistentNaming

namespace RPBNet.Crypt
{
    internal enum CryptProtocol : byte
    {


        S2C_START_ENC_HANDSHAKE,
        C2S_READY_ENC_HANDSHAKE,
        S2C_SHARE_RSA,
        C2S_SHARE_RSA,


        S2C_SHARE_AES,
        C2S_SHARE_AES_SUCCESS,

        S2C_HANDSHAKE_FAIL,
        C2S_HANDSHAKE_FAIL
    }
}
