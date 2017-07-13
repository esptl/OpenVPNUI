using Esp.Tools.OpenVPN.IPCProtocol.Configuration.Commands.Contracts;

namespace Esp.Tools.OpenVPN.IPCProtocol.Configuration.Commands
{
    public class SetConfigurationCertificateCommand : BaseMessage<SetConfigurationCertificateInfo>
    {
        public const string MessageKey = "SetConfigCert";

        public SetConfigurationCertificateCommand()
        {
            Code = MessageKey;
        }
    }
}