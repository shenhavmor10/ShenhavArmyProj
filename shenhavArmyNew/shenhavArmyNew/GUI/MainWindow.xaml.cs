using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Windows.Forms;
using Ookii.Dialogs.Wpf;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static int threadNumber = 0;
        internal static MainWindow main;
        public MainWindow()
        {
            InitializeComponent();
            main = this;
        }
        public void setTextBlock(string data,int number)
        {
            switch (number)
            {
                case 0:
                    Dispatcher.Invoke(new Action(() => { this.TextBlock1.Text = data; })) ;
                    break;
                case 1:
                    Dispatcher.Invoke(new Action(() => { this.TextBlock2.Text = data; }));
                    break;
                default:
                    break;
            }
        }
        private void ConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            string content = (sender as Button).Name.ToString();
            string path;
            if(content=="Connect")
            {
                path = FileNameTextBox1.Text + ',' + FileNameTextBox2.Text + ',' + FileNameTextBox3.Text + ',' + FileNameTextBox4.Text;
                //Connect.IsEnabled = false;
                Console.WriteLine("connect1 pressed.");
            }
            else
            {
                path = FileNameTextBox5.Text + ',' + FileNameTextBox6.Text + ',' + FileNameTextBox7.Text + ',' + FileNameTextBox8.Text;
                //Connect3.IsEnabled = false;
                Console.WriteLine("connect3 pressed.");

            }
            Thread clientThread;
            clientThread = new Thread(() => ClientConnection.ExecuteClient(path,threadNumber));
            clientThread.Start();
            threadNumber++;
        }

        private void BrowseButtonFile_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();
            // Launch OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = openFileDlg.ShowDialog();
            string content = (sender as Button).Name.ToString();
            // Get the selected file name and display in a TextBox.
            // Load content of file in a TextBlock
            if (result == true)
            {
                if(content=="Browse1")
                {
                    FileNameTextBox1.Text = openFileDlg.FileName;
                }
                else
                {
                    FileNameTextBox5.Text = openFileDlg.FileName;
                }
                
            }
        }
        private void BrowseButtonFolder_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            var folderResult = folderDialog.ShowDialog();
            if (folderResult.HasValue && folderResult.Value)
            {
                string content = (sender as Button).Name.ToString();
                switch (content)
                {
                    case "Browse2":
                        FileNameTextBox2.Text = folderDialog.SelectedPath;
                        break;
                    case "Browse3":
                        FileNameTextBox3.Text = folderDialog.SelectedPath;
                        break;
                    case "Browse4":
                        FileNameTextBox4.Text = folderDialog.SelectedPath;
                        break;
                    case "Browse6":
                        FileNameTextBox6.Text = folderDialog.SelectedPath;
                        break;
                    case "Browse7":
                        FileNameTextBox7.Text = folderDialog.SelectedPath;
                        break;
                    case "Browse8":
                        FileNameTextBox8.Text = folderDialog.SelectedPath;
                        break;
                    default:
                        break;

                }
                
            }
        }


    }
}
