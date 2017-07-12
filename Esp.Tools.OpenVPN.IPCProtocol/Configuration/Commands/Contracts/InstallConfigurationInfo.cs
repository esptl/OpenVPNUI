using System.Runtime.Serialization;

namespace Esp.Tools.OpenVPN.IPCProtocol.Configuration.Commands.Contracts
{
    [DataContract]
    public class InstallConfigurationInfo
    {
        [DataMember]
        public byte[] ConfigurationData { get; set; }

    }
}
