using NUnit.Framework;

namespace hagen.plugin.office.Tests
{
    [TestFixture]
    public class MarkdownMeetingMinutesTests
    {
        [Test, Explicit]
        public void Run()
        {
            var m = new MarkdownMeetingMinutes();
            m.CreateMeetingMinutes();
        }
    }
}
