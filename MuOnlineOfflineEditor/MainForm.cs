using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MuOnlineOfflineEditor
{
    public partial class MainForm : Form
    {
        //Vault with Char : Width=611
        //Vault without Char : Width=300
        Hashtable hs = new Hashtable();
        int hexLenght = 20;
        Hashtable infos = new Hashtable();
        Hashtable charInfos = new Hashtable();
        Control selectedPic = new Control();
        bool isMovingItem = false;
        Point oldLocation = new Point();
        List<string> linesList = new List<string>();
        List<string> charLinesList = new List<string>();
        string encriptionKey = "test";
        string IP = "";
        string myAccount = "";
        string vaultHex = "";
        string charHex = "";

        public MainForm(string Ip, int port, string account)
        {
            InitializeComponent();
            pbCharView.Visible = false;
            pbVaultChar.Width = 300;
            IP = Ip;
            myAccount = account;

            pbFH1.Visible = false;
            pbFH1.Parent = pbCharView;
            pbFH1.BackColor = Color.Transparent;
            pbSH2.Visible = false;
            pbSH2.Parent = pbCharView;
            pbSH2.BackColor = Color.Transparent;
            pbHelm3.Visible = false;
            pbHelm3.Parent = pbCharView;
            pbHelm3.BackColor = Color.Transparent;
            pbArmor4.Visible = false;
            pbArmor4.Parent = pbCharView;
            pbArmor4.BackColor = Color.Transparent;
            pbPants5.Visible = false;
            pbPants5.Parent = pbCharView;
            pbPants5.BackColor = Color.Transparent;
            pbGloves6.Visible = false;
            pbGloves6.Parent = pbCharView;
            pbGloves6.BackColor = Color.Transparent;
            pbBoots7.Visible = false;
            pbBoots7.Parent = pbCharView;
            pbBoots7.BackColor = Color.Transparent;
            pbWings8.Visible = false;
            pbWings8.Parent = pbCharView;
            pbWings8.BackColor = Color.Transparent;
            pbPet9.Visible = false;
            pbPet9.Parent = pbCharView;
            pbPet9.BackColor = Color.Transparent;
            pbPedant10.Visible = false;
            pbPedant10.Parent = pbCharView;
            pbPedant10.BackColor = Color.Transparent;
            pbLeftRing11.Visible = false;
            pbLeftRing11.Parent = pbCharView;
            pbLeftRing11.BackColor = Color.Transparent;
            pbRightRing12.Visible = false;
            pbRightRing12.Parent = pbCharView;
            pbRightRing12.BackColor = Color.Transparent;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                FileStream fileOld = new FileStream("ItemKor.hs", FileMode.Open);
                StreamReader readMap = new StreamReader(fileOld);
                BinaryFormatter bf = new BinaryFormatter();
                hs = (Hashtable)bf.Deserialize(readMap.BaseStream);
                readMap.Close();
                fileOld.Close();
            }
            catch { MessageBox.Show("ItemKor.hs is missing !", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            ServerConnection sc = new ServerConnection(IP, encriptionKey);
            vaultHex = sc.SendToServer("<retvault>" + myAccount);
            LoadVault(vaultHex);
            string chars = sc.SendToServer("<getchars>" + myAccount);
            if (chars.Length > 0)
            {
                foreach (string character in chars.Split(';'))
                {
                    comboBoxChars.Items.Add(character);
                }
            }
        }

        private void LoadVault(string vaultHex)
        {
            infos.Clear();
            pictureBox1.Controls.Clear();
            string user_items = vaultHex;

            //List<string> linesList = new List<string>();

            int index = 0;
            while (index + hexLenght <= user_items.Length)
            {
                linesList.Add(user_items.Substring(index, hexLenght));
                index += hexLenght;
            }

            string nullHex = "";
            for (int i = 0; i < hexLenght; i++)
            {
                nullHex += "F";
            }

            if (linesList.Count < 120)
            {
                while (linesList.Count < 120)
                {
                    linesList.Add(nullHex);
                }
            }

            int line = 1;
            int xx = 0;
            for (int i = 0; i < linesList.Count; i++)
            {
                xx++;
                if (xx == 9)
                {
                    xx = 1;
                    line++;
                }

                if (linesList[i] == nullHex)
                {
                    continue;
                }

                List<string> itemInfo = ItemInfo(linesList[i]);
                if (itemInfo.Count > 0)
                {
                    infos.Add(itemInfo[3], itemInfo);
                    Bitmap b = ItemImage(Convert.ToInt32(itemInfo[1]), Convert.ToInt32(itemInfo[2]), Convert.ToInt32(itemInfo[8]), 0);
                    PictureBox pb = new PictureBox();
                    pb.Parent = pictureBox1;
                    pb.BackColor = Color.Transparent;
                    pb.Location = new Point(32 * (xx - 1), 32 * (line - 1));
                    pb.Size = new System.Drawing.Size(32 * Convert.ToInt32(itemInfo[6]), 32 * Convert.ToInt32(itemInfo[7]));
                    pb.Image = b;
                    pb.BackColor = Color.FromArgb(25, 255, 255, 255);
                    PictureBox pbText = new PictureBox();
                    pb.Name = itemInfo[3];
                    pb.MouseEnter += (s, eArgs) =>
                    {
                        if (!isMovingItem)
                        {
                            string picText = "";
                            for (int j = 0; j < itemInfo.Count; j++)
                            {
                                picText += itemInfo[j] + Environment.NewLine;
                            }
                            Size size = TextRenderer.MeasureText(picText, pbText.Font);
                            pbText.Width = size.Width;
                            pbText.Height = size.Height;

                            Point mouseLocation = this.PointToClient(Cursor.Position);
                            if (this.Width - mouseLocation.X < size.Width)
                            {
                                mouseLocation.X = mouseLocation.X - size.Width;
                            }
                            if (this.Height - mouseLocation.Y < size.Height)
                            {
                                mouseLocation.Y = mouseLocation.Y - size.Height;
                            }
                            pbText.BackColor = Color.FromArgb(200, Color.Black);
                            pbText.Paint += (object sss, PaintEventArgs argss) =>
                            {
                                using (Font myFont = new Font("Arial", 9))
                                {
                                    argss.Graphics.DrawString(picText, myFont, Brushes.White, new Point(2, 2));

                                }
                            };
                            pbText.Parent = pictureBox1;
                            pbText.Location = mouseLocation;

                            this.Controls.Add(pbText);
                            this.Controls[this.Controls.Count - 1].BringToFront();
                        }
                    };

                    pb.MouseLeave += (s, eArgs) =>
                    {
                        this.Controls.Remove(pbText);
                    };

                    pb.MouseClick += (object send, MouseEventArgs ahah) =>
                    {
                        if (ahah.Button == System.Windows.Forms.MouseButtons.Left && !isMovingItem)
                        {
                            Control control = (Control)pb;
                            if (!isMovingItem)
                            {
                                oldLocation = control.Location;
                            }
                            if (infos.ContainsKey(control.Name))
                            {
                                selectedPic = control;
                                pb.BringToFront();
                                isMovingItem = true;
                            }
                        }
                    };

                    pictureBox1.MouseClick += (object sende, MouseEventArgs ahhh) =>
                    {
                        if (isMovingItem)
                        {
                            isMovingItem = false;
                            Point loc = selectedPic.Location;
                            int cellX = loc.X / 32;
                            int cellY = loc.Y / 32;
                            loc.X = cellX * 32;
                            loc.Y = cellY * 32;

                            int fcX = cellX;
                            int fcy = cellY;
                            int itemWidth = selectedPic.Width / 32;
                            int itemHeight = selectedPic.Height / 32;

                            Point sD = new Point(selectedPic.Location.X / 32, selectedPic.Location.Y / 32);
                            Point sC = new Point(selectedPic.Location.X / 32 + selectedPic.Width / 32, selectedPic.Location.Y / 32);
                            Point sA = new Point(selectedPic.Location.X / 32, selectedPic.Height / 32 + selectedPic.Location.Y / 32);
                            Point sB = new Point(selectedPic.Width / 32 + selectedPic.Location.X / 32, selectedPic.Height / 32 + selectedPic.Location.Y / 32);

                            Rectangle movingItem = new Rectangle(fcX, fcy, itemWidth, itemHeight);
                            Rectangle vaultBounds = new Rectangle(-1, -1, (256 / 32) + 2, (480 / 32) + 2);
                            bool isOnItem = false;
                            foreach (List<string> item in infos.Values)
                            {
                                if (selectedPic.Name != item[3])
                                {
                                    Control control = pictureBox1.Controls[item[3]];
                                    Point D = new Point(control.Location.X / 32, control.Location.Y / 32);
                                    Point C = new Point(control.Location.X / 32 + control.Width / 32, control.Location.Y / 32);
                                    Point A = new Point(control.Location.X / 32, control.Height / 32 + control.Location.Y / 32);
                                    Point B = new Point(control.Width / 32 + control.Location.X / 32, control.Height / 32 + control.Location.Y / 32);
                                    Point R = new Point(control.Width / 64 + control.Location.X / 32, control.Height / 64 + control.Location.Y / 32);
                                    bool isR = true;
                                    if (control.Width == 32)
                                    {
                                        isR = false;
                                    }

                                    if (control.Height >= (32 * 4))
                                    {
                                        Point R1 = new Point(control.Width / 64 + control.Location.X / 32, (control.Height / 64 + control.Location.Y / 32) - 1);
                                        Point R2 = new Point(control.Width / 64 + control.Location.X / 32, (control.Height / 64 + control.Location.Y / 32) + 1);
                                        if (IsOnItem(D, movingItem, false) || IsOnItem(C, movingItem, false) || IsOnItem(A, movingItem, false) || IsOnItem(B, movingItem, false) || IsOnItem(R, movingItem, isR) ||
                                            !IsOnItem(sD, vaultBounds, false) || !IsOnItem(sA, vaultBounds, false) || !IsOnItem(sB, vaultBounds, false) || !IsOnItem(sC, vaultBounds, false) ||
                                            IsOnItem(R1, movingItem, isR) || IsOnItem(R2, movingItem, isR))
                                        {
                                            selectedPic.Location = oldLocation;
                                            isOnItem = true;
                                            break;
                                        }
                                    }
                                    else if (control.Width >= (32 * 4))
                                    {
                                        Point R1 = new Point((control.Width / 64 + control.Location.X / 32) - 1, control.Height / 64 + control.Location.Y / 32);
                                        Point R2 = new Point((control.Width / 64 + control.Location.X / 32) + 1, control.Height / 64 + control.Location.Y / 32);
                                        if (IsOnItem(D, movingItem, false) || IsOnItem(C, movingItem, false) || IsOnItem(A, movingItem, false) || IsOnItem(B, movingItem, false) || IsOnItem(R, movingItem, isR) ||
                                            !IsOnItem(sD, vaultBounds, false) || !IsOnItem(sA, vaultBounds, false) || !IsOnItem(sB, vaultBounds, false) || !IsOnItem(sC, vaultBounds, false) ||
                                            IsOnItem(R1, movingItem, isR) || IsOnItem(R2, movingItem, isR))
                                        {
                                            selectedPic.Location = oldLocation;
                                            isOnItem = true;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        if (IsOnItem(D, movingItem, false) || IsOnItem(C, movingItem, false) || IsOnItem(A, movingItem, false) || IsOnItem(B, movingItem, false) || IsOnItem(R, movingItem, isR) ||
                                            !IsOnItem(sD, vaultBounds, false) || !IsOnItem(sA, vaultBounds, false) || !IsOnItem(sB, vaultBounds, false) || !IsOnItem(sC, vaultBounds, false))
                                        {
                                            selectedPic.Location = oldLocation;
                                            isOnItem = true;
                                            break;
                                        }
                                    }

                                }
                            }

                            if (!isOnItem)
                            {


                                linesList[(loc.X / 32) + ((loc.Y / 32) * 8)] = linesList[(oldLocation.X / 32) + ((oldLocation.Y / 32) * 8)];
                                linesList[(oldLocation.X / 32) + ((oldLocation.Y / 32) * 8)] = nullHex;
                                string tempVaultHex = string.Join("", linesList);

                                if (!VaultHexCheck(tempVaultHex))
                                {
                                    selectedPic.Location = oldLocation;
                                }
                                else
                                {
                                    selectedPic.Location = loc;
                                    oldLocation = loc;
                                    vaultHex = tempVaultHex;
                                }
                            }
                        }
                    };

                    pictureBox1.MouseMove += (object sndr, MouseEventArgs aaa) =>
                    {
                        if (isMovingItem)
                        {

                            Point pt = Cursor.Position;
                            pt.X = pt.X + 2;
                            pt.Y = pt.Y + 2;
                            selectedPic.Location = pictureBox1.PointToClient(pt);
                            pictureBox1.Refresh();
                        }
                    };
                }
            }
        }

        private void SetItemInfoOnPicture(string[] itemInfo,Control picture)
        {
            string nullHex = "";
            for (int i = 0; i < hexLenght; i++)
            {
                nullHex += "F";
            }

            PictureBox pb = (PictureBox)picture;
            charInfos.Add(itemInfo[3], itemInfo);
            Bitmap b = ItemImage(Convert.ToInt32(itemInfo[1]), Convert.ToInt32(itemInfo[2]), Convert.ToInt32(itemInfo[8]), 0);
           // pb.Size = new System.Drawing.Size(32 * Convert.ToInt32(itemInfo[6]), 32 * Convert.ToInt32(itemInfo[7]));
            pb.BackgroundImage = b;
            pb.Parent = pbVaultChar;
            pb.Location = new Point(pb.Location.X-315, pb.Location.Y);
            pb.Visible = true;
            pb.BackColor = Color.FromArgb(25, 255, 255, 255);
            PictureBox pbText = new PictureBox();
            pb.Name = itemInfo[3];

            #region MouseEnter
            pb.MouseEnter += (s, eArgs) =>
            {
                if (!isMovingItem)
                {
                    string picText = "";
                    for (int j = 0; j < itemInfo.Length; j++)
                    {
                        picText += itemInfo[j] + Environment.NewLine;
                    }
                    Size size = TextRenderer.MeasureText(picText, pbText.Font);
                    pbText.Width = size.Width;
                    pbText.Height = size.Height;

                    Point mouseLocation = this.PointToClient(Cursor.Position);
                    if (this.Width - mouseLocation.X < size.Width)
                    {
                        mouseLocation.X = mouseLocation.X - size.Width;
                    }
                    if (this.Height - mouseLocation.Y < size.Height)
                    {
                        mouseLocation.Y = mouseLocation.Y - size.Height;
                    }
                    pbText.BackColor = Color.FromArgb(200, Color.Black);
                    pbText.Paint += (object sss, PaintEventArgs argss) =>
                    {
                        using (Font myFont = new Font("Arial", 9))
                        {
                            argss.Graphics.DrawString(picText, myFont, Brushes.White, new Point(2, 2));

                        }
                    };
                    pbText.Parent = pictureBox1;
                    pbText.Location = mouseLocation;

                    this.Controls.Add(pbText);
                    this.Controls[this.Controls.Count - 1].BringToFront();
                }
            };
            #endregion

            #region MouseLeave
            pb.MouseLeave += (s, eArgs) =>
            {
                this.Controls.Remove(pbText);
            };
            #endregion

            #region pbMouseClick
            pb.MouseClick += (object send, MouseEventArgs ahah) =>
            {
                if (ahah.Button == System.Windows.Forms.MouseButtons.Left && !isMovingItem)
                {
                    Control control = (Control)pb;
                    if (!isMovingItem)
                    {
                        oldLocation = control.Location;
                    }
                    if (charInfos.ContainsKey(control.Name))
                    {
                        selectedPic = control;
                        pb.BringToFront();
                        isMovingItem = true;
                    }
                }
            };
            #endregion

            #region pictureBox1MouseClick
            pbVaultChar.MouseClick += (object sende, MouseEventArgs ahhh) =>
            {
                if (isMovingItem)
                {
                    isMovingItem = false;
                    Point loc = selectedPic.Location;
                    int cellX = loc.X / 32;
                    int cellY = loc.Y / 32;
                    loc.X = cellX * 32;
                    loc.Y = cellY * 32;

                    int fcX = cellX;
                    int fcy = cellY;
                    int itemWidth = selectedPic.Width / 32;
                    int itemHeight = selectedPic.Height / 32;

                    Point sD = new Point(selectedPic.Location.X / 32, selectedPic.Location.Y / 32);
                    Point sC = new Point(selectedPic.Location.X / 32 + selectedPic.Width / 32, selectedPic.Location.Y / 32);
                    Point sA = new Point(selectedPic.Location.X / 32, selectedPic.Height / 32 + selectedPic.Location.Y / 32);
                    Point sB = new Point(selectedPic.Width / 32 + selectedPic.Location.X / 32, selectedPic.Height / 32 + selectedPic.Location.Y / 32);

                    Rectangle movingItem = new Rectangle(fcX, fcy, itemWidth, itemHeight);
                    Rectangle vaultBounds = new Rectangle(-1, -1, (256 / 32) + 2, (480 / 32) + 2);
                    bool isOnItem = false;
                    foreach (List<string> item in charInfos.Values)
                    {
                        if (selectedPic.Name != item[3])
                        {
                            Control control = pictureBox1.Controls[item[3]];
                            Point D = new Point(control.Location.X / 32, control.Location.Y / 32);
                            Point C = new Point(control.Location.X / 32 + control.Width / 32, control.Location.Y / 32);
                            Point A = new Point(control.Location.X / 32, control.Height / 32 + control.Location.Y / 32);
                            Point B = new Point(control.Width / 32 + control.Location.X / 32, control.Height / 32 + control.Location.Y / 32);
                            Point R = new Point(control.Width / 64 + control.Location.X / 32, control.Height / 64 + control.Location.Y / 32);
                            bool isR = true;
                            if (control.Width == 32)
                            {
                                isR = false;
                            }

                            if (control.Height >= (32 * 4))
                            {
                                Point R1 = new Point(control.Width / 64 + control.Location.X / 32, (control.Height / 64 + control.Location.Y / 32) - 1);
                                Point R2 = new Point(control.Width / 64 + control.Location.X / 32, (control.Height / 64 + control.Location.Y / 32) + 1);
                                if (IsOnItem(D, movingItem, false) || IsOnItem(C, movingItem, false) || IsOnItem(A, movingItem, false) || IsOnItem(B, movingItem, false) || IsOnItem(R, movingItem, isR) ||
                                    !IsOnItem(sD, vaultBounds, false) || !IsOnItem(sA, vaultBounds, false) || !IsOnItem(sB, vaultBounds, false) || !IsOnItem(sC, vaultBounds, false) ||
                                    IsOnItem(R1, movingItem, isR) || IsOnItem(R2, movingItem, isR))
                                {
                                    selectedPic.Location = oldLocation;
                                    isOnItem = true;
                                    break;
                                }
                            }
                            else if (control.Width >= (32 * 4))
                            {
                                Point R1 = new Point((control.Width / 64 + control.Location.X / 32) - 1, control.Height / 64 + control.Location.Y / 32);
                                Point R2 = new Point((control.Width / 64 + control.Location.X / 32) + 1, control.Height / 64 + control.Location.Y / 32);
                                if (IsOnItem(D, movingItem, false) || IsOnItem(C, movingItem, false) || IsOnItem(A, movingItem, false) || IsOnItem(B, movingItem, false) || IsOnItem(R, movingItem, isR) ||
                                    !IsOnItem(sD, vaultBounds, false) || !IsOnItem(sA, vaultBounds, false) || !IsOnItem(sB, vaultBounds, false) || !IsOnItem(sC, vaultBounds, false) ||
                                    IsOnItem(R1, movingItem, isR) || IsOnItem(R2, movingItem, isR))
                                {
                                    selectedPic.Location = oldLocation;
                                    isOnItem = true;
                                    break;
                                }
                            }
                            else
                            {
                                if (IsOnItem(D, movingItem, false) || IsOnItem(C, movingItem, false) || IsOnItem(A, movingItem, false) || IsOnItem(B, movingItem, false) || IsOnItem(R, movingItem, isR) ||
                                    !IsOnItem(sD, vaultBounds, false) || !IsOnItem(sA, vaultBounds, false) || !IsOnItem(sB, vaultBounds, false) || !IsOnItem(sC, vaultBounds, false))
                                {
                                    selectedPic.Location = oldLocation;
                                    isOnItem = true;
                                    break;
                                }
                            }

                        }
                    }

                    if (!isOnItem)
                    {


                        linesList[(loc.X / 32) + ((loc.Y / 32) * 8)] = linesList[(oldLocation.X / 32) + ((oldLocation.Y / 32) * 8)];
                        linesList[(oldLocation.X / 32) + ((oldLocation.Y / 32) * 8)] = nullHex;
                        string tempVaultHex = string.Join("", linesList);

                        //if (!VaultHexCheck(tempVaultHex))
                       // {
                        //    selectedPic.Location = oldLocation;
                       // }
                       // else
                       // {
                            selectedPic.Location = loc;
                            oldLocation = loc;
                            vaultHex = tempVaultHex;
                       // }
                    }
                }
            };
            #endregion

            #region MouseMove
            pbVaultChar.MouseMove += (object sndr, MouseEventArgs aaa) =>
            {
                if (isMovingItem)
                {

                    Point pt = Cursor.Position;
                    pt.X = pt.X + 2;
                    pt.Y = pt.Y + 2;
                    selectedPic.Location = pictureBox1.PointToClient(pt);
                    pictureBox1.Refresh();
                }
            };
            #endregion
        }

        private void LoadChar(string charHex)
        {
            pbCharView.Controls.Clear();
            pbFH1.BackgroundImage = null;
            pbSH2.BackgroundImage = null;
            pbHelm3.BackgroundImage = null;
            pbArmor4.BackgroundImage = null;
            pbPants5.BackgroundImage = null;
            pbGloves6.BackgroundImage = null;
            pbBoots7.BackgroundImage = null;
            pbWings8.BackgroundImage = null;
            pbPet9.BackgroundImage = null;
            pbPedant10.BackgroundImage = null;
            pbLeftRing11.BackgroundImage = null;
            pbRightRing12.BackgroundImage = null;

            string user_items = charHex;

            int index = 0;
            while (index + hexLenght <= user_items.Length)
            {
                charLinesList.Add(user_items.Substring(index, hexLenght));
                index += hexLenght;
            }

            List<string> itemInfo = ItemInfo(charLinesList[0]);
            if (itemInfo.Count > 0)
            {
                SetItemInfoOnPicture(itemInfo.ToArray(), pbFH1);
                charLinesList.RemoveAt(0);
            }
            itemInfo = ItemInfo(charLinesList[1]);
            if (itemInfo.Count > 0)
            {
                SetItemInfoOnPicture(itemInfo.ToArray(), pbSH2);
                charLinesList.RemoveAt(1);
            }
            itemInfo = ItemInfo(charLinesList[2]);
            if (itemInfo.Count > 0)
            {
                SetItemInfoOnPicture(itemInfo.ToArray(), pbHelm3);
                charLinesList.RemoveAt(2);
            }
            itemInfo = ItemInfo(charLinesList[3]);
            if (itemInfo.Count > 0)
            {
                SetItemInfoOnPicture(itemInfo.ToArray(), pbArmor4);
                charLinesList.RemoveAt(3);
            }
            itemInfo = ItemInfo(charLinesList[4]);
            if (itemInfo.Count > 0)
            {
                SetItemInfoOnPicture(itemInfo.ToArray(), pbPants5);
                charLinesList.RemoveAt(4);
            }
            itemInfo = ItemInfo(charLinesList[5]);
            if (itemInfo.Count > 0)
            {
                SetItemInfoOnPicture(itemInfo.ToArray(), pbGloves6);
                charLinesList.RemoveAt(5);
            }
            itemInfo = ItemInfo(charLinesList[6]);
            if (itemInfo.Count > 0)
            {
                SetItemInfoOnPicture(itemInfo.ToArray(), pbBoots7);
                charLinesList.RemoveAt(6);
            }
            itemInfo = ItemInfo(charLinesList[7]);
            if (itemInfo.Count > 0)
            {
                SetItemInfoOnPicture(itemInfo.ToArray(), pbWings8);
                charLinesList.RemoveAt(7);
            }
            itemInfo = ItemInfo(charLinesList[8]);
            if (itemInfo.Count > 0)
            {
                SetItemInfoOnPicture(itemInfo.ToArray(), pbPet9);
                charLinesList.RemoveAt(8);
            }
        }

        private bool IsOnItem(Point p, Rectangle r, bool IsR)
        {
            if (IsR)
                return ((r.X <= p.X) && (p.X <= r.X + r.Width)) && ((r.Y <= p.Y) && (p.Y <= r.Y + r.Height));
            else
                return ((r.X < p.X) && (p.X < r.X + r.Width)) && ((r.Y < p.Y) && (p.Y < r.Y + r.Height));
        }

        private List<string> ItemInfo(string itemHex)
        {
            List<string> info = new List<string>();

            string nullHex = "";
            for (int i = 0; i < hexLenght; i++)
            {
                nullHex += "F";
            }

            if (itemHex.Length != hexLenght || !Regex.IsMatch(itemHex, "(^[a-zA-Z0-9])") || itemHex == nullHex)
            {
                //MessageBox.Show("Invalid Item!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return info;
            }

            // string asd = itemHex.Substring(0, 2);

            int itemId = Int32.Parse(itemHex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            int itemOption = Int32.Parse(itemHex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber); //Item Level/Skill/Option Data
            int itemDur = Int32.Parse(itemHex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            string itemSerial = itemHex.Substring(6, 8);
            int itemExOpt = Int32.Parse(itemHex.Substring(14, 2), System.Globalization.NumberStyles.HexNumber);
            int itemAncientData = Int32.Parse(itemHex.Substring(16, 2), System.Globalization.NumberStyles.HexNumber);
            int itemType = Int32.Parse(itemHex.Substring(18, 2), System.Globalization.NumberStyles.HexNumber);
            int itemtype = itemType / 16;

            if (itemAncientData == 4)
                itemAncientData = 5;
            if (itemAncientData == 9)
                itemAncientData = 10;

            switch (itemtype)
            {
                case 12:
                    itemtype = 14;
                    break;
                case 13:
                    itemtype = 12;
                    break;
                case 14:
                    itemtype = 13;
                    break;
            }

            //Item ID Check
            string level = itemHex.Substring(0, 1);
            string level2 = itemHex.Substring(1, 1);
            string level3 = itemHex.Substring(14, 2);
            string AA = level;
            string BB = level2;
            string CC = level3;
            int level1 = Int32.Parse(itemHex.Substring(0, 1), System.Globalization.NumberStyles.HexNumber);
            if ((level1 % 2) != 0)
            {
                level2 = "1" + level2;
                level1--;
            }
            if (Int32.Parse(level3, System.Globalization.NumberStyles.HexNumber) >= 128)
            { level1 += 16; }
            level1 /= 2;
            level2 = Int32.Parse(level2, System.Globalization.NumberStyles.HexNumber).ToString();



            string skill = "";
            if (itemOption < 128)
                skill = "";
            else
            {
                skill = "This weapon has a special skill";
                itemOption = itemOption - 128;
            }



            int itemlevel = (int)Math.Floor((double)itemOption / (double)8);
            itemOption = itemOption - itemlevel * 8;
            // Luck Check
            string luck = "";
            if (itemOption < 4)
                luck = "";
            else
            {
                luck = "Luck (success rate of jewel of soul +25%) " + Environment.NewLine + "Luck (critical damage rate +5%)<br>";
                itemOption = itemOption - 4;
            }

            int iopx1 = 0;
            int iopx2 = 0;
            int iopx3 = 0;
            int iopx4 = 0;
            int iopx5 = 0;
            int iopx6 = 0;

            // Excellent option check
            if (itemExOpt >= 64) { itemOption += 4; itemExOpt += -64; }
            if (itemExOpt < 32) { iopx6 = 0; } else { iopx6 = 1; itemExOpt += -32; }
            if (itemExOpt < 16) { iopx5 = 0; } else { iopx5 = 1; itemExOpt += -16; }
            if (itemExOpt < 8) { iopx4 = 0; } else { iopx4 = 1; itemExOpt += -8; }
            if (itemExOpt < 4) { iopx3 = 0; } else { iopx3 = 1; itemExOpt += -4; }
            if (itemExOpt < 2) { iopx2 = 0; } else { iopx2 = 1; itemExOpt += -2; }
            if (itemExOpt < 1) { iopx1 = 0; } else { iopx1 = 1; itemExOpt += -1; }

            string op1 = "";
            string op2 = "";
            string op3 = "";
            string op4 = "";
            string op5 = "";
            string op6 = "";
            string inf = "";
            bool nocolor = false;

            //string asd = hs[level1 + ";" + level2].ToString().Split(';')[1];
            if (!hs.Contains(level1 + ";" + level2))
            {
                return null;
            }
            int iopxltype = Convert.ToInt32(hs[level1 + ";" + level2].ToString().Split(';')[1]);

            switch (iopxltype)
            {
                case 0:
                    op1 = "Increase Mana per kill +8";
                    op2 = "Increase hit points per kill +8";
                    op3 = "Increase attacking(wizardly)speed+7";
                    op4 = "Increase wizardly damage +2%";
                    op5 = "Increase Damage +level/20";
                    op6 = "Excellent Damage Rate +10%";
                    inf = "Additional Damage";
                    break;
                case 1:
                    op1 = "Increase Zen After Hunt +40%";
                    op2 = "Defense success rate +10%";
                    op3 = "Reflect damage +5%";
                    op4 = "Damage Decrease +4%";
                    op5 = "Increase MaxMana +4%";
                    op6 = "Increase MaxHP +4%";
                    inf = "Additional Defense";
                    skill = "";
                    break;
                case 2:
                    op1 = " Increase HP";
                    op2 = " Increase MP";
                    op3 = " +Defense Ignore";
                    op4 = " +Stamina Increase";
                    op5 = " +Speed Increase";
                    op6 = "";
                    inf = "Additional Damage";
                    skill = "";
                    nocolor = true;
                    break;
                case 4: // v0.9
                    op1 = " +115 HP";
                    op2 = " +115 MP";
                    op3 = " Ignore Enemy defense 3%";
                    op4 = " +50 Max stamina";
                    op5 = " Wizardly speed +7";
                    op6 = "";
                    inf = "Additional Damage";
                    skill = "";
                    nocolor = true;
                    break;
            }
            string itemoption = "";
            if (level1 == 12)
            { //pedant
                itemoption = itemOption + "%";
                inf = " Automatic HP Recovery rate ";
            }
            else if (level1 == 13)
            { //shields
                itemoption = Convert.ToString((itemOption * 5));
                inf = " Additional Defense rate ";
            }
            else
                itemoption = Convert.ToString((itemOption * 4));


            string itemname = hs[level1 + ";" + level2].ToString().Split(';')[0];
            if (level1 == 12 && level2 == "37")
            {
                skill = "Plasma storm skill (Mana:50)";

                if (iopx1 == 1)
                {
                    itemname += " +Destroy";
                    itemoption = "Increase final damage 10% " + Environment.NewLine + "Increase speed";
                }
                else if (iopx2 == 1)
                {
                    itemname += " +Protect";
                    itemoption = "Absorb final damage 10% " + Environment.NewLine + "Increase speed";
                }
                else if (iopx3 == 1)
                { // v0.9
                    itemname = "Golden Fenrir";
                    itemoption = "Increase speed";
                }
            }
            string itemexl = "";
            if (iopx1 == 1) itemexl += op1 + Environment.NewLine;
            if (iopx2 == 1) itemexl += op2 + Environment.NewLine;
            if (iopx3 == 1) itemexl += op3 + Environment.NewLine;
            if (iopx4 == 1) itemexl += op4 + Environment.NewLine;
            if (iopx5 == 1) itemexl += op5 + Environment.NewLine;
            if (iopx6 == 1) itemexl += op6 + Environment.NewLine;

            int type = 0;
            if (!string.IsNullOrEmpty(itemexl))
            {
                type = 1;
            }

            //if (itemAncientData > 0)
            //{
            //    itemexl = "";
            //    itemoption = "Ancient +" + itemAncientData + " stamina.";
            //    type = 2;
            //}

            switch (type)
            {
                case 1:
                    itemname = "Excellent " + itemname;
                    break;
                case 2:
                    itemname = "Ancient " + itemname;
                    break;
                default:
                    break;
            }

            info.Add(itemname);
            info.Add(level1.ToString()); //type
            info.Add(level2); //id
            info.Add(itemSerial);
            info.Add(skill);
            info.Add(luck);
            info.Add(hs[level1 + ";" + level2].ToString().Split(';')[2]);
            info.Add(hs[level1 + ";" + level2].ToString().Split(';')[3]);
            info.Add(itemlevel.ToString());
            info.Add(itemexl);

            return info;
        }

        private Bitmap ItemImage(int type, int id, int level, int exlAnc)
        {
            string tnpl = "";
            switch (exlAnc)
            {
                case 1:	// Excellent item
                    tnpl = "10";
                    break;
                case 2:	// Ancient item
                    tnpl = "01";
                    break;
                default:// Normal Item
                    tnpl = "00";
                    break;

            }

            int itype = type * 16;
            string nxt = "";
            string tipaj = "";
            if (id > 31)
            {
                nxt = "F9";
                id += -32;
            }
            else
                nxt = "00";

            if (itype < 128)
            {
                tipaj = "00";
                id += itype;
            }
            else
            {
                tipaj = "80";
                itype += -128;
                id += itype;
            }

            itype += id;
            if (File.Exists("items\\" + tnpl + String.Format("{0:X2}", itype) + type + nxt + ".gif"))
            {
                return new Bitmap("items\\" + tnpl + String.Format("{0:X2}", itype) + tipaj + nxt + ".gif");
            }
            else
            {
                return new Bitmap("items\\00" + String.Format("{0:X2}", itype) + tipaj + nxt + ".gif");
            }
        }

        private bool VaultHexCheck(string hex)
        {
            string response = "";
            ServerConnection sc = new ServerConnection(IP, encriptionKey);
            response = sc.SendToServer("<arangevault>" + hex + ":" + myAccount);
            if (response.ToLower() == "true")
                return true;
            else
                return false;
        }

        private bool CharacterHexCheck(string hex)
        {
            return true;
        }

        private void comboBoxChars_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxChars.SelectedIndex > -1 && comboBoxChars.SelectedIndex < comboBoxChars.Items.Count)
            {
                pbVaultChar.Width = 611;
                pbCharView.Visible = true;
                ServerConnection sc = new ServerConnection(IP, encriptionKey);
                charHex = sc.SendToServer("<getcharhex>" + myAccount + ":" + comboBoxChars.SelectedItem.ToString());
                LoadChar(charHex);
            }
        }
    }
}
