// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UpdaterViewModel.cs" company="Fredrik Winkvist">
//   Copyright (c) Fredrik Winkvist. All rights reserved.
// </copyright>
// <summary>
//   Defines the UpdaterViewModel type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SwissTool.Application.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;

    using SwissTool.Application.Managers;
    using SwissTool.Application.Models;
    using SwissTool.Framework.UI.Commanding;
    using SwissTool.Framework.UI.Infrastructure;
    using SwissTool.Logging;

    using PackageManager = SwissTool.Application.Managers.PackageManager;

    /// <summary>
    /// The updater view model.
    /// </summary>
    public class UpdaterViewModel : ViewModelBase
    {
        /// <summary>
        /// Indicates whether the application is checking for updates.
        /// </summary>
        private bool fetchingUpdates;

        /// <summary>
        /// The list of available updates.
        /// </summary>
        private List<PackageViewModel> availableUpdates;

        /// <summary>
        /// The selected application version.
        /// </summary>
        private PackageViewModel selectedPackage;

        /// <summary>
        /// The status message
        /// </summary>
        private string statusMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdaterViewModel"/> class.
        /// </summary>
        /// <param name="mainViewModel">The main view model.</param>
        public UpdaterViewModel(MainViewModel mainViewModel)
            : this()
        {
            this.Parent = mainViewModel;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdaterViewModel"/> class.
        /// </summary>
        public UpdaterViewModel()
        {
            this.AvailableUpdates = new List<PackageViewModel>();
            this.FetchingUpdates = false;
            this.UpdateCommand = new RelayCommand(o => this.PerformUpdate(), o => this.HasCheckedPackages);
            this.FetchUpdatesCommand = new RelayCommand(o => this.GetAvailableUpdatesAsync());
        }

        /// <summary>
        /// Gets a value indicating whether this instance has checked packages.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has checked packages; otherwise, <c>false</c>.
        /// </value>
        public bool HasCheckedPackages
        {
            get
            {
                if (this.AvailableUpdates == null)
                {
                    return false;
                }

                return this.AvailableUpdates.Any(u => u.Install);
            }
        }

        /// <summary>
        /// Gets or sets the available updates.
        /// </summary>
        /// <value>The available updates.</value>
        public List<PackageViewModel> AvailableUpdates
        {
            get
            {
                return this.availableUpdates;
            }

            set
            {
                this.availableUpdates = value;
                this.NotifyPropertyChanged(nameof(this.AvailableUpdates));
                this.NotifyPropertyChanged(nameof(this.HasCheckedPackages));
            }
        }
    
        /// <summary>
        /// Gets or sets a value indicating whether the application is fetching updates.
        /// </summary>
        /// <value><c>true</c> if  the application is fetching updates; otherwise, <c>false</c>.</value>
        public bool FetchingUpdates
        {
            get
            {
                return this.fetchingUpdates;
            } 

            set
            {
                this.fetchingUpdates = value;

                this.NotifyPropertyChanged(nameof(this.FetchingUpdates));
                this.NotifyPropertyChanged(nameof(this.IsUpdatesListVisible));
            }
        }

        /// <summary>
        /// Gets a value indicating whether the list of updates is visible.
        /// </summary>
        /// <value>
        /// <c>true</c> if it's visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsUpdatesListVisible
        {
            get
            {
                return !this.FetchingUpdates && this.AvailableUpdates.Any();
            }
        }

        /// <summary>
        /// Gets or sets the status message.
        /// </summary>
        /// <value>
        /// The status message.
        /// </value>
        public string StatusMessage
        {
            get
            {
                return this.statusMessage;
            }

            set
            {
                this.statusMessage = value;

                this.NotifyPropertyChanged(nameof(this.StatusMessage));
            }
        }

        /// <summary>
        /// Gets the update command.
        /// </summary>
        /// <value>The update command.</value>
        public ICommand UpdateCommand { get; private set; }

        /// <summary>
        /// Gets the fetch updates command.
        /// </summary>
        /// <value>
        /// The fetch updates command.
        /// </value>
        public ICommand FetchUpdatesCommand { get; private set; }

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        /// <value>The parent.</value>
        public MainViewModel Parent { get; set; }

        /// <summary>
        /// Gets or sets the selected application version.
        /// </summary>
        /// <value>The selected application version.</value>
        public PackageViewModel SelectedPackage
        {
            get
            {
                return this.selectedPackage;
            }

            set
            {
                this.selectedPackage = value;
                this.NotifyPropertyChanged(nameof(this.SelectedPackage));
                this.NotifyPropertyChanged(nameof(this.SelectedPackageChangeLog));
            }
        }

        /// <summary>
        /// Gets the selected application version description.
        /// </summary>
        /// <value>The selected application version description.</value>
        public string SelectedPackageChangeLog
        {
            get
            {
                return this.selectedPackage == null 
                    ? string.Empty
                    : this.selectedPackage.ChangeLogString;
            }
        }

        /// <summary>
        /// Gets the available updates asynchronous.
        /// </summary>
        public void GetAvailableUpdatesAsync()
        {
            Task.Run(() => this.GetAvailableUpdates());
        }

        /// <summary>
        /// Gets the available updates.
        /// </summary>
        public void GetAvailableUpdates()
        {
            try
            {
                Logger.Info("Checking repository for new updates.");

                this.StatusMessage = "Checking for updates...";
                this.FetchingUpdates = true;

                var includePreReleases = ApplicationManager.Settings.IncludePreReleases;

                var applicationUpdates = PackageManager.GetApplicationUpdates(includePreReleases);

                this.AvailableUpdates = applicationUpdates
                        .OrderBy(a => a.Identifier)
                        .ThenBy(a => a.Version)
                        .GroupBy(a => a.Identifier)
                        .Select(u => new PackageViewModel(this)
                                {
                                    Model = u.Select(auv =>
                                            new DetailedPackageVersion
                                                {
                                                    Identifier = auv.Identifier,
                                                    Name = auv.Name,
                                                    DownloadFileSize = auv.DownloadFileSize,
                                                    DownloadUrl = auv.DownloadUrl,
                                                    DownloadFilename = auv.DownloadFilename,
                                                    ReleasedDate = auv.ReleasedDate,
                                                    Version = auv.Version,
                                                    Changes = auv.Changes
                                                })
                                        .OrderBy(auv => auv.Version)
                                        .ToList()
                                })
                        .ToList();

                this.SelectedPackage = this.AvailableUpdates.FirstOrDefault();

                if (this.availableUpdates == null || this.availableUpdates.Count == 0)
                {
                    Logger.Info("No new updates were found in the repository.");
                    this.StatusMessage = "There are no updates available at this time";
                }
                else
                {
                    Logger.Info("New updates were found in the repository.");
                    this.StatusMessage = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorException(ex, "An error occurred while fetching updates.");
                this.StatusMessage = "An error occurred while fetching updates";
            }
            finally
            {
                this.FetchingUpdates = false;
            }
        }

        /// <summary>
        /// Requeries the checked packages.
        /// </summary>
        public void RequeryCheckedPackages()
        {
            this.NotifyPropertyChanged(nameof(this.HasCheckedPackages));
        }

        /// <summary>
        /// Performs an update of the selected applications and extensions.
        /// </summary>
        public void PerformUpdate()
        {
            var updatesToInstall = this.AvailableUpdates.Where(au => au.Install).ToList();

            if (!updatesToInstall.Any())
            {
                MessageBox.Show(
                    Application.Current.MainWindow,
                    "At least one item must be selected",
                    "No item selected",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            var applicationUpdateInfo = updatesToInstall
                .SelectMany(u => u.Model)
                .Select(uti => new DetailedPackageVersion
                {
                    Identifier = uti.Identifier,
                    Name = uti.Name,
                    Version = uti.Version,
                    DownloadUrl = uti.DownloadUrl,
                    DownloadFilename = uti.DownloadFilename,
                    DownloadFileSize = uti.DownloadFileSize
                }).ToList();

            Task.Run(() => this.Parent.DownloadApplicationUpdates(applicationUpdateInfo));
            
            this.Close();
        }
    }
}
