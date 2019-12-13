using System.Collections.Generic;

namespace LogsNotIncluded
{
    public static class LogManager
    {
        private static Dictionary<string, NLog.Logger> modLoggers = new Dictionary<string, NLog.Logger>();

        public static NLog.Logger GetLogger(ModInfo modInfo)
        {
            return GetLogger(modInfo.ID);
        }

        public static NLog.Logger GetLogger(string id)
        {
            NLog.Logger logger;
            bool hasLogger = modLoggers.TryGetValue(id, out logger);
            if (hasLogger)
            {
                return logger;
            }
            else
            {
                logger = NLog.LogManager.GetLogger(id);
                modLoggers.Add(id, logger);
                return logger;
            }
        }

        public static int RegisteredAmount
        {
            get
            {
                return modLoggers.Count;
            }
        }
    }
}
