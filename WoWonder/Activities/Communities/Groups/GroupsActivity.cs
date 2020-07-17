using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.Gms.Ads;
using Android.Graphics;
using Android.Support.Design.Widget;
using Bumptech.Glide.Integration.RecyclerView;
using Bumptech.Glide.Util;
using WoWonder.Activities.Communities.Adapters;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Activities.Search;
using WoWonder.Activities.Suggested.Groups;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Group;
using WoWonderClient.Requests;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace WoWonder.Activities.Communities.Groups
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class GroupsActivity : AppCompatActivity
    {
        #region Variables Basic

        public SocialAdapter MAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private ViewStub EmptyStateLayout;
        private View Inflated;
        private TextView TxtSuggestedGroups;
        private string UserId;
        private AdView MAdView;
        private static GroupsActivity Instance;
        private FloatingActionButton CreateButton;

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
                SetContentView(Resource.Layout.RecyclerDefaultLayout);

                UserId = Intent.GetStringExtra("UserID") ?? "";

                Instance = this;

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();

                StartApiService();
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
                MAdView?.Resume();
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
                MAdView?.Pause();
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
                EmptyStateLayout = FindViewById<ViewStub>(Resource.Id.viewStub);

                SwipeRefreshLayout = (SwipeRefreshLayout)FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = true;

                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
                SwipeRefreshLayout.SetBackgroundColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#282828") : Color.White);
                MRecycler.SetBackgroundColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#282828") : Color.White);

                TxtSuggestedGroups = FindViewById<TextView>(Resource.Id.toolbar_title);
                TxtSuggestedGroups.Text = GetString(Resource.String.Lbl_SuggestedGroups);
                TxtSuggestedGroups.Visibility = ViewStates.Visible;

                CreateButton = FindViewById<FloatingActionButton>(Resource.Id.floatingActionButtonView);
                CreateButton.Visibility = ViewStates.Visible;
                CreateButton.SetImageResource(Resource.Drawable.ic_add);

                MAdView = FindViewById<AdView>(Resource.Id.adView);
                AdsGoogle.InitAdView(MAdView, MRecycler);
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
                    toolbar.Title = GetText(Resource.String.Lbl_ExploreGroup);
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
                MAdapter = new SocialAdapter(this, SocialModelType.Groups); 
                LayoutManager = new LinearLayoutManager(this);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.SetAdapter(MAdapter);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<SocialModelsClass>(this, MAdapter, sizeProvider, 8);
                MRecycler.AddOnScrollListener(preLoader);
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
                    TxtSuggestedGroups.Click += TxtSuggestedGroupsOnClick;
                    MAdapter.GroupItemClick += MAdapterOnItemClick;
                    SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
                    CreateButton.Click += CreateButtonOnClick;
                }
                else
                {
                    TxtSuggestedGroups.Click -= TxtSuggestedGroupsOnClick;
                    MAdapter.GroupItemClick -= MAdapterOnItemClick;
                    SwipeRefreshLayout.Refresh -= SwipeRefreshLayoutOnRefresh;
                    CreateButton.Click += CreateButtonOnClick;
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
                MAdView?.Destroy();

                MAdapter = null;
                SwipeRefreshLayout = null;
                MRecycler = null;
                EmptyStateLayout = null;
                Inflated = null;
                Instance = null;
                CreateButton = null;
                TxtSuggestedGroups = null;
                UserId = null;
                MAdView = null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static GroupsActivity GetInstance()
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

        #endregion

        #region Events

        private void TxtSuggestedGroupsOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(SuggestedGroupActivity));
                StartActivity(intent);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        //Refresh
        private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                MAdapter.SocialList.Clear();
                MAdapter.NotifyDataSetChanged();

                StartApiService();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void MAdapterOnItemClick(object sender, GroupsAdapterClickEventArgs e)
        {
            try
            {
                var item = MAdapter.GetItem(e.Position);
                if (item != null)
                { 
                    MainApplication.GetInstance()?.NavigateTo(this, typeof(GroupProfileActivity), item.GroupData);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Event Create New Group
        private void CreateButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(CreateGroupActivity));
                StartActivityForResult(intent, 200);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        #region Permissions && Result

        //Result
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                if (requestCode == 200 && resultCode == Result.Ok)
                {
                    string result = data.GetStringExtra("groupItem");

                    var item = JsonConvert.DeserializeObject<GroupClass>(result);

                    var check = MAdapter.SocialList.FirstOrDefault(a => a.MangedGroupsModel != null);
                    if (check != null)
                    {
                        check.MangedGroupsModel.GroupsList.Insert(0, item);
                        MAdapter.NotifyDataSetChanged();
                    }
                    else
                    {
                        var socialSection = new SocialModelsClass
                        {
                            Id = int.Parse(item.Id),
                            TypeView = SocialModelType.MangedGroups,
                            MangedGroupsModel = new GroupsModelClass()
                            {
                                GroupsList = new List<GroupClass>() { item }
                            }
                        };
                        MAdapter.SocialList.Add(socialSection);
                        MAdapter.NotifyDataSetChanged();
                    } 
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Get Communities >> Group

        private void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { GetCommunitiesListGroupApi });
        }
         
        private async Task GetCommunitiesListGroupApi()
        {
            if (Methods.CheckConnectivity())
            {
                var (apiStatus, respond) = await RequestsAsync.Global.Get_Community(UserId, "groups");
                if (apiStatus != 200 || !(respond is GetCommunityObject result) || result.Groups == null)
                {
                    Methods.DisplayReportResult(this, respond);
                }
                else
                { 
                    if (result.Groups.Count > 0)
                    {
                        var checkModel = MAdapter.SocialList.FirstOrDefault(a => a.TypeView == SocialModelType.MangedGroups);
                        if (checkModel == null)
                        {
                            var chkList = result.Groups.Where(a => a.UserId == UserDetails.UserId).ToList();
                            if (chkList.Count > 0)
                            {
                                ListUtils.MyGroupList = new ObservableCollection<GroupClass>(chkList);

                                var socialSection = new SocialModelsClass
                                {
                                    MangedGroupsModel = new GroupsModelClass
                                    {
                                        GroupsList = new List<GroupClass>(chkList),
                                        More = "",
                                        TitleHead = GetString(Resource.String.Lbl_Manage_Groups)
                                    },
                                    Id = 11111111,
                                    TypeView = SocialModelType.MangedGroups
                                };
                                MAdapter.SocialList.Add(socialSection);
                            }
                        }

                        var section = new SocialModelsClass
                        {
                            Id = 000001010101,
                            TitleHead = GetString(Resource.String.Lbl_Joined_Groups),
                            TypeView = SocialModelType.Section
                        };

                        MAdapter.SocialList.Add(section);

                        foreach (var group in result.Groups)
                        {
                            if (group.UserId == UserDetails.UserId)
                            {
                                checkModel?.MangedGroupsModel.GroupsList.Add(group);
                            }
                            else
                            {
                                var socialSection = new SocialModelsClass
                                {
                                    GroupData = group,
                                    Id = int.Parse(group.GroupId),
                                    TypeView = SocialModelType.JoinedGroups
                                };
                                group.IsJoined = "true";
                                MAdapter.SocialList.Add(socialSection);
                            }
                        }

                        RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                    }
                }
                
                RunOnUiThread(ShowEmptyPage);
            }
            else
            {
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

                if (MAdapter.SocialList.Count > 0)
                {
                    MRecycler.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone; 
                }
                else
                {
                    MRecycler.Visibility = ViewStates.Gone;

                    Inflated ??= EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoGroup);
                    if (!x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click += null;
                        x.EmptyStateButton.Click += SearchButtonOnClick;
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

        //Event Open Search And Get Group Random
        private void SearchButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(SearchTabbedActivity));
                intent.PutExtra("Key", "Random_Groups");
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
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
    }
}