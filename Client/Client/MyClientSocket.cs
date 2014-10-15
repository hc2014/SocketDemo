using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    public class MyClientSocket
    {


        static Stack s = new Stack();

        static Socket clientSocket;

        static String outBufferStr;
        static int BagSize;
        static Byte[] outBuffer;

        static Byte[] inBuffer;
        public MyClientSocket(int bagsize,Socket csocket)
        {
            BagSize = bagsize;
            outBuffer = new byte[BagSize];
            inBuffer = new byte[BagSize];
            clientSocket = csocket;
        }


        //public void BeginWord()
        //{

        //    IPEndPoint ipep = new IPEndPoint(ServerIP, 3001); //填写自己电脑的IP或者其他电脑的IP，如果是其他电脑IP的话需将ConsoleApplication_socketServer工程放在对应的电脑上。  

        //    clientSocket = new Socket(ipep.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        //    try
        //    {

        //        clientSocket.Connect(ipep);

        //        Thread recvWord = new Thread(send);
        //        recvWord.Start();

        //        while (true)
        //        {
        //            //接收服务器端信息                

        //            clientSocket.Receive(inBuffer, BagSize, SocketFlags.None);//如果接收的消息为空 阻塞 当前循环  

        //           //ThreadState a=recvWord.ThreadState;
        //            Console.WriteLine(Encoding.Unicode.GetString(inBuffer));
        //            Array.Clear(inBuffer, 0, inBuffer.Length);
        //        }

        //    }

        //    catch
        //    {

        //        Console.WriteLine("服务未开启！");
        //        Console.WriteLine();
        //    }
        //}

        Stack<string> SendList = new Stack<string>();
        public void AddValue(string value)
        {
            lock (SendList)
            {
                SendList.Push(value);
            }
        }

        public void send()
        {
            //发送消息   
            while (true)
            {
                string value = null;

                if (SendList.Count > 0)
                {
                    lock (SendList)
                    {
                        value = SendList.Pop();
                    }
                }
                if (!string.IsNullOrEmpty(value))
                {
                    outBufferStr =value;
                    outBuffer = Encoding.Unicode.GetBytes(outBufferStr);
                    splic(outBuffer, BagSize - 2, 0);
                    value = null;
                }

               

                while (s.Count > 0)
                {
                    byte[] newdata = new byte[BagSize];
                    byte[] len = BitConverter.GetBytes((UInt16)outBuffer.Length);
                    Array.Copy(len, 0, newdata, 0, 2);
                    Array.Copy((byte[])s.Pop(), 0, newdata, 2, outBuffer.Length > 30 ? 30 : outBuffer.Length);
                    //Console.WriteLine(System.Text.Encoding.Unicode.GetString(newdata));
                    clientSocket.Send(newdata, BagSize, SocketFlags.None);
                    
                }
            }
        }

        public void Revc()
        {
            while (true)
            {
                //接收服务器端信息                

                clientSocket.Receive(inBuffer, 32, SocketFlags.None);//如果接收的消息为空 阻塞 当前循环  

                Console.WriteLine("服务器说：");

                Console.WriteLine(Encoding.Unicode.GetString(inBuffer));
                Array.Clear(inBuffer, 0, inBuffer.Length);
            }
        }



        /// <summary>
        /// 数据分包
        /// </summary>
        /// <param name="data">数据源</param>
        /// <param name="len">包长</param>
        /// <param name="first">起始位置</param>
        protected void splic(byte[] data, int len, int first)
        {
            byte[] newdata = new byte[len];
            if (first <= data.Length)
            {

                if (data.Length - first < len)
                {
                    Array.Copy(data, first, newdata, 0, data.Length - first);
                }
                else
                {
                    Array.Copy(data, first, newdata, 0, len);
                    first = first + len;
                    splic(data, len, first);
                }
            }
            s.Push(newdata);
        }
    }
}
