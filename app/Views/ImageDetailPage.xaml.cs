using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Resources;
using System.IO.IsolatedStorage;
using System.Windows.Media.Imaging;
using System.IO;
using Microsoft.Xna.Framework.Media;
using QuierobesarteApp.ViewModel;
using QuierobesarteApp.Resources;

namespace QuierobesarteApp.Views
{
    public partial class ImageDetailPage : PhoneApplicationPage
    {
        public ImageDetailPage()
        {
            InitializeComponent();

            progressBar.Visibility = System.Windows.Visibility.Collapsed;
            BuildLocalizedApplicationBar();
        }

        private void SaveCurrentImage()
        {
            var vainViewModel = this.DataContext as MainViewModel;
            var webClient = new WebClient();
            webClient.OpenReadCompleted += WebClientOpenReadCompleted;
            webClient.OpenReadAsync(new Uri(vainViewModel.CurrentImage, UriKind.Absolute));
        }

        void WebClientOpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            const string tempJpeg = "TempJPEG";
            var streamResourceInfo = new StreamResourceInfo(e.Result, null);

            var userStoreForApplication = IsolatedStorageFile.GetUserStoreForApplication();
            if (userStoreForApplication.FileExists(tempJpeg))
            {
                userStoreForApplication.DeleteFile(tempJpeg);
            }

            var isolatedStorageFileStream = userStoreForApplication.CreateFile(tempJpeg);

            var bitmapImage = new BitmapImage { CreateOptions = BitmapCreateOptions.None };
            bitmapImage.SetSource(streamResourceInfo.Stream);

            var writeableBitmap = new WriteableBitmap(bitmapImage);
            writeableBitmap.SaveJpeg(isolatedStorageFileStream, writeableBitmap.PixelWidth, writeableBitmap.PixelHeight, 0, 85);

            isolatedStorageFileStream.Close();
            isolatedStorageFileStream = userStoreForApplication.OpenFile(tempJpeg, FileMode.Open, FileAccess.Read);

            // Save the image to the camera roll or saved pictures album.
            var mediaLibrary = new MediaLibrary();

            // Save the image to the saved pictures album.
            mediaLibrary.SavePicture(string.Format("SavedPicture{0}.jpg", DateTime.Now), isolatedStorageFileStream);

            isolatedStorageFileStream.Close();

            progressBar.Visibility = System.Windows.Visibility.Collapsed;
          
        }

        // Sample code for building a localized ApplicationBar
        private void BuildLocalizedApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();



            // Create a new menu item with the localized string from AppResources.
            ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
            appBarMenuItem.Click += appBarMenuItem_Click;
            ApplicationBar.MenuItems.Add(appBarMenuItem);
        }

        void appBarMenuItem_Click(object sender, EventArgs e)
        {
            progressBar.Visibility = System.Windows.Visibility.Visible;
            this.SaveCurrentImage();
        }

    }
}