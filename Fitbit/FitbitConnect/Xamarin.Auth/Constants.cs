using System;
using System.Text;

namespace FitbitConnect
{
    public static class Constants
    {
        public class Google
        {
            public static string AppName = "OAuthNativeFlow";

            // OAuth
            // For Google login, configure at https://console.developers.google.com/
            public static string iOSClientId = "com.googleusercontent.apps.6376-bbmd5ahj7rgp"; //TODO: it should be different. New one should be created at google dev site
            public static string AndroidClientId = "6379-bbmd5ahrgp.apps.googleusercontent.com";

            // These values do not need changing
            public static string Scope = "https://www.googleapis.com/auth/userinfo.email";
            public static string AuthorizeUrl = "https://accounts.google.com/o/oauth2/auth";
            public static string AccessTokenUrl = "https://www.googleapis.com/oauth2/v4/token";
            public static string UserInfoUrl = "https://www.googleapis.com/oauth2/v2/userinfo";

            // Set these to reversed iOS/Android client ids, with :/oauth2redirect appended
            public static string iOSRedirectUrl = "com.googleusercontent.apps.6376-bbmd5ahj7rgp:/oauth2redirect"; //TODO: it should be different for iOS 
            public static string AndroidRedirectUrl = "com.googleusercontent.apps.6376-bbmd5ahj7rgp:/oauth2redirect";
        }
        public class Fitbit
        {
            public static string AppName = "Fitbit Demo";

            // OAuth
            // For Fitbit login, configure at https://dev.fitbit.com/
            public static string iOSClientId = "21D173";
            public static string AndroidClientId = "21D173";
            public static string ClientSecret = "d820csdhskhdhsgdsbdigskig675b95c65e030";
            // These values do not need changing
            public static string Scope = "activity heartrate location nutrition profile settings sleep social weight";
            public static string AuthorizeUrl = "https://www.fitbit.com/oauth2/authorize";
            public static string AccessTokenUrl = "https://api.fitbit.com/oauth2/token";
            public static string ProfileUrl = "https://api.fitbit.com/1/user/-/profile.json";
            public static string AuthorizationBasicString = Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", Constants.Fitbit.AndroidClientId, Constants.Fitbit.ClientSecret)));
            // for fitbit these are same values unlike google client ids,
            public static string iOSRedirectUrl = "http://localhost/oauth2redirect";
            public static string AndroidRedirectUrl = "xamarin-auth://oauth2redirect"; //"http://xamarin-auth/oauth2redirect"; // "http://localhost/oauth2redirect";
            public static string ActivityUrl = "https://api.fitbit.com/1/user/-/activities/date/[date].json"; //The date in the format yyyy-MM-dd
            public static string HeartRateUrl = "https://api.fitbit.com/1/user/-/activities/heart/date/[date]/[range].json"; //date: 'today' or in the format of 'yyyy-MM-dd'  range:1d, 7d, 30d, 1w, 1m.
            public static string SleepUrl = "https://api.fitbit.com/1.2/user/-/sleep/date/[date].json"; //The date in the format yyyy-MM-dd
            public class Token
            {
                public string AccessToken { get; set; }
                public int Expires { get; set; }
                public string RefreshToken { get; set; }
                public string Scope { get; set; }
                public string TokenType { get; set; }
                public string UserId { get; set; } 
            }
        }
    }
}