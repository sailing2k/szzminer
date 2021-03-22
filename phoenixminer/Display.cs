using Newtonsoft.Json;
using phoenixminer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DisplayDll
{
    public class Display
    {
        public List<string> BUSID { get; set; }
        public List<string> Hashrate { get; set; }
        public List<string> Accepted { get; set; }
        public List<string> Rejected { get; set; }
        public static string getHtml(string html)//传入网址
        {
            string pageHtml = "";
            WebClient MyWebClient = new WebClient();
            MyWebClient.Credentials = CredentialCache.DefaultCredentials;//获取或设置用于向Internet资源的请求进行身份验证的网络凭据
            Byte[] pageData = MyWebClient.DownloadData(html); //从指定网站下载数据
            MemoryStream ms = new MemoryStream(pageData);
            using (StreamReader sr = new StreamReader(ms, Encoding.GetEncoding("UTF-8")))
            {
                pageHtml = sr.ReadLine();
            }
            return pageHtml;
        }

        public void getMinerInfo()
        {
            BUSID = new List<string>();
            Hashrate = new List<string>();
            Accepted = new List<string>();
            Rejected = new List<string>();
            List<string> LS = PhoenixMiner.GetPhoenixMinerInformation();
            List<double?> Hashrates = JsonConvert.DeserializeObject<List<double?>>($"[{LS[3].Replace(";", ",")}]");
            List<int?> ShAccepted = JsonConvert.DeserializeObject<List<int?>>($"[{LS[9].Replace(";", ",")}]");

            List<int?> ShRejected = JsonConvert.DeserializeObject<List<int?>>($"[{LS[10].Replace(";", ",")}]");

            List<int?> ShInvalid = JsonConvert.DeserializeObject<List<int?>>($"[{LS[11].Replace(";", ",")}]");

            List<int?> Busid = JsonConvert.DeserializeObject<List<int?>>($"[{LS[15].Replace(";", ",")}]");
            for (int k = 0; k < Busid.Count; k++)
            {
                BUSID.Add(Convert.ToString(Busid[k]));
                Hashrate.Add(Convert.ToString(Hashrates[k]/1000)+ " MH/S");
                Accepted.Add(Convert.ToString(ShAccepted[k]));
                Rejected.Add(Convert.ToString(ShRejected[k]+ShInvalid[k]));
            }
        }
        public List<string> getBUSID()
        {
            return BUSID;
        }
        public List<string> getHashrate()
        {
            return Hashrate;
        }
        public List<string> getAccepted()
        {
            return Accepted;
        }
        public List<string> getRejected()
        {
            return Rejected;
        }
    }
}
