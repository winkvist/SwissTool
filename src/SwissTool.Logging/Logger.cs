// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Logger.cs" company="Fredrik Winkvist">
//   Copyright (c) Fredrik Winkvist. All rights reserved.
// </copyright>
// <summary>
//   Defines the Logger type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SwissTool.Logging
{
    using System;

    using NLog;
    using NLog.Config;
    using NLog.Targets;

    /// <summary>
    /// The logger.
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// The logger instance.
        /// </summary>
        private static readonly NLog.Logger Instance = LogManager.GetLogger("SwissTool");

        /// <summary>
        /// Initializes static members of the <see cref="Logger"/> class. 
        /// </summary>
        static Logger()
        {
            Initialize(false);
        }

        /// <summary>
        /// Writes the diagnostic message at the Info level.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        public static void Info(string message, params object[] args)
        {
            if (!string.IsNullOrEmpty(message))
            {
                Instance.Info(message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the Info level.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        public static void InfoException(Exception exception, string message, params object[] args)
        {
            Instance.Info(exception, message, args);
        }

        /// <summary>
        /// Writes the diagnostic message at the Fatal level.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        public static void Fatal(string message, params object[] args)
        {
            if (!string.IsNullOrEmpty(message))
            {
                Instance.Fatal(message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the Fatal level.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        public static void FatalException(Exception exception, string message, params object[] args)
        {
            Instance.Fatal(exception, message, args);
        }

        /// <summary>
        /// Writes the diagnostic message at the Error level.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        public static void Error(string message, params object[] args)
        {
            if (!string.IsNullOrEmpty(message))
            {
                Instance.Error(message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the Error level.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        public static void ErrorException(Exception exception, string message, params object[] args)
        {
            Instance.Error(exception, message, args);
        }

        /// <summary>
        /// Writes the diagnostic message at the Trace level.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        public static void Trace(string message, params object[] args)
        {
            if (!string.IsNullOrEmpty(message))
            {
                Instance.Trace(message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the Trace level.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        public static void TraceException(Exception exception, string message, params object[] args)
        {
            Instance.Trace(exception, message, args);
        }

        /// <summary>
        /// Writes the diagnostic message at the Warning level.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        public static void Warn(string message, params object[] args)
        {
            if (!string.IsNullOrEmpty(message))
            {
                Instance.Warn(message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the Warning level.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        public static void WarnException(Exception exception, string message, params object[] args)
        {
            Instance.Warn(exception, message, args);
        }

        /// <summary>
        /// Writes the diagnostic message at the Debug level.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        public static void Debug(string message, params object[] args)
        {
            if (!string.IsNullOrEmpty(message))
            {
                Instance.Debug(message, args);
            }
        }

        /// <summary>
        /// Writes the diagnostic message at the Debug level.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        public static void DebugException(Exception exception, string message, params object[] args)
        {
            Instance.Debug(exception, message, args);
        }

        /// <summary>
        /// Initializes the specified debug enabled.
        /// </summary>
        /// <param name="debugEnabled">if set to <c>true</c> [debug enabled].</param>
        /// <param name="logFilePath">The log file path.</param>
        public static void Initialize(bool debugEnabled, string logFilePath = null)
        {
            var logConfig = new LoggingConfiguration();

            var fileTarget = new FileTarget();
            logConfig.AddTarget("file", fileTarget);

            fileTarget.FileName = !string.IsNullOrEmpty(logFilePath) ? logFilePath : "SwissTool.log";
            fileTarget.Layout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss} [${level:uppercase=true}] ${message} ${exception:format=ToString,StackTrace}";
            fileTarget.ArchiveAboveSize = 32000; // 250 kb
            fileTarget.ArchiveNumbering = ArchiveNumberingMode.Sequence;
            fileTarget.MaxArchiveFiles = 5;

            var logLevel = debugEnabled ? LogLevel.Trace : LogLevel.Info;
            var rule = new LoggingRule("*", logLevel, fileTarget);
            logConfig.LoggingRules.Add(rule);

            LogManager.Configuration = logConfig;
        }
    }
}
