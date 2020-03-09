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

using NUnit.Framework;

namespace hagen.Plugin.Db
{
    [TestFixture]
    public class MarkdownLinkTest
    {
        [Test]
        public void Test()
        {
            var text = @"[Google](http://www.google.com)";
            var markdownLink = MarkdownLink.Parse(text);
            Assert.That(markdownLink != null);
            Assert.That(markdownLink.Href, Is.EqualTo("http://www.google.com"));
            Assert.That(markdownLink.Title, Is.EqualTo("Google"));
        }

        [Test]
        public void Test2()
        {
            var text = @"[Chp Program Board](https://teams.microsoft.com/l/entity/com.microsoft.teamspace.tab.planner/_djb2_msteams_prefix_209840166?context=%7B%22subEntityId%22%3Anull%2C%22channelId%22%3A%2219%3A29f088ae953743d08b9197c30c09bab7%40thread.skype%22%7D&groupId=f0ed42fe-c7b4-4d43-8015-078f7e7f24f3&tenantId=5dbf1add-202a-4b8d-815b-bf0fb024e033)";
            var markdownLink = MarkdownLink.Parse(text);
            Assert.That(markdownLink != null);
            Assert.That(markdownLink.Href, Is.EqualTo("https://teams.microsoft.com/l/entity/com.microsoft.teamspace.tab.planner/_djb2_msteams_prefix_209840166?context=%7B%22subEntityId%22%3Anull%2C%22channelId%22%3A%2219%3A29f088ae953743d08b9197c30c09bab7%40thread.skype%22%7D&groupId=f0ed42fe-c7b4-4d43-8015-078f7e7f24f3&tenantId=5dbf1add-202a-4b8d-815b-bf0fb024e033"));
            Assert.That(markdownLink.Title, Is.EqualTo("Chp Program Board"));
        }
    }
}