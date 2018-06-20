/*================================================================//
//     __      ____   ____                                        //
//   /'_ `\   /'___\ /\  _`\             __                       //
//  /\ \L\ \ /\ \__/ \ \ \/\ \   __  __ /\_\     ___      ___     //
//  \/_> _ <_\ \  _``\\ \ \ \ \ /\ \/\ \\/\ \  /' _ `\   / __`\   //
//    /\ \L\ \\ \ \L\ \\ \ \_\ \\ \ \_\ \\ \ \ /\ \/\ \ /\ \L\ \  //
//    \ \____/ \ \____/ \ \____/ \ \____/ \ \_\\ \_\ \_\\ \____/  //
//     \/___/   \/___/   \/___/   \/___/   \/_/ \/_/\/_/ \/___/   //
//                                                                //
//                                       http://www.86duino.com   //
//================================================================//
 NewMotion.cs - DM&P 86ME
 Copyright (c) 2017 Sayter <sayter@dmp.com.tw>. All right reserved.
 Copyright (c) 2018 RoBoardGod <roboardgod@dmp.com.tw>. All right reserved.

 This program is free software; you can redistribute it and/or
 modify it under the terms of the GNU General Public License as
 published by the Free Software Foundation; either version 2 of
 the License, or (at your option) any later version.

 This program is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 GNU General Public License for more details.

 You should have received a copy of the GNU General Public License
 along with this program; if not, write to the Free Software
 Foundation, Inc., 51 Franklin St, Fifth Floor, Boston,
 MA  02110-1301  USA

 (If you need a commercial license, please contact soc@dmp.com.tw
  to get more information.)
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Collections;

namespace _86ME_ver2
{
    public partial class NewMotion : Form
    {
        public Dictionary<string, string> NewMotion_lang_dic;
        public Dictionary<int, int> mirror = new Dictionary<int,int>();
        public Arduino arduino = null;
        public Panel[] fpanel = new Panel[45];
        public Label[] flabel = new Label[45];
        public Label[] flabel2 = new Label[45];
        public ComboBox[] fbox = new ComboBox[45];
        public ComboBox[] fbox2 = new ComboBox[45];
        public ComboBox[] fbox3 = new ComboBox[45];
        public MaskedTextBox[] ftext = new MaskedTextBox[45];
        public MaskedTextBox[] ftext2 = new MaskedTextBox[45];
        public MaskedTextBox[] ftext3 = new MaskedTextBox[45];
        public MaskedTextBox[] ftext4 = new MaskedTextBox[45];
        public MaskedTextBox[] ftext5 = new MaskedTextBox[45];
        public MaskedTextBox[] ftext6 = new MaskedTextBox[45];
        public CheckBox[] fcheck = new CheckBox[45];
        public CheckBox[] fcheck_ps = new CheckBox[45];
        public HScrollBar[] fbar_off = new HScrollBar[45];
        public HScrollBar[] fbar_home = new HScrollBar[45];
        int[] offset = new int[45];
        int last_index;
        int last_IMU;
        int[] autoframe = new int[45];
        int[] autogain = new int[45];
        uint[] homeframe = new uint[45];
        uint[] Max = new uint[45];
        uint[] min = new uint[45];
        public double[] p_gain = new double[45];
        public double[] s_gain = new double[45];
        public Quaternion q = new Quaternion();
        private Quaternion autoq = new Quaternion();
        private double[] omega = new double[2];
        char[] delimiterChars = { ' ', '\t', '\r', '\n' };
        public string picfilename = null;
        public int[] channelx = new int[45];
        public int[] channely = new int[45];
        public bool newflag = false;
        public string mirrorfilename = null;
        public Thread sync;
        private Progress init_ProcessBar = null;
        private delegate bool IncreaseHandle(int nValue);
        private IncreaseHandle progress_Increase = null;
        private object serial_lock = new object();
        private bool send_msg = false;
        private bool renew_quaternion = true;
        private int[] last_fbox = new int[45];
        int dpi;
        private bool develop_mode = false;
        public NewMotion(Dictionary<string, string> lang_dic, bool dm = false)
        {
            Graphics graphics = this.CreateGraphics();
            dpi = (int)graphics.DpiX;
            InitializeComponent();
            NewMotion_lang_dic = lang_dic;
            develop_mode = dm;
            comboBox1.Items.AddRange(new object[] { "86Duino Zero",
                                                    "86Duino One",
                                                    "86Duino EduCake"
                                                    });

            comboBox1.SelectedIndex = 1;
            comboBox2.Items.Clear();
            comboBox2.Items.AddRange(new object[] { "NONE",
                                                    "RoBoard RM-G146",
                                                    "On-Board IMU"
                                                    });
            comboBox2.SelectedIndex = 0;
            maskedTextBox1.Text = (q.w).ToString();
            maskedTextBox2.Text = (q.x).ToString();
            maskedTextBox3.Text = (q.y).ToString();
            maskedTextBox4.Text = (q.z).ToString();
            maskedTextBox1.Enabled = false;
            maskedTextBox2.Enabled = false;
            maskedTextBox3.Enabled = false;
            maskedTextBox4.Enabled = false;
            getQ.Enabled = false;
            init_imu.Enabled = false;
            for (int i = 0; i < 45; i++)
            {
                offset[i] = 0;
                min[i] = 600;
                Max[i] = 2400;
                homeframe[i] = 1500;
                channelx[i] = 0;
                channely[i] = 0;
                p_gain[i] = 0;
                s_gain[i] = 0;
            }
            clear_Channels();
            create_panel(0, 45, 0);
            label10.Enabled = false;
            label11.Enabled = false;
            channelver.HorizontalScroll.Maximum = 0;
            channelver.AutoScroll = false;
            channelver.VerticalScroll.Visible = false;
            channelver.AutoScroll = true;
            comboBox1.MouseWheel += new MouseEventHandler(comboBox_MouseWheel);
            comboBox2.MouseWheel += new MouseEventHandler(comboBox_MouseWheel);

            applyLang();
            this.CenterToScreen();
        }
        public void develop_mode_change(bool dm)
        {
            int i = comboBox1.SelectedIndex;
            develop_mode = dm;
            comboBox1.Items.Clear();
            if (develop_mode)
            {
                comboBox1.Items.AddRange(new object[] { "86Duino Zero",
                                                        "86Duino One",
                                                        "86Duino EduCake",
                                                        "86Duino AI"
                                                        });
            }
            else
            {
                comboBox1.Items.AddRange(new object[] { "86Duino Zero",
                                                        "86Duino One",
                                                        "86Duino EduCake"
                                                        });
            }
            if (i < comboBox1.Items.Count)
                comboBox1.SelectedIndex = i;
            else
                comboBox1.SelectedIndex = 0;
        }
        public void start_synchronizer()
        {
            sync = new Thread(() => synchronizer());
            sync.IsBackground = true;
            sync.Start();
        }

        private void update_autoframe(int i)
        {
            if (fcheck[i].Checked == true)
            {
                if ((fbox2[i].SelectedIndex != 0 || fbox3[i].SelectedIndex != 0) && getQ.Enabled == true)
                {
                    try
                    {
                        if (renew_quaternion)
                        {
                            arduino.getQ();
                            DateTime time_start = DateTime.Now;
                            while (!arduino.dataRecieved && (DateTime.Now - time_start).TotalMilliseconds < 100) ;
                            arduino.dataRecieved = false;
                            autoq.w = arduino.quaternion[0];
                            autoq.x = arduino.quaternion[1];
                            autoq.y = arduino.quaternion[2];
                            autoq.z = arduino.quaternion[3];
                            arduino.pin_capture(11);
                            while (!arduino.dataRecieved && (DateTime.Now - time_start).TotalMilliseconds < 100) ;
                            arduino.dataRecieved = false;
                            omega[0] = 0.1*omega[0] + 0.9*arduino.captured_float;
                            arduino.pin_capture(12);
                            while (!arduino.dataRecieved && (DateTime.Now - time_start).TotalMilliseconds < 100) ;
                            arduino.dataRecieved = false;
                            omega[1] = 0.1*omega[1] + 0.9*arduino.captured_float;
                            renew_quaternion = false;
                        }
                        RollPitchYaw rpy = (autoq.Normalized().Round(4) * q.Normalized().Round(4).Inverse()).toRPY();
                        int gain = 0;
                        for (int src = 0; src < 2; src++)
                        {
                            if (src == 0 && p_gain[i] != 0)
                            {
                                if (fbox2[i].SelectedIndex == 1 || fbox2[i].SelectedIndex == 2)
                                    gain += (int)Math.Round(rpy.rpy[fbox2[i].SelectedIndex - 1] * (180 / Math.PI) * p_gain[i]);
                                else if (fbox2[i].SelectedIndex == 3 || fbox2[i].SelectedIndex == 4)
                                    gain += (int)Math.Round(omega[fbox2[i].SelectedIndex - 3] * p_gain[i]);
                            }
                            else if (src == 1 && s_gain[i] != 0)
                            {
                                if (fbox3[i].SelectedIndex == 1 || fbox3[i].SelectedIndex == 2)
                                    gain += (int)Math.Round(rpy.rpy[fbox3[i].SelectedIndex - 1] * (180 / Math.PI) * s_gain[i]);
                                else if (fbox3[i].SelectedIndex == 3 || fbox3[i].SelectedIndex == 4)
                                    gain += (int)Math.Round(omega[fbox3[i].SelectedIndex - 3] * s_gain[i]);
                            }
                        }
                        autogain[i] = gain;
                    }
                    catch
                    {
                        autogain[i] = 0;
                    }
                }
                else
                {
                    autogain[i] = 0;
                }
                int pos = (int)homeframe[i] + offset[i] + autogain[i];
                if ((uint)pos >= min[i] && (uint)pos <= Max[i])
                {
                    if (autoframe[i] != pos)
                    {
                        autoframe[i] = pos;
                        send_msg = true;
                    }
                    return;
                }
            }
            autoframe[i] = 0;
        }

        private void synchronizer()
        {
            while (true)
            {
                lock (serial_lock)
                {
                    if (arduino != null)
                    {
                        for (int i = 0; i < 45; i++)
                            if (string.Compare("---noServo---", fbox[i].Text) != 0)
                                update_autoframe(i);
                        renew_quaternion = true;
                        if (send_msg)
                        {
                            try
                            {
                                arduino.frameWrite(0x6F, autoframe, 0);
                                send_msg = false;
                            }
                            catch { }
                        }
                    }
                    Thread.Sleep(28);
                }
            }
        }

        public void applyLang()
        {
            this.Text = NewMotion_lang_dic["NewMotion_title"];
            checkBox2.Text = NewMotion_lang_dic["NewMotion_checkBox2_Text"];
            label1.Text = NewMotion_lang_dic["NewMotion_label1_Text"];
            label2.Text = NewMotion_lang_dic["NewMotion_label2_Text"];
            label3.Text = NewMotion_lang_dic["NewMotion_label3_Text"];
            label4.Text = NewMotion_lang_dic["NewMotion_label4_Text"];
            label5.Text = NewMotion_lang_dic["NewMotion_label5_Text"];
            button3.Text = NewMotion_lang_dic["NewMotion_button3_Text"];
            button4.Text = NewMotion_lang_dic["NewMotion_button4_Text"];
            init_imu.Text = NewMotion_lang_dic["NewMotion_init_imu_Text"];
            getQ.Text = NewMotion_lang_dic["NewMotion_getQ_Text"];
            ttp.SetToolTip(button3, NewMotion_lang_dic["NewMotion_loadpic_ToolTip"]);
            ttp.SetToolTip(checkBox2, NewMotion_lang_dic["NewMotion_minMax_ToolTip"]);
            ttp.SetToolTip(button4, NewMotion_lang_dic["NewMotion_button4_ToolTip"]);
            for (int i = 0; i < 45; i++)
            {
                fcheck[i].Text = NewMotion_lang_dic["NewMotion_fcheckText"];
                ttp.SetToolTip(fcheck[i], NewMotion_lang_dic["NewMotion_fcheck_ToolTip"]);
            }
        }

        private void IMU_DropDown(object sender, EventArgs e)
        {
            last_IMU = comboBox2.SelectedIndex;
        }

        public void SetIMUUI(int imu)
        {
            if (imu == 0)
            {
                init_imu.Enabled = false;
                getQ.Enabled = false;
                maskedTextBox1.Enabled = false;
                maskedTextBox2.Enabled = false;
                maskedTextBox3.Enabled = false;
                maskedTextBox4.Enabled = false;
                label10.Enabled = false;
                label11.Enabled = false;
                for (int i = 0; i < 45; i++)
                {
                    ftext5[i].Enabled = false;
                    fbox2[i].Enabled = false;
                    ftext6[i].Enabled = false;
                    fbox3[i].Enabled = false;
                    fcheck_ps[i].Enabled = false;
                }
            }
            else
            {
                if (arduino != null)
                    init_imu.Enabled = true;
                getQ.Enabled = false;
                maskedTextBox1.Enabled = true;
                maskedTextBox2.Enabled = true;
                maskedTextBox3.Enabled = true;
                maskedTextBox4.Enabled = true;
                label10.Enabled = true;
                label11.Enabled = true;
                for (int i = 0; i < 45; i++)
                {
                    ftext5[i].Enabled = true;
                    fbox2[i].Enabled = true;
                    ftext6[i].Enabled = true;
                    fbox3[i].Enabled = true;
                    fcheck_ps[i].Enabled = true;
                }
            }
        }

        private void IMU_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex == last_IMU)
                return;
            else
                SetIMUUI(comboBox2.SelectedIndex);
        }

        private void Quaternion_TextChanged(object sender, EventArgs e)
        {
            double output;
            if (double.TryParse(((MaskedTextBox)sender).Text, out output))
            {
                switch(((MaskedTextBox)sender).Name)
                {
                    case "maskedTextBox1":
                        q.w = output;
                        break;
                    case "maskedTextBox2":
                        q.x = output;
                        break;
                    case "maskedTextBox3":
                        q.y = output;
                        break;
                    case "maskedTextBox4":
                        q.z = output;
                        break;
                    default:
                        break;
                }
            }
            else if (((MaskedTextBox)sender).Text == "-" || ((MaskedTextBox)sender).Text == "" ||
                     ((MaskedTextBox)sender).Text == "-." || ((MaskedTextBox)sender).Text == ".")
            {
                switch (((MaskedTextBox)sender).Name)
                {
                    case "maskedTextBox1":
                        q.w = 1;
                        break;
                    case "maskedTextBox2":
                        q.x = 0;
                        break;
                    case "maskedTextBox3":
                        q.y = 0;
                        break;
                    case "maskedTextBox4":
                        q.z = 0;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                MessageBox.Show(NewMotion_lang_dic["errorMsg19"]);
                ((MaskedTextBox)sender).SelectAll();
            }
        }

        public void floatcheck(object sender, KeyPressEventArgs e) //Text number check
        {
            if ((int)e.KeyChar == 46 && ((MaskedTextBox)sender).Text.IndexOf('.') != -1)
            {
                e.Handled = true;
            }
            if (e.KeyChar == (char)('-') && ((MaskedTextBox)sender).Text.IndexOf('-') != -1)
            {
                e.Handled = true;
            }
            if (((int)e.KeyChar < 48 | (int)e.KeyChar > 57) & (int)e.KeyChar != 46 & (int)e.KeyChar != 8 & e.KeyChar != (char)('-'))
            {
                e.Handled = true;
            }
        }

        public void numbercheck(object sender, KeyPressEventArgs e) //Text number check
        {
            if (((int)e.KeyChar < 48 | (int)e.KeyChar > 57) & (int)e.KeyChar != 8 & (int)e.KeyChar != 189)
            {
                e.Handled = true;
            }
        }

        public void numbercheck_offset(object sender, KeyPressEventArgs e) //Text number check
        {
            if (e.KeyChar == (char)('-') && ((MaskedTextBox)sender).Text.IndexOf('-') != -1)
            {
                e.Handled = true;
            }
            if (((int)e.KeyChar < 48 | (int)e.KeyChar > 57) & (int)e.KeyChar != 8 & e.KeyChar != (char)('-'))
            {
                e.Handled = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (sync != null)
            {
                sync.Abort();
                sync = null;
            }
            this.DialogResult = DialogResult.Cancel;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (sync != null)
            {
                sync.Abort();
                sync = null;
            }
            if (String.Compare(comboBox1.SelectedItem.ToString(), "86Duino One") != 0 &&
                String.Compare(comboBox1.SelectedItem.ToString(), "86Duino Zero") != 0 &&
                String.Compare(comboBox1.SelectedItem.ToString(), "86Duino EduCake") != 0 &&
                String.Compare(comboBox1.SelectedItem.ToString(), "86Duino AI") != 0)
                MessageBox.Show(NewMotion_lang_dic["NewMotion_err1"]);
            else
                this.DialogResult = DialogResult.OK;
        }

        private void comboBox1_DropDown(object sender, EventArgs e)
        {
            last_index = comboBox1.SelectedIndex;
        }

        public void clear_Channels()
        {
            for (int i = 0; i < 45; i++)
            {
                if (fpanel[i] != null)
                {
                    //fbox[i].SelectedIndex = 0;
                    last_fbox[i] = fbox[i].SelectedIndex;
                    fpanel[i].Controls.Clear();
                }
                else
                {
                    last_fbox[i] = 0;
                }
            }
            channelver.Controls.Clear();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            MessageBox.Show(comboBox1.Text);
            if (string.Compare(comboBox1.Text, "86Duino One") == 0)
            {
                clear_Channels();
                create_panel(0, 45, 0);
            }
            else if (string.Compare(comboBox1.Text, "86Duino Zero") == 0)
            {
                clear_Channels();
                create_panel(0, 14, 0);
                create_panel(42, 45, 14);
            }
            else if (string.Compare(comboBox1.Text, "86Duino EduCake") == 0)
            {
                clear_Channels();
                create_panel(0, 21, 0);
                create_panel(31, 33, 21);
                create_panel(42, 45, 23);
            }
            else if (string.Compare(comboBox1.Text, "86Duino AI") == 0)
            {
                clear_Channels();
                create_panel(0, 36, 0);
            }

            int m = comboBox2.SelectedIndex;
            if (string.Compare(comboBox1.SelectedItem.ToString(), "86Duino One") == 0 || string.Compare(comboBox1.SelectedItem.ToString(), "86Duino AI") == 0)
            {

                comboBox2.Items.Clear();
                comboBox2.Items.AddRange(new object[] { "NONE",
                                                        "RoBoard RM-G146",
                                                        "On-Board IMU"
                                                        });
            }
            else
            {
                comboBox2.Items.Clear();
                comboBox2.Items.AddRange(new object[] { "NONE",
                                                        "RoBoard RM-G146"
                                                        });
            }
            if (m < comboBox2.Items.Count)
                comboBox2.SelectedIndex = m;
            else
                comboBox2.SelectedIndex = 0;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked == true)
            {
                for (int i = 0; i < 45; i++)
                {
                    ftext3[i].Enabled = true;
                    ftext4[i].Enabled = true;
                }
            }
            else
            {
                for (int i = 0; i < 45; i++)
                {
                    ftext3[i].Enabled = false;
                    ftext4[i].Enabled = false;
                }
            }
        }

        private void fcheck_ps_CheckedChanged(object sender, EventArgs e)
        {
            int index = int.Parse(((CheckBox)sender).Name);
            if (((CheckBox)sender).Checked == true)
            {
                ((CheckBox)sender).Image = Properties.Resources.s;
                ((CheckBox)sender).BackColor = System.Drawing.SystemColors.Control;
                ftext5[index].Visible = false;
                fbox2[index].Visible = false;
                ftext6[index].Visible = true;
                fbox3[index].Visible = true;
            }
            else
            {
                ((CheckBox)sender).Image = Properties.Resources.p;
                ((CheckBox)sender).BackColor = System.Drawing.SystemColors.Control;
                ftext6[index].Visible = false;
                fbox3[index].Visible = false;
                ftext5[index].Visible = true;
                fbox2[index].Visible = true;
            }
        }

        public void write_back()
        {
            q.w = double.Parse(maskedTextBox1.Text);
            q.x = double.Parse(maskedTextBox2.Text);
            q.y = double.Parse(maskedTextBox3.Text);
            q.z = double.Parse(maskedTextBox4.Text);
            for (int i=0; i<45; i++)
            {
                offset[i] = int.Parse(ftext[i].Text);
                homeframe[i] = uint.Parse(ftext2[i].Text);
                min[i] = uint.Parse(ftext3[i].Text);
                Max[i] = uint.Parse(ftext4[i].Text);
                p_gain[i] = double.Parse(ftext5[i].Text);
                s_gain[i] = double.Parse(ftext6[i].Text);
            }
        }
        
        public void create_panel(int low, int high, int start_pos)
        {
            for (int i = low; i < high; i++, start_pos++)
            {
                fpanel[i] = new Panel();
                flabel[i] = new Label();
                flabel2[i] = new Label();
                fbox[i] = new ComboBox();
                fbox2[i] = new ComboBox();
                fbox3[i] = new ComboBox();
                ftext[i] = new MaskedTextBox();     //offset
                ftext2[i] = new MaskedTextBox();    //home
                ftext3[i] = new MaskedTextBox();    //Max
                ftext4[i] = new MaskedTextBox();    //min
                ftext5[i] = new MaskedTextBox();
                ftext6[i] = new MaskedTextBox();
                fcheck[i] = new CheckBox();
                fcheck_ps[i] = new CheckBox();
                fbar_off[i] = new HScrollBar();
                fbar_home[i] = new HScrollBar();

                fpanel[i].Size = new Size(685 * dpi / 96, 50 * dpi / 96);
                fpanel[i].Top += (3 + start_pos * 50) * dpi / 96;

                flabel[i].Size = new Size(65 * dpi / 96, 18 * dpi / 96);
                flabel[i].Top += 5 * dpi / 96;
                flabel[i].Left += 5 * dpi / 96;

                flabel2[i].Size = new Size(2 * dpi / 96, 47 * dpi / 96);
                flabel2[i].Left += 534 * dpi / 96;
                flabel2[i].BorderStyle = BorderStyle.FixedSingle;

                fbox[i].DropDownStyle = ComboBoxStyle.DropDownList;
                fbox[i].Size = new Size(135 * dpi / 96, 22 * dpi / 96);
                fbox[i].Left += 70 * dpi / 96;

                fbox2[i].DropDownStyle = ComboBoxStyle.DropDownList;
                fbox2[i].Size = new Size(50 * dpi / 96, 22 * dpi / 96);
                fbox2[i].Left += 580 * dpi / 96;

                fbox3[i].DropDownStyle = ComboBoxStyle.DropDownList;
                fbox3[i].Size = new Size(50 * dpi / 96, 22 * dpi / 96);
                fbox3[i].Left += 580 * dpi / 96;
                fbox3[i].Top += 6 * dpi / 96;

                fcheck[i].Top += 21 * dpi / 96;
                fcheck[i].Left += 155 * dpi / 96;
                fcheck[i].Size = new Size(50 * dpi / 96, 22 * dpi / 96);
                fcheck[i].Text = NewMotion_lang_dic["NewMotion_fcheckText"];
                fcheck[i].Name = i.ToString();
                fcheck[i].Checked = false;
                fcheck[i].Enabled = false;
                ttp.SetToolTip(fcheck[i], NewMotion_lang_dic["NewMotion_fcheck_ToolTip"]);

                fcheck_ps[i].Appearance = Appearance.Button;
                fcheck_ps[i].FlatStyle = FlatStyle.Flat;
                fcheck_ps[i].FlatAppearance.BorderSize = 0;
                fcheck_ps[i].Image = Properties.Resources.p;
                fcheck_ps[i].BackgroundImageLayout = ImageLayout.Center;
                fcheck_ps[i].BackColor = System.Drawing.SystemColors.Control;
                fcheck_ps[i].Size = new Size(23 * dpi / 96, 22 * dpi / 96);
                fcheck_ps[i].Left += 552 * dpi / 96;
                fcheck_ps[i].Name = i.ToString();
                fcheck_ps[i].Enabled = false;
                fcheck_ps[i].CheckedChanged += new EventHandler(fcheck_ps_CheckedChanged);

                ftext[i].Name = i.ToString();
                ftext[i].Text = offset[i].ToString();
                ftext[i].TextAlign = HorizontalAlignment.Right;
                ftext[i].KeyPress += new KeyPressEventHandler(numbercheck_offset);
                ftext[i].TextChanged += new EventHandler(check_offset);
                ftext[i].Size = new Size(90 * dpi / 96, 22 * dpi / 96);
                ftext[i].Left += 425 * dpi / 96;

                ftext2[i].Name = i.ToString();
                ftext2[i].Text = homeframe[i].ToString();
                ftext2[i].TextAlign = HorizontalAlignment.Right;
                ftext2[i].KeyPress += new KeyPressEventHandler(numbercheck);
                ftext2[i].TextChanged += new EventHandler(check_homeframe);
                ftext2[i].Size = new Size(120 * dpi / 96, 22 * dpi / 96);
                ftext2[i].Left += 210 * dpi / 96;

                ftext3[i].Name = i.ToString();
                ftext3[i].Text = min[i].ToString();
                ftext3[i].TextAlign = HorizontalAlignment.Right;
                ftext3[i].KeyPress += new KeyPressEventHandler(numbercheck);
                ftext3[i].TextChanged += new EventHandler(check_range);
                ftext3[i].Size = new Size(40 * dpi / 96, 22 * dpi / 96);
                ftext3[i].Left += 335 * dpi / 96;
                ftext3[i].Enabled = false;

                ftext4[i].Name = i.ToString();
                ftext4[i].Text = Max[i].ToString();
                ftext4[i].TextAlign = HorizontalAlignment.Right;
                ftext4[i].KeyPress += new KeyPressEventHandler(numbercheck);
                ftext4[i].TextChanged += new EventHandler(check_range);
                ftext4[i].Size = new Size(40 * dpi / 96, 22 * dpi / 96);
                ftext4[i].Left += 375 * dpi / 96;
                ftext4[i].Enabled = false;

                ftext5[i].Name = i.ToString();
                ftext5[i].Text = p_gain[i].ToString();
                ftext5[i].TextAlign = HorizontalAlignment.Right;
                ftext5[i].KeyPress += new KeyPressEventHandler(floatcheck);
                ftext5[i].TextChanged += new EventHandler(check_pgain);
                ftext5[i].Size = new Size(40 * dpi / 96, 22 * dpi / 96);
                ftext5[i].Left += 635 * dpi / 96;
                ftext5[i].Enabled = false;

                ftext6[i].Name = i.ToString();
                ftext6[i].Text = s_gain[i].ToString();
                ftext6[i].TextAlign = HorizontalAlignment.Right;
                ftext6[i].KeyPress += new KeyPressEventHandler(floatcheck);
                ftext6[i].TextChanged += new EventHandler(check_sgain);
                ftext6[i].Size = new Size(40 * dpi / 96, 22 * dpi / 96);
                ftext6[i].Left += 635 * dpi / 96;
                ftext6[i].Top += 6 * dpi / 96;
                ftext6[i].Enabled = false;
                ftext6[i].Visible = false;

                fbar_off[i].Name = i.ToString();
                fbar_off[i].Top += 21 * dpi / 96;
                fbar_off[i].Left += 425 * dpi / 96;
                fbar_off[i].Size = new Size(90 * dpi / 96, 22 * dpi / 96);
                fbar_off[i].Minimum = -256;
                fbar_off[i].Maximum = 255 + 9;
                fbar_off[i].Value = int.Parse(ftext[i].Text);
                fbar_off[i].Scroll += new ScrollEventHandler(scroll_off);

                fbar_home[i].Name = i.ToString();
                fbar_home[i].Top += 21 * dpi / 96;
                fbar_home[i].Left += 210 * dpi / 96;
                fbar_home[i].Size = new Size(120 * dpi / 96, 22 * dpi / 96);
                fbar_home[i].Minimum = int.Parse(ftext3[i].Text);
                fbar_home[i].Maximum = int.Parse(ftext4[i].Text) + 9;
                fbar_home[i].Value = int.Parse(ftext2[i].Text);
                fbar_home[i].Scroll += new ScrollEventHandler(scroll_home);

                fbox[i].Items.AddRange(new object[] { "---noServo---",
                                                      "DMP_RS0263",
                                                      "DMP_RS1270",
                                                      "EMAX_ES08AII",
                                                      "EMAX_ES3104",
                                                      "FUTABA_S3003",
                                                      "GWS_S03T",
                                                      "GWS_S777",
                                                      "GWS_MICRO",
                                                      "HITEC_HSR8498",
                                                      "KONDO_KRS4014",
                                                      "KONDO_KRS4024",
                                                      "KONDO_KRS786",
                                                      "KONDO_KRS788",
                                                      "KONDO_KRS78X",
                                                      "SHAYYE_SYS214050",
                                                      "TOWERPRO_SG90",
                                                      "TOWERPRO_MG90S",
                                                      "TOWERPRO_MG995",
                                                      "TOWERPRO_MG996",
                                                      "OtherServos"});
                fbox[i].SelectedIndex = 0;
                fbox[i].Name = i.ToString();
                fbox[i].SelectedIndexChanged += new EventHandler(motors_SelectedIndexChanged);
                fbox[i].MouseWheel += new MouseEventHandler(comboBox_MouseWheel);

                fbox2[i].Items.AddRange(new object[] { "none", "roll", "pitch", "v_roll", "v_pitch" });
                fbox2[i].SelectedIndex = 0;
                fbox2[i].Name = i.ToString();
                fbox2[i].Enabled = false;

                fbox3[i].Items.AddRange(new object[] { "none", "roll", "pitch", "v_roll", "v_pitch" });
                fbox3[i].SelectedIndex = 0;
                fbox3[i].Name = i.ToString();
                fbox3[i].Enabled = false;
                fbox3[i].Visible = false;

                if (i < 10)
                    flabel[i].Text = "SetServo " + i.ToString() + ":";
                else
                    flabel[i].Text = "SetServo" + i.ToString() + ":";

                fpanel[i].Controls.Add(flabel[i]);
                fpanel[i].Controls.Add(flabel2[i]);
                fpanel[i].Controls.Add(fbox[i]);
                fpanel[i].Controls.Add(fbox2[i]);
                fpanel[i].Controls.Add(fbox3[i]);
                fpanel[i].Controls.Add(ftext[i]);
                fpanel[i].Controls.Add(ftext2[i]);
                fpanel[i].Controls.Add(ftext3[i]);
                fpanel[i].Controls.Add(ftext4[i]);
                fpanel[i].Controls.Add(ftext5[i]);
                fpanel[i].Controls.Add(ftext6[i]);
                fpanel[i].Controls.Add(fcheck[i]);
                fpanel[i].Controls.Add(fbar_off[i]);
                fpanel[i].Controls.Add(fbar_home[i]);
                fpanel[i].Controls.Add(fcheck_ps[i]);
                channelver.Controls.Add(fpanel[i]);
                fbox[i].SelectedIndex = last_fbox[i];
            }

        }
        static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp;
            temp = lhs;
            lhs = rhs;
            rhs = temp;
        }
        public void swap_config(int source, int target)
        {
            String swap_s;
            int swap_i;
            bool swap_b;


            swap_s = ftext[source].Text;
            ftext[source].Text = ftext[target].Text;
            ftext[target].Text = swap_s;

            swap_s = ftext2[source].Text;
            ftext2[source].Text = ftext2[target].Text;
            ftext2[target].Text = swap_s;

            swap_s = ftext3[source].Text;
            ftext3[source].Text = ftext3[target].Text;
            ftext3[target].Text = swap_s;
            swap_b = ftext3[source].Enabled;
            ftext3[source].Enabled = ftext3[target].Enabled;
            ftext3[target].Enabled = swap_b;


            swap_s = ftext4[source].Text;
            ftext4[source].Text = ftext4[target].Text;
            ftext4[target].Text = swap_s;
            swap_b = ftext4[source].Enabled;
            ftext4[source].Enabled = ftext4[target].Enabled;
            ftext4[target].Enabled = swap_b;

            swap_s = ftext5[source].Text;
            ftext5[source].Text = ftext5[target].Text;
            ftext5[target].Text = swap_s;
            swap_b = ftext5[source].Enabled;
            ftext5[source].Enabled = ftext5[target].Enabled;
            ftext5[target].Enabled = swap_b;

            swap_s = ftext6[source].Text;
            ftext6[source].Text = ftext6[target].Text;
            ftext6[target].Text = swap_s;
            swap_b = ftext6[source].Enabled;
            ftext6[source].Enabled = ftext6[target].Enabled;
            ftext6[target].Enabled = swap_b;
            swap_b = ftext6[source].Visible;
            ftext6[source].Visible = ftext6[target].Visible;
            ftext6[target].Visible = swap_b;

            swap_i = fbar_off[source].Value;
            fbar_off[source].Value = fbar_off[target].Value;
            fbar_off[target].Value = swap_i;


            swap_i = fbar_home[source].Minimum;
            fbar_home[source].Minimum = fbar_home[target].Minimum;
            fbar_home[target].Minimum = swap_i;
            swap_i = fbar_home[source].Maximum;
            fbar_home[source].Maximum = fbar_home[target].Maximum;
            fbar_home[target].Maximum = swap_i;
            swap_i = fbar_home[source].Value;
            fbar_home[source].Value = fbar_home[target].Value;
            fbar_home[target].Value = swap_i;

            swap_i = fbox[source].SelectedIndex;
            fbox[source].SelectedIndex = fbox[target].SelectedIndex;
            fbox[target].SelectedIndex = swap_i;

            swap_i = fbox2[source].SelectedIndex;
            fbox2[source].SelectedIndex = fbox2[target].SelectedIndex;
            fbox2[target].SelectedIndex = swap_i;
            swap_b = fbox2[source].Enabled;
            fbox2[source].Enabled = fbox2[target].Enabled;
            fbox2[target].Enabled = swap_b;

            swap_i = fbox3[source].SelectedIndex;
            fbox3[source].SelectedIndex = fbox3[target].SelectedIndex;
            fbox3[target].SelectedIndex = swap_i;
            swap_b = fbox3[source].Enabled;
            fbox3[source].Enabled = fbox3[target].Enabled;
            fbox3[target].Enabled = swap_b;
            swap_b = fbox3[source].Visible;
            fbox3[source].Visible = fbox3[target].Visible;
            fbox3[target].Visible = swap_b;


        }


        public void scroll_off(object sender, ScrollEventArgs e)
        {
            this.ftext[int.Parse(((HScrollBar)sender).Name)].Text = ((HScrollBar)sender).Value.ToString();
        }

        public void scroll_home(object sender, ScrollEventArgs e)
        {
            this.ftext2[int.Parse(((HScrollBar)sender).Name)].Text = ((HScrollBar)sender).Value.ToString();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofdPic = new OpenFileDialog();
            ofdPic.Filter = "JPG(*.JPG;*.JPEG);gif文件(*.GIF);PNG(*.png)|*.jpg;*.jpeg;*.gif;*.png";
            ofdPic.FilterIndex = 1;
            ofdPic.RestoreDirectory = true;
            ofdPic.FileName = "";
            if (ofdPic.ShowDialog() == DialogResult.OK)
            {
                picfilename = Path.GetFullPath(ofdPic.FileName);
                string short_picfilename = Path.GetFileName(ofdPic.FileName);
                if (short_picfilename.Length < 25)
                    pic_loaded.Text = short_picfilename;
                else
                    pic_loaded.Text = short_picfilename.Substring(0, 22) + "...";
                newflag = true;
            }
            else
            {
                picfilename = null;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog mirror_file = new OpenFileDialog();
            mirror_file.Filter = "cfg(*.cfg;*.CFG)|*.cfg;*.CFG";
            mirror_file.FilterIndex = 1;
            mirror_file.RestoreDirectory = true;
            mirror_file.FileName = "";
            if (mirror_file.ShowDialog() == DialogResult.OK)
            {
                mirrorfilename = Path.GetFullPath(mirror_file.FileName);
                if (parseMirror() == false)
                {
                    mirrorfilename = null;
                    mirror.Clear();
                    MessageBox.Show(NewMotion_lang_dic["errorMsg23"]);
                }
                else
                {
                    string short_mirrorfilename = Path.GetFileName(mirror_file.FileName);
                    if (short_mirrorfilename.Length < 25)
                        mirror_loaded.Text = short_mirrorfilename;
                    else
                        mirror_loaded.Text = short_mirrorfilename.Substring(0, 22) + "...";
                }
            }
        }

        public bool parseMirror()
        {
            mirror.Clear();
            char[] delimiterChar = { '-' };
            if (mirrorfilename == null)
                return false;
            using (StreamReader reader = new StreamReader(mirrorfilename))
            {
                while (!reader.EndOfStream)
                {
                    string data = reader.ReadLine();
                    if (data.Length < 1)
                        continue;
                    if (data[0] == '#')
                        continue;
                    string[] datas = data.Split(delimiterChar);
                    if (datas.Length != 2)
                        return false;
                    try
                    {
                        mirror.Add(int.Parse(datas[0]), int.Parse(datas[1]));
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void motors_SelectedIndexChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < 45; i++)
            {
                if (sender.Equals(fbox[i]))
                {
                    switch(fbox[i].Text)
                    {
                        case "---noServo---":
                            fcheck[i].Enabled = false;
                            fcheck[i].Checked = false;
                            ftext3[i].Text = "600";
                            ftext4[i].Text = "2400";
                            break;
                        case "EMAX_ES08AII":
                            fcheck[i].Enabled = true;
                            ftext3[i].Text = "600";
                            ftext4[i].Text = "2700";
                            break;
                        case "EMAX_ES3104":
                            fcheck[i].Enabled = true;
                            ftext3[i].Text = "600";
                            ftext4[i].Text = "2350";
                            break;
                        case "KONDO_KRS786":
                            fcheck[i].Enabled = true;
                            ftext3[i].Text = "500";
                            ftext4[i].Text = "2500";
                            break;
                        case "KONDO_KRS788":
                            fcheck[i].Enabled = true;
                            ftext3[i].Text = "500";
                            ftext4[i].Text = "2500";
                            break;
                        case "KONDO_KRS78X":
                            fcheck[i].Enabled = true;
                            ftext3[i].Text = "500";
                            ftext4[i].Text = "2500";
                            break;
                        case "KONDO_KRS4014":
                            fcheck[i].Enabled = true;
                            ftext3[i].Text = "450";
                            ftext4[i].Text = "2500";
                            break;
                        case "KONDO_KRS4024":
                            fcheck[i].Enabled = true;
                            ftext3[i].Text = "630";
                            ftext4[i].Text = "2380";
                            break;
                        case "HITEC_HSR8498":
                            fcheck[i].Enabled = true;
                            ftext3[i].Text = "550";
                            ftext4[i].Text = "2450";
                            break;
                        case "FUTABA_S3003":
                            fcheck[i].Enabled = true;
                            ftext3[i].Text = "450";
                            ftext4[i].Text = "2350";
                            break;
                        case "SHAYYE_SYS214050":
                            fcheck[i].Enabled = true;
                            ftext3[i].Text = "600";
                            ftext4[i].Text = "2350";
                            break;
                        case "TOWERPRO_MG90S":
                            fcheck[i].Enabled = true;
                            ftext3[i].Text = "700";
                            ftext4[i].Text = "2600";
                            break;
                        case "TOWERPRO_MG995":
                            fcheck[i].Enabled = true;
                            ftext3[i].Text = "800";
                            ftext4[i].Text = "2200";
                            break;
                        case "TOWERPRO_MG996":
                            fcheck[i].Enabled = true;
                            ftext3[i].Text = "800";
                            ftext4[i].Text = "2200";
                            break;
                        case "TOWERPRO_SG90":
                            fcheck[i].Enabled = true;
                            ftext3[i].Text = "500";
                            ftext4[i].Text = "2500";
                            break;
                        case "DMP_RS0263":
                            fcheck[i].Enabled = true;
                            ftext3[i].Text = "700";
                            ftext4[i].Text = "2280";
                            break;
                        case "DMP_RS1270":
                            fcheck[i].Enabled = true;
                            ftext3[i].Text = "670";
                            ftext4[i].Text = "2230";
                            break;
                        case "GWS_S777":
                            fcheck[i].Enabled = true;
                            ftext3[i].Text = "600";
                            ftext4[i].Text = "2350";
                            break;
                        case "GWS_S03T":
                            fcheck[i].Enabled = true;
                            ftext3[i].Text = "580";
                            ftext4[i].Text = "2540";
                            break;
                        case "GWS_MICRO":
                            fcheck[i].Enabled = true;
                            ftext3[i].Text = "580";
                            ftext4[i].Text = "2540";
                            break;
                        case "OtherServos":
                            fcheck[i].Enabled = true;
                            ftext3[i].Text = "500";
                            ftext4[i].Text = "2500";
                            break;
                    }
                }
            }
        }

        private void check_offset(object sender, EventArgs e)
        {
            int n;
            int i = int.Parse(((MaskedTextBox)sender).Name);
            if (int.TryParse(((MaskedTextBox)sender).Text, out n))
            {
                if (n < -256)
                    ((MaskedTextBox)sender).Text = "-256";
                else if (n > 255)
                    ((MaskedTextBox)sender).Text = "255";
                else
                {
                    fbar_off[i].Value = n;
                    offset[i] = n;
                }
            }
        }

        private void check_homeframe(object sender, EventArgs e)
        {
            uint n;
            int i = int.Parse(((MaskedTextBox)sender).Name);

            if (uint.TryParse(((MaskedTextBox)sender).Text, out n))
            {
                if (n >= min[i] && n <= Max[i])
                {
                    fbar_home[i].Value = (int)n;
                    homeframe[i] = n;
                }
                else if (n > Max[i])
                {
                    ((MaskedTextBox)sender).Text = Max[i].ToString();
                    homeframe[i] = Max[i];
                    fbar_home[i].Value = (int)Max[i];
                }
            }
            else
            {
                ((MaskedTextBox)sender).Text = "1500";
                fbar_home[i].Value = 1500;
                homeframe[i] = 1500;
            }
        }

        private void check_range(object sender, EventArgs e)
        {
            int i = int.Parse(((MaskedTextBox)sender).Name);
            int _min = int.Parse(ftext3[i].Text);
            int _max = int.Parse(ftext4[i].Text);
            fbar_home[i].Minimum = _min;
            fbar_home[i].Maximum = _max + 9;
            min[i] = (uint)_min;
            Max[i] = (uint)_max;
        }

        private void check_pgain(object sender, EventArgs e)
        {
            double n;
            int i = int.Parse(((MaskedTextBox)sender).Name);

            if (double.TryParse(((MaskedTextBox)sender).Text, out n))
            {
                p_gain[i] = n;
            }
            else if (((MaskedTextBox)sender).Text == "-" || ((MaskedTextBox)sender).Text == "" ||
                     ((MaskedTextBox)sender).Text == "-." || ((MaskedTextBox)sender).Text == ".")
            {
                p_gain[i] = 0;
            }
            else
            {
                p_gain[i] = 0;
                ((MaskedTextBox)sender).Text = "0";
            }
        }

        private void check_sgain(object sender, EventArgs e)
        {
            double n;
            int i = int.Parse(((MaskedTextBox)sender).Name);

            if (double.TryParse(((MaskedTextBox)sender).Text, out n))
            {
                s_gain[i] = n;
            }
            else if (((MaskedTextBox)sender).Text == "-" || ((MaskedTextBox)sender).Text == "" ||
                     ((MaskedTextBox)sender).Text == "-." || ((MaskedTextBox)sender).Text == ".")
            {
                s_gain[i] = 0;
            }
            else
            {
                s_gain[i] = 0;
                ((MaskedTextBox)sender).Text = "0";
            }
        }

        private void init_imu_Click(object sender, EventArgs e)
        {
            Thread show_progress = new Thread(new ThreadStart(progress_thread));
            if (arduino != null)
            {
                try
                {
                    if (string.Compare(comboBox2.SelectedItem.ToString(), "On-Board IMU") == 0)
                    {
                        if (string.Compare(comboBox1.SelectedItem.ToString(), "86Duino One") == 0)
                            arduino.init_IMU(1);
                        else if (string.Compare(comboBox1.SelectedItem.ToString(), "86Duino AI") == 0)
                            arduino.init_IMU(3);
                    }
                    else if (string.Compare(comboBox2.SelectedItem.ToString(), "RoBoard RM-G146") == 0)
                        arduino.init_IMU(2);
                    else
                        arduino.init_IMU(0);
                    show_progress.Start();
                }
                catch
                {
                    MessageBox.Show(NewMotion_lang_dic["errorMsg1"]);
                }
            }
        }

        private void getQ_Click(object sender, EventArgs e)
        {
            lock (serial_lock)
            {
                if (arduino != null)
                {
                    try
                    {
                        Quaternion detectQ = new Quaternion();
                        detectQ.w = 0;
                        int avg_times = 8;
                        for (int i = 0; i < avg_times; i++)
                        {
                            arduino.getQ();
                            DateTime time_start = DateTime.Now;
                            while (!arduino.dataRecieved && (DateTime.Now - time_start).TotalMilliseconds < 100) ;
                            arduino.dataRecieved = false;
                            detectQ.w += arduino.quaternion[0];
                            detectQ.x += arduino.quaternion[1];
                            detectQ.y += arduino.quaternion[2];
                            detectQ.z += arduino.quaternion[3];
                            Thread.Sleep(33);
                        }
                        detectQ.w /= avg_times;
                        detectQ.x /= avg_times;
                        detectQ.y /= avg_times;
                        detectQ.z /= avg_times;
                        detectQ = detectQ.Normalized();
                        maskedTextBox1.Text = detectQ.w.ToString("0.####");
                        maskedTextBox2.Text = detectQ.x.ToString("0.####");
                        maskedTextBox3.Text = detectQ.y.ToString("0.####");
                        maskedTextBox4.Text = detectQ.z.ToString("0.####");
                    }
                    catch
                    {
                        MessageBox.Show(NewMotion_lang_dic["errorMsg1"]);
                    }
                }
            }
        }

        private void ShowProcessBar()
        {
            init_ProcessBar = new Progress();
            progress_Increase = new IncreaseHandle(init_ProcessBar.Increase);
            init_ProcessBar.ShowDialog();
            init_ProcessBar = null;
        }

        private void progress_thread()
        {
            lock (serial_lock)
            {
                MethodInvoker mi = new MethodInvoker(ShowProcessBar);
                this.BeginInvoke(mi);
                Thread.Sleep(100);
                bool blnIncreased = false;
                object objReturn = null;
                do
                {
                    if (comboBox2.SelectedIndex == 1)
                        Thread.Sleep(50);
                    else
                        Thread.Sleep(130);
                    objReturn = this.Invoke(this.progress_Increase, new object[] { 1 });
                    blnIncreased = (bool)objReturn;
                }
                while (blnIncreased);
                DateTime time_start = DateTime.Now;
                while (!arduino.dataRecieved && (DateTime.Now - time_start).TotalMilliseconds < 500) ;
                if (arduino.dataRecieved)
                {
                    if (arduino.captured_data != 0)
                    {
                        getQ.Enabled = false;
                        MessageBox.Show(this, NewMotion_lang_dic["errorMsg21"]);
                    }
                    else
                        getQ.Enabled = true;
                }
                else
                {
                    getQ.Enabled = false;
                    MessageBox.Show(this, NewMotion_lang_dic["errorMsg1"]);
                }
                arduino.dataRecieved = false;
            }
        }

        private void comboBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
        }
        void comboBox_MouseWheel(object sender, MouseEventArgs e)
        {
            ((HandledMouseEventArgs)e).Handled = true;
        }

        private void channelver_MouseEnter(object sender, EventArgs e)
        {
            channelver.Focus();
        }
    }
}
