// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AppSettings.cs" company="Fredrik Winkvist">
//   Copyright (c) Fredrik Winkvist. All rights reserved.
// </copyright>
// <summary>
//   Defines the AppSettings type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SwissTool.Application.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// The app settings class.
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppSettings"/> class.
        /// </summary>
        public AppSettings()
        {
            this.HotKeyDefinitions = new List<HotKeyDefinition>();
            this.AutostartWithWindows = false;
            this.AutomaticallySearchForUpdates = true;
            this.UpdateCheckInterval = 8;
            this.AutomaticallyDownloadAndInstallUpdates = false;
            this.Theme = "Light";
            this.IncludePreReleases = false;
            this.Accent = "Blue";
        }

        /// <summary>
        /// Gets or sets the hot key definitions.
        /// </summary>
        /// <value>
        /// The hot key definitions.
        /// </value>
        public List<HotKeyDefinition> HotKeyDefinitions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to auto start with windows.
        /// </summary>
        /// <value>
        /// <c>true</c> if auto start with windows; otherwise, <c>false</c>.
        /// </value>
        public bool AutostartWithWindows { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to automatically search for updates.
        /// </summary>
        /// <value>
        /// <c>true</c> if automatically search for updates; otherwise, <c>false</c>.
        /// </value>
        public bool AutomaticallySearchForUpdates { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to automatically download and install updates.
        /// </summary>
        /// <value>
        /// <c>true</c> if [automatically download and install updates]; otherwise, <c>false</c>.
        /// </value>
        public bool AutomaticallyDownloadAndInstallUpdates { get; set; }

        /// <summary>
        /// Gets or sets the update check interval.
        /// </summary>
        /// <value>
        /// The update check interval.
        /// </value>
        public int UpdateCheckInterval { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include pre releases.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [include pre releases]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludePreReleases { get; set; }

        /// <summary>
        /// Gets or sets the theme.
        /// </summary>
        /// <value>
        /// The theme.
        /// </value>
        public string Theme { get; set; }

        /// <summary>
        /// Gets or sets the accent.
        /// </summary>
        /// <value>
        /// The accent.
        /// </value>
        public string Accent { get; set; }
    }
}