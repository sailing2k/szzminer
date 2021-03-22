using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace szzminer.Class
{
    class DownloadFile
    {
        public static void downloadIniFile(string url, string name)
        {
            string localfile = Application.StartupPath + name;
            long startPosition = 0; // 上次下载的文件起始位置
            FileStream writeStream = null; // 写入本地文件流对象
            long remoteFileLength = GetHttpLength(url);// 取得远程文件长度
            System.Console.WriteLine("remoteFileLength=" + remoteFileLength);
            if (remoteFileLength == 0)
            {
                //WriteLog("Line " + GetCodeLineNum(1).ToString() + ": 远程文件不存在");
                return;
            }

            // 判断要下载的文件夹是否存在
            if (File.Exists(localfile))
            {
                File.Delete(localfile);
                writeStream = new FileStream(localfile, FileMode.Create);// 文件不保存创建一个文件
                startPosition = 0;
            }
            else
            {
                writeStream = new FileStream(localfile, FileMode.Create);// 文件不保存创建一个文件
                startPosition = 0;
            }
            try
            {
                HttpWebRequest myRequest = (HttpWebRequest)HttpWebRequest.Create(url);// 打开网络连接
                if (startPosition > 0)
                {
                    myRequest.AddRange((int)startPosition);// 设置Range值,与上面的writeStream.Seek用意相同,是为了定义远程文件读取位置
                }
                Stream readStream = myRequest.GetResponse().GetResponseStream();// 向服务器请求,获得服务器的回应数据流
                byte[] btArray = new byte[512];// 定义一个字节数据,用来向readStream读取内容和向writeStream写入内容
                int contentSize = readStream.Read(btArray, 0, btArray.Length);// 向远程文件读第一次
                long currPostion = startPosition;
                while (contentSize > 0)// 如果读取长度大于零则继续读
                {
                    currPostion += contentSize;
                    writeStream.Write(btArray, 0, contentSize);// 写入本地文件
                    contentSize = readStream.Read(btArray, 0, btArray.Length);// 继续向远程文件读取
                }
                //关闭流
                writeStream.Close();
                readStream.Close();
            }
            catch (Exception)
            {
                writeStream.Close();
            }
        }
        private static long GetHttpLength(string url)
        {
            long length = 0;

            try
            {
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);// 打开网络连接
                HttpWebResponse rsp = (HttpWebResponse)req.GetResponse();

                if (rsp.StatusCode == HttpStatusCode.OK)
                {
                    length = rsp.ContentLength;// 从文件头得到远程文件的长度
                }

                rsp.Close();
                return length;
            }
            catch (Exception e)
            {
                return length;
            }
        }
        private static int GetCodeLineNum(int skipFrames)
        {
            StackTrace st = new StackTrace(skipFrames, true);
            StackFrame fram = st.GetFrame(0);
            int lineNum = fram.GetFileLineNumber();
            return lineNum;
        }
    }
}
