//  This file is part of OpenVPN UI.
//  Copyright 2011 ESP Technologies Ltd.
//
//  Author: James Martin - August 2011
//
//
//  Foobar is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  OpenVPN UI is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with OpenVPN UI.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Esp.Tools.OpenVPN.IPCProtocol
{
    public static class UtilityMethods
    {
        public static void WriteCommandResult(Stream pOutput, IMessage pMessage)
        {
            if (pOutput.CanWrite)
            {
                var bytes = GetBytes(pMessage);
                pOutput.Write(bytes, 0, bytes.Length);
                pOutput.Flush();
            }
        }

        private static byte[] GetBytes(IMessage pMessage)
        {
            var ms = new MemoryStream();
            var br = new BinaryWriter(ms);
            var codeBytes = Encoding.UTF8.GetBytes(pMessage.Code);
            br.Write(pMessage.Code);
            br.Write(pMessage.Connection);
            br.Write(pMessage.DataString);
            br.Flush();
            return ms.ToArray();
        }

        public static async Task WriteCommandResultAsync(Stream pOutput, IMessage pMessage, CancellationToken pCancellationToken)
        {
            if (pOutput.CanWrite)
            {
                var bytes = GetBytes(pMessage);
                await pOutput.WriteAsync(bytes, 0, bytes.Length, pCancellationToken);
                await pOutput.FlushAsync(pCancellationToken);
            }
        }

        public static async Task ReadMessage(byte[] pInput, IEnumerable<IMessageReader> pList)
        {
            var dict = pList.ToDictionary(pX => pX.Code);

            var memoryStream = new MemoryStream(pInput);
            var br = new BinaryReader(memoryStream);

            while (memoryStream.Position < memoryStream.Length)
            {

                var code = br.ReadString();
                var connection = br.ReadInt32();
                var dataString = br.ReadString();
                if (dict.ContainsKey(code))
                {
                    var reader = dict[code];
                    await reader.ProcessMessage(connection, dataString);
                }
            }
        }
    }
}