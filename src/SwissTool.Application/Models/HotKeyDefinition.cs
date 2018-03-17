// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HotKeyDefinition.cs" company="Fredrik Winkvist">
//   Copyright (c) Fredrik Winkvist. All rights reserved.
// </copyright>
// <summary>
//   Defines the HotKeyDefinition type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SwissTool.Application.Models
{
    /// <summary>
    /// The hotkey definition class.
    /// </summary>
    public class HotKeyDefinition
    {
        /// <summary>
        /// Gets or sets the binding path.
        /// </summary>
        /// <value>
        /// The binding path.
        /// </value>
        public string BindingPath { get; set; }

        /// <summary>
        /// Gets or sets the definition.
        /// </summary>
        /// <value>
        /// The definition.
        /// </value>
        public string Definition { get; set; }
    }
}
