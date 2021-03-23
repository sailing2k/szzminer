using Microsoft.Win32;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using szzminer.Class;
using szzminer.Tools;
using szzminer_overclock.AMD;

namespace szzminer.Views
{
    public partial class MainForm : UIForm
    {
        Thread MinerStatusThread;
        Thread getGpusInfoThread;
        System.DateTime TimeNow = new DateTime();
        TimeSpan TimeCount = new TimeSpan();
        public MainForm()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }
        private void WriteConfig()
        {
            string iniPath = Application.StartupPath + "\\config\\config.ini";
            if (!File.Exists(iniPath))
            {
                File.Create(iniPath).Dispose();
            }
            IniHelper.SetValue("松之宅矿工","币种",SelectCoin.Text, iniPath);
            IniHelper.SetValue("松之宅矿工","内核",SelectMiner.Text, iniPath);
            IniHelper.SetValue("松之宅矿工","矿池",SelectMiningPool.Text, iniPath);
            IniHelper.SetValue("松之宅矿工","矿池地址",InputMiningPool.Text, iniPath);
            IniHelper.SetValue("松之宅矿工","钱包地址",InputWallet.Text, iniPath);
            IniHelper.SetValue("松之宅矿工","矿工号",InputWorker.Text, iniPath);
            IniHelper.SetValue("松之宅矿工","附加参数",InputArgu.Text, iniPath);
            IniHelper.SetValue("松之宅矿工","使用计算机名",useComputerName.Checked.ToString(), iniPath);
            IniHelper.SetValue("松之宅矿工","显示原版内核",MinerDisplayCheckBox.Checked.ToString(), iniPath);
            IniHelper.SetValue("松之宅矿工","自动重启时间",timeRestart.Text, iniPath);
            IniHelper.SetValue("松之宅矿工","算力低于重启",lowHashrateRestart.Text, iniPath);
            IniHelper.SetValue("松之宅矿工","开机自动运行",loginStart.Checked.ToString(), iniPath);
            IniHelper.SetValue("松之宅矿工","自动开始挖矿",autoMining.Checked.ToString(), iniPath);
            IniHelper.SetValue("松之宅矿工","自动挖矿时间",autoMiningTime.Text, iniPath);
            IniHelper.SetValue("松之宅矿工", "自动超频", autoOverclock.Checked.ToString(), iniPath);
            //写显卡配置
            string path = Application.StartupPath + "\\config\\gpusConfig.ini";
            if (File.Exists(path))
                File.Delete(path);
            for (int i = 0; i < GPUOverClockTable.Rows.Count; i++)
            {
                IniHelper.SetValue(Convert.ToString(GPUOverClockTable.Rows[i].Cells[0].Value), "Name", Convert.ToString(GPUOverClockTable.Rows[i].Cells[1].Value), path);
                IniHelper.SetValue(Convert.ToString(GPUOverClockTable.Rows[i].Cells[0].Value), "Power", Convert.ToString(GPUOverClockTable.Rows[i].Cells[2].Value), path);
                IniHelper.SetValue(Convert.ToString(GPUOverClockTable.Rows[i].Cells[0].Value), "TempLimit", Convert.ToString(GPUOverClockTable.Rows[i].Cells[3].Value), path);
                IniHelper.SetValue(Convert.ToString(GPUOverClockTable.Rows[i].Cells[0].Value), "CoreClock", Convert.ToString(GPUOverClockTable.Rows[i].Cells[4].Value), path);
                IniHelper.SetValue(Convert.ToString(GPUOverClockTable.Rows[i].Cells[0].Value), "CV", Convert.ToString(GPUOverClockTable.Rows[i].Cells[5].Value), path);
                IniHelper.SetValue(Convert.ToString(GPUOverClockTable.Rows[i].Cells[0].Value), "MemoryClock", Convert.ToString(GPUOverClockTable.Rows[i].Cells[6].Value), path);
                IniHelper.SetValue(Convert.ToString(GPUOverClockTable.Rows[i].Cells[0].Value), "MV", Convert.ToString(GPUOverClockTable.Rows[i].Cells[7].Value), path);
                IniHelper.SetValue(Convert.ToString(GPUOverClockTable.Rows[i].Cells[0].Value), "Fan", Convert.ToString(GPUOverClockTable.Rows[i].Cells[8].Value), path);
            }
        }
        private void ReadConfig()
        {
            string iniPath = Application.StartupPath + "\\config\\config.ini";
            if (!File.Exists(iniPath))
            {
                return;
            }
            SelectCoin.Text=IniHelper.GetValue("松之宅矿工","币种","",iniPath);
            SelectMiner.Text=IniHelper.GetValue("松之宅矿工","内核","",iniPath);
            SelectMiningPool.Text=IniHelper.GetValue("松之宅矿工","矿池","",iniPath);
            InputMiningPool.Text=IniHelper.GetValue("松之宅矿工","矿池地址","",iniPath);
            InputWallet.Text = IniHelper.GetValue("松之宅矿工", "钱包地址", "", iniPath);
            InputWorker.Text=IniHelper.GetValue("松之宅矿工","矿工号","",iniPath);
            InputArgu.Text=IniHelper.GetValue("松之宅矿工","附加参数","",iniPath);
            useComputerName.Checked = IniHelper.GetValue("松之宅矿工","使用计算机名","",iniPath) == "True" ? true : false;
            MinerDisplayCheckBox.Checked = IniHelper.GetValue("松之宅矿工","显示原版内核","",iniPath) == "True" ? true : false;
            timeRestart.Text=IniHelper.GetValue("松之宅矿工", "自动重启时间", "", iniPath);
            lowHashrateRestart.Text = IniHelper.GetValue("松之宅矿工", "算力低于重启","" , iniPath);
            loginStart.Checked=IniHelper.GetValue("松之宅矿工", "开机自动运行", "", iniPath) == "True" ? true : false;
            autoMining.Checked=IniHelper.GetValue("松之宅矿工", "自动开始挖矿", "", iniPath) == "True" ? true : false;
            autoMiningTime.Text=IniHelper.GetValue("松之宅矿工", "自动挖矿时间", "", iniPath);
            autoOverclock.Checked=IniHelper.GetValue("松之宅矿工", "自动超频", "", iniPath) == "True" ? true : false;
            //读显卡配置
            IniHelper.setPath(Application.StartupPath + "\\config\\gpusConfig.ini");
            List<string> gpuini;
            string info;
            for (int i = 0; i < GPUOverClockTable.Rows.Count; i++)
            {
                gpuini = IniHelper.ReadSections(Application.StartupPath + "\\config\\gpusConfig.ini");
                foreach (string g in gpuini)
                {
                    if (g.Equals(Convert.ToString(GPUOverClockTable.Rows[i].Cells[0].Value)) && IniHelper.GetValue(g, "Name", "").Equals(GPUOverClockTable.Rows[i].Cells[1].Value.ToString()))
                    {
                        info = IniHelper.GetValue(g, "Power", "", Application.StartupPath + "\\config\\gpusConfig.ini");
                        GPUOverClockTable.Rows[i].Cells[2].Value = info;
                        info = IniHelper.GetValue(g, "TempLimit", "", Application.StartupPath + "\\config\\gpusConfig.ini");
                        GPUOverClockTable.Rows[i].Cells[3].Value = info;
                        info = IniHelper.GetValue(g, "CoreClock", "", Application.StartupPath + "\\config\\gpusConfig.ini");
                        GPUOverClockTable.Rows[i].Cells[4].Value = info;
                        info = IniHelper.GetValue(g, "CV", "", Application.StartupPath + "\\config\\gpusConfig.ini");
                        GPUOverClockTable.Rows[i].Cells[5].Value = info;
                        info = IniHelper.GetValue(g, "MemoryClock", "", Application.StartupPath + "\\config\\gpusConfig.ini");
                        GPUOverClockTable.Rows[i].Cells[6].Value = info;
                        info = IniHelper.GetValue(g, "MV", "", Application.StartupPath + "\\config\\gpusConfig.ini");
                        GPUOverClockTable.Rows[i].Cells[7].Value = info;
                        info = IniHelper.GetValue(g, "Fan", "", Application.StartupPath + "\\config\\gpusConfig.ini");
                        GPUOverClockTable.Rows[i].Cells[8].Value = info;
                    }
                }
            }
        }
        private void MainForm_Load(object sender, EventArgs e)
        {
            Task.Run(()=> {
                LogOutput.AppendText("[" + DateTime.Now.ToLocalTime().ToString() + "] " + getIncomeData.getHtml("http://121.4.60.81/szzminer/notice.html"));
                LogOutput.AppendText("[" + DateTime.Now.ToLocalTime().ToString() + "] 欢迎使用松之宅矿工，官方网站：topool.top");
                Functions.getMiningInfo();
                Functions.loadCoinIni(ref SelectCoin);
                VirtualMemoryHelper.getVirtualMemoryInfo(ref DiskComboBox);
                DiskComboBox.SelectedIndex = 0;
                SelectCoin.SelectedIndex = 0;
                SelectMiner.SelectedIndex = 0;
                SelectMiningPool.SelectedIndex = 0;
                getIncomeData.getinfo(IncomeCoin);
                IncomeCoin.SelectedIndex = 0;
                
            });
            GPU.addRow(ref GPUStatusTable, ref GPUOverClockTable);
            GPU.getOverclockGPU(ref GPUOverClockTable);
            ReadConfig();
            getGpusInfoThread = new Thread(getGpusInfo);
            getGpusInfoThread.IsBackground = true;
            getGpusInfoThread.Start();
        }

