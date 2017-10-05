// Copyright (c) 2016, Andreas Grimme

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sidi.Extensions;
using hagen;
using System.Windows.Forms;
using System.IO;

namespace hagen
{
    class NoteAction : IAction
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        Note note;
        ILastExecutedStore lastExecutedStore;

        public NoteAction(Note note, ILastExecutedStore lastExecutedStore)
        {
            this.lastExecutedStore = lastExecutedStore;
            this.note = note;
        }

        public System.Drawing.Icon Icon
        {
            get
            {
                return null;
            }
        }

        public string Id
        {
            get
            {
                return note.Name;
            }
        }

        public DateTime LastExecuted
        {
            get
            {
                return lastExecutedStore.Get(Id);
            }
        }

        public string Name
        {
            get
            {
                return note.Name + ": " + SingleLine(note.Content);
            }
        }

        public void Execute()
        {
            lastExecutedStore.Set(Id);
            var text = note.Content;
            Clipboard.SetText(text);
            SendKeys.Send("+{INS}");
        }

        static string SingleLine(string x)
        {
            using (var r = new StringReader(x))
            {
                return EnumerableExtensions.UntilNull(() => r.ReadLine()).Join(" - ");
            }
        }
    }
}
