using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MOE_Server
{
    public partial class Form1 : Form
    {
        string encryptionKey = "test";
        int hexLenght = 20;
        public Form1()
        {
            InitializeComponent();
            backgroundWorkerSocket.RunWorkerAsync();
        }

        private void backgroundWorkerSocket_DoWork(object sender, DoWorkEventArgs e)
        {
            // Data buffer for incoming data.
            byte[] bytes = new Byte[4400];

            IPAddress ipAddress = IPAddress.Parse("192.168.1.50");
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

            Socket listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            Crypto cr = new Crypto();
            listener.Bind(localEndPoint);
            listener.Listen(10);

            // Start listening for connections.
            while (true)
            {
                try
                {
                    Socket handler = listener.Accept();
                    string data = null;

                    bytes = new byte[4400];
                    int bytesRec = handler.Receive(bytes);
                    data = cr.DeCrypt(Encoding.ASCII.GetString(bytes, 0, bytesRec), encryptionKey);

                    if (data.Contains("<arangevault>"))

                        richTextBox1.Invoke((MethodInvoker)delegate { richTextBox1.Text = richTextBox1.Text + Environment.NewLine + "Vault arrange : " + data.Split(':')[1]; });
                    else
                        richTextBox1.Invoke((MethodInvoker)delegate { richTextBox1.Text = richTextBox1.Text + Environment.NewLine + data; });

                    data = ResolveMessage(data);

                    byte[] msg = Encoding.ASCII.GetBytes(cr.Crypt(data, encryptionKey));

                    handler.Send(msg);
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
                catch (Exception ex)
                {
                    richTextBox1.Invoke((MethodInvoker)delegate { richTextBox1.Text += "Error : " + ex.Message; });
                }
            }
        }

        private string ResolveMessage(string msg)
        {
            if (msg.ToLower().Contains("<retvault>"))
            {
                msg = msg.Replace("<retvault>", "");
                msg = GetUserVault(msg);
            }
            else if (msg.ToLower().Contains("logincheck"))
            {
                msg = msg.Replace("<logincheck>", "");
                if (LoginCheck(msg.Split(':')[0], msg.Split(':')[1]))
                    msg = "true";
                else
                    msg = "false";
            }
            else if (msg.ToLower().Contains("<arangevault>"))
            {
                msg = msg.Replace("<arangevault>", "");
                if (VaultCheckAndArrange(msg.Split(':')[0], msg.Split(':')[1]))
                    msg = "true";
                else
                    msg = "false";
            }
            else if (msg.ToLower().Contains("<getchars>"))
            {
                msg = msg.Replace("<getchars>", "");
                msg = GetChars(msg);
            }
            else if (msg.ToLower().Contains("<getcharhex>"))
            {
                msg = msg.Replace("<getcharhex>", "");
                msg = GetCharHex(msg.Split(':')[0], msg.Split(':')[1]);
            }
            return msg;
        }

        private string GetUserVault(string account)
        {
            string vaultHex = "";
            DataTable tbl = new DataTable();
            using (SqlConnection conn = new SqlConnection("Server=.;Database=MuOnline2;User Id=sa;Password=trash7ar4321;"))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"declare @it varbinary(1200); 
set @it=(select [Items] from [warehouse] where [AccountID]='" + account + @"'); 
select @it", conn);
                SqlDataReader reader = cmd.ExecuteReader();
                tbl.Load(reader);
                conn.Close();
            }
            if (tbl.Rows.Count > 0)
            {
                byte[] ba = (byte[])tbl.Rows[0][0];
                string hex = BitConverter.ToString(ba);
                hex = hex.Replace("-", "");
                vaultHex = hex;
            }
            tbl.Dispose();
            return vaultHex;
        }

        private bool LoginCheck(string acc, string pass)
        {
            DataTable tbl = new DataTable();
            using (SqlConnection conn = new SqlConnection("Server=.;Database=MuOnline2;User Id=sa;Password=trash7ar4321;"))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("select memb___id from MEMB_INFO where memb___id='" + acc + "' and memb__pwd='" + pass + "'", conn);
                SqlDataReader reader = cmd.ExecuteReader();
                tbl.Load(reader);
                conn.Close();
            }

            if (tbl.Rows.Count > 0 && tbl.Rows[0][0].ToString() == acc)
            {
                tbl.Dispose();
                return true;
            }
            tbl.Dispose();
            return false;
        }

        private bool VaultCheckAndArrange(string hex, string account)
        {
            string vaultHex = "";
            DataTable tbl = new DataTable();
            using (SqlConnection conn = new SqlConnection("Server=.;Database=MuOnline2;User Id=sa;Password=trash7ar4321;"))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"declare @it varbinary(1200); 
set @it=(select [Items] from [warehouse] where [AccountID]='" + account + @"'); 
select @it", conn);
                SqlDataReader reader = cmd.ExecuteReader();
                tbl.Load(reader);
                conn.Close();
            }
            if (tbl.Rows.Count > 0)
            {
                byte[] ba = (byte[])tbl.Rows[0][0];
                string oldHex = BitConverter.ToString(ba);
                oldHex = oldHex.Replace("-", "");
                vaultHex = oldHex;
            }
            tbl.Dispose();

            List<string> itemsInVault = new List<string>();
            string nullHex = "";
            for (int i = 0; i < hexLenght; i++)
            {
                nullHex += "F";
            }

            int index = 0;
            while (index + hexLenght <= vaultHex.Length)
            {
                string item = vaultHex.Substring(index, hexLenght);
                if (item != nullHex)
                {
                    itemsInVault.Add(item);
                }
                index += hexLenght;
            }

            List<string> newItems = new List<string>();
            index = 0;
            while (index + hexLenght <= hex.Length)
            {
                string item = hex.Substring(index, hexLenght);
                if (item != nullHex)
                {
                    newItems.Add(item);
                }
                index += hexLenght;
            }

            if (newItems.Count == 0)
            {
                return false;
            }

            foreach (string item in newItems)
            {
                if (!itemsInVault.Contains(item))
                {
                    return false;
                }
            }

            using (SqlConnection conn = new SqlConnection("Server=.;Database=MuOnline2;User Id=sa;Password=trash7ar4321;"))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"update Warehouse set Items=0x" + hex + " where AccountId='" + account + "'", conn);
                cmd.ExecuteNonQuery();
                conn.Close();
            }

            return true;
        }

        private string GetChars(string acc)
        {
            string chars = "";
            DataTable tbl = new DataTable();
            using (SqlConnection conn = new SqlConnection("Server=.;Database=MuOnline2;User Id=sa;Password=trash7ar4321;"))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"select Name from Character where AccountId='" + acc + "'", conn);
                SqlDataReader reader = cmd.ExecuteReader();
                tbl.Load(reader);
                conn.Close();
            }
            if (tbl.Rows.Count > 0)
            {
                for (int i = 0; i < tbl.Rows.Count; i++)
                {
                    if (i == tbl.Rows.Count - 1)
                    {
                        chars += tbl.Rows[i][0].ToString();
                    }
                    else
                    {
                        chars += tbl.Rows[i][0].ToString() + ";";
                    }
                }
            }
            tbl.Dispose();
            return chars;
        }

        private string GetCharHex(string acc, string character)
        {

            string charHex = "";
            DataTable tbl = new DataTable();
            using (SqlConnection conn = new SqlConnection("Server=.;Database=MuOnline2;User Id=sa;Password=trash7ar4321;"))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"declare @it varbinary(1200); 
set @it=(select [Inventory] from [Character] where [AccountID]='" + acc + "' and Name='" + character + @"'); 
select @it", conn);
                SqlDataReader reader = cmd.ExecuteReader();
                tbl.Load(reader);
                conn.Close();
            }
            if (tbl.Rows.Count > 0)
            {
                byte[] ba = (byte[])tbl.Rows[0][0];
                string hex = BitConverter.ToString(ba);
                hex = hex.Replace("-", "");
                charHex = hex;
            }
            tbl.Dispose();
            return charHex;
        }

        private byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
    }
}
