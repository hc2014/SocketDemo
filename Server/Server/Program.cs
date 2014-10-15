using System;

using System.Collections.Generic;

using System.Linq;

using System.Text;

using System.IO;

using System.Net;

using System.Net.Sockets;

using System.Threading;

using System.Collections;
using Server;

namespace ConsoleApplication_socketServer
{

    class Server
    {

        static Socket serverSocket;

        static Socket clientSocket;

        //创建所有链接的集合
        static List<Socket> al = new List<Socket>();

        static Thread thread;

        private static void Add()
        {
            while (true)
            {
                ss.AddValue(Console.ReadLine());
            }
        }
        static ServerSockte ss;
        static void Main(string[] args)
        {

            ss = new ServerSockte(32);
            
            Thread begin = new Thread(new ThreadStart(ss.BeginWord));
            begin.Start();

            Thread add = new Thread(new ThreadStart(Add));
            add.Start();
            


            ////创建监听所有链接的线程
            //Thread lisenterAccept = new Thread(lisenterAcceptThread);
            //lisenterAccept.Start();

            ////创建发送信息的线程
            //Thread sendWord = new Thread(sendWordThread);
            //sendWord.Start();
            ////创建接受信息的线程
            //Thread recvWord = new Thread(recvWordThread);
            //recvWord.Start();
            //try
            //{
            //    while (true)
            //    {
            //        string value = Console.ReadLine();

            //        for (int i = 0; i < al.Count; i++)
            //        {
            //            //添加发送任务的信息
            //            addSendJob(al[i], value);
            //        }

            //    }
            //}
            //catch
            //{
            //    Console.WriteLine("主线程时关闭了一个客户端");
            //    Console.WriteLine();
            //}
            

        }

        private static void lisenterAcceptThread()
        {
            //监听所有连接
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 3001);

            serverSocket = new Socket(ipep.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            //绑定关联
            serverSocket.Bind(ipep);
            //设定挂起队列长度
            serverSocket.Listen(10);

            while (true)
            {
                //建立连接
                clientSocket = serverSocket.Accept();
                
                al.Add(clientSocket);
                //创建接受客户端信息的线程
                thread = new Thread(doWork);

                thread.Start(clientSocket);

            }
        }
        //处理发送数据
        private static void sendWordThread()
        {
            try
            {
                while (true)
                {
                    Dictionary<string, object> job;
                    lock (sendJobList)
                    {
                        if (sendJobList.Count == 0)
                            continue;

                        job = (Dictionary<string, object>)sendJobList.Pop();
                    }


                    Socket client = (Socket)job["client"];
                    string content = (string)job["content"];
                    
                    outBuffer = Encoding.Unicode.GetBytes(content);
                    if (client == null)
                        continue;
                    client.Send(outBuffer, outBuffer.Length, SocketFlags.None);
                }
            }
            catch
            {
                Console.WriteLine("发送时候关闭了一个客户端");
                Console.WriteLine();
            }
        }
        //处理接收数据
        private static void recvWordThread()
        {
            while (true)
            {
                Dictionary<string, object> job;
                lock (recvJobList)
                {
                    if (recvJobList.Count == 0)
                        continue;

                    job = (Dictionary<string, object>)recvJobList.Pop();

                }

                Socket client = (Socket)job["client"];
                string content = (string)job["content"];



                Console.WriteLine(getIP(client) + "  说:" + content);


                addSendJob(client, "OK!");
            }



        }
        //获取ip
        private static string getIP(Socket s)
        {
            IPEndPoint ipEndPoint = (IPEndPoint)s.RemoteEndPoint;

            String address = ipEndPoint.Address.ToString();

            String port = ipEndPoint.Port.ToString();

            return address + ":" + port;
        }
        //包大小
        static int bagsize = 32;

        static Byte[] inBuffer = new Byte[bagsize];

        static Byte[] outBuffer = new Byte[bagsize];

        //接收工作 列表
        private static Stack recvJobList = new Stack();
        //发送工作列表
        private static Stack sendJobList = new Stack();
        /// <summary>
        /// 添加发送数据的任务
        /// </summary>
        /// <param name="client">客户端Socket</param>
        /// <param name="content">数据</param>
        private static void addSendJob(Socket client, string content)
        {
            lock (sendJobList)
            {
                Dictionary<string, object> job = new System.Collections.Generic.Dictionary<string, object>();
                job["client"] = client;
                job["content"] = content;
                
                sendJobList.Push(job);
            }

        }

        /// <summary>
        /// 添加接收数据的任务
        /// </summary>
        /// <param name="client">客户端Socket</param>
        /// <param name="content">数据</param>
        private static void addRecvJob(Socket client, string content)
        {
            lock (recvJobList)
            {
                Dictionary<string, object> job = new System.Collections.Generic.Dictionary<string, object>();
                job["client"] = client;
                job["content"] = content;

                recvJobList.Push(job);
            }

        }


        //客户端数据长度
        static int SourceLen;
        //申请一个大容量 来存放临时数据
        static byte[] FullByte=new byte[512];
        static int index;
        //总长度
        static int FullLen;
        static int len;
        private static void doWork(object clientObject)
        {
            Socket client = (Socket)clientObject;

            index = 0;
            try
            {
                while (true)
                {


                    client.Receive(inBuffer, bagsize, SocketFlags.None);//如果接收的消息为空 阻塞 当前循环  

                    //内存流
                    using (MemoryStream ms = new MemoryStream(inBuffer))
                    {
                        //二进制读取流
                        BinaryReader br = new BinaryReader(ms);
                        //获取 文件的分包文件的包长  因为发生时候用的是短整型所有 只占2个字节
                        SourceLen = br.ReadUInt16();
                        len = 0;
                        //如果 接受的文件长度 小于包长  说明 数据是分包发送过来的，要多次接收完成后才显示
                        if (SourceLen > inBuffer.Length - 2)
                        {

                            for (int i = 2; i < inBuffer.Length; i++)
                            {
                                if (Convert.ToChar(inBuffer[i]) != 0)
                                {
                                    len++;//记录 字节中 不为0的 字节长度；因为传送的过程中 字节不够预先设定byte[]的长度的用/0来补充，接受时候为了方便所有用char 来获取这个文件的实际含有内容的长度
                                }
                            }

                            FullLen += len;
                            //将获取到的 byte[] 复制到临时byte[]中
                            Array.Copy(inBuffer, 2, FullByte, index, inBuffer.Length - 2);
                            //修改 索引，一遍下一次紧跟着改索引位子接着复制数据
                            index += (inBuffer.Length - 2);
                            //如果分包过来的数据已经接受完成
                            if (FullLen == SourceLen / 2)
                            {

                                addRecvJob(client, System.Text.Encoding.Unicode.GetString(FullByte));
                                //各种清空操作
                                FullLen = 0;
                                len = 0;
                                index = 0;
                                Array.Clear(FullByte, 0, SourceLen);
                            }
                        }
                        else
                        {
                            //将接收过来的数据复制 到FullByte中 
                            Array.Copy(inBuffer, 2, FullByte, 0, SourceLen);

                            addRecvJob(client, System.Text.Encoding.Unicode.GetString(FullByte));
                            //清空操作  不然会有缓存
                            Array.Clear(FullByte, 0, SourceLen);
                        }
                    }
                }
            }
            catch
            {
                //如果客户端关闭了 立马移除这个链接 
                al.Remove(client);
                Console.WriteLine("客户端已经关闭！");
                Console.WriteLine();
            }



        }

    }

}
