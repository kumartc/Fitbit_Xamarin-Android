using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using Xamarin.Auth;
using System.Threading;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Text.RegularExpressions;
using System.IO;
using System.Text;
using System.Net;

namespace FitbitConnect
{
    [Activity(Label = "FitbitConnect", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : Activity
    {
        Xamarin.Auth.OAuth2Authenticator authenticator = null;
        Android.Content.Intent auth_ui = null;
        //Account account;
        //AccountStore store;
        Constants.Fitbit.Token _token = null; // new Constants.Fitbit.Token();
        EditText textResult = null;
        Spinner spinner = null;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            spinner = FindViewById<Spinner>(Resource.Id.spinner);
            var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, new string[] {"<none>", "Profile", "Activity", "Heart Rate", "Sleep" });
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinner.Adapter = adapter;
            spinner.ItemSelected += (object sender, AdapterView.ItemSelectedEventArgs e) =>
            {
                if (e.Id == 1) //get profile
                    GetData(Constants.Fitbit.ProfileUrl);
                else if (e.Id == 2) //get activity
                    GetData(Constants.Fitbit.ActivityUrl.Replace("[date]",DateTime.Today.ToString("yyyy-MM-dd")));
                else if (e.Id == 3) // get heart rate
                    GetData(Constants.Fitbit.HeartRateUrl.Replace("[date]", "today").Replace("[range]","1d"));
                else if (e.Id == 4) //Sleep
                    GetData(Constants.Fitbit.SleepUrl.Replace("[date]", DateTime.Today.ToString("yyyy-MM-dd")));

            };
            spinner.Enabled = false;
            textResult = FindViewById<EditText>(Resource.Id.editText1);

            Button button1 = FindViewById<Button>(Resource.Id.button1);
            // wire up add task button handler
            if (button1 != null)
            {
                button1.Click += (sender, e) =>
                {
                    Toast.MakeText(this, "Trying to authenticate with fitbit!", ToastLength.Short);

                    //store = AccountStore.Create(this, "test123");
                    //account = store.FindAccountsForService(Constants.Fitbit.AppName).FirstOrDefault();

                    /*** OAuth2Authenticator object fails to authenticate with fitbit (it works for google though). However, it fires the OnAuthError method which we can use our way to get around the authentication.
                     * we simply use it currently to present the fitbit authentication screen for the user, and make use of the OnAuthError event to proceed with our authentication mechanism. 
                     */
                    authenticator = new OAuth2Authenticator(Constants.Fitbit.AndroidClientId, Constants.Fitbit.ClientSecret, Constants.Fitbit.Scope,
                        new Uri(Constants.Fitbit.AuthorizeUrl),
                        new Uri(Constants.Fitbit.AndroidRedirectUrl), new Uri(Constants.Fitbit.AccessTokenUrl), null, true);
                    authenticator.AllowCancel = true;
                    authenticator.Completed += OnAuthCompleted;
                    authenticator.Error += OnAuthError;

                    AuthenticationState.Authenticator = authenticator;

                    auth_ui = authenticator.GetUI(this);
                    StartActivity(auth_ui);
                };
            }
        }

        async void OnAuthCompleted(object sender, AuthenticatorCompletedEventArgs e)
        {
            var authenticator = sender as OAuth2Authenticator;
            if (authenticator != null)
            {
                authenticator.Completed -= OnAuthCompleted;
                authenticator.Error -= OnAuthError;
            }

            if (e.IsAuthenticated)
            {
                /*** OAuth2Request object fails authenticate with fitbit authentication methods. However, it fires the OnAuthError method and we can use that event
                  * to write our way to get around the authentication.
                 */
                GetToken(AuthenticationState.RedirectedUri.OriginalString);
                return;
                //* Use eventArgs.Account to do wonderful things
                var request = new OAuth2Request("GET", new Uri(Constants.Fitbit.ProfileUrl), null, e.Account);
                //var request = new OAuth2Request("GET", new Uri(Constants.Google.UserInfoUrl), null, e.Account);
                var response = await request.GetResponseAsync();
                if (response != null)
                {
                    // Deserialize the data and store it in the account store
                    // The users email address will be used to identify data in SimpleDB
                    string userJson = await response.GetResponseTextAsync();
                    //user = JsonConvert.DeserializeObject<User>(userJson);
                    System.Diagnostics.Debug.WriteLine("Output:" + userJson);
                }
            }
            else
            {
                // The user cancelled
            }

        }
        void OnAuthError(object sender, AuthenticatorErrorEventArgs e)
        {
            var authenticator = sender as OAuth2Authenticator;
            if (authenticator != null)
            {
                authenticator.Completed -= OnAuthCompleted;
                authenticator.Error -= OnAuthError;
            }

            System.Diagnostics.Debug.WriteLine("Authentication error: " + e.Message);

            //    Message: "Expected access_token in access token response, but did not receive one."
            if (e.Message.StartsWith("Expected access_token", StringComparison.CurrentCultureIgnoreCase))
                GetToken(AuthenticationState.RedirectedUri.OriginalString);
            /*** OAuth2Request object fails to get us the data need. Does not seem to work with fitbit authentication methods
             * so, we have to write our own methods
            var request = new OAuth2Request("GET", new Uri(Constants.Fitbit.AccessTokenUrl), null, e.Account);
            var response = await request.GetResponseAsync();
            if (response != null)
            {
                // Deserialize the data and store it in the account store
                // The users email address will be used to identify data in SimpleDB
                string userJson = await response.GetResponseTextAsync();
                //user = JsonConvert.DeserializeObject<User>(userJson);
                System.Diagnostics.Debug.WriteLine("Output:" + userJson);
            }
            ***/
        }
        
        //*** using request object Xamarin.Auth fails as well with fitbit. Throws an error
        async void GetToken_old(string urlString)
        {
            var request = new Request("POST", new Uri(Constants.Fitbit.AccessTokenUrl), null, null);
            /**
              curl -X POST -i -H 'Authorization: Basic MjJEODdMOmQ4dssdhsbfjhgsugJHGSJHGFhNTU2NzViOTVjNjVlMDMw' -H 'Content-Type: application/x-www-form-urlencoded' 
                -d "clientId=22D87L" -d "grant_type=authorization_code" -d "redirect_uri=http%3A%2F%2Fxamarin-auth%2Foauth2redirect" 
                -d "code=1448a0a6ee154ea2b6cae0c9d429f460cdf4d899" https://api.fitbit.com/oauth2/token
             * */
            request.AddMultipartData("clientId", new MemoryStream(Encoding.UTF8.GetBytes(Constants.Fitbit.AndroidClientId)), "application/x-www-form-urlencoded");
            request.AddMultipartData("redirect_uri", new MemoryStream(Encoding.UTF8.GetBytes(Uri.EscapeDataString(Constants.Fitbit.AndroidRedirectUrl))), "application/x-www-form-urlencoded");
            request.AddMultipartData("grant_type", new MemoryStream(Encoding.UTF8.GetBytes(Uri.EscapeDataString("authorization_code"))), "application/x-www-form-urlencoded");
            //Dictionary<string, string> paramData = new Dictionary<string, string>(); 
            if (!urlString.Equals(string.Empty))
            {
                //parse the url for code and state values
                Match m = Regex.Match(urlString, @"\?(?<kv>[^#]+)");
                string kvpair = m.Success ? m.Groups["kv"].Value : urlString;
                m = Regex.Match(kvpair, @"(?<key>[^=&]+)=?(?<val>[^&]*)?");
                while (m.Success)
                {
                    request.AddMultipartData(m.Groups["key"].Value, new MemoryStream(Encoding.UTF8.GetBytes(m.Groups["val"].Value)), "application/x-www-form-urlencoded");
                    m = m.NextMatch();
                }
            }
            var response = await request.GetResponseAsync();
            if (response != null)
            {
                // Deserialize the data and store it in the account store
                // The users email address will be used to identify data in SimpleDB
                string userJson = await response.GetResponseTextAsync();
                //user = JsonConvert.DeserializeObject<User>(userJson);
                System.Diagnostics.Debug.WriteLine("Output:" + userJson);
            }
            /***/
        }
        async void GetToken(string urlString)
        {
            string postData = string.Empty;

            if (!urlString.Equals(string.Empty))
            {
                //parse the url for code and state values
                Match m = Regex.Match(urlString, @"\?(?<kv>[^#]+)");
                string kvpair = m.Success ? m.Groups["kv"].Value : urlString;
                m = Regex.Match(kvpair, @"(?<key>[^=&]+)=?(?<val>[^&]*)?");
                while (m.Success)
                {
                    postData += string.Format("{0}={1}&", m.Groups["key"].Value, m.Groups["val"].Value);
                    m = m.NextMatch();
                }
            }
            postData += string.Format("clientId={0}&", Constants.Fitbit.AndroidClientId);
            postData += string.Format("redirect_uri={0}&", Constants.Fitbit.AndroidRedirectUrl);
            postData += "grant_type=authorization_code";
            var data = Encoding.ASCII.GetBytes(postData);

            HttpWebRequest request = new HttpWebRequest(new Uri(Constants.Fitbit.AccessTokenUrl))
            {
                Method = "POST",
                ContentType = "application/x-www-form-urlencoded",
                ContentLength = data.Length,
            };
            //request.Headers.Add("Authorization", "Basic MjJEODdMOmQ4MjBjZWJhODU3OTBmOGFhNTU2NzViOTVjNjVlMDMw");
            request.Headers.Add("Authorization", string.Format("Basic {0}", Constants.Fitbit.AuthorizationBasicString));
            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }
            
            var response = await request.GetResponseAsync();
            if (response != null)
            {
                //var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                var responseString = response.GetResponseText();
                //Dictionary<string,string> x = (Dictionary<string, string>) Newtonsoft.Json.JsonConvert.DeserializeObject(responseString);
                Dictionary<string, string> x= Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);

                //System.Diagnostics.Debug.WriteLine("Output:" + responseString);
                if (!x.ContainsKey("errors"))
                {
                    spinner.Enabled = true;
                    _token = new Constants.Fitbit.Token();
                    _token.AccessToken = x["access_token"];
                    _token.Expires = Convert.ToInt32(x["expires_in"]);
                    _token.Scope = x["scope"];
                    _token.RefreshToken = x["refresh_token"];
                    _token.UserId = x["user_id"];
                }
                else
                    Toast.MakeText(this, "Connection failed!", ToastLength.Short);
            }
        }

        async void GetData(string urlString)
        {
            if (_token == null)
                return;

            HttpWebRequest request = new HttpWebRequest(new Uri(urlString))
            {
                Method = "GET",
            };
            request.Headers.Add("Authorization", string.Format("Bearer {0}", _token.AccessToken));
            var response = await request.GetResponseAsync();
            if (response != null)
            {
                //var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                textResult.Text = response.GetResponseText();
            }
        }
    }

}

