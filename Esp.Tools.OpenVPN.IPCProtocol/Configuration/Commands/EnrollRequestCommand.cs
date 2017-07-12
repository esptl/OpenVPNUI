using Esp.Tools.OpenVPN.IPCProtocol.Configuration.Commands.Contracts;
using Esp.Tools.OpenVPN.IPCProtocol.Controller.Commands.Contracts;

namespace Esp.Tools.OpenVPN.IPCProtocol.Configuration.Commands
{
    public class EnrollRequestCommand: BaseMessage<EnrollRequestInfo>
    {
        public EnrollRequestCommand()
        {
            Code = MessageKey;
        }

        public const string MessageKey = "EntrollRequest";
    }
}
