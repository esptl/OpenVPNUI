using Esp.Tools.OpenVPN.IPCProtocol.Configuration.Messages.Contracts;

namespace Esp.Tools.OpenVPN.IPCProtocol.Configuration.Messages
{
    public class EnrollRequestResponseMessage : BaseMessage<EnrollRequestResponseInfo>
    {
        public const string MessageKey = "EnrollRequestResponse";

        public EnrollRequestResponseMessage(int pConnection)
        {
            Connection = pConnection;
            Code = MessageKey;
        }
    }
}