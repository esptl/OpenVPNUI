using System.Runtime.Serialization;

namespace Esp.Tools.OpenVPN.IPCProtocol.Configuration.Messages.Contracts
{
    [DataContract]
    public class EnrollResponseInfo
    {
        [DataMember]
        public string ThumbPrint { get; set; }
    }
}