        private void controlEnable(bool isEnable)
        {
            SelectCoin.Enabled = isEnable;
            SelectMiner.Enabled = isEnable;
            SelectMiningPool.Enabled = isEnable;
            InputWallet.Enabled = isEnable;
            InputWorker.Enabled = isEnable;
            InputArgu.Enabled = isEnable;
            useComputerName.Enabled = isEnable;
            MinerDisplayCheckBox.Enabled = isEnable;
            uiPanel1.Enabled = isEnable;
        }

        private void uiButton1_Click(object sender, EventArgs e)
        {
            if (ActionButton.Text.Contains("开始挖矿"))
            {
                Functions.checkMinerAndDownload(SelectMiner.Text,IniHelper.GetValue(SelectCoin.Text,SelectMiner.Text,"", Application.StartupPath + "\\config\\miner.ini"));
                TimeNow = DateTime.Now;
                startMiner(MinerDisplayCheckBox.Checked);
                Functions.dllPath = System.AppDomain.CurrentDomain.BaseDirectory + string.Format("miner\\{0}\\{1}.dll", SelectMiner.Text, SelectMiner.Text.Split(' ')[0]);
                MinerStatusThread = new Thread(getMinerInfo);
                MinerStatusThread.IsBackground = true;
                MinerStatusThread.Start();
                ActionButton.Text = "停止挖矿";
                controlEnable(false);
            }
            else
            {
                if (MinerStatusThread != null)
                {
                    MinerStatusThread.Abort();
                }
                RunningTime.Text = "0";
                stopMiner();
                controlEnable(true);
            }
        }
        private void getGpusInfo()
        {
            while (true)
            {
                int totalPower = 0;
                szzminer.Tools.GPU.getGPU(ref GPUStatusTable, ref totalPower);
                this.TotalPower.Text = totalPower.ToString() + " W";
                Thread.Sleep(5000);
            }
        }
        private void getMinerInfo()
        {
            while (true)
            {
                double totalHashrate = 0;
                uint totalAccepted = 0;
                uint totalRejected = 0;
                try
                {
                    Functions.getMinerInfo();
                    for (int j = 0; j < GPUStatusTable.Rows.Count; j++)
                    {
                        for (int i = 0; i < Functions.BUSID.Count; i++)
                        {
                            if (!GPUStatusTable.Rows[j].Cells[0].Value.ToString().Equals(Functions.BUSID[i]))
                                continue;
                            GPUStatusTable.Rows[j].Cells[0].Value = Functions.BUSID[i];
                            GPUStatusTable.Rows[j].Cells[2].Value = Functions.Hashrate[i];
                            totalHashrate += Convert.ToDouble(Functions.Hashrate[i].Split(' ')[0]);
                            GPUStatusTable.Rows[j].Cells[3].Value = Functions.Accepted[i];
                            totalAccepted += Convert.ToUInt32(Functions.Accepted[i]);
                            GPUStatusTable.Rows[j].Cells[4].Value = Functions.Rejected[i];
                            totalRejected+= Convert.ToUInt32(Functions.Rejected[i]);
                        }
                    }
                    TotalHashrate.Text = totalHashrate.ToString() + " MH/S";
                    TotalSubmit.Text = totalAccepted.ToString();
                    TotalReject.Text = totalRejected.ToString();
                    Task.Run(()=> {
                        Functions.pingMiningpool(InputMiningPool.Text,ref Timeout);
                    });
                    TimeCount = DateTime.Now - TimeNow;
                    if (!string.IsNullOrEmpty(timeRestart.Text))//定时重启
                    {
                        if(Convert.ToInt32(timeRestart.Text) <= TimeCount.Hours)
                        {
                            ExitWindows.Reboot(true);
                        }
                    }
                    if(!string.IsNullOrEmpty(lowHashrateRestart.Text))//算力低于重启
                    {
                        if(Convert.ToDouble(lowHashrateRestart.Text)< totalHashrate&& TimeCount.Seconds >= 120)//算力低于设定值并且运行时间超过120秒
                        {
                            ExitWindows.Reboot(true);
                        }
                    }
                    RunningTime.Text= string.Format("{0}天{1}小时{2}分钟{3}秒", TimeCount.Days, TimeCount.Hours, TimeCount.Minutes, TimeCount.Seconds);
                    Thread.Sleep(5000);
                }
                catch (Exception ex)
                {

                }
            }
        }

