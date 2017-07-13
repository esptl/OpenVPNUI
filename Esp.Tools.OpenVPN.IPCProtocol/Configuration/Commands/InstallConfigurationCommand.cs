using Esp.Tools.OpenVPN.IPCProtocol.Configuration.Commands.Contracts;

namespace Esp.Tools.OpenVPN.IPCProtocol.Configuration.Commands
{
    public class InstallConfigurationCommand : BaseMessage<InstallConfigurationInfo>
    {
        public const string MessageKey = "InstallConfig";

        public InstallConfigurationCommand()
        {
            Code = MessageKey;
        }
    }
}