// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SettingsViewModel.cs" company="Fredrik Winkvist">
//   Copyright (c) Fredrik Winkvist. All rights reserved.
// </copyright>
// <summary>
//   Defines the SettingsViewModel type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SwissTool.Application.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;

    using Microsoft.Win32;

    using SwissTool.Application.Managers;
    using SwissTool.Application.Models;
    using SwissTool.Framework.Infrastructure;
    using SwissTool.Framework.Commanding;
    using SwissTool.Framework.UI.Infrastructure;
    using SwissTool.Framework.UI.Models;
    using SwissTool.Framework.Utilities.Serialization;
    using SwissTool.Logging;
    
    /// <summary>
    /// The settings view model.
    /// </summary>
    public class SettingsViewModel : ViewModelBase
    {
        /// <summary>
        /// The selected extension.
        /// </summary>
        private ExtensionViewModel selectedExtension;

        /// <summary>
        /// The selected theme
        /// </summary>
        private Theme selectedTheme;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsViewModel"/> class.
        /// </summary>
        public SettingsViewModel()
        {
            this.SaveChangesCommand = new RelayCommand((o) => this.SaveChanges());
            this.SettingsCopy = JsonUtils.Clone(ApplicationManager.Settings);

            this.Extensions = new List<ExtensionViewModel>();

            foreach (var extension in ApplicationManager.Extensions)
            {
                var viewModel = new ExtensionViewModel(this) { Extension = extension };

                foreach (var action in extension.Actions)
                {
                    var assignedHotKey = ApplicationManager.HotKeys.FirstOrDefault(hk => hk.Extension == extension && hk.Action == action);

                    var extensionAction = new ExtensionActionViewModel(viewModel)
                                              {
                                                  Action = action,
                                                  AssignedHotKey = assignedHotKey,
                                                  Extension = extension
                                              };

                    viewModel.Actions.Add(extensionAction);
                }

                this.Extensions.Add(viewModel);
            }

            this.SelectedExtension = this.Extensions.FirstOrDefault();

            this.Themes = WindowManager.Themes;
            this.Accents = WindowManager.Accents;

            this.SelectedTheme = this.Themes.FirstOrDefault(t => t.DirectoryName == this.SettingsCopy.Theme);
        }

        /// <summary>
        /// Occurs when [request apply settings].
        /// </summary>
        public event Action RequestApplySettings;

        /// <summary>
        /// Gets or sets the save changes command.
        /// </summary>
        /// <value>
        /// The save changes command.
        /// </value>
        public ICommand SaveChangesCommand { get; set; }

        /// <summary>
        /// Gets or sets the selected extension.
        /// </summary>
        /// <value>
        /// The selected extension.
        /// </value>
        public ExtensionViewModel SelectedExtension
        {
            get
            {
                return this.selectedExtension;
            }

            set
            {
                this.selectedExtension = value;

                this.NotifyPropertyChanged(nameof(this.SelectedExtension));
            }
        }

        /// <summary>
        /// Gets or sets the selected theme.
        /// </summary>
        /// <value>
        /// The selected theme.
        /// </value>
        public Theme SelectedTheme
        {
            get
            {
                return this.selectedTheme;
            }

            set
            {
                this.selectedTheme = value;
                this.SettingsCopy.Theme = value.DirectoryName;

                this.NotifyPropertyChanged(nameof(this.SelectedTheme));
                this.NotifyPropertyChanged(nameof(this.IsRestartRequired));
            }
        }

        /// <summary>
        /// Gets or sets the selected accent.
        /// </summary>
        /// <value>
        /// The selected accent.
        /// </value>
        public string SelectedAccent
        {
            get
            {
                return this.SettingsCopy.Accent;
            }

            set
            {
                this.SettingsCopy.Accent = value;

                this.NotifyPropertyChanged(nameof(this.SelectedAccent));
                this.NotifyPropertyChanged(nameof(this.IsRestartRequired));
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is restart required.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is restart required; otherwise, <c>false</c>.
        /// </value>
        public bool IsRestartRequired
        {
            get
            {
                return ApplicationManager.Settings.Theme != this.SettingsCopy.Theme 
                    || ApplicationManager.Settings.Accent != this.SettingsCopy.Accent;
            }
        }

        /// <summary>
        /// Gets or sets the extensions.
        /// </summary>
        /// <value>
        /// The extensions.
        /// </value>
        public List<ExtensionViewModel> Extensions { get; set; }

        /// <summary>
        /// Gets or sets the themes.
        /// </summary>
        /// <value>
        /// The themes.
        /// </value>
        public List<Theme> Themes { get; set; }

        /// <summary>
        /// Gets or sets the accents.
        /// </summary>
        /// <value>
        /// The accents.
        /// </value>
        public List<string> Accents { get; set; }

        /// <summary>
        /// Gets or sets the settings copy.
        /// </summary>
        /// <value>
        /// The settings copy.
        /// </value>
        public AppSettings SettingsCopy { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is check for updates enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is check for updates enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsCheckForUpdatesEnabled
        {
            get
            {
                return this.SettingsCopy.AutomaticallySearchForUpdates;
            }
        }

        /// <summary>
        /// Gets the current hot keys.
        /// </summary>
        /// <value>The current hot keys.</value>
        public ILookup<string, HotKey> SettingsHotKeys 
        { 
            get
            {
                return this.Extensions.SelectMany(e => e.Actions)
                    .Where(a => a.AssignedHotKey != null)
                    .Select(a => a.AssignedHotKey)
                    .ToLookup(a => a.GetDefinition());
            }
        }

        /// <summary>
        /// Triggers the UI refresh.
        /// </summary>
        public void TriggerUiRefresh()
        {
            this.NotifyPropertyChanged(nameof(this.IsCheckForUpdatesEnabled));
        }

        /// <summary>
        /// Saves the changes.
        /// </summary>
        public void SaveChanges()
        {
            Logger.Info("Saving application settings.");

            var hotKeys = new List<HotKey>();

            foreach (var extension in this.Extensions)
            {
                var assignedHotKeys = extension.Actions
                    .Where(a => a.AssignedHotKey != null)
                    .Select(a => a.AssignedHotKey).ToList();

                hotKeys.AddRange(assignedHotKeys);
            }

            if (this.Extensions.SelectMany(a => a.Actions).Any(a => a.CollisionDetected))
            {
                MessageBox.Show(
                    "One or more assigned hotkeys use the same key combination. Make sure every action has a unique combination of keys.",
                    "Faulty key combinations",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                
                return;
            }

            ApplicationManager.Settings.HotKeyDefinitions.Clear();

            foreach (var hotKey in hotKeys)
            {
                ApplicationManager.Settings.HotKeyDefinitions.Add(hotKey.ToDefinition());
            }

            ApplicationManager.Settings.AutomaticallySearchForUpdates = this.SettingsCopy.AutomaticallySearchForUpdates;
            ApplicationManager.Settings.AutostartWithWindows = this.SettingsCopy.AutostartWithWindows;
            ApplicationManager.Settings.UpdateCheckInterval = this.SettingsCopy.UpdateCheckInterval;
            ApplicationManager.Settings.AutomaticallyDownloadAndInstallUpdates = this.SettingsCopy.AutomaticallyDownloadAndInstallUpdates;
            ApplicationManager.Settings.Theme = this.SettingsCopy.Theme;
            ApplicationManager.Settings.Accent = this.SettingsCopy.Accent;

            using (var key = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                if (key != null)
                {
                    var appName = AppConstants.ApplicationName;

                    var keyExists = key.GetValue(appName) != null;

                    var applicationDirectory = ApplicationManager.Application.HomeDirectory;
                    var applicationPath = Path.Combine(applicationDirectory, "Loader.exe");

                    if (ApplicationManager.Settings.AutostartWithWindows)
                    {
                        key.SetValue(appName, "\"" + applicationPath + "\"");
                    }
                    else
                    {
                        if (keyExists)
                        {
                            key.DeleteValue(appName); 
                        }
                    }
                }
            }

            ApplicationManager.SaveSettings();

            this.RequestApplySettings?.Invoke();

            this.Close();
        }
    }
}
