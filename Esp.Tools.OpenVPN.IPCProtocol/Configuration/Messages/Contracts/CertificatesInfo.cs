using System.Runtime.Serialization;
using Esp.Tools.OpenVPN.Certificates;

namespace Esp.Tools.OpenVPN.IPCProtocol.Configuration.Messages.Contracts
{
    [DataContract]
    public class CertificatesInfo
    {
        [DataMember]
        public CertificateDetails[] Certificates { get; set; }
    }
}
