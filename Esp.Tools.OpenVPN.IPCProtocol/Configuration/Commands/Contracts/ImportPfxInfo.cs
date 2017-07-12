using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

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
