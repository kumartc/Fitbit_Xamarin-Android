
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

//using Xamarin.Auth;
//using Xamarin.Auth.XamarinForms;
//using Xamarin.Auth.XamarinForms.XamarinAndroid;

namespace FitbitConnect
{
    [Activity(Label = "ActivityCustomUrlSchemeInterceptor", NoHistory = true, LaunchMode = Android.Content.PM.LaunchMode.SingleTop)]
    // Walthrough Step 4
    //      Intercepting/Catching/Detecting [redirect] url change 
    //      App Linking / Deep linking - custom url schemes
    //      
    // 
    [
        IntentFilter
        (
            actions: new[] { Intent.ActionView },
            Categories = new[]
                    {
                        Intent.CategoryDefault,
                        Intent.CategoryBrowsable
                    },
            DataSchemes = new[] 
                    { "xamarinauth", "xamarin-auth", "xamarin.auth", "http", "com.googleusercontent.apps.6376-bbmd5ahj7rgp"
                    },
            //DataHost = "xamarin-auth",
            //DataPath = "/oauth2redirect"
            DataPath = "oauth2redirect"
            //DataPort = "8888"
        )
    ]
    public class ActivityCustomUrlSchemeInterceptor : Activity
    //=================================================================
    {
        //string message;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            /*
            global::Android.Net.Uri uri_android = Intent.Data;

            #if DEBUG
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("ActivityCustomUrlSchemeInterceptor.OnCreate()");
            sb.Append("     uri_android = ").AppendLine(uri_android.ToString());
            System.Diagnostics.Debug.WriteLine(sb.ToString());
            #endif

            // Convert iOS NSUrl to C#/netxf/BCL System.Uri - common API
            Uri uri_netfx = new Uri(uri_android.ToString());

            // load redirect_url Page
            //WebAuthenticator wa = 
            //    (WebAuthenticator)AuthenticatorPageRenderer.Authenticator;

            //wa?.OnPageLoading(uri_netfx);

            */

            // Convert Android.Net.Url to Uri
            var uri = new Uri(Intent.Data.ToString()); //.Replace("//","/"));

            // Load redirectUrl page
            AuthenticationState.Authenticator.OnPageLoading(uri);
            AuthenticationState.RedirectedUri = uri;

            System.Diagnostics.Debug.WriteLine("URI returned:" + uri.ToString());
            /***/
            var intent = new Intent(this, typeof(MainActivity));
            intent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
            StartActivity(intent);
            /***/
            this.Finish();

            return;
        }
    }
}
