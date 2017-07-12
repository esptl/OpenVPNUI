using Esp.Tools.OpenVPN.IPCProtocol.Configuration.Commands.Contracts;

namespace Esp.Tools.OpenVPN.IPCProtocol.Configuration.Commands
{
    public class GetCertificatesCommand : BaseMessage<GetCertificatesInfo>
    {
        public GetCertificatesCommand() 
        {
            Code = MessageKey;
        }

        public const string MessageKey = "GetCertificates";
    }
}
