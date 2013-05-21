using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PasswordStorer
{
    public partial class Form1 : Form
    {
        bool edit = false;

        public Form1()
        {
            Program.RUN = false;
            InitializeComponent();
            updateList();
        }

        void updateList()
        {
            dataGridView1.Rows.Clear();
            foreach (Pwd p in Program.passwords)
            {
                dataGridView1.Rows.Add(
                    p.nameSpace, 
                    p.datetime.ToString("dd-MM-yyyy"), 
                    p.category, 
                    p.username, 
                    p.email,
                    p.KEY
                    );
            }
        }

        void updateList(List<Pwd> pwd)
        {
            dataGridView1.Rows.Clear();
            foreach (Pwd p in pwd)
            {
                dataGridView1.Rows.Add(
                    p.nameSpace, 
                    p.datetime.ToString("dd-MM-yyyy"), 
                    p.category, 
                    p.username, 
                    p.email,
                    p.KEY
                    );
            }
        }

        // Save a new or edited entry
        private void button1_Click(object sender, EventArgs e)
        {
            if (edit)
            {
                int key = Convert.ToInt32(dataGridView1.Rows[dataGridView1.SelectedCells[0].RowIndex].Cells[5].Value);
                Program.passwords.Find(Pwd => Pwd.KEY == key).nameSpace = textBox_name.Text;
                Program.passwords.Find(Pwd => Pwd.KEY == key).datetime = dateTimePicker_date.Value;
                Program.passwords.Find(Pwd => Pwd.KEY == key).category = textBox_category.Text;
                Program.passwords.Find(Pwd => Pwd.KEY == key).username = textBox_username.Text;
                Program.passwords.Find(Pwd => Pwd.KEY == key).email = textBox_email.Text;
            }
            else
            {
                Pwd pwd = new Pwd(textBox_name.Text, textBox_category.Text, dateTimePicker_date.Value, textBox_username.Text, textBox_email.Text, textBox_password.Text, textBox_notes.Text);
                pwd.KEY = Program.IDENTIFIER;
                Program.IDENTIFIER++;
                Program.passwords.Add(pwd);
                updateList();
            }
        }

        // Delete an entry
        private void button2_Click(object sender, EventArgs e)
        {
            if (edit)
            {
                int rindex = dataGridView1.SelectedCells[0].RowIndex;
                foreach (Pwd p in Program.passwords)
                {
                    if (p.KEY == Convert.ToInt32(dataGridView1.Rows[rindex].Cells[5].Value))
                    {
                        Program.passwords.Remove(p);
                        break;
                    }
                }
            }
            updateList();
            button3_Click(null, null);
            edit = false;
            groupBox2.Text = "Content";
        }

        // Clear all fields to make a new entry
        private void button3_Click(object sender, EventArgs e)
        {
            edit = false;
            groupBox2.Text = "Content";
            textBox_name.Text = "";
            textBox_category.Text = "";
            textBox_username.Text = "";
            textBox_password.Text = "";
            textBox_email.Text = "";
            textBox_notes.Text = "";
            dataGridView1.ClearSelection();
        }

        // Print out the data from the selected entry
        private void dataGridView1_CellContentClick(object sender, EventArgs e)
        {
            try
            {
                edit = true;
                groupBox2.Text = "Content - Editing";
                foreach (Pwd p in Program.passwords)
                {
                    int rindex = dataGridView1.SelectedCells[0].RowIndex;
                    if (p.KEY == Convert.ToInt32(dataGridView1.Rows[rindex].Cells[5].Value))
                    {
                        textBox_name.Text = p.nameSpace;
                        textBox_category.Text = p.category;
                        dateTimePicker_date.Value = p.datetime;
                        textBox_username.Text = p.username;
                        textBox_password.Text = p.password;
                        textBox_email.Text = p.email;
                        textBox_notes.Text = p.notes.Replace("\n", Environment.NewLine);
                    }

                }
                dataGridView1.Rows[dataGridView1.SelectedCells[0].RowIndex].Selected = true;
            }
            catch
            {
                edit = false;
                groupBox2.Text = "Content";
            }
        }

        // Filter the entries to the text
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            List<Pwd> pwds = new List<Pwd>();
            foreach (Pwd p in Program.passwords)
            {
                if (compareString(p.nameSpace,textBox1.Text) ||
                    compareString(p.username,textBox1.Text) ||
                    compareString(p.email,textBox1.Text) ||
                    compareString(p.category,textBox1.Text)
                    )
                {
                    pwds.Add(p);
                }
            }
            updateList(pwds);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            PromptPwd pp = new PromptPwd();
            if (pp.ShowDialog() == DialogResult.OK)
            {
                if (pp.key != "" && pp.key != null)
                {
                    Program.KEY = pp.key;
                }
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                textBox_password.UseSystemPasswordChar = false;
            }
            else
            {
                textBox_password.UseSystemPasswordChar = true;
            }
        }

        private bool compareString(string text, string input)
        {
            CultureInfo culture = CultureInfo.CurrentCulture;
            return text.ToLower(culture).Contains(input.ToLower(culture));
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Program.RUN = true;
            this.Close();
        }
    }
}
