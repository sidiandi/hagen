using hagen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hagen.ActionSource
{
    /*
    public class Test_AlwaysCrash : IActionSource
    {
        public IEnumerable<IAction> GetActions(string query)
        {
            throw new NotImplementedException();
        }
    }

    public class Test_AlwaysCrashOnEnum : IActionSource
    {
        public IEnumerable<IAction> GetActions(string query)
        {
            foreach (var i in Enumerable.Range(0,3))
            {
                yield return new SimpleAction(i.ToString(), () => {});
            }
            throw new NotImplementedException();
        }
    }
     */
}
