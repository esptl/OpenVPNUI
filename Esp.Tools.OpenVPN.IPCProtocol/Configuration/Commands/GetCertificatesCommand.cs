using Esp.Tools.OpenVPN.IPCProtocol.Configuration.Commands.Contracts;

namespace Esp.Tools.OpenVPN.IPCProtocol.Configuration.Commands
{
    public class GetCertificatesCommand : BaseMessage<GetCertificatesInfo>
    {
        public const string MessageKey = "GetCertificates";

        public GetCertificatesCommand()
        {
            Code = MessageKey;
        }
    }
}