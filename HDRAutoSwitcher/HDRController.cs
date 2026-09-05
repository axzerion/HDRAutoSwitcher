using System;
using System.Runtime.InteropServices;

namespace HDRAutoSwitcher
{
    internal class HDRController
    {
        private const uint QDC_ONLY_ACTIVE_PATHS = 0x00000002;
        private const int ERROR_SUCCESS = 0;
        private const int DISPLAYCONFIG_DEVICE_INFO_GET_ADVANCED_COLOR_INFO = 9;
        private const int DISPLAYCONFIG_DEVICE_INFO_SET_ADVANCED_COLOR_STATE = 10;

        // ============================================================
        // Windows API
        // ============================================================

        [DllImport("user32.dll")]
        private static extern int GetDisplayConfigBufferSizes(
            uint flags,
            out int numPathArrayElements,
            out int numModeInfoArrayElements);

        [DllImport("user32.dll")]
        private static extern int QueryDisplayConfig(
            uint flags,
            ref int numPathArrayElements,
            [In, Out] DISPLAYCONFIG_PATH_INFO[] pathArray,
            ref int numModeInfoArrayElements,
            [In, Out] DISPLAYCONFIG_MODE_INFO[] modeInfoArray,
            IntPtr currentTopologyId);

        [DllImport("user32.dll")]
        private static extern int DisplayConfigGetDeviceInfo(
            ref DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO requestPacket);

        [DllImport("user32.dll")]
        private static extern int DisplayConfigSetDeviceInfo(
            ref DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE requestPacket);

        [StructLayout(LayoutKind.Sequential)]
        private struct DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE
        {
            public DISPLAYCONFIG_DEVICE_INFO_HEADER header;

            // Bit 0 controls whether advanced color is enabled.
            public uint value;
        }

        // ============================================================
        // Basic structures
        // ============================================================

        [StructLayout(LayoutKind.Sequential)]
        private struct LUID
        {
            public uint LowPart;
            public int HighPart;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DISPLAYCONFIG_DEVICE_INFO_HEADER
        {
            public int type;
            public int size;
            public LUID adapterId;
            public uint id;
        }

        // ============================================================
        // HDR structure
        // ============================================================

        [StructLayout(LayoutKind.Sequential)]
        private struct DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO
        {
            public DISPLAYCONFIG_DEVICE_INFO_HEADER header;

            // Native structure contains a UINT32 union.
            public uint value;

            public int colorEncoding;

            public int bitsPerColorChannel;

            public bool AdvancedColorSupported =>
                (value & 0x1) != 0;

            public bool AdvancedColorEnabled =>
                (value & 0x2) != 0;

            public bool WideColorEnforced =>
                (value & 0x4) != 0;

            public bool AdvancedColorForceDisabled =>
                (value & 0x8) != 0;
        }

        // ============================================================
        // Display configuration
        // ============================================================

