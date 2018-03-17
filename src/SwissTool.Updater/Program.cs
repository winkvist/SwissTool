// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Fredrik Winkvist">
//   Copyright (c) Fredrik Winkvist. All rights reserved.
// </copyright>
// <summary>
//   Defines the Program type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SwissTool.Updater
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Windows;

    using MessageBox = System.Windows.MessageBox;

    /// <summary>
    /// The updater application.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The current execution path.
        /// </summary>
        private static readonly string CurrentPath;

        /// <summary>
        /// The installation packages path.
        /// </summary>
        private static readonly string UpdatesPath;

        /// <summary>
        /// The main application unique identifier.
        /// </summary>
        private static readonly string Identifier = "6E96735D-84F1-48AB-916D-5639DC763E47";

        /// <summary>
        /// The main application path
        /// </summary>
        private static string mainApplicationPath;

        /// <summary>
        /// The debug enabled
        /// </summary>
        private static bool debugEnabled;

        /// <summary>
        /// Initializes static members of the <see cref="Program"/> class. 
        /// </summary>
        static Program()
        {
            CurrentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (CurrentPath == null)
            {
                throw new ApplicationException("Unable to load updater assembly.");
            }

            UpdatesPath = Path.Combine(CurrentPath, "Updates");
        }

        /// <summary>
        /// The application main entry.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static void Main(string[] args)
        {
            try
            {
                var validArguments = ValidateArguments(args, out mainApplicationPath, out debugEnabled);

                if (!validArguments)
                {
                    MessageBox.Show("Please use Loader.exe to start SwissTool.", "SwissTool", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (!Directory.Exists(UpdatesPath))
                {
                    MessageBox.Show("The updates path doesn't exist, exiting.", "Missing update path", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (InstanceRunning("Loader"))
                {
                    MessageBox.Show("The updater application timed out waiting for loader application to close.", "Timeout", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var updateSignal = InstallUpdates();

                StartMainApplication(updateSignal);
            }
            finally
            {
                // Removes the update directory.
                RemoveInstallationFiles();
            }
        }

        /// <summary>
        /// Validates the arguments.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <param name="applicationPath">The application path.</param>
        /// <param name="isDebugEnabled">if set to <c>true</c> [is debug enabled].</param>
        /// <returns>
        /// A value indicating whether the arguments are valid.
        /// </returns>
        private static bool ValidateArguments(string[] arguments, out string applicationPath, out bool isDebugEnabled)
        {
            isDebugEnabled = false;
            applicationPath = string.Empty;

            if (arguments == null || arguments.Length < 3)
            {
                return false;
            }

            if (!Directory.Exists(arguments[0]))
            {
                return false;
            }

            applicationPath = arguments[0];

            if (arguments[1] != Identifier)
            {
                return false;
            }

            if (!bool.TryParse(arguments[2], out isDebugEnabled))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Check whether the main application is running.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        /// A value indicating whether the application is running.
        /// </returns>
        private static bool InstanceRunning(string name)
        {
            var process = Process.GetProcessesByName(name).FirstOrDefault();
            if (process != null && !process.HasExited)
            {
                process.WaitForExit(5000);
                return !process.HasExited;
            }

            return false;
        }

        /// <summary>
        /// Starts the main application.
        /// </summary>
        /// <param name="updateSignal">The update signal.</param>
        private static void StartMainApplication(string updateSignal)
        {
            var applicationPath = Path.Combine(mainApplicationPath, "SwissTool.exe");

            var startInfo = new ProcessStartInfo(applicationPath, Identifier + " " + updateSignal + " " + debugEnabled)
            {
                Verb = "runas",
                UseShellExecute = true
            };

            Process.Start(startInfo);
        }

        /// <summary>
        /// Cleanups the installation files.
        /// </summary>
        private static void RemoveInstallationFiles()
        {
            if (Directory.Exists(UpdatesPath))
            {
                Directory.Delete(UpdatesPath, true);
            }
        }

        /// <summary>
        /// Installs the updates.
        /// </summary>
        /// <returns>An update signal.</returns>
        private static string InstallUpdates()
        {
            bool updatesInstalled;

            try
            {
                updatesInstalled = ApplicationUpdater.InstallUpdates(UpdatesPath);
            }
            catch
            {
                return "failed";
            }

            return updatesInstalled ? "success" : "none";
        }
    }
}
