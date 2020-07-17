using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Com.Google.Android.Exoplayer2;
using Com.Google.Android.Exoplayer2.Source;
using Com.Google.Android.Exoplayer2.Source.Dash;
using Com.Google.Android.Exoplayer2.Source.Hls;
using Com.Google.Android.Exoplayer2.Source.Smoothstreaming;
using Com.Google.Android.Exoplayer2.Trackselection;
using Com.Google.Android.Exoplayer2.UI;
using Com.Google.Android.Exoplayer2.Upstream;
using Com.Google.Android.Exoplayer2.Upstream.Cache;
using Java.Util.Concurrent.Atomic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Java.Util;
using WoWonder.Activities.NativePost.Pages;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Activities.Tabbes;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Utils;
using WoWonderClient;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Requests;
using Console = System.Console;
using Exception = System.Exception;
using LayoutDirection = Android.Views.LayoutDirection;
using Object = Java.Lang.Object;
using Uri = Android.Net.Uri;
using Util = Com.Google.Android.Exoplayer2.Util.Util;

namespace WoWonder.Activities.NativePost.Extra
{
    public class WRecyclerView : RecyclerView, IOnLoadMoreListener
    {
        private static WRecyclerView Instance;
        private enum VolumeState { On, Off }

        private FrameLayout MediaContainerLayout;
        private ImageView Thumbnail, PlayControl;
        private PlayerView VideoSurfaceView;
        private SimpleExoPlayer VideoPlayer;
        private View ViewHolderParent;
        private int VideoSurfaceDefaultHeight;
        private int ScreenDefaultHeight;
        private Context MainContext;
        private int PlayPosition = -1;
        private bool IsVideoViewAdded;
        private Uri VideoUrl;
        private RecyclerScrollListener MainScrollEvent;
        public NativePostAdapter NativeFeedAdapter;
        private SwipeRefreshLayout SwipeRefreshLayoutView;
        private FloatingActionButton PopupBubbleView;
        private VolumeState VolumeStateProvider;

        private CacheDataSourceFactory CacheDataSourceFactory;
        private static SimpleCache Cache;
        private static readonly DefaultBandwidthMeter BandwidthMeter = new DefaultBandwidthMeter();
        private DefaultDataSourceFactory DefaultDataSourceFac;
        private string Hash;
        private static bool ShowFindMoreAlert;
        private static PostModelType LastAdsType = PostModelType.AdMob1;

        protected WRecyclerView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public WRecyclerView(Context context) : base(context)
        {
            Init(context);
        }

        public WRecyclerView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Init(context);
        }

