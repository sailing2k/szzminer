using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace phoenixminer
{
    class PhoenixMiner
    {
        public static byte[] req;
        static public List<string> GetPhoenixMinerInformation()
        {
            try
            {
                using (TcpClient client = new TcpClient("127.0.0.1", 22333))
                {
                    using (NetworkStream stream = client.GetStream())
                    {
                        req = new byte[51];
                        var xxx = Encoding.UTF8.GetBytes("{\"id\":0,\"jsonrpc\":\"2.0\",\"method\":\"miner_getstat2\"}");
                        for (int i = 0; i < xxx.Length; i++) req[i] = xxx[i];
                        req[50] = 10;
                        stream.Write(req, 0, req.Length);
                        byte[] phdata = new byte[1024];
                        int bytes = stream.Read(phdata, 0, phdata.Length);
                        string message = Encoding.UTF8.GetString(phdata, 0, bytes);
                        //mainform.WriteLog(message);
                        List<string> LS = JsonConvert.DeserializeObject<PhoenixMinerInfo>(message).result;
                        return LS;
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        private class PhoenixMinerInfo
        {
#pragma warning disable IDE1006 // Стили именования
            public List<string> result { get; set; }
#pragma warning restore IDE1006 // Стили именования
        }
    }
}
