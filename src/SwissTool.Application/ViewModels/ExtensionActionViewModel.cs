// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExtensionActionViewModel.cs" company="Fredrik Winkvist">
//   Copyright (c) Fredrik Winkvist. All rights reserved.
// </copyright>
// <summary>
//   Defines the ExtensionActionViewModel type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SwissTool.Application.ViewModels
{
    using System;
    using System.Linq;

    using SwissTool.Framework.Definitions;
    using SwissTool.Framework.Enums;
    using SwissTool.Framework.UI.Infrastructure;

    /// <summary>
    /// The extension action view model.
    /// </summary>
    public class ExtensionActionViewModel : ViewModelBase
    {
        /// <summary>
        /// The assigned hotkey.
        /// </summary>
        private Models.HotKey assignedHotKey;

        /// <summary>
        /// The selected first modifier key.
        /// </summary>
        private string selectedFirstModifierKey;

        /// <summary>
        /// The selected second modifier.
        /// </summary>
        private string selectedSecondModifierKey;

        /// <summary>
        /// The selected key
        /// </summary>
        private string selectedKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtensionActionViewModel"/> class.
        /// </summary>
        /// <param name="extensionViewModel">The extension view model.</param>
        public ExtensionActionViewModel(ExtensionViewModel extensionViewModel)
        {
            this.selectedFirstModifierKey = this.selectedSecondModifierKey = Enum.GetName(typeof(HotKeyModifier), HotKeyModifier.None);
            this.selectedKey = Enum.GetName(typeof(HotKey), HotKey.None);
            this.ExtensionViewModel = extensionViewModel;
        }

        /// <summary>
        /// Gets or sets the extension view model.
        /// </summary>
        /// <value>The extension view model.</value>
        public ExtensionViewModel ExtensionViewModel { get; set; }

        /// <summary>
        /// Gets or sets the action.
        /// </summary>
        /// <value>
        /// The action.
        /// </value>
        public IExtensionHotKeyAction Action { get; set; }

        /// <summary>
        /// Gets or sets the assigned hot key.
        /// </summary>
        /// <value>
        /// The assigned hot key.
        /// </value>
        public Models.HotKey AssignedHotKey
        {
            get
            {
                return this.assignedHotKey;
            }

            set
            {
                this.assignedHotKey = value;
                this.SetSelectedValues();
                this.NotifyPropertyChanged(nameof(this.AssignedHotKey));
            }
        }

        /// <summary>
        /// Gets or sets the extension.
        /// </summary>
        /// <value>
        /// The extension.
        /// </value>
        public IExtension Extension { get; set; }

        /// <summary>
        /// Gets the assigned hot key display string.
        /// </summary>
        public string AssignedHotKeyDisplayString
        {
            get
            {
                if (this.AssignedHotKey == null)
                {
                    return "Not assigned";
                }

                var firstModifier = this.AssignedHotKey.ModifierKey;
                var secondModifier = this.AssignedHotKey.SecondModifierKey;
                var key = this.AssignedHotKey.Key;

                var displayString = string.Empty;

                switch (firstModifier)
                {
                    case HotKeyModifier.Control:
                        displayString += "CTRL + ";
                        break;
                    case HotKeyModifier.Alt:
                        displayString += "ALT + ";
                        break;
                    case HotKeyModifier.Shift:
                        displayString += "SHIFT + ";
                        break;
                    case HotKeyModifier.Windows:
                        displayString += "WIN + ";
                        break;
                }

                switch (secondModifier)
                {
                    case HotKeyModifier.Control:
                        displayString += "CTRL + ";
                        break;
                    case HotKeyModifier.Alt:
                        displayString += "ALT + ";
                        break;
                    case HotKeyModifier.Shift:
                        displayString += "SHIFT + ";
                        break;
                    case HotKeyModifier.Windows:
                        displayString += "WIN + ";
                        break;
                }

                displayString += Enum.GetName(typeof(HotKey), key);

                return displayString;
            }
        }

        /// <summary>
        /// Gets or sets the selected second modifier key.
        /// </summary>
        /// <value>
        /// The selected second modifier key.
        /// </value>
        public string SelectedSecondModifierKey
        {
            get
            {
                return this.selectedSecondModifierKey;
            }

            set
            {
                if (this.selectedSecondModifierKey != value)
                {
                    this.selectedSecondModifierKey = value;

                    this.AssignHotKey();

                    this.NotifyPropertyChanged(nameof(this.SelectedSecondModifierKey));
                    this.NotifyPropertyChanged(nameof(this.FirstModifierKeys));
                    this.NotifyPropertyChanged(nameof(this.AssignedHotKeyDisplayString));
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected first modifier key.
        /// </summary>
        /// <value>
        /// The selected first modifier key.
        /// </value>
        public string SelectedFirstModifierKey
        {
            get
            {
                return this.selectedFirstModifierKey;
            }
            set
            {
                if (this.selectedFirstModifierKey != value)
                {
                    this.selectedFirstModifierKey = value;

                    if (value == "None")
                    {
                        this.selectedSecondModifierKey = "None";
                        this.NotifyPropertyChanged(nameof(this.SelectedSecondModifierKey));
                        this.NotifyPropertyChanged(nameof(this.FirstModifierKeys));
                    }

                    this.AssignHotKey();

                    this.NotifyPropertyChanged(nameof(this.HasAssignedFirstModifierKey));
                    this.NotifyPropertyChanged(nameof(this.SelectedFirstModifierKey));
                    this.NotifyPropertyChanged(nameof(this.SecondModifierKeys));
                    this.NotifyPropertyChanged(nameof(this.AssignedHotKeyDisplayString));
                }
            }
        }


        /// <summary>
        /// Gets or sets the selected key.
        /// </summary>
        /// <value>
        /// The selected key.
        /// </value>
        public string SelectedKey
        {
            get
            {
                return this.selectedKey;
            }

            set
            {
                if (this.selectedKey != value)
                {
                    this.selectedKey = value;

                    this.AssignHotKey();

                    this.NotifyPropertyChanged(nameof(this.SelectedKey));
                    this.NotifyPropertyChanged(nameof(this.AssignedHotKeyDisplayString));
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether collision is detected.
        /// </summary>
        /// <value><c>true</c> if collision is detected; otherwise, <c>false</c>.</value>
        public bool CollisionDetected
        {
            get
            {
                if (this.AssignedHotKey != null)
                {
                    var definition = this.assignedHotKey.GetDefinition();
                    var settingsHotKeys = this.ExtensionViewModel.SettingsViewModel.SettingsHotKeys;

                    if (settingsHotKeys.Contains(definition) && settingsHotKeys[definition].Count() > 1)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Gets the first modifier keys.
        /// </summary>
        public string[] FirstModifierKeys
        {
            get
            {
                var values = Enum.GetNames(typeof(HotKeyModifier));
                return values.Where(v => v == "None" || v != this.SelectedSecondModifierKey).ToArray();
            }
        }

        /// <summary>
        /// Gets the second modifier keys.
        /// </summary>
        public string[] SecondModifierKeys
        {
            get
            {
                var values = Enum.GetNames(typeof(HotKeyModifier));
                return values.Where(v => v == "None" || v != this.SelectedFirstModifierKey).ToArray();
            }
        }

        /// <summary>
        /// Gets the keys.
        /// </summary>
        public string[] Keys
        {
            get
            {
                return Enum.GetNames(typeof(HotKey));
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has assigned first modifier key.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has assigned first modifier key; otherwise, <c>false</c>.
        /// </value>
        public bool HasAssignedFirstModifierKey
        {
            get
            {
                return !string.IsNullOrEmpty(this.SelectedFirstModifierKey) && this.SelectedFirstModifierKey != "None";
            }
        }

        /// <summary>
        /// Assigns the hot key.
        /// </summary>
        private void AssignHotKey()
        {
            this.AssignedHotKey = (string.IsNullOrEmpty(this.SelectedKey) || this.SelectedKey == "None")
                                 ? null
                                 : new Models.HotKey
                                       {
                                           Action = this.Action,
                                           Extension = this.Extension,
                                           Key = (HotKey)Enum.Parse(typeof(HotKey), this.SelectedKey),
                                           ModifierKey = (HotKeyModifier)Enum.Parse(typeof(HotKeyModifier), this.SelectedFirstModifierKey),
                                           SecondModifierKey = (HotKeyModifier)Enum.Parse(typeof(HotKeyModifier), this.SelectedSecondModifierKey)
                                       };

            this.NotifyPropertyChanged(nameof(this.CollisionDetected));
        }

        /// <summary>
        /// Sets the selected values.
        /// </summary>
        private void SetSelectedValues()
        {
            if (this.AssignedHotKey != null)
            {
                this.selectedFirstModifierKey = Enum.GetName(typeof(HotKeyModifier), this.AssignedHotKey.ModifierKey);
                this.selectedSecondModifierKey = Enum.GetName(typeof(HotKeyModifier), this.AssignedHotKey.SecondModifierKey);
                this.selectedKey = Enum.GetName(typeof(HotKey), this.AssignedHotKey.Key);

                this.NotifyPropertyChanged(nameof(this.SelectedKey));
                this.NotifyPropertyChanged(nameof(this.SelectedFirstModifierKey));
                this.NotifyPropertyChanged(nameof(this.SelectedSecondModifierKey));
            }
        }
    }
}
