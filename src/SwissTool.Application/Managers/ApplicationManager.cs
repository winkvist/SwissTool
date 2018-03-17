// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApplicationManager.cs" company="Fredrik Winkvist">
//   Copyright (c) Fredrik Winkvist. All rights reserved.
// </copyright>
// <summary>
//   Defines the ApplicationManager type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SwissTool.Application.Managers
{
    using System.Collections.Generic;

    using SwissTool.Application.Models;
    using SwissTool.Framework.Definitions;

    using HotKey = SwissTool.Application.Models.HotKey;

    /// <summary>
    /// Defines the application manager.
    /// </summary>
    public static class ApplicationManager
    {
        /// <summary>
        /// Gets the application.
        /// </summary>
        /// <value>The application.</value>
        internal static IApplication Application { get; private set; }

        /// <summary>
        /// Gets the extensions.
        /// </summary>
        /// <value>The extensions.</value>
        internal static IEnumerable<IExtension> Extensions { get; private set; }

        /// <summary>
        /// Gets the hot keys.
        /// </summary>
        /// <value>The hot keys.</value>
        internal static IEnumerable<HotKey> HotKeys { get; private set; }

        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <value>The settings.</value>
        internal static AppSettings Settings { get; private set; }

        /// <summary>
        /// Gets the hot keys by definition.
        /// </summary>
        /// <value>The hot keys by definition.</value>
        internal static Dictionary<string, HotKey> HotKeysByDefinition 
        { 
            get
            {
                var dict = new Dictionary<string, HotKey>();

                foreach (var hotKey in HotKeys)
                {
                    var definition = hotKey.GetDefinition();
                    if (!dict.ContainsKey(definition))
                    {
                        dict.Add(definition, hotKey);
                    }
                }

                return dict;
            }
        }

        /// <summary>
        /// Setups the specified application.
        /// </summary>
        /// <param name="extensions">The extensions.</param>
        /// <param name="hotKeys">The hot keys.</param>
        /// <param name="settings">The settings.</param>
        internal static void Setup(IEnumerable<IExtension> extensions, IEnumerable<HotKey> hotKeys, AppSettings settings)
        {
            Extensions = extensions;
            HotKeys = hotKeys;
            Settings = settings;
        }

        /// <summary>
        /// Setups the specified extensions.
        /// </summary>
        /// <param name="extensions">The extensions.</param>
        internal static void Setup(IEnumerable<IExtension> extensions)
        {
            Extensions = extensions;
        }

        /// <summary>
        /// Setups the specified settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        internal static void Setup(AppSettings settings)
        {
            Settings = settings;
        }

        /// <summary>
        /// Setups the specified hot keys.
        /// </summary>
        /// <param name="hotKeys">The hot keys.</param>
        internal static void Setup(IEnumerable<HotKey> hotKeys)
        {
            HotKeys = hotKeys;
        }

        /// <summary>
        /// Setups the specified app base.
        /// </summary>
        /// <param name="application">The application.</param>
        internal static void Setup(IApplication application)
        {
            Application = application;
        }

        /// <summary>
        /// Saves the settings.
        /// </summary>
        internal static void SaveSettings()
        {
            Application.SaveConfiguration(Settings);
        }

        /// <summary>
        /// Executes the action command.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="action">The action.</param>
        internal static void ExecuteActionCommand(object sender, IExtensionAction action)
        {
            action.Command?.Execute(sender);
        }
    }
}
