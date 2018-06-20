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
 ScratchProperty.cs - DM&P 86ME
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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _86ME_ver2
{
    enum firmata_method { serial, bluetooth, wifi, ethernet, esp8266, esp8266AP, last};

    public partial class ScratchProperty : Form
    {
        Dictionary<string, string> sp_lang_dic;
        public bool[] method;
        public string project_name;
        public string bt_baud;
        public string bt_serial;
        public string esp_baud;
        public string esp_serial;
        public string esp_ssid;
        public string esp_password;
        public string espAP_baud;
        public string espAP_serial;
        public string espAP_ssid;
        public string espAP_password;
        public string wifi_ssid;
        public string wifi_password;
        public string port;
        public string path;
        public string readme;
        int lh = 26;
        public ComboBox espCHPDComboBox = new ComboBox();
        

        public ScratchProperty(Dictionary<string, string> lang_dic, bool develop_mode = false)
        {
            InitializeComponent();
            this.btBaudComboBox.SelectedIndex = 0;
            this.btPortComboBox.SelectedIndex = 0;
            this.espBaudComboBox.SelectedIndex = 4;
            this.espPortComboBox.SelectedIndex = 0;
            this.espAPBaudComboBox.SelectedIndex = 4;
            this.espAPPortComboBox.SelectedIndex = 0;
            for (int i = 0; i < 44; i++)
                espCHPDComboBox.Items.Add(i);
            this.espCHPDComboBox.SelectedIndex = 10;

            this.pathMaskedTextBox.Text = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            this.sp_lang_dic = lang_dic;

            method = new bool[(int)firmata_method.last];
            for (int i = 0; i < (int)firmata_method.last; i++)
                method[i] = false;
            Size s = this.Size;
            this.MaximumSize = new Size(s.Width, s.Height - lh * (develop_mode ? 6 : 7));
            this.MinimumSize = new Size(s.Width, s.Height - lh * (develop_mode ? 6 : 7));
            WificheckBox.Visible = develop_mode;
            project_name = nameTextBox.Text;
        }
        private void ScratchProperty_Load(object sender, EventArgs e)
        {
            SerialcheckBox.Checked = true;
            btcheckBox.Checked = false;
            EthernetcheckBox.Checked = false;
            WificheckBox.Checked = false;
            espcheckBox.Checked = false;
            espAPcheckBox.Checked = false;
        }

        private void numbercheck(object sender, KeyPressEventArgs e)
        {
            if (((int)e.KeyChar < 48 | (int)e.KeyChar > 57) & (int)e.KeyChar != 8)
                e.Handled = true;
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog path = new FolderBrowserDialog();
            path.Description = sp_lang_dic["GenerateScratch_Description"];
            var dialogResult = path.ShowDialog();
            if (!Directory.Exists(path.SelectedPath) || dialogResult != DialogResult.OK)
                return;

            this.pathMaskedTextBox.Text = path.SelectedPath;
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void OK_button_Click(object sender, EventArgs e)
        {
            /*
            char c = nameTextBox.Text[0];
            if (!(c >= 'a' && c <= 'z') && !(c >= 'A' && c <= 'Z'))
            {
                MessageBox.Show("Invalid project name, the first character should be alphabetic.", "Error");
                return;
            }
            */
            nameTextBox.Text = nameTextBox.Text.Replace(' ', '_');
            if (SerialcheckBox.Checked)
            {
                method[(int)firmata_method.serial] = true;
            }
            if (btcheckBox.Checked)
            {
                method[(int)firmata_method.bluetooth] = true;
                bt_baud = btBaudComboBox.Text;
                bt_serial = btPortComboBox.Text;
            }
            if (WificheckBox.Checked)
            {
                method[(int)firmata_method.wifi] = true;
                wifi_ssid = WifiSSIDTextBox.Text;
                wifi_password = WifiPWTextBox.Text;
            }
            if (EthernetcheckBox.Checked)
            {
                method[(int)firmata_method.ethernet] = true;
            }
            if (espcheckBox.Checked)
            {
                method[(int)firmata_method.esp8266] = true;
                esp_baud = espBaudComboBox.Text;
                esp_serial = espPortComboBox.Text;
                esp_ssid = espSSIDTextBox.Text;
                esp_password = espPWTextBox.Text;
            }
            if (espAPcheckBox.Checked)
            {
                method[(int)firmata_method.esp8266AP] = true;
                espAP_baud = espAPBaudComboBox.Text;
                espAP_serial = espAPPortComboBox.Text;
                espAP_ssid = espAPSSIDTextBox.Text;
                espAP_password = espAPPWTextBox.Text;
            }
            project_name = nameTextBox.Text;
            readme = IntroductionTextBox.Text;

            int tryPort;
            if (int.TryParse(portTextBox.Text, out tryPort) == false)
            {
                MessageBox.Show("Invalid port number, please check again.", "Error");
                return;
            }
            else if (tryPort > 65535 || tryPort < 1)
            {
                MessageBox.Show("Invalid port number, please check again.", "Error");
                return;
            }
            else
            {
                port = tryPort.ToString();
            }

            if (!Directory.Exists(pathMaskedTextBox.Text))
            {
                MessageBox.Show("Invalid path, please check again.", "Error");
                return;
            }
            else
            {
                path = pathMaskedTextBox.Text;
            }
            if (!Directory.Exists(path + "\\" + project_name))
            {
                this.DialogResult = DialogResult.OK;
            }
            else
            {

                DialogResult file_exit = MessageBox.Show(project_name + "already exists.\nDo you want to replace it?", "Project exists", MessageBoxButtons.YesNo);
                if (file_exit == DialogResult.Yes)
                {
                    this.DialogResult = DialogResult.OK;
                }
                else if (file_exit == DialogResult.No)
                {
                    return;
                }
            }
        }

        private void SerialcheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (SerialcheckBox.Checked)
            {

            }
            else
            {


            }
        }

        private void btcheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Size s = this.Size;
            if (btcheckBox.Checked)
            {
                btBaudLabel.Visible = true;
                btBaudComboBox.Visible = true;
                btPortLabel.Visible = true;
                btPortComboBox.Visible = true;
                this.MaximumSize = new Size(s.Width, s.Height + lh);
                this.MinimumSize = new Size(s.Width, s.Height + lh);

            }
            else
            {
                btBaudLabel.Visible = false;
                btBaudComboBox.Visible = false;
                btPortLabel.Visible = false;
                btPortComboBox.Visible = false;
                this.MaximumSize = new Size(s.Width, s.Height - lh);
                this.MinimumSize = new Size(s.Width, s.Height - lh);
            }
        }

        private void EthernetcheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (EthernetcheckBox.Checked)
            {

            }
            else
            {


            }

        }

        private void WificheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Size s = this.Size;
            if (WificheckBox.Checked)
            {
                WifiSSIDLabel.Visible = true;
                WifiSSIDTextBox.Visible = true;
                WifiPWLabel.Visible = true;
                WifiPWTextBox.Visible = true;
                Wifilabel.Visible = true;
                this.MaximumSize = new Size(s.Width, s.Height + lh);
                this.MinimumSize = new Size(s.Width, s.Height + lh);
            }
            else
            {
                WifiSSIDLabel.Visible = false;
                WifiSSIDTextBox.Visible = false;
                WifiPWLabel.Visible = false;
                WifiPWTextBox.Visible = false;
                Wifilabel.Visible = false;
                this.MaximumSize = new Size(s.Width, s.Height - lh);
                this.MinimumSize = new Size(s.Width, s.Height - lh);
            }

        }

        private void espcheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Size s = this.Size;
            if (espcheckBox.Checked)
            {
                espBaudLabel.Visible = true;
                espBaudComboBox.Visible = true;
                espPortLabel.Visible = true;
                espPortComboBox.Visible = true;
                espSSIDLabel.Visible = true;
                espSSIDTextBox.Visible = true;
                espPWLabel.Visible = true;
                espPWTextBox.Visible = true;
                esplabel.Visible = true;
                this.MaximumSize = new Size(s.Width, s.Height + lh * 2);
                this.MinimumSize = new Size(s.Width, s.Height + lh * 2);
            }
            else
            {
                espBaudLabel.Visible = false;
                espBaudComboBox.Visible = false;
                espPortLabel.Visible = false;
                espPortComboBox.Visible = false;
                espSSIDLabel.Visible = false;
                espSSIDTextBox.Visible = false;
                espPWLabel.Visible = false;
                espPWTextBox.Visible = false;
                esplabel.Visible = false;
                this.MaximumSize = new Size(s.Width, s.Height - lh * 2);
                this.MinimumSize = new Size(s.Width, s.Height - lh * 2);
            }

        }

        private void espAPcheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Size s = this.Size;
            if (espAPcheckBox.Checked)
            {
                espAPBaudLabel.Visible = true;
                espAPBaudComboBox.Visible = true;
                espAPPortLabel.Visible = true;
                espAPPortComboBox.Visible = true;
                espAPSSIDLabel.Visible = true;
                espAPSSIDTextBox.Visible = true;
                espAPPWLabel.Visible = true;
                espAPPWTextBox.Visible = true;
                espAPlabel.Visible = true;
                this.MaximumSize = new Size(s.Width, s.Height + lh * 2);
                this.MinimumSize = new Size(s.Width, s.Height + lh * 2);
            }
            else
            {
                espAPBaudLabel.Visible = false;
                espAPBaudComboBox.Visible = false;
                espAPPortLabel.Visible = false;
                espAPPortComboBox.Visible = false;
                espAPSSIDLabel.Visible = false;
                espAPSSIDTextBox.Visible = false;
                espAPPWLabel.Visible = false;
                espAPPWTextBox.Visible = false;
                espAPlabel.Visible = false;
                this.MaximumSize = new Size(s.Width, s.Height - lh * 2);
                this.MinimumSize = new Size(s.Width, s.Height - lh * 2);
            }

        }

        private void nameTextBox_TextChanged(object sender, EventArgs e)
        {
            if(project_name == espAPSSIDTextBox.Text)
            {
                project_name = nameTextBox.Text;
                espAPSSIDTextBox.Text = project_name;
            }
        }
    }
}
    