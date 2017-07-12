using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Esp.Tools.OpenVPN.IPCProtocol.Configuration.Commands.Contracts;

namespace Esp.Tools.OpenVPN.IPCProtocol.Configuration.Commands
{
    public class ImportPfxCommand: BaseMessage<ImportPfxInfo>
    {
        public ImportPfxCommand()
        {
            Code = MessageKey;
        }

        public const string MessageKey = "ImportPfx";
    }
}
