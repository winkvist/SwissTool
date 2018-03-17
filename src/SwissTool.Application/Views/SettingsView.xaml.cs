// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SettingsView.xaml.cs" company="Fredrik Winkvist">
//   Copyright (c) Fredrik Winkvist. All rights reserved.
// </copyright>
// <summary>
//   Interaction logic for SettingsView.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SwissTool.Application.Views
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    using SwissTool.Application.ViewModels;
    using SwissTool.Framework.UI.Controls;

    /// <summary>
    /// Interaction logic for SettingsView
    /// </summary>
    public partial class SettingsView : MetroDialogWindow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsView"/> class.
        /// </summary>
        public SettingsView()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Preview key down event of a combo box.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
        private void ComboBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            var keyString = string.Empty;
            
            switch (e.Key)
            {
                case Key.LWin:
                case Key.RWin:
                    keyString = "Windows";
                    break;

                case Key.LeftCtrl:
                case Key.RightCtrl:
                    keyString = "Control";
                    break;

                case Key.LeftAlt:
                case Key.RightAlt:
                    keyString = "Alt";
                    break;

                case Key.LeftShift:
                case Key.RightShift:
                    keyString = "Shift";
                    break;

                case Key.System:
                    if (e.SystemKey == Key.LeftAlt || e.SystemKey == Key.RightAlt)
                    {
                        keyString = "Alt";
                    }
                    break;
            }

            var comboBox = sender as ComboBox;
            if (comboBox != null)
            {
                comboBox.SelectedItem = keyString;
            }

            e.Handled = true;
        }

        /// <summary>
        /// Preview key down event of a combo box.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
        private void ComboBoxKeyPreviewKeyDown(object sender, KeyEventArgs e)
        {
            var keyString = Enum.GetName(typeof(Key), e.Key == Key.System ? e.SystemKey : e.Key);

            var comboBox = sender as ComboBox;
            if (comboBox != null)
            {
                comboBox.SelectedItem = keyString;
            }

            e.Handled = true;
        }

        /// <summary>
        /// Click event of a check box.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void CheckBoxClick(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as SettingsViewModel;

            viewModel?.TriggerUiRefresh();
        }
    }
}
