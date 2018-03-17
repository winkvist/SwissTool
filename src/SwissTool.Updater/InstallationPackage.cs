// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InstallationPackage.cs" company="Fredrik Winkvist">
//   Copyright (c) Fredrik Winkvist. All rights reserved.
// </copyright>
// <summary>
//   Defines the InstallationPackage type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SwissTool.Updater
{
    /// <summary>
    /// Defines an installation package.
    /// </summary>
    public class InstallationPackage
    {
        /// <summary>
        /// Gets or sets the application identifier.
        /// </summary>
        /// <value>
        /// The application identifier.
        /// </value>
        public string ApplicationIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the application version.
        /// </summary>
        /// <value>
        /// The application version.
        /// </value>
        public string ApplicationVersion { get; set; }

        /// <summary>
        /// Gets or sets the application path.
        /// </summary>
        /// <value>
        /// The application path.
        /// </value>
        public string ApplicationPath { get; set; }

        /// <summary>
        /// Gets or sets the installation file.
        /// </summary>
        /// <value>
        /// The installation file.
        /// </value>
        public string InstallationFile { get; set; }
    }
}
