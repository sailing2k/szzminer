using Newtonsoft.Json;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using szzminerServer.Class;
using szzminerServer.Tools;

namespace szzminerServer.Views
{
    public partial class overClockForm : UIForm
    {
        List<RemoteMinerStatus> remoteMinerStatusList;
        public overClockForm(List<RemoteMinerStatus> remoteMinerStatusList)
        {
            InitializeComponent();
            this.remoteMinerStatusList = remoteMinerStatusList;
        }

        private void uiButton3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void overClockForm_Load(object sender, EventArgs e)
        {
            for(int i = 0; i < remoteMinerStatusList.Count; i++)
            {
                for(int j = 0; j < remoteMinerStatusList[i].Devices.Count;j++)
                {
                    bool addFlag = true;
                    for(int k = 0; k < selectGPU.Items.Count; k++)
                    {
                        if(remoteMinerStatusList[i].Devices[j].name.Equals(selectGPU.Items[k].ToString()))
                        {
                            addFlag = false;
                        }
                    }
                    if (addFlag)
                    {
                        selectGPU.Items.Add(remoteMinerStatusList[i].Devices[j].name);
                    }
                }
            }
            selectGPU.SelectedIndex = 0;
        }

        private void uiButton1_Click(object sender, EventArgs e)
        {
            RemoteOverclock remoteOverclock = new RemoteOverclock();
            remoteOverclock.function = "overclock";
            GPUOverClock gPUOverClock = new GPUOverClock();
            remoteOverclock.OVData = gPUOverClock;
            remoteOverclock.OVData.Name = selectGPU.Text;
            remoteOverclock.OVData.Power = uiTextBox1.Text;
            remoteOverclock.OVData.TempLimit = uiTextBox2.Text;
            remoteOverclock.OVData.CoreClock = uiTextBox3.Text;
            remoteOverclock.OVData.MemoryClock = uiTextBox4.Text;
            remoteOverclock.OVData.CV = uiTextBox5.Text;
            remoteOverclock.OVData.MV = uiTextBox6.Text;
            remoteOverclock.OVData.Fan = uiTextBox7.Text;
            string msg=JsonConvert.SerializeObject(remoteOverclock);
            for(int i = 0; i < remoteMinerStatusList.Count; i++)
            {
                UDPHelper.Send(msg,remoteMinerStatusList[i].IP);
            }
            UIMessageBox.Show("设置完成","提示");
        }

        private void uiButton2_Click(object sender, EventArgs e)
        {
            RemoteOverclock remoteOverclock = new RemoteOverclock();
            remoteOverclock.function = "overclock";
            GPUOverClock gPUOverClock = new GPUOverClock();
            remoteOverclock.OVData = gPUOverClock;
            remoteOverclock.OVData.Name = selectGPU.Text;
            remoteOverclock.OVData.Power = "0";
            remoteOverclock.OVData.TempLimit = "0";
            remoteOverclock.OVData.CoreClock = "0";
            remoteOverclock.OVData.MemoryClock = "0";
            if (selectGPU.Text.Contains("NVIDIA"))
            {
                remoteOverclock.OVData.CV = "N/A";
                remoteOverclock.OVData.MV = "N/A";
            }
            else
            {
                remoteOverclock.OVData.CV = "0";
                remoteOverclock.OVData.MV = "0";
            }
            remoteOverclock.OVData.Fan = "0";
            string msg = JsonConvert.SerializeObject(remoteOverclock);
            for (int i = 0; i < remoteMinerStatusList.Count; i++)
            {
                UDPHelper.Send(msg, remoteMinerStatusList[i].IP);
            }
            UIMessageBox.Show("设置完成", "提示");
        }

        private void selectGPU_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (selectGPU.Text.Contains("NVIDIA"))
            {
                uiTextBox5.Enabled = false; uiTextBox5.Text = "N/A";
                uiTextBox6.Enabled = false; uiTextBox6.Text = "N/A";
            }
            else
            {
                uiTextBox5.Enabled = false; uiTextBox5.Text = "";
                uiTextBox6.Enabled = false; uiTextBox6.Text = "";
            }
        }
    }
}
