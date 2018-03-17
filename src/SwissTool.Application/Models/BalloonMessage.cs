// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BalloonMessage.cs" company="Fredrik Winkvist">
//   Copyright (c) Fredrik Winkvist. All rights reserved.
// </copyright>
// <summary>
//   The balloon message class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SwissTool.Application.Models
{
    using Hardcodet.Wpf.TaskbarNotification;

    /// <summary>
    /// The balloon message class.
    /// </summary>
    public class BalloonMessage
    {
        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>The title.</value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>The message.</value>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the icon.
        /// </summary>
        /// <value>The icon.</value>
        public BalloonIcon Icon { get; set; }
    }
}
