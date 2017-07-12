using Esp.Tools.OpenVPN.IPCProtocol.Configuration.Commands.Contracts;
using Esp.Tools.OpenVPN.IPCProtocol.Controller.Commands.Contracts;

namespace Esp.Tools.OpenVPN.IPCProtocol.Configuration.Commands
{
    public class EnrollCommand: BaseMessage<EnrollInfo>
    {
        public EnrollCommand()
        {
            Code = MessageKey;
        }

        public const string MessageKey = "Enroll";
    }
}
