using System;
using System.Collections.Generic;
using System.Text;
using RPBNet.NetworkBase;
using RPBUtilities.Logging;
using RPBUtilities.Logging.Loggers;

namespace RPBNet
{
    public static class RPBNet
    {
        public static void Initialize()
        {
            RPBLog.Initialize();
        }
    }

    internal static class RPBLog
    {
        public static void Initialize()
        {
            Logger<RPBLoggerType>.Initialize(new Dictionary<RPBLoggerType, IRPBLogger>()
            {
                {RPBLoggerType.COMMON_FILE, new FileLogger("./logs/RPBNet.Common.log", LogLevel.FULL)}
            });
        }

        public static void Log(RPBLoggerType type, string message, LogLevel level)
        {
            Logger<RPBLoggerType>.Log(message,level,type);
        }
    }
}
