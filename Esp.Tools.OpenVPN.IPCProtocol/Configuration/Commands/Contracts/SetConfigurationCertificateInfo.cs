using System.Runtime.Serialization;

namespace Esp.Tools.OpenVPN.IPCProtocol.Configuration.Commands.Contracts
{
    [DataContract]
    public class SetConfigurationCertificateInfo
    {
        [DataMember]
        public string ThumbPrint { get; set; }

        [DataMember]
        public string Name { get; set; }
    }
}