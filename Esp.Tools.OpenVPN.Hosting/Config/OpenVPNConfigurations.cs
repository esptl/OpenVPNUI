//  This file is part of OpenVPN UI.
//  Copyright 2011 ESP Technologies Ltd.
//
//  Author: James Martin - August 2011
//
//
//  Foobar is free software: you can redistribute it and/or modify
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Esp.Tools.OpenVPN.Certificates;
using Esp.Tools.OpenVPN.Configuration;
using Esp.Tools.OpenVPN.ConnectionFile;
using Esp.Tools.OpenVPN.IPCProtocol.Contracts;
using Esp.Tools.OpenVPN.IPCProtocol.Controller.Messages.Contracts;

namespace Esp.Tools.OpenVPN.Hosting.Config
{
    public class OpenVPNConfigurations : IEnumerable<OpenVPNConfiguration>
    {
        private readonly List<OpenVPNConfiguration> _configs = new List<OpenVPNConfiguration>(GetConfigs());

        public OpenVPNConfigurations()
        {
            foreach (var co in _configs)
            {
                var con = co;
                con.OutputRecieved += pLine => OnOutputRecieved(con, pLine);
                con.StatusChanged += pStatus => OnStatusChanged(con, pStatus);
                con.InterfaceChanged += pInterface => OnInterfaceChanged(con, pInterface);
                con.AuthInfoRequired += () => OnAuthInfoRequired(con);
            }
            while (TapDeviceManager.GetTapDevices().ToArray().Length < _configs.Count)
                TapDeviceManager.SetupTapDevice();
            foreach (var config in _configs)
                config.Deleted += OnDeleted;
        }

        public OpenVPNConfiguration this[int pI] => _configs[pI];

        private void OnAuthInfoRequired(OpenVPNConfiguration pCon)
        {
            if (AuthInfoRequired != null)
                AuthInfoRequired(pCon);
        }

        private void OnInterfaceChanged(OpenVPNConfiguration pCon, string pInterface)
        {
            if (InterfaceChanged != null)
                InterfaceChanged(pCon, pInterface);
        }

        private static IEnumerable<OpenVPNConfiguration> GetConfigs()
        {
            var i = 0;
            return new ConnectionDefinitionFiles().Select(pCon => new OpenVPNConfiguration(i++, pCon));
        }

        private void OnStatusChanged(OpenVPNConfiguration pCon, ConnectionStatus pStatus)
        {
            if (StatusChanged != null)
                StatusChanged(pCon, pStatus);
        }

        private void OnOutputRecieved(OpenVPNConfiguration pCon, OutputLine pLine)
        {
            if (OutputRecieved != null)
                OutputRecieved(pCon, pLine);
        }

        public event Action<OpenVPNConfiguration, OutputLine> OutputRecieved;
        public event Action<OpenVPNConfiguration, ConnectionStatus> StatusChanged;
        public event Action<OpenVPNConfiguration, string> InterfaceChanged;
        public event Action<OpenVPNConfiguration> AuthInfoRequired;
        public event Action<OpenVPNConfiguration> NewConfiguration;
        public event Action<OpenVPNConfiguration> DeletedConfiguration;

        public void Shutdown()
        {
            try
            {
                var configs = _configs.ToArray();
                foreach (var config in configs)
                    if (config.Status != ConnectionStatus.Disconnected)
                        config.Disconnect(false);

                var waiting = true;
                while (waiting)
                {
                    waiting = false;
                    foreach (var config in configs)
                        if (config.Status != ConnectionStatus.Disconnected)
                            waiting = true;
                    Thread.Sleep(250);
                }
            }
            catch (Exception)
            {
            }
        }

        public void InstallConfiguration(ConnectionDefinitionFile pConfigFile)
        {
            var fileName = Configuration.Configuration.Current.ConnecitonDataPath + "\\" + pConfigFile.ConnectionName +
                           ".openvpn";

            if (pConfigFile.AuthorityCert != null)
                CertificateManager.Current.InstallCertificate(pConfigFile.AuthorityCert);
            pConfigFile.SaveFile(fileName);
            var configuration = new OpenVPNConfiguration(_configs.Count, pConfigFile);
            configuration.Deleted += OnDeleted;
            _configs.Add(configuration);

            if (TapDeviceManager.GetTapDevices().ToArray().Length < _configs.Count)
                TapDeviceManager.SetupTapDevice();

            if (NewConfiguration != null)
                NewConfiguration(configuration);
        }

        private void OnDeleted(OpenVPNConfiguration pConfiguration)
        {
            _configs.Remove(pConfiguration);
            if (DeletedConfiguration != null)
                DeletedConfiguration(pConfiguration);
        }

        #region IEnumerable<OpenVPNConfiguration> Members

        public IEnumerator<OpenVPNConfiguration> GetEnumerator()
        {
            return _configs.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}