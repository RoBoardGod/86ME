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
 about.cs - DM&P 86ME
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
using System.Windows.Forms;

namespace _86ME_ver2
{
    public partial class about : Form
    {
        public System.Diagnostics.Process p = new System.Diagnostics.Process();

        public about(Dictionary<string, string> lang_dic)
        {
            InitializeComponent();
            if (lang_dic.Count > 0)
            {
                richTextBox1.LanguageOption = RichTextBoxLanguageOptions.DualFont;
                this.Text = lang_dic["about_title"];
                richTextBox1.Text = lang_dic["content"];
                author_label.Text = lang_dic["author_label_Text"];
                license_label.Text = lang_dic["license_label_Text"];
                version_label.Text = lang_dic["version_label_Text"];
                richTextBox1.SelectionStart = richTextBox1.TextLength;
            }
        }

        private void richTextBox1_LinkClicked(object sender,
        System.Windows.Forms.LinkClickedEventArgs e)
        {
            p = System.Diagnostics.Process.Start(e.LinkText);
        }

        private void fb_button_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.facebook.com/groups/164926427017235/");
        }

        private void web_button_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.86duino.com");
        }

        private void github_button_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/RoBoardGod/86ME");
        }
    }
}
