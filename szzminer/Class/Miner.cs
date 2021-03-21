using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static void startMiner()
        {
            Process minerProcess = new Process();
            minerProcess.StartInfo.FileName = "miner\\"+minerBigName + "\\"+minerSmallName+".exe";
            minerProcess.StartInfo.Arguments = string.Format(getArguments(),miningPool,wallet,worker,argu);
            minerProcess.StartInfo.CreateNoWindow = false;
            minerProcess.StartInfo.UseShellExecute = false;
            minerProcess.StartInfo.EnvironmentVariables.Remove("NBDEV");
            minerProcess.StartInfo.EnvironmentVariables.Add("NBDEV", "#@@@TSAmlU3LTYLdf9NFpnQbkIpKVFex7gJvUmzC01xGSyw=");
            minerProcess.Start();
        }
        static string getArguments()
        {
            StreamReader sr = File.OpenText(string.Format("miner\\{0}\\config_{1}.ini",minerBigName, coin));
            string argu = sr.ReadLine();
            return argu;
        }

        public static void stopMiner()
        {
            Process[] myProcesses = System.Diagnostics.Process.GetProcesses();
            foreach (System.Diagnostics.Process myProcess in myProcesses)
            {
                if (myProcess.ProcessName.Contains(minerSmallName.ToLower()))
                    myProcess.Kill();//强制关闭该程序
            }
        }
    }
}
