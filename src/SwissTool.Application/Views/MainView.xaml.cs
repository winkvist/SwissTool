// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainView.xaml.cs" company="Fredrik Winkvist">
//   Copyright (c) Fredrik Winkvist. All rights reserved.
// </copyright>
// <summary>
//   Interaction logic for MainView.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SwissTool.Application.Views
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    using MahApps.Metro.Controls;

    using Models;

    /// <summary>
    /// Interaction logic for MainView
    /// </summary>
    public partial class MainView : MetroWindow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainView"/> class.
        /// </summary>
        public MainView()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Shows the balloon message.
        /// </summary>
        /// <param name="balloonMessage">The balloon message.</param>
        public void ShowBalloonMessage(BalloonMessage balloonMessage)
        {
            this.MainTrayIcon.ShowBalloonTip(balloonMessage.Title, balloonMessage.Message, balloonMessage.Icon);
        }

        /// <summary>
        /// Handles the OnLoaded event of the ImageIcon control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ImageIcon_OnLoaded(object sender, RoutedEventArgs e)
        {
            // Workaround to stretch icon in Windows 8. 
            // Details: https://connect.microsoft.com/VisualStudio/feedback/details/767328/menuitem-icon-wont-stretch-when-changing-size-in-windows-8
            
            var image = sender as Image;
            if (image != null)
            {
                var parent = VisualTreeHelper.GetParent(image) as ContentPresenter;

                if (parent != null)
                {
                    parent.Width = image.Width;
                    parent.Height = image.Height;
                }
            }
        }

        /// <summary>
        /// Handles the OnTrayBalloonTipClicked event of the MainTrayIcon control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void MainTrayIcon_OnTrayBalloonTipClicked(object sender, RoutedEventArgs e)
        {
            
        }

        /// <summary>
        /// Handles the OnTrayBalloonTipShown event of the MainTrayIcon control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void MainTrayIcon_OnTrayBalloonTipShown(object sender, RoutedEventArgs e)
        {
        }

        /// <summary>
        /// Handles the OnTrayBalloonTipClosed event of the MainTrayIcon control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void MainTrayIcon_OnTrayBalloonTipClosed(object sender, RoutedEventArgs e)
        {
        }
    }
}
