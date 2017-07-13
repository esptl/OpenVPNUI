using System.Runtime.Serialization;
using Esp.Tools.OpenVPN.ConnectionFile;

namespace Esp.Tools.OpenVPN.IPCProtocol.Contracts
{
    [DataContract]
    public class ConnectionConfigurationInfo
    {
        [DataMember]
        public bool AutoStart { get; set; }


        [DataMember]
        public ConnectionSaveLevel KeyAuthSaveLevel { get; set; }

        [DataMember]
        public ConnectionSaveLevel RemoteAuthSaveLevel { get; set; }
    }
}