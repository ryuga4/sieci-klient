using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Client
{
    public class FileSender
    {

        public TcpClient Connection { get; set; }
        public event EventHandler<FilesEventArgs> FilesEvent;
        public FileSender()
        
        {
            this.Connection = new TcpClient("localhost", 3000);
            new Thread(ReceiveFiles).Start();
        }

        public void SendFile(byte[] bytes, string type, string name)
        {
            NetworkStream s = Connection.GetStream();
            byte[] t = 
                (type == "txt") ? new byte[] {1} :
                (type == "png") ? new byte[] {2} :
                new byte[] {3};

            byte[] ns = new byte[] {(byte) System.Text.ASCIIEncoding.UTF8.GetByteCount(name)};
            byte[] cs = BitConverter.GetBytes((Int32) bytes.Length).Reverse().ToArray();
            byte[] name2 = Encoding.UTF8.GetBytes(name);
            
            var toSend = new List<byte>();
            
            toSend.AddRange(t);
            toSend.AddRange(ns);
            toSend.AddRange(cs);
            toSend.AddRange(name2);
            toSend.AddRange(bytes);
            
            s.Write(toSend.ToArray(), 0 ,toSend.Count);

        }

        void ReceiveFiles()
        {
            while (true)
            {
                NetworkStream s = Connection.GetStream();
                
                byte[] countA = new byte[1];
                s.Read(countA, 0, 1);

                var files = new List<KeyValuePair<string,string>>();
                for (int i = 0; i < countA.First(); i++)
                {
                    byte[] t_ns = new byte[2];
                    s.Read(t_ns, 0, 2);
                    byte[] name = new byte[t_ns[1]];
                    s.Read(name, 0, t_ns[1]);


                    string type = t_ns[0] == 1 ? "txt" :
                        t_ns[0] == 2 ? "png" :
                        "...";

                    files.Add(new KeyValuePair<string, string>(type, Encoding.UTF8.GetString(name)));
                }
                
                var fileArgs = new FilesEventArgs();
                fileArgs.Length = countA.First();
                fileArgs.Files = files;

                FilesEvent.Invoke(this, fileArgs);
            }
        }
    }

    public class FilesEventArgs : EventArgs
    {
        public int Length { get; set; }
        public List<KeyValuePair<string,string>> Files { get; set; }
    }
}