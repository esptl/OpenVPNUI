using Esp.Tools.OpenVPN.IPCProtocol.Configuration.Messages.Contracts;

namespace Esp.Tools.OpenVPN.IPCProtocol.Configuration.Messages
{
    public class ConfigurationsMessage : BaseMessage<ConfigurationsInfo>
    {
        public const string MessageKey = "Configurations";

        public ConfigurationsMessage()
        {
            Code = MessageKey;
        }
    }
}