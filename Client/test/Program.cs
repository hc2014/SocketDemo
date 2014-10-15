using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {

            while (true)
            {
                string str = Console.ReadLine();
                splic(System.Text.Encoding.Unicode.GetBytes(str), 28, 0);
                byte[] by;
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryWriter bw = new BinaryWriter(ms);
                    BinaryReader br = new BinaryReader(ms);

                    
                    while (s.Count > 0)
                    {
                        bw.Write((UInt16)System.Text.Encoding.Unicode.GetBytes(str).Length);
                        bw.Write((byte[])s.Pop());
                    }
                     by= new byte[ms.Length];

                     int count = 0;
                     ms.Position = 0;
                     while (count < ms.Length)
                     {
                         by[count++] = br.ReadByte();
                     }
                    //ms.Position = 0;
                    //Console.WriteLine(br.ReadInt16());
                   //Console.WriteLine(System.Text.Encoding.Unicode.GetString(by));
                }

                using (MemoryStream ms = new MemoryStream(by))
                {
                    BinaryReader br = new BinaryReader(ms);
                    //int i = (int)ms.Length;
                    //Console.WriteLine(br.ReadInt16());
                    byte[] newby=new byte[by.Length-2];
                    ms.Position = 0;
                    Array.Copy(by, 2, newby, 0, by.Length - 2);

                    Console.WriteLine(System.Text.Encoding.Unicode.GetString(newby));
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
            byte[] newdata=new byte[len];
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
