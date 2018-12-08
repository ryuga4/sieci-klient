using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Client.ServerConnector;
using Microsoft.Win32;

namespace Client.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();

            availableFiles = new ObservableCollection<string>();
            _serverFacade = new ServerFacade("localhost",3000);
            _serverFacade.FilesEvent += OnFileDiscovered;

            FilesList.ItemsSource = availableFiles;
            FilesList.SelectionChanged += FilesList_SelectionChanged;
            FileContent.TextChanged += FileContent_TextChanged;

        }

        private void FileContent_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (fileReference != null)
            {
                
                var content = (sender as TextBox).Text;
                fileReference.UpdateFile(Encoding.UTF8.GetBytes(content));
            }
        }

        private void FilesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var fileName = ((sender as ListView)?.SelectedItem as string);
            if(fileReference !=null)
                fileReference.Disconnect();
            fileReference = new RemoteFileReference("localhost",3001,fileName);
            fileReference.FileUpdatedEvent += FileReference_FileUpdatedEvent;            
        }

        private void FileReference_FileUpdatedEvent(object sender, FileUpdateEventArgs e)
        {
            FileContent.Text = Encoding.UTF8.GetString(e.Content);
        }

        private ServerFacade _serverFacade;
        private RemoteFileReference fileReference;

        private ObservableCollection<string> availableFiles;

        private void AddFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                var fileName = openFileDialog.FileName.Split('\\').Last();
                var fileContent = File.ReadAllBytes(openFileDialog.FileName);
                var fileType = fileName.Split('.').Last();

                _serverFacade.SendFile(fileContent,fileType,fileName);
                availableFiles.Add(fileName);
            }
            
        }

        private void OnFileDiscovered(object sender, FilesEventArgs args)
        {
            foreach (var file in args.Files.Select(x => x.Value))
            {
                availableFiles.Add(file);
            }            
        }
    }
}
