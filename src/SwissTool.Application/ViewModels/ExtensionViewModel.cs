// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExtensionViewModel.cs" company="Fredrik Winkvist">
//   Copyright (c) Fredrik Winkvist. All rights reserved.
// </copyright>
// <summary>
//   Defines the ExtensionViewModel type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SwissTool.Application.ViewModels
{
    using System.Collections.Generic;

    using SwissTool.Framework.Definitions;
    using SwissTool.Framework.UI.Infrastructure;

    /// <summary>
    /// The extension view model.
    /// </summary>
    public class ExtensionViewModel : ViewModelBase
    {
        /// <summary>
        /// The selected action.
        /// </summary>
        private ExtensionActionViewModel selectedAction;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtensionViewModel"/> class.
        /// </summary>
        /// <param name="settingsViewModel">The settings view model.</param>
        public ExtensionViewModel(SettingsViewModel settingsViewModel)
        {
            this.Actions = new List<ExtensionActionViewModel>();
            this.SelectedAction = null;
            this.SettingsViewModel = settingsViewModel;
        }

        /// <summary>
        /// Gets or sets the settings view model.
        /// </summary>
        /// <value>The settings view model.</value>
        public SettingsViewModel SettingsViewModel { get; set; }

        /// <summary>
        /// Gets or sets the extension.
        /// </summary>
        /// <value>
        /// The extension.
        /// </value>
        public IExtension Extension { get; set; }

        /// <summary>
        /// Gets or sets the actions.
        /// </summary>
        /// <value>
        /// The actions.
        /// </value>
        public List<ExtensionActionViewModel> Actions { get; set; }

        /// <summary>
        /// Gets or sets the selected action.
        /// </summary>
        /// <value>
        /// The selected action.
        /// </value>
        public ExtensionActionViewModel SelectedAction
        {
            get
            {
                return this.selectedAction;
            }

            set
            { 
                this.selectedAction = value;

                this.NotifyPropertyChanged(nameof(this.SelectedAction));
                this.NotifyPropertyChanged(nameof(this.HasSelectedAction));
            }
        }

        /// <summary>
        /// Gets a value indicating whether HasSelectedAction.
        /// </summary>
        public bool HasSelectedAction
        {
            get
            {
                return this.SelectedAction != null;
            }
        }
    }
}
