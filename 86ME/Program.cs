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
 Program.cs - DM&P 86ME
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
using System.Windows.Forms;
using System.IO;

namespace _86ME_ver2
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
      {
            string lang = "en";
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            string locale_file = Application.StartupPath + "\\locale.ini";
            if (File.Exists(locale_file))
            {
                StreamReader reader = new StreamReader(locale_file);
                char[] delimiterChars = { ' ', '\t', '\r', '\n' };
                string[] datas = reader.ReadToEnd().Split(delimiterChars);
                reader.Dispose();
                reader.Close();
                if (datas.Length > 0)
                    lang = datas[0];
                if (!(String.Compare(lang, "en") == 0 || String.Compare(lang, "zh-TW") == 0 || String.Compare(lang, "zh-Hans")==0))
                    lang = "en";
            }
            lang = Application.StartupPath + "\\locales\\" + lang + ".ini";
            SetLanguage sl = new SetLanguage(lang);
            SetForm sform = new SetForm(sl.lang_dic);
            var execute = sform.ShowDialog();
            if (execute == DialogResult.Yes)
            {
                Main f1;
                if (args.Length > 0)
                    f1 = new Main(args[0], sl.lang_dic);
                else
                    f1 = new Main(sl.lang_dic);
                f1.com_port = sform.com_port;
                f1.connect_comport();
                Application.Run(f1);
            }
        }
    }
}
