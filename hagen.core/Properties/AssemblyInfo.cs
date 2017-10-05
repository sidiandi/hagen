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

using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("hagen")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

//In order to begin building localizable applications, set 
//<UICulture>CultureYouAreCodingWith</UICulture> in your .csproj file
//inside a <PropertyGroup>.  For example, if you are using US english
//in your source files, set the <UICulture> to en-US.  Then uncomment
//the NeutralResourceLanguage attribute below.  Update the "en-US" in
//the line below to match the UICulture setting in the project file.

//[assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.Satellite)]


[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, //where theme specific resource dictionaries are located
    //(used if a resource is not found in the page, 
    // or application resource dictionaries)
    ResourceDictionaryLocation.SourceAssembly //where the generic resource dictionary is located
    //(used if a resource is not found in the page, 
    // app, or any theme specific resource dictionaries)
)]

[assembly: InternalsVisibleTo("hagen.core.Tests, PublicKey=0024000004800000140100000602000000240000525341310008000001000100fd40d9aae52d5b4f362013ac6bf630044d9033ad0de2713af39499c95f40632794b77b464435c959c66a8e8e9bc6f950170a4449ffa051487769ecb4c9870f7f0b78f670c24c38bac2e532b8fde2e462e53f4f9eb8e47cfd8aff4d8b3942764feeedbe251efc4fe937e57de95b4b9d55bdd4885b5e907bcc12ecc4a8d86cc15c81614bc259672083984de5d71fa0fc8d533f781593b0a5cc4eccbfc3340dee0d8ab2b3cf650cba34c9e155066e2f9ea51018c4f43acf4412f9c67c58482c47db8c55b4beb0e00800f998ff14ae222ed2afb3e84973b643bbf37aa916a394095edad16e0c1aa95765ade9fef35a0f51fd8a6e93b2628b2c039803eff087ef1ed4")]