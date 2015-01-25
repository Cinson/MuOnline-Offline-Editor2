using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ParseItemKor
{
    public partial class Form1 : Form
    {
        string[] itemKor = null;
        Hashtable hs = new Hashtable();

        public Form1()
        {
            InitializeComponent();
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                itemKor = File.ReadAllLines(fd.FileName);
            }

            DecodeItemKor();
        }

        private void DecodeItemKor()
        {
            dataGridView1.Rows.Clear();
            int i = 0;
            int type = 0;
            foreach (string item in itemKor)
            {
                string repl = item.Replace("\t", ";");
                int itemNum = -1;
                string itemName = "";
                int itemX = 0;
                int itemY = 0;
                if (item.Length < 3 && !string.IsNullOrEmpty(item) && int.TryParse(item, out type))
                {
                    type = Convert.ToInt32(item);
                }

                if (item.Contains("\t") && (item.IndexOf("\t") == 1 || repl.IndexOf("\t") == 2))
                {

                    itemNum = Convert.ToInt32(item.Substring(0, item.IndexOf("\t")));
                    itemName = item.Split('\"')[1].Split('\"')[0];

                    itemX = Convert.ToInt32(repl.Split(';')[3]);
                    itemY = Convert.ToInt32(repl.Split(';')[4]);
                }
                else if (item.Contains("\t") && item.IndexOf("\t") == 0)
                {
                    itemNum = Convert.ToInt32(item.Substring(1).Substring(0, item.Substring(1).IndexOf("\t")));
                    itemName = item.Split('\"')[1].Split('\"')[0];

                    itemX = Convert.ToInt32(repl.Split(';')[4]);
                    itemY = Convert.ToInt32(repl.Split(';')[5]);
                }

                if (itemNum < 0)
                {
                    continue;
                }

                int ex_type = -1;

                if (type < 6)
                {
                    ex_type = 0;
                }
                else if (type > 5 && type < 12)
                {
                    ex_type = 1;
                }
                else if (type == 12)
                {
                    ex_type = 4;
                }

                dataGridView1.Rows.Add();
                dataGridView1.Rows[i].Cells[0].Value = type;
                dataGridView1.Rows[i].Cells[1].Value = itemNum;
                dataGridView1.Rows[i].Cells[2].Value = itemName;
                dataGridView1.Rows[i].Cells[3].Value = ex_type;
                dataGridView1.Rows[i].Cells[4].Value = itemX;
                dataGridView1.Rows[i].Cells[5].Value = itemY;
                hs.Add(type.ToString() + ";" + itemNum.ToString(), itemName + ";" + ex_type + ";" + itemX + ";" + itemY);
                i++;
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fd = new FolderBrowserDialog();

            if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                BinaryFormatter bfw = new BinaryFormatter();
                FileStream filee = new FileStream(Path.Combine(fd.SelectedPath, "ItemKor.hs"), FileMode.Create);
                StreamWriter ws = new StreamWriter(filee);
                bfw.Serialize(ws.BaseStream, hs);
                filee.Close();
            }
        }
    }
}