        [StructLayout(LayoutKind.Sequential)]
        private struct DISPLAYCONFIG_PATH_INFO
        {
            public DISPLAYCONFIG_PATH_SOURCE_INFO sourceInfo;
            public DISPLAYCONFIG_PATH_TARGET_INFO targetInfo;
            public uint flags;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DISPLAYCONFIG_PATH_SOURCE_INFO
        {
            public LUID adapterId;
            public uint id;
            public uint modeInfoIdx;
            public uint statusFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DISPLAYCONFIG_PATH_TARGET_INFO
        {
            public LUID adapterId;
            public uint id;
            public uint modeInfoIdx;

            public uint outputTechnology;
            public uint rotation;
            public uint scaling;

            public DISPLAYCONFIG_RATIONAL refreshRate;

            public uint scanLineOrdering;

            public bool targetAvailable;

            public uint statusFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DISPLAYCONFIG_RATIONAL
        {
            public uint Numerator;
            public uint Denominator;
        }

        // ============================================================
        // MODE_INFO
        // ============================================================

        [StructLayout(LayoutKind.Explicit)]
        private struct DISPLAYCONFIG_MODE_INFO_UNION
        {
            [FieldOffset(0)]
            public DISPLAYCONFIG_TARGET_MODE targetMode;

            [FieldOffset(0)]
            public DISPLAYCONFIG_SOURCE_MODE sourceMode;

            [FieldOffset(0)]
            public DISPLAYCONFIG_DESKTOP_IMAGE_INFO desktopImageInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DISPLAYCONFIG_MODE_INFO
        {
            public uint infoType;
            public uint id;
            public LUID adapterId;
            public DISPLAYCONFIG_MODE_INFO_UNION info;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DISPLAYCONFIG_TARGET_MODE
        {
            public DISPLAYCONFIG_VIDEO_SIGNAL_INFO targetVideoSignalInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DISPLAYCONFIG_SOURCE_MODE
        {
            public uint width;
            public uint height;
            public uint pixelFormat;
            public DISPLAYCONFIG_POINTL position;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DISPLAYCONFIG_DESKTOP_IMAGE_INFO
        {
            public DISPLAYCONFIG_POINTL PathSourceSize;
            public DISPLAYCONFIG_RECT DesktopImageRegion;
            public DISPLAYCONFIG_RECT DesktopImageClip;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DISPLAYCONFIG_VIDEO_SIGNAL_INFO
        {
            public ulong pixelRate;

            public DISPLAYCONFIG_RATIONAL hSyncFreq;
            public DISPLAYCONFIG_RATIONAL vSyncFreq;

            public DISPLAYCONFIG_2DREGION activeSize;
            public DISPLAYCONFIG_2DREGION totalSize;

            public uint videoStandard;
            public uint scanLineOrdering;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DISPLAYCONFIG_2DREGION
        {
            public uint cx;
            public uint cy;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DISPLAYCONFIG_POINTL
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DISPLAYCONFIG_RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        // ============================================================
        // Test
        // ============================================================

        public void TestDisplayQuery()
        {
            int result = GetDisplayConfigBufferSizes(
                QDC_ONLY_ACTIVE_PATHS,
                out int pathCount,
                out int modeCount);

            if (result != ERROR_SUCCESS)
            {
                Console.WriteLine(
                    $"GetDisplayConfigBufferSizes failed: {result}");
                return;
            }

            DISPLAYCONFIG_PATH_INFO[] paths =
                new DISPLAYCONFIG_PATH_INFO[pathCount];

            DISPLAYCONFIG_MODE_INFO[] modes =
                new DISPLAYCONFIG_MODE_INFO[modeCount];

            result = QueryDisplayConfig(
                QDC_ONLY_ACTIVE_PATHS,
                ref pathCount,
                paths,
                ref modeCount,
                modes,
                IntPtr.Zero);

            if (result != ERROR_SUCCESS)
            {
                Console.WriteLine(
                    $"QueryDisplayConfig failed: {result}");
                return;
            }

            Console.WriteLine(
                $"Successfully queried {pathCount} display path(s).");

            for (int i = 0; i < pathCount; i++)
            {
                DISPLAYCONFIG_PATH_INFO path = paths[i];

                Console.WriteLine($"Display path {i}:");

                Console.WriteLine(
                    $"  Adapter ID: {path.targetInfo.adapterId.LowPart}");

                Console.WriteLine(
                    $"  Target ID: {path.targetInfo.id}");

                Console.WriteLine(
                    $"  Target Mode Index: {path.targetInfo.modeInfoIdx}");

                Console.WriteLine(
                    $"  Source Adapter ID: {path.sourceInfo.adapterId.LowPart}");

                Console.WriteLine(
                    $"  Source ID: {path.sourceInfo.id}");

                GetHDRState(path);
            }
        }

        // ============================================================
        // Get HDR state
        // ============================================================

        private void GetHDRState(
            DISPLAYCONFIG_PATH_INFO path)
        {
            DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO colorInfo = new();

            colorInfo.header.type =
                DISPLAYCONFIG_DEVICE_INFO_GET_ADVANCED_COLOR_INFO;

            colorInfo.header.size =
                Marshal.SizeOf<DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO>();

            colorInfo.header.adapterId =
                path.targetInfo.adapterId;

            colorInfo.header.id =
                path.targetInfo.id;

            Console.WriteLine(
                $"  Request size: {colorInfo.header.size}");

            Console.WriteLine(
                $"  Request adapter: {colorInfo.header.adapterId.LowPart}");

            Console.WriteLine(
                $"  Request target: {colorInfo.header.id}");

            int result =
                DisplayConfigGetDeviceInfo(ref colorInfo);

            if (result != ERROR_SUCCESS)
            {
                Console.WriteLine(
                    $"  DisplayConfigGetDeviceInfo failed: {result}");

                Console.WriteLine(
                    $"  ERROR_INVALID_PARAMETER = 87");

                return;
            }

            Console.WriteLine(
                $"  HDR supported: {colorInfo.AdvancedColorSupported}");

            Console.WriteLine(
                $"  HDR enabled: {colorInfo.AdvancedColorEnabled}");

            Console.WriteLine(
                $"  Wide color enforced: {colorInfo.WideColorEnforced}");

            Console.WriteLine(
                $"  HDR force disabled: {colorInfo.AdvancedColorForceDisabled}");

            Console.WriteLine(
                $"  Bits per color channel: {colorInfo.bitsPerColorChannel}");
        }
        public bool SetHDRState(bool enabled)
        {
            // First find the active display.
            int result = GetDisplayConfigBufferSizes(
                QDC_ONLY_ACTIVE_PATHS,
                out int pathCount,
                out int modeCount);

            if (result != ERROR_SUCCESS)
            {
                Console.WriteLine(
                    $"GetDisplayConfigBufferSizes failed: {result}");

                return false;
            }

            DISPLAYCONFIG_PATH_INFO[] paths =
                new DISPLAYCONFIG_PATH_INFO[pathCount];

            DISPLAYCONFIG_MODE_INFO[] modes =
                new DISPLAYCONFIG_MODE_INFO[modeCount];

            result = QueryDisplayConfig(
                QDC_ONLY_ACTIVE_PATHS,
                ref pathCount,
                paths,
                ref modeCount,
                modes,
                IntPtr.Zero);

            if (result != ERROR_SUCCESS)
            {
                Console.WriteLine(
                    $"QueryDisplayConfig failed: {result}");

                return false;
            }

            if (pathCount == 0)
            {
                Console.WriteLine("No active display found.");
                return false;
            }

            // For now we use the first active display.
            DISPLAYCONFIG_PATH_INFO path = paths[0];

            DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE request = new();

            request.header = new DISPLAYCONFIG_DEVICE_INFO_HEADER
            {
                type = DISPLAYCONFIG_DEVICE_INFO_SET_ADVANCED_COLOR_STATE,

                size = Marshal.SizeOf<DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE>(),

                adapterId = path.targetInfo.adapterId,

                id = path.targetInfo.id
            };

            request.value = enabled ? 1u : 0u;

            Console.WriteLine(
                $"Setting HDR {(enabled ? "ON" : "OFF")}...");

            result = DisplayConfigSetDeviceInfo(
                ref request);

            if (result != ERROR_SUCCESS)
            {
                Console.WriteLine(
                    $"DisplayConfigSetDeviceInfo failed: {result}");

                return false;
            }

            Console.WriteLine(
                $"HDR {(enabled ? "enabled" : "disabled")} successfully.");

            return true;
        }
        public bool IsHDREnabled()
        {
            int result = GetDisplayConfigBufferSizes(
                QDC_ONLY_ACTIVE_PATHS,
                out int pathCount,
                out int modeCount);

            if (result != ERROR_SUCCESS || pathCount == 0)
            {
                return false;
            }

            DISPLAYCONFIG_PATH_INFO[] paths =
                new DISPLAYCONFIG_PATH_INFO[pathCount];

            DISPLAYCONFIG_MODE_INFO[] modes =
                new DISPLAYCONFIG_MODE_INFO[modeCount];

            result = QueryDisplayConfig(
                QDC_ONLY_ACTIVE_PATHS,
                ref pathCount,
                paths,
                ref modeCount,
                modes,
                IntPtr.Zero);

            if (result != ERROR_SUCCESS || pathCount == 0)
            {
                return false;
            }

            DISPLAYCONFIG_PATH_INFO path = paths[0];

            DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO colorInfo = new();

            colorInfo.header = new DISPLAYCONFIG_DEVICE_INFO_HEADER
            {
                type = DISPLAYCONFIG_DEVICE_INFO_GET_ADVANCED_COLOR_INFO,
                size = Marshal.SizeOf<DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO>(),
                adapterId = path.targetInfo.adapterId,
                id = path.targetInfo.id
            };

            result = DisplayConfigGetDeviceInfo(ref colorInfo);

            if (result != ERROR_SUCCESS)
            {
                return false;
            }

            return colorInfo.AdvancedColorEnabled;
        }
        public bool EnableHDR()
        {
            return SetHDRState(true);
        }

        public bool DisableHDR()
        {
            return SetHDRState(false);
        }
    }
}