using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShadowTH_Text_Editor
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void buttonOpen_Click(object sender, EventArgs e)
        {
            /*
                .fnt file layout
                Everything is little endian.
                ================
                Header: length 0x4. Number of entries.

                Entry info:
                0x00: Text line internal address - Modify based on length of replacement text
                0x04: External address? - Retain original value
                0x08: Text type - Retain original value
                0x0C: Subtitle active time - Retain original value
                0x10: Subtitle ID - Retain original value
             */
        }
    }
}
