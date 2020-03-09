using hagen.plugin.healthineers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hagen.Test
{
    [TestFixture]
    public class ScdTests
    {
        [Test]
        public void FindPerson()
        {
            var scd = new Scd();
            var persons = scd.FindPersons("Andreas Grimme");
            Assert.That(persons.Count(), Is.EqualTo(1));
        }

        [Test]
        public void FindPersonByMail()
        {
            var scd = new Scd();
            var persons = scd.FindPersons("andreas.grimme@siemens-healthineers.com");
            Assert.That(persons.Count(), Is.EqualTo(1));
        }

        [Test]
        public void IsMail()
        {
            Assert.That(Scd.IsMail("andreas.grimme@siemens-healthineers.com"));
            Assert.That(!Scd.IsMail("andreas.grimme"));
        }

        [Test]
        public void FindMails()
        {
            var mails = Scd.FindMails("Fassbinder, Peter (IOT C OC-2 EU-JK) <peter.fassbinder@siemens.com>");
            Assert.That(mails.Count(), Is.EqualTo(1));
            Assert.That(mails.First(), Is.EqualTo("peter.fassbinder@siemens.com"));
        }
    }
}
