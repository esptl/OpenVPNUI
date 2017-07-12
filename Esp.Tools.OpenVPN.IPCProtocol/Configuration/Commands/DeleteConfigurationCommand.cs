using Esp.Tools.OpenVPN.IPCProtocol.Configuration.Commands.Contracts;

namespace Esp.Tools.OpenVPN.IPCProtocol.Configuration.Commands
{
    public class DeleteConfigurationCommand: BaseMessage<DeleteConfigurationInfo>
    {
        public DeleteConfigurationCommand() 
        {
            Code = MessageKey;
        }

        public const string MessageKey = "DeleteConfig";
    }
}
