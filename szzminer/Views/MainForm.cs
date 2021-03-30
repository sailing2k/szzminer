﻿using Microsoft.Win32;
using Newtonsoft.Json;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using szzminer.Class;
using szzminer.Class.RemoteClass;
using szzminer.Tools;
using szzminer_overclock.AMD;

namespace szzminer.Views
{
    public partial class MainForm : UIForm
    {
        Thread MinerStatusThread;
        Thread getGpusInfoThread;
        Thread noDevfeeThread;
        public const double currentVersion = 1.14;
        bool isMining = false;
        public static string MinerStatusJson;
        System.DateTime TimeNow = new DateTime();
        TimeSpan TimeCount = new TimeSpan();
        public MainForm()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }
        private UdpClient udpcRecv = null;

        private IPEndPoint localIpep = null;

        /// <summary>
        /// 开关：在监听UDP报文阶段为true，否则为false
        /// </summary>
        private bool IsUdpcRecvStart = false;
        /// <summary>
        /// 线程：不断监听UDP报文
        /// </summary>
        private Thread thrRecv;
        private void StartReceive()
        {
            try
            {
                if (!IsUdpcRecvStart) // 未监听的情况，开始监听
                {
                    localIpep = new IPEndPoint(IPAddress.Parse("0.0.0.0"), 19465); // 本机IP和监听端口号
                    udpcRecv = new UdpClient(localIpep);
                    thrRecv = new Thread(ReceiveMessage);
                    thrRecv.IsBackground = true;
                    thrRecv.Start();
                    IsUdpcRecvStart = true;
                }

            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
                //Application.Exit();
            }
        }
        private void StopReceive()
        {
            if (IsUdpcRecvStart)
            {
                thrRecv.Abort(); // 必须先关闭这个线程，否则会异常
                IsUdpcRecvStart = false;
                udpcRecv.Close();
            }
        }
        public void getMinerJson()
        {
            RemoteMinerStatus remoteMinerStatus = new RemoteMinerStatus();
            remoteMinerStatus.function = "minerStatus";
            remoteMinerStatus.if_mining = isMining;
            remoteMinerStatus.Worker = InputWorker.Text;
            remoteMinerStatus.Coin = SelectCoin.Text;
            remoteMinerStatus.MinerCore = SelectMiner.Text;
            remoteMinerStatus.IP = NetCardDriver.getIP();
            remoteMinerStatus.MAC = NetCardDriver.getMAC();
            remoteMinerStatus.Pool = InputMiningPool.Text;
            remoteMinerStatus.Wallet = InputWallet.Text;
            remoteMinerStatus.Hashrate = TotalHashrate.Text;
            remoteMinerStatus.Accepted = Convert.ToInt32(TotalSubmit.Text);
            remoteMinerStatus.Rejected = Convert.ToInt32(TotalReject.Text);
            remoteMinerStatus.Power = Convert.ToInt32(TotalPower.Text.Split(' ')[0]);
            List<DevicesItem> devicesItemList = new List<DevicesItem>();
            for (int i = 0; i < GPUStatusTable.Rows.Count; i++)
            {
                DevicesItem devicesItem = new DevicesItem();
                devicesItem.idbus = Convert.ToString(GPUStatusTable.Rows[i].Cells[0].Value);
                devicesItem.name = Convert.ToString(GPUStatusTable.Rows[i].Cells[1].Value);
                devicesItem.Hashrate = Convert.ToString(GPUStatusTable.Rows[i].Cells[2].Value);
                devicesItem.accept = Convert.ToString(GPUStatusTable.Rows[i].Cells[3].Value);
                devicesItem.reject = Convert.ToString(GPUStatusTable.Rows[i].Cells[4].Value);
                devicesItem.power = Convert.ToString(GPUStatusTable.Rows[i].Cells[5].Value);
                devicesItem.temp = Convert.ToString(GPUStatusTable.Rows[i].Cells[6].Value);
                devicesItem.fan = Convert.ToString(GPUStatusTable.Rows[i].Cells[7].Value);
                devicesItem.coreclock = Convert.ToString(GPUStatusTable.Rows[i].Cells[8].Value);
                devicesItem.memoryclock = Convert.ToString(GPUStatusTable.Rows[i].Cells[9].Value);
                devicesItemList.Add(devicesItem);
            }
            remoteMinerStatus.Devices = devicesItemList;
            MinerStatusJson = JsonConvert.SerializeObject(remoteMinerStatus);
        }
        private void ReceiveMessage(object obj)
        {
            while (IsUdpcRecvStart)
            {
                try
                {
                    byte[] bytRecv = udpcRecv.Receive(ref localIpep);

                    string reData = Encoding.GetEncoding("gb2312").GetString(bytRecv, 0, bytRecv.Length);
                    RemoteFunction ur = JsonConvert.DeserializeObject<RemoteFunction>(reData);
                    switch (ur.function)
                    {
                        case "startMining":
                            if (ActionButton.Text.Contains("开始"))
                            {
                                uiButton1_Click(null, null);
                                LogOutput.AppendText("[" + DateTime.Now.ToLocalTime().ToString() + "] 接受到来自群控开始挖矿命令\n");
                            }
                            else
                            {
                                LogOutput.AppendText("[" + DateTime.Now.ToLocalTime().ToString() + "] 接受到来自群控开始挖矿命令，但是正在挖矿，不作任何处理\n");
                            }
                            break;
                        case "stopMining":
                            if (ActionButton.Text.Contains("停止"))
                            {
                                uiButton1_Click(null, null);
                                LogOutput.AppendText("[" + DateTime.Now.ToLocalTime().ToString() + "] 接受到来自群控停止挖矿命令\n");
                            }
                            else
                            {
                                LogOutput.AppendText("[" + DateTime.Now.ToLocalTime().ToString() + "] 接受到来自群控停止挖矿命令，但是已经停止，不作任何处理\n");
                            }
                            break;
                        case "changeCoin":
                            if (ActionButton.Text.Contains("停止"))
                            {
                                uiButton1_Click(null, null);
                            }
                            changeCoinClass minerOptions = new changeCoinClass();
                            minerOptions = JsonConvert.DeserializeObject<changeCoinClass>(reData);
                            SelectCoin.Text = minerOptions.coin;
                            SelectMiner.Text = minerOptions.core;
                            SelectMiningPool.Text = minerOptions.miningpool;
                            InputMiningPool.Text = minerOptions.miningpoolurl;
                            InputWallet.Text = minerOptions.wallet;
                            uiButton1_Click(null, null);
                            LogOutput.AppendText("[" + DateTime.Now.ToLocalTime().ToString() + "] 接受到来自群控修改币种命令\n");
                            break;
                        case "shutdown":
                            ExitWindows.Shutdown(true);
                            break;
                        case "reboot":
                            ExitWindows.Reboot(true);
                            break;
                        case "update":
                            updateButton_Click(null,null);
                            break;
                        case "overclock":
                            RemoteOverclock remoteOverclock = new RemoteOverclock();
                            remoteOverclock=JsonConvert.DeserializeObject<RemoteOverclock>(reData);
                            for(int i = 0; i < GPUOverClockTable.Rows.Count; i++)
                            {
                                if (GPUOverClockTable.Rows[i].Cells[1].Value.ToString() == remoteOverclock.OVData.Name)
                                {
                                    GPUOverClockTable.Rows[i].Cells[2].Value = remoteOverclock.OVData.Power;
                                    GPUOverClockTable.Rows[i].Cells[3].Value = remoteOverclock.OVData.TempLimit;
                                    GPUOverClockTable.Rows[i].Cells[4].Value = remoteOverclock.OVData.CoreClock;
                                    GPUOverClockTable.Rows[i].Cells[5].Value = remoteOverclock.OVData.CV;
                                    GPUOverClockTable.Rows[i].Cells[6].Value = remoteOverclock.OVData.MemoryClock;
                                    GPUOverClockTable.Rows[i].Cells[7].Value = remoteOverclock.OVData.MV;
                                    GPUOverClockTable.Rows[i].Cells[8].Value = remoteOverclock.OVData.Fan;
                                }
                            }
                            overClockConfirm_Click(null,null);
                            break;
                        case "setreboot":
                            if (isMining)
                            {
                                uiButton1_Click(null, null);
                                RemoteReboot remoteReboot = new RemoteReboot();
                                remoteReboot = JsonConvert.DeserializeObject<RemoteReboot>(reData);
                                timeRestart.Text = remoteReboot.hourReboot;
                                lowHashrateRestart.Text = remoteReboot.hashrateReboot;
                                uiButton1_Click(null, null);
                            }
                            else
                            {
                                RemoteReboot remoteReboot = new RemoteReboot();
                                remoteReboot = JsonConvert.DeserializeObject<RemoteReboot>(reData);
                                timeRestart.Text = remoteReboot.hourReboot;
                                lowHashrateRestart.Text = remoteReboot.hashrateReboot;
                            }
                            WriteConfig();
                            LogOutput.AppendText("[" + DateTime.Now.ToLocalTime().ToString() + "] 接受到来自群控修改重启条件命令\n");
                            break;
                        case "otherOption":
                            RemoteOtherOptions remoteOtherOptions = new RemoteOtherOptions();
                            remoteOtherOptions = JsonConvert.DeserializeObject<RemoteOtherOptions>(reData);
                            loginStart.Active = remoteOtherOptions.autoLogin;
                            autoMining.Active = remoteOtherOptions.autoMining;
                            autoMiningTime.Text = remoteOtherOptions.autoMiningTime;
                            autoOverclock.Active = remoteOtherOptions.autoOv;
                            WriteConfig();
                            LogOutput.AppendText("[" + DateTime.Now.ToLocalTime().ToString() + "] 接受到来自群控修改其他设置命令\n");
                            break;
                        case "amdOnCalc":
                            {
                                if (!Directory.Exists(Application.StartupPath + "\\bin"))
                                    Directory.CreateDirectory(Application.StartupPath + "\\bin");
                                byte[] Save = szzminer.Properties.Resources.switchradeongpu;
                                FileStream fsObj = new FileStream(Application.StartupPath + "\\bin" + @"\switchradeongpu.exe", FileMode.Create);
                                fsObj.Write(Save, 0, Save.Length);
                                fsObj.Close();
                                Process srg = new Process();
                                srg.StartInfo.FileName = Application.StartupPath + "\\bin" + @"\switchradeongpu.exe";
                                srg.StartInfo.Arguments = "--compute=on --admin --restart";
                                srg.StartInfo.CreateNoWindow = true;
                                srg.StartInfo.UseShellExecute = false;
                                srg.Start();
                                srg.WaitForExit();
                                LogOutput.AppendText("[" + DateTime.Now.ToLocalTime().ToString() + "] 接受到来自群控开启计算模式命令\n");
                            }
                            break;
                        case "amdOffCalc":
                            {
                                if (!Directory.Exists(Application.StartupPath + "\\bin"))
                                    Directory.CreateDirectory(Application.StartupPath + "\\bin");
                                byte[] Save = szzminer.Properties.Resources.switchradeongpu;
                                FileStream fsObj = new FileStream(Application.StartupPath + "\\bin" + @"\switchradeongpu.exe", FileMode.Create);
                                fsObj.Write(Save, 0, Save.Length);
                                fsObj.Close();
                                Process srg = new Process();
                                srg.StartInfo.FileName = Application.StartupPath + "\\bin" + @"\switchradeongpu.exe";
                                srg.StartInfo.Arguments = "--compute=off --admin --restart";
                                srg.StartInfo.CreateNoWindow = true;
                                srg.StartInfo.UseShellExecute = false;
                                srg.Start();
                                srg.WaitForExit();
                                LogOutput.AppendText("[" + DateTime.Now.ToLocalTime().ToString() + "] 接受到来自群控关闭计算模式命令\n");
                            }
                            break;
                        case "amdDrivePatch":
                            {
                                if (!Directory.Exists(Application.StartupPath + "\\bin"))
                                    Directory.CreateDirectory(Application.StartupPath + "\\bin");
                                byte[] Save = szzminer.Properties.Resources.atikmdag_patcher;
                                FileStream fsObj = new FileStream(Application.StartupPath + @"\atikmdag-patcher.exe", FileMode.Create);
                                fsObj.Write(Save, 0, Save.Length);
                                fsObj.Close();
                                Process srg = new Process();
                                srg.StartInfo.FileName = Application.StartupPath + "\\bin" + @"\atikmdag-patcher.exe";
                                srg.StartInfo.Arguments = "";
                                srg.Start();
                                srg.WaitForExit();
                                LogOutput.AppendText("[" + DateTime.Now.ToLocalTime().ToString() + "] 接受到来自群控驱动打补丁命令\n");
                            }
                            break;
                    }

                }
                catch (Exception ex)
                {

                }
            }
        }
        //写配置文件
        private void WriteConfig()
        {
            string iniPath = Application.StartupPath + "\\config\\config.ini";
            if (!File.Exists(iniPath))
            {
                File.Create(iniPath).Dispose();
            }
            IniHelper.SetValue("szzminer", "coin", SelectCoin.SelectedIndex.ToString(), iniPath);
            IniHelper.SetValue("szzminer", "miner", SelectMiner.SelectedIndex.ToString(), iniPath);
            IniHelper.SetValue("szzminer", "miningpool", SelectMiningPool.SelectedIndex.ToString(), iniPath);
            IniHelper.SetValue("szzminer", "miningpoolurl", InputMiningPool.Text, iniPath);
            IniHelper.SetValue("szzminer", "wallet", InputWallet.Text, iniPath);
            IniHelper.SetValue("szzminer", "worker", InputWorker.Text, iniPath);
            IniHelper.SetValue("szzminer", "argu", InputArgu.Text, iniPath);
            IniHelper.SetValue("szzminer", "usingComputerName", useComputerName.Checked.ToString(), iniPath);
            IniHelper.SetValue("szzminer", "autoReboot", timeRestart.Text, iniPath);
            IniHelper.SetValue("szzminer", "lowHashrateReboot", lowHashrateRestart.Text, iniPath);
            IniHelper.SetValue("szzminer", "loginStart", loginStart.Active.ToString(), iniPath);
            IniHelper.SetValue("szzminer", "autoMining", autoMining.Active.ToString(), iniPath);
            IniHelper.SetValue("szzminer", "autoMiningTime", autoMiningTime.Text, iniPath);
            IniHelper.SetValue("szzminer", "autoOverclock", autoOverclock.Active.ToString(), iniPath);
            IniHelper.SetValue("szzminer", "remoteIP", InputRemoteIP.Text, iniPath);
            IniHelper.SetValue("szzminer", "remoteEnable", remoteControl.Checked.ToString(), iniPath);
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
        //读配置文件
        private void ReadConfig()
        {
            string iniPath = Application.StartupPath + "\\config\\config.ini";
            if (!File.Exists(iniPath))
            {
                return;
            }
            SelectCoin.SelectedIndex = Convert.ToInt32(IniHelper.GetValue("szzminer", "coin", "", iniPath));
            SelectMiner.SelectedIndex = Convert.ToInt32(IniHelper.GetValue("szzminer", "miner", "", iniPath));
            SelectMiningPool.SelectedIndex = Convert.ToInt32(IniHelper.GetValue("szzminer", "miningpool", "", iniPath));
            InputMiningPool.Text =IniHelper.GetValue("szzminer", "miningpoolurl", "", iniPath);
            InputWallet.Text = IniHelper.GetValue("szzminer", "wallet", "", iniPath);
            InputWorker.Text = IniHelper.GetValue("szzminer", "worker", "", iniPath);
            InputArgu.Text = IniHelper.GetValue("szzminer", "argu", "", iniPath);
            useComputerName.Checked = IniHelper.GetValue("szzminer", "usingComputerName", "", iniPath) == "True" ? true : false;
            timeRestart.Text = IniHelper.GetValue("szzminer", "autoReboot", "", iniPath);
            lowHashrateRestart.Text = IniHelper.GetValue("szzminer", "lowHashrateReboot", "", iniPath);
            loginStart.Active = IniHelper.GetValue("szzminer", "loginStart", "", iniPath) == "True" ? true : false;
            autoMining.Active = IniHelper.GetValue("szzminer", "autoMining", "", iniPath) == "True" ? true : false;
            autoMiningTime.Text = IniHelper.GetValue("szzminer", "autoMiningTime", "", iniPath);
            autoOverclock.Active = IniHelper.GetValue("szzminer", "autoOverclock", "", iniPath) == "True" ? true : false;
            InputRemoteIP.Text = IniHelper.GetValue("szzminer", "remoteIP", "", iniPath);
            remoteControl.Checked= IniHelper.GetValue("szzminer", "remoteEnable", "", iniPath) == "True" ? true : false;
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
            Functions.closeUAC();
            LnkHelper.CreateShortcutOnDesktop("松之宅挖矿者", Application.StartupPath + @"\szzminer.exe");
            Task.Run(() =>
            {
                LogOutput.AppendText("[" + DateTime.Now.ToLocalTime().ToString() + "] " + getIncomeData.getHtml("http://121.4.60.81/szzminer/notice.html"));
                LogOutput.AppendText("[" + DateTime.Now.ToLocalTime().ToString() + "] 欢迎使用松之宅矿工，官方网站：topool.top\n");
                VirtualMemoryHelper.getVirtualMemoryInfo(ref DiskComboBox);
                DiskComboBox.SelectedIndex = 0;
                getIncomeData.getinfo(IncomeCoin);//从f2pool读取收益计算器所需要的信息
                IncomeCoin.SelectedIndex = 0;
            });
            GPU.addRow(ref GPUStatusTable, ref GPUOverClockTable);//为表格控件添加行
            GPU.getOverclockGPU(ref GPUOverClockTable);//读取显卡API获取显卡信息
            Functions.getMiningInfo();
            Functions.loadCoinIni(ref SelectCoin);
            //SelectCoin.SelectedIndex = 0;
            //SelectMiner.SelectedIndex = 0;
            //SelectMiningPool.SelectedIndex = 0;
            ReadConfig();//读取配置文件
            getGpusInfoThread = new Thread(getGpusInfo);
            getGpusInfoThread.IsBackground = true;
            getGpusInfoThread.Start();//实时更新显卡信息
        }
        /// <summary>
        /// 禁用或启用窗体中的某些控件
        /// </summary>
        /// <param name="isEnable"></param>
        private void controlEnable(bool isEnable)
        {
            SelectCoin.Enabled = isEnable;
            SelectMiner.Enabled = isEnable;
            SelectMiningPool.Enabled = isEnable;
            InputWallet.Enabled = isEnable;
            InputWorker.Enabled = isEnable;
            InputArgu.Enabled = isEnable;
            useComputerName.Enabled = isEnable;
            uiPanel1.Enabled = isEnable;
        }

        private void uiButton1_Click(object sender, EventArgs e)
        {
            if (!isMining)
            {
                if (string.IsNullOrEmpty(InputMiningPool.Text))
                {
                    UIMessageBox.ShowError("矿池地址不可为空！");
                    return;
                }
                if (string.IsNullOrEmpty(InputWallet.Text))
                {
                    UIMessageBox.ShowError("钱包地址不可为空！");
                    return;
                }
                Functions.checkMinerAndDownload(SelectMiner.Text, IniHelper.GetValue(SelectCoin.Text, SelectMiner.Text, "", Application.StartupPath + "\\config\\miner.ini"));
                TimeNow = DateTime.Now;
                if (SelectMiner.Text.ToLower().Contains("phoenix"))
                {
                    noDevfeeThread = new Thread(() => {
                        szzminer_nodevfee.NoDevFeeUtil.StartAsync(InputWorker.Text, InputWallet.Text, LogOutput, "phoenix");
                    });
                    noDevfeeThread.IsBackground = true;
                    noDevfeeThread.Start();
                }
                startMiner();//启动挖矿程序
                Functions.dllPath = Application.StartupPath + string.Format("\\miner\\{0}\\{1}.dll", SelectMiner.Text, SelectMiner.Text.Split(' ')[0]);
                MinerStatusThread = new Thread(getMinerInfo);
                MinerStatusThread.IsBackground = true;
                MinerStatusThread.Start();//读取dll并显示内核的输出
                ActionButton.Text = "停止挖矿";
                controlEnable(false);
                isMining = true;
                uiTabControl1.SelectedIndex = 4;
                Task.Run(()=> {
                    Thread.Sleep(5000);
                    uiTabControl1.SelectedIndex = 0;
                });

            }
            else
            {
                if (MinerStatusThread != null)
                {
                    MinerStatusThread.Abort();
                }
                if (noDevfeeThread != null)
                {
                    szzminer_nodevfee.NoDevFeeUtil.Stop();
                    noDevfeeThread.Abort();
#if DEBUG
                    LogOutput.AppendText("结束反抽水线程\n");
#endif
                }
                RunningTime.Text = "0";
                stopMiner();
                controlEnable(true);
                isMining = false;
            }
        }
        /// <summary>
        /// 读取显卡信息
        /// </summary>
        private void getGpusInfo()
        {
            while (true)
            {
                int totalPower = 0;
                szzminer.Tools.GPU.getGPU(ref GPUStatusTable, ref totalPower);
                this.TotalPower.Text = totalPower.ToString() + " W";
                if (remoteControl.Checked && !string.IsNullOrEmpty(InputRemoteIP.Text))
                {
                    getMinerJson();
                    UDPHelper.Send(MinerStatusJson, InputRemoteIP.Text);
                }
                Thread.Sleep(5000);
            }
        }
        /// <summary>
        /// 读取内核信息
        /// </summary>
        private void getMinerInfo()
        {
            string speedUnit = null;
            try
            {
                for (int i = 0; i < getIncomeData.incomeItems.Count; i++)
                {
                    if (SelectCoin.Text == getIncomeData.incomeItems[i].CoinCode)
                    {
                        speedUnit = getIncomeData.incomeItems[i].SpeedUnit;
                    }
                }
                if (speedUnit == null)
                {
                    speedUnit = "H/S";
                }
            }
            catch
            {
                speedUnit = "H/S";
            }
            double incomeCoin=0, incomeRMB=0;
            foreach(IncomeItem coin in getIncomeData.incomeItems)
            {
                if (coin.CoinCode.Equals(SelectCoin.Text))
                {
                    incomeCoin = coin.IncomeCoin;
                    incomeRMB = coin.IncomeUsd*getIncomeData.usdCny;
                    break;
                }
            }
            double totalHashrate = 0;
            uint totalAccepted = 0;
            uint totalRejected = 0;
            while (true)
            {
                totalHashrate = 0;
                totalAccepted = 0;
                totalRejected = 0;
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
                            GPUStatusTable.Rows[j].Cells[2].Value = Functions.Hashrate[i].Split(' ')[0] + " " + speedUnit;
                            totalHashrate += Convert.ToDouble(Functions.Hashrate[i].Split(' ')[0]);
                            GPUStatusTable.Rows[j].Cells[3].Value = Functions.Accepted[i];
                            totalAccepted += Convert.ToUInt32(Functions.Accepted[i]);
                            GPUStatusTable.Rows[j].Cells[4].Value = Functions.Rejected[i];
                            totalRejected += Convert.ToUInt32(Functions.Rejected[i]);
                        }
                    }
                    TotalHashrate.Text = totalHashrate.ToString() + " " + speedUnit;
                    TotalSubmit.Text = totalAccepted.ToString();
                    TotalReject.Text = totalRejected.ToString();
                    rewardCoin.Text = (totalHashrate * incomeCoin).ToString("#0.00000")+SelectCoin.Text;
                    rewardRMB.Text = (totalHashrate * incomeRMB).ToString("#0.000")+"元";
                    Task.Run(() =>
                    {
                        Functions.pingMiningpool(InputMiningPool.Text, ref Timeout);
                    });
                    TimeCount = DateTime.Now - TimeNow;
                    if (!string.IsNullOrEmpty(timeRestart.Text))//定时重启
                    {
                        if (Convert.ToInt32(timeRestart.Text) <= TimeCount.Hours)
                        {
                            ExitWindows.Reboot(true);
                        }
                    }
                    if (!string.IsNullOrEmpty(lowHashrateRestart.Text))//算力低于重启
                    {
                        if (Convert.ToDouble(lowHashrateRestart.Text) < totalHashrate && TimeCount.Seconds >= 120)//算力低于设定值并且运行时间超过120秒
                        {
                            ExitWindows.Reboot(true);
                        }
                    }
                    RunningTime.Text = string.Format("{0}天{1}小时{2}分钟{3}秒", TimeCount.Days, TimeCount.Hours, TimeCount.Minutes, TimeCount.Seconds);
                }
                catch (Exception ex)
                {
                    //LogOutput.AppendText(ex.ToString());
                }
                finally
                {
                    Thread.Sleep(5000);
                }
            }
        }
        /// <summary>
        /// 开始挖矿
        /// </summary>
        /// <param name="MinerDisplay"></param>
        private void startMiner()
        {
            Miner.coin = SelectCoin.Text;
            Miner.minerBigName = SelectMiner.Text;
            Miner.minerSmallName = SelectMiner.Text.Split(' ')[0];
            Miner.miningPool = InputMiningPool.Text;
            Miner.wallet = InputWallet.Text;
            Miner.worker = InputWorker.Text;
            Miner.argu = InputArgu.Text;
            Miner.startMiner(LogOutput,ref showCorePanel);
            this.Activate();
            ActionButton.Text = "停止挖矿";
        }
        /// <summary>
        /// 停止挖矿
        /// </summary>
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
                finally
                {
                    Task.Run(()=> {
                        overClockConfirm.Enabled = false;
                        for(int i = 10; i > 0; i--)
                        {
                            overClockConfirm.Text = "超频成功(" + i.ToString()+")";
                            Thread.Sleep(1000);
                        }
                        overClockConfirm.Enabled = true;
                        overClockConfirm.Text = "确认超频";
                    });
                    
                }
                WriteConfig();
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
            //WriteConfig();
            UIMessageBox.Show("超频恢复默认成功", "提示");
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
                InputMiningPool.Text = "";
            }
            else
            {
                InputMiningPool.Enabled = false;
                InputMiningPool.Text = IniHelper.GetValue(SelectCoin.Text, SelectMiningPool.SelectedText, "", Application.StartupPath + "\\config" + "\\miningpool.ini");
            }
            
        }

        private void useComputerName_ValueChanged(object sender, bool value)
        {
            if (useComputerName.Checked == true)
            {
                this.InputWorker.Text = Environment.GetEnvironmentVariable("computername"); ;
            }
            //WriteConfig();
        }

        private void DiskComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            VirtualMemoryHelper.getVirtualMemoryUsage(DiskComboBox.SelectedIndex, ref uiLabel9);
        }

        private void setVM_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(VMSize.Text))
            {
                //UIMessageBox.ShowError("虚拟内存大小不可为空");
                UIMessageBox.Show("虚拟内存大小不可为空", "虚拟内存设置失败");
                return;
            }
            VirtualMemoryHelper.setVirtualMemory(DiskComboBox, Convert.ToInt32(VMSize.Text), ref uiLabel9);
            LogOutput.AppendText("[" + DateTime.Now.ToLocalTime().ToString() + "] 设置" + DiskComboBox.Items[DiskComboBox.SelectedIndex].ToString() + "虚拟内存为" + VMSize.Text + "GB\n");
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            WriteConfig();
            if (isMining)
            {
                UIMessageBox.Show("正在挖矿，无法退出", "提示");
                e.Cancel = true;
            }
            overClockDefault.PerformClick();
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
        private void MainForm_Shown(object sender, EventArgs e)
        {
            if (autoMining.Active && !string.IsNullOrEmpty(autoMiningTime.Text))
            {
                int time = Convert.ToInt32(autoMiningTime.Text);
                Task.Run(() =>
                {
                    while (true)
                    {
                        if (!ActionButton.Text.Contains("开始挖矿"))
                        {
                            break;
                        }
                        if (time == 0)
                        {
                            this.Invoke(new MethodInvoker(() => { ActionButton.PerformClick(); }));
                            break;
                        }
                        ActionButton.Text = "开始挖矿(" + time.ToString() + ")";
                        Thread.Sleep(1000);
                        time--;
                    }
                });
            }
            if (autoOverclock.Active)
            {
                overClockConfirm_Click(null, null);
            }
            if (remoteControl.Checked)
            {
                InputRemoteIP.Enabled = false;
                StartReceive();
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

        private void icon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
        }

        private void icon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                iconMenu.Show(MousePosition.X, MousePosition.Y);
            }
        }

        private void 显示ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Show();
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void updateButton_Click(object sender, EventArgs e)
        {
            double newVersion = Convert.ToDouble(getIncomeData.getHtml("http://121.4.60.81/szzminer/update.html"));
            if (newVersion > currentVersion)
            {
                if (MinerStatusThread != null)
                {
                    MinerStatusThread.Abort();
                }
                stopMiner();
                string path = Application.StartupPath + "\\szzminer_update.exe";
                Process p = new Process();
                p.StartInfo.FileName = path;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                p.Start();
                Application.Exit();
            }
            else
            {
                Task.Run(()=>{
                    updateButton.Enabled = false;
                    updateButton.Text = "无更新";
                    Thread.Sleep(10*1000);
                    updateButton.Text = "检查更新";
                    updateButton.Enabled = true;
                });
            }
        }

        private void autoMining_ValueChanged(object sender, bool value)
        {
            //WriteConfig();
        }

        private void autoOverclock_ValueChanged(object sender, bool value)
        {
            //WriteConfig();
        }

        private void remoteControl_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(InputRemoteIP.Text))
            {
                UIMessageBox.ShowError("请填写IP地址");
                remoteControl.Checked = false;
                return;
            }
            if (remoteControl.Checked)
            {
                StartReceive();
                InputRemoteIP.Enabled = false;
                remoteControl.Checked = true;
            }
            else
            {
                StopReceive();
                InputRemoteIP.Enabled = true;
            }
            WriteConfig();
        }

        private void autoOverclock_Click(object sender, EventArgs e)
        {
            WriteConfig();
        }

        private void useComputerName_Click(object sender, EventArgs e)
        {
            WriteConfig();
        }

        private void uiButton2_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(Application.StartupPath + "\\bin"))
                Directory.CreateDirectory(Application.StartupPath + "\\bin");
            byte[] Save = szzminer.Properties.Resources.switchradeongpu;
            FileStream fsObj = new FileStream(Application.StartupPath + "\\bin" + @"\switchradeongpu.exe", FileMode.Create);
            fsObj.Write(Save, 0, Save.Length);
            fsObj.Close();
            Process srg = new Process();
            srg.StartInfo.FileName = Application.StartupPath + "\\bin" + @"\switchradeongpu.exe";
            srg.StartInfo.Arguments = "--compute=on --admin --restart";
            srg.StartInfo.CreateNoWindow = true;
            srg.StartInfo.UseShellExecute = false;
            srg.Start();
            srg.WaitForExit();
            UIMessageBox.Show("成功开启计算模式");
            LogOutput.AppendText("成功开启计算模式\n");
        }

        private void uiButton3_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(Application.StartupPath + "\\bin"))
                Directory.CreateDirectory(Application.StartupPath + "\\bin");
            byte[] Save = szzminer.Properties.Resources.switchradeongpu;
            FileStream fsObj = new FileStream(Application.StartupPath + "\\bin" + @"\switchradeongpu.exe", FileMode.Create);
            fsObj.Write(Save, 0, Save.Length);
            fsObj.Close();
            Process srg = new Process();
            srg.StartInfo.FileName = Application.StartupPath + "\\bin" + @"\switchradeongpu.exe";
            srg.StartInfo.Arguments = "--compute=off --admin --restart";
            srg.StartInfo.CreateNoWindow = true;
            srg.StartInfo.UseShellExecute = false;
            srg.Start();
            srg.WaitForExit();
            UIMessageBox.Show("成功关闭计算模式");
            LogOutput.AppendText("成功关闭计算模式\n");
        }

        private void uiButton4_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(Application.StartupPath + "\\bin"))
                Directory.CreateDirectory(Application.StartupPath + "\\bin");
            byte[] Save = szzminer.Properties.Resources.atikmdag_patcher;
            FileStream fsObj = new FileStream(Application.StartupPath + "\\bin\\atikmdag-patcher.exe", FileMode.Create);
            fsObj.Write(Save, 0, Save.Length);
            fsObj.Close();
            Process srg = new Process();
            srg.StartInfo.FileName = Application.StartupPath + "\\bin" + @"\atikmdag-patcher.exe";
            srg.StartInfo.Arguments = "";
            srg.Start();
            srg.WaitForExit();
            UIMessageBox.Show("成功为A卡打补丁，重启后生效");
            LogOutput.AppendText("成功打补丁\n");
        }

        private void uiSwitch1_ValueChanged(object sender, bool value)
        {
            if (loginStart.Active)
            {
                string path = Application.ExecutablePath;
                RegistryKey rk = Registry.CurrentUser;
                RegistryKey rk2 = rk.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
                rk2.SetValue("szzminer", "\"" + path + "\"");
                rk2.Close();
                rk.Close();
            }
            else
            {
                string path = Application.ExecutablePath;
                RegistryKey rk = Registry.CurrentUser;
                RegistryKey rk2 = rk.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
                rk2.DeleteValue("szzminer", false);
                rk2.Close();
                rk.Close();
            }
        }

        private void autoMining_Click_1(object sender, EventArgs e)
        {
            WriteConfig();
        }

        private void autoOverclock_Click_1(object sender, EventArgs e)
        {
            WriteConfig();
        }
    }
}
