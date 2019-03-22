using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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
    public partial class MainWindow : Window
    {
        private MLApp.MLApp matlab;
        private string BaseDir = System.AppDomain.CurrentDomain.BaseDirectory;
        private OpenFileDialog openFileDialog;
        public MainWindow()
        {
            InitializeComponent();

            //Thread th = new Thread(Start_MLApp);
            //th.Start();

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
            //if (matlab == null)
            //{
            //    MessageBox.Show("App Not Completed initialization.Please wait.");
            //    return;
            //}
            //matlab.Execute($"fileLoc='{openFileDialog.FileName}'");
            //var result = matlab.Execute($"run('{BaseDir}/Recogniser/main.m');");
            //MessageBox.Show(result);
            //http://35.243.209.31/LPR/Recognize

            PostImage();
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

        public void PostImage()
        {
            HttpClient httpClient = new HttpClient();
            MultipartFormDataContent form = new MultipartFormDataContent();

            byte[] imagebytearraystring = ImageFileToByteArray(openFileDialog.FileName);
            form.Add(new ByteArrayContent(imagebytearraystring, 0, imagebytearraystring.Count()), "photo", openFileDialog.SafeFileName);
            HttpResponseMessage response = httpClient.PostAsync("http://35.243.209.31/Lpr/Recognize", form).Result;
            //HttpResponseMessage response = httpClient.PostAsync("http://localhost:5000/Lpr/Recognize", form).Result;

            httpClient.Dispose();
            var res = response.Content.ReadAsStringAsync().Result;
            if (res.Length > 1000)
            {
                MessageBox.Show("Server Error. Please Try Again");
            }
            else
            {
                var plates = res.Split('-');
                //for (int i = 1; i < plates.Length; i++)
                //{

                //}
                MessageBox.Show(res);
                MessageBox.Show(plates[1], "Most Confident Plate");
            }
        }

        private byte[] ImageFileToByteArray(string fullFilePath)
        {
            FileStream fs = File.OpenRead(fullFilePath);
            byte[] bytes = new byte[fs.Length];
            fs.Read(bytes, 0, Convert.ToInt32(fs.Length));
            fs.Close();
            return bytes;
        }

        private static string sendHttpRequest(string url, NameValueCollection values, NameValueCollection files = null)
        {
            string boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");
            // The first boundary
            byte[] boundaryBytes = System.Text.Encoding.UTF8.GetBytes("\r\n--" + boundary + "\r\n");
            // The last boundary
            byte[] trailer = System.Text.Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");
            // The first time it itereates, we need to make sure it doesn't put too many new paragraphs down or it completely messes up poor webbrick
            byte[] boundaryBytesF = System.Text.Encoding.ASCII.GetBytes("--" + boundary + "\r\n");

            // Create the request and set parameters
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            request.Method = "POST";
            request.KeepAlive = true;
            request.Credentials = System.Net.CredentialCache.DefaultCredentials;

            // Get request stream
            Stream requestStream = request.GetRequestStream();

            foreach (string key in values.Keys)
            {
                // Write item to stream
                byte[] formItemBytes = System.Text.Encoding.UTF8.GetBytes(string.Format("Content-Disposition: form-data; name=\"{0}\";\r\n\r\n{1}", key, values[key]));
                requestStream.Write(boundaryBytes, 0, boundaryBytes.Length);
                requestStream.Write(formItemBytes, 0, formItemBytes.Length);
            }

            if (files != null)
            {
                foreach (string key in files.Keys)
                {
                    if (File.Exists(files[key]))
                    {
                        int bytesRead = 0;
                        byte[] buffer = new byte[2048];
                        byte[] formItemBytes = System.Text.Encoding.UTF8.GetBytes(string.Format("Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: application/octet-stream\r\n\r\n", key, files[key]));
                        requestStream.Write(boundaryBytes, 0, boundaryBytes.Length);
                        requestStream.Write(formItemBytes, 0, formItemBytes.Length);

                        using (FileStream fileStream = new FileStream(files[key], FileMode.Open, FileAccess.Read))
                        {
                            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                            {
                                // Write file content to stream, byte by byte
                                requestStream.Write(buffer, 0, bytesRead);
                            }

                            fileStream.Close();
                        }
                    }
                }
            }

            // Write trailer and close stream
            requestStream.Write(trailer, 0, trailer.Length);
            requestStream.Close();

            using (StreamReader reader = new StreamReader(request.GetResponse().GetResponseStream()))
            {
                return reader.ReadToEnd();
            };
        }

    }
}