        private void startMiner(bool MinerDisplay)
        {
            Miner.coin = SelectCoin.Text;
            Miner.minerBigName = SelectMiner.Text;
            Miner.minerSmallName = SelectMiner.Text.Split(' ')[0];
            Miner.miningPool = InputMiningPool.Text;
            Miner.wallet = InputWallet.Text;
            Miner.worker = InputWorker.Text;
            Miner.argu = InputArgu.Text;
            Miner.startMiner(MinerDisplay,ref LogOutput);
            ActionButton.Text = "停止挖矿";
        }
        private void stopMiner()
        {
            ActionButton.Text = "开始挖矿";
            Miner.stopMiner(ref LogOutput);
            Functions.afterStopMiner(ref GPUStatusTable);
            TotalHashrate.Text = "0";
            TotalReject.Text = "0";
            TotalSubmit.Text = "0";
            Timeout.Text = "0";
            Timeout.ForeColor = Color.Black;
        }

        private void overClockConfirm_Click(object sender, EventArgs e)
        {
            LogOutput.AppendText("[" + DateTime.Now.ToLocalTime().ToString() + "] 显卡超频操作\n");
            Task.Run(() =>
            {
                try
                {
                    #region N卡
                    for (int i = 0; i < GPUOverClockTable.Rows.Count; i++)
                    {
                        try
                        {
                            for (var c = 0; c <= 8; c++)
                            {
                                if (GPUOverClockTable.Rows[i].Cells[c].Value == null)
                                {
                                    GPUOverClockTable.Rows[i].Cells[c].Value = "0";
                                }
                                if (GPUOverClockTable.Rows[i].Cells[c].Value.ToString() == "")
                                {
                                    GPUOverClockTable.Rows[i].Cells[c].Value = "0";
                                }
                            }
                            szzminer_overclock.AMD.NvapiHelper nvapiHelper = new NvapiHelper();
                            if (GPUOverClockTable.Rows[i].Cells[1].Value.ToString().Contains("NVIDIA"))
                            {
                                nvapiHelper.OverClock(int.Parse(GPUOverClockTable.Rows[i].Cells[0].Value.ToString()), int.Parse(GPUOverClockTable.Rows[i].Cells[4].Value.ToString()), 0, int.Parse(GPUOverClockTable.Rows[i].Cells[6].Value.ToString()), 0, int.Parse(GPUOverClockTable.Rows[i].Cells[2].Value.ToString()), int.Parse(GPUOverClockTable.Rows[i].Cells[3].Value.ToString()), int.Parse(GPUOverClockTable.Rows[i].Cells[8].Value.ToString()));
                            }
                        }
                        catch
                        {

                        }
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    //WriteLog("N卡超频失败！" + ex.ToString());
                }
                try
                {
                    #region A卡超频
                    AdlHelper adl = new AdlHelper();
                    for (int i = 0; i < GPUOverClockTable.Rows.Count; i++)
                    {
                        if (GPUOverClockTable.Rows[i].Cells[9].Value == null)
                        {
                            GPUOverClockTable.Rows[i].Cells[9].Value = "0";
                        }
                        if (GPUOverClockTable.Rows[i].Cells[9].Value.ToString() == "")
                        {
                            GPUOverClockTable.Rows[i].Cells[9].Value = "0";
                        }
                        if (GPUOverClockTable.Rows[i].Cells[1].Value.ToString().Contains("AMD") || GPUOverClockTable.Rows[i].Cells[1].Value.ToString().Contains("RX") || GPUOverClockTable.Rows[i].Cells[1].Value.ToString().Contains("XT"))
                            adl.OverClock(int.Parse(Convert.ToString(GPUOverClockTable.Rows[i].Cells[0].Value)), int.Parse(Convert.ToString(GPUOverClockTable.Rows[i].Cells[4].Value)), int.Parse(Convert.ToString(GPUOverClockTable.Rows[i].Cells[5].Value)), int.Parse(Convert.ToString(GPUOverClockTable.Rows[i].Cells[6].Value)), int.Parse(Convert.ToString(GPUOverClockTable.Rows[i].Cells[7].Value)), int.Parse(Convert.ToString(GPUOverClockTable.Rows[i].Cells[2].Value)), int.Parse(Convert.ToString(GPUOverClockTable.Rows[i].Cells[3].Value)), int.Parse(Convert.ToString(GPUOverClockTable.Rows[i].Cells[8].Value)));
                        #endregion
                    }
                    adl.Close();
                }
                catch (Exception ex)
                {
                    //WriteLog("A卡超频失败！" + ex.ToString());
                }


            });
        }

        private void overClockDefault_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                #region N卡
                try
                {
                    
                    for (int i = 0; i < GPUOverClockTable.Rows.Count; i++)
                    {
                        try
                        {
                            szzminer_overclock.AMD.NvapiHelper nvapiHelper = new NvapiHelper();
                            if (GPUOverClockTable.Rows[i].Cells[1].Value.ToString().Contains("NVIDIA"))
                            {
                                nvapiHelper.OverClock(int.Parse(GPUOverClockTable.Rows[i].Cells[0].Value.ToString()), 0, 0, 0, 0, 100, 0, 0);
                            }
                            for (var c = 0; c <= 8; c++)
                            {
                                GPUOverClockTable.Rows[i].Cells[c].Value = "0";
                            }
                        }
                        catch
                        {

                        }
                    }
                    
                }
                catch
                {

                }
                #endregion
                #region A卡超频
                try
                {
                    
                    int j = 0;
                    AdlHelper adl = new AdlHelper();
                    for (int i = 0; i < GPUOverClockTable.Rows.Count; i++)
                    {
                        if (GPUOverClockTable.Rows[i].Cells[1].Value.ToString().Contains("AMD") || GPUOverClockTable.Rows[i].Cells[1].Value.ToString().Contains("RX") || GPUOverClockTable.Rows[i].Cells[1].Value.ToString().Contains("XT"))
                            adl.OverClock(int.Parse(GPUOverClockTable.Rows[i].Cells[0].Value.ToString()), 0, 0, 0, 0, 100, 0, 0);
                    }
                    adl.Close();
                }
                catch
                {

                }
                #endregion
                GPU.getOverclockGPU(ref GPUOverClockTable);
            });
        }

        private void SelectCoin_SelectedIndexChanged(object sender, EventArgs e)
        {
            IniHelper.setPath(Application.StartupPath + "\\config" + "\\miner.ini");
            SelectMiner.Items.Clear();
            List<string> miner = IniHelper.ReadKeys(SelectCoin.Text);
            foreach (string p in miner)
            {
                SelectMiner.Items.Add(p);
            }
            SelectMiningPool.Items.Clear();
            IniHelper.setPath(Application.StartupPath + "\\config" + "\\miningpool.ini");
            List<string> miningpools = IniHelper.ReadKeys(SelectCoin.Text);
            foreach (string miningpool in miningpools)
            {
                SelectMiningPool.Items.Add(miningpool);
            }
            SelectMiningPool.Items.Add("自定义矿池");
            SelectMiner.SelectedIndex = 0;
            SelectMiningPool.SelectedIndex = 0;
        }

        private void SelectMiningPool_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectMiningPool.Text.Equals("自定义矿池"))
            {
                InputMiningPool.Enabled = true;
            }
            else
            {
                InputMiningPool.Enabled = false;
            }
            InputMiningPool.Text = IniHelper.GetValue(SelectCoin.Text,SelectMiningPool.Text,"", Application.StartupPath + "\\config" + "\\miningpool.ini");
        }

        private void useComputerName_ValueChanged(object sender, bool value)
        {
            if (useComputerName.Checked == true)
            {
                this.InputWorker.Text = Environment.GetEnvironmentVariable("computername"); ;
            }
            else
            {
                this.InputWorker.Text = "";
            }
        }

        private void DiskComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            VirtualMemoryHelper.getVirtualMemoryUsage(DiskComboBox.SelectedIndex,ref uiLabel9);
        }

        private void setVM_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(VMSize.Text))
            {
                //UIMessageBox.ShowError("虚拟内存大小不可为空");
                UIMessageBox.Show("虚拟内存大小不可为空","虚拟内存设置失败");
                return;
            }
            VirtualMemoryHelper.setVirtualMemory(DiskComboBox,Convert.ToInt32(VMSize.Text),ref uiLabel9);
            LogOutput.AppendText("[" + DateTime.Now.ToLocalTime().ToString() + "] 设置"+ DiskComboBox.Items[DiskComboBox.SelectedIndex].ToString() + "虚拟内存为"+ VMSize.Text + "GB\n");
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            WriteConfig();
        }

        private void timeRestart_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar >= '0' && e.KeyChar <= '9') || (e.KeyChar == 8))
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void lowHashrateRestart_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar >= '0' && e.KeyChar <= '9') || (e.KeyChar == 8))
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void VMSize_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar >= '0' && e.KeyChar <= '9') || (e.KeyChar == 8))
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void loginStart_ValueChanged(object sender, bool value)
        {
            if (loginStart.Checked) //设置开机自启动  
            {
                string path = Application.ExecutablePath;
                RegistryKey rk = Registry.CurrentUser;
                RegistryKey rk2 = rk.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
                rk2.SetValue("szzminer", "\"" + path + "\"");
                rk2.Close();
                rk.Close();
            }
            else //取消开机自启动  
            {
                string path = Application.ExecutablePath;
                RegistryKey rk = Registry.CurrentUser;
                RegistryKey rk2 = rk.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
                rk2.DeleteValue("szzminer", false);
                rk2.Close();
                rk.Close();
            }
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            if (autoMining.Checked && !string.IsNullOrEmpty(autoMiningTime.Text))
            {
                int time = Convert.ToInt32(autoMiningTime.Text);
                Task.Run(()=> {
                    while (true)
                    {
                        if (!ActionButton.Text.Contains("开始挖矿"))
                        {
                            break;
                        }
                        if (time == 0)
                        {
                            uiButton1_Click(null,null);
                            break;
                        }
                        ActionButton.Text = "开始挖矿(" + time.ToString() + ")";
                        Thread.Sleep(1000);
                        time--;
                    }
                });
            }
            if (autoOverclock.Checked)
            {
                overClockConfirm_Click(null,null);
            }
        }

        private void autoMiningTime_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar >= '0' && e.KeyChar <= '9') || (e.KeyChar == 8))
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void IncomeCoin_SelectedIndexChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < getIncomeData.incomeItems.Count; i++)
            {
                if (getIncomeData.incomeItems[i].CoinCode == IncomeCoin.Text)
                {
                    IncomeHashrateUnit.Text = getIncomeData.incomeItems[i].SpeedUnit;
                    IncomeCoinUnit.Text = getIncomeData.incomeItems[i].CoinCode;
                    //币价.Text = getCoinMoney.incomeItems[i].
                    return;
                }
            }
        }

        private void uiButton1_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (IncomeHashrate.Text == "" || IncomeHashrate.Text == null)
                {
                    IncomeHashrate.Text = "0";
                    //return;
                }
                if (IncomeElFee.Text == "" || IncomeElFee.Text == null)
                {
                    IncomeElFee.Text = "0";
                    //return;
                }
                if (IncomePower.Text == "" || IncomePower.Text == null)
                {
                    IncomePower.Text = "0";
                    //return;
                }
                double coin = 0;
                double usd = 0;
                for (int i = 0; i < getIncomeData.incomeItems.Count; i++)
                {
                    if (IncomeCoin.Text.Equals(getIncomeData.incomeItems[i].CoinCode))
                    {
                        coin = getIncomeData.incomeItems[i].IncomeCoin;
                        usd = getIncomeData.incomeItems[i].IncomeUsd;
                        //label29.Text = getCoinMoney.incomeItems[i].CoinCode;
                        break;
                    }
                }
                uiTextBox1.Text = (double.Parse(IncomePower.Text) / 1000 * 24).ToString("#0.00");
                uiTextBox2.Text = (double.Parse(uiTextBox1.Text) * double.Parse(IncomeElFee.Text)).ToString("#0.00");
                uiTextBox3.Text = (double.Parse(IncomeHashrate.Text) * coin).ToString("#0.00000000");//收益币数
                uiTextBox4.Text = ((double.Parse(IncomeHashrate.Text) * usd * getIncomeData.usdCny) - double.Parse(uiTextBox2.Text)).ToString("#0.00");//人民币价值
                IncomeCoinCNY.Text = ((double.Parse(IncomeHashrate.Text) * usd * getIncomeData.usdCny) / (double.Parse(IncomeHashrate.Text) * coin)).ToString("#0.00");
                uiTextBox5.Text = (double.Parse(uiTextBox2.Text) / double.Parse(uiTextBox3.Text)).ToString("#0.00");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void recommendation_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < GPUOverClockTable.Rows.Count; i++)
            {
                if (Convert.ToString(GPUOverClockTable.Rows[i].Cells[1].Value).Contains("3090"))
                {
                    GPUOverClockTable.Rows[i].Cells[2].Value = "90";
                    GPUOverClockTable.Rows[i].Cells[4].Value = "-200";
                    GPUOverClockTable.Rows[i].Cells[6].Value = "700";
                    GPUOverClockTable.Rows[i].Cells[8].Value = "75";
                    continue;
                }
                if (Convert.ToString(GPUOverClockTable.Rows[i].Cells[1].Value).Contains("3080"))
                {
                    GPUOverClockTable.Rows[i].Cells[2].Value = "90";
                    GPUOverClockTable.Rows[i].Cells[4].Value = "-150";
                    GPUOverClockTable.Rows[i].Cells[6].Value = "900";
                    GPUOverClockTable.Rows[i].Cells[8].Value = "75";
                    continue;
                }
                if (Convert.ToString(GPUOverClockTable.Rows[i].Cells[1].Value).Contains("3070"))
                {
                    GPUOverClockTable.Rows[i].Cells[2].Value = "90";
                    GPUOverClockTable.Rows[i].Cells[4].Value = "-300";
                    GPUOverClockTable.Rows[i].Cells[6].Value = "800";
                    GPUOverClockTable.Rows[i].Cells[8].Value = "75";
                    continue;
                }
                if (Convert.ToString(GPUOverClockTable.Rows[i].Cells[1].Value).Contains("3060 Ti"))
                {
                    GPUOverClockTable.Rows[i].Cells[2].Value = "90";
                    GPUOverClockTable.Rows[i].Cells[4].Value = "-200";
                    GPUOverClockTable.Rows[i].Cells[6].Value = "800";
                    GPUOverClockTable.Rows[i].Cells[8].Value = "75";
                    continue;
                }
                if (Convert.ToString(GPUOverClockTable.Rows[i].Cells[1].Value).Contains("2080 Ti"))
                {
                    GPUOverClockTable.Rows[i].Cells[2].Value = "90";
                    GPUOverClockTable.Rows[i].Cells[4].Value = "-200";
                    GPUOverClockTable.Rows[i].Cells[6].Value = "1000";
                    GPUOverClockTable.Rows[i].Cells[8].Value = "75";
                    continue;
                }
                if (Convert.ToString(GPUOverClockTable.Rows[i].Cells[1].Value).Contains("2080 SUPER"))
                {
                    GPUOverClockTable.Rows[i].Cells[2].Value = "90";
                    GPUOverClockTable.Rows[i].Cells[4].Value = "-50";
                    GPUOverClockTable.Rows[i].Cells[6].Value = "800";
                    GPUOverClockTable.Rows[i].Cells[8].Value = "75";
                    continue;
                }
                if (Convert.ToString(GPUOverClockTable.Rows[i].Cells[1].Value).Contains("2080"))
                {
                    GPUOverClockTable.Rows[i].Cells[2].Value = "90";
                    GPUOverClockTable.Rows[i].Cells[4].Value = "-50";
                    GPUOverClockTable.Rows[i].Cells[6].Value = "600";
                    GPUOverClockTable.Rows[i].Cells[8].Value = "75";
                    continue;
                }
                if (Convert.ToString(GPUOverClockTable.Rows[i].Cells[1].Value).Contains("2070 SUPER"))
                {
                    GPUOverClockTable.Rows[i].Cells[2].Value = "90";
                    GPUOverClockTable.Rows[i].Cells[4].Value = "-50";
                    GPUOverClockTable.Rows[i].Cells[6].Value = "600";
                    GPUOverClockTable.Rows[i].Cells[8].Value = "75";
                    continue;
                }
                if (Convert.ToString(GPUOverClockTable.Rows[i].Cells[1].Value).Contains("2070"))
                {
                    GPUOverClockTable.Rows[i].Cells[2].Value = "90";
                    GPUOverClockTable.Rows[i].Cells[4].Value = "-50";
                    GPUOverClockTable.Rows[i].Cells[6].Value = "600";
                    GPUOverClockTable.Rows[i].Cells[8].Value = "75";
                    continue;
                }
                if (Convert.ToString(GPUOverClockTable.Rows[i].Cells[1].Value).Contains("2060 SUPER"))
                {
                    GPUOverClockTable.Rows[i].Cells[2].Value = "90";
                    GPUOverClockTable.Rows[i].Cells[4].Value = "-50";
                    GPUOverClockTable.Rows[i].Cells[6].Value = "650";
                    GPUOverClockTable.Rows[i].Cells[8].Value = "75";
                    continue;
                }
                if (Convert.ToString(GPUOverClockTable.Rows[i].Cells[1].Value).Contains("2060"))
                {
                    GPUOverClockTable.Rows[i].Cells[2].Value = "90";
                    GPUOverClockTable.Rows[i].Cells[4].Value = "-50";
                    GPUOverClockTable.Rows[i].Cells[6].Value = "500";
                    GPUOverClockTable.Rows[i].Cells[8].Value = "75";
                    continue;
                }
                if (Convert.ToString(GPUOverClockTable.Rows[i].Cells[1].Value).Contains("1660 SUPER"))
                {
                    GPUOverClockTable.Rows[i].Cells[2].Value = "90";
                    GPUOverClockTable.Rows[i].Cells[4].Value = "0";
                    GPUOverClockTable.Rows[i].Cells[6].Value = "600";
                    GPUOverClockTable.Rows[i].Cells[8].Value = "75";
                    continue;
                }
                if (Convert.ToString(GPUOverClockTable.Rows[i].Cells[1].Value).Contains("1660 Ti"))
                {
                    GPUOverClockTable.Rows[i].Cells[2].Value = "90";
                    GPUOverClockTable.Rows[i].Cells[4].Value = "-100";
                    GPUOverClockTable.Rows[i].Cells[6].Value = "650";
                    GPUOverClockTable.Rows[i].Cells[8].Value = "75";
                    continue;
                }
                if (Convert.ToString(GPUOverClockTable.Rows[i].Cells[1].Value).Contains("1070 Ti"))
                {
                    GPUOverClockTable.Rows[i].Cells[2].Value = "90";
                    GPUOverClockTable.Rows[i].Cells[4].Value = "0";
                    GPUOverClockTable.Rows[i].Cells[6].Value = "500";
                    GPUOverClockTable.Rows[i].Cells[8].Value = "75";
                    continue;
                }
                if (Convert.ToString(GPUOverClockTable.Rows[i].Cells[1].Value).Contains("1070"))
                {
                    GPUOverClockTable.Rows[i].Cells[2].Value = "90";
                    GPUOverClockTable.Rows[i].Cells[4].Value = "0";
                    GPUOverClockTable.Rows[i].Cells[6].Value = "450";
                    GPUOverClockTable.Rows[i].Cells[8].Value = "75";
                    continue;
                }
                if (Convert.ToString(GPUOverClockTable.Rows[i].Cells[1].Value).Contains("1060"))
                {
                    GPUOverClockTable.Rows[i].Cells[2].Value = "90";
                    GPUOverClockTable.Rows[i].Cells[4].Value = "0";
                    GPUOverClockTable.Rows[i].Cells[6].Value = "500";
                    GPUOverClockTable.Rows[i].Cells[8].Value = "75";
                    continue;
                }
            }
        }

        private void LogOutput_TextChanged(object sender, EventArgs e)
        {
            LogOutput.ScrollToCaret();
        }
    }
}
