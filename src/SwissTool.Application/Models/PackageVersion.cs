// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageVersion.cs" company="Fredrik Winkvist">
//   Copyright (c) Fredrik Winkvist. All rights reserved.
// </copyright>
// <summary>
//   The applicatoin version model class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SwissTool.Application.Models
{
    using System;

    /// <summary>
    /// The application version model class.
    /// </summary>
    public class PackageVersion
    {
        /// <summary>
        /// Gets or sets the application identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public Guid Identifier { get; set; }

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>The version.</value>
        public Version Version { get; set; }
    }
}
