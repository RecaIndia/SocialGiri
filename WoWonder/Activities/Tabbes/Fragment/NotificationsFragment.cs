using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Bumptech.Glide.Integration.RecyclerView;
using Bumptech.Glide.Util;
using Newtonsoft.Json;
using WoWonder.Activities.Communities.Groups;
using WoWonder.Activities.Communities.Pages;
using WoWonder.Activities.Events;
using WoWonder.Activities.Memories;
using WoWonder.Activities.NativePost.Pages;
using WoWonder.Activities.Story;
using WoWonder.Activities.Tabbes.Adapters;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Story;
using WoWonderClient.Requests;

namespace WoWonder.Activities.Tabbes.Fragment
{
    public class NotificationsFragment : Android.Support.V4.App.Fragment
    {
        #region Variables Basic

        private NotificationsAdapter MAdapter;
        private TabbedMainActivity GlobalContext;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private ViewStub EmptyStateLayout;
        private View Inflated;
        private RecyclerViewOnScrollListener MainScrollEvent;

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
            GlobalContext = (TabbedMainActivity)Activity ?? TabbedMainActivity.GetInstance();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            { 
                View view = inflater.Inflate(Resource.Layout.Tab_Notifications_Layout, container, false);

                InitComponent(view); 
                SetRecyclerViewAdapters();

                if (!AppSettings.SetTabOnButton)
                {
                    var parasms = (RelativeLayout.LayoutParams) SwipeRefreshLayout.LayoutParameters;
                    // Check if we're running on Android 5.0 or higher
                    parasms.TopMargin = (int) Build.VERSION.SdkInt < 23 ? 80 : 120;

                    MRecycler.LayoutParameters = parasms;
                    MRecycler.SetPadding(0, 0, 0, 0);
                }

                if (!Methods.CheckConnectivity())
                    Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                else
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadGeneralData(true) });

                return view;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
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

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                MRecycler = (RecyclerView)view.FindViewById(Resource.Id.recyler);
                EmptyStateLayout = view.FindViewById<ViewStub>(Resource.Id.viewStub);

