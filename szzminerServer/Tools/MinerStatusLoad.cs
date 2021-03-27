using Newtonsoft.Json;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using szzminerServer.Class;

namespace szzminerServer.Tools
{
    class MinerStatusLoad
    {
        public static List<RemoteMinerStatus> remoteMinerStatusList = new List<RemoteMinerStatus>();//记录矿机信息
        public static void remoteMinerStatus(RemoteMinerStatus minerStatus)
        {
            bool add = true;
            minerStatus.flushtime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            for (var i = 0; i < remoteMinerStatusList.Count; i++)
            {
                if (remoteMinerStatusList[i].MAC == minerStatus.MAC)
                {
                    remoteMinerStatusList[i] = minerStatus;
                    add = false;
                }
            }
            if (add)
            {
                remoteMinerStatusList.Add(minerStatus);
            }
            /*RemoteMinerStatus remoteMinerStatus = JsonConvert.DeserializeObject<RemoteMinerStatus>(json);
            int i;
            for(i = 0 ; i < MinerStatusTable.Rows.Count; i++)
            {
                if (remoteMinerStatus.MAC == Convert.ToString(MinerStatusTable.Rows[i].Cells[12].Value))
                {
                    MinerStatusTable.Rows[i].Cells[0].Value = remoteMinerStatus.Worker;
                    //MinerStatusTable.Rows[i].Cells[1].Value = remoteMinerStatus.Worker;
                    MinerStatusTable.Rows[i].Cells[2].Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    MinerStatusTable.Rows[i].Cells[3].Value = remoteMinerStatus.Coin;
                    MinerStatusTable.Rows[i].Cells[4].Value = remoteMinerStatus.MinerCore;
                    MinerStatusTable.Rows[i].Cells[5].Value = remoteMinerStatus.Pool;
                    MinerStatusTable.Rows[i].Cells[6].Value = remoteMinerStatus.Wallet;
                    MinerStatusTable.Rows[i].Cells[7].Value = remoteMinerStatus.Accepted;
                    MinerStatusTable.Rows[i].Cells[8].Value = remoteMinerStatus.Rejected;
                    MinerStatusTable.Rows[i].Cells[9].Value = remoteMinerStatus.Pool;
                    MinerStatusTable.Rows[i].Cells[10].Value = remoteMinerStatus.Hashrate;
                    MinerStatusTable.Rows[i].Cells[11].Value = remoteMinerStatus.IP;
                    MinerStatusTable.Rows[i].Cells[12].Value = remoteMinerStatus.MAC;
                    break;
                }
            }
            if(i == MinerStatusTable.Rows.Count)
            {
                MinerStatusTable.AddRow();
                MinerStatusTable.Rows[i].Cells[0].Value = remoteMinerStatus.Worker;
                //MinerStatusTable.Rows[i].Cells[1].Value = remoteMinerStatus.Worker;
                MinerStatusTable.Rows[i].Cells[2].Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                MinerStatusTable.Rows[i].Cells[3].Value = remoteMinerStatus.Coin;
                MinerStatusTable.Rows[i].Cells[4].Value = remoteMinerStatus.MinerCore;
                MinerStatusTable.Rows[i].Cells[5].Value = remoteMinerStatus.Pool;
                MinerStatusTable.Rows[i].Cells[6].Value = remoteMinerStatus.Wallet;
                MinerStatusTable.Rows[i].Cells[7].Value = remoteMinerStatus.Accepted;
                MinerStatusTable.Rows[i].Cells[8].Value = remoteMinerStatus.Rejected;
                MinerStatusTable.Rows[i].Cells[9].Value = remoteMinerStatus.Pool;
                MinerStatusTable.Rows[i].Cells[10].Value = remoteMinerStatus.Hashrate;
                MinerStatusTable.Rows[i].Cells[11].Value = remoteMinerStatus.IP;
                MinerStatusTable.Rows[i].Cells[12].Value = remoteMinerStatus.MAC;
            }*/
        }
    }
}
