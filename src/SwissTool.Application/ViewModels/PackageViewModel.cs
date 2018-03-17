// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageViewModel.cs" company="Fredrik Winkvist">
//   Copyright (c) Fredrik Winkvist. All rights reserved.
// </copyright>
// <summary>
//   The application version view model.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SwissTool.Application.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SwissTool.Application.Models;
    using SwissTool.Framework.UI.Infrastructure;

    /// <summary>
    /// The application version view model.
    /// </summary>
    public class PackageViewModel : ViewModelBase
    {
        /// <summary>
        /// The parent view model
        /// </summary>
        private readonly UpdaterViewModel parent;

        /// <summary>
        /// The model
        /// </summary>
        private List<DetailedPackageVersion> model;

        /// <summary>
        /// The install
        /// </summary>
        private bool install;

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageViewModel"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        public PackageViewModel(UpdaterViewModel parent)
            : this()
        {
            this.parent = parent;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageViewModel"/> class. 
        /// </summary>
        public PackageViewModel()
        {
            this.Install = true;
            this.Model = new List<DetailedPackageVersion>();
        }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>The model.</value>
        public List<DetailedPackageVersion> Model
        {
            get
            {
                return this.model;
            }

            set
            {
                if (value != null)
                {
                    this.model = value.OrderBy(p => p.Version).ToList();
                    this.LatestVersion = this.model.LastOrDefault();
                }
                else
                {
                    this.model = value;
                }

                this.NotifyPropertyChanged(nameof(this.Model));
            }
        }

        /// <summary>
        /// Gets or sets the latest version.
        /// </summary>
        /// <value>
        /// The latest version.
        /// </value>
        public DetailedPackageVersion LatestVersion { get; set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name
        {
            get
            {
                return this.LatestVersion != null ? this.LatestVersion.Name : string.Empty;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ApplicationVersionViewModel"/> is install.
        /// </summary>
        /// <value><c>true</c> if install; otherwise, <c>false</c>.</value>
        public bool Install
        {
            get
            {
                return this.install;
            }

            set
            {
                this.install = value;
                this.NotifyPropertyChanged(nameof(this.Install));

                this.parent?.RequeryCheckedPackages();
            }
        }

        /// <summary>
        /// Gets the version string.
        /// </summary>
        /// <value>The version string.</value>
        public string VersionString
        {
            get
            {
                return "v." + this.LatestVersion.Version;
            }
        }

        /// <summary>
        /// Gets the download file size string.
        /// </summary>
        /// <value>The download file size string.</value>
        public string DownloadFileSizeString
        {
            get
            {
                var fileSizeSum = this.Model.Sum(m => m.DownloadFileSize);

                var kiloBytes = (decimal)fileSizeSum / (decimal)1024;

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
        public List<VersionChanges> ChangeLog
        {
            get
            {
                var orderedPackages = this.model.OrderByDescending(m => m.Version).ToList();
                return orderedPackages.Select(package => new VersionChanges { Version = package.Version, ReleasedDate = package.ReleasedDate, Changes = package.Changes }).ToList();
            }
        }

        /// <summary>
        /// Gets the change log string.
        /// </summary>
        /// <value>
        /// The change log string.
        /// </value>
        public string ChangeLogString
        {
            get
            {
                var changeLog = string.Empty;

                foreach (var versionChanges in this.ChangeLog)
                {
                    changeLog += "v." + versionChanges.Version + " - " + System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(versionChanges.ReleasedDate.ToString("d MMMM, yyyy")) + Environment.NewLine;
                    changeLog = versionChanges.Changes.Aggregate(changeLog, (current, change) => current + ("   * " + change + Environment.NewLine));
                    changeLog += Environment.NewLine;
                }

                return changeLog.Trim();
            }
        }
    }
}
