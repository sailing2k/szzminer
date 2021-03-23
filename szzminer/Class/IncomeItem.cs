using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace szzminer.Class
{
    class IncomeItem
    {
        public string DataCode { get; set; }
        public string CoinCode { get; set; }
        public double Speed { get; set; }
        public string SpeedUnit { get; set; }
        public double IncomeCoin { get; set; }
        public double IncomeUsd { get; set; }
        public double IncomeCny { get; set; }
        public double NetSpeed { get; set; }
        public string NetSpeedUnit { get; set; }
    }
}
