using ASM_uwp2.Entity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ASM_uwp2.Service
{
    class ApiHandle
    {   
        private static string API_URL = "https://2-dot-backup-server-002.appspot.com/_api/v2/members";
        private static string SONG_API_URL = "https://2-dot-backup-server-002.appspot.com/_api/v2/songs";
        private static string MINE_API_URL = "https://2-dot-backup-server-002.appspot.com/_api/v2/songs/get-mine";
        private static string INFO_URL = "http://2-dot-backup-server-002.appspot.com/_api/v2/members/information";



        public static async Task<Boolean> Check_info()
        {
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", await Token_local());
            var response = httpClient.GetAsync(INFO_URL).Result;
            var contents = await response.Content.ReadAsStringAsync();
            Debug.WriteLine(contents);
            if (response.StatusCode == System.Net.HttpStatusCode.Created)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static async Task<Member> Get_info()
        {
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", await Token_local());
            var response = httpClient.GetAsync(INFO_URL).Result;
            var contents = await response.Content.ReadAsStringAsync();
            Member member;
            member = JsonConvert.DeserializeObject<Member>(contents);
            return member;
            //if (response.StatusCode == System.Net.HttpStatusCode.Created)
            //{
                
            //}
            
            
            
        }


        public static async Task<string> Token_local()
        {
            Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

            StorageFile sampleFile = await localFolder.GetFileAsync("token.txt");
            
            Debug.WriteLine(sampleFile);
            String timestamp = await FileIO.ReadTextAsync(sampleFile);
            Debug.WriteLine(timestamp);
            if (timestamp == "" || timestamp == "null")
            {
                return "";
            }
            else
            {
                TokenResponse token = JsonConvert.DeserializeObject<TokenResponse>(timestamp);
                Debug.WriteLine(token);
            
                return token.token;
            }
            
        }

        public async static Task<bool> Sign_Up(Member member)
        {
            HttpClient httpClient = new HttpClient();
            var content = new StringContent(JsonConvert.SerializeObject(member), System.Text.Encoding.UTF8, "application/json");
            var response = httpClient.PostAsync(API_URL, content);
            var contents = await response.Result.Content.ReadAsStringAsync();
            Debug.WriteLine(contents);
            Debug.WriteLine(response.Result.StatusCode);
            if (response.Result.StatusCode == System.Net.HttpStatusCode.Created)
            {
                return true;
            }
            else
            {
                return false;
            }
                
        }

        
        public async static Task<string> Create_Song(Song song)
        {
            
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", await Token_local());
            var content = new StringContent(JsonConvert.SerializeObject(song), System.Text.Encoding.UTF8, "application/json");
            var response = httpClient.PostAsync(SONG_API_URL, content);
            var contents = await response.Result.Content.ReadAsStringAsync();
            Debug.WriteLine(contents);
            return contents;
        }
        public static async Task<string> Get_Song()
        {
            //List<Song> list_song = await GetListAsync();
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", await Token_local());
            //var content = new StringContent(JsonConvert.SerializeObject(song), System.Text.Encoding.UTF8, "application/json");
            var response = httpClient.GetAsync(SONG_API_URL);
            string contents = await response.Result.Content.ReadAsStringAsync();
            return contents;
        }
        public async static Task<string> Get_Mine_Song()
        {
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", await Token_local());
            //var content = new StringContent(JsonConvert.SerializeObject(song), System.Text.Encoding.UTF8, "application/json");
            var response = httpClient.GetAsync(MINE_API_URL);
            Debug.WriteLine(response);
            string contents = await response.Result.Content.ReadAsStringAsync();
            Debug.WriteLine(contents);
            return contents;
        }
    }
}
