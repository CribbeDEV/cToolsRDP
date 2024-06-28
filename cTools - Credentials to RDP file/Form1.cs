using System;

using System.Text;
using System.IO;

using ExpoRDP;
using static ExpoRDP.DPAPI;
using System.Runtime.InteropServices;

namespace cTools___Credentials_to_RDP_file
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;
            }
        }

        public static string EncryptToRDP(string password)
        {

            string result = string.Empty;

            DATA_BLOB pDataIn = new DATA_BLOB();
            DPAPI.InitBLOB(Encoding.Unicode.GetBytes(password), ref pDataIn);

            DATA_BLOB pDataEncrypted = new DATA_BLOB();

            DATA_BLOB pEntropy = new DATA_BLOB();

            CRYPTPROTECT_PROMPTSTRUCT prompt =
                                       new CRYPTPROTECT_PROMPTSTRUCT();
            DPAPI.InitPrompt(ref prompt);

            DPAPI.CryptProtectData(ref pDataIn, "psw",
                     ref pEntropy, IntPtr.Zero, ref prompt, DPAPI.CRYPTPROTECT_UI_FORBIDDEN, ref pDataEncrypted);

            MemoryStream epwsb = new MemoryStream();

            byte[] pwdBytes = new byte[pDataEncrypted.cbData];
            Marshal.Copy(pDataEncrypted.pbData,
                             pwdBytes,
                             0,
                             pDataEncrypted.cbData);
            result = BitConverter.ToString(pwdBytes).Replace("-", string.Empty);

            return result;
        }

        static void lineChanger(string newText, string fileName, int line_to_edit)
        {
            string[] arrLine = File.ReadAllLines(fileName);
            arrLine[line_to_edit] = newText;
            File.WriteAllLines(fileName, arrLine);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            bool willReturn = false;
            if (textBox2.Text == "")
            {
                errorProvider1.SetError(this.textBox2, "Username required");
                willReturn = true;
            }
            if (textBox3.Text == "")
            {
                errorProvider2.SetError(this.textBox3, "Password required");
                willReturn = true;
            }
            if (textBox1.Text == "" || !File.Exists(textBox1.Text))
            {
                MessageBox.Show("The file provided isn't existing.", "Provide File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                willReturn = true;
            }

            if (willReturn) { return; }

            string fileName = textBox1.Text;
            string[] lines;
            string password = EncryptToRDP(textBox3.Text);
            using (StreamReader streamReader = File.OpenText(fileName))
            {
                string text = streamReader.ReadToEnd();
                lines = text.Split(Environment.NewLine);
            }

            bool usernameFound = false;
            bool passwordFound = false;

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("username"))
                {
                    usernameFound = true;
                    lineChanger($"username:s:EMEA\\{textBox2.Text}", fileName, i);
                } else if (lines[i].Contains("password"))
                {
                    passwordFound = true;
                    lineChanger($"password 51:b:{password}", fileName, i);
                }
            }

            if (!usernameFound)
            {
                File.AppendAllText(fileName, Environment.NewLine + Environment.NewLine + $"username:s:EMEA\\{textBox2.Text}");
            } 
            if (!passwordFound)
            {
                File.AppendAllText(fileName, Environment.NewLine + $"password 51:b:{password}");
            }

            DialogResult result = MessageBox.Show("Successfully added/changed the credentials", "Success");
            this.Close();
        }
    }
}
