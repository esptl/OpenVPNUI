using Esp.Tools.OpenVPN.IPCProtocol.Configuration.Messages.Contracts;

namespace Esp.Tools.OpenVPN.IPCProtocol.Configuration.Messages
{
    public class AvailableCertificatesMessage : BaseMessage<CertificatesInfo>
    {
        public const string MessageKey = "Certificates";

        public AvailableCertificatesMessage(int pConnection)
        {
            Connection = pConnection;
            Code = MessageKey;
        }
    }
}