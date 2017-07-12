using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Esp.Tools.OpenVPN.IPCProtocol.Configuration.Messages.Contracts;

namespace Esp.Tools.OpenVPN.IPCProtocol.Configuration.Messages
{
    public class ConfigurationsMessage: BaseMessage<ConfigurationsInfo>
    {
        public ConfigurationsMessage()
        {
    
            Code = MessageKey;
        }

        public const string MessageKey = "Configurations";
    }
}
