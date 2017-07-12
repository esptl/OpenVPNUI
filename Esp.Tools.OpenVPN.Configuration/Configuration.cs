//  This file is part of OpenVPN UI.
//  Copyright 2011 ESP Technologies Ltd.
//
//  Author: James Martin - September 2011
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
//  This file is part of OpenVPN UI.
//  Copyright 2011 ESP Technologies Ltd.

using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using Microsoft.Win32;

namespace Esp.Tools.OpenVPN.Configuration
{
    public sealed class Configuration
    {
        private const string TapDriverName = "tap0901";
        static Configuration()
        {
            Current = new Configuration();
        }

        public static Configuration Current { get; private set; }

        public string ControlPipe
        {
            get { return "opepnvpnui_control"; }
        }

        public string ConfigPipe
        {
            get { return "opepnvpnui_config"; }
        }

        private ControllerAccessConfiguration _controllerAccess;
        public ControllerAccessConfiguration ControllerAccess
        {
            get
            {
                if(_controllerAccess==null)
                    _controllerAccess = new ControllerAccessConfiguration();
                return _controllerAccess;
            }
           
        }

        public string AppDataPath
        {
            get
            {
                var path = Environment.GetEnvironmentVariable("ALLUSERSPROFILE") + "\\OpenVPN UI\\";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                return path;
            }
        }

        public string ServiceName
        {
            get { return "OpenVPNUIHostService"; }
        }

        public string ConnecitonDataPath
        {
            get
            {
                var path = AppDataPath + "connecitons";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                return path;
            }
        }

        public List<NetworkInterface> TapInterfaces
        {
            get
            {
                var result = new List<NetworkInterface>();
                using (var registry =
                    Registry.LocalMachine.OpenSubKey(
                        "SYSTEM\\CurrentControlSet\\Control\\Class\\{4D36E972-E325-11CE-BFC1-08002BE10318}"))
                {
                    var interFaces = NetworkInterface.GetAllNetworkInterfaces().ToDictionary(pX => pX.Id);
                    foreach (var key in registry.GetSubKeyNames())
                    {
                        var val = 0;
                        if (Int32.TryParse(key, out val))
                        {
                            using (var subkey = registry.OpenSubKey(key))
                            {
                                var componentId = subkey.GetValue("ComponentId");
                                if(componentId.ToString()==TapDriverName)
                                {
                                    var instanceId =subkey.GetValue("NetCfgInstanceId").ToString();
                                    if(interFaces.ContainsKey(instanceId))
                                        result.Add(interFaces[instanceId]);
                                }
                            }
                        }
                    }
                }

                return result;
            }
        }

        public string WorkingPath
        {
            get
            {
                return
                    Path.GetDirectoryName(
                        System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase.Substring(8).Replace('/',
                                                                                                                  '\\')) +
                    "\\";
            }
        }
    }
}
