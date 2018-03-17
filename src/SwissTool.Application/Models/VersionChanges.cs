// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VersionChanges.cs" company="Fredrik Winkvist">
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
    /// The version changes class.
    /// </summary>
    public class VersionChanges
    {
        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>The version.</value>
        public Version Version { get; set; }

        /// <summary>
        /// Gets or sets the released date.
        /// </summary>
        /// <value>
        /// The released date.
        /// </value>
        public DateTime ReleasedDate { get; set; }

        /// <summary>
        /// Gets or sets the changes.
        /// </summary>
        /// <value>
        /// The changes.
        /// </value>
        public List<string> Changes { get; set; } 
    }
}
