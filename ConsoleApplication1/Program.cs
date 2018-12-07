using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Client;
    
namespace ConsoleApplication1
{
    internal class Program
    {
        public static void Main(string[] args)
        {

            //////////////////////////////////////////////////
            
            var c1 = new FileSender("localhost");
            
            var x = File.ReadAllBytes("/home/michal/dupa"); 
          
            
            //c1.SendFile(x, "txt", "dupa3");
            ////dodaje nowy plik na serwer
            ////nazwa musi być unikalna, jak dasz istniejącą to się wysypie xdd
            
            c1.FilesEvent += PrintListOfFiles; 
            // Jak tylko serwer wyśle info o nowej liście plików to je wyświetl
            // serwer póki co wysyła liste plików tylko raz, zaraz po nawiązaniu połączenia
            
            
            
            /////////////////////////////////////////////////
            
            var c2 = new FileUpdater("localhost", "dupa"); 
            // Podajesz nazwe pliku który chcesz obserwować
            // jak dasz nazwe pliku którego nie ma na serwerze to może być kaszana
            
            c2.UpdateFile(Encoding.UTF8.GetBytes("nowa treść pliku"));
            // modyfikuje treść pliku
            // póki co zmiany nie zachodzą w bazie danych, nowa treść pliku trzymana jest w ramie
            

            c2.FileUpdatedEvent += PrintUpdatedContent; 
            // wyświetla treść pliku jak tylko się zmieni
            // uważaj, bo jeżeli wyłączysz serwer w trakie jak to się już odapali
            // to zaczyna się kręcić w kółko jak pojebane
            // nie ma żadnej prostej metody sprawdzenia czy serwer się wyłączył czy nie z tego co wiem
            
            
            Thread.Sleep(Timeout.Infinite);
            
        }

        public static void PrintUpdatedContent(object sender, FileUpdateEventArgs args)
        {
            //nie zmieniałem byte[] na string bo nie wiem czy i tak byś przypadkiem byte[] nie potrzebował w tym wypadku
            Console.WriteLine(Encoding.UTF8.GetString(args.Content));
        }


        public static void PrintListOfFiles(object sender, FilesEventArgs args)
        {
           //args.Files to lista par <typ, nazwa> 
           args.Files.Select(x => x.Value).ToList().ForEach(Console.WriteLine);
        }



       
    }
}