        public WRecyclerView(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
        {
            Init(context);
        }

        private void Init(Context context)
        {
            try
            {
                MainContext = context;

                Instance = this;

                if (AppSettings.FlowDirectionRightToLeft)
                    LayoutDirection = LayoutDirection.Rtl;

                HasFixedSize = true;
                SetItemViewCacheSize(15);
                //SetItemAnimator(new DefaultItemAnimator());
                GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.ColorPost, 15);
                GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.ImagePost, 30);
                GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.TextSectionPostPart, 30);
                GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.BottomPostPart, 30);
                GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.PrevBottomPostPart, 40);
                GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.HeaderPost, 30);
                GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.PromotePost, 10);
                GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.SharedHeaderPost, 10);
                GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.Story, 1);
                GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.SuggestedGroupsBox, 1);
                GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.SuggestedUsersBox, 1);
                GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.MultiImage2, 10);
                GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.MultiImage3, 10);
                GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.MultiImage4, 10);
                GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.PollPost, 20);
                GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.AlertBox, 2);
                GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.YoutubePost, 15);
                GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.VideoPost, 15);
                GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.AdMob1, 10);
                GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.AdMob2, 10);
                GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.AddPostBox, 1);
                GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.AdsPost, 6);
                GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.DeepSoundPost, 5);
                GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.Divider, 20);

                ClearAnimation();
                var f = GetItemAnimator();
                ((SimpleItemAnimator)f).SupportsChangeAnimations = false;
                f.ChangeDuration = 0;

                //DividerItemDecoration itemDecorator = new DividerItemDecoration(MainContext, DividerItemDecoration.Vertical);
                //itemDecorator.SetDrawable(ContextCompat.GetDrawable(MainContext, Resource.Drawable.Post_Devider_Shape));
                //AddItemDecoration(itemDecorator);

                var display = Context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>().DefaultDisplay;

                var point = new Point();
                display.GetSize(point);
                VideoSurfaceDefaultHeight = point.X;
                ScreenDefaultHeight = point.Y;

                VideoSurfaceView = new PlayerView(MainContext)
                {
                    ResizeMode = AspectRatioFrameLayout.ResizeModeFixedWidth
                };

                //===================== Exo Player ========================
                SetPlayer();

                //=============================================

                MainScrollEvent = new RecyclerScrollListener(this);
                AddOnScrollListener(MainScrollEvent);
                AddOnChildAttachStateChangeListener(new ChildAttachStateChangeListener(this));
                MainScrollEvent.LoadMoreEvent += MainScrollEvent_LoadMoreEvent;
                MainScrollEvent.IsLoading = false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static WRecyclerView GetInstance()
        {
            try
            {
                return Instance;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public void SetXAdapter(NativePostAdapter adapter, SwipeRefreshLayout swipeRefreshLayout)
        {
            try
            {
                NativeFeedAdapter = adapter;
                NativeFeedAdapter.SetOnLoadMoreListener(this);
                SwipeRefreshLayoutView = swipeRefreshLayout;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //PopupBubble 
        public void SetXPopupBubble(FloatingActionButton popupBubble)
        {
            try
            {
                if (popupBubble != null)
                {
                    PopupBubbleView = popupBubble;
                    PopupBubbleView.Visibility = ViewStates.Gone;
                    PopupBubbleView.Click += PopupBubbleViewOnClick;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void PopupBubbleViewOnClick(object sender, EventArgs e)
        {
            try
            {
                PopupBubbleView.Visibility = ViewStates.Gone;
                ScrollToPosition(0);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //play the video in the row
        private void PlayVideo(bool isEndOfList)
        {
            try
            {
                int targetPosition;
                if (!isEndOfList)
                {
                    int startPosition = ((LinearLayoutManager)Objects.RequireNonNull(GetLayoutManager())).FindFirstVisibleItemPosition();
                    int endPosition = ((LinearLayoutManager)GetLayoutManager()).FindLastVisibleItemPosition();
                    // if there is more than 2 list-items on the screen, set the difference to be 1
                    if (endPosition - startPosition > 1)
                    {
                        endPosition = startPosition + 1;
                    }

                    // something is wrong. return.
                    if (startPosition < 0 || endPosition < 0)
                    {
                        return;
                    }

                    // if there is more than 1 list-item on the screen
                    if (startPosition != endPosition)
                    {
                        int startPositionVideoHeight = GetVisibleVideoSurfaceHeight(startPosition);
                        int endPositionVideoHeight = GetVisibleVideoSurfaceHeight(endPosition);
                        targetPosition = startPositionVideoHeight > endPositionVideoHeight ? startPosition : endPosition;
                    }
                    else
                    {
                        targetPosition = startPosition;
                    }
                }
                else
                {
                    targetPosition = NativeFeedAdapter.ListDiffer.Count - 1;
                }

                // video is already playing so return
                if (targetPosition == PlayPosition)
                    return;

                // set the position of the list-item that is to be played
                PlayPosition = targetPosition;
                if (VideoSurfaceView == null)
                    return;

                int currentPosition = targetPosition - ((LinearLayoutManager)Objects.RequireNonNull(GetLayoutManager())).FindFirstVisibleItemPosition();

                View child = GetChildAt(currentPosition);
                if (child == null)
                    return;

                var holder = (AdapterHolders.PostVideoSectionViewHolder)child.Tag;
                if (holder == null)
                {
                    PlayPosition = -1;
                    return;
                }

                // remove any old surface views from previously playing videos
                VideoSurfaceView.Visibility = ViewStates.Invisible;
                RemoveVideoView(VideoSurfaceView);

                if (VideoPlayer == null)
                    SetPlayer();

                MediaContainerLayout = holder.MediaContainer;
                Thumbnail = holder.VideoImage;
                ViewHolderParent = holder.ItemView;
                PlayControl = holder.PlayControl;

                if (!IsVideoViewAdded)
                    AddVideoView();

                VideoSurfaceView.Player = VideoPlayer;

                var controlView = VideoSurfaceView.FindViewById<PlayerControlView>(Resource.Id.exo_controller);

                var item = NativeFeedAdapter.ListDiffer[targetPosition].PostData;

                VideoUrl = Uri.Parse(item.PostFileFull);

                holder.VideoUrl = VideoUrl.ToString();

                if (item.Blur != "0")
                    return;

                TabbedMainActivity.GetInstance()?.SetOnWakeLock();

                //>> Old Code 
                //===================== Exo Player ======================== 
                var lis = new ExoPlayerRecyclerEvent(controlView, this, holder);
                lis.MFullScreenButton.SetOnClickListener(new NewClicker(lis.MFullScreenButton, holder.VideoUrl, this));

                var videoSource = GetMediaSourceFromUrl(VideoUrl, "normal");

                var dataSpec = new DataSpec(VideoUrl); //0, 1000 * 1024, null

                if (Cache == null)
                    CacheVideosFiles(VideoUrl);

                CacheDataSourceFactory ??= new CacheDataSourceFactory(Cache, DefaultDataSourceFac);

                var counters = new CacheUtil.CachingCounters();

                CacheUtil.GetCached(dataSpec, Cache, counters);
                if (counters.ContentLength == counters.TotalCachedBytes())
                {
                    videoSource = new ExtractorMediaSource.Factory(CacheDataSourceFactory).CreateMediaSource(VideoUrl);
                }
                else if (counters.TotalCachedBytes() == 0)
                {
                    videoSource = new ExtractorMediaSource.Factory(CacheDataSourceFactory).CreateMediaSource(VideoUrl);
                    // not cached at all
                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            var cacheDataSource = new CacheDataSource(Cache, CacheDataSourceFactory.CreateDataSource());
                            CacheUtil.Cache(dataSpec, Cache, cacheDataSource, counters, new AtomicBoolean());
                            double downloadPercentage = (counters.TotalCachedBytes() * 100d) / counters.ContentLength;
                            Console.WriteLine(downloadPercentage);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    });
                }
                else
                {
                    // partially cached
                    videoSource ??= new ExtractorMediaSource.Factory(CacheDataSourceFactory).CreateMediaSource(VideoUrl);
                }

                VideoPlayer.Prepare(videoSource);
                VideoPlayer.AddListener(lis);
                VideoPlayer.PlayWhenReady = true;

                if (Methods.CheckConnectivity())
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.Get_Post_Data(item.PostId, "post_data", "1") });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void PlayVideo(bool isEndOfList, AdapterHolders.PostVideoSectionViewHolder holder, PostDataObject item)
        {
            try
            {
                Console.WriteLine(isEndOfList);
                if (VideoPlayer == null)
                    SetPlayer();

                if (VideoSurfaceView == null)
                    return;

                VideoSurfaceView.Visibility = ViewStates.Invisible;
                RemoveVideoView(VideoSurfaceView);

                MediaContainerLayout = holder.MediaContainer;
                Thumbnail = holder.VideoImage;
                ViewHolderParent = holder.ItemView;
                PlayControl = holder.PlayControl;

                if (!IsVideoViewAdded)
                    AddVideoView();

                VideoSurfaceView.Player = VideoPlayer;

                var controlView = VideoSurfaceView.FindViewById<PlayerControlView>(Resource.Id.exo_controller);
                VideoUrl = Uri.Parse(item.PostFileFull);

                holder.VideoUrl = VideoUrl.ToString();

                if (item.Blur != "0")
                    return;

                TabbedMainActivity.GetInstance()?.SetOnWakeLock();

                //>> Old Code 
                //===================== Exo Player ======================== 
                var lis = new ExoPlayerRecyclerEvent(controlView, this, holder);
                lis.MFullScreenButton.SetOnClickListener(new NewClicker(lis.MFullScreenButton, holder.VideoUrl, this));

                var videoSource = GetMediaSourceFromUrl(VideoUrl, "normal");

                var dataSpec = new DataSpec(VideoUrl); //0, 1000 * 1024, null

                if (Cache == null)
                    CacheVideosFiles(VideoUrl);

                CacheDataSourceFactory ??= new CacheDataSourceFactory(Cache, DefaultDataSourceFac);

                var counters = new CacheUtil.CachingCounters();

                CacheUtil.GetCached(dataSpec, Cache, counters);
                if (counters.ContentLength == counters.TotalCachedBytes())
                {
                    videoSource = new ExtractorMediaSource.Factory(CacheDataSourceFactory).CreateMediaSource(VideoUrl);
                }
                else if (counters.TotalCachedBytes() == 0)
                {
                    videoSource = new ExtractorMediaSource.Factory(CacheDataSourceFactory).CreateMediaSource(VideoUrl);
                    // not cached at all
                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            var cacheDataSource = new CacheDataSource(Cache, CacheDataSourceFactory.CreateDataSource());
                            CacheUtil.Cache(dataSpec, Cache, cacheDataSource, counters, new AtomicBoolean());
                            double downloadPercentage = (counters.TotalCachedBytes() * 100d) / counters.ContentLength;
                            Console.WriteLine(downloadPercentage);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    });
                }
                else
                {
                    // partially cached
                    videoSource ??= new ExtractorMediaSource.Factory(CacheDataSourceFactory).CreateMediaSource(VideoUrl);
                }

                VideoPlayer.Prepare(videoSource);
                VideoPlayer.AddListener(lis);
                VideoPlayer.PlayWhenReady = true;

                if (Methods.CheckConnectivity())
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.Get_Post_Data(item.PostId, "post_data", "1") });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private class NewClicker : Object, IOnClickListener
        {
            private string VideoUrl { get; set; }
            private FrameLayout FullScreenButton { get; set; }
            private WRecyclerView WRecyclerViewController { get; set; }
            public NewClicker(FrameLayout fullScreenButton, string videoUrl, WRecyclerView wRecyclerViewController)
            {
                WRecyclerViewController = wRecyclerViewController;
                FullScreenButton = fullScreenButton;
                VideoUrl = videoUrl;
            }
            public void OnClick(View v)
            {
                if (v.Id == FullScreenButton.Id)
                {
                    try
                    {

                        WRecyclerViewController.VideoPlayer.PlayWhenReady = false;

                        Intent intent = new Intent(WRecyclerViewController.MainContext, typeof(VideoFullScreenActivity));
                        intent.PutExtra("videoUrl", VideoUrl);
                        //  intent.PutExtra("videoDuration", videoPlayer.Duration.ToString());
                        MainApplication.GetInstance().Activity.StartActivity(intent);
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                    }
                }
            }
        }

        private void MainScrollEvent_LoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                if (NativeFeedAdapter.NativePostType == NativeFeedType.Memories || NativeFeedAdapter.NativePostType == NativeFeedType.Share)
                    return;

                MainScrollEvent.IsLoading = true;
                var diff = NativeFeedAdapter.ListDiffer;
                var list = new List<AdapterModelsClass>(diff);
                if (list.Count <= 20)
                    return;

                var item = list.LastOrDefault();

                var lastItem = list.IndexOf(item);

                item = list[lastItem];

                string offset;

                if (item.TypeView == PostModelType.Divider || item.TypeView == PostModelType.ViewProgress || item.TypeView == PostModelType.AdMob1 || item.TypeView == PostModelType.AdMob2 || item.TypeView == PostModelType.FbAdNative || item.TypeView == PostModelType.AdsPost || item.TypeView == PostModelType.SuggestedGroupsBox || item.TypeView == PostModelType.SuggestedUsersBox || item.TypeView == PostModelType.CommentSection || item.TypeView == PostModelType.AddCommentSection)
                {
                    item = list.LastOrDefault(a => a.TypeView != PostModelType.Divider && a.TypeView != PostModelType.ViewProgress && a.TypeView != PostModelType.AdMob1 && a.TypeView != PostModelType.AdMob2 && a.TypeView != PostModelType.FbAdNative && a.TypeView != PostModelType.AdsPost && a.TypeView != PostModelType.SuggestedGroupsBox && a.TypeView != PostModelType.SuggestedUsersBox && a.TypeView != PostModelType.CommentSection && a.TypeView != PostModelType.AddCommentSection);
                    offset = item?.PostData?.Id ?? "0";
                    Console.WriteLine(offset);
                }
                else
                {
                    offset = item.PostData?.Id ?? "0";
                }

                Console.WriteLine(offset);

                if (!Methods.CheckConnectivity())
                    Toast.MakeText(MainContext, MainContext.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                else
                {
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => NativeFeedAdapter.NativePostType != NativeFeedType.HashTag ? FetchNewsFeedApiPosts(offset) : FetchNewsFeedApiPosts(offset, "Add", Hash) });
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public void InsertByRowIndex(AdapterModelsClass item, string index = "")
        {
            try
            {
                var diff = NativeFeedAdapter.ListDiffer;
                var diffList = new List<AdapterModelsClass>(diff);

                int countIndex = 1;
                var model1 = diffList.FirstOrDefault(a => a.TypeView == PostModelType.Story);
                var model2 = diffList.FirstOrDefault(a => a.TypeView == PostModelType.AddPostBox);
                var model3 = diffList.FirstOrDefault(a => a.TypeView == PostModelType.AlertBox);
                var model4 = diffList.FirstOrDefault(a => a.TypeView == PostModelType.SearchForPosts);

                if (model4 != null)
                    countIndex += diffList.IndexOf(model4);
                else if (model3 != null)
                    countIndex += diffList.IndexOf(model3);
                else if (model2 != null)
                    countIndex += diffList.IndexOf(model2);
                else if (model1 != null)
                    countIndex += diffList.IndexOf(model1);
                else
                    countIndex = 0;

                if (!string.IsNullOrEmpty(index))
                    countIndex = int.Parse(index);

                diffList.Insert(countIndex, item);

                var emptyStateChecker = diffList.FirstOrDefault(a => a.TypeView == PostModelType.EmptyState);
                if (emptyStateChecker != null && diffList.Count > 1)
                {
                    diffList.Remove(emptyStateChecker);

                }

                NativeFeedAdapter.NotifyItemRangeInserted(diff.Count - 1, diffList.Count);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void RemoveByRowIndex(AdapterModelsClass item)
        {
            try
            {
                var diff = NativeFeedAdapter.ListDiffer;
                var index = diff.IndexOf(item);
                if (index <= 0)
                    return;

                diff.RemoveAt(index);
                NativeFeedAdapter.NotifyItemRemoved(index);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected override void OnDetachedFromWindow()
        {
            try
            {
                base.OnDetachedFromWindow();
                if (GetAdapter() != null)
                {
                    SetAdapter(null);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public async Task FetchNewsFeedApiPosts(string offset = "0", string typeRun = "Add", string hash = "")
        {
            if (!Methods.CheckConnectivity())
                return;

            Hash = hash;
            int apiStatus;
            dynamic respond;

            switch (NativeFeedAdapter.NativePostType)
            {
                case NativeFeedType.Global:
                    (apiStatus, respond) = await RequestsAsync.Posts.GetGlobalPost(AppSettings.PostApiLimitOnScroll, offset, "get_news_feed", NativeFeedAdapter.IdParameter);
                    break;
                case NativeFeedType.User:
                    (apiStatus, respond) = await RequestsAsync.Posts.GetGlobalPost(AppSettings.PostApiLimitOnScroll, offset, "get_user_posts", NativeFeedAdapter.IdParameter);
                    break;
                case NativeFeedType.Group:
                    (apiStatus, respond) = await RequestsAsync.Posts.GetGlobalPost(AppSettings.PostApiLimitOnScroll, offset, "get_group_posts", NativeFeedAdapter.IdParameter);
                    break;
                case NativeFeedType.Page:
                    (apiStatus, respond) = await RequestsAsync.Posts.GetGlobalPost(AppSettings.PostApiLimitOnScroll, offset, "get_page_posts", NativeFeedAdapter.IdParameter);
                    break;
                case NativeFeedType.Event:
                    (apiStatus, respond) = await RequestsAsync.Posts.GetGlobalPost(AppSettings.PostApiLimitOnScroll, offset, "get_event_posts", NativeFeedAdapter.IdParameter);
                    break;
                case NativeFeedType.Saved:
                    (apiStatus, respond) = await RequestsAsync.Posts.GetGlobalPost(AppSettings.PostApiLimitOnScroll, offset, "saved");
                    break;
                case NativeFeedType.HashTag:
                    (apiStatus, respond) = await RequestsAsync.Posts.GetGlobalPost(AppSettings.PostApiLimitOnScroll, offset, "hashtag", "", hash);
                    break;
                case NativeFeedType.Popular:
                    (apiStatus, respond) = await RequestsAsync.Posts.GetPopularPost(AppSettings.PostApiLimitOnScroll, offset);
                    break;
                default:
                    return;
            }

            if (apiStatus != 200 || !(respond is PostObject result) || result.Data == null)
            {
                MainScrollEvent.IsLoading = false;
                var activity = TabbedMainActivity.GetInstance() ?? (Activity)Context;
                Methods.DisplayReportResult(activity, respond);
            }
            else
                LoadDataApi(apiStatus, respond, offset, typeRun);
        }

        public async Task FetchSearchForPosts(string offset, string id, string searchQuery, string type)
        {
            if (!Methods.CheckConnectivity())
                return;

            var (apiStatus, respond) = await RequestsAsync.Posts.SearchForPosts(AppSettings.PostApiLimitOnScroll, offset, id, searchQuery, type);
            if (apiStatus != 200 || !(respond is PostObject result) || result.Data == null)
            {
                MainScrollEvent.IsLoading = false;
                Methods.DisplayReportResult((Activity)Context, respond);
            }
            else LoadDataApi(apiStatus, respond, offset);
        }

        private void LoadDataApi(int apiStatus, dynamic respond, string offset, string typeRun = "Add")
        {
            try
            {
                if (apiStatus != 200 || !(respond is PostObject result) || result.Data == null)
                {
                    MainScrollEvent.IsLoading = false;
                    Methods.DisplayReportResult((Activity)Context, respond);
                }
                else
                {
                    MainScrollEvent.IsLoading = true;

                    if (SwipeRefreshLayoutView != null && SwipeRefreshLayoutView.Refreshing)
                        SwipeRefreshLayoutView.Refreshing = false;

                    var countList = NativeFeedAdapter.ItemCount;
                    if (result.Data.Count > 0)
                    {
                        result.Data.RemoveAll(a => a.Publisher == null && a.UserData == null);

                        if (offset == "0" && countList > 10 && typeRun == "Insert")
                        {
                            result.Data.Reverse();
                            bool add = false;

                            foreach (var post in from post in result.Data let check = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a?.PostData?.Id == post.Id) where check == null select post)
                            {
                                if (post.Publisher == null && post.UserData == null)
                                    continue;

                                add = true;

                                var combine = new FeedCombiner(RegexFilterText(post), NativeFeedAdapter.ListDiffer, MainContext);

                                if (post.PostType == "ad")
                                {
                                    combine.AddAdsPost("Top");
                                }
                                else
                                {
                                    combine.CombineDefaultPostSections("Top");
                                }
                            }

                            if (add)
                            {
                                ((Activity)MainContext).RunOnUiThread(() =>
                                {
                                    try
                                    {
                                        NativeFeedAdapter.NotifyItemRangeInserted(countList, NativeFeedAdapter.ListDiffer.Count - countList);
                                        MainScrollEvent.IsLoading = false;

                                        if (PopupBubbleView != null && PopupBubbleView.Visibility != ViewStates.Visible && AppSettings.ShowNewPostOnNewsFeed)
                                        {
                                            PopupBubbleView.Visibility = ViewStates.Visible;
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine(e);
                                    }
                                }); 
                            }
                            return; 
                        }
                        else
                        {
                            bool add = false;
                            foreach (var post in from post in result.Data let check = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a?.PostData?.Id == post.Id) where check == null select post)
                            {
                                if (post.Publisher == null && post.UserData == null)
                                    continue;

                                add = true;
                                var combiner = new FeedCombiner(null, NativeFeedAdapter.ListDiffer, MainContext);

                                if (NativeFeedAdapter.NativePostType == NativeFeedType.Global)
                                {
                                    if (result.Data.Count < 6)
                                    {
                                        if (!ShowFindMoreAlert)
                                        {
                                            ShowFindMoreAlert = true;

                                            combiner.AddFindMoreAlertPostView("Pages");
                                            combiner.AddFindMoreAlertPostView("Groups");
                                        }
                                    }

                                    var check1 = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.SuggestedGroupsBox);
                                    if (check1 == null && AppSettings.ShowSuggestedGroup && NativeFeedAdapter.ListDiffer.Count > 0 && NativeFeedAdapter.ListDiffer.Count % AppSettings.ShowSuggestedGroupCount == 0 && ListUtils.SuggestedGroupList.Count > 0)
                                    {
                                        combiner.AddSuggestedBoxPostView(PostModelType.SuggestedGroupsBox);
                                    }

                                    var check2 = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.SuggestedUsersBox);
                                    if (check2 == null && AppSettings.ShowSuggestedUser && NativeFeedAdapter.ListDiffer.Count > 0 && NativeFeedAdapter.ListDiffer.Count % AppSettings.ShowSuggestedUserCount == 0 && ListUtils.SuggestedUserList.Count > 0)
                                    {
                                        combiner.AddSuggestedBoxPostView(PostModelType.SuggestedUsersBox);
                                    }

                                    if (NativeFeedAdapter.ListDiffer.Count % AppSettings.ShowAdMobNativeCount == 0 && NativeFeedAdapter.ListDiffer.Count > 0 && AppSettings.ShowAdMobNativePost)
                                    {
                                        if (LastAdsType == PostModelType.AdMob2)
                                        {
                                            LastAdsType = PostModelType.AdMob1;
                                            combiner.AddAdsPostView(PostModelType.AdMob1);
                                        }
                                        else
                                        {
                                            LastAdsType = PostModelType.AdMob2;
                                            combiner.AddAdsPostView(PostModelType.AdMob2);
                                        } 
                                    }
                                    else if (NativeFeedAdapter.ListDiffer.Count % AppSettings.ShowFbNativeAdsCount == 0 && NativeFeedAdapter.ListDiffer.Count > 0 && AppSettings.ShowFbNativeAds)
                                    {
                                        combiner.AddAdsPostView(PostModelType.FbAdNative);
                                    }
                                }

                                var combine = new FeedCombiner(RegexFilterText(post), NativeFeedAdapter.ListDiffer, MainContext);
                                if (post.PostType == "ad")
                                {
                                    combine.AddAdsPost();
                                }
                                else
                                {
                                    bool isPromoted = post.IsPostBoosted == "1" || post.SharedInfo.SharedInfoClass != null && post.SharedInfo.SharedInfoClass?.IsPostBoosted == "1";
                                    if (isPromoted) //Promoted
                                    {
                                        combine.CombineDefaultPostSections("Top");
                                    }
                                    else
                                    {
                                        combine.CombineDefaultPostSections();
                                    }
                                }
                            }

                            if (add)
                            {
                                ((Activity)MainContext).RunOnUiThread(() =>
                                {
                                    try
                                    {
                                        NativeFeedAdapter.NotifyItemRangeInserted(countList, NativeFeedAdapter.ListDiffer.Count - countList);
                                        MainScrollEvent.IsLoading = false;
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine(e);
                                    }
                                });
                            }
                            //else
                            //{
                            //    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_NoMorePost), ToastLength.Short).Show(); 
                            //}
                        }
                    }

                    NativeFeedAdapter.SetLoaded();
                    var viewProgress = NativeFeedAdapter.ListDiffer.FirstOrDefault(anjo => anjo.TypeView == PostModelType.ViewProgress);
                    if (viewProgress != null)
                        RemoveByRowIndex(viewProgress);

                    var emptyStateCheck = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a.PostData != null && a.TypeView != PostModelType.AddPostBox && a.TypeView != PostModelType.SearchForPosts);
                    if (emptyStateCheck != null)
                    {
                        var emptyStateChecker = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.EmptyState);
                        if (emptyStateChecker != null && NativeFeedAdapter.ListDiffer.Count > 1)
                            RemoveByRowIndex(emptyStateChecker);
                    }
                    else
                    {
                        var emptyStateChecker = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.EmptyState);
                        if (emptyStateChecker == null)
                        {
                            var data = new AdapterModelsClass()
                            {
                                TypeView = PostModelType.EmptyState,
                                Id = 744747447,
                            };
                            NativeFeedAdapter.ListDiffer.Add(data);
                            NativeFeedAdapter.NotifyItemInserted(NativeFeedAdapter.ListDiffer.IndexOf(data));
                        }
                    }
                }
                MainScrollEvent.IsLoading = false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                MainScrollEvent.IsLoading = false;
            }
        }

        public void LoadMemoriesDataApi(int apiStatus, dynamic respond, List<AdapterModelsClass> diffList)
        {
            try
            {
                if (apiStatus != 200 || !(respond is FetchMemoriesObject result) || result.Data == null)
                {
                    MainScrollEvent.IsLoading = false;
                    Methods.DisplayReportResult((Activity)Context, respond);
                }
                else
                {
                    if (SwipeRefreshLayoutView != null && SwipeRefreshLayoutView.Refreshing)
                        SwipeRefreshLayoutView.Refreshing = false;

                    var countList = NativeFeedAdapter.ItemCount;
                    if (result.Data.Posts.Count > 0)
                    {
                        result.Data.Posts.RemoveAll(a => a.Publisher == null && a.UserData == null);
                        result.Data.Posts.Reverse();

                        foreach (var post in from post in result.Data.Posts let check = diffList.FirstOrDefault(a => a.Id == int.Parse(post.Id)) where check == null select post)
                        {
                            if (post.Publisher == null && post.UserData == null)
                                continue;

                            var combine = new FeedCombiner(RegexFilterText(post), NativeFeedAdapter.ListDiffer, MainContext);
                            combine.CombineDefaultPostSections();
                        }

                        ((Activity)MainContext).RunOnUiThread(() =>
                        {
                            NativeFeedAdapter.NotifyItemRangeInserted(countList, NativeFeedAdapter.ListDiffer.Count - countList);
                            MainScrollEvent.IsLoading = false;
                        });
                    }
                    else
                    {
                        MainScrollEvent.IsLoading = true;
                    }
                }
                MainScrollEvent.IsLoading = false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                MainScrollEvent.IsLoading = false;
            }
        }

        private static PostDataObject RegexFilterText(PostDataObject item)
        {
            try
            {
                Dictionary<string, string> dataUser = new Dictionary<string, string>();

                if (string.IsNullOrEmpty(item.PostText))
                    return item;

                if (item.PostText.Contains("data-id="))
                {
                    try
                    {
                        string pattern = @"(data-id=[""'](.*?)[""']|href=[""'](.*?)[""'])";
                        var aa = Regex.Matches(item.PostText, pattern);
                        if (aa.Count > 0)
                        {
                            for (int i = 0; i < aa.Count; i++)
                            {
                                var userid = aa[i].Value.Replace("data-id=", "").Replace('"', ' ').Replace(" ", "");
                                var username = aa[i + 1].Value.Replace("href=", "").Replace('"', ' ').Replace(" ", "").Replace(Client.WebsiteUrl, "");

                                var data = dataUser.FirstOrDefault(a => a.Key?.ToString() == userid && a.Value?.ToString() == username);
                                if (data.Key != null) continue;
                                i++;
                                dataUser.Add(userid, username);
                            }

                            item.RegexFilterList = dataUser;
                            return item;
                        }
                        else
                        {
                            return item;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
                return item;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return item;
            }
        }

        public class RecyclerScrollListener : OnScrollListener
        {
            public delegate void LoadMoreEventHandler(object sender, EventArgs e);

            public event LoadMoreEventHandler LoadMoreEvent;

            public bool IsLoading { get; set; }
            public bool IsScrolling { get; set; }

            private PreCachingLayoutManager LayoutManager { get; set; }
            private readonly WRecyclerView XRecyclerView;

            public RecyclerScrollListener(WRecyclerView recyclerView)
            {
                XRecyclerView = recyclerView;
                IsLoading = false;
            }

            public override void OnScrollStateChanged(RecyclerView recyclerView, int newState)
            {
                try
                {
                    base.OnScrollStateChanged(recyclerView, newState);

                    if (newState == (int)Android.Widget.ScrollState.TouchScroll)
                    {
                        IsScrolling = true;
                        //Glide.With(XRecyclerView.Context).PauseRequests();
                    }
                    if (newState == (int)Android.Widget.ScrollState.Fling)
                    {
                        IsScrolling = true;
                        //if (Glide.With(XRecyclerView.Context).IsPaused)
                        //    Glide.With(XRecyclerView.Context).ResumeRequests();
                    }
                    if (newState == (int)Android.Widget.ScrollState.Idle)
                    {

                        // There's a special case when the end of the list has been reached.
                        // Need to handle that with this bit of logic
                        if (AppSettings.AutoPlayVideo)
                        {
                            XRecyclerView.PlayVideo(!recyclerView.CanScrollVertically(1));
                        }

                        //if (Glide.With(XRecyclerView.Context).IsPaused)
                        //    Glide.With(XRecyclerView.Context).ResumeRequests();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
            {
                try
                {
                    base.OnScrolled(recyclerView, dx, dy);

                    LayoutManager ??= (PreCachingLayoutManager)recyclerView.GetLayoutManager();

                    var visibleItemCount = recyclerView.ChildCount;
                    var totalItemCount = recyclerView.GetAdapter().ItemCount;

                    var pastItems = LayoutManager.FindFirstVisibleItemPosition();

                    var counter = visibleItemCount + pastItems + 35;
                    if (counter < totalItemCount)
                        return;

                    if (IsLoading)
                        return;

                    Console.WriteLine("WOLog" + counter + "WOLog totalItemCount=" + totalItemCount);
                    LoadMoreEvent?.Invoke(this, null);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        #region Listeners

        private class ChildAttachStateChangeListener : Object, IOnChildAttachStateChangeListener
        {
            private readonly WRecyclerView XRecyclerView;

            public ChildAttachStateChangeListener(WRecyclerView recyclerView)
            {
                XRecyclerView = recyclerView;
            }

            public void OnChildViewAttachedToWindow(View view)
            {
                try
                {
                    if (XRecyclerView.ViewHolderParent != null && XRecyclerView.ViewHolderParent.Equals(view))
                    {
                        if (!XRecyclerView.IsVideoViewAdded)
                            return;

                        XRecyclerView.RemoveVideoView(XRecyclerView.VideoSurfaceView);
                        XRecyclerView.PlayPosition = -1;
                        XRecyclerView.VideoSurfaceView.Visibility = ViewStates.Invisible;

                        XRecyclerView.ReleasePlayer();

                        if (XRecyclerView.Thumbnail != null)
                        {
                            XRecyclerView.Thumbnail.Visibility = ViewStates.Visible;
                            if (!string.IsNullOrEmpty(XRecyclerView.VideoUrl.Path))
                                XRecyclerView.NativeFeedAdapter.FullGlideRequestBuilder.Load(XRecyclerView.VideoUrl).Into(XRecyclerView.Thumbnail);
                        }

                        if (XRecyclerView.PlayControl != null) XRecyclerView.PlayControl.Visibility = ViewStates.Visible;

                        var mainHolder = XRecyclerView.GetChildViewHolder(view);
                        if (!(mainHolder is AdapterHolders.PostVideoSectionViewHolder holder))
                            return;

                        holder.VideoImage.Visibility = ViewStates.Visible;
                        holder.PlayControl.Visibility = ViewStates.Visible;
                        holder.VideoProgressBar.Visibility = ViewStates.Gone;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            public void OnChildViewDetachedFromWindow(View view)
            {
                try
                {
                    if (XRecyclerView.ViewHolderParent != null && XRecyclerView.ViewHolderParent.Equals(view))
                    {
                        if (!XRecyclerView.IsVideoViewAdded)
                            return;

                        XRecyclerView.RemoveVideoView(XRecyclerView.VideoSurfaceView);
                        XRecyclerView.PlayPosition = -1;
                        XRecyclerView.VideoSurfaceView.Visibility = ViewStates.Invisible;

                        XRecyclerView.ReleasePlayer();

                        if (XRecyclerView.Thumbnail != null)
                        {
                            XRecyclerView.Thumbnail.Visibility = ViewStates.Visible;
                            if (!string.IsNullOrEmpty(XRecyclerView.VideoUrl.Path))
                                XRecyclerView.NativeFeedAdapter.FullGlideRequestBuilder.Load(XRecyclerView.VideoUrl).Into(XRecyclerView.Thumbnail);
                        }

                        if (XRecyclerView.PlayControl != null) XRecyclerView.PlayControl.Visibility = ViewStates.Visible;

                        var mainHolder = XRecyclerView.GetChildViewHolder(view);
                        if (!(mainHolder is AdapterHolders.PostVideoSectionViewHolder holder))
                            return;

                        holder.VideoImage.Visibility = ViewStates.Visible;
                        holder.PlayControl.Visibility = ViewStates.Visible;
                        holder.VideoProgressBar.Visibility = ViewStates.Gone;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private class ExoPlayerRecyclerEvent : Object, IPlayerEventListener, PlayerControlView.IVisibilityListener
        {
            private readonly ProgressBar LoadingProgressBar;
            private readonly ImageButton VideoPlayButton, VideoResumeButton;
            private readonly ImageView VolumeControl;
            public readonly FrameLayout MFullScreenButton;
            private readonly WRecyclerView XRecyclerView;
            public ExoPlayerRecyclerEvent(PlayerControlView controlView, WRecyclerView recyclerView, AdapterHolders.PostVideoSectionViewHolder holder)
            {
                try
                {
                    XRecyclerView = recyclerView;
                    if (controlView == null)
                        return;

                    var mFullScreenIcon = controlView.FindViewById<ImageView>(Resource.Id.exo_fullscreen_icon);
                    MFullScreenButton = controlView.FindViewById<FrameLayout>(Resource.Id.exo_fullscreen_button);

                    VideoPlayButton = controlView.FindViewById<ImageButton>(Resource.Id.exo_play);
                    VideoResumeButton = controlView.FindViewById<ImageButton>(Resource.Id.exo_pause);
                    VolumeControl = controlView.FindViewById<ImageView>(Resource.Id.exo_volume_icon);

                    if (!AppSettings.ShowFullScreenVideoPost)
                    {
                        mFullScreenIcon.Visibility = ViewStates.Gone;
                        MFullScreenButton.Visibility = ViewStates.Gone;
                    }

                    if (holder != null) LoadingProgressBar = holder.VideoProgressBar;

                    SetVolumeControl(XRecyclerView.VolumeStateProvider == VolumeState.On ? VolumeState.On : VolumeState.Off);

                    if (!VolumeControl.HasOnClickListeners)
                    {
                        VolumeControl.Click += (sender, args) =>
                        {
                            ToggleVolume();
                        };
                    }

                    if (!MFullScreenButton.HasOnClickListeners)
                    {
                        MFullScreenButton.Click += (sender, args) =>
                        {
                            new PostClickListener((Activity)recyclerView.Context).InitFullscreenDialog(Uri.Parse(holder?.VideoUrl), null);
                        };
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            private void ToggleVolume()
            {
                try
                {
                    if (XRecyclerView.VideoPlayer == null)
                        return;

                    switch (XRecyclerView.VolumeStateProvider)
                    {
                        case VolumeState.Off:
                            SetVolumeControl(VolumeState.On);
                            break;
                        case VolumeState.On:
                            SetVolumeControl(VolumeState.Off);
                            break;
                        default:
                            SetVolumeControl(VolumeState.Off);
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            private void SetVolumeControl(VolumeState state)
            {
                try
                {
                    XRecyclerView.VolumeStateProvider = state;
                    switch (state)
                    {
                        case VolumeState.Off:
                            XRecyclerView.VolumeStateProvider = VolumeState.Off;
                            XRecyclerView.VideoPlayer.Volume = 0f;
                            AnimateVolumeControl();
                            break;
                        case VolumeState.On:
                            XRecyclerView.VolumeStateProvider = VolumeState.On;
                            XRecyclerView.VideoPlayer.Volume = 1f;
                            AnimateVolumeControl();
                            break;
                        default:
                            XRecyclerView.VideoPlayer.Volume = 1f;
                            AnimateVolumeControl();
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            private void AnimateVolumeControl()
            {
                try
                {
                    if (VolumeControl == null)
                        return;

                    VolumeControl.BringToFront();
                    switch (XRecyclerView.VolumeStateProvider)
                    {
                        case VolumeState.Off:
                            VolumeControl.SetImageResource(Resource.Drawable.ic_volume_off_grey_24dp);

                            break;
                        case VolumeState.On:
                            VolumeControl.SetImageResource(Resource.Drawable.ic_volume_up_grey_24dp);
                            break;
                        default:
                            VolumeControl.SetImageResource(Resource.Drawable.ic_volume_off_grey_24dp);
                            break;
                    }
                    //VolumeControl.Animate().Cancel();

                    //VolumeControl.Alpha = (1f);

                    //VolumeControl.Animate().Alpha(0f).SetDuration(600).SetStartDelay(1000);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            public void OnLoadingChanged(bool p0)
            {
            }

            public void OnPlaybackParametersChanged(PlaybackParameters p0)
            {
            }

            public void OnPlayerError(ExoPlaybackException p0)
            {
            }

            public void OnPlayerStateChanged(bool playWhenReady, int playbackState)
            {
                try
                {
                    //if (VideoResumeButton == null || VideoPlayButton == null || LoadingProgressBar == null)
                    //    return;

                    if (playbackState == Player.StateEnded)
                    {
                        if (playWhenReady == false)
                            VideoResumeButton.Visibility = ViewStates.Visible;
                        else
                        {
                            VideoResumeButton.Visibility = ViewStates.Gone;
                            VideoPlayButton.Visibility = ViewStates.Visible;
                        }

                        LoadingProgressBar.Visibility = ViewStates.Invisible;
                        XRecyclerView.VideoPlayer.SeekTo(0);

                        TabbedMainActivity.GetInstance()?.SetOffWakeLock();
                    }
                    else if (playbackState == Player.StateReady)
                    {

                        if (playWhenReady == false)
                        {
                            VideoResumeButton.Visibility = ViewStates.Gone;
                            VideoPlayButton.Visibility = ViewStates.Visible;
                        }
                        else
                        {
                            VideoResumeButton.Visibility = ViewStates.Visible;
                        }

                        LoadingProgressBar.Visibility = ViewStates.Invisible;

                        if (!XRecyclerView.IsVideoViewAdded)
                            XRecyclerView.AddVideoView();

                        TabbedMainActivity.GetInstance()?.SetOnWakeLock();
                    }
                    else if (playbackState == Player.StateBuffering)
                    {
                        VideoPlayButton.Visibility = ViewStates.Invisible;
                        LoadingProgressBar.Visibility = ViewStates.Visible;
                        VideoResumeButton.Visibility = ViewStates.Invisible;
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
            }

            public void OnPositionDiscontinuity(int p0)
            {
            }

            public void OnRepeatModeChanged(int p0)
            {
            }

            public void OnSeekProcessed()
            {
            }

            public void OnShuffleModeEnabledChanged(bool p0)
            {
            }

            public void OnTimelineChanged(Timeline p0, Object p1, int p2)
            {
            }

            public void OnTracksChanged(TrackGroupArray p0, TrackSelectionArray p1)
            {
            }

            public void OnVisibilityChange(int p0)
            {
            }
        }

        #endregion

        #region VideoObject player

        private void SetPlayer()
        {
            try
            {
                AdaptiveTrackSelection.Factory trackSelectionFactory = new AdaptiveTrackSelection.Factory();
                var trackSelector = new DefaultTrackSelector(trackSelectionFactory);
                trackSelector.SetParameters(new DefaultTrackSelector.ParametersBuilder().Build());

                VideoPlayer = ExoPlayerFactory.NewSimpleInstance(MainContext, trackSelector);

                DefaultDataSourceFac = new DefaultDataSourceFactory(MainContext, Util.GetUserAgent(MainContext, AppSettings.ApplicationName), BandwidthMeter);
                VideoSurfaceView.UseController = true;
                VideoSurfaceView.Player = VideoPlayer;

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private IMediaSource GetMediaSourceFromUrl(Uri uri, string tag)
        {
            try
            {
                var mBandwidthMeter = new DefaultBandwidthMeter();
                var buildHttpDataSourceFactory = new DefaultDataSourceFactory(MainContext, mBandwidthMeter, new DefaultHttpDataSourceFactory(Util.GetUserAgent(MainContext, AppSettings.ApplicationName), new DefaultBandwidthMeter()));
                var buildHttpDataSourceFactoryNull = new DefaultDataSourceFactory(MainContext, mBandwidthMeter, new DefaultHttpDataSourceFactory(Util.GetUserAgent(MainContext, AppSettings.ApplicationName), null));
                int type = Util.InferContentType(uri, null);
                IMediaSource src = type switch
                {
                    C.TypeSs => new SsMediaSource.Factory(new DefaultSsChunkSource.Factory(buildHttpDataSourceFactory), buildHttpDataSourceFactoryNull).SetTag(tag).CreateMediaSource(uri),
                    C.TypeDash => new DashMediaSource.Factory(new DefaultDashChunkSource.Factory(buildHttpDataSourceFactory), buildHttpDataSourceFactoryNull).SetTag(tag).CreateMediaSource(uri),
                    C.TypeHls => new HlsMediaSource.Factory(buildHttpDataSourceFactory).SetTag(tag).CreateMediaSource(uri),
                    C.TypeOther => new ExtractorMediaSource.Factory(buildHttpDataSourceFactory).SetTag(tag).CreateMediaSource(uri),
                    _ => new ExtractorMediaSource.Factory(buildHttpDataSourceFactory).SetTag(tag).CreateMediaSource(uri)
                };
                return src;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public void CacheVideosFiles(Uri url)
        {
            try
            {
                Cache ??= new SimpleCache(MainContext.CacheDir, new NoOpCacheEvictor());

                CacheDataSourceFactory ??= new CacheDataSourceFactory(Cache, DefaultDataSourceFac);

                var dataSpec = new DataSpec(url, 0, 3000 * 1024, null); //0, 1000 * 1024, null
                var counters = new CacheUtil.CachingCounters();

                CacheUtil.GetCached(dataSpec, Cache, counters);

                if (counters.ContentLength == counters.TotalCachedBytes())
                {

                }
                else if (counters.TotalCachedBytes() == 0)
                {
                    // not cached at all
                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            var cacheDataSource = new CacheDataSource(Cache, CacheDataSourceFactory.CreateDataSource());
                            CacheUtil.Cache(dataSpec, Cache, cacheDataSource, counters, new AtomicBoolean());
                            double downloadPercentage = (counters.TotalCachedBytes() * 100d) / counters.ContentLength;
                            Console.WriteLine(downloadPercentage);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    });
                }
                else
                {
                    // just few mb cached
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private int GetVisibleVideoSurfaceHeight(int playPosition)
        {
            try
            {
                var at = playPosition - ((LinearLayoutManager)Objects.RequireNonNull(GetLayoutManager())).FindFirstVisibleItemPosition();

                var child = GetChildAt(at);
                if (child == null)
                {
                    return 0;
                }
                int[] location = new int[2];
                child.GetLocationInWindow(location);
                if (location[1] < 0)
                {
                    return location[1] + VideoSurfaceDefaultHeight;
                }
                else
                {
                    return ScreenDefaultHeight - location[1];
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 0;
            }
        }

        private void AddVideoView()
        {
            try
            {
                //var d = MediaContainerLayout.FindViewById<PlayerView>(VideoSurfaceView.Id);
                //if (d == null)
                //{
                MediaContainerLayout.AddView(VideoSurfaceView);
                IsVideoViewAdded = true;
                VideoSurfaceView.RequestFocus();
                VideoSurfaceView.Visibility = ViewStates.Visible;

                //}

                Thumbnail.Visibility = ViewStates.Gone;
                PlayControl.Visibility = ViewStates.Gone;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void RemoveVideoView(PlayerView videoView)
        {
            try
            {
                var parent = (ViewGroup)videoView.Parent;
                if (parent == null)
                    return;

                var index = parent.IndexOfChild(videoView);
                if (index < 0)
                    return;

                parent.RemoveViewAt(index);
                IsVideoViewAdded = false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void StopVideo()
        {
            try
            {
                if (VideoSurfaceView.Player == null) return;
                if (VideoSurfaceView.Player.PlaybackState == Player.StateReady)
                {
                    VideoSurfaceView.Player.PlayWhenReady = false;
                    VideoSurfaceView.Player.Stop();
                    TabbedMainActivity.GetInstance()?.SetOffWakeLock();
                }

                ReleasePlayer();

                //GC Collect
                GC.Collect();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public void ReleasePlayer()
        {
            try
            {
                if (VideoPlayer != null)
                {
                    VideoPlayer.Release();
                    VideoPlayer = null;
                }

                ViewHolderParent = null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        public void OnLoadMore(int currentPage)
        {
            try
            {
                if (NativeFeedAdapter.NativePostType == NativeFeedType.Memories || NativeFeedAdapter.NativePostType == NativeFeedType.Share)
                    return;

                MainScrollEvent.IsLoading = true;
                var diff = NativeFeedAdapter.ListDiffer;
                var list = new List<AdapterModelsClass>(diff);
                if (list.Count <= 20)
                    return;

                var item = list.LastOrDefault();

                NativeFeedAdapter.SetLoading();

                var lastItem = list.IndexOf(item);

                item = list[lastItem];

                string offset;

                if (item.TypeView == PostModelType.Divider || item.TypeView == PostModelType.ViewProgress || item.TypeView == PostModelType.AdMob1 || item.TypeView == PostModelType.AdMob2 || item.TypeView == PostModelType.FbAdNative || item.TypeView == PostModelType.AdsPost || item.TypeView == PostModelType.SuggestedGroupsBox || item.TypeView == PostModelType.SuggestedUsersBox || item.TypeView == PostModelType.CommentSection || item.TypeView == PostModelType.AddCommentSection)
                {
                    item = list.LastOrDefault(a => a.TypeView != PostModelType.Divider && a.TypeView != PostModelType.ViewProgress && a.TypeView != PostModelType.AdMob1 && a.TypeView != PostModelType.AdMob2 && a.TypeView != PostModelType.FbAdNative && a.TypeView != PostModelType.AdsPost && a.TypeView != PostModelType.SuggestedGroupsBox && a.TypeView != PostModelType.SuggestedUsersBox && a.TypeView != PostModelType.CommentSection && a.TypeView != PostModelType.AddCommentSection);
                    offset = item?.PostData?.Id ?? "0";
                    Console.WriteLine(offset);
                }
                else
                {
                    offset = item.PostData?.Id ?? "0";
                }

                Console.WriteLine(offset);

                if (!Methods.CheckConnectivity())
                    Toast.MakeText(MainContext, MainContext.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                else
                {
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => NativeFeedAdapter.NativePostType != NativeFeedType.HashTag ? FetchNewsFeedApiPosts(offset) : FetchNewsFeedApiPosts(offset, "Add", Hash) });
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

         //wael function fix emojis 
        public static Dictionary<string, string> GetAddDiscountList()
        {
            try
            {
                var arrayAdapter = new Dictionary<string, string>
                {
                    {":)", "smile"},
                    {"(&lt;", "joy"},
                    {"**)", "relaxed"},
                    {":p", "stuck-out-tongue-winking-eye"},
                    {":_p", "stuck-out-tongue"},
                    {"B)", "sunglasses"},
                    {";)", "wink"},
                    {":D", "grin"},
                    {"/_)", "smirk"},
                    {"0)", "innocent"},
                    {":_(", "cry"},
                    {":__(", "sob"},
                    {":(", "disappointed"},
                    {":*", "kissing-heart"},
                    {"&lt;3", "heart"},
                    {"&lt;/3", "broken-heart"},
                    {"*_*", "heart-eyes"},
                    {"&lt;5", "star"},
                    {":o", "open-mouth"},
                    {":0", "scream"},
                    {"o(", "anguished"},
                    {"-_(", "unamused"},
                    {"x(", "angry"},
                    {"X(", "rage"},
                    {"-_-", "expressionless"},
                    {":-/", "confused"},
                    {":|", "neutral-face"},
                    {"!_", "exclamation"},
                    {":|", "neutral-face"},
                    {":|", "neutral-face"},
                    {":yum:", "yum"},
                    {":triumph:", "triumph"},
                    {":imp:", "imp"},
                    {":hear_no_evil:", "hear-no-evil"},
                    {":alien:", "alien"},
                    {":yellow_heart:", "yellow-heart"},
                    {":sleeping:", "sleeping"},
                    {":mask:", "mask"},
                    {":no_mouth:", "no-mouth"},
                    {":weary:", "weary"},
                    {":dizzy_face:", "dizzy-face"},
                    {":man:", "man"},
                    {":woman:", "woman"},
                    {":boy:", "boy"},
                    {":girl:", "girl"},
                    {":?lder_man:", "older-man"},
                    {":?lder_woman:", "older-woman"},
                    {":cop:", "cop"},
                    {":dancers:", "dancers"},
                    {":speak_no_evil:", "speak-no-evil"},
                    {":lips:", "lips"},
                    {":see_no_evil:", "see-no-evil"},
                    {":dog:", "dog"},
                    {":bear:", "bear"},
                    {":rose:", "rose"},
                    {":gift_heart:", "gift-heart"},
                    {":ghost:", "ghost"},
                    {":bell:", "bell"},
                    {":video_game:", "video-game"},
                    {":soccer:", "soccer"},
                    {":books:", "books"},
                    {":moneybag:", "moneybag"},
                    {":mortar_board:", "mortar-board"},
                    {":hand:", "hand"},
                    {":tiger:", "tiger"},
                    {":elephant:", "elephant"},
                    {":scream_cat:", "scream-cat"},
                    {":monkey:", "monkey"},
                    {":bird:", "bird"},
                    {":snowflake:", "snowflake"},
                    {":sunny:", "sunny"},
                    {":?cean:", "ocean"},
                    {":umbrella:", "umbrella"},
                    {":hibiscus:", "hibiscus"},
                    {":tulip:", "tulip"},
                    {":computer:", "computer"},
                    {":bomb:", "bomb"},
                    {":gem:", "gem"},
                    {":ring:", "ring"},
                    {":)", "??"},
                    {"(&lt;", "??"},
                    {"**)", "??"},
                    {":p", "??"},
                    {":_p", "??"},
                    {"B)", "??"},
                    {";)", "??"},
                    {":D", "??"},
                    {"/_)", "smirk"},
                    {"0)", "innocent"},
                    {":_(", "cry"},
                    {":__(", "sob"},
                    {":(", "disappointed"},
                    {":*", "kissing-heart"},
                    {"&lt;3", "heart"},
                    {"&lt;/3", "broken-heart"},
                    {"*_*", "heart-eyes"},
                    {"&lt;5", "star"},
                    {":o", "open-mouth"},
                    {":0", "scream"},
                    {"o(", "anguished"},
                    {"-_(", "unamused"},
                    {"x(", "angry"},
                    {"X(", "rage"},
                    {"-_-", "expressionless"},
                    {":-/", "confused"},
                    {":|", "neutral-face"},
                    {"!_", "exclamation"},
                    {":|", "neutral-face"},
                };

                return arrayAdapter;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new Dictionary<string, string>();
            }
        }
    }
}