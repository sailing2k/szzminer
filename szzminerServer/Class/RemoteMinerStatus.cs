﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace szzminerServer.Class
{
    public class DevicesItem
    {
        /// <summary>
        /// 
        /// </summary>
        ///
        public string name { get; set; }
        public string idbus { get; set; }
        public string power { get; set; }
        public string Hashrate { get; set; }
        public string temp { get; set; }
        public string fan { get; set; }
        public string accept { get; set; }
        public string reject { get; set; }
        public string coreclock { get; set; }
        public string memoryclock { get; set; }
    }
    public class GPUOverClock
    {
        public string Busid { get; set; }
        public string Name { get; set; }
        public string Power { get; set; }
        public string TempLimit { get; set; }
        public string CoreClock { get; set; }
        public string CV { get; set; }
        public string MemoryClock { get; set; }
        public string MV { get; set; }
        public string Fan { get; set; }
    }
    public class RemoteMinerStatus
    {
        public string function { get; set; }
        public bool if_mining { get; set; }
        public string flushtime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string IP { get; set; }
        public string MAC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// 
        public string Coin { get; set; }
        public string MinerCore { get; set; }
        public string Worker { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Pool { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Wallet { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Accepted { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Rejected { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int Power { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Hashrate { get; set; }

        public List<GPUOverClock> GPU { get; set; }
        public List<DevicesItem> Devices { get; set; }
    }
}
