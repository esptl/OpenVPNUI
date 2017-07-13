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

using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace Esp.Tools.OpenVPN.IPCProtocol
{
    public interface IMessage
    {
        string Code { get; }
        int Connection { get; }
        string DataString { get; set; }
    }

    public class BaseMessage<TDataContract> : IMessage where TDataContract : new()
    {
        private static readonly DataContractSerializer _serializer = new DataContractSerializer(typeof(TDataContract));

        public BaseMessage()
        {
            Data = new TDataContract();
        }

        public TDataContract Data { get; set; }

        #region IMessage Members

        public string Code { get; protected internal set; }

        public int Connection { get; set; }

        public string DataString
        {
            get
            {
                var sw = new StringWriter();
                var ms = XmlWriter.Create(sw);
                _serializer.WriteObject(ms, Data);
                ms.Flush();
                return sw.ToString();
            }
            set
            {
                var sr = new StringReader(value);
                var ms = XmlReader.Create(sr);
                Data = (TDataContract) _serializer.ReadObject(ms);
            }
        }

        #endregion
    }
}