//  This file is part of OpenVPN UI.
//  Copyright 2011 ESP Technologies Ltd.
//
//  Author: James Martin - October 2011
//
//
//  OpenVPN UI is free software: you can redistribute it and/or modify
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

using System.Diagnostics;

namespace Esp.Tools.OpenVPN.EventLog
{
    public class EventLogHelper
    {
        private const string Source = "OpenVPNService";

        public static void LogEvent(string pMessage)
        {
            if (!System.Diagnostics.EventLog.SourceExists(Source))
                System.Diagnostics.EventLog.CreateEventSource(Source, "Application");
            System.Diagnostics.EventLog.WriteEntry(Source, pMessage, EventLogEntryType.Warning);
        }
    }
}