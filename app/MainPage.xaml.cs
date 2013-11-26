using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Newtonsoft.Json;
using System.Net.Http;
using QuierobesarteApp.Model;
using Windows.Storage;
using Windows.Storage.Streams;
using QuierobesarteApp.ViewModel;
using Microsoft.Practices.ServiceLocation;
using GalaSoft.MvvmLight.Messaging;
using System.IO.IsolatedStorage;

namespace QuierobesarteApp
{
    public partial class MainPage : PhoneApplicationPage
    {
        public MainPage()
        {
            InitializeComponent();
            this.Loaded += MainPage_Loaded;

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            txbWeddingId.Text = string.Empty;
            //Utils.LocalizationMessage();
        }



        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.CurrentNavigationService == null)
            {
                App.CurrentNavigationService = base.NavigationService;
            }
        }



        private async void Enviar_Click(object sender, RoutedEventArgs e)
        {
            progressBar.Visibility = System.Windows.Visibility.Visible;
            btnEnviar.IsEnabled = false;

            var weddingId = this.txbWeddingId.Text;

            if (!string.IsNullOrWhiteSpace(weddingId))
            {
                var client = Utils.GetClient();
                var result = await client.GetAsync("http://" + App.baseUrl + "/api/Wedding/" + HttpUtility.UrlEncode(weddingId));

                if ((int)result.StatusCode == 426)
                {
                    ShowIncorrectClientVersionMessage();
                    progressBar.Visibility = System.Windows.Visibility.Collapsed;
                    btnEnviar.IsEnabled = true;
                    txbWeddingId.Text = string.Empty;
                    return;
                }

                var content = await result.Content.ReadAsStringAsync();

                var wedding = JsonConvert.DeserializeObject<WeddingDto>(content);
                if (wedding != null)
                {
                    AppModel appModel = new AppModel() { CurrentWedding = wedding };


                    IsolatedStorageSettings.ApplicationSettings["AppModel"] = appModel;
                    IsolatedStorageSettings.ApplicationSettings.Save();

                    Dispatcher.BeginInvoke(() =>
                    {

                        NavigationService.Navigate(new Uri("/Views/UploaderPage.xaml?weddingId=" + weddingId, UriKind.Relative));

                    });

                }
                else
                {
                    ShowErrorMessage(weddingId);

                }

                Dispatcher.BeginInvoke(() =>
                {
                    progressBar.Visibility = System.Windows.Visibility.Collapsed;
                    btnEnviar.IsEnabled = true;
                });



            }
            else
            {
                progressBar.Visibility = System.Windows.Visibility.Collapsed;
                btnEnviar.IsEnabled = true;
            }

        }
        void ShowErrorMessage(string weddingPublicId)
        {
            Dispatcher.BeginInvoke(() =>
            {
                MessageBox.Show("El número '" + weddingPublicId + "' no corresponde a ninguna boda activa. ¡Verifica el número e intentalo de nuevo!.");
            });
        }

        void ShowIncorrectClientVersionMessage()
        {
            Dispatcher.BeginInvoke(() =>
            {
                MessageBox.Show("La aplicacion necesita actulizarse para poder funcionar correctamente. Por favor descargate la ultima versión desde el MarketPlace.");
            });
        }


    }
}