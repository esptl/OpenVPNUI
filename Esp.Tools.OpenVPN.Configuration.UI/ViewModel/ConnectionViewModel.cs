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
using System.Linq;
using Esp.Tools.OpenVPN.Certificates;
using Esp.Tools.OpenVPN.Client;
using Esp.Tools.OpenVPN.ConnectionFile;
using Esp.Tools.OpenVPN.IPCProtocol.Contracts;
using Esp.Tools.OpenVPN.SharedUI;

namespace Esp.Tools.OpenVPN.Configuration.UI.ViewModel
{
    public class SaveLevelViewModel : ViewModelBase
    {
        public SaveLevelViewModel(ConnectionSaveLevel pLevel, string pDescription)
        {
            Level = pLevel;
            Description = pDescription;
        }

        public ConnectionSaveLevel Level { get; }

        public string Description { get; }

        public bool Equals(SaveLevelViewModel other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Level, Level);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(SaveLevelViewModel)) return false;
            return Equals((SaveLevelViewModel) obj);
        }

        public override int GetHashCode()
        {
            return Level.GetHashCode();
        }

        public override string ToString()
        {
            return Description;
        }
    }

    public class ConnectionViewModel : ViewModelBase
    {
        private readonly ConfigurationPipeClient _configClient;
        private readonly ConfigurationInfo _configurationInfo;

        public ConnectionViewModel(ConfigurationPipeClient pConfigClient, ConfigurationInfo pConfigurationInfo)
        {
            _configClient = pConfigClient;
            _configurationInfo = pConfigurationInfo;
        }

        public IEnumerable<SaveLevelViewModel> AvailableSaveLevels => new[]
        {
            new SaveLevelViewModel(ConnectionSaveLevel.Session,
                "Save password for session."),
            new SaveLevelViewModel(ConnectionSaveLevel.Persistent,
                "Save password perminantly."),
            new SaveLevelViewModel(ConnectionSaveLevel.None, "Never save password."),
            new SaveLevelViewModel(ConnectionSaveLevel.Both,
                "Allow user to choose.")
        };

        public ConnectionCertItemViewModel[] AvailableCertificates
        {
            get
            {
                return _configClient.Certificates.Where(
                        pX => pX.ChainThumbPrints.Length > 1 && pX.ChainThumbPrints.ToList()
                                  .Contains(_configurationInfo.AuthorityThumbPrint))
                    .Select(pX => new ConnectionCertItemViewModel(this, pX))
                    .ToArray();
            }
        }

        public string ThumbPrint
        {
            get => _configurationInfo.ThumbPrint;
            set => _configurationInfo.ThumbPrint = value;
        }

        public ConnectionCertItemViewModel Certificate
        {
            get
            {
                return string.IsNullOrEmpty(ThumbPrint)
                    ? null
                    : AvailableCertificates.FirstOrDefault(pX => pX.Details.ThumbPrint == ThumbPrint);
            }

            set
            {
                if (value == null)
                    ThumbPrint = null;
                else
                    UseCert(value.Details);
            }
        }


        public SaveLevelViewModel KeyAuthSaveLevel
        {
            get
            {
                return
                    AvailableSaveLevels.First(pX => pX.Level == _configurationInfo.Configuration.KeyAuthSaveLevel);
            }
            set => _configurationInfo.Configuration.KeyAuthSaveLevel = value.Level;
        }

        public SaveLevelViewModel RemoteAuthSaveLevel
        {
            get
            {
                return AvailableSaveLevels.First(
                    pX => pX.Level == _configurationInfo.Configuration.RemoteAuthSaveLevel);
            }
            set => _configurationInfo.Configuration.RemoteAuthSaveLevel = value.Level;
        }

        public string SubjectName
        {
            get
            {
                var cert = Certificate;
                return cert != null ? cert.Details.CommonName : null;
            }
        }

        public string Issuer
        {
            get
            {
                var cert = Certificate;
                return cert != null ? cert.Details.IssuerName : null;
            }
        }

        public DateTime ValidFrom
        {
            get
            {
                var cert = Certificate;
                return cert != null ? cert.Details.ValidFrom : DateTime.Now;
            }
        }

        public DateTime ValidUntil
        {
            get
            {
                var cert = Certificate;
                return cert != null ? cert.Details.ValidTo : DateTime.Now;
            }
        }


        public bool HasCert => Certificate != null;

        public bool HasNoCert => !HasCert;

        public override string ToString()
        {
            return string.Format("{0}", _configurationInfo.Name);
        }

        internal void UseCert(CertificateDetails pCertificate)
        {
            _configurationInfo.ThumbPrint = pCertificate.ThumbPrint;
            OnPropertyChanged("Issuer");
            OnPropertyChanged("SubjectName");
            OnPropertyChanged("ThumbPrint");
            _configClient.SendSetConfigurationCertificateCommand(_configurationInfo.Name, pCertificate);
        }
    }
}