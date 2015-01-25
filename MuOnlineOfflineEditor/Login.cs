using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MuOnlineOfflineEditor
{
    public partial class Login : Form
    {
        string IP = "";
        string port = "";
        string account = "";
        string password = "";
        bool autoLogin = false;

        public Login()
        {
            InitializeComponent();
        }

        private void Login_Load(object sender, EventArgs e)
        {
            string line;
            StreamReader config = new StreamReader(Path.Combine(Directory.GetParent(Assembly.GetEntryAssembly().Location).FullName, "MOEConfig.ini"));
            while ((line = config.ReadLine()) != null)
            {
                if (line.Contains('=') && line.Contains(';') && line.Substring(line.IndexOf('='), line.IndexOf(';')).Length > 0)
                {
                    if (line.Substring(0, line.IndexOf('=')) == "Server")
                    {
                        IP = line.Substring(line.IndexOf('=') + 1, line.IndexOf(';') - line.IndexOf('=') - 1);
                    }
                    else if (line.Substring(0, line.IndexOf('=')) == "ConnectionPort")
                    {
                        port = line.Substring(line.IndexOf('=') + 1, line.IndexOf(';') - line.IndexOf('=') - 1);
                    }
                    else if (line.Substring(0, line.IndexOf('=')) == "AutoLogin")
                    {
                        autoLogin = Convert.ToBoolean(Convert.ToInt32(line.Substring(line.IndexOf('=') + 1, line.IndexOf(';') - line.IndexOf('=') - 1)));
                    }
                    else if (line.Substring(0, line.IndexOf('=') - line.IndexOf('=')) == "Username")
                    {
                        account = line.Substring(line.IndexOf('=') + 1, line.IndexOf(';') - line.IndexOf('=') - 1);
                    }
                    else if (line.Substring(0, line.IndexOf('=')) == "Password")
                    {
                        password = line.Substring(line.IndexOf('=') + 1, line.IndexOf(';') - line.IndexOf('=') - 1);
                    }
                }
            }
        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            if (CheckLogin(textBox1.Text, textBox2.Text))
            {
                account = textBox1.Text;
                Thread tr = new Thread(new ThreadStart(StartApp));
                tr.SetApartmentState(ApartmentState.STA);
                tr.Start();
                Close();
            }
            else
            {
                MessageBox.Show("Wrong account or password !");
            }
        }

        private void StartApp()
        {
            MainForm mf = new MainForm(IP, 11000, account);
            Application.Run(mf);
        }

        private bool CheckLogin(string user, string pass)
        {
            string response = "";
            ServerConnection sc = new ServerConnection(IP, "test");
            response = sc.SendToServer("<logincheck>" + user + ":" + pass);
            if (response.ToLower().Contains("<error>"))
            {
                MessageBox.Show("Server is offline !");
                return false;
            }

            if (response.ToLower() == "true")
            {
                return true;
            }
            return false;
        }
    }
}
