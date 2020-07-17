using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Ads;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;
using Java.Lang;
using WoWonder.Activities.Default;
using WoWonder.Activities.Tabbes;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.SQLite;
using Exception = System.Exception;

namespace WoWonder.Activities
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/SplashScreenTheme", NoHistory = true, MainLauncher = true, ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class SplashScreenActivity : AppCompatActivity
    {
        private SqLiteDatabase DbDatabase;
         
        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                DbDatabase = new SqLiteDatabase();
                DbDatabase.CheckTablesStatus();

                new Handler(Looper.MainLooper).Post(new Runnable(FirstRunExcite));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
          
        private void FirstRunExcite()
        {
            try
            { 
                DbDatabase = new SqLiteDatabase();
                DbDatabase.CheckTablesStatus();

                if (!string.IsNullOrEmpty(AppSettings.Lang))
                {
                    LangController.SetApplicationLang(this, AppSettings.Lang);
                }
                else
                {
                    UserDetails.LangName = Resources.Configuration.Locale.Language.ToLower();
                    LangController.SetApplicationLang(this, UserDetails.LangName);
                }

                var result = DbDatabase.Get_data_Login_Credentials();
                if (result != null)
                {
                    switch (result.Status)
                    {
                        case "Active":
                        case "Pending":
                            StartActivity(new Intent(Application.Context, typeof(TabbedMainActivity)));
                            break;
                        default:
                            StartActivity(new Intent(Application.Context, typeof(FirstActivity)));
                            break;
                    }
                }
                else
                {
                    StartActivity(new Intent(Application.Context, typeof(FirstActivity)));
                }

                DbDatabase.Dispose();

                if (AppSettings.ShowAdMobBanner || AppSettings.ShowAdMobInterstitial || AppSettings.ShowAdMobRewardVideo || AppSettings.ShowAdMobNative || AppSettings.ShowAdMobNativePost)
                    MobileAds.Initialize(this, GetString(Resource.String.admob_app_id));

                if (AppSettings.ShowFbBannerAds || AppSettings.ShowFbInterstitialAds || AppSettings.ShowFbRewardVideoAds)
                    InitializeFacebook.Initialize(this);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                Toast.MakeText(this, exception.Message, ToastLength.Short).Show();
            }
        } 
    }
}