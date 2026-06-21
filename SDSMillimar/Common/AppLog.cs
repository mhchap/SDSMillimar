using NLog;

namespace SDSMillimar.Common
{
    public class AppLog
    {
        public static Logger Plc => LogManager.GetLogger("Plc");
        public static Logger PlcHeartbeat => LogManager.GetLogger("Heartbeat");
        public static Logger FilterData => LogManager.GetLogger("FilterData");
        public static Logger ProcessData => LogManager.GetLogger("ProcessData");
        public static Logger Production => LogManager.GetLogger("Production");
        public static Logger Sql => LogManager.GetLogger("Sql");
    }
}
