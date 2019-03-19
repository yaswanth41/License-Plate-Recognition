using Microsoft.Win32;
using System;
using System.Collections.Generic;
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

namespace LPR_GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MLApp.MLApp matlab;
        private string BaseDir = System.AppDomain.CurrentDomain.BaseDirectory;
        private OpenFileDialog openFileDialog;
        public MainWindow()
        {
            InitializeComponent();

            Thread th = new Thread(Start_MLApp);
            th.Start();

            openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "JPEG Files (*.jpg)|*.jpg|PNG Files (*.png)|*.png";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        }

        public async void Start_MLApp()
        {
            MessageBox.Show("Please Wait.", "Initializing Matlab");
            matlab = new MLApp.MLApp();
            MessageBox.Show("Initialised");
        }

        private async void BtnRecognise_Click(object sender, RoutedEventArgs e)
        {
            if (matlab == null)
            {
                MessageBox.Show("App Not Completed initialization.Please wait.");
                return;
            }
            matlab.Execute($"fileLoc='{openFileDialog.FileName}'");
            var result = matlab.Execute($"run('{BaseDir}/Recogniser/main.m');");
            MessageBox.Show(result);
        }

        private void BtnSelectImg_Click(object sender, RoutedEventArgs e)
        {
            if (openFileDialog.ShowDialog() == true)
            {
                BitmapImage src = new BitmapImage();
                src.BeginInit();
                src.UriSource = new Uri(openFileDialog.FileName, UriKind.Absolute);
                src.CacheOption = BitmapCacheOption.OnLoad;
                src.EndInit();
                previewImg.Source = src;
            }
            else
            {
                MessageBox.Show("Not Found");
            }
        }
    }
}
