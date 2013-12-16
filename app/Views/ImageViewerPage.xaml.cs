using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Net.Http;
using Newtonsoft.Json;
using Microsoft.Practices.ServiceLocation;
using QuierobesarteApp.ViewModel;
using QuierobesarteApp.Model;
using System.Diagnostics;

namespace QuierobesarteApp.Views
{
    public partial class ImageViewerPage : PhoneApplicationPage
    {
        int _offsetKnob = 3;
        int _pageNumber = 1;
        MainViewModel _viewModel;
        static bool _isLoading = false;

        public ImageViewerPage()
        {
            InitializeComponent();
            LongList.GridCellSize = new Size(155, 155);
            LongList.LayoutMode = LongListSelectorLayoutMode.Grid;
            LongList.ItemRealized += LongList_ItemRealized;
            _viewModel = this.DataContext as MainViewModel;
            this.Loaded += ImageViewerPage_Loaded;
        }



        async void LongList_ItemRealized(object sender, ItemRealizationEventArgs e)
        {
            if (!_isLoading && LongList.ItemsSource != null && LongList.ItemsSource.Count >= _offsetKnob)
            {
                if (e.ItemKind == LongListSelectorItemKind.Item)
                {
                    var realizedContent = e.Container.Content as ImageDto;
                    var currentContetnt = LongList.ItemsSource[LongList.ItemsSource.Count - _offsetKnob] as ImageDto;
                    if (realizedContent.id == currentContetnt.id)
                    {
                        _isLoading = true;
                        _pageNumber++;
                        await AddItems(_pageNumber);
                        //_isLoading = false;
                    }
                }
            }
        }

        string _weddingId;


        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _pageNumber = 1;

            NavigationContext.QueryString.TryGetValue("weddingId", out _weddingId);

            if (LongList.ItemsSource == null || (LongList.ItemsSource != null && LongList.ItemsSource.Count == 0))
            {
                Dispatcher.BeginInvoke(() =>
                {
                    progressBar.Visibility = System.Windows.Visibility.Visible;
                });

                var mainViewModel = ServiceLocator.Current.GetInstance<MainViewModel>();
                mainViewModel.Images.Clear();
                _pageNumber = 1;
                await AddItems(_pageNumber);
                Dispatcher.BeginInvoke(() =>
                {
                    progressBar.Visibility = System.Windows.Visibility.Collapsed;
                });
            }
        }

        async void ImageViewerPage_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private async System.Threading.Tasks.Task AddItems(int page)
        {
            Debug.WriteLine("Searching for {0}", page);
            _isLoading = true;
            var client = Utils.GetClient();
            var url = "http://" + App.baseUrl + "/api/images/" + HttpUtility.UrlEncode(_weddingId) + "?page=" + page +
                      "&numItems=18";

            try
            {
                var getresponse = await client.GetAsync(url);
                string json = await getresponse.Content.ReadAsStringAsync();
                var images = JsonConvert.DeserializeObject<List<ImageDto>>(json);

                foreach (var item in images)
                {
                    item.originalPath = "http://" + App.baseUrl + item.originalPath;
                    item.thumbnailPath = "http://" + App.baseUrl + item.thumbnailPath;
                }

                var mainViewModel = ServiceLocator.Current.GetInstance<MainViewModel>();


                images.ForEach((image) => mainViewModel.Images.Add(image));

                _isLoading = false;
            }
            catch
            {
                _isLoading = false;
            }

           

        }


        private void Image_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                NavigationService.Navigate(new Uri("/Views/ImageDetailPage.xaml", UriKind.Relative));
            });
        }





    }
}