using Newtonsoft.Json;
using QuierobesarteApp.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.Devices.Geolocation;
using Windows.Storage;
using Windows.Storage.Streams;

namespace QuierobesarteApp
{
    public class Utils
    {


       
        public static HttpClient GetClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("App-Version", App.CurrentVersion.ToString(new CultureInfo("en-US")));
            client.DefaultRequestHeaders.Add("App-Device", Environment.OSVersion.Version.ToString());
            client.DefaultRequestHeaders.Add("App-Platform", Environment.OSVersion.Platform.ToString());

            if (IsolatedStorageSettings.ApplicationSettings.Contains("AppModel"))
            {
                var appModel = (AppModel)IsolatedStorageSettings.ApplicationSettings["AppModel"];
                if (appModel != null && !string.IsNullOrWhiteSpace(appModel.CurrentUserName))
                {
                    client.DefaultRequestHeaders.Add("App-User", appModel.CurrentUserName);
                }
            }

            return client;
        }

        public static void LocalizationMessage()
        {
            if (IsolatedStorageSettings.ApplicationSettings.Contains("LocationConsent"))
            {
                // User has opted in or out of Location
                return;
            }
            else
            {
                MessageBoxResult result =
                    MessageBox.Show("¿Permite acceder a su localización?",
                    "Location",
                    MessageBoxButton.OKCancel);

                if (result == MessageBoxResult.OK)
                {
                    IsolatedStorageSettings.ApplicationSettings["LocationConsent"] = true;
                }
                else
                {
                    IsolatedStorageSettings.ApplicationSettings["LocationConsent"] = false;
                }

                IsolatedStorageSettings.ApplicationSettings.Save();
            }
        }

        private async Task<Geocoordinate> GetLoaction(object sender, RoutedEventArgs e)
        {

            if ((bool)IsolatedStorageSettings.ApplicationSettings["LocationConsent"] != true)
            {
                // The user has opted out of Location.
                return null;
            }

            Geolocator geolocator = new Geolocator();
            geolocator.DesiredAccuracyInMeters = 50;

            try
            {
                var c = await geolocator.GetGeopositionAsync(maximumAge: TimeSpan.FromMinutes(5), timeout: TimeSpan.FromSeconds(10));

                return c.Coordinate;


            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
