using System.Runtime.Serialization;

namespace Esp.Tools.OpenVPN.IPCProtocol.Configuration.Commands.Contracts
{
    [DataContract]
    public class DeleteCertificateInfo
    {
        [DataMember]
        public string ThumbPrint { get; set; }
    }
}
