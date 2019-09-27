// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApplicationHost.cs" company="Fredrik Winkvist">
//   Copyright (c) Fredrik Winkvist. All rights reserved.
// </copyright>
// <summary>
//   Defines the ApplicationHost type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Net;

namespace SwissTool.Application
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Interop;

    using SwissTool.Application.Code.Events;
    using SwissTool.Application.Managers;
    using SwissTool.Application.Models;
    using SwissTool.Application.ViewModels;
    using SwissTool.Application.Views;
    using SwissTool.Framework.Definitions;
    using SwissTool.Framework.Enums;
    using SwissTool.Framework.Infrastructure;
    using SwissTool.Logging;

    using BalloonIcon = SwissTool.Framework.Enums.BalloonIcon;

    /// <summary>
    /// The application base class.
    /// </summary>
    [Export(typeof(IHost))]
    public class ApplicationHost : ApplicationBase, IHost, IPartImportsSatisfiedNotification
    {
        /// <summary>
        /// The hotkey constant.
        /// </summary>
        public const int WM_HOTKEY = 0x0312;

        /// <summary>
        /// Gets the main view model.
        /// </summary>
        private MainViewModel mainViewModel;

        /// <summary>
        /// Gets or sets the window helper.
        /// </summary>
        /// <value>The window helper.</value>
        private WindowInteropHelper windowHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationHost"/> class.
        /// </summary>
        public ApplicationHost()
        {
            ComponentDispatcher.ThreadPreprocessMessage += this.ComponentDispatcherThreadPreprocessMessage;

            ApplicationManager.Setup(this);
            PackageManager.Setup(this);
        }

        /// <summary>
        /// The theme changed event handler.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="ThemeChangedEventArgs"/> instance containing the event data.</param>
        public delegate void ThemeChangedEventHandler(object sender, ThemeChangedEventArgs args);

        /// <summary>
        /// Occurs when the theme has changed.
        /// </summary>
        public event ThemeChangedEventHandler ThemeChanged;

        /// <summary>
        /// Gets or sets the settings.
        /// </summary>
        /// <value>The settings.</value>
        public AppSettings Settings { get; set; }

        /// <summary>
        /// Gets the main view.
        /// </summary>
        /// <value>The main view.</value>
        public MainView MainWindow { get; private set; }

        /// <summary>
        /// Gets or sets the hot keys.
        /// </summary>
        /// <value>The hot keys.</value>
        public IList<Models.HotKey> HotKeys { get; set; } = new List<Models.HotKey>();

        /// <summary>
        /// Gets the extension lookup.
        /// </summary>
        /// <value>The extension lookup.</value>
        private Dictionary<string, IExtension> ExtensionLookup { get; } = new Dictionary<string, IExtension>();

        /// <summary>
        /// Gets or sets the extensions.
        /// </summary>
        /// <value>The extensions.</value>
        [ImportMany]
        private IEnumerable<IExtension> Extensions { get; set; }

        /// <summary>
        /// Registers the hot key.
        /// </summary>
        /// <param name="hWnd">The h WND.</param>
        /// <param name="id">The id.</param>
        /// <param name="fsModifiers">The fs modifiers.</param>
        /// <param name="vlc">The VLC.</param>
        /// <returns>A boolean value.</returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, HotKeyModifier fsModifiers, Framework.Enums.HotKey vlc);

        /// <summary>
        /// Unregisters the hot key.
        /// </summary>
        /// <param name="hWnd">The h WND.</param>
        /// <param name="id">The id.</param>
        /// <returns>A boolean value.</returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        /// <summary>
        /// Initializes the application.
        /// </summary>
        public void Initialize()
        {
            this.Settings = this.LoadConfiguration<AppSettings>();
            
            var mainView = new MainView();

            var viewModel = new MainViewModel();
            viewModel.RequestClose += mainView.Close;
            viewModel.RequestShowBalloonToolTip += this.ShowBalloonToolTip;
            viewModel.RequestApplyHotkeyDefinitions += this.ApplyHotKeyDefinitions;
            viewModel.RequestApplyTheme += (themeName, accent) => this.OnThemeChanged(new ThemeChangedEventArgs(themeName, accent));
            mainView.DataContext = viewModel;

            this.MainWindow = mainView;
            this.mainViewModel = viewModel;
            
            this.windowHelper = new WindowInteropHelper(mainView);

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            ApplicationManager.Setup(this.Settings);

            WindowManager.Initialize();
            
            this.mainViewModel.Initialize();
        }

        /// <summary>
        /// Loads the extensions.
        /// </summary>
        public void LoadExtensions()
        {
            this.ComposeParts();
        }

        /// <summary>
        /// Shows the balloon tool tip.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="message">The message.</param>
        /// <param name="balloonIcon">The balloon icon.</param>
        public void ShowBalloonToolTip(string title, string message, BalloonIcon balloonIcon = BalloonIcon.None)
        {
            var enumName = Enum.GetName(typeof(BalloonIcon), balloonIcon);
            Hardcodet.Wpf.TaskbarNotification.BalloonIcon taskBarBalloonIcon;

            if (!Enum.TryParse(enumName, true, out taskBarBalloonIcon))
            {
                throw new ApplicationException("Invalid balloon icon symbol.");
            }

            this.MainWindow.ShowBalloonMessage(new BalloonMessage { Title = title, Message = message, Icon = taskBarBalloonIcon });
        }

        /// <summary>
        /// Called when all part's imports have been satisfied and it is safe to use.
        /// </summary>
        public void OnImportsSatisfied()
        {
            var errors = false;

            // Initialize all the extension modules
            foreach (var extension in this.Extensions)
            {
                try
                {
                    var key = extension.Identifier.ToString();
                  
                    if (string.IsNullOrEmpty(key))
                    {
                        throw new ApplicationException($"The extension {extension.Name ?? "Noname"} has an invalid identifier.");
                    }

                    if (this.ExtensionLookup.ContainsKey(key))
                    {
                        throw new ApplicationException("Extension uniqueness validation failed. Duplicated identifiers exists.");
                    }

                    this.ExtensionLookup.Add(key, extension);

                    Logger.Info("Initializing {0} extension.", extension.Name);
                    extension.Initialize();
                }
                catch (Exception ex)
                {
                    Logger.ErrorException(ex, ex.Message);
                    errors = true;
                }
            }

            if (errors)
            {
                this.ShowBalloonToolTip("Initialize extensions failed", "One or more extensions couldn't be initialized. Check log file for more information.", BalloonIcon.Error);
            }

            ApplicationManager.Setup(this.Extensions);

            this.mainViewModel.ReloadMenuAlternatives();

            // Applies the extension hotkey definitions
            this.ApplyHotKeyDefinitions(true);
        }

        /// <summary>
        /// Raises the <see cref="E:ThemeChanged" /> event.
        /// </summary>
        /// <param name="args">The <see cref="ThemeChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnThemeChanged(ThemeChangedEventArgs args)
        {
            this.ThemeChanged?.Invoke(this, args);
        }

        /// <summary>
        /// Registers global hot keys.
        /// </summary>
        private void RegisterHotKeys()
        {
            Logger.Debug("Registering global hotkeys.");

            for (var i = 0; i < this.HotKeys.Count; i++)
            {
                var hotKey = this.HotKeys[i];

                hotKey.Registered = RegisterHotKey(this.windowHelper.Handle, i, hotKey.ModifierKey | hotKey.SecondModifierKey, hotKey.Key);
            }
        }

        /// <summary>
        /// Unregister global hotkeys
        /// </summary>
        private void UnregisterHotKeys()
        {
            Logger.Debug("Unregistering global hotkeys.");

            for (var i = 0; i < this.HotKeys.Count; i++)
            {
                var hotKey = this.HotKeys[i];
                hotKey.Registered = UnregisterHotKey(this.windowHelper.Handle, i);
            }
        }

        /// <summary>
        /// Composes the parts.
        /// </summary>
        private void ComposeParts()
        {
            var errors = false;

            // Load the modules.
            var extensionCatalogs = new AggregateCatalog();
            var extensionsDirectory = Path.Combine(this.HomeDirectory, "Extensions");

            // Create the extension directory if it doesn't exist.
            if (!Directory.Exists(extensionsDirectory))
            {
                Directory.CreateDirectory(extensionsDirectory);
                Logger.Debug("Missing directory {0} has been re-created.", extensionsDirectory);
            }

            foreach (var subDirectory in Directory.GetDirectories(extensionsDirectory))
            {
                var catalog = new DirectoryCatalog(subDirectory);

                try
                {
                    if (catalog.Parts.Any())
                    {
                        extensionCatalogs.Catalogs.Add(catalog);
                        Logger.Debug("Adding a new DirectoryCatalog {0}.", subDirectory);
                    }
                }
                catch (Exception ex)
                {
                    Logger.ErrorException(ex, ex.Message);
                    errors = true;
                }
            }

            try
            {
                Logger.Debug("Loading composition parts.");
                var compositionContainer = new CompositionContainer(extensionCatalogs);

                compositionContainer.ComposeExportedValue(nameof(IHost), this);
                
                compositionContainer.ComposeParts(this);
            }
            catch (Exception ex)
            {
                Logger.ErrorException(ex, ex.Message);
                errors = true;
            }

            if (errors)
            {
                this.ShowBalloonToolTip("Loading extensions failed", "One or more extensions failed to load. Check log file for more information.", BalloonIcon.Error);
            }
        }

        /// <summary>
        /// Components the dispatcher thread preprocess message.
        /// </summary>
        /// <param name="msg">The message.</param>
        /// <param name="handled">if set to <c>true</c> [handled].</param>
        private void ComponentDispatcherThreadPreprocessMessage(ref MSG msg, ref bool handled)
        {
            if (msg.message == WM_HOTKEY)
            {
                // Handle hot key kere
                var hotKeyIndex = (int)msg.wParam;
                var hotKey = this.HotKeys.ElementAt(hotKeyIndex);

                try
                {
                    ApplicationManager.ExecuteActionCommand(this, hotKey.Action);
                }
                catch (Exception ex)
                {
                    Logger.ErrorException(ex, ex.Message);
                    MessageBox.Show("Unable to perform the requested action.", "Method execution failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Applies the hot key definitions.
        /// </summary>
        /// <param name="useDefaults">if set to <c>true</c> [use defaults].</param>
        private void ApplyHotKeyDefinitions(bool useDefaults)
        {
            Logger.Info("Applies the main application hotkey definitions.");

            this.UnregisterHotKeys();

            this.HotKeys.Clear();
            
            var allActions = new Dictionary<string, IExtensionHotKeyAction>();

            // Locate all actions and their binding paths.
            foreach (var extension in this.Extensions)
            {
                foreach (var action in extension.Actions)
                {
                    var bindingPath = $"{extension.Identifier}/{action.Identifier}";

                    if (!allActions.ContainsKey(bindingPath))
                    {
                        allActions.Add(bindingPath, action);
                    }
                }
            }

            // Locate all hotkey bound extensions.
            var reservedHotKeys = new Dictionary<string, Models.HotKey>();
            var reservedHotKeyDuplicates = new List<HotKeyDefinition>();
            var faultyReservedHotKeys = new List<HotKeyDefinition>();

            foreach (var hotKeyDefinition in this.Settings.HotKeyDefinitions)
            {
                try
                {
                    var extensionIdentifier = hotKeyDefinition.BindingPath.Split(new[] { '/' }).First();
                    var action = allActions[hotKeyDefinition.BindingPath];
                    var extension = this.ExtensionLookup[extensionIdentifier];

                    var hotKey = Models.HotKey.FromDefinition(hotKeyDefinition, action, extension);

                    if (!allActions.ContainsKey(hotKeyDefinition.BindingPath))
                    {
                        // If the action is not found for any of the loaded extensions, skip it.
                        continue;
                    }

                    if (reservedHotKeys.ContainsKey(hotKeyDefinition.BindingPath))
                    {
                        reservedHotKeyDuplicates.Add(hotKeyDefinition);
                        continue;
                    }

                    reservedHotKeys.Add(hotKeyDefinition.BindingPath, hotKey);
                }
                catch (Exception ex)
                {
                    Logger.Info(ex.Message);
                    faultyReservedHotKeys.Add(hotKeyDefinition);
                }
            }

            var saveSettings = false;

            // Remove duplicates
            if (faultyReservedHotKeys.Count > 0)
            {
                Logger.Debug("Removing hotkey assignments due to an error.");
                this.Settings.HotKeyDefinitions.RemoveAll(faultyReservedHotKeys.Contains);
                saveSettings = true;
            }

            if (reservedHotKeyDuplicates.Count > 0)
            {
                Logger.Debug("Removing hotkey assignments due to duplicated assignments.");
                this.Settings.HotKeyDefinitions.RemoveAll(reservedHotKeyDuplicates.Contains);
                saveSettings = true;
            }

            if (saveSettings)
            {
                this.SaveConfiguration(this.Settings);
            }

            var reservedHotKeyByDefinition = new Dictionary<string, Models.HotKey>();
            var duplicatesFound = false;
            var appliedDefaultKeys = false;

            // Apply the reserved hotkeys or the default values.
            foreach (var extension in this.Extensions)
            {
                foreach (var action in extension.Actions)
                {
                    var bindingPath = $"{extension.Identifier}/{action.Identifier}";

                    if (reservedHotKeys.ContainsKey(bindingPath))
                    {
                        // Primary, if available, use the reserved key combination.
                        var hotKey = reservedHotKeys[bindingPath];
                        var definition = hotKey.GetDefinition();

                        if (!reservedHotKeyByDefinition.ContainsKey(definition))
                        {
                            this.HotKeys.Add(hotKey);
                            reservedHotKeyByDefinition.Add(definition, hotKey);
                        }
                        else
                        {
                            duplicatesFound = true;
                        }
                    }
                    else if (useDefaults && action.DefaultHotKey != null)
                    {
                        // Secondary, if available, use the default key combination.
                        var hotKey = Models.HotKey.FromExtensionHotKeyAction(action, extension);
                        var definition = hotKey.GetDefinition();

                        if (!reservedHotKeyByDefinition.ContainsKey(definition))
                        {
                            this.HotKeys.Add(hotKey);
                            reservedHotKeyByDefinition.Add(definition, hotKey);
                            this.Settings.HotKeyDefinitions.Add(hotKey.ToDefinition());
                            appliedDefaultKeys = true;
                        }
                        else
                        {
                            duplicatesFound = true;
                        }
                    }
                }
            }

            if (appliedDefaultKeys)
            {
                this.SaveConfiguration(this.Settings);
            }

            if (duplicatesFound)
            {
                this.ShowBalloonToolTip("Duplicated extension hotkeys", "Unable to apply all extension hotkeys due to duplicated key combinations.", BalloonIcon.Warning);
            }

            ApplicationManager.Setup(this.HotKeys);

            this.RegisterHotKeys();
        }
    }
}
