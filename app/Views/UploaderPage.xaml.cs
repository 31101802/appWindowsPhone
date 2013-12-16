using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System.Windows.Media.Imaging;
using System.IO;
using System.Net.Http;
using Microsoft.Practices.ServiceLocation;
using QuierobesarteApp.ViewModel;
using System.Threading.Tasks;
using System.IO.IsolatedStorage;
using QuierobesarteApp.Model;
using GalaSoft.MvvmLight.Messaging;

namespace QuierobesarteApp.Views
{
    public partial class UploaderPage : PhoneApplicationPage
    {

        PhotoChooserTask _photoChooserTask;
        string _fileName;
        string _weddingId;

        public UploaderPage()
        {
            InitializeComponent();


            _photoChooserTask = new PhotoChooserTask();
            _photoChooserTask.ShowCamera = true;
            _photoChooserTask.Completed +=
                new EventHandler<PhotoResult>(OnPhotoChooserTaskCompleted);
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            progressBar.Visibility = Visibility.Collapsed;
            if (NavigationContext.QueryString.TryGetValue("weddingId", out _weddingId))
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains("AppModel"))
                {
                    var appModel = (AppModel)IsolatedStorageSettings.ApplicationSettings["AppModel"];
                    appModel.ShowUserPanel = string.IsNullOrWhiteSpace(appModel.CurrentUserName);
                    if (appModel.ShowUserPanel && !appModel.CurrentWedding.IsActive)
                    {
                        appModel.ShowUserPanel = false;
                    }

                    IsolatedStorageSettings.ApplicationSettings["AppModel"] = appModel;
                    IsolatedStorageSettings.ApplicationSettings.Save();

                    Messenger.Default.Send<AppModel>(appModel);

                }
            }
        }




        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _photoChooserTask.Show();
        }

        private void btnViewImages_Click(object sender, RoutedEventArgs e)
        {
            progressBar.Visibility = Visibility.Visible;
            Dispatcher.BeginInvoke(() =>
            {
                NavigationService.Navigate(new Uri("/Views/ImageViewerPage.xaml?weddingId=" + _weddingId, UriKind.Relative));
            });
        }

        private void OnPhotoChooserTaskCompleted(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                if (MessageBox.Show("¿Estas seguro que quieres subir la foto?", string.Empty, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    _fileName = e.OriginalFileName;


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


                    //Delete Sentences


                    WriteableBitmap writeableBitmap = new WriteableBitmap(bitmapImage);
                    MemoryStream ms = new MemoryStream();
                    if (Path.GetExtension(e.OriginalFileName) == ".png")
                    {
                        writeableBitmap.WritePNG(ms, 80);
                    }
                    if (Path.GetExtension(e.OriginalFileName) == ".jpg")
                    {
                        writeableBitmap.SaveJpeg(ms, (int)resizeWidth, (int)resizeHeight, 0, 80);
                    }
                    UploadFile(ms);

                }
                else
                {
                    _photoChooserTask.Show();
                    //NavigationService.GoBack();
                }


            }
        }

        private async void UploadFile(MemoryStream ms)
        {
            Dispatcher.BeginInvoke(() =>
            {
                btnUploadImage.IsEnabled = false;
                btnViewImages.IsEnabled = false;
                progressBar.Visibility = System.Windows.Visibility.Visible;
            });



            if (ms != null)
            {
                var fileUploadUrl = @"http://" + App.baseUrl + "/Uploader/Upload/?guid=" + HttpUtility.UrlEncode(_weddingId);
                var client = Utils.GetClient();
                ms.Position = 0;
                MultipartFormDataContent content = new MultipartFormDataContent();
                content.Add(new StreamContent(ms), "file", _fileName);
                await client.PostAsync(fileUploadUrl, content)
                    .ContinueWith(async (postTask) =>
                    {
                        EnableButtons();

                        if (postTask.Result.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            Dispatcher.BeginInvoke(() =>
                            {
                                btnUploadImage.Visibility = Visibility.Collapsed;
                                MessageBox.Show("La boda '" + _weddingId + "' no esta activa por lo que no se pueden subir fotos.");
                            });

                            if (IsolatedStorageSettings.ApplicationSettings.Contains("AppModel"))
                            {
                                var data = (AppModel)IsolatedStorageSettings.ApplicationSettings["AppModel"];
                                data.CurrentWedding.IsActive = false;
                                IsolatedStorageSettings.ApplicationSettings["AppModel"] = data;
                                IsolatedStorageSettings.ApplicationSettings.Save();
                            }
                        }
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







        private void txbUserName_LostFocus(object sender, RoutedEventArgs e)
        {
            var userName = txbUserName.Text;
            if (!string.IsNullOrWhiteSpace(userName))
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains("AppModel"))
                {
                    var appModel = (AppModel)IsolatedStorageSettings.ApplicationSettings["AppModel"];
                    appModel.CurrentUserName = txbUserName.Text;
                    appModel.ShowUserPanel = false;
                    IsolatedStorageSettings.ApplicationSettings["AppModel"] = appModel;
                    IsolatedStorageSettings.ApplicationSettings.Save();

                    Messenger.Default.Send<AppModel>(appModel);
                }


            }
        }


    }
}