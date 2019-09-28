// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainViewModel.cs" company="Fredrik Winkvist">
//   Copyright (c) Fredrik Winkvist. All rights reserved.
// </copyright>
// <summary>
//   Defines the MainViewModel type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SwissTool.Application.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using System.Timers;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    using SwissTool.Application.Managers;
    using SwissTool.Application.Models;
    using SwissTool.Application.Views;
    using SwissTool.Framework.Commanding;
    using SwissTool.Framework.Definitions;
    using SwissTool.Framework.UI.Infrastructure;
    using SwissTool.Logging;

    using WindowManager = SwissTool.Framework.UI.Managers.WindowManager;

    /// <summary>
    /// The main view model.
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// The update timer.
        /// </summary>
        private Timer updateTimer;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModel"/> class.
        /// </summary>
        public MainViewModel()
        {
            this.ExtensionMenuItems = new ObservableCollection<MenuItem>();

            this.ExitCommand = new RelayCommand(o => this.Exit());
            this.ShowAboutCommand = new RelayCommand(o => this.ShowAboutWindow());
            this.CheckForUpdatesCommand = new RelayCommand(o => this.CheckForUpdates());
            this.ShowSettingsCommand = new RelayCommand(o => this.ShowSettingsWindow());
        }

        /// <summary>
        /// Occurs when [show balloon message].
        /// </summary>
        public event Action<string, string, Framework.Enums.BalloonIcon> RequestShowBalloonToolTip;

        /// <summary>
        /// Occurs when [request apply hotkey definitions].
        /// </summary>
        public event Action<bool> RequestApplyHotkeyDefinitions;

        /// <summary>
        /// Occurs when [request apply theme].
        /// </summary>
        public event Action<string, string> RequestApplyTheme;

        /// <summary>
        /// Gets the app name and version.
        /// </summary>
        /// <value>The app name and version.</value>
        public string AppNameAndVersion
        {
            get
            {
                return $"{ApplicationManager.Application.Name} v{ApplicationManager.Application.Version.Major}.{ApplicationManager.Application.Version.Minor}";
            }
        }

        /// <summary>
        /// Gets the name of the app.
        /// </summary>
        /// <value>The name of the app.</value>
        public string AppName
        {
            get
            {
                return ApplicationManager.Application.Name;
            }
        }

        /// <summary>
        /// Gets or sets the extension menu items.
        /// </summary>
        /// <value>The extension menu items.</value>
        public ObservableCollection<MenuItem> ExtensionMenuItems { get; set; }

        /// <summary>
        /// Gets the about display string.
        /// </summary>
        /// <value>The about display string.</value>
        public string AboutDisplayString
        {
            get
            {
                return string.Format("About {0}...", this.AppName);
            }
        }

        /// <summary>
        /// Gets or sets the exit command.
        /// </summary>
        /// <value>
        /// The exit command.
        /// </value>
        public ICommand ExitCommand { get; set; }

        /// <summary>
        /// Gets or sets the show about command.
        /// </summary>
        /// <value>
        /// The show about command.
        /// </value>
        public ICommand ShowAboutCommand { get; set; }

        /// <summary>
        /// Gets or sets the check for updates command.
        /// </summary>
        /// <value>
        /// The check for updates command.
        /// </value>
        public ICommand CheckForUpdatesCommand { get; set; }

        /// <summary>
        /// Gets or sets the show settings command.
        /// </summary>
        /// <value>
        /// The show settings command.
        /// </value>
        public ICommand ShowSettingsCommand { get; set; }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public void Initialize()
        {
            var updateCheckInterval = ApplicationManager.Settings.UpdateCheckInterval;

            if (updateCheckInterval > 0)
            {
                // Sets the update timer interval. Setting is specified in hours.
                this.updateTimer = new Timer(updateCheckInterval * 3600000);
                this.updateTimer.Elapsed += this.UpdateTimerElapsed;

                if (ApplicationManager.Settings.AutomaticallySearchForUpdates)
                {
                    // Starts the timer
                    this.updateTimer.Start();
                }
            }
        }

        /// <summary>
        /// Shows the balloon message.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="message">The message.</param>
        /// <param name="icon">The icon.</param>
        public void ShowBalloonToolTip(string title, string message, Framework.Enums.BalloonIcon icon = Framework.Enums.BalloonIcon.None)
        {
            this.RequestShowBalloonToolTip?.Invoke(title, message, icon);
        }

        /// <summary>
        /// Shows the about window.
        /// </summary>
        public void ShowAboutWindow()
        {
            Logger.Debug("Shows the About-window.");

            var model = new AboutModel(Assembly.GetExecutingAssembly());

            WindowManager.ShowDialog<AboutView>(new AboutViewModel { Model = model });
        }

        /// <summary>
        /// Shows the settings window.
        /// </summary>
        public void ShowSettingsWindow()
        {
            Logger.Debug("Shows the Settings-window.");

            var viewModel = new SettingsViewModel();
            viewModel.RequestApplySettings += this.ApplySettings;

            try
            {
                WindowManager.ShowDialog<SettingsView>(viewModel);
            }
            finally
            {
                viewModel.RequestApplySettings -= this.ApplySettings;
            }
        }

        /// <summary>
        /// Exits the application.
        /// </summary>
        public void Exit()
        {
            this.Close();
        }

        /// <summary>
        /// Checks for updates.
        /// </summary>
        public void CheckForUpdates()
        {
            Logger.Debug("Shows the Update-window.");

            var viewModel = new UpdaterViewModel(this);
            WindowManager.ShowDialog<UpdaterView>(viewModel);
        }

        /// <summary>
        /// Downloads the application updates.
        /// </summary>
        /// <param name="updateInfo">The update info.</param>
        public void DownloadApplicationUpdates(IEnumerable<DetailedPackageVersion> updateInfo)
        {
            try
            {
                Logger.Info("Downloading application updates.");
                PackageManager.DownloadUpdateFiles(updateInfo);
                
                this.ShowBalloonToolTip("Updates downloaded", "The update files were successfully downloaded and will be installed the next time SwissTool is restarted.", Framework.Enums.BalloonIcon.Info);
            }
            catch (Exception ex)
            {
                Logger.Error("Unable to download application updates.");
                this.ShowBalloonToolTip("Update error", "An error occurred while trying to download the application updates: " + ex.Message + ".", Framework.Enums.BalloonIcon.Error);
            }
        }

        /// <summary>
        /// Reloads the menu alternatives.
        /// </summary>
        internal void ReloadMenuAlternatives()
        {
            Logger.Debug("Reloading menu alternatives.");

            this.ExtensionMenuItems.Clear();

            foreach (var extension in ApplicationManager.Extensions)
            {
                var extensionMenuItem = new MenuItem
                {
                    DataContext = extension,
                    Header = extension.Name,
                };

                foreach (var menuItem in extension.MenuItems)
                {
                    var mi = new MenuItem
                    {
                        DataContext = menuItem,
                        Header = menuItem.Name,
                        Tag = menuItem,
                    };

                    mi.Click += MenuItem_ClickHandler;

                    extensionMenuItem.Items.Add(mi);
                }

                this.ExtensionMenuItems.Add(extensionMenuItem);
            }
        }

        /// <summary>
        /// A common event handler.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private static void MenuItem_ClickHandler(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;

            // Get the menu item itself
            var extensionMenuItem = (IExtensionMenuItem)menuItem?.Tag;
            if (extensionMenuItem == null)
            {
                return;
            }

            try
            {
                // Performs the default action
                Logger.Info("Executing action: \"{0}\".", extensionMenuItem.Name);

                ApplicationManager.ExecuteActionCommand(sender, extensionMenuItem);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to perform the requested action.", "Method execution failed", MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.ErrorException(ex, ex.Message);
            }
        }

        /// <summary>
        /// Applies the settings.
        /// </summary>
        private void ApplySettings()
        {
            Logger.Info("Applies main application settings.");

            this.updateTimer.Interval = ApplicationManager.Settings.UpdateCheckInterval * 3600000;

            if (ApplicationManager.Settings.AutomaticallySearchForUpdates)
            {
                this.updateTimer.Start();
            }
            else
            {
                this.updateTimer.Stop();
            }

            this.RequestApplyHotkeyDefinitions?.Invoke(false);
        }
        
        /// <summary>
        /// Executed when the update timer elapsed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Timers.ElapsedEventArgs"/> instance containing the event data.</param>
        private void UpdateTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                Logger.Info("Checking for application updates.");

                var updatesAvailable = PackageManager.CheckApplicationUpdates();

                if (updatesAvailable)
                {
                    Logger.Info("New updates are available.");

                    if (ApplicationManager.Settings.AutomaticallyDownloadAndInstallUpdates)
                    {
                        var includePreReleases = ApplicationManager.Settings.IncludePreReleases;

                        var updates = PackageManager.GetApplicationUpdates(includePreReleases);
                        this.DownloadApplicationUpdates(updates);
                    }
                    else
                    {
                        // Show a balloon message of the update.
                        this.ShowBalloonToolTip("Updates available", "New application updates are available for download.");
                    }
                    
                    // Stop the timer.
                    this.updateTimer.Stop();
                }
                else
                {
                    Logger.Info("No new updates available.");
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorException(ex, ex.Message);
            }
        }
    }
}
