using Sunny.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace szzminer.Class
{
    class Miner
    {
        public static string coin;
        public static string minerBigName;
        public static string minerSmallName;
        public static string miningPool;
        public static string wallet;
        public static string worker;
        public static string argu;

        public static void startMiner(bool MinerDisplay,ref UIRichTextBox LogOutput)
        {
            try
            {
                Process minerProcess = new Process();
                minerProcess.StartInfo.FileName = Application.StartupPath+"\\miner\\" + minerBigName + "\\" + minerSmallName + ".exe";
                minerProcess.StartInfo.Arguments = string.Format(getArguments(), miningPool, wallet, worker, argu);
                LogOutput.AppendText("[" + DateTime.Now.ToLocalTime().ToString() + "] 开始挖矿，启动参数:"+ minerProcess.StartInfo.Arguments + "，若长时间无反应，请勾选显示原版内核查看错误提示\n");
                if (MinerDisplay)
                {
                    minerProcess.StartInfo.CreateNoWindow = false;
                }
                else
                {
                    minerProcess.StartInfo.CreateNoWindow = true;
                }
                minerProcess.StartInfo.UseShellExecute = false;
                minerProcess.StartInfo.EnvironmentVariables.Remove("NBDEV");
                minerProcess.StartInfo.EnvironmentVariables.Add("NBDEV", "#@@@TSAmlU3LTYLdf9NFpnQbkIpKVFex7gJvUmzC01xGSyw=");
                minerProcess.Start();
            }
            catch(Exception ex)
            {
                UIMessageBox.ShowError("错误:"+ex.ToString());
            }
        }
        static string getArguments()
        {
            StreamReader sr = File.OpenText(string.Format("miner\\{0}\\config_{1}.ini",minerBigName, coin));
            string argu = sr.ReadLine();
            return argu;
        }

        public static void stopMiner(ref UIRichTextBox LogOutput)
        {
            Process[] myProcesses = System.Diagnostics.Process.GetProcesses();
            foreach (System.Diagnostics.Process myProcess in myProcesses)
            {
                if (myProcess.ProcessName.ToLower().Contains(minerSmallName.ToLower()))
                {
                    myProcess.Kill();//强制关闭该程序
                    LogOutput.AppendText("[" + DateTime.Now.ToLocalTime().ToString() + "] 停止挖矿，结束进程:" + myProcess.ProcessName + ".exe\n");
                }
            }
        }
    }
}
