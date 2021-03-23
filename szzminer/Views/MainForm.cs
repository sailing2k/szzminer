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
        }
        private void MainForm_Load(object sender, EventArgs e)
        {
            Task.Run(()=> {
                Functions.getMiningInfo();
                Functions.loadCoinIni(ref SelectCoin);
                VirtualMemoryHelper.getVirtualMemoryInfo(ref DiskComboBox);
                DiskComboBox.SelectedIndex = 0;
                SelectCoin.SelectedIndex = 0;
                SelectMiner.SelectedIndex = 0;
                SelectMiningPool.SelectedIndex = 0;
                ReadConfig();
            });
            GPU.addRow(ref GPUStatusTable, ref GPUOverClockTable);
            GPU.getOverclockGPU(ref GPUOverClockTable);
            getGpusInfoThread = new Thread(getGpusInfo);
            getGpusInfoThread.IsBackground = true;
            getGpusInfoThread.Start();
            LogOutput.AppendText("[" + DateTime.Now.ToLocalTime().ToString() + "] 欢迎使用松之宅矿工，官方网站：topool.top\n");
        }

        private void controlEnable(bool isEnable)
        {
            SelectCoin.Enabled = isEnable;
            SelectMiner.Enabled = isEnable;
            SelectMiningPool.Enabled = isEnable;
            InputWallet.Enabled = isEnable;
            InputWorker.Enabled = isEnable;
            InputArgu.Enabled = isEnable;
        }

        private void uiButton1_Click(object sender, EventArgs e)
        {
            if (ActionButton.Text.Equals("开始挖矿"))
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
            Miner.startMiner(MinerDisplay);
            ActionButton.Text = "停止挖矿";
        }
        private void stopMiner()
        {
            ActionButton.Text = "开始挖矿";
            Miner.stopMiner();
            Functions.afterStopMiner(ref GPUStatusTable);
            TotalHashrate.Text = "0";
            TotalReject.Text = "0";
            TotalSubmit.Text = "0";
            Timeout.Text = "0";
            Timeout.ForeColor = Color.Black;
        }

        private void overClockConfirm_Click(object sender, EventArgs e)
        {
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
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            WriteConfig();
        }
    }
}
