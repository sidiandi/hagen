// Copyright (c) 2012, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
// 
// This file is part of hagen.
// 
// hagen is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// hagen is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with hagen. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Sidi;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Runtime.InteropServices;  //DllImport, MarshalAs
using System.ComponentModel;       //Win32Exception
using Sidi.IO;
using Sidi.Extensions;

namespace hagen.Plugin.Db
{
    public static class Extensions
    {
        public static string ReadFixedLengthUnicodeString(this Stream s, int length)
        {
            byte[] fn = new byte[length * 2];
            s.Read(fn, 0, fn.Length);
            string r = ASCIIEncoding.Unicode.GetString(fn);
            return r.Substring(0, r.IndexOf((char)0));
        }

        public static string Truncate(this string x, int maxLength)
        {
            if (x.Length > maxLength)
            {
                return x.Substring(0, maxLength);
            }
            else
            {
                return x;
            }
        }

        public static string FileNameWithContext(this IFileSystemInfo fileSystemInfo)
        {
            var p = fileSystemInfo.FullName.GetUniversalName();
            var parts = p.Parts.Reverse();
            if (p.IsUnc)
            {
                parts = new[] { p.Server, p.Share }.Concat(parts);
            }
            return parts.Take(4).Join(" < ");
        }

        public static string FileNameWithContext(this LPath resolvedPath)
        {
            var parts = resolvedPath.Parts.Reverse();
            if (resolvedPath.IsUnc)
            {
                parts = new[] { resolvedPath.Server, resolvedPath.Share }.Concat(parts);
            }
            return parts.Take(4).Join(" < ");
        }

        [DllImport("mpr.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.U4)]
        static extern int WNetGetUniversalName(
        string lpLocalPath,
        [MarshalAs(UnmanagedType.U4)] int dwInfoLevel,
        IntPtr lpBuffer,
        [MarshalAs(UnmanagedType.U4)] ref int lpBufferSize);

        const int UNIVERSAL_NAME_INFO_LEVEL = 0x00000001;
        const int REMOTE_NAME_INFO_LEVEL = 0x00000002;

        const int ERROR_MORE_DATA = 234;
        const int NOERROR = 0;

        /// <summary>
        ///  converts a drive-based path for a network resource to the UNC form
        /// </summary>
        /// <param name="localPath"></param>
        /// <returns></returns>
        public static LPath GetUniversalName(this LPath localPath)
        {
            if (localPath.IsUnc)
            {
                return localPath;
            }

            try
            {

                // The return value.
                string retVal = null;

                // The pointer in memory to the structure.
                IntPtr buffer = IntPtr.Zero;

                // Wrap in a try/catch block for cleanup.
                try
                {
                    // First, call WNetGetUniversalName to get the size.
                    int size = 0;

                    // Make the call.
                    // Pass IntPtr.Size because the API doesn't like null, even though
                    // size is zero.  We know that IntPtr.Size will be
                    // aligned correctly.
                    int apiRetVal = WNetGetUniversalName(localPath, UNIVERSAL_NAME_INFO_LEVEL, (IntPtr)IntPtr.Size, ref size);

                    // If the return value is not ERROR_MORE_DATA, then
                    // raise an exception.
                    if (apiRetVal != ERROR_MORE_DATA)
                        // Throw an exception.
                        throw new Win32Exception(apiRetVal);

                    // Allocate the memory.
                    buffer = Marshal.AllocCoTaskMem(size);

                    // Now make the call.
                    apiRetVal = WNetGetUniversalName(localPath, UNIVERSAL_NAME_INFO_LEVEL, buffer, ref size);

                    // If it didn't succeed, then throw.
                    if (apiRetVal != NOERROR)
                        // Throw an exception.
                        throw new Win32Exception(apiRetVal);

                    // Now get the string.  It's all in the same buffer, but
                    // the pointer is first, so offset the pointer by IntPtr.Size
                    // and pass to PtrToStringAnsi.
                    retVal = Marshal.PtrToStringAuto(new IntPtr(buffer.ToInt64() + IntPtr.Size), size);
                    retVal = retVal.Substring(0, retVal.IndexOf('\0'));
                }
                finally
                {
                    // Release the buffer.
                    Marshal.FreeCoTaskMem(buffer);
                }

                // First, allocate the memory for the structure.

                // That's all folks.
                return retVal;
            }
            catch
            {
                return localPath;
            }
    }
}
}
