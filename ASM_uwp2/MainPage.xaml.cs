using ASM_uwp2.Entity;
using ASM_uwp2.Service;
using ASM_uwp2.view;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ASM_uwp2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>

    public sealed partial class MainPage : Page
    {

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

            StorageFile sampleFile = await localFolder.GetFileAsync("token.txt");
            Debug.WriteLine(sampleFile);
            string timestamp = await FileIO.ReadTextAsync(sampleFile);
            TokenResponse token = JsonConvert.DeserializeObject<TokenResponse>(timestamp);
            Debug.WriteLine(token.token);

            HttpClient client2 = new HttpClient();
            //client2.DefaultRequestHeaders.Add("Authorization", "Basic " + token);
            client2.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", token.token);
            var resp = client2.GetAsync("http://2-dot-backup-server-002.appspot.com/_api/v2/members/information").Result;
            Debug.WriteLine(resp);
            Debug.WriteLine(resp.StatusCode);
            Debug.WriteLine(System.Net.HttpStatusCode.Created);
            if (resp.StatusCode == System.Net.HttpStatusCode.Created)
            {
                var rootFrame = Window.Current.Content as Frame;
                rootFrame.Navigate(typeof(view.SplitView));
            }
            else
            {
                Login login = new Login();
                await login.ShowAsync();
            }

            
        }

        private void TextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {


        }
    }
    //public sealed partial class MainPage : Page
    //{
    //    //MediaPlayer mediaPlayer = new MediaPlayer();

    //    public MainPage()
    //    {
    //        this.InitializeComponent();
    //    }
    //    private async void Page_Loaded(object sender, RoutedEventArgs e)
    //    {
    //        Login login = new Login();
    //        await login.ShowAsync();
    //    }
    //    private void TextBlock_SelectionChanged(object sender, RoutedEventArgs e)
    //    {


    //    }

    //    //private void Button_Click(object sender, RoutedEventArgs e)
    //    //{
    //    //    //mediaPlayer.Source = MediaSource.CreateFromUri(new Uri("http://data.chiasenhac.com/downloads/1963/6/1962559-f4a83884/128/Thang%20Dien%20-%20Justatee_%20Phuong%20Ly.mp3"));
    //    //    //mediaPlayer.Play();
    //    //    var session = mediaPlayer.PlaybackSession;
    //    //    session.Position = session.Position + TimeSpan.FromSeconds(10);
    //    //}
    //    //private void Button_Click_Stop(object sender, RoutedEventArgs e)
    //    //{
    //    //    //MediaPlayer.Dispose();
    //    //    _mediaPlayerElement.SetMediaPlayer(mediaPlayer);
    //    //    _mediaPlayerElement.Source = MediaSource.CreateFromUri(new Uri("http://data.chiasenhac.com/downloads/1963/6/1962559-f4a83884/128/Thang%20Dien%20-%20Justatee_%20Phuong%20Ly.mp3"));
    //    //    mediaPlayer = _mediaPlayerElement.MediaPlayer;
    //    //    mediaPlayer.Play();
    //    //}

    //    //private void Button_Click_1(object sender, RoutedEventArgs e)
    //    //{
    //    //    mediaPlayer.PlaybackSession.PlaybackRate = 2.0;
    //    //}

    //    //private void Button_Click_2(object sender, RoutedEventArgs e)
    //    //{
    //    //    //mediaPlayer.PlaybackSession.PlaybackRate = 1.0;
    //    //    mediaPlayer.PlaybackSession.PlaybackRotation = MediaRotation.Clockwise90Degrees;

    //    //}


    //}
}
