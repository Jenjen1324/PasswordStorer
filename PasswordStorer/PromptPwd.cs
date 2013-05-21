using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PasswordStorer
{
    public partial class PromptPwd : Form
    {
        public PromptPwd()
        {
            InitializeComponent();
        }

        public string key
        {
            get { return textBox1.Text; }
        }
    }
}
