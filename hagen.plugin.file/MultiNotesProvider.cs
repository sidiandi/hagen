using System;
using System.Collections.Generic;

namespace hagen
{
    internal class MultiNotesProvider : INotesProvider
    {
        readonly IEnumerable<INotesProvider> providers;

        public MultiNotesProvider(IEnumerable<INotesProvider> providers)
        {
            this.providers = providers;
        }

        public IEnumerable<Note> GetNotes()
        {
            return providers.SafeSelectMany(_ => _.GetNotes());
        }
    }
}