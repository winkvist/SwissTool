// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WindowManager.cs" company="Fredrik Winkvist">
//   Copyright (c) Fredrik Winkvist. All rights reserved.
// </copyright>
// <summary>
//   Defines the WindowManager type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SwissTool.Application.Managers
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    using SwissTool.Framework.UI.Models;
    using SwissTool.Logging;

    /// <summary>
    /// The window manager.
    /// </summary>
    internal static class WindowManager
    {
        /// <summary>
        /// Gets the accents.
        /// </summary>
        /// <value>
        /// The accents.
        /// </value>
        public static List<string> Accents { get; } = new List<string>
                                                          {
                                                              "Red",
                                                              "Green",
                                                              "Blue",
                                                              "Purple",
                                                              "Orange",
                                                              "Lime",
                                                              "Emerald",
                                                              "Teal",
                                                              "Cyan",
                                                              "Cobalt",
                                                              "Indigo",
                                                              "Violet",
                                                              "Pink",
                                                              "Magenta",
                                                              "Crimson",
                                                              "Amber",
                                                              "Yellow",
                                                              "Brown",
                                                              "Olive",
                                                              "Steel",
                                                              "Mauve",
                                                              "Taupe",
                                                              "Sienna"
                                                          };

        /// <summary>
        /// Gets the themes.
        /// </summary>
        /// <value>
        /// The themes.
        /// </value>
        public static List<Theme> Themes { get; private set; } = new List<Theme>();

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        internal static void Initialize()
        {
            LoadThemes();
        }

        /// <summary>
        /// Loads the themes.
        /// </summary>
        private static void LoadThemes()
        {
            var themes = new List<Theme>();

            var applicationDirectory = ApplicationManager.Application.HomeDirectory;
            var themesPath = Path.Combine(applicationDirectory, "Themes");
            var themeDirectories = Directory.GetDirectories(themesPath);

            foreach (var themeDirectory in themeDirectories)
            {
                var themeFilePath = Path.Combine(themeDirectory, "theme.json");

                try
                {
                    var themeFileContents = File.ReadAllText(themeFilePath);

                    var theme = JsonConvert.DeserializeObject<Theme>(themeFileContents, new VersionConverter());
                    theme.Path = themeDirectory;
                    theme.DirectoryName = Path.GetFileName(themeDirectory);
                    
                    themes.Add(theme);
                }
                catch (Exception ex)
                {
                    Logger.ErrorException(ex, "Unable to load the theme {0}.", themeDirectory ?? string.Empty);
                }
            }

            Themes = themes;
        }
    }
}
