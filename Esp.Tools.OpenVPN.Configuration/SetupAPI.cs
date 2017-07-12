using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Esp.Tools.OpenVPN.Configuration
{
    public static class SetupApi
    {
        /// <summary>
        ///     Flags for UpdateDriverForPlugAndPlayDevices
        /// </summary>
        [Flags]
        public enum INSTALLFLAG : uint
        {
            /// <summary>
            ///     Force the installation of the specified driver
            /// </summary>
            FORCE = 0x00000001,

            /// <summary>
            ///     Do a read-only install (no file copy)
            /// </summary>
            READONLY = 0x00000002,

            /// <summary>
            ///     No UI shown at all. API will fail if any UI must be shown.
            /// </summary>
            NONINTERACTIVE = 0x00000004,

            /// <summary>
            ///     Mask all flag bits.
            /// </summary>
            BITS = 0x00000007
        }

        public const int DIF_REGISTERDEVICE = 0x19;
        public const int SPDRP_HARDWAREID = 1;
        public const int DIGCF_PRESENT = (0x00000002);
        public const int MAX_DEV_LEN = 1000;
        public const int SPDRP_FRIENDLYNAME = (0x0000000C);
        // FriendlyName (R/W)
        public const int SPDRP_DEVICEDESC = (0x00000000);

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiGetINFClass(
            string pInfName,
            ref Guid pClassGuid,
            string pClassName,
            int pClassNameSize,
            int pRequiredSize
            );

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern IntPtr SetupDiCreateDeviceInfoList(
            ref Guid pClassGuid,
            IntPtr pHwndParent
            );

        [DllImport("Setupapi.dll")]
        public static extern bool SetupDiCreateDeviceInfo(IntPtr DeviceInfoSet, string DeviceName, ref Guid ClassGuid,
            string DeviceDescription, IntPtr hwndParent,
            int CreationFlags, ref SP_DEVINFO_DATA DeviceInfoData);

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiSetDeviceRegistryProperty(
            IntPtr pDeviceInfoSet,
            ref SP_DEVINFO_DATA pDeviceInfoData,
            int pProperty,
            string pPropertyBuffer,
            int pPropertyBufferSize
            );

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiCallClassInstaller(
            uint pInstallFunction,
            IntPtr pDeviceInfoSet,
            ref SP_DEVINFO_DATA pDeviceInfoData
            );

        [DllImport("newdev.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool UpdateDriverForPlugAndPlayDevices(IntPtr pHwndParent,
            [In, MarshalAs(UnmanagedType.LPTStr)] string pHardwareId,
            [In, MarshalAs(UnmanagedType.LPTStr)] string pFullInfPath, INSTALLFLAG pInstallFlags, IntPtr pBRebootRequired);

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiDestroyDeviceInfoList(
            IntPtr pDeviceInfoSet
            );

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetLastError();

        [DllImport("setupapi.dll")] //
        public static extern bool
            SetupDiClassGuidsFromNameA(string pClassN, ref Guid pGuids,
                uint pClassNameSize, ref uint pReqSize);

        [DllImport("setupapi.dll")]
        public static extern IntPtr //result HDEVINFO
            SetupDiGetClassDevsA(ref Guid pClassGuid, uint pEnumerator,
                IntPtr pHwndParent, uint pFlags);

        [DllImport("setupapi.dll")]
        public static extern bool
            SetupDiEnumDeviceInfo(IntPtr pDeviceInfoSet, uint pMemberIndex,
                ref SP_DEVINFO_DATA pDeviceInfoData);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupDiGetDeviceRegistryProperty(
            IntPtr pDeviceInfoSet,
            ref SP_DEVINFO_DATA pDeviceInfoData,
            uint pProperty,
            out uint pPropertyRegDataType,
            StringBuilder pPropertyBuffer,
            uint pPropertyBufferSize,
            out uint pRequiredSize
            );

        public static int EnumerateDevices(uint pDeviceIndex,
            string pClassName,
            StringBuilder pDeviceName)
        {
            uint requiredSize = 0;
            var guid = Guid.Empty;
            var guids = new Guid[1];
            var DeviceInfoData = new SP_DEVINFO_DATA();


            var res = SetupDiClassGuidsFromNameA(pClassName,
                ref guids[0], requiredSize,
                ref requiredSize);

            if (requiredSize == 0)
            {
                //incorrect class name:
                pDeviceName = new StringBuilder("");
                return -2;
            }

            if (!res)
            {
                guids = new Guid[requiredSize];
                res = SetupDiClassGuidsFromNameA(pClassName, ref guids[0], requiredSize,
                    ref requiredSize);

                if (!res || requiredSize == 0)
                {
                    //incorrect class name:
                    pDeviceName = new StringBuilder("");
                    return -2;
                }
            }

            //get device info set for our device class
            var newDeviceInfoSet = SetupDiGetClassDevsA(ref guids[0], 0, IntPtr.Zero,
                DIGCF_PRESENT);
            if (newDeviceInfoSet == IntPtr.Add(IntPtr.Zero,-1))
                if (!res)
                {
                    //device information is unavailable:
                    pDeviceName = new StringBuilder("");
                    return -3;
                }

            DeviceInfoData.cbSize = 28;
            //is devices exist for class
            DeviceInfoData.DevInst = 0;
            DeviceInfoData.ClassGuid = Guid.Empty;
            DeviceInfoData.Reserved = (IntPtr) 0;

            res = SetupDiEnumDeviceInfo(newDeviceInfoSet,
                pDeviceIndex, ref DeviceInfoData);
            if (!res)
            {
                //no such device:
                SetupDiDestroyDeviceInfoList(newDeviceInfoSet);
                pDeviceName = new StringBuilder("");
                return -1;
            }


            pDeviceName.Capacity = MAX_DEV_LEN;
            uint index = 0;
            uint requiredsize = 0;
            if (!SetupDiGetDeviceRegistryProperty(newDeviceInfoSet,
                ref DeviceInfoData,
                SPDRP_FRIENDLYNAME, out index, pDeviceName, MAX_DEV_LEN, out requiredsize))
            {
                res = SetupDiGetDeviceRegistryProperty(newDeviceInfoSet,
                    ref DeviceInfoData, SPDRP_DEVICEDESC, out index, pDeviceName, MAX_DEV_LEN,
                    out requiredsize);
                if (!res)
                {
                    //incorrect device name:
                    SetupDiDestroyDeviceInfoList(newDeviceInfoSet);
                    pDeviceName = new StringBuilder("");
                    return -4;
                }
            }
            return 0;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SP_DEVINFO_DATA
        {
            public uint cbSize;
            public Guid ClassGuid;
            public uint DevInst;
            public IntPtr Reserved;
        }
    }

    public static class TapDeviceManager
    {
        private const string ClassName = "tap0901";
        private const string HardwareID = ClassName;
        private static readonly string _infPath;

        static TapDeviceManager()
        {
            _infPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"\OpenVPN\tap\oemvista.inf";
        }

        private static Guid GetClassGuid()
        {
            var guid = new Guid();
            // var inf = 
            if (SetupApi.SetupDiGetINFClass(_infPath, ref guid, ClassName, 32, 0))
            {
                return guid;
            }
            throw new IOException("Inf file could not be loaded");
        }

        public static IEnumerable<TapDevice> GetTapDevices()
        {
            var guid = GetClassGuid();
            var propertyBuffer = new StringBuilder();
            var deviceInfoData = new SetupApi.SP_DEVINFO_DATA();
            var deviceInfoSet = SetupApi.SetupDiGetClassDevsA(ref guid, 0, IntPtr.Zero,
                SetupApi.DIGCF_PRESENT);
            if (deviceInfoSet == IntPtr.Add(IntPtr.Zero, -1))

            {
            }

            var cont = true;
            for (var i = 0; cont; i++)
            {
                deviceInfoData.cbSize = (uint) Marshal.SizeOf(typeof (SetupApi.SP_DEVINFO_DATA));
                ;
                //is devices exist for class
                deviceInfoData.DevInst = 0;
                deviceInfoData.ClassGuid = guid;
                deviceInfoData.Reserved = (IntPtr) 0;

                var res = SetupApi.SetupDiEnumDeviceInfo(deviceInfoSet,
                    (uint) i, ref deviceInfoData);
                if (!res && deviceInfoData.DevInst == 0)
                {
                    cont = false;
                    break;
                }


                propertyBuffer.Capacity = SetupApi.MAX_DEV_LEN;

                uint index = 0;
                uint requiredSize = 0;
                res = SetupApi.SetupDiGetDeviceRegistryProperty(deviceInfoSet,
                    ref deviceInfoData, SetupApi.SPDRP_DEVICEDESC, out index,
                    propertyBuffer, SetupApi.MAX_DEV_LEN,
                    out requiredSize);
                if (!res)
                {
                    var error = SetupApi.GetLastError();
                    SetupApi.SetupDiDestroyDeviceInfoList(deviceInfoSet);
                    throw new Win32Exception(error);
                }

                var device = new TapDevice
                {
                    Name = propertyBuffer.ToString(),
                    DevInst = deviceInfoData.DevInst
                };
                index = 0;
                requiredSize = 0;
                res = SetupApi.SetupDiGetDeviceRegistryProperty(deviceInfoSet,
                    ref deviceInfoData, SetupApi.SPDRP_HARDWAREID, out index,
                    propertyBuffer, SetupApi.MAX_DEV_LEN,
                    out requiredSize);
                if (!res)
                {
                    var error = SetupApi.GetLastError();
                    SetupApi.SetupDiDestroyDeviceInfoList(deviceInfoSet);
                    throw new Win32Exception(error);
                }
                device.HardwareId = propertyBuffer.ToString();
                if (device.HardwareId == HardwareID)
                    yield return device;
            }
        }

        public static bool SetupTapDevice()
        {
            var guid = GetClassGuid();

            var deviceInfoSet = SetupApi.SetupDiCreateDeviceInfoList(ref guid, IntPtr.Zero);
            var da = new SetupApi.SP_DEVINFO_DATA();
            da.cbSize = (uint) Marshal.SizeOf(typeof (SetupApi.SP_DEVINFO_DATA));
            var success = SetupApi.SetupDiCreateDeviceInfo(deviceInfoSet, ClassName, ref guid, null,
                IntPtr.Zero, 0x00000001, ref da);
            if (!success)
                throw new Win32Exception(SetupApi.GetLastError());

            var result = SetupApi.SetupDiSetDeviceRegistryProperty(deviceInfoSet, ref da,
                SetupApi.SPDRP_HARDWAREID,
                HardwareID, 2000);
            if (!result)
            {
                var error = SetupApi.GetLastError();
                SetupApi.SetupDiDestroyDeviceInfoList(deviceInfoSet);
                throw new Win32Exception(error, string.Format("SetupDiSetDeviceRegistryProperty {0}", error));
            }
            result = SetupApi.SetupDiCallClassInstaller(SetupApi.DIF_REGISTERDEVICE, deviceInfoSet, ref da);
            if (!result)
            {
                var error = SetupApi.GetLastError();
                SetupApi.SetupDiDestroyDeviceInfoList(deviceInfoSet);
                throw new Win32Exception(error, string.Format("SetupDiCallClassInstaller {0}", error));
            }
            var rest = IntPtr.Zero;
            result = SetupApi.UpdateDriverForPlugAndPlayDevices(IntPtr.Zero, HardwareID, _infPath,
                SetupApi.INSTALLFLAG.FORCE, rest);
            if (!result)
            {
                var error = SetupApi.GetLastError();
                SetupApi.SetupDiCallClassInstaller(0x00000005, deviceInfoSet, ref da);
                throw new Win32Exception(error, string.Format("UpdateDriverForPlugAndPlayDevices {0}", error));
            }
            return true;
        }

        public class TapDevice
        {
            public string Name { get; internal set; }

            public string HardwareId { get; set; }

            public uint DevInst { get; set; }
        }
    }
}