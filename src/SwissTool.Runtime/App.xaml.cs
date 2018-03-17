// --------------------------------------------------------------------------------------------------------------------
// <copyright file="App.xaml.cs" company="Fredrik Winkvist">
//   Copyright (c) Fredrik Winkvist. All rights reserved.
// </copyright>
// <summary>
//   Defines the App type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SwissTool.Runtime
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Threading;

    using SwissTool.Application;
    using SwissTool.Application.Code.Events;
    using SwissTool.Application.Models;
    using SwissTool.Framework.Enums;
    using SwissTool.Framework.Infrastructure;
    using SwissTool.Framework.UI.Managers;
    using SwissTool.Logging;

    /// <summary>
    /// Interaction logic for App
    /// </summary>
    /// <seealso cref="System.Windows.Application" />
    public partial class App
    {
        /// <summary>
        /// The current path.
        /// </summary>
        private static readonly string CurrentPath;

        /// <summary>
        /// The main application unique identifier
        /// </summary>
        private static readonly string Identifier = "6E96735D-84F1-48AB-916D-5639DC763E47";

        /// <summary>
        /// Initializes static members of the <see cref="App"/> class.
        /// </summary>
        /// <exception cref="ApplicationException">
        /// Unable to load main application assembly.
        /// </exception>
        static App()
        {
            CurrentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (CurrentPath == null)
            {
                throw new ApplicationException("Unable to load main application assembly.");
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Application.Startup"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.StartupEventArgs"/> that contains the event data.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            this.Dispatcher.UnhandledException += this.DispatcherOnUnhandledException;

            UpdateSignal updateSignal;
            string identifier;
            bool debugEnabled;

            var validArguments = this.ValidateArguments(e.Args, out identifier, out updateSignal, out debugEnabled);

            if (!validArguments)
            {
                var appName = AppConstants.ApplicationName;

                MessageBox.Show("Please use Loader.exe to start " + appName + ".", appName, MessageBoxButton.OK, MessageBoxImage.Information);
                Environment.Exit(0);
            }

            if (!this.InstanceRunning("Updater"))
            {
                var updaterFile = Path.Combine(AppConstants.ApplicationDataPath, "Updater.exe");
                var nunrarFile = Path.Combine(AppConstants.ApplicationDataPath, "NUnrar.dll");

                if (File.Exists(updaterFile))
                {
                    File.Delete(updaterFile);
                }

                if (File.Exists(nunrarFile))
                {
                    File.Delete(nunrarFile);
                }
            }

            var app = (App)Current;

            // Starts the main application.
            this.StartApplication(updateSignal, debugEnabled);
        }

        /// <summary>
        /// Validates the arguments.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <param name="identifier">The identifier.</param>
        /// <param name="updateSignal">The update signal.</param>
        /// <param name="debugEnabled">if set to <c>true</c> [debug enabled].</param>
        /// <returns>A value indicating whether the arguments are valid.</returns>
        private bool ValidateArguments(string[] arguments, out string identifier, out UpdateSignal updateSignal, out bool debugEnabled)
        {
            updateSignal = UpdateSignal.None;
            debugEnabled = false;
            identifier = string.Empty;

            if (Debugger.IsAttached)
            {
                // Skip validation in debug mode.
                return true;
            }

            if (arguments == null || arguments.Length < 3)
            {
                return false;
            }

            identifier = arguments[0];

            if (identifier != Identifier)
            {
                return false;
            }

            var updateStatusStr = arguments[1];

            if (!Enum.TryParse(updateStatusStr, true, out updateSignal))
            {
                return false;
            }

            if (!bool.TryParse(arguments[2], out debugEnabled))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Starts the application.
        /// </summary>
        /// <param name="updateSignal">The update signal.</param>
        /// <param name="debugEnabled">if set to <c>true</c> [debug enabled].</param>
        private void StartApplication(UpdateSignal updateSignal, bool debugEnabled)
        {
            Logger.Initialize(debugEnabled, AppConstants.LogFilePath);

            var mainApplication = new ApplicationHost();
            mainApplication.Initialize();
            this.MainWindow = mainApplication.MainWindow;

            mainApplication.ThemeChanged += this.MainApplicationOnThemeChanged;

            if (updateSignal == UpdateSignal.Failed)
            {
                // Signal failed update.
                mainApplication.ShowBalloonToolTip("Update error", "An error occurred while installing the application updates.", BalloonIcon.Error);
            }
            else if (updateSignal == UpdateSignal.Success)
            {
                // Signal successful update.
                mainApplication.ShowBalloonToolTip("Updates installed", "Application updates has been successfully installed.", BalloonIcon.Info);
            }

            this.ApplyTheme(mainApplication.Settings.Theme, mainApplication.Settings.Accent);

            mainApplication.LoadExtensions();
        }

        /// <summary>
        /// Mains the application on theme changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="ThemeChangedEventArgs"/> instance containing the event data.</param>
        private void MainApplicationOnThemeChanged(object sender, ThemeChangedEventArgs args)
        {
            this.ApplyTheme(args.Theme, args.Accent);
        }

        /// <summary>
        /// Check whether the main application is running.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>
        /// A value indicating whether the application is running.
        /// </returns>
        private bool InstanceRunning(string name)
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
        /// Applies the theme.
        /// </summary>
        /// <param name="themeName">Name of the theme.</param>
        /// <param name="accentName">Name of the accent.</param>
        /// <exception cref="System.NullReferenceException">
        /// Unable to load the theme
        /// or
        /// Unable to load the accent
        /// </exception>
        private void ApplyTheme(string themeName, string accentName)
        {
            var themeDict = this.LoadThemeDictionary(themeName);

            if (themeDict == null)
            {
                // Requested theme not found. Load default.
                var defaultSettings = new AppSettings().Theme;
                themeDict = this.LoadThemeDictionary(defaultSettings);
            }

            if (themeDict == null)
            {
                throw new NullReferenceException("Unable to load the theme");
            }

            var accentDict = this.LoadAccentDictionary(accentName);

            if (accentDict == null)
            {
                // Requested accent not found. Load default.
                var defaultSettings = new AppSettings().Accent;
                accentDict = this.LoadAccentDictionary(defaultSettings);
            }

            if (accentDict == null)
            {
                throw new NullReferenceException("Unable to load the accent");
            }

            var app = (App)Current;
            
            app.Resources.MergedDictionaries.Clear();
            app.Resources.MergedDictionaries.Add(themeDict);
            app.Resources.MergedDictionaries.Add(accentDict);

            var themes = SwissTool.Application.Managers.WindowManager.Themes;
            var currentTheme = themes.FirstOrDefault(t => t.DirectoryName == themeName);

            WindowManager.CurrentTheme = currentTheme;
            WindowManager.CurrentAccent = accentName;
        }
        
        /// <summary>
        /// Loads the theme dictionary.
        /// </summary>
        /// <param name="themeName">Name of the theme.</param>
        /// <returns>The resource dictionary.</returns>
        private ResourceDictionary LoadThemeDictionary(string themeName)
        {
            ResourceDictionary resourceDict = null;

            var uri = new Uri("/SwissTool;component/Themes/" + themeName + "/Main.xaml", UriKind.Relative);

            try
            {
                resourceDict = LoadComponent(uri) as ResourceDictionary;
            }
            catch (Exception ex)
            {
                Logger.ErrorException(ex, ex.Message);
            }

            return resourceDict;
        }

        /// <summary>
        /// Loads the accent dictionary.
        /// </summary>
        /// <param name="accentName">Name of the accent.</param>
        /// <returns>The resource dictionary.</returns>
        private ResourceDictionary LoadAccentDictionary(string accentName)
        {
            ResourceDictionary resourceDict = null;

            var uri = new Uri("/MahApps.Metro;component/Styles/Accents/" + accentName + ".xaml", UriKind.RelativeOrAbsolute);

            try
            {
                resourceDict = LoadComponent(uri) as ResourceDictionary;
            }
            catch (Exception ex)
            {
                Logger.ErrorException(ex, ex.Message);
            }

            return resourceDict;
        }

        /// <summary>
        /// Catches all unhandled exceptions.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="dispatcherUnhandledExceptionEventArgs">The <see cref="System.Windows.Threading.DispatcherUnhandledExceptionEventArgs"/> instance containing the event data.</param>
        private void DispatcherOnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs dispatcherUnhandledExceptionEventArgs)
        {
            var exception = dispatcherUnhandledExceptionEventArgs.Exception;

            Logger.FatalException(exception, exception.Message);

            MessageBox.Show(
                "SwissTool encountered a unhandled exception. See log file for more details.",
                "Unhandled exception",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            dispatcherUnhandledExceptionEventArgs.Handled = true;
        }

        /// <summary>
        /// The update signal
        /// </summary>
        private enum UpdateSignal
        {
            /// <summary>
            /// Signals that no update has been made.
            /// </summary>
            None,

            /// <summary>
            /// Signals that one or more update installations has failed.
            /// </summary>
            Failed,

            /// <summary>
            /// Signals that all updates were successfully installed.
            /// </summary>
            Success
        }
    }
}
