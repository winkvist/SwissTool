// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageInstallationInfo.cs" company="Fredrik Winkvist">
//   Copyright (c) Fredrik Winkvist. All rights reserved.
// </copyright>
// <summary>
//   The information about an installation package.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SwissTool.Application.Models
{
    using System;

    /// <summary>
    /// The information about an installation package.
    /// </summary>
    [Serializable]
    public class PackageInstallationInfo
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
        /// <value>The application path.</value>
        public string ApplicationPath { get; set; }

        /// <summary>
        /// Gets or sets the installation file.
        /// </summary>
        /// <value>The installation file.</value>
        public string InstallationFile { get; set; }
    }
}