                SwipeRefreshLayout = (SwipeRefreshLayout)view.FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));

                SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;

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
         
        private void SetRecyclerViewAdapters()
        {
            try
            {
                MAdapter = new NotificationsAdapter(Activity) { NotificationsList = new ObservableCollection<Notification>() };
                MAdapter.ItemClick += MAdapterOnItemClick;
                LayoutManager = new LinearLayoutManager(Activity);
                MRecycler.SetLayoutManager(LayoutManager); 
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<Notification>(Activity, MAdapter, sizeProvider, 10);
                MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter);

                MRecycler.NestedScrollingEnabled = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion
         
        #region Event

        //Open user profile
        private void MAdapterOnItemClick(object sender, NotificationsAdapterClickEventArgs e)
        {
            try
            {
                if (e.Position > -1)
                {
                    var item = MAdapter.GetItem(e.Position);
                    if (item != null)
                    {
                        if (item.Type == "following" || item.Type == "visited_profile" ||item.Type == "accepted_request")
                        {
                            WoWonderTools.OpenProfile(Activity, item.Notifier.UserId, item.Notifier);
                        }
                        else if (item.Type == "liked_page" || item.Type == "invited_page" ||item.Type == "accepted_invite")
                        { 
                            var intent = new Intent(Context, typeof(PageProfileActivity));
                            //intent.PutExtra("PageObject", JsonConvert.SerializeObject(item));
                            intent.PutExtra("PageId", item.PageId);
                            Context.StartActivity(intent);
                        }
                        else if (item.Type == "joined_group" || item.Type == "accepted_join_request" ||item.Type == "added_you_to_group")
                        {
                            var intent = new Intent(Context, typeof(GroupProfileActivity));
                            //intent.PutExtra("GroupObject", JsonConvert.SerializeObject(item));
                            intent.PutExtra("GroupId", item.GroupId);
                            Context.StartActivity(intent);
                        }
                        else if (item.Type == "comment" || item.Type == "wondered_post" ||
                                 item.Type == "wondered_comment" || item.Type == "reaction" ||
                                 item.Type == "wondered_reply_comment" || item.Type == "comment_mention" ||
                                 item.Type == "comment_reply_mention" ||
                                 item.Type == "liked_post" || item.Type == "liked_comment" ||
                                 item.Type == "liked_reply_comment" || item.Type == "post_mention" ||
                                 item.Type == "share_post" || item.Type == "shared_your_post" ||  item.Type == "comment_reply" ||
                                 item.Type == "also_replied" || item.Type == "profile_wall_post")
                        {
                            var intent = new Intent(Context, typeof(ViewFullPostActivity));
                            intent.PutExtra("Id", item.PostId); 
                            // intent.PutExtra("DataItem", JsonConvert.SerializeObject(item));
                            Context.StartActivity(intent);
                        }
                        else if (item.Type == "going_event")
                        { 
                            var intent = new Intent(Context, typeof(EventViewActivity));
                            intent.PutExtra("EventView", JsonConvert.SerializeObject(item.Event));
                            intent.PutExtra("Id", item.EventId); 
                            Context.StartActivity(intent); 
                        }
                        else if (item.Type == "viewed_story")
                        {
                            //"url": "https:\/\/demo.wowonder.com\/timeline&u=Matan&story=true&story_id=1946",
                            //var id = item.Url.Split("/").Last().Split("&story_id=").Last();

                            GetUserStoriesObject.StoryObject dataMyStory = GlobalContext?.NewsFeedTab?.PostFeedAdapter?.HolderStory?.StoryAdapter?.StoryList?.FirstOrDefault(o => o.UserId == UserDetails.UserId);
                            if (dataMyStory != null)
                            { 
                              Intent intent = new Intent(Context, typeof(ViewStoryActivity));
                              intent.PutExtra("UserId", dataMyStory.UserId); 
                              intent.PutExtra("DataItem", JsonConvert.SerializeObject(dataMyStory)); 
                              Context.StartActivity(intent);
                            } 
                        }
                        else if (item.Type == "requested_to_join_group")
                        {
                            var intent = new Intent(Context, typeof(JoinRequestActivity));
                            intent.PutExtra("GroupId", item.GroupId);
                            Context.StartActivity(intent);
                        }
                        else if (item.Type == "memory")
                        {
                            var intent = new Intent(Context, typeof(MemoriesActivity));
                            Context.StartActivity(intent);
                        }
                        else
                        {
                            WoWonderTools.OpenProfile(Activity,item.Notifier.UserId,item.Notifier);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Refresh
        private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                MAdapter.NotificationsList.Clear();
                MAdapter.NotifyDataSetChanged();

                if (MainScrollEvent != null) MainScrollEvent.IsLoading = false;

                MRecycler.Visibility = ViewStates.Visible;
                EmptyStateLayout.Visibility = ViewStates.Gone;

                if (!Methods.CheckConnectivity())
                    Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                else
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadGeneralData(true) });
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
                //Code get last id where LoadMore >>
                var item = MAdapter.NotificationsList.LastOrDefault();
                if (item != null && !string.IsNullOrEmpty(item.NotifierId) && !MainScrollEvent.IsLoading)
                {
                    if (!Methods.CheckConnectivity())
                        Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                    else
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadGeneralData(false, item.NotifierId) });
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
         
        #endregion

        #region Load Notification 

        //Get General Data Using Api >> notifications , pro_users , promoted_pages , trending_hashTag
        public async Task<(string, string, string, string)> LoadGeneralData(bool seenNotifications, string offset = "")
        {
            try
            {
                if (MainScrollEvent.IsLoading)
                    return ("", "", "", "");

                if (Methods.CheckConnectivity())
                {
                    MainScrollEvent.IsLoading = true;

                    var countNotificationsList = MAdapter.NotificationsList?.Count ?? 0; 
                    var countPromotedPagesList = GlobalContext.ProPagesAdapter?.MProPagesList.Count ?? 0;

                    (int apiStatus, var respond) = await RequestsAsync.Global.Get_General_Data(seenNotifications, TabbedMainActivity.OnlineUsers, UserDetails.DeviceId, offset);
                    if (apiStatus == 200)
                    {
                        if (respond is GetGeneralDataObject result)
                        {
                           Activity.RunOnUiThread(() =>
                           {
                               try
                               {
                                   // Notifications
                                   var respondList = result.Notifications.Count;
                                   if (respondList > 0)
                                   {
                                       if (countNotificationsList > 0)
                                       {
                                           var listNew = result.Notifications?.Where(c => !MAdapter.NotificationsList.Select(fc => fc.Id).Contains(c.Id)).ToList();
                                           if (listNew.Count > 0)
                                           { 
                                               foreach (var notification in listNew)
                                               {
                                                   MAdapter.NotificationsList.Insert(0 , notification);
                                               }

                                               MAdapter.NotifyItemRangeInserted(countNotificationsList - 1, MAdapter.NotificationsList.Count - countNotificationsList);
                                           }
                                       }
                                       else
                                       {
                                           MAdapter.NotificationsList = new ObservableCollection<Notification>(result.Notifications);
                                           MAdapter.NotifyDataSetChanged();
                                       }
                                   }
                                   else
                                   {
                                       if (MAdapter.NotificationsList.Count > 10 && !MRecycler.CanScrollVertically(1))
                                           Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreNotifications), ToastLength.Short).Show();
                                   }

                                   if (AppSettings.ShowTrendingPage &&GlobalContext.TrendingTab != null)
                                   {
                                       // Friend Requests
                                       if (result.FriendRequests.Count > 0)
                                       {
                                           GlobalContext.FriendRequestsList = new ObservableCollection<UserDataObject>(result.FriendRequests);

                                           GlobalContext.TrendingTab.LayoutFriendRequest.Visibility = ViewStates.Visible;
                                           try
                                           {
                                               for (var i = 0; i < 4; i++)
                                                   switch (i)
                                                   {
                                                       case 0:
                                                           GlideImageLoader.LoadImage(Activity, GlobalContext.FriendRequestsList[i].Avatar, GlobalContext.TrendingTab.FriendRequestImage3, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                                                           break;
                                                       case 1:
                                                           GlideImageLoader.LoadImage(Activity, GlobalContext.FriendRequestsList[i].Avatar, GlobalContext.TrendingTab.FriendRequestImage2, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                                                           break;
                                                       case 2:
                                                           GlideImageLoader.LoadImage(Activity, GlobalContext.FriendRequestsList[i].Avatar, GlobalContext.TrendingTab.FriendRequestImage1, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                                                           break;
                                                   }
                                           }
                                           catch (Exception e)
                                           {
                                               Console.WriteLine(e);
                                           }
                                       }
                                       else if (GlobalContext.TrendingTab != null)
                                       {
                                           GlobalContext.TrendingTab.LayoutFriendRequest.Visibility = ViewStates.Gone;
                                       }

                                       if (GlobalContext.TrendingTab != null && AppSettings.ShowProUsersMembers)
                                       {
                                           var isPro = ListUtils.MyProfileList?.FirstOrDefault()?.IsPro ?? "0";
                                           if (isPro == "0" && ListUtils.SettingsSiteList?.Pro == "1" && AppSettings.ShowGoPro)
                                           {
                                               var dataOwner = GlobalContext.ProUsersAdapter.MProUsersList.FirstOrDefault(a => a.Type == "Your");
                                               if (dataOwner == null)
                                               {
                                                   GlobalContext.ProUsersAdapter.MProUsersList.Insert(0, new UserDataObject
                                                   {
                                                       Avatar = UserDetails.Avatar,
                                                       Type = "Your",
                                                       Username = Context.GetText(Resource.String.Lbl_AddMe),
                                                   });

                                                   GlobalContext.ProUsersAdapter.NotifyDataSetChanged();

                                                   if (GlobalContext.TrendingTab.LayoutSuggestionProUsers.Visibility != ViewStates.Visible)
                                                       GlobalContext.TrendingTab.LayoutSuggestionProUsers.Visibility = ViewStates.Visible;
                                               }
                                           }

                                           // Pro Users
                                           var countProUsersList = GlobalContext.ProUsersAdapter.MProUsersList.Count;
                                           var respondListProUsers = result.ProUsers.Count;
                                           if (respondListProUsers > 0)
                                           {
                                               foreach (var item in from item in result.ProUsers let check = GlobalContext.ProUsersAdapter.MProUsersList.FirstOrDefault(a => a.UserId == item.UserId) where check == null select item)
                                               {
                                                   GlobalContext.ProUsersAdapter.MProUsersList.Add(item);
                                               }

                                               if (countProUsersList > 0)
                                               {
                                                   GlobalContext.ProUsersAdapter.NotifyItemRangeInserted(countProUsersList - 1, GlobalContext.ProUsersAdapter.MProUsersList.Count - countProUsersList);
                                               }
                                               else
                                               {
                                                   GlobalContext.ProUsersAdapter.NotifyDataSetChanged();
                                               }

                                               //Scroll Down >> 
                                               GlobalContext.TrendingTab.ProUserRecyclerView.ScrollToPosition(0);

                                               if (GlobalContext.ProUsersAdapter.MProUsersList.Count > 0 && GlobalContext.TrendingTab.LayoutSuggestionProUsers.Visibility != ViewStates.Visible)
                                                   GlobalContext.TrendingTab.LayoutSuggestionProUsers.Visibility = ViewStates.Visible;
                                           }
                                           else
                                           {
                                               if (GlobalContext.ProUsersAdapter.MProUsersList.Count == 0 && GlobalContext.TrendingTab.LayoutSuggestionProUsers.Visibility != ViewStates.Gone)
                                                   GlobalContext.TrendingTab.LayoutSuggestionProUsers.Visibility = ViewStates.Gone;
                                           }
                                       }
                                       else
                                       {
                                           if (GlobalContext.TrendingTab != null && GlobalContext.ProUsersAdapter.MProUsersList.Count == 0 && GlobalContext.TrendingTab.LayoutSuggestionProUsers.Visibility != ViewStates.Gone)
                                               GlobalContext.TrendingTab.LayoutSuggestionProUsers.Visibility = ViewStates.Gone;
                                       }

                                       if (GlobalContext.TrendingTab != null && AppSettings.ShowPromotedPages)
                                       {
                                           // Pro Pages
                                           var respondListPromotedPages = result.PromotedPages.Count;
                                           if (respondListPromotedPages > 0)
                                           {
                                               if (countPromotedPagesList > 0)
                                               {
                                                   foreach (var item in from item in result.PromotedPages let check = GlobalContext.ProPagesAdapter.MProPagesList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                                   {
                                                       GlobalContext.ProPagesAdapter.MProPagesList.Add(item);
                                                   }

                                                   GlobalContext.ProPagesAdapter.NotifyItemRangeInserted(countPromotedPagesList - 1, GlobalContext.ProPagesAdapter.MProPagesList.Count - countPromotedPagesList);
                                               }
                                               else
                                               {
                                                   GlobalContext.ProPagesAdapter.MProPagesList = new ObservableCollection<PageClass>(result.PromotedPages);
                                                   GlobalContext.ProPagesAdapter.NotifyDataSetChanged();
                                               }

                                               GlobalContext.TrendingTab.LayoutSuggestionPromotedPage.Visibility = ViewStates.Visible;
                                           }
                                           else
                                           {
                                               GlobalContext.TrendingTab.LayoutSuggestionPromotedPage.Visibility = ViewStates.Gone;
                                           }
                                       }
                                       else
                                       {
                                           GlobalContext.TrendingTab.LayoutSuggestionPromotedPage.Visibility = ViewStates.Gone;
                                       }

                                   }

                                   if (AppSettings.ShowTrendingHashTags)
                                       if (result.TrendingHashtag.Count > 0)
                                           GlobalContext.HashTagUserAdapter.MHashtagList = new ObservableCollection<TrendingHashtag>(result.TrendingHashtag);

                                   MainScrollEvent.IsLoading = false;
                                   ShowEmptyPage();
                               }
                               catch (Exception e)
                               {
                                   Console.WriteLine(e);
                               }
                           }); 
                            return (result.NewNotificationsCount, result.NewFriendRequestsCount, result.CountNewMessages, result.Announcement?.AnnouncementClass?.TextDecode);
                        }
                    }
                    else Methods.DisplayReportResult(Activity, respond);

                    Activity.RunOnUiThread(ShowEmptyPage);
                    MainScrollEvent.IsLoading = false;

                    return ("", "", "", "");
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

                    Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                }
            }
            catch (Exception e)
            {
                MainScrollEvent.IsLoading = false;
                Console.WriteLine(e); 
            }
            MainScrollEvent.IsLoading = false;
            return ("", "", "", "");
        }

        private void ShowEmptyPage()
        {
            try
            {
                MainScrollEvent.IsLoading = false;
                SwipeRefreshLayout.Refreshing = false;

                if (MAdapter.NotificationsList.Count > 0)
                {
                    MRecycler.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    MRecycler.Visibility = ViewStates.Gone;

                    Inflated ??= EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoNotifications);
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

        //No Internet Connection 
        private void EmptyStateButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                    Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                else
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadGeneralData(true) });
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

    }
}