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
 SetLanguage.cs - DM&P 86ME
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

using System.Collections.Generic;
using System.IO;

namespace _86ME_ver2
{
    class SetLanguage
    {
        public Dictionary<string, string> lang_dic = new Dictionary<string, string>();
        string language;
        public SetLanguage(string lang)
        {
            language = lang;
            read_ini();
        }
        void read_ini()
        {
            lang_dic.Clear();
            char[] delimiterChar = { '=' };
            using (StreamReader reader = new StreamReader(language))
            {
                while (!reader.EndOfStream)
                {
                    string data = reader.ReadLine();
                    if (data.Length < 1)
                        continue;
                    if (data[0] == '#')
                        continue;
                    string[] cmd = data.Split(delimiterChar);
                    string rstring = "";
                    for (int i = 1; i < cmd.Length; i++)
                    {
                        if (i != 1)
                            rstring += '=';
                        rstring += cmd[i];
                    }
                    lang_dic.Add(cmd[0], convertNewLine(rstring));
                }
            }
        }
        private string convertNewLine(string input)
        {
            string output = "";
            for (int i = 0; i < input.Length; i++ )
            {
                if (input[i] == '\\')
                {
                    if (i + 1 < input.Length)
                    {
                        ++i;
                        if (input[i] == 'n')
                            output += '\n';
                        else if (input[i] == 't')
                            output += '\t';
                    }
                }
                else
                    output += input[i];
            }
            return output;
        }
    }
}
