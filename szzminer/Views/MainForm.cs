using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using szzminer.Class;

namespace szzminer.Views
{
    public partial class MainForm : UIForm
    {
        Thread MinerStatusThread;
        public MainForm()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void uiButton1_Click(object sender, EventArgs e)
        {
            if (ActionButton.Text.Equals("开始挖矿"))
            {
                //startMiner();
                Functions.dllPath = System.AppDomain.CurrentDomain.BaseDirectory + string.Format("miner\\{0}\\{1}.dll", SelectMiner.Text, SelectMiner.Text.Split(' ')[0]);
                MinerStatusThread = new Thread(getMinerInfo);
                MinerStatusThread.IsBackground = true;
                MinerStatusThread.Start();
            }
            else
            {
                stopMiner();
            }
        }

        private void getMinerInfo()
        {
            while (true)
            {
                try
                {
                    
                    Functions.getMinerInfo();
                    for (int i = 0; i < Functions.BUSID.Count; i++)
                    {
                        GPUStatusTable.AddRow();
                        GPUStatusTable.Rows[i].Cells[0].Value = Functions.BUSID[i];
                        GPUStatusTable.Rows[i].Cells[2].Value = Functions.Hashrate[i];
                        GPUStatusTable.Rows[i].Cells[3].Value = Functions.Accepted[i];
                        GPUStatusTable.Rows[i].Cells[4].Value = Functions.Rejected[i];
                    }
                    Thread.Sleep(5000);
                }
                catch(Exception ex)
                {

                }
            }
        }

        private void startMiner()
        {
            Miner.coin = SelectCoin.Text;
            Miner.minerBigName = SelectMiner.Text;
            Miner.minerSmallName = SelectMiner.Text.Split(' ')[0];
            Miner.miningPool = InputMiningPool.Text;
            Miner.wallet = InputWallet.Text;
            Miner.worker = InputWorker.Text;
            Miner.argu = InputArgu.Text;
            Miner.startMiner();
            ActionButton.Text = "停止挖矿";
        }
        private void stopMiner()
        {
            ActionButton.Text = "开始挖矿";
            Miner.stopMiner();
        }
    }
}
