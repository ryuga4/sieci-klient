using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Client
{
    public class FileUpdater
    {
        private TcpClient Connection { get; set; }
        public event EventHandler<FileUpdateEventArgs> FileUpdatedEvent;
        public string FileName { get; set; }

        public FileUpdater(string address, string fileName)
        {
            Connection = new TcpClient(address, 3001);
            FileName = fileName;
            NetworkStream stream = Connection.GetStream();

            var toSend = new List<byte>();

            byte[] type = new byte[] {0};
            byte[] nameSize = new byte[] {(byte) System.Text.ASCIIEncoding.UTF8.GetByteCount(FileName)};
            byte[] name = Encoding.UTF8.GetBytes(FileName);
            
            toSend.AddRange(type);
            toSend.AddRange(nameSize);
            toSend.AddRange(name);
            
            stream.Write(toSend.ToArray(),0,toSend.Count);
            
            
            
            GetUpdate();
            
            
        }




        void Disconnect()
        {
            Connection.Close();
        }

        async void GetUpdate()
        {
            while (Connection.Connected)
            {
                NetworkStream stream = Connection.GetStream();
                byte[] contentSizeA = new byte[4];
                await stream.ReadAsync(contentSizeA, 0, 4);
                Int32 contentSize = BitConverter.ToInt32(contentSizeA.Reverse().ToArray(), 0);
                byte[] content = new byte[contentSize];
                await stream.ReadAsync(content, 0, contentSize);
                FileUpdatedEvent.Invoke(this, new FileUpdateEventArgs(contentSize, content));
            }
        }

        public void UpdateFile(byte[] content)
        {
            byte[] contentSize = BitConverter.GetBytes(content.Length).Reverse().ToArray();
            byte[] nameSize = new byte[] {(byte) FileName.Length};
            byte[] type = new byte[] {1};

            var toSend = new List<byte>();
            toSend.AddRange(type);
            toSend.AddRange(nameSize);
            toSend.AddRange(contentSize);
            toSend.AddRange(content);

            NetworkStream stream = Connection.GetStream();
            stream.Write(toSend.ToArray(), 0, toSend.Count);

        }

    }

    public class FileUpdateEventArgs : EventArgs
    {
        public int Length { get; set; }
        public byte[] Content { get; set; }

        public FileUpdateEventArgs(int l, byte[] c)
        {
            this.Length = l;
            this.Content = c;
        }
    }

}