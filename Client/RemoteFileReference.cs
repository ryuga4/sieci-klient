using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Client
{
    public class RemoteFileReference
    {
        private readonly TcpClient _tcpClient;

        public event EventHandler<FileUpdateEventArgs> FileUpdatedEvent;

        public string FileName { get ;}
       
        public RemoteFileReference(string address,int port, string fileName)
        {
            _tcpClient = new TcpClient(address, port);
            FileName = fileName;            

            var toSend = new List<byte>();

            var type = new byte[] {0};
            var nameSize = new[] {(byte) Encoding.UTF8.GetByteCount(FileName)};
            var name = Encoding.UTF8.GetBytes(FileName);
            
            toSend.AddRange(type);
            toSend.AddRange(nameSize);
            toSend.AddRange(name);

            using (var stream = _tcpClient.GetStream())
            {
                stream.Write(toSend.ToArray(), 0, toSend.Count);
            }                      
                       
            GetUpdate();
                        
        }

        void Disconnect()
        {
            _tcpClient.Close();
        }

        async void GetUpdate()
        {
            while (_tcpClient.Connected)
            {
                using (var stream = _tcpClient.GetStream())
                {
                    var contentSizeA = new byte[4];
                    await stream.ReadAsync(contentSizeA, 0, 4);
                    var contentSize = BitConverter.ToInt32(contentSizeA.Reverse().ToArray(), 0);
                    var content = new byte[contentSize];
                    await stream.ReadAsync(content, 0, contentSize);
                    FileUpdatedEvent?.Invoke(this, new FileUpdateEventArgs(contentSize, content));
                }                
            }
        }

        public void UpdateFile(byte[] content)
        {
            var contentSize = BitConverter.GetBytes(content.Length).Reverse().ToArray();
            var nameSize = new[] {(byte) FileName.Length};
            var type = new byte[] {1};

            var toSend = new List<byte>();
            toSend.AddRange(type);
            toSend.AddRange(nameSize);
            toSend.AddRange(contentSize);
            toSend.AddRange(content);

            using (var stream = _tcpClient.GetStream())
            {                
                stream.Write(toSend.ToArray(), 0, toSend.Count);
            }

        }
    }

    public class FileUpdateEventArgs : EventArgs
    {
        public int Length { get; }
        public byte[] Content { get; }

        public FileUpdateEventArgs(int length, byte[] content)
        {
            this.Length = length;
            this.Content = content;
        }
    }

}