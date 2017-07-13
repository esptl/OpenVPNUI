using Esp.Tools.OpenVPN.IPCProtocol.Configuration.Messages.Contracts;

namespace Esp.Tools.OpenVPN.IPCProtocol.Configuration.Messages
{
    public class EnrollResponseMessage : BaseMessage<EnrollResponseInfo>
    {
        public const string MessageKey = "EnrollResponse";

        public EnrollResponseMessage(int pConnection)
        {
            Connection = pConnection;
            Code = MessageKey;
        }
    }
}