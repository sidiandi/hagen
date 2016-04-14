using CredentialManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CredentialManagementTestProgram
{
    class Program
    {
        static void Main(string[] args)
        {
            var cp = new CredentialProvider("test");
            var c = cp.GetCredential();
            var c1 = cp.GetCredential();
        }
    }
}
