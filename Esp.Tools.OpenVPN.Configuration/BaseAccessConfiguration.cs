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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Esp.Tools.OpenVPN.Configuration
{
    public class BaseAccessConfiguration
    {
        private readonly string _default;
        private readonly string _registryKeyName;
        private readonly string _registryKeyPath;
        private readonly List<string> _groups;

        public BaseAccessConfiguration(string pRegistryKeyPath, string pRegistryKeyName, string pDefault)
        {
            _registryKeyPath = pRegistryKeyPath;
            _registryKeyName = pRegistryKeyName;
            _default = pDefault;
            _groups = LoadAvailableGroups();
        }

        public List<string> UnselectedGroups
        {
            get
            {
                var selected = SelectedGroups;
                return _groups.Where(pX => !selected.Contains(pX)).ToList();
            }
        }


        public List<string> SelectedGroups
        {
            get
            {
                using (var path = Registry.LocalMachine.OpenSubKey(_registryKeyPath, true) ??
                                  Registry.LocalMachine.CreateSubKey(_registryKeyPath))
                {
                    var value = path.GetValue(_registryKeyName);
                    if (value == null)
                    {
                        var availableGroups = LoadAvailableGroups();
                        if (availableGroups.Contains("Home ") || availableGroups.Contains(_default))
                            value = _default;
                        path.SetValue(_registryKeyName, value);
                    }
                    return value.ToString().Split(',').Select(pX => pX.Trim()).ToList();
                }
            }

            set
            {
                var str = value.Aggregate("", (pAccum, pItem) => pAccum + "," + pItem).Substring(1);
                using (var path = Registry.LocalMachine.OpenSubKey(_registryKeyPath, true) ??
                                  Registry.LocalMachine.CreateSubKey(_registryKeyPath))
                {
                    path.SetValue(_registryKeyName, str);
                }
            }
        }

        #region Windows API Wrapper for loading available groups

        private static class Win32API
        {
            #region Win32 API Interfaces

            [DllImport("netapi32.dll", EntryPoint = "NetApiBufferFree")]
            internal static extern void NetApiBufferFree(IntPtr pBufptr);

            [DllImport("netapi32.dll", EntryPoint = "NetLocalGroupEnum")]
            internal static extern uint NetLocalGroupEnum(
                IntPtr pServerName,
                uint pLevel,
                ref IntPtr pSiPtr,
                uint pRefmaxlen,
                ref uint pEntriesread,
                ref uint pTotalentries,
                IntPtr pResumeHandle);


            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            internal struct LOCALGROUP_INFO_1
            {
                public readonly IntPtr lpszGroupName;
                public readonly IntPtr lpszComment;
            }

            #endregion
        }

        private List<string> LoadAvailableGroups()
        {
            int localgroupInfo1Size;

            unsafe
            {
                localgroupInfo1Size = sizeof(Win32API.LOCALGROUP_INFO_1);
            }

            const uint level = 1;
            const uint prefmaxlen = 0xFFFFFFFF;
            uint entriesread = 0;
            uint totalentries = 0;

            var groupInfoPtr = IntPtr.Zero;

            Win32API.NetLocalGroupEnum(
                IntPtr.Zero,
                level,
                ref groupInfoPtr,
                prefmaxlen,
                ref entriesread,
                ref totalentries,
                IntPtr.Zero);

            try
            {
                var result = new List<string>();

                for (var i = 0; i < totalentries; i++)
                {
                    var newOffset = groupInfoPtr.ToInt32() +
                                    localgroupInfo1Size * i;
                    var groupInfo =
                        (Win32API.LOCALGROUP_INFO_1) Marshal.PtrToStructure(
                            new IntPtr(newOffset), typeof(Win32API.LOCALGROUP_INFO_1));
                    var currentGroupName = Marshal.PtrToStringAuto(groupInfo.lpszGroupName);

                    result.Add(currentGroupName);
                }
                return result;
            }
            finally
            {
                Win32API.NetApiBufferFree(groupInfoPtr);
            }
        }

        #endregion
    }
}