//  This file is part of OpenVPN UI.
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

using System;
using System.IO;
using System.Windows;
using Esp.Tools.OpenVPN.Certificates;
using Esp.Tools.OpenVPN.Client;
using Esp.Tools.OpenVPN.Configuration.UI.ViewModel;
using Esp.Tools.OpenVPN.ConnectionFile;
using Microsoft.Win32;

namespace Esp.Tools.OpenVPN.Configuration.UI
{
    public class ViewModelDialogs : IViewModelDialogs
    {
        private readonly Window _parentWindow;

        public ViewModelDialogs(Window pParentWindow)
        {
            _parentWindow = pParentWindow;
        }

        public string GetEntrollmentFile()
        {
            var openDialog = new OpenFileDialog();
            openDialog.Filter = "Certificate|*.crt;*.cer";
            var result = openDialog.ShowDialog();
            if(result.Value)
            {
                return File.ReadAllText(openDialog.FileName);
            }
            return null;

        }

     

        public bool ConfirmReplaceConnection(string pName)
        {
            return
               MessageBox.Show("Are you sure you wish to replace this connection", pName, MessageBoxButton.YesNo, MessageBoxImage.Question) ==
               MessageBoxResult.Yes;
        }

        public ConnectionDefinitionFile GetConnectionFile()
        {
            var openDialog = new OpenFileDialog();
            openDialog.Filter = "OpenVPN UI Configuration File|*.openvpn";
            var result = openDialog.ShowDialog();
            if (result.Value)
            {
                return ConnectionDefinitionFile.LoadFile(openDialog.FileName);
            }
            return null;
        }

        public string GetImportCertificateFile()
        {
            var openDialog = new OpenFileDialog();
            openDialog.Filter = "Certificate|*.crt;*.cer|PFX file|*.pfx";
            var result = openDialog.ShowDialog();
            if (result.Value)
            {
                return openDialog.FileName;
            }
            return null;
        }

        public void ShowError(string pError)
        {
            MessageBox.Show(pError);
        }

        public void ShowAbout()
        {
            var dialog = new AboutDialog();
            dialog.Owner = _parentWindow;
            dialog.ShowDialog();
        }

        private bool _restarting;
        private bool _reconnecting;

        public void RestartService()
        {
            if (!_restarting && !_reconnecting)
            {
                _restarting = true;
                var dialog = new RestartServiceView();
                dialog.Owner = _parentWindow;
                dialog.ShowDialog();
                _restarting = false;
            }
        }

        public void ShowReconnecting(ConfigurationPipeClient pConfigClient)
        {
            if (!_restarting && !_reconnecting)
            {
                _reconnecting = true;
                var dialog = new ReconnectingDialog();
                pConfigClient.Connected+=()=> Application.Current.Dispatcher.BeginInvoke(new Action(dialog.Close));
                dialog.Owner = _parentWindow;
                dialog.ShowDialog();
                _reconnecting = false;
            } 
            
        }

        EnrollRequestDetails IViewModelDialogs.CreateEnrollRequestCertificate()
        {
            var dialog = new CreateEnrollRequestView(this);
            dialog.Owner = _parentWindow;

            return dialog.GetEnrollmentDetails();
        }
    }
}