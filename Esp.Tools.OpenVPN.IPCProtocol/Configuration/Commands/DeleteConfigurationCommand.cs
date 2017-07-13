using Esp.Tools.OpenVPN.IPCProtocol.Configuration.Commands.Contracts;

namespace Esp.Tools.OpenVPN.IPCProtocol.Configuration.Commands
{
    public class DeleteConfigurationCommand : BaseMessage<DeleteConfigurationInfo>
    {
        public const string MessageKey = "DeleteConfig";

        public DeleteConfigurationCommand()
        {
            Code = MessageKey;
        }
    }
}