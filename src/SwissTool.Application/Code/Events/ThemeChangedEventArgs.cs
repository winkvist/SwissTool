// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ThemeChangedEventArgs.cs" company="Fredrik Winkvist">
//   Copyright (c) Fredrik Winkvist. All rights reserved.
// </copyright>
// <summary>
//   The theme changed event arguments.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SwissTool.Application.Code.Events
{
    using System;

    /// <summary>
    /// The theme changed event arguments.
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class ThemeChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ThemeChangedEventArgs"/> class.
        /// </summary>
        /// <param name="theme">The theme.</param>
        /// <param name="accent">The accent.</param>
        public ThemeChangedEventArgs(string theme, string accent)
        {
            this.Theme = theme;
            this.Accent = accent;
        }

        /// <summary>
        /// Gets the theme.
        /// </summary>
        /// <value>
        /// The theme.
        /// </value>
        public string Theme { get; }

        /// <summary>
        /// Gets the accent.
        /// </summary>
        /// <value>
        /// The accent.
        /// </value>
        public string Accent { get; }
    }
}
