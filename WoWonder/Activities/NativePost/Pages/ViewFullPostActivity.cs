using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using AutoMapper;
using WoWonder.Activities.Comment.Adapters;
using WoWonder.Activities.NativePost.Extra;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Comments;
using WoWonderClient.Classes.Global;
using WoWonderClient.Requests;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Uri = Android.Net.Uri;

namespace WoWonder.Activities.NativePost.Pages
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class ViewFullPostActivity : AppCompatActivity
    {
        #region Variables Basic

        private WRecyclerView MainRecyclerView;
        private NativePostAdapter MAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerView CommentsRecyclerView;
        private CommentAdapter CommentAdapter;
        private ViewStub EmptyStateLayout;
        private View Inflated;
        private string PostId;
        
        private AdsGoogle.AdMobRewardedVideo RewardedVideoAd;

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
                SetContentView(Resource.Layout.ViewFullPostLayout);

                PostId = Intent.GetStringExtra("Id") ?? string.Empty;

                //Get Value And Set Toolbar 
                InitToolbar();
                SetRecyclerViewAdapters();

                StartApiService();
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
                RewardedVideoAd?.OnDestroy(this);
                MainRecyclerView.ReleasePlayer();
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

        private void InitToolbar()
        {
            try
            {
                var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolbar != null)
                {
                    toolbar.Title = GetString(Resource.String.Lbl_Post);
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
                EmptyStateLayout = FindViewById<ViewStub>(Resource.Id.viewStub);
                MainRecyclerView = FindViewById<WRecyclerView>(Resource.Id.Recyler);
                MAdapter = new NativePostAdapter(this,"", MainRecyclerView, NativeFeedType.Global, SupportFragmentManager);

                SwipeRefreshLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
                 
                MainRecyclerView.SetXAdapter(MAdapter, SwipeRefreshLayout); 

                CommentsRecyclerView = FindViewById<RecyclerView>(Resource.Id.recycler_view);
                CommentsRecyclerView.NestedScrollingEnabled = false;
                CommentAdapter = new CommentAdapter(this)
                {
                    CommentList = new ObservableCollection<CommentObjectExtra>()
                }; 
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
                    SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
                }
                else
                {
                    SwipeRefreshLayout.Refresh -= SwipeRefreshLayoutOnRefresh;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Event

        //Refresh
        private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                MAdapter.ListDiffer.Clear();
                MAdapter.NotifyDataSetChanged();

                CommentAdapter.CommentList.Clear();
                CommentAdapter.NotifyDataSetChanged();
                
                StartApiService();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        #region LoadPostData
         
        private void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadPostDataAsync});
        }

        private async Task LoadPostDataAsync()
        {
            if (Methods.CheckConnectivity())
            {
                (int apiStatus, var respond) = await RequestsAsync.Global.Get_Post_Data(PostId, "post_data");
                if (apiStatus == 200)
                {
                    if (respond is GetPostDataObject result)
                    {
                        var data = result.PostData;
                        if (data != null)
                        { 
                            RunOnUiThread(() =>
                            {
                                try
                                {
                                    MainRecyclerView.CacheVideosFiles(Uri.Parse(data.PostFileFull));

                                    var combine = new FeedCombiner(data, MAdapter.ListDiffer, this);
                                    combine.CombineDefaultPostSections();

                                    MAdapter.NotifyDataSetChanged();  
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                }
                            });

                             PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadDataComment("0") });
                        }
                    }
                }
                else Methods.DisplayReportResult(this, respond);

                SwipeRefreshLayout.Refreshing = false;
                RunOnUiThread(ShowEmptyPage);
            }
            else
            {
                SwipeRefreshLayout.Refreshing = false;

                Inflated = EmptyStateLayout.Inflate();
                EmptyStateInflater x = new EmptyStateInflater();
                x.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                    x.EmptyStateButton.Click += null;
                    x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                }

                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
            }
        }
         
        private void ShowEmptyPage()
        {
            try
            {
                SwipeRefreshLayout.Refreshing = false;

                if (MAdapter.ListDiffer.Count > 0)
                {
                    MainRecyclerView.Visibility = ViewStates.Visible;
                    CommentsRecyclerView.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;

                }
                else
                {
                    MainRecyclerView.Visibility = ViewStates.Gone;
                    CommentsRecyclerView.Visibility = ViewStates.Gone;

                    Inflated ??= EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoPost);
                    if (!x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click += null;
                    }
                    EmptyStateLayout.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception e)
            {
                SwipeRefreshLayout.Refreshing = false;
                Console.WriteLine(e);
            }
        }

        //No Internet Connection 
        private void EmptyStateButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                StartApiService();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        #region LoadDataComment
         
        private async Task LoadDataComment(string offset)
        {
            if (Methods.CheckConnectivity())
            {
                int countList = CommentAdapter.CommentList.Count;
                (int apiStatus, var respond) = await RequestsAsync.Comment.GetPostComments(PostId, "10", offset);
                if (apiStatus != 200 || !(respond is CommentObject result) || result.CommentList == null)
                {
                    Methods.DisplayReportResult(this, respond);
                }
                else
                {
                    var respondList = result.CommentList?.Count;
                    if (respondList > 0)
                    {
                        foreach (var item in from item in result.CommentList let check = CommentAdapter.CommentList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                        {
                            var db = Mapper.Map<CommentObjectExtra>(item);
                            if (db != null) CommentAdapter.CommentList.Add(db);
                        }

                        if (countList > 0)
                        {
                            RunOnUiThread(() => { CommentAdapter.NotifyItemRangeInserted(countList, CommentAdapter.CommentList.Count - countList); });
                        }
                        else
                        {
                            RunOnUiThread(() => { CommentAdapter.NotifyDataSetChanged(); });
                        }
                    }
                }

                RunOnUiThread(ShowEmptyPage2);
            }
        }

        private void ShowEmptyPage2()
        {
            try
            {
                SwipeRefreshLayout.Refreshing = false;

                if (CommentAdapter.CommentList.Count > 0)
                {
                    CommentsRecyclerView.Visibility = ViewStates.Visible;

                    var emptyStateChecker = CommentAdapter.CommentList.FirstOrDefault(a => a.Text == CommentAdapter.EmptyState);
                    if (emptyStateChecker != null && CommentAdapter.CommentList.Count > 1)
                    {
                        CommentAdapter.CommentList.Remove(emptyStateChecker);
                        CommentAdapter.NotifyDataSetChanged();
                    }
                }
                else
                {
                    CommentsRecyclerView.Visibility = ViewStates.Gone;

                    CommentAdapter.CommentList.Clear();
                    var d = new CommentObjectExtra { Text = CommentAdapter.EmptyState };
                    CommentAdapter.CommentList.Add(d);
                    CommentAdapter.NotifyDataSetChanged();
                }
            }
            catch (Exception e)
            {
                SwipeRefreshLayout.Refreshing = false;
                Console.WriteLine(e);
            }
        }

        #endregion
         
    }
}