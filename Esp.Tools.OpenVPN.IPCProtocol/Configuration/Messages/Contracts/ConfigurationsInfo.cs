using System.Runtime.Serialization;
using Esp.Tools.OpenVPN.IPCProtocol.Contracts;

namespace Esp.Tools.OpenVPN.IPCProtocol.Configuration.Messages.Contracts
{
    [DataContract]
    public class ConfigurationsInfo
    {
        [DataMember]
        public ConfigurationInfo[] Configurations { get; set; }
    }
}