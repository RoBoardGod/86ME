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
 Progress.cs - DM&P 86ME
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

using System.Windows.Forms;
using System.Threading;

namespace _86ME_ver2
{
    public partial class Progress : Form
    {
        public Progress()
        {
            InitializeComponent();
        }

        public bool Increase(int nValue)
        {
            if (nValue > 0)
            {
                if (progressBar1.Value + nValue < progressBar1.Maximum)
                {
                    progressBar1.Value += nValue;
                    return true;
                }
                else
                {
                    progressBar1.Maximum = 101;
                    progressBar1.Value = 101;
                    progressBar1.Maximum = 100;
                    progressBar1.Value = 100;
                    Thread.Sleep(900);
                    this.Close();
                    return false;
                }
            }
            return false;
        }
    }
}
