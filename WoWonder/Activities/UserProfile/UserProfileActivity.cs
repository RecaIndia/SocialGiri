﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AFollestad.MaterialDialogs;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using AT.Markushi.UI;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Com.Tuyenmonkey.Textdecorator;
using Java.Lang;
using Newtonsoft.Json;
using Plugin.Share;
using Plugin.Share.Abstractions;
using Refractored.Controls;
using WoWonder.Activities.Contacts;
using WoWonder.Activities.Gift;
using WoWonder.Activities.NativePost.Extra;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Activities.UsersPages;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.SQLite;
using WoWonderClient.Classes.Album;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Classes.Product;
using WoWonderClient.Classes.User;
using WoWonderClient.Requests;
using Console = System.Console;
using Exception = System.Exception;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace WoWonder.Activities.UserProfile
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class UserProfileActivity : AppCompatActivity, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region Variables Basic

        private CollapsingToolbarLayout CollapsingToolbar;
        private AppBarLayout AppBarLayout;
        private WRecyclerView MainRecyclerView;
        private NativePostAdapter PostFeedAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private CircleButton BtnAddUser, BtnMessage, BtnMore;
        private TextView TxtUsername, TxtFollowers, TxtFollowing, TxtLikes, TxtPoints;
        private TextView TxtCountFollowers, TxtCountLikes, TxtCountFollowing, TxtCountPoints;
        private ImageView UserProfileImage, CoverImage, IconBack;
        private CircleImageView OnlineView;
        private UserDataObject UserData;
        private LinearLayout LayoutCountFollowers, LayoutCountFollowing, LayoutCountLikes, CountPointsLayout, HeaderSection;
        private string SPrivacyBirth, SPrivacyFollow, SPrivacyFriend, SPrivacyMessage;
        public static string SUserId;
        private string SUrlUser, SCanFollow, SUserName;
        private bool IsPoked, IsFamily;
        private View ViewPoints, ViewLikes, ViewFollowers;
        private FeedCombiner Combiner;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);

                View mContentView = Window.DecorView;
                var uiOptions = (int)mContentView.SystemUiVisibility;
                var newUiOptions = uiOptions;

                // newUiOptions |= (int)SystemUiFlags.Fullscreen;
                newUiOptions |= (int)SystemUiFlags.LayoutStable;
                mContentView.SystemUiVisibility = (StatusBarVisibility)newUiOptions;

                //Window.AddFlags(WindowManagerFlags.Fullscreen);

                Methods.App.FullScreenApp(this);

                // Create your application here
                SetContentView(Resource.Layout.Native_UserProfile);

                SUserName = Intent.GetStringExtra("name") ?? string.Empty;

                SUserId = Intent.GetStringExtra("UserId") ?? string.Empty;
                var userObject = Intent.GetStringExtra("UserObject");
                if (!string.IsNullOrEmpty(userObject))
                    UserData = JsonConvert.DeserializeObject<UserDataObject>(userObject);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();

                if (!string.IsNullOrEmpty(SUserName))
                {
                    GetDataUserByName();
                }
                else
                {
                    if (UserData != null)
                        LoadPassedDate(UserData);

                    StartApiService();
                }
                GetGiftsLists();

                AdsGoogle.Ad_Interstitial(this);
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
                MainRecyclerView.ReleasePlayer();
                SUserId = "";
                DestroyBasic();
                base.OnDestroy();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
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
                CollapsingToolbar = (CollapsingToolbarLayout)FindViewById(Resource.Id.collapsingToolbar);
                CollapsingToolbar.Title = "";

                AppBarLayout = FindViewById<AppBarLayout>(Resource.Id.appBarLayout);
                AppBarLayout.SetExpanded(true);

                BtnAddUser = (CircleButton)FindViewById(Resource.Id.AddUserbutton);
                BtnMessage = (CircleButton)FindViewById(Resource.Id.message_button);
                BtnMore = (CircleButton)FindViewById(Resource.Id.morebutton);

                IconBack = (ImageView)FindViewById(Resource.Id.back);
                TxtUsername = (TextView)FindViewById(Resource.Id.username_profile);
                TxtCountFollowers = (TextView)FindViewById(Resource.Id.CountFollowers);
                TxtCountFollowing = (TextView)FindViewById(Resource.Id.CountFollowing);
                TxtCountLikes = (TextView)FindViewById(Resource.Id.CountLikes);
                TxtCountPoints = (TextView)FindViewById(Resource.Id.CountPoints);
                TxtFollowers = FindViewById<TextView>(Resource.Id.txtFollowers);
                TxtFollowing = FindViewById<TextView>(Resource.Id.txtFollowing);
                TxtLikes = FindViewById<TextView>(Resource.Id.txtLikes);
                TxtPoints = FindViewById<TextView>(Resource.Id.txtPoints);
                UserProfileImage = (ImageView)FindViewById(Resource.Id.profileimage_head);
                CoverImage = (ImageView)FindViewById(Resource.Id.cover_image);
                OnlineView = FindViewById<CircleImageView>(Resource.Id.online_view);
                MainRecyclerView = FindViewById<WRecyclerView>(Resource.Id.newsfeedRecyler);
                HeaderSection = FindViewById<LinearLayout>(Resource.Id.headerSection);
                LayoutCountFollowers = FindViewById<LinearLayout>(Resource.Id.CountFollowersLayout);
                LayoutCountFollowing = FindViewById<LinearLayout>(Resource.Id.CountFollowingLayout);
                LayoutCountLikes = FindViewById<LinearLayout>(Resource.Id.CountLikesLayout);
                CountPointsLayout = FindViewById<LinearLayout>(Resource.Id.CountPointsLayout);

                ViewPoints = FindViewById<View>(Resource.Id.ViewPoints);
                ViewLikes = FindViewById<View>(Resource.Id.ViewLikes);
                ViewFollowers = FindViewById<View>(Resource.Id.ViewFollowers);

                SwipeRefreshLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = false;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));


                if (AppSettings.FlowDirectionRightToLeft)
                    IconBack.SetImageResource(Resource.Drawable.ic_action_ic_back_rtl);

                if (AppSettings.ConnectivitySystem == 1) // Following
                {
                    TxtFollowers.Text = GetText(Resource.String.Lbl_Followers);
                    TxtFollowing.Text = GetText(Resource.String.Lbl_Following);
                }
                else // Friend
                {
                    TxtFollowers.Text = GetText(Resource.String.Lbl_Friends);
                    TxtFollowing.Text = GetText(Resource.String.Lbl_Post);
                }

                BtnAddUser.Visibility = ViewStates.Invisible;
                BtnMore.Visibility = ViewStates.Visible;
                BtnMessage.Visibility = ViewStates.Invisible;

                if (!AppSettings.ShowUserPoint)
                {
                    ViewPoints.Visibility = ViewStates.Gone;
                    CountPointsLayout.Visibility = ViewStates.Gone;

                    HeaderSection.WeightSum = 3;
                }

                OnlineView.Visibility = ViewStates.Gone;
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
                    toolbar.Title = " ";
                    toolbar.SetTitleTextColor(Color.Black);
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
                PostFeedAdapter = new NativePostAdapter(this, SUserId, MainRecyclerView, NativeFeedType.User, SupportFragmentManager);
                MainRecyclerView.SetXAdapter(PostFeedAdapter, SwipeRefreshLayout);
                Combiner = new FeedCombiner(null, PostFeedAdapter.ListDiffer, this);

                PostFeedAdapter.SetLoading();
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
                    BtnAddUser.Click += BtnAddUserOnClick;
                    BtnMessage.Click += BtnMessageOnClick;
                    BtnMore.Click += BtnMoreOnClick;
                    IconBack.Click += IconBackOnClick;
                    LayoutCountFollowers.Click += LayoutCountFollowersOnClick;
                    LayoutCountFollowing.Click += LayoutCountFollowingOnClick;
                    LayoutCountLikes.Click += LayoutCountLikesOnClick;
                }
                else
                {
                    BtnAddUser.Click -= BtnAddUserOnClick;
                    BtnMessage.Click -= BtnMessageOnClick;
                    BtnMore.Click -= BtnMoreOnClick;
                    IconBack.Click -= IconBackOnClick;
                    LayoutCountFollowers.Click -= LayoutCountFollowersOnClick;
                    LayoutCountFollowing.Click -= LayoutCountFollowingOnClick;
                    LayoutCountLikes.Click -= LayoutCountLikesOnClick;
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
                SwipeRefreshLayout = null;
                CollapsingToolbar = null;
                AppBarLayout = null;
                BtnAddUser = null;
                BtnMessage = null;
                BtnMore = null;
                IconBack = null;
                TxtUsername = null;
                TxtCountFollowers = null;
                TxtCountFollowing = null;
                TxtCountLikes = null;
                TxtCountPoints = null;
                TxtFollowers = null;
                TxtFollowing = null;
                TxtLikes = null;
                TxtPoints = null;
                UserProfileImage = null;
                CoverImage = null;
                OnlineView = null;
                PostFeedAdapter = null;
                MainRecyclerView = null;
                HeaderSection = null;
                LayoutCountFollowers = null;
                LayoutCountFollowing = null;
                LayoutCountLikes = null;
                CountPointsLayout = null;
                ViewPoints = null;
                ViewLikes = null;
                ViewFollowers = null;
                Combiner = null;
                SPrivacyBirth = null;
                SPrivacyFollow = null;
                SPrivacyFriend = null;
                SPrivacyMessage = null;
                SUrlUser = null;
                SCanFollow = null;
                SUserName = null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Get Profile

        private void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
            else
                PollyController.RunRetryPolicyFunction( new List<Func<Task>> { GetProfileApi , GetAlbumUserApi });
        }

        private async Task GetProfileApi()
        {
            var (apiStatus, respond) = await RequestsAsync.Global.Get_User_Data(SUserId); 
            if (apiStatus != 200 || !(respond is GetUserDataObject result) || result.UserData == null)
            {
                Methods.DisplayReportResult(this, respond);
            }
            else
            { 
                LoadPassedDate(result.UserData);

                if (result.Family.Count > 0)
                {
                    var data = result.Family.FirstOrDefault(o => o.UserData.UserId == UserDetails.UserId);
                    IsFamily = data != null;
                }
                else
                {
                    IsFamily = false;
                }

                if (result.UserData.UserId != UserDetails.UserId)
                {
                    SCanFollow = result.UserData.CanFollow;

                    if (SCanFollow == "0" && result.UserData.IsFollowing == "0")
                        BtnAddUser.Visibility = ViewStates.Gone;

                    SetProfilePrivacy(result.UserData);
                }
                else
                {
                    BtnAddUser.Visibility = ViewStates.Gone;
                }

                if (result.Followers.Count > 0)
                {
                    //var ListDataUserFollowers = new ObservableCollection<UserDataObject>(result.Followers);
                }

                if (result.LikedPages.Count > 0)
                    RunOnUiThread(() => { LoadPagesLayout(result.LikedPages); });

                if (result.JoinedGroups.Count > 0)
                    RunOnUiThread(() => { if (result.UserData.Details.DetailsClass != null) LoadGroupsLayout(result.JoinedGroups, Methods.FunString.FormatPriceValue(int.Parse(result.UserData.Details.DetailsClass.GroupsCount))); });

                if (SPrivacyFriend == "0" || result.UserData?.IsFollowing == "1" && SPrivacyFriend == "1" || SPrivacyFriend == "2")
                    if (result.Following.Count > 0)
                        RunOnUiThread(() => { if (result.UserData.Details.DetailsClass != null) LoadFriendsLayout(result.Following, Methods.FunString.FormatPriceValue(int.Parse(result.UserData.Details.DetailsClass.FollowingCount))); });

                string postPrivacy;
                if (result.UserData.PostPrivacy.Contains("everyone"))
                {
                    postPrivacy = "1";
                }
                else if (result.UserData.PostPrivacy.Contains("ifollow") && result.UserData?.IsFollowing == "1" && result.UserData?.IsFollowingMe == "1")
                {
                    postPrivacy = "1";
                }
                else if (result.UserData.PostPrivacy.Contains("me")) //Lbl_People_Follow_Me
                {
                    postPrivacy = "1";
                }
                else // Lbl_No_body
                {
                    postPrivacy = "0";
                }

                if (postPrivacy == "1")
                {
                    //##Set the AddBox place on Main RecyclerView
                    //------------------------------------------------------------------------
                    var check7 = PostFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.SocialLinks);
                    var check = PostFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.PagesBox);
                    var check2 = PostFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.GroupsBox);
                    var check3 = PostFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.FollowersBox);
                    var check4 = PostFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.ImagesBox);
                    var check5 = PostFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.AboutBox);
                    var check6 = PostFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.EmptyState);

                    if (check7 != null)
                    {
                        Combiner.AddPostBoxPostView("user", PostFeedAdapter.ListDiffer.IndexOf(check7) + 1);
                    }
                    else if (check != null)
                    {
                        Combiner.AddPostBoxPostView("user", PostFeedAdapter.ListDiffer.IndexOf(check) + 1);
                    }
                    else if (check2 != null)
                    {
                        Combiner.AddPostBoxPostView("user", PostFeedAdapter.ListDiffer.IndexOf(check2) + 1);
                    }
                    else if (check3 != null)
                    {
                        Combiner.AddPostBoxPostView("user", PostFeedAdapter.ListDiffer.IndexOf(check3) + 1);
                    }
                    else if (check4 != null)
                    {
                        Combiner.AddPostBoxPostView("user", PostFeedAdapter.ListDiffer.IndexOf(check4) + 1);
                    }
                    else if (check5 != null)
                    {
                        Combiner.AddPostBoxPostView("user", PostFeedAdapter.ListDiffer.IndexOf(check5) + 1);
                    }
                    else if (check6 != null)
                    {
                        Combiner.AddPostBoxPostView("user", PostFeedAdapter.ListDiffer.IndexOf(check6) + 1);
                    } 
                }
                   
                if (AppSettings.ShowSearchForPosts)
                    Combiner.SearchForPostsView("user");

                RunOnUiThread(() => { PostFeedAdapter.NotifyDataSetChanged(); });
                //------------------------------------------------------------------------ 

                PollyController.RunRetryPolicyFunction(new List<Func<Task>> {() => MainRecyclerView.FetchNewsFeedApiPosts() });
            }
        }

        private async Task GetAlbumUserApi()
        {
            var (apiStatus, respond) = await RequestsAsync.Album.GetPostByType(SUserId , "photos");
            if (apiStatus != 200 || !(respond is GetImagesObject result) || result.Data == null)
            {
                Methods.DisplayReportResult(this, respond);
            }
            else
            {
                if (result.Data.Count > 0)
                {
                    result.Data.RemoveAll(w => string.IsNullOrEmpty(w.PostFileFull));
                     
                    var count = result.Data.Count;
                    if (count > 10)
                        RunOnUiThread(() => { LoadImagesLayout(result.Data.Take(9).ToList(), Methods.FunString.FormatPriceValue(int.Parse(count.ToString()))); });
                    else if (count > 5)
                        RunOnUiThread(() => { LoadImagesLayout(result.Data.Take(6).ToList(), Methods.FunString.FormatPriceValue(int.Parse(count.ToString()))); });
                    else if (count > 0)
                        RunOnUiThread(() => { LoadImagesLayout(result.Data.ToList(), Methods.FunString.FormatPriceValue(int.Parse(count.ToString()))); });
                }
            }
        }

        private void LoadPassedDate(UserDataObject result)
        {
            try
            {
                UserData = result;

                GlideImageLoader.LoadImage(this, result.Avatar, UserProfileImage, ImageStyle.CircleCrop, ImagePlaceholders.Color);
                //GlideImageLoader.LoadImage(this, result.Cover, CoverImage, ImageStyle.FitCenter, ImagePlaceholders.Color, false);
                Glide.With(this).Load(result.Cover).Apply(new RequestOptions().Placeholder(Resource.Drawable.Cover_image).Error(Resource.Drawable.Cover_image)).Into(CoverImage);

                //TxtUsername.Text = result.Name;
                SUrlUser = result.Url;

                var font = Typeface.CreateFromAsset(Application.Context.Resources.Assets, "ionicons.ttf");
                TxtUsername.SetTypeface(font, TypefaceStyle.Normal);

                var textHighLighter = result.Name;
                var textIsPro = string.Empty;
                 
                if (result.Verified == "1")
                    textHighLighter += " " + IonIconsFonts.CheckmarkCircled;

                if (result.IsPro == "1")
                {
                    textIsPro = " " + IonIconsFonts.Flash;
                    textHighLighter += textIsPro;
                }

                var decorator = TextDecorator.Decorate(TxtUsername, textHighLighter);

                if (result.Verified == "1")
                    decorator.SetTextColor(Resource.Color.Post_IsVerified, IonIconsFonts.CheckmarkCircled);
                 
                decorator.SetTextColor(Resource.Color.white, textIsPro);

                decorator.Build();

                var online = WoWonderTools.GetStatusOnline(int.Parse(result.LastseenUnixTime), result.LastseenStatus);
                if (online)
                    OnlineView.Visibility = ViewStates.Visible;

                if (result.UserId == UserDetails.UserId)
                    BtnAddUser.Visibility = ViewStates.Gone;

                var checkAboutBox = PostFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.AboutBox);
                if (checkAboutBox == null)
                {
                    Combiner.AboutBoxPostView(WoWonderTools.GetAboutFinal(result), 0); 
                }
                else
                {
                    checkAboutBox.AboutModel.Description = WoWonderTools.GetAboutFinal(result);
                    //PostFeedAdapter.NotifyItemChanged(0);
                }

                var checkSocialBox = PostFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.SocialLinks);
                if (checkSocialBox == null)
                {
                    var socialLinksModel = new SocialLinksModelClass()
                    {
                        Facebook = result.Facebook,
                        Instegram = result.Instagram,
                        Twitter = result.Twitter,
                        Google = result.Google,
                        Vk = result.Vk,
                        Youtube = result.Youtube,
                    };

                    Combiner.SocialLinksBoxPostView(socialLinksModel, -1);
                    //PostFeedAdapter.NotifyItemInserted(0);
                }
                else
                {
                    checkSocialBox.SocialLinksModel.Facebook = result.Facebook;
                    checkSocialBox.SocialLinksModel.Instegram = result.Instagram;
                    checkSocialBox.SocialLinksModel.Twitter = result.Twitter;
                    checkSocialBox.SocialLinksModel.Google = result.Google;
                    checkSocialBox.SocialLinksModel.Vk = result.Vk;
                    checkSocialBox.SocialLinksModel.Youtube = result.Youtube;
                }
                 
                //Set Privacy User
                //==================================
                SPrivacyBirth = result.BirthPrivacy;
                SPrivacyFollow = result.FollowPrivacy;
                SPrivacyFriend = result.FriendPrivacy;
                SPrivacyMessage = result.MessagePrivacy;
                if (result.Details.DetailsClass != null) TxtCountLikes.Text = Methods.FunString.FormatPriceValue(int.Parse(result.Details.DetailsClass.LikesCount));

                SetProfilePrivacy(result);

                if (AppSettings.ShowUserPoint)
                    TxtCountPoints.Text = Methods.FunString.FormatPriceValue(int.Parse(result.Points));

                if (result.Details.DetailsClass != null)
                {
                    // Following
                    if (AppSettings.ConnectivitySystem == 1)
                    {
                        TxtFollowers.Text = GetText(Resource.String.Lbl_Followers);
                        TxtFollowing.Text = GetText(Resource.String.Lbl_Following);

                        if (result.Details.DetailsClass != null) TxtCountFollowers.Text = Methods.FunString.FormatPriceValue(int.Parse(result.Details.DetailsClass.FollowersCount));
                        if (result.Details.DetailsClass != null) TxtCountFollowing.Text = Methods.FunString.FormatPriceValue(int.Parse(result.Details.DetailsClass.FollowingCount));

                        LayoutCountFollowing.Tag = "Following";
                    }
                    else // Friend
                    {
                        TxtFollowers.Text = GetText(Resource.String.Lbl_Friends);
                        TxtFollowing.Text = GetText(Resource.String.Lbl_Post);

                        TxtCountFollowers.Text = Methods.FunString.FormatPriceValue(int.Parse(result.Details.DetailsClass.FollowersCount));
                        TxtCountFollowing.Text = Methods.FunString.FormatPriceValue(int.Parse(result.Details.DetailsClass.PostCount));

                        LayoutCountFollowing.Tag = "Post";
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }

        private void SetAddFriendCondition()
        {
            try
            {
                if (BtnAddUser.Tag?.ToString() == "Add") //(is_following == "0") >> Not Friend
                {
                    BtnAddUser.SetColor(Color.ParseColor("#efefef"));
                    BtnAddUser.SetImageResource(Resource.Drawable.ic_tick);
                    BtnAddUser.Drawable.SetTint(Color.ParseColor("#444444"));
                    BtnAddUser.Tag = "friends";
                }
                else if (BtnAddUser.Tag?.ToString() == "request") //(is_following == "2") >> Request
                {
                    BtnAddUser.SetColor(Color.ParseColor("#efefef"));
                    BtnAddUser.SetImageResource(Resource.Drawable.ic_tick);
                    BtnAddUser.Drawable.SetTint(Color.ParseColor("#444444"));
                    BtnAddUser.Tag = "Add";
                }
                else //(is_following == "1") >> Friend
                {
                    BtnAddUser.SetColor(Color.ParseColor("#6666ff"));
                    BtnAddUser.SetImageResource(Resource.Drawable.ic_add);
                    BtnAddUser.Drawable.SetTint(Color.ParseColor("#ffffff"));
                    BtnAddUser.Tag = "Add";
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void SetProfilePrivacy(UserDataObject result)
        {
            try
            {
                if (result.IsFollowing == "0")
                    BtnAddUser.Tag = "friends";
                else if (result.IsFollowing == "1")
                    BtnAddUser.Tag = "Add";

                SetAddFriendCondition();

                switch (SPrivacyFollow)
                {
                    // Everyone
                    case "0":
                        BtnAddUser.Visibility = ViewStates.Visible;
                        break;
                    // People i Follow
                    case "1":
                        if (result.IsFollowingMe == "0" && result.IsFollowing == "0")
                            BtnAddUser.Visibility = ViewStates.Gone;
                        else
                            BtnAddUser.Visibility = result.IsFollowingMe == "0" ? ViewStates.Visible : ViewStates.Gone;
                        break;
                    //Nobody
                    default:
                        BtnAddUser.Visibility = ViewStates.Gone;
                        break;
                }

                if (AppSettings.MessengerIntegration)
                {
                    switch (SPrivacyMessage)
                    {
                        // Everyone
                        case "0":
                            BtnMessage.Visibility = ViewStates.Visible;
                            break;
                        // People i Follow
                        case "1":
                            if (result.IsFollowingMe == "0" && result.IsFollowing == "0")
                                BtnMessage.Visibility = ViewStates.Gone;
                            else
                                BtnMessage.Visibility = result.IsFollowingMe == "0" ? ViewStates.Visible : ViewStates.Gone;
                            break;
                        //Nobody
                        default:
                            BtnMessage.Visibility = ViewStates.Gone;
                            break;
                    }
                }
                else
                {
                    BtnMessage.Visibility = ViewStates.Gone;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void LoadFriendsLayout(List<UserDataObject> followers, string friendsCounter)
        {
            try
            {
                var followersClass = new FollowersModelClass
                {
                    TitleHead = GetText(AppSettings.ConnectivitySystem == 1 ? Resource.String.Lbl_Following : Resource.String.Lbl_Friends), FollowersList = new List<UserDataObject>(followers.Take(12)),
                    More = friendsCounter
                };
                 
                var check = PostFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.FollowersBox);
                if (check != null)
                {
                    check.FollowersModel = followersClass;
                    //PostFeedAdapter.NotifyItemInserted(PostFeedAdapter.ListDiffer.IndexOf(check) + 1);
                }
                else
                {
                    Combiner.FollowersBoxPostView(followersClass, 2);
                    //PostFeedAdapter.NotifyItemInserted(1);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void LoadImagesLayout(List<PostDataObject> images, string imagesCounter)
        {
            try
            {
                var imagesClass = new ImagesModelClass
                {
                    TitleHead = GetText(Resource.String.Lbl_Profile_Picture),
                    ImagesList = images,
                    More = images.Count.ToString()
                };
                
                var check = PostFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.ImagesBox);
                if (check != null)
                {
                    check.ImagesModel = imagesClass;
                    //PostFeedAdapter.NotifyItemInserted(PostFeedAdapter.ListDiffer.IndexOf(check) + 1);
                }
                else
                {
                    Combiner.ImagesBoxPostView(imagesClass, 2); 
                    //PostFeedAdapter.NotifyItemInserted(1);
                }

                RunOnUiThread(() => { PostFeedAdapter.NotifyDataSetChanged(); });
                //------------------------------------------------------------------------ 
                Console.WriteLine(imagesCounter);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void LoadPagesLayout(List<PageClass> pages)
        {
            try
            {
                var checkNull = pages.Where(a => a.PageId == null).ToList();
                if (checkNull.Count > 0)
                    foreach (var item in checkNull)
                        pages.Remove(item);

                var pagesClass = new PagesModelClass { PagesList = new List<PageClass>(pages) };
                 
                var check = PostFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.PagesBox);
                if (check != null)
                {
                    check.PagesModel = pagesClass;
                    //PostFeedAdapter.NotifyItemInserted(PostFeedAdapter.ListDiffer.IndexOf(check) + 1);
                }
                else
                {
                    Combiner.PagesBoxPostView(pagesClass, 2);
                    //PostFeedAdapter.NotifyItemInserted(1);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void LoadGroupsLayout(List<GroupClass> groups, string groupsCounter)
        {
            try
            {
                var groupsClass = new GroupsModelClass
                {
                    TitleHead = GetText(Resource.String.Lbl_Groups),
                    GroupsList = new List<GroupClass>(groups.Take(12)),
                    More = groupsCounter,
                    UserProfileId = SUserId
                };
                 
                var check = PostFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.GroupsBox);
                if (check != null)
                {
                    check.GroupsModel = groupsClass;
                    //PostFeedAdapter.NotifyItemInserted(PostFeedAdapter.ListDiffer.IndexOf(check) + 1);
                }
                else
                {
                    Combiner.GroupsBoxPostView(groupsClass, 2);
                    //PostFeedAdapter.NotifyItemInserted(1);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Event

        //Event Show More : Block User , Copy Link To Profile
        private void BtnMoreOnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                arrayAdapter.Add(GetText(Resource.String.Lbl_Block));
                arrayAdapter.Add(GetText(Resource.String.Lbl_CopeLink));
                arrayAdapter.Add(GetText(Resource.String.Lbl_Share));

                if (AppSettings.ShowPokes)
                    arrayAdapter.Add(IsPoked ? GetText(Resource.String.Lbl_Poked) : GetText(Resource.String.Lbl_Poke));

                if (!IsFamily)
                    arrayAdapter.Add(GetText(Resource.String.Lbl_AddToFamily));
                 
                if (AppSettings.ShowGift && ListUtils.GiftsList.Count > 0)
                    arrayAdapter.Add(GetText(Resource.String.Lbl_SentGift));

                dialogList.Title(Resource.String.Lbl_More);
                dialogList.Items(arrayAdapter);
                dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(this);
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Event Send Messages To User
        private void BtnMessageOnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                if (AppSettings.MessengerIntegration)
                {
                    var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                    dialog.Title(Resource.String.Lbl_Warning);
                    dialog.Content(GetText(Resource.String.Lbl_ContentAskOPenAppMessenger));
                    dialog.PositiveText(GetText(Resource.String.Lbl_Yes)).OnPositive((materialDialog, action) =>
                    {
                        try
                        {
                            Methods.App.OpenAppByPackageName(this, AppSettings.MessengerPackageName, "OpenChat", new ChatObject(){ UserId = SUserId , Name = UserData.Name , Avatar = UserData.Avatar});
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine(exception);
                        }
                    });
                    dialog.NegativeText(GetText(Resource.String.Lbl_No)).OnNegative(this);
                    dialog.AlwaysCallSingleChoiceCallback();
                    dialog.Build().Show();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Event Add Friends Or Follower User
        private async void BtnAddUserOnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                }
                else
                {
                    SetAddFriendCondition();

                    var (apiStatus, respond) = await RequestsAsync.Global.Follow_User(SUserId).ConfigureAwait(false); 
                    if (apiStatus != 200 || !(respond is FollowUserObject result) || result.FollowStatus == null)
                    {
                        Methods.DisplayReportResult(this, respond);
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Event Back
        private void IconBackOnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                Finish();
                OverridePendingTransition(Resource.Animation.slide_out_left, Resource.Animation.slide_out_left);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Event Show All Users Followers 
        private void LayoutCountFollowersOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(MyContactsActivity));
                intent.PutExtra("ContactsType", "Followers");
                intent.PutExtra("UserId", SUserId);
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Event Show All Page Likes
        private void LayoutCountLikesOnClick(object sender, EventArgs e)
        {
            try
            {
                var check = PostFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.PagesBox);
                if (check != null)
                {
                    var intent = new Intent(this, typeof(AllViewerActivity));
                    intent.PutExtra("Type", "PagesModel"); //StoryModel , FollowersModel , GroupsModel , PagesModel
                    intent.PutExtra("itemObject", JsonConvert.SerializeObject(check));
                    StartActivity(intent);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Event Show All Users Following  
        private void LayoutCountFollowingOnClick(object sender, EventArgs e)
        {
            try
            {
                if (LayoutCountFollowing.Tag.ToString() == "Following")
                {
                    var intent = new Intent(this, typeof(MyContactsActivity));
                    intent.PutExtra("ContactsType", "Following");
                    intent.PutExtra("UserId", SUserId);
                    StartActivity(intent);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion Event

        #region MaterialDialog

        public void OnSelection(MaterialDialog p0, View p1, int itemId, ICharSequence itemString)
        {
            try
            {
                string text = itemString.ToString();
                if (text == GetText(Resource.String.Lbl_Block))
                {
                    BlockUserButtonClick();
                }
                else if (text == GetText(Resource.String.Lbl_CopeLink))
                {
                    OnCopeLinkToProfile_Button_Click();
                }
                else if (text == GetText(Resource.String.Lbl_Share))
                {
                    OnShare_Button_Click();
                }
                else if (text == GetText(Resource.String.Lbl_Poke))
                {
                    if (Methods.CheckConnectivity())
                    {
                        IsPoked = true;
                        Toast.MakeText(this, GetString(Resource.String.Lbl_YouHavePoked), ToastLength.Short).Show();

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.CreatePoke(SUserId) });
                    }
                    else
                        Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                }
                else if (text == GetText(Resource.String.Lbl_Poked))
                {
                    Toast.MakeText(this, GetString(Resource.String.Lbl_YouSentPoked), ToastLength.Short).Show();
                }
                else if (text == GetText(Resource.String.Lbl_AddToFamily))
                {
                    if (ListUtils.FamilyList.Count > 0)
                    {
                        var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                        var arrayAdapter = ListUtils.FamilyList.Select(item => item.FamilyName.Replace("_", " ")).ToList();

                        dialogList.Title(GetText(Resource.String.Lbl_AddToFamily));
                        dialogList.Items(arrayAdapter);
                        dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(this);
                        dialogList.AlwaysCallSingleChoiceCallback();
                        dialogList.ItemsCallback(this).Build().Show();
                    }
                }
                else if (text == GetText(Resource.String.Lbl_SentGift))
                {
                    GiftButtonOnClick();
                }
                else
                {
                    var familyId = ListUtils.FamilyList.FirstOrDefault(a => a.FamilyName == itemString.ToString())?.FamilyId;
                    if (!string.IsNullOrEmpty(familyId))
                    {
                        IsFamily = true;
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Sent_successfully), ToastLength.Short).Show();
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.AddToFamilyAsync(SUserId, familyId) });
                    }
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

        //Event Block User
        private async void BlockUserButtonClick()
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short)
                        .Show();
                }
                else
                {
                    var (apiStatus, respond) = await RequestsAsync.Global.Block_User(SUserId, true).ConfigureAwait(true); //true >> "block"
                    if (apiStatus == 200)
                    {
                        var dbDatabase = new SqLiteDatabase();
                        dbDatabase.Delete_UsersContact(SUserId);
                        dbDatabase.Dispose();

                        Toast.MakeText(this, GetString(Resource.String.Lbl_Blocked_successfully), ToastLength.Short).Show();
                        Finish();
                    }
                    else Methods.DisplayReportResult(this, respond);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Event Menu >> Cope Link To Profile
        private void OnCopeLinkToProfile_Button_Click()
        {
            try
            {
                var clipboardManager = (ClipboardManager)GetSystemService(ClipboardService);

                var clipData = ClipData.NewPlainText("text", SUrlUser);
                clipboardManager.PrimaryClip = clipData;


                Toast.MakeText(this, GetText(Resource.String.Lbl_Copied), ToastLength.Short).Show();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Event Menu >> Share
        private async void OnShare_Button_Click()
        {
            try
            {
                //Share Plugin same as video
                if (!CrossShare.IsSupported) return;

                await CrossShare.Current.Share(new ShareMessage
                {
                    Title = UserDetails.Username,
                    Text = "",
                    Url = SUrlUser
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Sent Gift
        private void GiftButtonOnClick()
        {
            try
            {
                Bundle bundle = new Bundle();
                bundle.PutString("UserId", SUserId);

                GiftDialogFragment mGiftFragment = new GiftDialogFragment
                {
                    Arguments = bundle
                };

                mGiftFragment.Show(SupportFragmentManager, mGiftFragment.Tag);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        #region Result

        //Result
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);

                if (requestCode == 2500 && resultCode == Result.Ok) //add post
                {
                    if (!string.IsNullOrEmpty(data.GetStringExtra("itemObject")))
                    {
                        var postData = JsonConvert.DeserializeObject<PostDataObject>(data.GetStringExtra("itemObject"));
                        if (postData != null)
                        {
                            var countList = PostFeedAdapter.ItemCount;

                            var combine = new FeedCombiner(postData, PostFeedAdapter.ListDiffer, this);
                            combine.CombineDefaultPostSections("Top");

                            int countIndex = 1;
                            var model1 = PostFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.Story);
                            var model2 = PostFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.AddPostBox);
                            var model3 = PostFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.AlertBox);
                            var model4 = PostFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.SearchForPosts);

                            if (model4 != null)
                                countIndex += PostFeedAdapter.ListDiffer.IndexOf(model4) + 1;
                            else if (model3 != null)
                                countIndex += PostFeedAdapter.ListDiffer.IndexOf(model3) + 1;
                            else if (model2 != null)
                                countIndex += PostFeedAdapter.ListDiffer.IndexOf(model2) + 1;
                            else if (model1 != null)
                                countIndex += PostFeedAdapter.ListDiffer.IndexOf(model1) + 1;
                            else
                                countIndex = 0;

                            PostFeedAdapter.NotifyItemRangeInserted(countIndex, PostFeedAdapter.ListDiffer.Count - countList);
                        }
                    }
                    else
                    {
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => MainRecyclerView.FetchNewsFeedApiPosts() });
                    }
                }
                else if (requestCode == 3950 && resultCode == Result.Ok) //Edit post
                {
                    var postId = data.GetStringExtra("PostId") ?? "";
                    var postText = data.GetStringExtra("PostText") ?? "";
                    var diff = PostFeedAdapter.ListDiffer;
                    List<AdapterModelsClass> dataGlobal = diff.Where(a => a.PostData?.Id == postId).ToList();
                    if (dataGlobal.Count > 0)
                    {
                        foreach (var postData in dataGlobal)
                        {
                            postData.PostData.Orginaltext = postText;
                            var index = diff.IndexOf(postData);
                            if (index > -1)
                            {
                                PostFeedAdapter.NotifyItemChanged(index);
                            }
                        }

                        var checkTextSection = dataGlobal.FirstOrDefault(w => w.TypeView == PostModelType.TextSectionPostPart);
                        if (checkTextSection == null)
                        {
                            var collection = dataGlobal.FirstOrDefault()?.PostData;
                            var item = new AdapterModelsClass
                            {
                                TypeView = PostModelType.TextSectionPostPart,
                                Id = int.Parse((int)PostModelType.TextSectionPostPart + collection?.Id),
                                PostData = collection,
                                IsDefaultFeedPost = true
                            };

                            var headerPostIndex = diff.IndexOf(dataGlobal.FirstOrDefault(w => w.TypeView == PostModelType.HeaderPost));
                            if (headerPostIndex > -1)
                            {
                                diff.Insert(headerPostIndex + 1, item);
                                PostFeedAdapter.NotifyItemInserted(headerPostIndex + 1);
                            }
                        }
                    }
                }
                else if (requestCode == 3500 && resultCode == Result.Ok) //Edit post product 
                {
                    if (string.IsNullOrEmpty(data.GetStringExtra("itemData"))) return;
                    var item = JsonConvert.DeserializeObject<ProductDataObject>(data.GetStringExtra("itemData"));
                    if (item != null)
                    {
                        var diff = PostFeedAdapter.ListDiffer;
                        var dataGlobal = diff.Where(a => a.PostData?.Id == item.PostId).ToList();
                        if (dataGlobal.Count > 0)
                        {
                            foreach (var postData in dataGlobal)
                            {
                                var index = diff.IndexOf(postData);
                                if (index > -1)
                                {
                                    var productUnion = postData.PostData.Product?.ProductClass;
                                    if (productUnion != null) productUnion.Id = item.Id;
                                    productUnion = item;
                                    Console.WriteLine(productUnion);

                                    PostFeedAdapter.NotifyItemChanged(PostFeedAdapter.ListDiffer.IndexOf(postData));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        private void GetGiftsLists()
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                    return;
                }

                var sqlEntity = new SqLiteDatabase(); 
                var listGifts = sqlEntity.GetGiftsList();

                if (ListUtils.GiftsList.Count > 0 && listGifts != null)
                    ListUtils.GiftsList = listGifts;
                else
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { ApiRequest.GetGifts });

                sqlEntity.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private async void GetDataUserByName()
        {
            try
            {
                var dictionary = new Dictionary<string, string>
                {
                    {"user_id", UserDetails.UserId},
                    {"limit", "30"},
                    {"user_offset", "0"},
                    {"gender", UserDetails.SearchGender},
                    {"search_key", SUserName},
                };

                (int apiStatus, var respond) = await RequestsAsync.Global.Get_Search(dictionary);
                if (apiStatus == 200)
                {
                    if (respond is GetSearchObject result)
                    {
                        var respondUserList = result.Users?.Count;
                        if (respondUserList > 0)
                        {
                            UserData = result.Users.FirstOrDefault(a => a.Username == SUserName);
                            if (UserData != null)
                            {
                                SUserId = UserData.UserId;

                                SetRecyclerViewAdapters();

                                LoadPassedDate(UserData);

                                StartApiService();
                            }
                            else
                            {
                                Finish();
                                Toast.MakeText(this, GetText(Resource.String.Lbl_NotHaveThisUser), ToastLength.Short).Show();
                            }
                        }
                        else
                        {
                            Toast.MakeText(this, GetText(Resource.String.Lbl_No_more_users), ToastLength.Short).Show();
                        }
                    }
                }
                else Methods.DisplayReportResult(this, respond);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}