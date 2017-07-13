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

using Esp.Tools.OpenVPN.Certificates;
using Esp.Tools.OpenVPN.Client;
using Esp.Tools.OpenVPN.ConnectionFile;

namespace Esp.Tools.OpenVPN.Configuration.UI.ViewModel
{
    public interface IViewModelDialogs
    {
        EnrollRequestDetails CreateEnrollRequestCertificate();
        string GetEntrollmentFile();
        bool ConfirmReplaceConnection(string pName);
        ConnectionDefinitionFile GetConnectionFile();
        string GetImportCertificateFile();
        void ShowError(string pError);
        void ShowAbout();
        void RestartService();
        void ShowReconnecting(ConfigurationPipeClient pConfigClient);
    }
}