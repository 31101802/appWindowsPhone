using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using QuierobesarteApp.Model;
using System;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace QuierobesarteApp.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        AppModel _appModel;
        RelayCommand<string> _openUrlInBrowserCommand;

        public RelayCommand<string> OpenUrlInBrowserCommand
        {
            get { return _openUrlInBrowserCommand; }
            private set { _openUrlInBrowserCommand = value; }
        }

        public MainViewModel()
        {
            if (IsInDesignMode)
            {
                // Code runs in Blend --> create design time data.
                for (int i = 0; i < 10; i++)
                {
                    Images.Add(new ImageDto { });
                }

                CurrentImage = "http://" + App.baseUrl + "/uploads/_MG_7004.jpg";
                CurrentWedding = new WeddingDto
                {
                    Name = "Boda de Juan y Marta",
                    Id = "JuanMarta2014"
                };

            }
            else
            {
                Messenger.Default.Register<AppModel>(this, (appModel) =>
                {
                    _appModel = appModel;
                    this.RaisePropertyChanged("IsWeddingActive");
                    this.RaisePropertyChanged("IsUserPanelVisivility");
                    CurrentWedding = _appModel.CurrentWedding;

                });

                _onImageTapCommand = new RelayCommand<string>(OnImageTap);
                _openUrlInBrowserCommand = new RelayCommand<string>(OpenUrlInBrowser);
            }

        }

        private void OpenUrlInBrowser(string url)
        {
            var task = new Microsoft.Phone.Tasks.WebBrowserTask { URL = url };
            task.Show();
        }

        RelayCommand<string> _onImageTapCommand;

        public RelayCommand<string> OnImageTapCommand
        {
            get { return _onImageTapCommand; }
            private set { _onImageTapCommand = value; }
        }




        ObservableCollection<ImageDto> _images = new ObservableCollection<ImageDto>();

        public ObservableCollection<ImageDto> Images
        {
            get { return _images; }
            set
            {
                _images = value;
                this.RaisePropertyChanged("Images");
            }
        }


        string _currentImage;

        public string CurrentImage
        {
            get { return _currentImage; }
            set
            {
                _currentImage = value;
                this.RaisePropertyChanged("CurrentImage");
            }
        }

        private void OnImageTap(string image)
        {
            CurrentImage = image;

        }



        public Visibility IsWeddingActive
        {
            get
            {
                Visibility ret = System.Windows.Visibility.Visible;

                if (IsolatedStorageSettings.ApplicationSettings.Contains("AppModel"))
                {
                    var appModel = (AppModel)IsolatedStorageSettings.ApplicationSettings["AppModel"];
                    if (appModel != null && appModel.CurrentWedding != null)
                    {
                        ret = appModel.CurrentWedding.IsActive ? Visibility.Visible : Visibility.Collapsed;
                    }

                }
                return ret;
            }

        }

        public Visibility IsUserPanelVisivility
        {
            get
            {
                Visibility ret = System.Windows.Visibility.Visible;

                if (IsolatedStorageSettings.ApplicationSettings.Contains("AppModel"))
                {
                    var appModel = (AppModel)IsolatedStorageSettings.ApplicationSettings["AppModel"];
                    if (appModel != null)
                    {
                        ret = appModel.ShowUserPanel ? Visibility.Visible : Visibility.Collapsed;
                    }

                }
                return ret;
            }

        }

        WeddingDto _currentWedding;

        public WeddingDto CurrentWedding
        {
            get { return _currentWedding; }
            set
            {
                _currentWedding = value;
                this.RaisePropertyChanged("CurrentWedding");
            }
        }




        public bool IsLoading { get; set; }
    }
}