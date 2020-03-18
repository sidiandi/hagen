using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace hagen
{
    class ExtractTextLocations : EnumerableActionSource
    {
        protected override IEnumerable<IResult> GetResults(IQuery query)
        {
            return TextLocation.Find(query.Context.ClipboardText)
                .Select(_ => ToAction(_, query.Context))
                .Select(_ => _.ToResult());
        }

        static IAction ToAction(TextLocation textLocation, IContext context) => new TextLocationAction(textLocation, context.GetService<IFileIconProvider>());
    }
}
