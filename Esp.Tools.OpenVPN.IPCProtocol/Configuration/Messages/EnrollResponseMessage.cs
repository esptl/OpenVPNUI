using Esp.Tools.OpenVPN.IPCProtocol.Configuration.Messages.Contracts;

namespace Esp.Tools.OpenVPN.IPCProtocol.Configuration.Messages
{
    public class EnrollResponseMessage: BaseMessage<EnrollResponseInfo>
    {
        public EnrollResponseMessage(int pConnection)
        {
            Connection = pConnection;
            Code = MessageKey;
        }

        public const string MessageKey = "EnrollResponse";
    }
}
