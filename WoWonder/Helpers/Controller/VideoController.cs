using System;
using System.Collections.Generic;
using AFollestad.MaterialDialogs;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using Com.Google.Ads.Interactivemedia.V3.Api;
using Com.Google.Android.Exoplayer2;
using Com.Google.Android.Exoplayer2.Ext.Ima;
using Com.Google.Android.Exoplayer2.Source;
using Com.Google.Android.Exoplayer2.Source.Ads;
using Com.Google.Android.Exoplayer2.Source.Dash;
using Com.Google.Android.Exoplayer2.Source.Hls;
using Com.Google.Android.Exoplayer2.Source.Smoothstreaming;
using Com.Google.Android.Exoplayer2.Trackselection;
using Com.Google.Android.Exoplayer2.UI;
using Com.Google.Android.Exoplayer2.Upstream;
using Com.Google.Android.Exoplayer2.Upstream.Cache;
using Com.Google.Android.Exoplayer2.Util;
using Java.Lang;
using Java.Net;
using Plugin.Share;
using Plugin.Share.Abstractions;
using WoWonder.Activities.Tabbes;
using WoWonder.Activities.Videos;
using WoWonder.MediaPlayer;
using WoWonderClient.Classes.Movies;
using Exception = System.Exception;
using Uri = Android.Net.Uri;
 
namespace WoWonder.Helpers.Controller
{
    public class VideoController : Java.Lang.Object, View.IOnClickListener, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region Variables Basic

        private Activity ActivityContext { get; set; }
        private string ActivityName { get; set; }

        public Player Factory;
        private IDataSourceFactory DefaultDataMediaFactory;
        private static SimpleExoPlayer Player { get; set; }

        private ImaAdsLoader ImaAdsLoader;
        private PlayerEvents PlayerListener;
        private static PlayerView FullscreenPlayerView;

        public PlayerView SimpleExoPlayerView;
        private FrameLayout MainVideoFrameLayout , MFullScreenButton;
        private PlayerControlView ControlView;

        private ImageView MFullScreenIcon;

        private static IMediaSource VideoSource;
        private static readonly DefaultBandwidthMeter BandwidthMeter = new DefaultBandwidthMeter();

        private readonly int ResumeWindow = 0;
        private readonly long ResumePosition = 0;


        public GetMoviesObject.Movie VideoData;


        #endregion

