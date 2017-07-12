using System.Runtime.Serialization;

namespace Esp.Tools.OpenVPN.IPCProtocol.Configuration.Commands.Contracts
{
    [DataContract]
    public class EnrollInfo
    {
        [DataMember]
        public string Certificate { get; set; }
    }
}