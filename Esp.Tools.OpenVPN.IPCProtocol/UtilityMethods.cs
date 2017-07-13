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

namespace Esp.Tools.OpenVPN.IPCProtocol
{
    public static class UtilityMethods
    {
        public static void WriteCommandResult(Stream pOutput, IMessage pMessage)
        {
            if (pOutput.CanWrite)
            {
                var str = pMessage.Code + ":" + pMessage.Connection + ":" +
                          Convert.ToBase64String(Encoding.UTF8.GetBytes(pMessage.DataString));
                var bytes = Encoding.UTF8.GetBytes(str);
                pOutput.Write(bytes, 0, bytes.Length);
                pOutput.Flush();
            }
        }

        public static void ReadMessage(byte[] pInput, IEnumerable<IMessageReader> pList)
        {
            var dict = pList.ToDictionary(pX => pX.Code);
            var line = Encoding.UTF8.GetString(pInput);
            var parts = line.Split(':');
            var code = parts[0];
            var connection = int.Parse(parts[1]);
            var dataStr = Encoding.UTF8.GetString(Convert.FromBase64String(parts[2]));

            if (dict.ContainsKey(code))
            {
                var reader = dict[code];
                reader.ProcessMessage(connection, dataStr);
            }
        }
    }
}