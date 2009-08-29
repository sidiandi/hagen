using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Sidi.Persistence;
using Sidi.Collections;
using Sidi.Util;

namespace hagen.wf
{
    public partial class SearchBox : UserControl
    {
        Sidi.Forms.ItemView<Action> itemView;

        Collection<Action> data;

        public Collection<Action> Data
        {
            set
            {
                data = value;
                asyncQuery = new AsyncQuery(data);
                asyncQuery.Complete += new EventHandler(asyncQuery_Complete);
            }
            get
            {
                return data;
            }
        }

        void asyncQuery_Complete(object sender, EventArgs e)
        {
            this.Invoke(new Action<IList<Action>>(x =>
                {
                    itemView.List = x;
                    SelectItem(0);
                }), asyncQuery.Result);
        }

        void SelectItem(int index)
        {
            itemView.Selection = new IntSet(new Interval(index, index+1));
            itemView.FocusedItemIndex = index;
        }

        AsyncQuery asyncQuery;
        
        public SearchBox()
        {
            itemView = new Sidi.Forms.ItemView<Action>();
            itemView.Dock = DockStyle.Fill;
            itemView.TabStop = false;
            itemView.ItemLayout = new Sidi.Forms.ItemLayoutRows(20);

            this.Controls.Add(itemView);

            InitializeComponent();

            itemView.ItemsActivated += new EventHandler(itemView_ItemsActivated);
            itemView.GotFocus += new EventHandler(itemView_GotFocus);

            textBoxQuery.KeyDown += new KeyEventHandler(textBoxQuery_KeyDown);

            textBoxQuery.TextChanged += new EventHandler(textBoxQuery_TextChanged);
        }

        void itemView_GotFocus(object sender, EventArgs e)
        {
            textBoxQuery.Focus();
        }

        public void Start()
        {
            textBoxQuery.SelectAll();
        }

        public event EventHandler ItemsActivated;

        void itemView_ItemsActivated(object sender, EventArgs e)
        {
            OnItemsActivated();
        }

        void OnItemsActivated()
        {
            if (ItemsActivated != null)
            {
                ItemsActivated(this, EventArgs.Empty);
            }
        }

        public IEnumerable<Action> SelectedActions
        {
            get
            {
                return itemView.SelectionEnumerator;
            }
        }

        void textBoxQuery_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Down:
                    SelectItem(itemView.FocusedItemIndex+1);
                    e.Handled = true;
                    break;
                case Keys.Up:
                    SelectItem(itemView.FocusedItemIndex-1);
                    e.Handled = true;
                    break;
                case Keys.Enter:
                    OnItemsActivated();
                    e.Handled = true;
                    break;
            }
        }

        void textBoxQuery_TextChanged(object sender, EventArgs e)
        {
            asyncQuery.Query = textBoxQuery.Text;
        }
    }
}
