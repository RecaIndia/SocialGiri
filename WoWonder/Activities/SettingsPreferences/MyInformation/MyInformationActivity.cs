﻿using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using Java.IO;
using WoWonder.Activities.SettingsPreferences.Adapters;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;
using WoWonderClient.Requests;
using Console = System.Console;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Uri = Android.Net.Uri;

namespace WoWonder.Activities.SettingsPreferences.MyInformation
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MyInformationActivity : AppCompatActivity
    {
        #region Variables Basic

        private ImageView ImageUser;

        private MyInformationAdapter MAdapter; 
        private RecyclerView MRecycler;
        private GridLayoutManager LayoutManager;
        private TextView TxtName;
        private AdsGoogle.AdMobRewardedVideo RewardedVideoAd;
        private Button BtnDownload;
        private string Link;
        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);
                Methods.App.FullScreenApp(this);

                // Create your application here
                SetContentView(Resource.Layout.MyInformationLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();

                RewardedVideoAd = AdsGoogle.Ad_RewardedVideo(this);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                AddOrRemoveEvent(true);
                RewardedVideoAd?.OnResume(this);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected override void OnPause()
        {
            try
            {
                base.OnPause();
                AddOrRemoveEvent(false);
                RewardedVideoAd?.OnPause(this);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override void OnTrimMemory(TrimMemory level)
        {
            try
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnTrimMemory(level);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected override void OnDestroy()
        {
            try
            {
                DestroyBasic();
                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        #region Menu

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                MRecycler = (RecyclerView)FindViewById(Resource.Id.recyler);

                ImageUser = FindViewById<ImageView>(Resource.Id.imageUser);
                TxtName = FindViewById<TextView>(Resource.Id.nameUser);

                BtnDownload = FindViewById<Button>(Resource.Id.downloadButton);
                BtnDownload.Visibility = ViewStates.Gone;
                 
                var myProfile = ListUtils.MyProfileList.FirstOrDefault();
                if (myProfile != null)
                {
                    GlideImageLoader.LoadImage(this, myProfile.Avatar, ImageUser, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                    TxtName.Text = WoWonderTools.GetNameFinal(myProfile); 
                } 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void InitToolbar()
        {
            try
            {
                var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolbar != null)
                {
                    toolbar.Title = GetText(Resource.String.Lbl_DownloadMyInformation);
                    toolbar.SetTitleTextColor(Color.White);
                    SetSupportActionBar(toolbar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void SetRecyclerViewAdapters()
        {
            try
            {
                MAdapter = new MyInformationAdapter(this);
                LayoutManager = new GridLayoutManager(this, 2);
                LayoutManager.SetSpanSizeLookup(new MySpanSizeLookup(4, 1, 1)); //5, 1, 2 
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true; 
                MRecycler.SetAdapter(MAdapter);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
         
         
        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                {
                    MAdapter.ItemClick += MAdapterOnItemClick;
                    BtnDownload.Click += BtnDownloadOnClick;
                }
                else
                {
                    MAdapter.ItemClick -= MAdapterOnItemClick;
                    BtnDownload.Click -= BtnDownloadOnClick;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void DestroyBasic()
        {
            try
            {
                RewardedVideoAd?.OnDestroy(this);

                MAdapter = null;
                MRecycler = null;
                ImageUser= null;
                TxtName = null; 
                BtnDownload = null;
                RewardedVideoAd = null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Events

        private void BtnDownloadOnClick(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(Link)) return;

                 var fileName = Link.Split('/').Last();
                 Link = WoWonderTools.GetFile("", Methods.Path.FolderDcimFile, fileName, Link);

                 var fileSplit = Link.Split('/').Last();
                 string getFile = Methods.MultiMedia.GetMediaFrom_Disk(Methods.Path.FolderDcimFile, fileSplit);
                 if (getFile != "File Dont Exists")
                 {
                     File file2 = new File(getFile);
                     var photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);

                     Intent openFile = new Intent(Intent.ActionView, photoUri);
                     openFile.SetFlags(ActivityFlags.NewTask);
                     openFile.SetFlags(ActivityFlags.GrantReadUriPermission);
                     StartActivity(openFile);
                 }
                 else
                 {
                     Intent intent = new Intent(Intent.ActionView, Uri.Parse(Link));
                     StartActivity(intent);
                 }
                  
                Toast.MakeText(this, GetText(Resource.String.Lbl_YourFileIsDownloaded), ToastLength.Long).Show();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception); 
            }
        }


        //Download Info
        private async void MAdapterOnItemClick(object sender, MyInformationAdapterClickEventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long).Show();
                    return;
                }
                 
                var position = e.Position;
                if (position > -1 )
                {
                    var item = MAdapter.GetItem(position);
                    if (item != null)
                    {
                        //Show a progress
                        AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                        var (apiStatus, respond) = await RequestsAsync.Global.DownloadInfoAsync(item.Type);
                        if (apiStatus == 200)
                        {
                            if (respond is DownloadInfoObject result)
                            {
                                Link = result.Link;
                                var fileName = Link.Split('/').Last();
                                 WoWonderTools.GetFile("", Methods.Path.FolderDcimFile, fileName, Link);

                                BtnDownload.Visibility = ViewStates.Visible;
                                 
                                Toast.MakeText(this, GetText(Resource.String.Lbl_YourFileIsReady), ToastLength.Long).Show(); 
                                
                                AndHUD.Shared.Dismiss(this); 
                            }
                        }
                        else
                        {
                            Methods.DisplayAndHUDErrorResult(this, respond);
                        } 
                    }
                } 
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                AndHUD.Shared.Dismiss(this);
            }
        }

        #endregion 
    }
}