using ASM_uwp2.Entity;
using ASM_uwp2.Service;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ASM_uwp2.view
{
   
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Sign : Page
    {
        private Member currentMember;
        private static StorageFile file;
        private static string UploadUrl;
        public Sign()
        {
            this.currentMember = new Member();
            this.InitializeComponent();
            GetUploadUrl();
        }


        private async void Capture_Photo(object sender, RoutedEventArgs e)
        {
            CameraCaptureUI captureUI = new CameraCaptureUI();
            captureUI.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
            captureUI.PhotoSettings.CroppedSizeInPixels = new Size(200, 200);
            file = await captureUI.CaptureFileAsync(CameraCaptureUIMode.Photo);
            if (file == null)
            {
                // User cancelled photo capture
                return;
            }
            HttpUploadFile(UploadUrl, "myFile", "image/png");
        }

        private static async void GetUploadUrl()
        {
            Windows.Web.Http.HttpClient httpClient = new Windows.Web.Http.HttpClient();
            Uri requestUri = new Uri("https://2-dot-backup-server-002.appspot.com/get-upload-token");
            Windows.Web.Http.HttpResponseMessage httpResponse = new Windows.Web.Http.HttpResponseMessage();
            string httpResponseBody = "";
            try
            {
                httpResponse = await httpClient.GetAsync(requestUri);
                httpResponse.EnsureSuccessStatusCode();
                httpResponseBody = await httpResponse.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                httpResponseBody = "Error: " + ex.HResult.ToString("X") + " Message: " + ex.Message;
            }
            Debug.WriteLine(httpResponseBody);
            UploadUrl = httpResponseBody;
        }

        public async void HttpUploadFile(string url, string paramName, string contentType)
        {
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
            Debug.WriteLine(url);
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
            wr.ContentType = "multipart/form-data; boundary=" + boundary;
            wr.Method = "POST";

            Stream rs = await wr.GetRequestStreamAsync();
            rs.Write(boundarybytes, 0, boundarybytes.Length);

            string header = string.Format("Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n", paramName, "path_file", contentType);
            byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
            rs.Write(headerbytes, 0, headerbytes.Length);

            // write file.
            Stream fileStream = await file.OpenStreamForReadAsync();
            byte[] buffer = new byte[4096];
            int bytesRead = 0;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                rs.Write(buffer, 0, bytesRead);
            }

            byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            rs.Write(trailer, 0, trailer.Length);

            WebResponse wresp = null;
            try
            {
                wresp = await wr.GetResponseAsync();
                Stream stream2 = wresp.GetResponseStream();
                StreamReader reader2 = new StreamReader(stream2);
                //Debug.WriteLine(string.Format("File uploaded, server response is: @{0}@", reader2.ReadToEnd()));
                //string imgUrl = reader2.ReadToEnd();
                Uri u = new Uri(reader2.ReadToEnd(), UriKind.Absolute);
                Debug.WriteLine(u.AbsoluteUri);
                ImageUrl.Text = u.AbsoluteUri;
                MyAvatar.Source = new BitmapImage(u);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error uploading file", ex.StackTrace);
                Debug.WriteLine("Error uploading file", ex.InnerException);
                if (wresp != null)
                {
                    wresp = null;
                }
            }
            finally
            {
                wr = null;
            }
        }

        private void Select_Gender(object sender, RoutedEventArgs e)
        {
            RadioButton radioGender = sender as RadioButton;
            this.currentMember.gender = Int32.Parse(radioGender.Tag.ToString());
            Debug.WriteLine(this.currentMember.gender);
        }


        private void Sign_In(object sender, TappedRoutedEventArgs e)
        {
            var rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(MainPage));
        }

        private async void Handle_Signup(object sender, TappedRoutedEventArgs e)
        {

            this.currentMember.firstName = this.FirstName.Text;
            this.currentMember.lastName = this.LastName.Text;
            this.currentMember.email = this.Email.Text;
            this.currentMember.password = this.Password.Password.ToString();
            this.currentMember.avatar = this.ImageUrl.Text;
            this.currentMember.phone = this.Phone.Text;
            this.currentMember.address = this.Address.Text;
            this.currentMember.introduction = this.Introduction.Text;

            bool validate = true;
            if (this.currentMember.email == "")
            {
                validate = false;
                email.Text = "Email khong duoc de trong!";
                email.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
                Email.Focus(FocusState.Programmatic);
               
            }
            else
            {
                email.Text = "";
            }
            if (this.currentMember.password == "")
            {
                validate = false;
                password.Text = "Mat khau khong duoc de trong!";
                password.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
                Password.Focus(FocusState.Programmatic);
            }
            else
            {
                password.Text = "";
            }
            if (Confirm_Password.Password.ToString() != Password.Password.ToString())
            {
                validate = false;
                Confirm_Password_Message.Text = "Ban nhap lai mat khau khong dung";
                Confirm_Password_Message.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
                Confirm_Password.Focus(FocusState.Programmatic);
            }
            else
            {
                Confirm_Password_Message.Text = "";
            }

            if (this.currentMember.firstName == "")
            {
                validate = false;
                firstName.Text = "firstName khong duoc de trong!";
                firstName.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
                FirstName.Focus(FocusState.Programmatic);
            }
            else
            {
                firstName.Text = "";
            }

            if (this.currentMember.lastName == "")
            {
                validate = false;
                lastName.Text = "lastName khong duoc de trong!";
                lastName.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
                LastName.Focus(FocusState.Programmatic);
            }
            else
            {
                lastName.Text = "";
            }

            if (this.currentMember.avatar == "")
            {
                validate = false;
                avatar.Text = "avatar khong duoc de trong!";
                avatar.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
                ImageUrl.Focus(FocusState.Programmatic);
            }
            else
            {
                avatar.Text = "";
            }

            if (this.currentMember.address == "")
            {
                validate = false;
                address.Text = "Dia chi khong duoc de trong!";
                address.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
                Address.Focus(FocusState.Programmatic);
            }
            else
            {
                address.Text = "";
            }

            if (this.currentMember.phone == "")
            {
                validate = false;
                phone.Text = "So dien thoai khong duoc de trong!";
                phone.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
                Phone.Focus(FocusState.Programmatic);
            }
            else
            {
                phone.Text = "";
            }
            if (validate)
            {
                
                if (await ApiHandle.Sign_Up(this.currentMember))
                {
                    Debug.WriteLine("Action success.");
                    Login login = new Login();
                    await login.ShowAsync();
                }
                else
                {
                    validate = false;
                    email.Text = "Invalid email format!";
                    email.Foreground = new SolidColorBrush(Windows.UI.Colors.Red);
                    Email.Focus(FocusState.Programmatic);
                }
            }
            
            //if (httpResponseMessage.Result.StatusCode == HttpStatusCode.Created)
            //{
            //    Debug.WriteLine("Success");
            //}
            //else
            //{
            //    var errorJson = await httpResponseMessage.Result.Content.ReadAsStringAsync();
            //    ErrorResponse errResponse = JsonConvert.DeserializeObject<ErrorResponse>(errorJson);
            //    foreach (var errorField in errResponse.error.Keys)
            //    {
            //        TextBlock textBlock = this.FindName(errorField) as TextBlock;
            //        textBlock.Text = errResponse.error[errorField];
            //    }
            //}

            //Debug.WriteLine(jsonResult);
            //var rs = JObject.Parse(jsonResult);
            //if ((int)rs["status"] != 201)
            //{
            //    ErrorResponse errResponse = JsonConvert.DeserializeObject<ErrorResponse>(jsonResult);
            //    Debug.WriteLine(errResponse);
            //}

        }

        private void Change_Birthday(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            this.currentMember.birthday = sender.Date.Value.ToString("yyyy-MM-dd");
        }
    }
}
