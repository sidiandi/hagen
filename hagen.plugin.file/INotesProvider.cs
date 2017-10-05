// Copyright (c) 2016, Andreas Grimme

using System.Collections.Generic;

namespace hagen
{
    internal interface INotesProvider
    {
        IEnumerable<Note> GetNotes();
    }
}
