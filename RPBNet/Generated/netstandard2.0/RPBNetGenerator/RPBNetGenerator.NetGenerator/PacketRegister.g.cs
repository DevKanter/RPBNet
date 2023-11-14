  
using RPBNet.NetworkBase.Server;
using RPBCommon.Packet.Packets.Character;
using SMG1Common.Packet.Packets.Connection;
using C2SCharacterCreate = SMG1Common.Packet.Packets.Character.C2SCharacterCreate;

namespace RPBNet.Packets
{
internal static partial class PacketRegister
        {
            static partial void _initialize()
            {
                PacketParser.AddPacketFactory<C2SCharacterCreate>();
PacketParser.AddPacketFactory<S2CHello>();
PacketParser.AddPacketFactory<C2SHello>();

            }
        }
}