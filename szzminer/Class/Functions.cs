using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace szzminer.Class
{
    class Functions
    {
        public static string dllPath;
        public static List<string> BUSID =new List<string>();
        public static List<string> Hashrate = new List<string>();
        public static List<string> Accepted = new List<string>();
        public static List<string> Rejected = new List<string>();


        public static void getMinerInfo()
        {
            var asm = Assembly.LoadFile(dllPath);

            var type = asm.GetType("DisplayDll.Display");

            var instance = asm.CreateInstance("DisplayDll.Display");


            var method = type.GetMethod("getMinerInfo");
            method.Invoke(instance, null);

            method = type.GetMethod("getBUSID");
            BUSID= (List<string>)method.Invoke(instance,null);
            method = type.GetMethod("getHashrate");
            Hashrate= (List<string>)method.Invoke(instance,null);
            method = type.GetMethod("getAccepted");
            Accepted = (List<string>)method.Invoke(instance, null);
            method = type.GetMethod("getRejected");
            Rejected = (List<string>)method.Invoke(instance, null);
        }
        
    }
}
