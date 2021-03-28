using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace szzminer.Class.RemoteClass
{
    class RemoteOverclock
    {
        public string function { get; set; }
        public GPUOverClock OVData { get; set; }
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
}