        public VideoController(Activity activity, string activtyName)
        {
            try
            {
                var defaultCookieManager = new CookieManager();
                defaultCookieManager.SetCookiePolicy(CookiePolicy.AcceptOriginalServer);

                ActivityName = activtyName;
                ActivityContext = activity;
                 
                Initialize();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void Initialize()
        {
            try
            {
                PlayerListener = new PlayerEvents(ActivityContext, ControlView);

                if (ActivityName != "FullScreen")
                {
                    SimpleExoPlayerView = ActivityContext.FindViewById<PlayerView>(Resource.Id.player_view);
                    SimpleExoPlayerView.SetControllerVisibilityListener(PlayerListener);
                    SimpleExoPlayerView.RequestFocus();

                    //Player initialize
                    ControlView = SimpleExoPlayerView.FindViewById<PlayerControlView>(Resource.Id.exo_controller);
                    PlayerListener = new PlayerEvents(ActivityContext, ControlView);

                    MFullScreenIcon = ControlView.FindViewById<ImageView>(Resource.Id.exo_fullscreen_icon);
                    MFullScreenButton = ControlView.FindViewById<FrameLayout>(Resource.Id.exo_fullscreen_button);

                    MainVideoFrameLayout = ActivityContext.FindViewById<FrameLayout>(Resource.Id.root);
                    MainVideoFrameLayout.SetOnClickListener(this);
                      
                    if (!MFullScreenButton.HasOnClickListeners)
                        MFullScreenButton.SetOnClickListener(this); 
                }
                else
                {
                    FullscreenPlayerView = ActivityContext.FindViewById<PlayerView>(Resource.Id.player_view2);
                    ControlView = FullscreenPlayerView.FindViewById<PlayerControlView>(Resource.Id.exo_controller);
                    PlayerListener = new PlayerEvents(ActivityContext, ControlView);
                   
                    MFullScreenIcon = ControlView.FindViewById<ImageView>(Resource.Id.exo_fullscreen_icon);
                    MFullScreenButton = ControlView.FindViewById<FrameLayout>(Resource.Id.exo_fullscreen_button);

                    if (!MFullScreenButton.HasOnClickListeners)
                        MFullScreenButton.SetOnClickListener(this);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
         
        public void PlayVideo(string videoUrL, GetMoviesObject.Movie videoObject)
        {
            try
            {
                if (videoObject != null)
                {
                    VideoData = videoObject;

                    VideoViewerActivity.GetInstance()?.TabVideosAbout?.LoadVideo_Data(videoObject);

                    TabbedMainActivity.GetInstance()?.SetOnWakeLock();

                    ReleaseVideo();

                    MFullScreenIcon.SetImageDrawable(ActivityContext.GetDrawable(Resource.Drawable.ic_action_ic_fullscreen_expand));

                    Uri videoUrl = Uri.Parse(!string.IsNullOrEmpty(videoUrL) ? videoUrL : VideoData.Source);

                    AdaptiveTrackSelection.Factory trackSelectionFactory = new AdaptiveTrackSelection.Factory();
                    var trackSelector = new DefaultTrackSelector(trackSelectionFactory);
                    trackSelector.SetParameters(new DefaultTrackSelector.ParametersBuilder().Build());

                    Player = ExoPlayerFactory.NewSimpleInstance(ActivityContext, trackSelector);

                    DefaultDataMediaFactory = new DefaultDataSourceFactory(ActivityContext, Util.GetUserAgent(ActivityContext, AppSettings.ApplicationName), BandwidthMeter);

                    // Produces DataSource instances through which media data is loaded.
                    var defaultSource = GetMediaSourceFromUrl(videoUrl, "normal");

                    VideoSource = null;

                    //Set Interactive Media Ads 
                    if (PlayerSettings.ShowInteractiveMediaAds)
                        VideoSource = CreateMediaSourceWithAds(defaultSource, PlayerSettings.ImAdsUri);

                    if (SimpleExoPlayerView == null)
                        Initialize();

                    //Set Cache Media Load
                    if (PlayerSettings.EnableOfflineMode)
                    {
                        VideoSource = VideoSource == null? CreateCacheMediaSource(defaultSource, videoUrl): CreateCacheMediaSource(VideoSource, videoUrl);
                        if (VideoSource != null)
                        {
                            SimpleExoPlayerView.Player = Player;
                            Player.Prepare(VideoSource);
                            Player.AddListener(PlayerListener);
                            Player.PlayWhenReady = true;

                            bool haveResumePosition = ResumeWindow != C.IndexUnset;
                            if (haveResumePosition)
                                Player.SeekTo(ResumeWindow, ResumePosition);

                            return;
                        }
                    }

                    if (VideoSource == null)
                    {
                        if (!string.IsNullOrEmpty(videoUrL))
                        {
                            if (videoUrL.Contains("youtube") || videoUrL.Contains("Youtube") || videoUrL.Contains("youtu"))
                            {
                                //Task.Factory.StartNew(async () =>
                                //{
                                //    var newurl = await VideoInfoRetriever.GetEmbededVideo(VideoData.source);
                                //    videoSource = CreateDefaultMediaSource(Android.Net.Uri.Parse(newurl));
                                //});
                            }
                            else
                            {
                                VideoSource = GetMediaSourceFromUrl(Uri.Parse(videoUrL), "normal");

                                SimpleExoPlayerView.Player = Player;
                                Player.Prepare(VideoSource);
                                Player.AddListener(PlayerListener);
                                Player.PlayWhenReady = true;

                                bool haveResumePosition = ResumeWindow != C.IndexUnset;
                                if (haveResumePosition)
                                    Player.SeekTo(ResumeWindow, ResumePosition);
                            }
                        }
                    }
                    else
                    {
                        SimpleExoPlayerView.Player = Player;
                        Player.Prepare(VideoSource);
                        Player.AddListener(PlayerListener);
                        Player.PlayWhenReady = true;

                        bool haveResumePosition = ResumeWindow != C.IndexUnset;
                        if (haveResumePosition)
                            Player.SeekTo(ResumeWindow, ResumePosition);
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public void ReleaseVideo()
        {
            try
            {
                if (Player != null)
                {
                    SetStopVideo();

                    Player?.Release();
                    Player = null;

                    //GC Collector
                    GC.Collect();
                }

                TabbedMainActivity.GetInstance()?.SetOffWakeLock();

                //var tab = VideoViewerActivity.GetInstance()?.TabVideosAbout;
                //if (tab != null)
                //{
                //    tab.VideoDescriptionLayout.Visibility = ViewStates.Visible;
                //    tab.VideoTittle.Text = VideoData?.Name;
                //}

                SimpleExoPlayerView.Player = null;
                ReleaseAdsLoader();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public void SetStopVideo()
        {
            try
            {
                if (SimpleExoPlayerView.Player != null)
                {
                    if (SimpleExoPlayerView.Player.PlaybackState == Com.Google.Android.Exoplayer2.Player.StateReady)
                    {
                        SimpleExoPlayerView.Player.PlayWhenReady = false;
                    }

                    //GC Collector
                    GC.Collect();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
         
        #region Video player

        private IMediaSource CreateCacheMediaSource(IMediaSource videoSource, Uri videoUrL)
        {
            try
            {
                if (PlayerSettings.EnableOfflineMode)
                {
                    //Set the video for offline mode 
                    if (!string.IsNullOrEmpty(VideoData.Id))
                    {
                        string file = VideoDownloadAsyncControler.GetDownloadedDiskVideoUri(VideoData.Id);

                        SimpleCache cache = new SimpleCache(ActivityContext.CacheDir,new LeastRecentlyUsedCacheEvictor(1024 * 1024 * 10));
                        CacheDataSourceFactory cacheDataSource =new CacheDataSourceFactory(cache, DefaultDataMediaFactory);

                        if (file != null)
                        {
                            videoUrL = Uri.Parse(file);
                            Console.WriteLine(videoUrL);
                            videoSource = new ExtractorMediaSource.Factory(cacheDataSource).SetTag("normal").CreateMediaSource(videoUrL); 
                            return videoSource;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return null;
            }
        }

        private IMediaSource CreateMediaSourceWithAds(IMediaSource videoSource, Uri imAdsUri)
        {
            try
            {
                Player = ExoPlayerFactory.NewSimpleInstance(ActivityContext);
                SimpleExoPlayerView.Player = Player;

                if (ImaAdsLoader == null)
                {
                    var imaSdkSettings = ImaSdkFactory.Instance.CreateImaSdkSettings();
                    imaSdkSettings.AutoPlayAdBreaks = true;
                    imaSdkSettings.DebugMode = true;

                    ImaAdsLoader = new ImaAdsLoader.Builder(ActivityContext)
                        .SetImaSdkSettings(imaSdkSettings)
                        .SetMediaLoadTimeoutMs(30 * 1000)
                        .SetVastLoadTimeoutMs(30 * 1000)
                        .BuildForAdTag(imAdsUri); // here is url for vast xml file

                    ImaAdsLoader.SetPlayer(Player);

                    IMediaSource mediaSourceWithAds = new AdsMediaSource(
                        videoSource,
                        new AdMediaSourceFactory(this),
                        ImaAdsLoader,
                        SimpleExoPlayerView);

                    return mediaSourceWithAds;
                }

                return new AdsMediaSource(videoSource, new AdMediaSourceFactory(this), ImaAdsLoader, SimpleExoPlayerView);
            }
            catch (ClassNotFoundException e)
            {
                Console.WriteLine(e.Message);
                // IMA extension not loaded.
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        private class AdMediaSourceFactory : Java.Lang.Object, AdsMediaSource.IMediaSourceFactory
        {
            private readonly VideoController Activity;

            public AdMediaSourceFactory(VideoController activity)
            {
                Activity = activity;
            }

            public IMediaSource CreateMediaSource(Uri uri)
            {
                int type = Util.InferContentType(uri);
                var dataSourceFactory = new DefaultDataSourceFactory(Activity.ActivityContext, Util.GetUserAgent(Activity.ActivityContext, AppSettings.ApplicationName));
                switch (type)
                {
                    case C.TypeDash:
                        return new DashMediaSource.Factory(dataSourceFactory).SetTag("Ads").CreateMediaSource(uri);
                    case C.TypeSs:
                        return new SsMediaSource.Factory(dataSourceFactory).SetTag("Ads").CreateMediaSource(uri);
                    case C.TypeHls:
                        return new HlsMediaSource.Factory(dataSourceFactory).SetTag("Ads").CreateMediaSource(uri);
                    case C.TypeOther:
                        return new ExtractorMediaSource.Factory(dataSourceFactory).SetTag("Ads").CreateMediaSource(uri);
                    default:
                        return new ExtractorMediaSource.Factory(dataSourceFactory).SetTag("Ads").CreateMediaSource(uri);
                }
            }
             
            public int[] GetSupportedTypes()
            {
                return new[] { C.TypeDash, C.TypeSs, C.TypeHls, C.TypeOther };
            }
        }

        private IMediaSource GetMediaSourceFromUrl(Uri uri, string tag)
        {
            try
            { 
                var mBandwidthMeter = new DefaultBandwidthMeter();
                //DefaultDataSourceFactory dataSourceFactory = new DefaultDataSourceFactory(ActivityContext, Util.GetUserAgent(ActivityContext, AppSettings.ApplicationName), mBandwidthMeter);
                var buildHttpDataSourceFactory = new DefaultDataSourceFactory(ActivityContext, mBandwidthMeter, new DefaultHttpDataSourceFactory(Util.GetUserAgent(ActivityContext, AppSettings.ApplicationName), new DefaultBandwidthMeter()));
                var buildHttpDataSourceFactoryNull = new DefaultDataSourceFactory(ActivityContext, mBandwidthMeter, new DefaultHttpDataSourceFactory(Util.GetUserAgent(ActivityContext, AppSettings.ApplicationName), null));
                int type = Util.InferContentType(uri, null);
                IMediaSource src;
                switch (type)
                {
                    case C.TypeSs:
                        src = new SsMediaSource.Factory(new DefaultSsChunkSource.Factory(buildHttpDataSourceFactory), buildHttpDataSourceFactoryNull).SetTag(tag).CreateMediaSource(uri);
                        break;
                    case C.TypeDash:
                        src = new DashMediaSource.Factory(new DefaultDashChunkSource.Factory(buildHttpDataSourceFactory), buildHttpDataSourceFactoryNull).SetTag(tag).CreateMediaSource(uri);
                        break;
                    case C.TypeHls:
                        src = new HlsMediaSource.Factory(buildHttpDataSourceFactory).SetTag(tag).CreateMediaSource(uri);
                        break;
                    case C.TypeOther:
                        src = new ExtractorMediaSource.Factory(buildHttpDataSourceFactory).SetTag(tag).CreateMediaSource(uri);
                        break;
                    default:
                        //src = Exception("Unsupported type: " + type); 
                        src = new ExtractorMediaSource.Factory(buildHttpDataSourceFactory).SetTag(tag).CreateMediaSource(uri);
                        break;
                }
                return src;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
         
        public void OnClick(View v)
        {
            try
            {
                if (v.Id == MFullScreenIcon.Id || v.Id == MFullScreenButton.Id)
                {
                    InitFullscreenDialog();
                }

            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void ReleaseAdsLoader()
        {
            try
            {
                if (ImaAdsLoader != null)
                {
                    ImaAdsLoader.Release();
                    ImaAdsLoader = null;
                    SimpleExoPlayerView.OverlayFrameLayout.RemoveAllViews();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
         
        public void RestartPlayAfterShrinkScreen()
        {
            try
            {
                SimpleExoPlayerView.Player = null;
                SimpleExoPlayerView.Player = Player;
                SimpleExoPlayerView.Player.PlayWhenReady = true;
                MFullScreenIcon.SetImageDrawable(ActivityContext.GetDrawable(Resource.Drawable.ic_action_ic_fullscreen_expand));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public void PlayFullScreen()
        {
            try
            {
                if (FullscreenPlayerView != null)
                {
                    Player?.AddListener(PlayerListener);
                    FullscreenPlayerView.Player = Player;
                    if (FullscreenPlayerView.Player != null) FullscreenPlayerView.Player.PlayWhenReady = true;
                    MFullScreenIcon.SetImageDrawable(ActivityContext.GetDrawable(Resource.Drawable.ic_action_ic_fullscreen_skrink));
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        #region Event 
         
        //Full Screen
        public void InitFullscreenDialog(string action = "Open")
        {
            try
            {
                if (ActivityName != "FullScreen" && action == "Open")
                {
                    Intent intent = new Intent(ActivityContext, typeof(FullScreenVideoActivity));
                    //intent.PutExtra("Downloaded", DownloadIcon.Tag.ToString());
                    intent.PutExtra("Type", "Movies");
                    ActivityContext.StartActivityForResult(intent, 2000);
                }
                else
                {
                    Intent intent = new Intent();
                    ActivityContext.SetResult(Result.Ok, intent);
                    ActivityContext.Finish();
                }

            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Menu More >>  
        public void MoreButton_OnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(ActivityContext).Theme(AppSettings.SetTabDarkTheme ? Theme.Dark : Theme.Light);

                arrayAdapter.Add(ActivityContext.GetString(Resource.String.Lbl_CopeLink));
                arrayAdapter.Add(ActivityContext.GetString(Resource.String.Lbl_Share));

                dialogList.Title(ActivityContext.GetString(Resource.String.Lbl_More));
                dialogList.Items(arrayAdapter);
                dialogList.NegativeText(ActivityContext.GetText(Resource.String.Lbl_Close)).OnNegative(this);
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Event Menu >> Share
        public async void OnMenu_ShareIcon_Click(GetMoviesObject.Movie video)
        {
            try
            {
                //Share Plugin same as flame
                if (!CrossShare.IsSupported)
                {
                    return;
                }

                await CrossShare.Current.Share(new ShareMessage
                {
                    Title = video.Name,
                    Text = video.Description,
                    Url = video.Url
                });
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Event Menu >> Cope Link
        private void OnMenu_CopeLink_Click(GetMoviesObject.Movie video)
        {
            try
            {
                var clipboardManager = (ClipboardManager)ActivityContext.GetSystemService(Context.ClipboardService);

                ClipData clipData = ClipData.NewPlainText("text", video.Url);
                clipboardManager.PrimaryClip = clipData;

                Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_Copied), ToastLength.Short).Show();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Share
        public void ShareIcon_Click(object sender, EventArgs e)
        {
            try
            {
                OnMenu_ShareIcon_Click(VideoData);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
         
        #endregion

        #region MaterialDialog

        public void OnSelection(MaterialDialog p0, View p1, int itemId, ICharSequence itemString)
        {
            try
            {
                string text = itemString.ToString();
                if (text == ActivityContext.GetString(Resource.String.Lbl_CopeLink))
                {
                    OnMenu_CopeLink_Click(VideoData);
                }
                else if (text == ActivityContext.GetString(Resource.String.Lbl_Share))
                {
                    OnMenu_ShareIcon_Click(VideoData);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OnClick(MaterialDialog p0, DialogAction p1)
        {
            try
            {
                if (p1 == DialogAction.Positive)
                {
                }
                else if (p1 == DialogAction.Negative)
                {
                    p0.Dismiss();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion
    }
}