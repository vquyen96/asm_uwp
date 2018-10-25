using ASM_uwp2.Entity;
using ASM_uwp2.Service;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ASM_uwp2.view
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ListView : Page
    {
        private Song currentSong;
        //private static string SONG_API_URL = "https://2-dot-backup-server-002.appspot.com/_api/v2/songs";
        private bool isPlaying = false;

        int onPlay = 0;
        Boolean myList = false;

        TimeSpan _position;

        DispatcherTimer _timer = new DispatcherTimer();

        private ObservableCollection<Song> listSong;
        private ObservableCollection<Song> listMySong;

        internal ObservableCollection<Song> ListSong { get => listSong; set => listSong = value; }
        internal ObservableCollection<Song> ListMySong { get => listMySong; set => listMySong = value; }
        Member member;

        public ListView()
        {
            this.currentSong = new Song();
            this.ListSong = new ObservableCollection<Song>();
            LoadAllSong(ListSong);
            this.ListMySong = new ObservableCollection<Song>();
            
            LoadMySong(ListMySong);
            Debug.WriteLine(ListMySong);
            GetMember();

            this.InitializeComponent();
            this.VolumeSlider.Value = 100;
            _timer.Interval = TimeSpan.FromMilliseconds(1000);
            _timer.Tick += ticktock;
            _timer.Start();
        }

        

        private async void LoadAllSong(ObservableCollection<Song> songs)
        {
            string content = await ApiHandle.Get_Song();
            ObservableCollection<Song> list_song = JsonConvert.DeserializeObject<ObservableCollection<Song>>(content);

            foreach (var data in list_song)
            {
                string str = data.thumbnail;
                string lastWord = str.Split('.').Last();
                if (lastWord.Contains("?"))
                {
                    Debug.WriteLine(lastWord);
                    lastWord = lastWord.Split('?')[0];
                    Debug.WriteLine(lastWord);
                }
                

                bool check = lastWord == "jpg" || lastWord == "png";
                if (!check)
                {
                    data.thumbnail = "https://api.adorable.io/avatars/175/" + data.name + ".png";
                }
                Debug.WriteLine(data.thumbnail);
                songs.Insert(0, data);
            }
        }
        private async void LoadMySong(ObservableCollection<Song> mysongs)
        {
            string content = await ApiHandle.Get_Mine_Song();
            ObservableCollection<Song> list_my_song = JsonConvert.DeserializeObject<ObservableCollection<Song>>(content);
            foreach (var data in list_my_song)
            {
                mysongs.Insert(0, data);
            }
        }

        private async void GetMember()
        {
            this.member = await ApiHandle.Get_info();
            avatar.ImageSource = new BitmapImage(new Uri(member.avatar));
            username.Text = member.email;
        }

        private void ticktock(object sender, object e)
        {
            MinDuration.Text = MediaPlayer.Position.Minutes + ":" + MediaPlayer.Position.Seconds;
            Progress.Minimum = 0;
            Progress.Maximum = MediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
            MaxDuration.Text = MediaPlayer.NaturalDuration.TimeSpan.Minutes + ":" + MediaPlayer.NaturalDuration.TimeSpan.Seconds;
            Progress.Value = MediaPlayer.Position.TotalSeconds;
            //Debug.WriteLine(MinDuration.Text + "--" + MaxDuration.Text);
            if (MinDuration.Text == MaxDuration.Text && MaxDuration.Text != "0:0")
            {
                Debug.WriteLine("Hết");
                MediaPlayer.Stop();
                if (myList)
                {
                    if (onPlay < ListMySong.Count - 1)
                    {
                        onPlay = onPlay + 1;
                    }
                    else
                    {
                        onPlay = 0;
                    }
                    LoadSong(ListMySong[onPlay]);
                    PlaySong();
                    MyList.SelectedIndex = onPlay;
                }
                else
                {
                    if (onPlay < ListSong.Count - 1)
                    {
                        onPlay = onPlay + 1;
                    }
                    else
                    {
                        onPlay = 0;
                    }
                    LoadSong(ListSong[onPlay]);
                    PlaySong();
                    MenuList.SelectedIndex = onPlay;
                }
                
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void StackPanel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            StackPanel panel = sender as StackPanel;
            Song selectedSong = panel.Tag as Song;
            Debug.WriteLine(ListSong[0].name);
            onPlay = MenuList.SelectedIndex;
            LoadSong(selectedSong);
            PlaySong();
        }
        private void StackPanel_Tapped_MySong(object sender, TappedRoutedEventArgs e)
        {
            StackPanel panel = sender as StackPanel;
            Song selectedSong = panel.Tag as Song;
            Debug.WriteLine(ListSong[0].name);
            onPlay = MyList.SelectedIndex;
            LoadSong(selectedSong);
            PlaySong();
            myList = true; 
        }

        private void PlaySong()
        {
            MediaPlayer.Play();
            PlayButton.Icon = new SymbolIcon(Symbol.Pause);
            isPlaying = true;
        }
        private void PauseSong()
        {
            MediaPlayer.Pause();
            PlayButton.Icon = new SymbolIcon(Symbol.Play);
            isPlaying = false;
        }
        private void PlayClick(object sender, RoutedEventArgs e)
        {
            if (isPlaying)
            {
                PauseSong();
            }
            else
            {
                PlaySong();
            }
        }
        private void PlayBack(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Stop();
            if (onPlay > 0)
            {
                onPlay = onPlay - 1;
            }
            else
            {
                onPlay = ListSong.Count - 1;
            }
            LoadSong(ListSong[onPlay]);
            PlaySong();
            MenuList.SelectedIndex = onPlay;
        }

        private void PlayNext(object sender, RoutedEventArgs e)
        {
            MediaPlayer.Stop();
            if (onPlay < ListSong.Count - 1)
            {
                onPlay = onPlay + 1;
            }
            else
            {
                onPlay = 0;
            }
            LoadSong(ListSong[onPlay]);
            PlaySong();
            MenuList.SelectedIndex = onPlay;
        }
        private void LoadSong(Entity.Song currentSong)
        {
            this.NowPlaying.Text = "Loading";
            MediaPlayer.Source = new Uri(currentSong.link);
            Debug.WriteLine(MediaPlayer.NaturalDuration.TimeSpan.TotalSeconds);
            this.NowPlaying.Text = currentSong.name + " - " + currentSong.singer;
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (MediaPlayer.Source != null && MediaPlayer.NaturalDuration.HasTimeSpan)
            {
                Progress.Minimum = 0;
                Progress.Maximum = MediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                Progress.Value = MediaPlayer.Position.TotalSeconds;

            }
        }

        private void VolumeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Slider vol = sender as Slider;
            if (vol != null)
            {
                MediaPlayer.Volume = vol.Value / 100;
                this.volume.Text = vol.Value.ToString();
            }
        }

        private void MenuList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private async void BtnSignup_Click(object sender, RoutedEventArgs e)
        {
            bool validate = true;
            this.currentSong.name = this.Name.Text;
            this.currentSong.description = this.Description.Text;
            this.currentSong.singer = this.Singer.Text;
            this.currentSong.author = this.Author.Text;
            this.currentSong.thumbnail = this.Thumbnail.Text;
            this.currentSong.link = this.Link.Text;

            if (this.currentSong.name == "")
            {
                validate = false;
                name.Text = "Ten khong duoc de trong!";
                name.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
                Name.Focus(FocusState.Programmatic);
            }
            else
            {
                name.Text = "";
            }

            if (this.currentSong.description == "")
            {
                validate = false;
                description.Text = "Chi tiet khong duoc de trong!";
                description.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
                Description.Focus(FocusState.Programmatic);
            }
            else
            {
                description.Text = "";
            }

            if (this.currentSong.singer == "")
            {
                validate = false;
                singer.Text = "Ca si khong duoc de trong!";
                singer.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
                Singer.Focus(FocusState.Programmatic);
            }
            else
            {
                singer.Text = "";
            }

            if (this.currentSong.author == "")
            {
                validate = false;
                author.Text = "Tac gia khong duoc de trong!";
                author.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
                Author.Focus(FocusState.Programmatic);
            }
            else
            {
                author.Text = "";
            }

            if (this.currentSong.thumbnail == "")
            {
                validate = false;
                thumbnail.Text = "Anh dai dien khong duoc de trong!";
                thumbnail.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
                Thumbnail.Focus(FocusState.Programmatic);
            }
            else
            {
                thumbnail.Text = "";
            }

            if (this.currentSong.link == "")
            {
                validate = false;
                link.Text = "Link khong duoc de trong!";
                link.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
                Link.Focus(FocusState.Programmatic);
            }
            else
            {
                link.Text = "";
            }

            if (validate)
            {
                await ApiHandle.Create_Song(this.currentSong);
                Debug.WriteLine("Action success.");
                var _Frame = Window.Current.Content as Frame;
                _Frame.Navigate(_Frame.Content.GetType());
                _Frame.GoBack();
            }
            
        }

        private async void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            StorageFile file = await folder.CreateFileAsync("token.txt", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, "");
            Login login = new Login();
            await login.ShowAsync();
        }

        public async Task<bool> isFilePresent(string fileName)
        {
            var item = await ApplicationData.Current.LocalFolder.TryGetItemAsync(fileName);
            return item != null;
        }
    }
}
