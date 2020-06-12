using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LSBgenerator
{
    public partial class CommandHelpList : Form
    {
        public CommandHelpList(List<string> cmds)
        {
            InitializeComponent();
            cmds.ForEach(s => listBox1.Items.Add(s));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
