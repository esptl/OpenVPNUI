using System.Runtime.Serialization;
using Esp.Tools.OpenVPN.Certificates;

namespace Esp.Tools.OpenVPN.IPCProtocol.Configuration.Commands.Contracts
{
    [DataContract]
    public class EnrollRequestInfo
    {
        [DataMember]
        public EnrollRequestDetails Request { get; set; }
    }
}