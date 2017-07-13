using Esp.Tools.OpenVPN.IPCProtocol.Configuration.Commands.Contracts;

namespace Esp.Tools.OpenVPN.IPCProtocol.Configuration.Commands
{
    public class EnrollRequestCommand : BaseMessage<EnrollRequestInfo>
    {
        public const string MessageKey = "EntrollRequest";

        public EnrollRequestCommand()
        {
            Code = MessageKey;
        }
    }
}