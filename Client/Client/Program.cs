using System;

using System.Collections.Generic;

using System.Linq;

using System.Text;

using System.Net;

using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Collections;
using Client;


namespace ConsoleApplication_socketClient
{

    class Client
    {


        static Socket clientSocket;

        static String outBufferStr;
        
        static int bagsize = 32;
        static Byte[] outBuffer = new Byte[bagsize];

        static Byte[] inBuffer = new Byte[bagsize];



        static void send()
        {
            //发送消息   
            while (true)
            {
                outBufferStr = Console.ReadLine();

                outBuffer = Encoding.Unicode.GetBytes(outBufferStr);

                splic(outBuffer, bagsize - 2, 0);
                
                while (s.Count > 0)
                {
                    byte[] newdata = new byte[bagsize];
                    byte[] len = BitConverter.GetBytes((UInt16)outBuffer.Length);
                    //插入数据长度到byte[]为包头
                    Array.Copy(len, 0, newdata, 0, 2);
                    //插入正式的数据
                    Array.Copy((byte[])s.Pop(), 0, newdata, 2, outBuffer.Length > 30 ? 30 : outBuffer.Length);
                    //发送数据
                    clientSocket.Send(newdata, bagsize, SocketFlags.None);
                }
            }
        }


        static Stack s = new Stack();
        /// <summary>
        /// 数据分包
        /// </summary>
        /// <param name="data">数据源</param>
        /// <param name="len">包长</param>
        /// <param name="first">起始位置</param>
        static void splic(byte[] data, int len, int first)
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

        static MyClientSocket ms;
        private static void Add()
        {
            while (true)
            {
                string str = Console.ReadLine();
                ms.AddValue(str);
            }
        }


        static void Main(string[] args)
        {
         
           
            //将网络端点表示为IP地址和端口 用于socket侦听时绑定         
            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("192.168.7.120"), 3001); //填写自己电脑的IP或者其他电脑的IP，如果是其他电脑IP的话需将ConsoleApplication_socketServer工程放在对应的电脑上。  
            
            clientSocket = new Socket(ipep.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            
            //将Socket连接到服务器    

            try
            {
                ms = new MyClientSocket(32, clientSocket);
                clientSocket.Connect(ipep);
                Thread send = new Thread(ms.send);
                send.Start();


                Thread revc = new Thread(ms.Revc);
                revc.Start();

                Thread add = new Thread(new ThreadStart(Add));
                add.Start();


                //while (true)
                //{
                //    //接收服务器端信息                

                //    clientSocket.Receive(inBuffer, 16, SocketFlags.None);//如果接收的消息为空 阻塞 当前循环  

                //    Console.WriteLine("服务器说：");

                //    Console.WriteLine(Encoding.Unicode.GetString(inBuffer));
                //    Array.Clear(inBuffer, 0, inBuffer.Length);
                //}

            }

            catch
            {

                Console.WriteLine("服务未开启！");

                Console.ReadLine();

            }
            
        }

    }



}
