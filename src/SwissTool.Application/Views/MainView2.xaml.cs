// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainView.xaml.cs" company="Winkvist.com">
//   Copyright (c) 2011 Winkvist.com. All rights reserved.
// </copyright>
// <summary>
//   Interaction logic for MainView.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SwissTool.XT.Views
{
    using System.Windows;

    using SwissTool.XT.Models;

    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainView"/> class.
        /// </summary>
        public MainView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Shows the balloon message.
        /// </summary>
        /// <param name="balloonMessage">The balloon message.</param>
        public void ShowBalloonMessage(BalloonMessage balloonMessage)
        {
            this.MainTrayIcon.ShowBalloonTip(balloonMessage.Title, balloonMessage.Message, balloonMessage.Icon);
        }
    }
}
