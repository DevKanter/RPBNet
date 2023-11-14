using System.Collections.Generic;
using RPBNet.NetworkBase;
using RPBNet.Packets;
using RPBUtilities.Logging;
using RPBUtilities.Logging.Loggers;

namespace RPBNet
{
    public static class Network
    {
        public static void Initialize()
        {
            PacketRegister.Initialize();

            Log.Initialize();
        }
    }

    internal static class Log
    {
        public static void Initialize()
        {
            Logger<RPBLoggerType>.Initialize(new Dictionary<RPBLoggerType, IRPBLogger>
            {
                {RPBLoggerType.COMMON_FILE, new FileLogger(RPBLoggerType.COMMON_FILE.ToString(), LogLevel.FULL)}
            });
        }

        public static void Write(RPBLoggerType type, string message, LogLevel level)
        {
            Logger<RPBLoggerType>.Log(message, level, type);
        }
    }
}