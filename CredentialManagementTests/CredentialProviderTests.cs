using NUnit.Framework;
using CredentialManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CredentialManagement.Tests
{
    [TestFixture, Apartment(System.Threading.ApartmentState.STA)]
    public class CredentialProviderTests
    {
        [Test]
        public void GetCredentialTest()
        {
            var cp = new CredentialProvider("test");
            cp.Reset();
            var c = cp.GetCredential();
            var c1 = cp.GetCredential();
            Assert.AreEqual(c.UserName, c1.UserName);
            Assert.AreEqual(c.SecurePassword, c1.SecurePassword);
        }
    }
}