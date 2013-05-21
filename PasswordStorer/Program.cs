using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text;

namespace PasswordStorer
{
    static class Program
    {
        internal static string saveDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\PasswordStorer\\";
        internal static string saveFile = saveDir + "pwd.enc";
        internal static List<Pwd> passwords = new List<Pwd>();
        internal static string KEY;
        internal static int IDENTIFIER = 0;

        internal static bool RUN = true;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Create the nessecary files/folders
            if (!Directory.Exists(saveDir))
            {
                Directory.CreateDirectory(saveDir);
            }
            if (!File.Exists(saveFile))
            {
                File.WriteAllText(saveFile, "");
            }


            while (Program.RUN)
            {
                PromptPwd pp = new PromptPwd();
                if (pp.ShowDialog() == DialogResult.OK)
                {

                    Program.KEY = pp.key;
                    bool run = true;
                    // Loading data
                    string fdata = File.ReadAllText(Program.saveFile);
                    if (Program.KEY == "" || Program.KEY == null)
                    {
                        fdata = "";
                        run = false;
                    }
                    try
                    {
                        if (fdata != "")
                        {
                            string DATA = Pwd.DecryptStringAES(fdata, Program.KEY);
                            List<Pwd> tmp = Pwd.fromXml(DATA);
                            foreach (Pwd p in tmp)
                            {
                                p.KEY = Program.IDENTIFIER;
                                Program.passwords.Add(p);
                                Program.IDENTIFIER++;
                            }
                            DATA = null;
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Incorrect password!");
                        run = false;
                    }

                    if (run)
                    {
                        Application.Run(new Form1());

                        // Saving data
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine("<DECRYPTED_PASSWORDS>");
                        foreach (Pwd p in passwords)
                        {
                            sb.AppendLine(Pwd.toXml(p));
                        }
                        sb.AppendLine("</DECRYPTED_PASSWORDS>");
                        string EDATA = Pwd.EncryptStringAES(sb.ToString(), Program.KEY);
                        File.WriteAllText(Program.saveFile, EDATA);
                        KEY = null;
                        sb = null;
                        passwords = new List<Pwd>();
                        IDENTIFIER = 0;

                    }
                }
                else
                {
                    Program.RUN = false;
                }
            }
        }
    }
}
