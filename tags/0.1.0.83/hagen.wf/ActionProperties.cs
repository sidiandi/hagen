using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace hagen.wf
{
    public partial class ActionProperties : Form
    {
        public ActionProperties()
        {
            InitializeComponent();
        }

        public object EditedObject
        {
            get
            {
                return propertyGrid.SelectedObject;
            }

            set
            {
                propertyGrid.SelectedObject = value;
            }
        }
    }
}
