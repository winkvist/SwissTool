// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApplicationInfo.cs" company="Fredrik Winkvist">
//   Copyright (c) Fredrik Winkvist. All rights reserved.
// </copyright>
// <summary>
//   Defines the ApplicationVersion type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SwissTool.Application.Models
{
    using System;

    /// <summary>
    /// The application version class
    /// </summary>
    [Serializable]
    public class ApplicationInfo
    {
        /// <summary>
        /// Gets or sets the application identifier.
        /// </summary>
        /// <value>The application identifier.</value>
        public Guid Identifier { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>The version.</value>
        public Version Version { get; set; }

        /// <summary>
        /// Gets or sets the directory path.
        /// </summary>
        /// <value>The directory.</value>
        public string Path { get; set; }
    }
}