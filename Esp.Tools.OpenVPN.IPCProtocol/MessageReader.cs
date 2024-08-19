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
using System.Threading.Tasks;

namespace Esp.Tools.OpenVPN.IPCProtocol
{
    public interface IMessageReader
    {
        string Code { get; }
        Task ProcessMessage(int pConnection, string pMessageData);
    }

    public class MessageReader<TMessageType> : IMessageReader where TMessageType : new()
    {
        public Func<BaseMessage<TMessageType>, Task> MessageRecieved;

        public MessageReader(string pCode)
        {
            Code = pCode;
        }

        #region IMessageReader Members

        public string Code { get; }

        public async Task ProcessMessage(int pConnection, string pMessageData)
        {
            var result = new BaseMessage<TMessageType>
            {
                Code = Code,
                Connection = pConnection,
                DataString = pMessageData
            };
            if (MessageRecieved != null)
                await MessageRecieved(result);
        }

        #endregion
    }
}