using Sunny.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using szzminer.Views;

namespace szzminer
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }
            catch(Exception ex)
            {
                UIMessageBox.Show("程序发生严重错误！错误："+ex.ToString(),"程序崩溃");
            }
        }
    }
}
