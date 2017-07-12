using Esp.Tools.OpenVPN.IPCProtocol.Configuration.Commands.Contracts;

namespace Esp.Tools.OpenVPN.IPCProtocol.Configuration.Commands
{
    public class DeleteCertificateCommand : BaseMessage<DeleteCertificateInfo>
    {
        public DeleteCertificateCommand() 
        {
            Code = MessageKey;
        }

        public const string MessageKey = "DeleteCertificate";
    }
}
