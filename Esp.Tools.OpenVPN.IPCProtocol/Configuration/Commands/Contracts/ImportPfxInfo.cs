using System.Runtime.Serialization;

namespace Esp.Tools.OpenVPN.IPCProtocol.Configuration.Commands.Contracts
{
    [DataContract]
    public class ImportPfxInfo
    {
        [DataMember]
        public byte[] PfxData { get; set; }

        [DataMember]
        public string Password { get; set; }
    }
}