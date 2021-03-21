using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayDll
{
    class nbminer
    {
        /// <summary>
        /// 
        /// </summary>
        public Miner miner { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int reboot_times { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int start_time { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Stratum stratum { get; set; }
    }
    public class Stratum
    {
        /// <summary>
        /// 
        /// </summary>
        public int accepted_shares { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int accepted_shares2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string algorithm { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string difficulty { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string difficulty2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string dual_mine { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int latency { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int latency2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int rejected_shares { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int rejected_shares2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string url { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string url2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string use_ssl { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string use_ssl2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string user { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string user2 { get; set; }
    }
    public class DevicesItem
    {
        /// <summary>
        /// 
        /// </summary>
        public int accepted_shares { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int accepted_shares2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int core_clock { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int core_utilization { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int fan { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double fidelity1 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int fidelity2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string hashrate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string hashrate2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double hashrate2_raw { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double hashrate_raw { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string info { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int mem_clock { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int mem_utilization { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int pci_bus_id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int power { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int rejected_shares { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int rejected_shares2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int temperature { get; set; }
    }
    public class Miner
    {
        /// <summary>
        /// 
        /// </summary>
        public List<DevicesItem> devices { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string total_hashrate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string total_hashrate2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double total_hashrate2_raw { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double total_hashrate_raw { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int total_power_consume { get; set; }
    }
}
