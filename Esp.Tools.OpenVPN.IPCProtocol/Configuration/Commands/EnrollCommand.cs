using Esp.Tools.OpenVPN.IPCProtocol.Configuration.Commands.Contracts;

namespace Esp.Tools.OpenVPN.IPCProtocol.Configuration.Commands
{
    public class EnrollCommand : BaseMessage<EnrollInfo>
    {
        public const string MessageKey = "Enroll";

        public EnrollCommand()
        {
            Code = MessageKey;
        }
    }
}