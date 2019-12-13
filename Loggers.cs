namespace LogsNotIncluded
{
    public static class Loggers
    {
        public static readonly NLog.Logger LogsNotIncluded = logger("LogsNotIncluded");
        public static readonly NLog.Logger Klei = logger("Klei");
        public static readonly NLog.Logger Debug = logger("Debug");
        public static readonly NLog.Logger ModLoader = logger("ModLoader");
        public static readonly NLog.Logger DLLLoader = logger("DLLLoader");
        public static readonly NLog.Logger Utilities = logger("Utilties");

        private static NLog.Logger logger(string name)
        {
            return NLog.LogManager.GetLogger(name);
        }
    }
}
