using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Esp.Tools.OpenVPN.IPCProtocol.Contracts;

namespace Esp.Tools.OpenVPN.IPCProtocol.Messages
{
    public class ShutDownMessage : BaseMessage<ShutDownInfo>
    {
        public ShutDownMessage()
        {
            Code = MessageKey;
        }
        public const string MessageKey = "Shutdown";
    }
}
