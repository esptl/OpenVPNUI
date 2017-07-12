﻿//  This file is part of OpenVPN UI.
//  Copyright 2011 ESP Technologies Ltd.
//
//  Author: James Martin - October 2011
//
//
//  OpenVPN UI is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  OpenVPN UI is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with OpenVPN UI.  If not, see <http://www.gnu.org/licenses/>.
using System.Windows.Controls;

namespace Esp.Tools.OpenVPN.Configuration.UI
{
    /// <summary>
    /// Interaction logic for ConnectionsView.xaml
    /// </summary>
    public partial class ConnectionsView
    {
        public ConnectionsView()
        {
            InitializeComponent();
        }

        private void _actionsButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            popup1.IsOpen = true;
        }
    }
}
