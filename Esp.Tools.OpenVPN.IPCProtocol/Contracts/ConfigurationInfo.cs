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

using System.Runtime.Serialization;

namespace Esp.Tools.OpenVPN.IPCProtocol.Contracts
{
    [DataContract]
    public class ConfigurationInfo
    {
        private ConnectionConfigurationInfo _configuration;

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public ConnectionStatus ConnectionStatus { get; set; }

        [DataMember]
        public ConnectionConfigurationInfo Configuration
        {
            get
            {
                if (_configuration == null)
                    _configuration = new ConnectionConfigurationInfo();
                return _configuration;
            }
            set => _configuration = value;
        }

        [DataMember]
        public string Interface { get; set; }

        [DataMember]
        public string GenericErrorMessage { get; set; }

        [DataMember]
        public ConnectionError Error { get; set; }

        [DataMember]
        public bool RequiresUsername { get; set; }

        [DataMember]
        public string ThumbPrint { get; set; }

        [DataMember]
        public string AuthorityThumbPrint { get; set; }
    }
}