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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using Esp.Tools.OpenVPN.Certificates;
using Esp.Tools.OpenVPN.Client;
using Esp.Tools.OpenVPN.IPCProtocol.Configuration.Messages.Contracts;
using Esp.Tools.OpenVPN.SharedUI;

namespace Esp.Tools.OpenVPN.Configuration.UI.ViewModel
{
    public class CertificatesViewModel : ViewModelBase, IDisposable
    {
        private readonly ConfigurationPipeClient _configClient;
        private readonly IViewModelDialogs _dialogs;
        private IEnumerable<CertificateViewModel> _certificates;

        private CertificateViewModel _selectedItem;


        public CertificatesViewModel(IViewModelDialogs pDialogs, ConfigurationPipeClient pConfigClient)
        {
            _dialogs = pDialogs;
            _configClient = pConfigClient;
            _configClient.EnrollRequestResponse += OnEnrollRequestResponse;
            _configClient.Enrolled += OnEnrolled;
            _configClient.CertificatesChanged += OnCertificatesChanged;
            if (_configClient.Certificates != null && _configClient.Certificates.Length > 0)
                UpdateCertificates();
            CreateEnrollCommand =
                new BasicCommand(
                    () =>
                    {
                        var certDetails = pDialogs.CreateEnrollRequestCertificate();
                        if (certDetails == null) return;

                        _configClient.SendRequestEnrollCommand(certDetails);
                    });
            ImportEnrollCommand =
                new BasicCommand(
                    () =>
                    {
                        var file = pDialogs.GetEntrollmentFile();
                        if (file == null) return;
                        _configClient.SendEnrollCommand(file);
                    });

            ImportCommand =
                new BasicCommand(
                    () =>
                    {
                        var file = pDialogs.GetImportCertificateFile();
                        if (file == null || !File.Exists(file)) return;

                        var ext = Path.GetExtension(file);
                        byte[] pfxData = null;
                        var password = string.Empty;
                        switch (ext.ToLower())
                        {
                            case ".cer":
                            case ".crt":
                                var keyFileName = Path.GetDirectoryName(file) + "\\" +
                                                  Path.GetFileNameWithoutExtension(file) + ".key";
                                if (!File.Exists(keyFileName))
                                {
                                    _dialogs.ShowError("Missing corrosponding key file");
                                    return;
                                }

                                pfxData = CertificateManager.Current.CreatePfx(file, keyFileName);
                                password = "password";
                                break;

                            default:
                                pfxData = File.ReadAllBytes(file);
                                break;
                        }
                        _configClient.SendImportPfxCommand(pfxData, password);
                    });
        }

        public BasicCommand ImportCommand { get; }

        public BasicCommand ImportEnrollCommand { get; }

        public BasicCommand CreateEnrollCommand { get; }

        public CertificateViewModel SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged("SelectedItem");
            }
        }

        public IEnumerable<CertificateViewModel> Certificates
        {
            get => _certificates;
            set
            {
                _certificates = value;
                OnPropertyChanged("Certificates");
            }
        }

        public void Dispose()
        {
        }

        private async Task OnCertificatesChanged()
        {
            UpdateCertificates();
        }

        private void UpdateCertificates()
        {
            Certificates =
                _configClient.Certificates.Select(pX => new CertificateViewModel(_dialogs, _configClient, pX));
            if (Certificates != null && SelectedItem == null)
                SelectedItem = Certificates.FirstOrDefault();
        }

        private async Task OnEnrolled(EnrollResponseInfo pObj)
        {
            Application.Current.Dispatcher.BeginInvoke(
                new Action(() => MessageBox.Show("Enrolled: " + pObj.ThumbPrint)));
        }

        private async Task OnEnrollRequestResponse(EnrollRequestResponseInfo pRequest)
        {
            Application.Current.Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    Clipboard.SetText(pRequest.Request.Replace("\n", "\r\n"));
                    MessageBox.Show("Copied CSR to clipboard");
                }));
        }
    }
}