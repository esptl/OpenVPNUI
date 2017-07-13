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
using Esp.Tools.OpenVPN.Certificates;
using Esp.Tools.OpenVPN.Client;
using Esp.Tools.OpenVPN.SharedUI;

namespace Esp.Tools.OpenVPN.Configuration.UI.ViewModel
{
    public class CertificateViewModel : ViewModelBase
    {
        private readonly CertificateDetails _certificate;
        private readonly ConfigurationPipeClient _configClient;
        private readonly IViewModelDialogs _dialogs;

        public CertificateViewModel(IViewModelDialogs pDialogs, ConfigurationPipeClient pConfigClient,
            CertificateDetails pCertificate)
        {
            _dialogs = pDialogs;
            _configClient = pConfigClient;
            _certificate = pCertificate;
            DeleteCommand = new BasicCommand(OnDelete);
        }

        public BasicCommand DeleteCommand { get; }


        public string ThumbPrint => _certificate.ThumbPrint;

        public DateTime ValidFrom => _certificate.ValidFrom;

        public DateTime ValidUntil => _certificate.ValidTo;

        public string SubjectName => _certificate.CommonName;

        public string Issuer => _certificate.IssuerName;

        private void OnDelete()
        {
            _configClient.SendDeleteCertificateCommand(_certificate.ThumbPrint);
        }

        public override string ToString()
        {
            return string.Format("{0} ({1})", SubjectName, Issuer);
        }


        public bool Equals(CertificateViewModel pOther)
        {
            if (ReferenceEquals(null, pOther)) return false;
            if (ReferenceEquals(this, pOther)) return true;
            return Equals(pOther._certificate, _certificate);
        }

        public override bool Equals(object pObj)
        {
            if (ReferenceEquals(null, pObj)) return false;
            if (ReferenceEquals(this, pObj)) return true;
            if (pObj.GetType() != typeof(CertificateViewModel)) return false;
            return Equals((CertificateViewModel) pObj);
        }

        public override int GetHashCode()
        {
            return _certificate != null ? _certificate.GetHashCode() : 0;
        }
    }
}