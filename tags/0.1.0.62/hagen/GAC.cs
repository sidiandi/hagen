// Copyright (c) 2009, Andreas Grimme (http://andreas-grimme.gmxhome.de/)
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
using System.Text;
using System.Reflection;
using System.IO;
using System.Runtime.InteropServices;

[ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("e707dcde-d1cd-11d2-bab9-00c04f8eceae")]
internal interface IAssemblyCache
{

    [PreserveSig()]

    int UninstallAssembly(uint dwFlags, [MarshalAs(UnmanagedType.LPWStr)] string pszAssemblyName, IntPtr pvReserved, out uint pulDisposition);

    [PreserveSig()]

    int QueryAssemblyInfo(uint dwFlags, [MarshalAs(UnmanagedType.LPWStr)] string pszAssemblyName, IntPtr pAsmInfo);

    [PreserveSig()]

    int CreateAssemblyCacheItem(uint dwFlags, IntPtr pvReserved, out /*IAssemblyCacheItem*/IntPtr ppAsmItem, [MarshalAs(UnmanagedType.LPWStr)] String pszAssemblyName);

    [PreserveSig()]

    int CreateAssemblyScavenger(out object ppAsmScavenger);

    [PreserveSig()]

    int InstallAssembly(uint dwFlags, [MarshalAs(UnmanagedType.LPWStr)] string pszManifestFilePath, IntPtr pvReserved);

}

public class FusionInstall
{

    static internal int AddAssemblyToCache(string assembly)
    {

        IAssemblyCache ac = null;

        int hr = CreateAssemblyCache(out ac, 0);

        if (hr != 0)

            return hr;

        else

            return ac.InstallAssembly(0, assembly, (IntPtr)0);

    }

    static internal int RemoveAssemblyFromCache(string assembly)
    {

        IAssemblyCache ac = null;

        uint n;

        int hr = CreateAssemblyCache(out ac, 0);

        if (hr != 0)

            return hr;

        else

            return ac.UninstallAssembly(0, assembly, (IntPtr)0, out n);

    }

    [DllImport("Fusion.dll", CharSet = CharSet.Auto)]

    internal static extern int CreateAssemblyCache(out IAssemblyCache ppAsmCache, uint dwReserved);

}
