using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;



using Client;
    
namespace ConsoleApplication1
{
    internal class Program
    {
        public static void Main(string[] args)
        {

            
            var c = new FileSender();
            var x = File.ReadAllBytes("C:\\Users\\micjan11\\dupa.txt.txt");
            c.SendFile(x, "txt", "dwa");
            c.FilesEvent += PrintFiles;


        }

        public static void PrintFiles(object sender, FilesEventArgs args)
        {
            Console.WriteLine(args.Length);
            foreach (var keyValuePair in args.Files)
            {
                Console.WriteLine(keyValuePair.Value);
            }
        }



       
    }
}