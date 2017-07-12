using Esp.Tools.OpenVPN.IPCProtocol.Configuration.Commands.Contracts;

namespace Esp.Tools.OpenVPN.IPCProtocol.Configuration.Commands
{
    public class InstallConfigurationCommand: BaseMessage<InstallConfigurationInfo>
    {
        public InstallConfigurationCommand() 
        {
            Code = MessageKey;
        }

        public const string MessageKey = "InstallConfig";
    }
}
