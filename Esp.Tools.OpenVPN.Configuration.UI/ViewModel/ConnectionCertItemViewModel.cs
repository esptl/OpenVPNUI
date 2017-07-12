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
using Esp.Tools.OpenVPN.SharedUI;

namespace Esp.Tools.OpenVPN.Configuration.UI.ViewModel
{
    public class ConnectionCertItemViewModel : ViewModelBase
    {
        private readonly ConnectionViewModel _connection;
        private CertificateDetails _certificate;

        public ConnectionCertItemViewModel(ConnectionViewModel pConnection, CertificateDetails pCertificate)
        {
            _connection = pConnection;
            _certificate = pCertificate;
            UseCommand = new BasicCommand(OnUseCert);
        }

        public BasicCommand UseCommand
        {
            get; private set; 
        }

        private void OnUseCert()
        {
            _connection.UseCert(_certificate);
            OnPropertyChanged("InUse");
        }

        public CertificateDetails Details { get { return _certificate; } 
            set { _certificate = value; } }

       
        public bool InUse
        {
            get { return _connection.ThumbPrint == _certificate.ThumbPrint; }
            
        }

        public override string ToString()
        {
            return _certificate.ToString();
        }

        public bool Equals(ConnectionCertItemViewModel other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._certificate, _certificate);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (ConnectionCertItemViewModel)) return false;
            return Equals((ConnectionCertItemViewModel) obj);
        }

        public override int GetHashCode()
        {
            return (_certificate != null ? _certificate.GetHashCode() : 0);
        }
    }
}