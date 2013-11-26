using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using QuierobesarteApp.Resources;
using Microsoft.Phone.Tasks;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Media.Imaging;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Windows.Storage.Streams;

namespace QuierobesarteApp
{
    public partial class MainPage1 : PhoneApplicationPage
    {

        PhotoChooserTask photoChooserTask;
        string fileName;
        //string baseUrl = "10.0.2.18:8001";
        //string baseUrl = "10.1.2.233:8001";
        string baseUrl = "loreto.cc";
        decimal currentWeddingId;


        // Constructor
        public MainPage1()
        {
            InitializeComponent();




            // Set the data context of the LongListSelector control to the sample data
            DataContext = App.ViewModel;
            photoChooserTask = new PhotoChooserTask();
            photoChooserTask.ShowCamera = true;
            photoChooserTask.Completed +=
                new EventHandler<PhotoResult>(OnPhotoChooserTaskCompleted);
         
        }


        private void OnChoosePicture(object sender,
                     System.Windows.Input.GestureEventArgs e)
        {
            photoChooserTask.Show();
        }
        // Called when an existing photo is chosen with the photo chooser.
        private async void OnPhotoChooserTaskCompleted(object sender, PhotoResult e)
        {

            // Make sure the PhotoChooserTask is resurning OK
            if (e.TaskResult == TaskResult.OK)
            {
                fileName = e.OriginalFileName;


                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.SetSource(e.ChosenPhoto);
                decimal resizeWidth;
                decimal resizeHeight;
                decimal ratio;

                if (bitmapImage.PixelWidth > bitmapImage.PixelHeight)
                {
                    ratio = (decimal)bitmapImage.PixelWidth / (decimal)bitmapImage.PixelHeight;
                    resizeWidth = 1920;
                    resizeHeight = 1920 / ratio;
                }
                else
                {
                    ratio = (decimal)bitmapImage.PixelHeight / (decimal)bitmapImage.PixelWidth;
                    resizeHeight = 1920;
                    resizeWidth = 1920 / ratio;
                }

               

                WriteableBitmap writeableBitmap = new WriteableBitmap(bitmapImage);
                MemoryStream ms = new MemoryStream();
                writeableBitmap.SaveJpeg(ms, (int)resizeWidth, (int)resizeHeight, 0, 80);
                UploadFile(ms);

                /*
                var photoStream = new MemoryStream();
                e.ChosenPhoto.CopyTo(photoStream);
                UploadFile(photoStream);
                 * */

            }

        }

      

       
        // uploads the file
        private async void UploadFile(MemoryStream ms)
        {

            btnUploadImage.IsEnabled = false;
            btnViewImages.IsEnabled = false;
            progressBar.Visibility = System.Windows.Visibility.Visible;
            // Make sure there is a picture selected



            if (ms != null)
            {
                var fileUploadUrl = @"http://" + baseUrl + "/Uploader/Upload/?guid=" + currentWeddingId;
                var client = new HttpClient();
                ms.Position = 0;
                MultipartFormDataContent content = new MultipartFormDataContent();
                content.Add(new StreamContent(ms), "file", fileName);
                await client.PostAsync(fileUploadUrl, content)
                    .ContinueWith((postTask) =>
                    {
                        EnableButtons();
                        //postTask.Result.EnsureSuccessStatusCode();

                    });
            }



        }

        private void EnableButtons()
        {
            Dispatcher.BeginInvoke(() =>
            {
                btnUploadImage.IsEnabled = true;
                btnViewImages.IsEnabled = true;
                progressBar.Visibility = System.Windows.Visibility.Collapsed;
            });

        }

        public byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[input.Length];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public async Task<string> UploadImage(byte[] image)
        {
            var client = new HttpClient();

            var content = new MultipartFormDataContent();

            var imageContent = new ByteArrayContent(image);

            imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");

            content.Add(imageContent, "image", string.Format("{0}.jpg", Guid.NewGuid().ToString()));

            return await client.PostAsync("http://" + baseUrl + "/Uploader/Upload/?guid=" + currentWeddingId, content)
                .Result.Content.ReadAsStringAsync()
                .ContinueWith(t =>
            {
                return t.Result;
            });
        }

        // Load data for the ViewModel Items
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            
        }



        private void Button_Click(object sender, RoutedEventArgs e)
        {
            photoChooserTask.Show();
        }

        private async void Enviar_Click(object sender, RoutedEventArgs e)
        {
            var weddingId = this.weddingPublicId.Text;



            if (this.weddingPublicId.Text != string.Empty)
            {
                var client = new HttpClient();
                await client.GetStringAsync("http://" + baseUrl + "/api/Wedding/" + weddingId)
                   .ContinueWith(t =>
                   {

                       var wedding = JsonConvert.DeserializeObject<WeddingDto>(t.Result);
                       if (wedding != null)
                       {

                           currentWeddingId = decimal.Parse(weddingId);
                           ShowUploaderView();
                       }
                       else
                       {
                           ShowErrorMessage(weddingId);
                       }


                   });

            }


        }

        void ShowUploaderView()
        {
            Dispatcher.BeginInvoke(() =>
            {
                this.loginPanel.Visibility = System.Windows.Visibility.Collapsed;
                this.uploaderPanel.Visibility = System.Windows.Visibility.Visible;
                this.viewer.Visibility = System.Windows.Visibility.Collapsed;
            });
        }

        void ShowViewer()
        {
            Dispatcher.BeginInvoke(() =>
            {
                this.loginPanel.Visibility = System.Windows.Visibility.Collapsed;
                this.uploaderPanel.Visibility = System.Windows.Visibility.Collapsed;
                this.viewer.Visibility = System.Windows.Visibility.Visible;
            });
        }

        void ShowErrorMessage(string weddingPublicId)
        {
            Dispatcher.BeginInvoke(() =>
            {
                MessageBox.Show("El número '" + weddingPublicId + "' no corresponde a ninguna boda activa. ¡Verifica el número e intentalo de nuevo!.");
            });
        }

        private async void btnViewImages_Click(object sender, RoutedEventArgs e)
        {
            var client = new HttpClient();


            HttpResponseMessage getresponse = await client.GetAsync("http://" + baseUrl + "/api/images/" + currentWeddingId + "?page=0&numItems=30");
            string json = await getresponse.Content.ReadAsStringAsync();
            var imeges = JsonConvert.DeserializeObject<List<ImageDto>>(json);

            foreach (var item in imeges)
            {
                item.originalPath = "http://" + baseUrl + item.originalPath;
                item.thumbnailPath = "http://" + baseUrl + item.thumbnailPath;
            }
            imageItems.ItemsSource = imeges;
            ShowViewer();


        }


        private void Ellipse_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (currentWeddingId > 0)
            {
                ShowUploaderView();
            }

        }

    }

 


 
}