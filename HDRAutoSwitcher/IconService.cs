using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HDRAutoSwitcher
{
    internal static class IconService
    {
        private const uint SHGFI_ICON = 0x000000100;
        private const uint SHGFI_LARGEICON = 0x000000000;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        [DllImport(
            "shell32.dll",
            CharSet = CharSet.Unicode)]
        private static extern IntPtr SHGetFileInfo(
            string pszPath,
            uint dwFileAttributes,
            out SHFILEINFO psfi,
            uint cbFileInfo,
            uint uFlags);

        [DllImport("user32.dll")]
        private static extern bool DestroyIcon(
            IntPtr hIcon);

        public static ImageSource? GetIcon(
            string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return null;
            }

            SHFILEINFO fileInfo;

            IntPtr result = SHGetFileInfo(
                filePath,
                0,
                out fileInfo,
                (uint)Marshal.SizeOf<SHFILEINFO>(),
                SHGFI_ICON | SHGFI_LARGEICON);

            if (result == IntPtr.Zero ||
                fileInfo.hIcon == IntPtr.Zero)
            {
                return null;
            }

            try
            {
                BitmapSource icon =
                    Imaging.CreateBitmapSourceFromHIcon(
                        fileInfo.hIcon,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());

                icon.Freeze();

                return icon;
            }
            finally
            {
                DestroyIcon(fileInfo.hIcon);
            }
        }
    }
}