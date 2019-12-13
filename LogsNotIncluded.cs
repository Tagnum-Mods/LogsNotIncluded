using Harmony;
using NLog;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace LogsNotIncluded
{
    public static class LogsNotIncluded
    {
        public static bool Initialized { get; private set; } = false;
        private static bool initializing = false;

        private static NLog.Logger logger = Loggers.LogsNotIncluded;

        private static void Initialize()
        {
            if (initializing || Initialized) return;
            initializing = true;
            try
            {
                Debug.Log("LogsNotIncluded Starting Up");
                Debug.Log("Log Directory: " + Path.Combine(Utils.GetLogDirectory(), "latest.log"));
                //Application.logMessageReceived += HandleLog;
                Application.logMessageReceivedThreaded += HandleLog;
                Application.quitting += Shutdown;

                Utils.MakeLogDirectory();

                var config = new NLog.Config.LoggingConfiguration();

                var file = new NLog.Targets.FileTarget("file")
                {
                    FileName = Path.Combine(Utils.GetLogDirectory(), "latest.log"),
                    ArchiveFileName = Path.Combine(Utils.GetLogDirectory(), "${date:format=yyyy-MM-dd:cached=true}-{#}.log"),
                    ArchiveEvery = NLog.Targets.FileArchivePeriod.None,
                    ArchiveNumbering = NLog.Targets.ArchiveNumberingMode.Rolling,
                    ArchiveDateFormat = "yyyy-MM-dd HH_mm_ss",
                    ArchiveOldFileOnStartup = true,
                    Layout = "[${time}][${threadid}/${level:uppercase=true}]: ${message}${onexception:${newline}${exception}}",
                    MaxArchiveFiles = 100,

                };
                config.AddRule(LogLevel.Info, LogLevel.Fatal, file);
#if DEBUG
                var debug_file = new NLog.Targets.FileTarget("debug_file")
                {
                    FileName = Path.Combine(Utils.GetLogDirectory(), "debug.log"),
                    ArchiveFileName = Path.Combine(Utils.GetLogDirectory(), "debug.{#}.log"),
                    ArchiveNumbering = NLog.Targets.ArchiveNumberingMode.Rolling,
                    ArchiveOldFileOnStartup = true,
                    MaxArchiveFiles = 5,
                    Layout = "[${time}][${threadid}/${level:uppercase=true}][${logger}]: ${message}${onexception:${newline}${exception}}",
                };
                config.AddRule(LogLevel.Trace, LogLevel.Fatal, debug_file);
#endif

                NLog.LogManager.Configuration = config;

                logger.Info("Finised starting up");
                Initialized = true;
            }
            catch (System.Exception e)
            {
                Debug.Log("LogsNotIncluded failed to initialize: " + e.ToString());
            }
            initializing = false;
        }

        public static void PrePatch(HarmonyInstance instance)
        {
            if (!Initialized) Initialize();
            logger.Info("Running PrePatch");
            HarmonyInstance.DEBUG = true;
        }

        public static void PostPatch(HarmonyInstance instance)
        {
            logger.Info("Running PostPatch");
        }

        public static void OnLoad(string str)
        {
            if (!Initialized) Initialize();
            logger.Info("Running OnLoad");

            //throw new System.Exception("Test Failed Load");
        }

        private static void HandleLog(string message, string stack_trace, LogType type)
        {
            LogLevel level = LogLevel.Info;
            switch (type)
            {
                case LogType.Error:
                    level = LogLevel.Error;
                    break;
                case LogType.Assert:
                    level = LogLevel.Trace;
                    break;
                case LogType.Warning:
                    level = LogLevel.Warn;
                    break;
                case LogType.Exception:
                    level = LogLevel.Error;
                    break;
            }

            if (string.IsNullOrEmpty(stack_trace))
            {
                stack_trace = new StackTrace().ToString();
            }

            Loggers.Klei.Log(level, message + " : " + stack_trace);
        }

        private static void Shutdown()
        {
            logger.Info("LogsNotIncluded is shutting down");
            Loggers.Klei.Info("Application is shutting down");
            NLog.LogManager.Shutdown();
        }
    }
}
