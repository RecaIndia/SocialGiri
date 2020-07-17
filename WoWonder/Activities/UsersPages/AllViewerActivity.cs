using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Ads;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Bumptech.Glide.Integration.RecyclerView;
using Bumptech.Glide.Util;
using Newtonsoft.Json;
using WoWonder.Activities.Communities.Groups;
using WoWonder.Activities.Communities.Pages;
using WoWonder.Activities.Contacts.Adapters;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Activities.Search.Adapters;
using WoWonder.Activities.Story;
using WoWonder.Activities.Story.Adapters;
using WoWonder.Activities.UserProfile.Adapters;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Album;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Classes.Story;
using WoWonderClient.Classes.User;
using WoWonderClient.Requests;
using Exception = System.Exception;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace WoWonder.Activities.UsersPages
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class AllViewerActivity : AppCompatActivity
    {
        #region Variables Basic

        private dynamic MAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private GridLayoutManager GridLayoutManager;
        private ViewStub EmptyStateLayout;
        private View Inflated;
        //Type = MangedGroupsModel , MangedPagesModel >> itemObject =  SocialModelsClass
        //Type = StoryModel , FollowersModel , GroupsModel , PagesModel >> itemObject = AdapterModelsClass
        //private SocialModelsClass ModelsClass;
        private AdapterModelsClass AdapterModelsClass;
        private FollowersModelClass FollowersModel;
        private GroupsModelClass GroupsModel;
        private PagesModelClass PagesModel;
        private ImagesModelClass ImagesModel; 
        private string Type, PassedId;
        private RecyclerViewOnScrollListener MainScrollEvent;
        private AdView MAdView;
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
                SetContentView(Resource.Layout.RecyclerDefaultLayout);

                Type = Intent.GetStringExtra("Type") ?? "";
                PassedId = Intent.GetStringExtra("PassedId") ?? "";

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();

                SetData();
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
                MAdView?.Resume();
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
                MAdView?.Pause();
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
                EmptyStateLayout = FindViewById<ViewStub>(Resource.Id.viewStub);

                SwipeRefreshLayout = (SwipeRefreshLayout)FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
 

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
                    switch (Type)
                    {
                        case "MangedGroupsModel":
                            toolbar.Title = GetString(Resource.String.Lbl_Manage_Groups);
                            break;
                        case "MangedPagesModel":
                            toolbar.Title = GetString(Resource.String.Lbl_Manage_Pages);
                            break;
                        case "StoryModel":
                            toolbar.Title = GetString(Resource.String.Lbl_Story);
                            break;
                        case "FollowersModel":
                            toolbar.Title = GetString(Resource.String.Lbl_Following);
                            break;
                        case "GroupsModel":
                            toolbar.Title = GetString(Resource.String.Lbl_Groups);
                            break;
                        case "PagesModel":
                            toolbar.Title = GetString(Resource.String.Lbl_Pages);
                            break;
                        case "ImagesModel":
                            toolbar.Title = GetString(Resource.String.Lbl_Photos);
                            break;
                    }
                     
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
                dynamic preLoader = null;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);

                switch (Type)
                {
                    case "GroupsModel":
                    case "MangedGroupsModel":
                        LayoutManager = new LinearLayoutManager(this);
                        MAdapter = new SearchGroupAdapter(this) {GroupList = new ObservableCollection<GroupClass>()};
                        if (MAdapter is SearchGroupAdapter adapter1) adapter1.ItemClick += GroupsModelOnItemClick;
                        preLoader = new RecyclerViewPreloader<GroupClass>(this, MAdapter, sizeProvider, 10);
                        break;
                    case "PagesModel":
                    case "MangedPagesModel":
                        LayoutManager = new LinearLayoutManager(this);
                        MAdapter = new SearchPageAdapter(this) {PageList = new ObservableCollection<PageClass>()};
                        if (MAdapter is SearchPageAdapter adapter2) adapter2.ItemClick += PagesModelOnItemClick;
                        preLoader = new RecyclerViewPreloader<PageClass>(this, MAdapter, sizeProvider, 10);
                        break;
                    case "StoryModel":
                        LayoutManager = new LinearLayoutManager(this);
                        MAdapter = new RowStoryAdapter(this) { StoryList = new ObservableCollection<GetUserStoriesObject.StoryObject>() };
                        if (MAdapter is RowStoryAdapter adapter3) adapter3.ItemClick += StoryModelOnItemClick;
                        preLoader = new RecyclerViewPreloader<GetUserStoriesObject.StoryObject>(this, MAdapter, sizeProvider, 10);
                        break;
                    case "FollowersModel":
                        LayoutManager = new LinearLayoutManager(this);
                        MAdapter = new ContactsAdapter(this, true, ContactsAdapter.TypeTextSecondary.LastSeen){UserList = new ObservableCollection<UserDataObject>()};
                        if (MAdapter is ContactsAdapter adapter4) adapter4.ItemClick += AdapterFollowersOnItemClick;
                        preLoader = new RecyclerViewPreloader<UserDataObject>(this, MAdapter, sizeProvider, 10);
                        break;
                    case "ImagesModel":
                        GridLayoutManager = new GridLayoutManager(this, 3);
                        GridLayoutManager.SetSpanSizeLookup(new MySpanSizeLookup(4, 1, 1)); //5, 1, 2 
                        MAdapter = new UserPhotosAdapter(this){UserPhotosList =new ObservableCollection<PostDataObject>()};
                        if (MAdapter is UserPhotosAdapter adapter5) adapter5.ItemClick += AdapterPhotosOnItemClick;
                        preLoader = new RecyclerViewPreloader<PostDataObject>(this, MAdapter, sizeProvider, 10);
                        break; 
                }

                MRecycler.SetLayoutManager(LayoutManager ?? GridLayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true; 
                MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter);
                 
                RecyclerViewOnScrollListener xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(LayoutManager);
                MainScrollEvent = xamarinRecyclerViewOnScrollListener;
                MainScrollEvent.LoadMoreEvent += MainScrollEventOnLoadMoreEvent;
                MRecycler.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);
                MainScrollEvent.IsLoading = false;

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
                RewardedVideoAd?.OnDestroy(this);

                MAdapter = null;
                SwipeRefreshLayout = null;
                MRecycler = null;
                EmptyStateLayout = null;
                Inflated = null;
                RewardedVideoAd = null;
                MAdView = null; 
                AdapterModelsClass = null;
                FollowersModel = null;
                GroupsModel = null;
                PagesModel = null;
                ImagesModel = null;

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        #endregion

        #region Events


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

        private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                if (!MainScrollEvent.IsLoading)
                    switch (Type)
                    {
                        case "StoryModel":

                            break;
                        case "FollowersModel":
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadContactsAsync });
                            break;
                        case "GroupsModel":

                            break;
                        case "PagesModel":

                            break;
                        case "ImagesModel":
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadMyImage });
                            break;
                    }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
         
        private void PagesModelOnItemClick(object sender, SearchPageAdapterClickEventArgs e)
        {
            try
            {
                if (MAdapter is SearchPageAdapter adapter)
                {
                    var item = adapter.GetItem(e.Position);
                    if (item != null)
                    {
                        MainApplication.GetInstance()?.NavigateTo(this, typeof(PageProfileActivity), item);
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            } 
        }

        private void GroupsModelOnItemClick(object sender, SearchGroupAdapterClickEventArgs e)
        {
            try
            {
                if (MAdapter is SearchGroupAdapter adapter)
                {
                    var item = adapter.GetItem(e.Position);
                    if (item != null)
                    {
                        MainApplication.GetInstance()?.NavigateTo(this, typeof(GroupProfileActivity), item);
                    }
                } 
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void AdapterFollowersOnItemClick(object sender, ContactsAdapterClickEventArgs e)
        {
            try
            {
                if (MAdapter is ContactsAdapter adapter)
                {
                    var item = adapter.GetItem(e.Position);
                    if (item != null)
                    {
                        WoWonderTools.OpenProfile(this, item.UserId, item);
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void StoryModelOnItemClick(object sender, RowStoryAdapterClickEventArgs e)
        {
            try
            {
                if (MAdapter is RowStoryAdapter adapter)
                {
                    try
                    {
                        //Open View Story Or Create New Story
                        var item = adapter.GetItem(e.Position);
                        if (item != null)
                        { 
                            if (item.Type != "Your")
                            {
                                Intent intent = new Intent(this, typeof(ViewStoryActivity));
                                intent.PutExtra("UserId", item.UserId);
                                intent.PutExtra("DataItem", JsonConvert.SerializeObject(item));
                                StartActivity(intent);
                            } 
                        }
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                    } 
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void AdapterPhotosOnItemClick(object sender, UserPhotosAdapterClickEventArgs e)
        {
            try
            {
                if (MAdapter is UserPhotosAdapter adapter)
                {
                    try
                    {
                        //Open View Story Or Create New Story
                        var item = adapter.GetItem(e.Position);
                        if (item != null)
                        {
                            var intent = new Intent(this, typeof(ImagePostViewerActivity));
                            intent.PutExtra("itemIndex", "00"); 
                            intent.PutExtra("AlbumObject", JsonConvert.SerializeObject(item)); // PostDataObject
                            StartActivity(intent); 
                        }
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        private void SetData()
        {
            try
            {
                var item = Intent.GetStringExtra("itemObject");
                if (string.IsNullOrEmpty(item))
                    return;
                  
                switch (Type)
                {
                    case "MangedGroupsModel":
                    case "MangedPagesModel":
                    {
                        if (MAdapter is SearchGroupAdapter adapter1)
                        {
                            GroupsModel = JsonConvert.DeserializeObject<GroupsModelClass>(item);
                            if (GroupsModel != null)
                            {
                                adapter1.GroupList = new ObservableCollection<GroupClass>(GroupsModel.GroupsList);
                                adapter1.NotifyDataSetChanged();
                            } 
                        }
                        else if (MAdapter is SearchPageAdapter adapter2)
                        {
                            PagesModel = JsonConvert.DeserializeObject<PagesModelClass>(item);
                            if (PagesModel != null)
                            {
                                adapter2.PageList = new ObservableCollection<PageClass>(PagesModel.PagesList);
                                adapter2.NotifyDataSetChanged();
                            }
                        }

                        break;
                    } 
                    case "StoryModel":
                    case "FollowersModel":
                    case "GroupsModel":
                    case "PagesModel":
                    case "ImagesModel":
                    {
                            if (MAdapter is SearchGroupAdapter adapter1)
                            {
                                GroupsModel = JsonConvert.DeserializeObject<GroupsModelClass>(item);
                                if (GroupsModel != null)
                                {
                                    adapter1.GroupList = new ObservableCollection<GroupClass>(GroupsModel.GroupsList);
                                    adapter1.NotifyDataSetChanged();
                                }
                            }
                            else if (MAdapter is SearchPageAdapter adapter2)
                            {
                                PagesModel = JsonConvert.DeserializeObject<PagesModelClass>(item);
                                if (PagesModel != null)
                                {
                                    adapter2.PageList = new ObservableCollection<PageClass>(PagesModel.PagesList);
                                    adapter2.NotifyDataSetChanged();
                                }
                            }
                            else if (MAdapter is ContactsAdapter adapter3)
                            {
                                FollowersModel = JsonConvert.DeserializeObject<FollowersModelClass>(item);
                                if (FollowersModel != null)
                                {
                                    adapter3.UserList = new ObservableCollection<UserDataObject>(FollowersModel.FollowersList);
                                    adapter3.NotifyDataSetChanged();
                                }
                            }
                            else if (MAdapter is RowStoryAdapter adapter4)
                            {
                                AdapterModelsClass = JsonConvert.DeserializeObject<AdapterModelsClass>(item);
                                if (AdapterModelsClass != null)
                                {
                                    adapter4.StoryList = new ObservableCollection<GetUserStoriesObject.StoryObject>(AdapterModelsClass.StoryList);
                                    adapter4.StoryList.Remove(adapter4.StoryList.FirstOrDefault(a => a.Type == "Your"));
                                    adapter4.NotifyDataSetChanged();
                                }
                            }
                            else if (MAdapter is UserPhotosAdapter adapter5)
                            {
                                ImagesModel = JsonConvert.DeserializeObject<ImagesModelClass>(item);
                                if (ImagesModel != null)
                                {
                                    adapter5.UserPhotosList = new ObservableCollection<PostDataObject>(ImagesModel.ImagesList);
                                    adapter5.NotifyDataSetChanged();
                                }
                            } 
                            break;
                    } 
                }

                if (MAdapter?.ItemCount > 0)
                {
                    MRecycler.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;
                }
                else
                    ShowEmptyPage();

                SwipeRefreshLayout.Refreshing = false;
                SwipeRefreshLayout.Enabled = false;

                StartApiService(); 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                SwipeRefreshLayout.Refreshing = false;
                SwipeRefreshLayout.Enabled = false;
            }
        }

        private void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
            else
            {
                switch (Type)
                {
                    case "StoryModel":
                        
                        break;
                    case "FollowersModel":
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadContactsAsync });
                        break;
                    case "GroupsModel":
                         
                        break;
                    case "PagesModel":
                         
                        break;
                    case "ImagesModel":
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> {LoadMyImage });
                        break;
                } 
            }
        }

        private void ShowEmptyPage()
        {
            try
            {
                MainScrollEvent.IsLoading = false;
                SwipeRefreshLayout.Refreshing = false;

                if (MAdapter?.ItemCount > 0)
                {
                    MRecycler.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    MRecycler.Visibility = ViewStates.Gone;

                    Inflated ??= EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();

                    switch (Type)
                    {
                        case "GroupsModel":
                        case "MangedGroupsModel":
                            x.InflateLayout(Inflated, EmptyStateInflater.Type.NoGroup);
                            break;
                        case "PagesModel":
                        case "MangedPagesModel":
                            x.InflateLayout(Inflated, EmptyStateInflater.Type.NoPage);
                            break;
                        case "StoryModel":
                            x.InflateLayout(Inflated, EmptyStateInflater.Type.NoGroup);
                            break;
                        case "FollowersModel":
                            x.InflateLayout(Inflated, EmptyStateInflater.Type.NoUsers);
                            break;
                        case "ImagesModel":
                            x.InflateLayout(Inflated, EmptyStateInflater.Type.NoAlbum);
                            break;
                    }

                    if (!x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click += null;
                    }
                    EmptyStateLayout.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception e)
            {
                MainScrollEvent.IsLoading = false;
                SwipeRefreshLayout.Refreshing = false;
                Console.WriteLine(e);
            }
        }

        #region Load Contacts 

        private async Task LoadContactsAsync()
        {
            if (MAdapter is ContactsAdapter adapter)
            {
                if (MainScrollEvent.IsLoading)
                    return;

                var lastIdUser = adapter.UserList.LastOrDefault()?.UserId ?? "0";
                if (Methods.CheckConnectivity())
                {
                    MainScrollEvent.IsLoading = true;

                    int countList = adapter.UserList.Count;
                    (int apiStatus, var respond) = await RequestsAsync.Global.GetFriendsAsync(PassedId, "following", "10", lastIdUser);
                    if (apiStatus != 200 || !(respond is GetFriendsObject result) || result.DataFriends == null)
                    {
                        MainScrollEvent.IsLoading = false;
                        Methods.DisplayReportResult(this, respond);
                    }
                    else
                    {
                        var respondList = result.DataFriends.Following.Count;
                        if (respondList > 0)
                        {
                            if (countList > 0)
                            {
                                foreach (var item in from item in result.DataFriends.Followers let check = adapter.UserList.FirstOrDefault(a => a.UserId == item.UserId) where check == null select item)
                                {
                                    adapter.UserList.Add(item);
                                }

                                RunOnUiThread(() => { adapter.NotifyItemRangeInserted(countList, adapter.UserList.Count - countList); });
                            }
                            else
                            {
                                adapter.UserList = new ObservableCollection<UserDataObject>(result.DataFriends.Following);
                                RunOnUiThread(() => { adapter.NotifyDataSetChanged(); });
                            }
                        }
                        else
                        {
                            if (adapter.UserList.Count > 10 && !MRecycler.CanScrollVertically(1))
                                Toast.MakeText(this, GetText(Resource.String.Lbl_No_more_users), ToastLength.Short).Show();
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
                    MainScrollEvent.IsLoading = false;
                }
                MainScrollEvent.IsLoading = false;
            }
        }
         
        #endregion

        #region Load Image 
          
        private async Task LoadMyImage()
        {
            if (MAdapter is UserPhotosAdapter adapter)
            {
                if (MainScrollEvent.IsLoading)
                    return;

                if (Methods.CheckConnectivity())
                {
                    MainScrollEvent.IsLoading = true;

                    var offset = adapter.UserPhotosList.LastOrDefault()?.Id ?? "0";
                    int countList = adapter.UserPhotosList.Count;
                    (int apiStatus, var respond) = await RequestsAsync.Album.GetPostByType(PassedId , "photos", "10", offset);
                    if (apiStatus != 200 || !(respond is GetImagesObject result) || result.Data == null)
                    {
                        Methods.DisplayReportResult(this, respond);
                    }
                    else
                    {
                        var respondList = result.Data?.Count;
                        if (respondList > 0)
                        {
                            result.Data.RemoveAll(w => string.IsNullOrEmpty(w.PostFileFull));

                            if (countList > 0)
                            {
                                foreach (var item in from item in result.Data let check = adapter.UserPhotosList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                {
                                    adapter.UserPhotosList.Add(item);
                                }

                                RunOnUiThread(() => { adapter.NotifyItemRangeInserted(countList, adapter.UserPhotosList.Count - countList); });
                            }
                            else
                            {
                                adapter.UserPhotosList = new ObservableCollection<PostDataObject>(result.Data);
                                RunOnUiThread(() => { adapter.NotifyDataSetChanged(); });
                            }
                        }
                        else
                        {
                            if (adapter.UserPhotosList.Count > 10 && !MRecycler.CanScrollVertically(1))
                                Toast.MakeText(this, GetText(Resource.String.Lbl_NoMorePhoto), ToastLength.Short).Show();
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
                    MainScrollEvent.IsLoading = false;
                }
                MainScrollEvent.IsLoading = false;
            }
        }

        #endregion
         
    }
}