using Esp.Tools.OpenVPN.IPCProtocol.Configuration.Commands.Contracts;

namespace Esp.Tools.OpenVPN.IPCProtocol.Configuration.Commands
{
    public class ImportPfxCommand : BaseMessage<ImportPfxInfo>
    {
        public const string MessageKey = "ImportPfx";

        public ImportPfxCommand()
        {
            Code = MessageKey;
        }
    }
}