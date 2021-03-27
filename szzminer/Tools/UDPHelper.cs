using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace szzminer.Tools
{
    class UDPHelper
    {
        public static void Send(string msg, string ip)
        {
            UdpClient client = new UdpClient(new IPEndPoint(IPAddress.Any, 0));
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(ip), 22429);
            byte[] buf = Encoding.GetEncoding("gb2312").GetBytes(msg);
            client.Send(buf, buf.Length, endpoint);
        }
    }
}
