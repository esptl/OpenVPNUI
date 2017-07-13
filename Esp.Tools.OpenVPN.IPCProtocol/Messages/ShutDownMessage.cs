using Esp.Tools.OpenVPN.IPCProtocol.Contracts;

namespace Esp.Tools.OpenVPN.IPCProtocol.Messages
{
    public class ShutDownMessage : BaseMessage<ShutDownInfo>
    {
        public const string MessageKey = "Shutdown";

        public ShutDownMessage()
        {
            Code = MessageKey;
        }
    }
}