// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApplicationVersionViewModel.cs" company="Fredrik Winkvist">
//   Copyright (c) Fredrik Winkvist. All rights reserved.
// </copyright>
// <summary>
//   The application version view model.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SwissTool.Application.ViewModels
{
    using System;
    using System.Linq;

    using SwissTool.Application.Models;
    using SwissTool.Framework.UI.Infrastructure;

    /// <summary>
    /// The application version view model.
    /// </summary>
    public class ApplicationVersionViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationVersionViewModel"/> class.
        /// </summary>
        public ApplicationVersionViewModel()
        {
            this.Install = true;
            this.Model = new DetailedPackageVersion();
        }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>The model.</value>
        public DetailedPackageVersion Model { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ApplicationVersionViewModel"/> is install.
        /// </summary>
        /// <value><c>true</c> if install; otherwise, <c>false</c>.</value>
        public bool Install { get; set; }

        /// <summary>
        /// Gets the version string.
        /// </summary>
        /// <value>The version string.</value>
        public string VersionString => "v." + this.Model.Version;

        /// <summary>
        /// Gets the download file size string.
        /// </summary>
        /// <value>The download file size string.</value>
        public string DownloadFileSizeString
        {
            get
            {
                var kiloBytes = (decimal)this.Model.DownloadFileSize / (decimal)1024;

                if (kiloBytes > 1024)
                {
                    var megaBytes = (decimal)kiloBytes / (decimal)1024;
                    var roundedMegaBytes = Math.Round((decimal)megaBytes, 2);

                    return roundedMegaBytes + " MB";
                }

                var rounded = Math.Round((decimal)kiloBytes, 2);

                return rounded + " KB";
            }
        }

        /// <summary>
        /// Gets the changes.
        /// </summary>
        /// <value>The changes.</value>
        public string ChangeLog
        {
            get
            {
                if (this.Model.Changes == null || this.Model.Changes.Count == 0)
                {
                    return string.Empty;
                }

                var changeLog = string.Empty;

                changeLog += "Changes:" + Environment.NewLine;

                return this.Model.Changes.Aggregate(changeLog, (current, change) => current + ("* " + change + Environment.NewLine));
            }
        }
    }
}
