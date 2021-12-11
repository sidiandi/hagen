using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hagen
{
    [TestFixture]
    public class GraphUtilTests
    {
        [Test]
        public async Task Token()
        {
            var token = await GraphUtil.GetTokenFromGraphExplorer();
        }
    }
}