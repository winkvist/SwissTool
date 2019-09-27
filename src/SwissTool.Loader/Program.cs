// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Fredrik Winkvist">
//   Copyright (c) Fredrik Winkvist. All rights reserved.
// </copyright>
// <summary>
//   The loader application.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SwissTool.Loader
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Windows;

    using SwissTool.Framework.Infrastructure;
    using SwissTool.Logging;

    /// <summary>
    /// The loader application.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The main application unique identifier
        /// </summary>
        private const string Identifier = "6E96735D-84F1-48AB-916D-5639DC763E47";

        /// <summary>
        /// The current directory.
        /// </summary>
        private static readonly string CurrentDirectory;

        /// <summary>
        /// The debug enabled
        /// </summary>
        private static bool debugEnabled;

        /// <summary>
        /// Initializes static members of the <see cref="Program"/> class. 
        /// </summary>
        static Program()
        {
            CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (CurrentDirectory == null)
            { 
                throw new ApplicationException("Unable to locate the application directory.");
            }
        }

        /// <summary>
        /// The main entry point of the loader application.
        /// </summary>
        /// <param name="args">The args.</param>
        public static void Main(string[] args)
        {
            // Check for running instance.
            if (IsMaxInstanceLevelReached("SwissTool"))
            {
                MessageBox.Show("An instance of SwissTool is already running, only one instance is allowed to run at a time.", "Instance is running", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            
            if (args != null && args.Length > 0)
            {
                bool.TryParse(args[0], out debugEnabled);
            }

            Logger.Initialize(debugEnabled, AppConstants.LogFilePath);

            Logger.Info("Executing SwissTool Loader application");

            try
            {
                Run();
            }
            catch (Exception ex)
            {
                Logger.ErrorException(ex, "An error occurred while starting SwissTool");
                MessageBox.Show("Unable to start SwissTool. See log file for more details.", "Unable to run", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Runs this instance.
        /// </summary>
        private static void Run()
        {
            if (Directory.Exists(AppConstants.UpdatesDataPath))
            {
                RunUpdater();
            }
            else
            {
                RunApplication();
            }
        }

        /// <summary>
        /// Runs the updater application.
        /// </summary>
        private static void RunUpdater()
        {
            var updaterFile = Path.Combine(AppConstants.ApplicationDataPath, "Updater.exe");
            var unrarFile = Path.Combine(AppConstants.ApplicationDataPath, "SharpCompress.dll");
            
            if (File.Exists(updaterFile))
            {
                File.Delete(updaterFile);
            }

            if (File.Exists(unrarFile))
            {
                File.Delete(unrarFile);
            }

            WriteResourceToFile("SwissTool.Loader.Resources.SharpCompress.dll", unrarFile);
            WriteResourceToFile("SwissTool.Loader.Resources.Updater.exe", updaterFile);

            var arguments = "\"" + CurrentDirectory + "\"" + " " + Identifier + " " + debugEnabled;

            var startInfo = new ProcessStartInfo(updaterFile, arguments)
            {
                UseShellExecute = true,
                Verb = "runas"
            };

            Logger.Info("Update files have been found. Executing updater application.");

            Process.Start(startInfo);
        }

        /// <summary>
        /// Runs the main application.
        /// </summary>
        private static void RunApplication()
        {
            var applicationFile = Path.Combine(CurrentDirectory, "SwissTool.exe");

            var startInfo = new ProcessStartInfo(applicationFile, Identifier + " " + "none" + " " + debugEnabled)
            {
                UseShellExecute = true,
                Verb = "runas"
            };

            Logger.Info("Starting main application.");

            Process.Start(startInfo);
        }

        /// <summary>
        /// Writes the resource to file.
        /// </summary>
        /// <param name="resourceName">Name of the resource.</param>
        /// <param name="fileLocation">The file location.</param>
        private static void WriteResourceToFile(string resourceName, string fileLocation)
        {
            using (var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (resourceStream == null)
                {
                    throw new ApplicationException("The application resource could not be found.");
                }

                using (var fileStream = File.Create(fileLocation))
                {
                    resourceStream.CopyTo(fileStream);
                }
            }
        }

        /// <summary>
        /// Checks the instance.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="maxConcurrent">The max concurrent.</param>
        /// <returns>Whether the instance has reached its max concurrent instance level.</returns>
        private static bool IsMaxInstanceLevelReached(string name, int maxConcurrent = 0)
        {
            return Process.GetProcessesByName(name).Count() > maxConcurrent;
        }
    }
}
