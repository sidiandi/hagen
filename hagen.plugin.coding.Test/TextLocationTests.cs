using Amg.Extensions;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;

namespace hagen.plugin.coding.Test
{
    [TestFixture]
    public class TextLocationTests
    {
        [Test]
        public void Find()
        {
            var locations = TextLocation.Find(@"1>------ Build started: Project: hagen.plugin, Configuration: Debug Any CPU ------
1>C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\Microsoft.Common.CurrentVersion.targets(2106,5): warning MSB3277: Found conflicts between different versions of ""System.Data.SQLite"" that could not be resolved.  These reference conflicts are listed in the build log when log verbosity is set to detailed.
1>  hagen.plugin -> C:\src\hagen\out\Debug\hagen.plugin\bin\hagen.plugin.dll
2>------ Build started: Project: hagen.core, Configuration: Debug Any CPU ------
3>------ Build started: Project: hagen.plugin.Test, Configuration: Debug Any CPU ------
4>------ Build started: Project: hagen.plugin.db, Configuration: Debug Any CPU ------
5>------ Build started: Project: hagen.plugin.file, Configuration: Debug Any CPU ------
").ToList();
            Console.WriteLine(locations.Join());
            Assert.That(locations[0], Is.EqualTo(new TextLocation(@"C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\Microsoft.Common.CurrentVersion.targets", 2106, 5,
                "warning MSB3277: Found conflicts between different versions of \"System.Data.SQLite\" that could not be resolved.  These reference conflicts are listed in the build log when log verbosity is set to detailed."
                )));
            Assert.That(locations[1], Is.EqualTo(new TextLocation(@"C:\src\hagen\out\Debug\hagen.plugin\bin\hagen.plugin.dll")));
        }

        [Test]
        public void MsBuildOutput()
        {
            var text = @"25484:Program.cs(179,20): warning CS8618: Non-nullable field 'Attendance' is uninitialized. Consider declaring the field as nullable. [C:\src\hagen\activityReport\activityReport.csproj]";
            var locations = TextLocation.Find(text)
                .ToList();
            Assert.That(locations[0], Is.EqualTo(new TextLocation(
                @"C:\src\hagen\activityReport\Program.cs",
                179, 20, text)));
        }

        [Test]
        public void MsBuildOutput2()
        {
            var locations = TextLocation.Find(@"25484:TextLocation.cs(11,23): warning CS8618: Non-nullable field 'FileName' is uninitialized. Consider declaring the field as nullable. [C:\src\hagen\hagen.plugin.office\hagen.plugin.office.csproj]
25484:TextLocation.cs(12,23): warning CS8618: Non-nullable field 'Text' is uninitialized. Consider declaring the field as nullable. [C:\src\hagen\hagen.plugin.office\hagen.plugin.office.csproj]
25484:OutlookExtensions.cs(151,24): warning CS8601: Possible null reference assignment. [C:\src\hagen\hagen.plugin.office\hagen.plugin.office.csproj]
25484:OutlookExtensions.cs(155,20): warning CS8625: Cannot convert null literal to non-nullable reference type. [C:\src\hagen\hagen.plugin.office\hagen.plugin.office.csproj]
25484:TimeParser.cs(164,31): warning CS8600: Converting null literal or possible null value to non-nullable type. [C:\src\hagen\hagen.plugin.office\hagen.plugin.office.csproj]
25484:TimeParser.cs(234,27): warning CS8618: Non-nullable property 'Text' is uninitialized. Consider declaring the property as nullable. [C:\src\hagen\hagen.plugin.office\hagen.plugin.office.csproj]
25484:TimeParser.cs(184,20): warning CS8629: Nullable value type may be null. [C:\src\hagen\hagen.plugin.office\hagen.plugin.office.csproj]
25484:OutlookExtensions.cs(308,28): warning CS8625: Cannot convert null literal to non-nullable reference type. [C:\src\hagen\hagen.plugin.office\hagen.plugin.office.csproj]
25484:OutlookExtensions.cs(315,13): warning CS8602: Dereference of a possibly null reference. [C:\src\hagen\hagen.plugin.office\hagen.plugin.office.csproj]
25484:  hagen.plugin.office -> C:\src\hagen\out\Release\hagen.plugin.office\bin\hagen.plugin.office.dll
25484:  hagen.plugin.office.Test -> C:\src\hagen\out\Release\hagen.plugin.office.Test\bin\hagen.plugin.office.Test.dll
25484:  Copying test files to C:\src\hagen\out\Release\hagen.plugin.office.Test\bin\\..\test-data
25484:  hagen.plugin.screen -> C:\src\hagen\out\Release\hagen.plugin.screen\bin\hagen.plugin.screen.dll
25484:  hagen.plugin.web -> C:\src\hagen\out\Release\hagen.plugin.web\bin\hagen.plugin.web.dll
2")
                .ToList();
            Assert.That(locations[0], Is.EqualTo(new TextLocation(
                @"C:\src\hagen\activityReport\Program.cs",
                11,23,
                @"25484:TextLocation.cs(11,23): warning CS8618: Non-nullable field 'FileName' is uninitialized. Consider declaring the field as nullable. [C:\src\hagen\hagen.plugin.office\hagen.plugin.office.csproj]"
                )));
        }
    }
}
