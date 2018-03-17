// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DetailedPackageVersion.cs" company="Fredrik Winkvist">
//   Copyright (c) Fredrik Winkvist. All rights reserved.
// </copyright>
// <summary>
//   The applicatoin version model class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SwissTool.Application.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The application version model class.
    /// </summary>
    public class DetailedPackageVersion
    {
        /// <summary>
        /// Gets or sets the application identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public Guid Identifier { get; set; }

        /// <summary>
        /// Gets or sets the name of the application.
        /// </summary>
        /// <value>The name of the application.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>The version.</value>
        public Version Version { get; set; }

        /// <summary>
        /// Gets or sets the size of the download file.
        /// </summary>
        /// <value>The size of the download file.</value>
        public long DownloadFileSize { get; set; }

        /// <summary>
        /// Gets or sets the released date.
        /// </summary>
        /// <value>
        /// The released date.
        /// </value>
        public DateTime ReleasedDate { get; set; }

        /// <summary>
        /// Gets or sets the download URL.
        /// </summary>
        /// <value>The download URL.</value>
        public string DownloadUrl { get; set; }

        /// <summary>
        /// Gets or sets the download filename.
        /// </summary>
        /// <value>
        /// The download filename.
        /// </value>
        public string DownloadFilename { get; set; }

        /// <summary>
        /// Gets or sets the changes.
        /// </summary>
        /// <value>The changes.</value>
        public List<string> Changes { get; set; }
    }
}
