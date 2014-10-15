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

namespace Server
{
    class ServerSockte
    {

        static UInt16 SourceLen;
        static byte[] FullByte = new byte[512];
        static int index;
        static int FullLen;
        static int len;

        static Socket serverSocket;

        static Socket clientSocket;

        static List<Socket> al = new List<Socket>();

        static Thread thread;

        static int BagSize;

        static Byte[] inBuffer ;

        static Byte[] outBuffer;

        private static Stack recvJobList = new Stack();
        private static Stack sendJobList = new Stack();

        public ServerSockte(int bagsize)
        {
            BagSize = bagsize;
            inBuffer = new Byte[BagSize];
            outBuffer = new Byte[BagSize];
        }


        Stack<string> SendList=new Stack<string>();
        public void AddValue(string value)
        {
            lock (SendList)
            {
                SendList.Push(value);
            }
        }

        public void BeginWord()
        {
            //创建监听所有链接的线程
            Thread lisenterAccept = new Thread(new ThreadStart(lisenterAcceptThread));
            lisenterAccept.Start();

            //创建发送信息的线程Start
            Thread sendWord = new Thread(new ThreadStart(sendWordThread));
            sendWord.Start();
            //创建接受信息的线程
            Thread recvWord = new Thread(new ThreadStart(recvWordThread));
            recvWord.Start();

            while (true)
            {
                string value=null;

                if (SendList.Count > 0)
                {
                    lock (SendList)
                    {
                        value = SendList.Pop();
                    }
                }
                

                for (int i = 0; i < al.Count; i++)
                {
                    //添加发送任务的信息
                    if (string.IsNullOrEmpty(value))
                        continue;
                    addSendJob(al[i], value);
                }

            }
        }


        private static void lisenterAcceptThread()
        {
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any,3001);

            serverSocket = new Socket(ipep.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            serverSocket.Bind(ipep);

            serverSocket.Listen(10);

            while (true)
            {

                clientSocket = serverSocket.Accept();

                al.Add(clientSocket);

                thread = new Thread(doWork);

                thread.Start(clientSocket);

            }
        }

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

                    client.Send(outBuffer, outBuffer.Length, SocketFlags.None);
                    Array.Clear(outBuffer, 0, outBuffer.Length);
                    //SocketAsyncEventArgs a = new SocketAsyncEventArgs();

                }
            }
            catch
            {
                Console.WriteLine("发送时候关闭了一个客户端");
                Console.WriteLine();
            }
        }

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

        private static string getIP(Socket s)
        {
            IPEndPoint ipEndPoint = (IPEndPoint)s.RemoteEndPoint;

            String address = ipEndPoint.Address.ToString();

            String port = ipEndPoint.Port.ToString();

            return address + ":" + port;
        }
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

        protected static bool isUInt16(object o,out UInt16 result)
        {
            try
            {
                result = Convert.ToUInt16(o);
                return true;
            }
            catch
            {
                result = 0;
                return false;
            }
        }

        private static void doWork(object clientObject)
        {
            Socket client = (Socket)clientObject;
            
            index = 0;
            try
            {
                while (true)
                {


                    client.Receive(inBuffer, BagSize, SocketFlags.None);//如果接收的消息为空 阻塞 当前循环  


                    MemoryStream ms = new MemoryStream(inBuffer);
                    BinaryReader br = new BinaryReader(ms);
                    if (!isUInt16(br.ReadUInt16(), out SourceLen))
                    {
                        addRecvJob(client,"包头异常！");
                        continue;
                    }
                    //FullByte = new byte[SourceLen];
                    len = 0;
                    if (SourceLen > inBuffer.Length - 2)
                    {

                        for (int i = 2; i < inBuffer.Length; i++)
                        {
                            if (Convert.ToChar(inBuffer[i]) != 0)
                            {
                                len++;
                            }
                        }
                        FullLen += len;
                        //FullByte = new byte[SourceLen];
                        Array.Copy(inBuffer, 2, FullByte, index, inBuffer.Length - 2);
                        index += (inBuffer.Length - 2);
                        if (FullLen == SourceLen / 2)
                        {
                            addRecvJob(client, System.Text.Encoding.Unicode.GetString(FullByte));
                            FullLen = 0;
                            len = 0;
                            index = 0;
                            Array.Clear(FullByte, 0, SourceLen);
                        }
                    }
                    else
                    {

                        Array.Copy(inBuffer, 2, FullByte, 0, SourceLen);
                        addRecvJob(client, System.Text.Encoding.Unicode.GetString(FullByte));
                        Array.Clear(FullByte, 0, SourceLen);
                    }

                }

            }
            catch
            {
                //如果客户端关闭了 立马移除这个链接 
                al.Remove(client);
               string Endpoint=getIP(client);
               Console.WriteLine("客户端:" + Endpoint + "已经关闭！");
                Console.WriteLine();
            }

        }
    }
}
