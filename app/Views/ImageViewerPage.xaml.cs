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
        int _offsetKnob = 12;
        int _pageNumber = 1;
        MainViewModel _viewModel;

        public ImageViewerPage()
        {
            InitializeComponent();
            LongList.GridCellSize = new Size(155, 155);
            LongList.LayoutMode = LongListSelectorLayoutMode.Grid;
            LongList.ItemRealized += LongList_ItemRealized;
            _viewModel = this.DataContext as MainViewModel;
        }

        void LongList_ItemRealized(object sender, ItemRealizationEventArgs e)
        {
            if (!_viewModel.IsLoading && LongList.ItemsSource != null && LongList.ItemsSource.Count >= _offsetKnob)
            {
                if (e.ItemKind == LongListSelectorItemKind.Item)
                {
                    if ((e.Container.Content as ImageDto).Equals(LongList.ItemsSource[LongList.ItemsSource.Count - _offsetKnob]))
                    {
                        Debug.WriteLine("Searching for {0}", _pageNumber);
                        // _viewModel.LoadPage(_searchTerm, _pageNumber++);
                        AddItems(_pageNumber++);
                    }
                }
            }
        }

        string _weddingId;


        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (NavigationContext.QueryString.TryGetValue("weddingId", out _weddingId))
            {
                await AddItems(1);

            }
        }

        private async System.Threading.Tasks.Task AddItems(int page)
        {
            var client = Utils.GetClient();


            HttpResponseMessage getresponse = await client.GetAsync("http://" + App.baseUrl + "/api/images/" +
                HttpUtility.UrlEncode(_weddingId) + "?page=" + (_pageNumber - 1) + "&numItems=" + _offsetKnob);
            string json = await getresponse.Content.ReadAsStringAsync();
            var images = JsonConvert.DeserializeObject<List<ImageDto>>(json);
            images = images.OrderBy(i => i.created).ToList();

            foreach (var item in images)
            {
                item.originalPath = "http://" + App.baseUrl + item.originalPath;
                item.thumbnailPath = "http://" + App.baseUrl + item.thumbnailPath;
            }

            var mainViewModel = ServiceLocator.Current.GetInstance<MainViewModel>();
             
        
            images.ForEach((image) =>
            {
                if (!mainViewModel.Images.Any(i=>i.id==image.id))
                {
                    mainViewModel.Images.Insert(0,image);
                }
            });

        }


        private void Image_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                NavigationService.Navigate(new Uri("/Views/ImageDetailPage.xaml", UriKind.Relative));
            });
        }


        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            progressBar.Visibility = System.Windows.Visibility.Collapsed;
        }






    }
}