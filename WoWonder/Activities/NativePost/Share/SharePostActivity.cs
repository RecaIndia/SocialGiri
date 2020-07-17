using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using WoWonder.Activities.NativePost.Extra;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Activities.Tabbes;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Requests;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace WoWonder.Activities.NativePost.Share
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class SharePostActivity : AppCompatActivity
    { 
        #region Variables Basic

        private Toolbar TopToolBar;
        private ImageView PostSectionImage;
        private TextView TxtSharePost, TxtUserName;
        private EditText TxtContentPost; 
        private WRecyclerView MainRecyclerView;
        private NativePostAdapter PostFeedAdapter;
        private GroupClass GroupData;
        private PageClass PageData;
        private PostDataObject PostData;
        private string TypePost = "";

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
                SetContentView(Resource.Layout.SharePostLayout);

                var postdate = Intent.GetStringExtra("ShareToType") ?? "Data not available";
                if (postdate != "Data not available" && !string.IsNullOrEmpty(postdate)) TypePost = postdate; //Group , Page , MyTimeline
                 
                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();

                GetDataPost();
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
                TxtSharePost = FindViewById<TextView>(Resource.Id.toolbar_title);
                TxtContentPost = FindViewById<EditText>(Resource.Id.editTxtEmail); 
                PostSectionImage = FindViewById<ImageView>(Resource.Id.postsectionimage); 
                TxtUserName = FindViewById<TextView>(Resource.Id.card_name); 

                Methods.SetColorEditText(TxtContentPost, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                TxtContentPost.ClearFocus(); 
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
                TopToolBar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (TopToolBar != null)
                {
                    TopToolBar.Title = GetText(Resource.String.Lbl_SharePost);
                    TopToolBar.SetTitleTextColor(Color.White);
                    SetSupportActionBar(TopToolBar);
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
                MainRecyclerView = FindViewById<WRecyclerView>(Resource.Id.Recyler);
                PostFeedAdapter = new NativePostAdapter(this,"", MainRecyclerView, NativeFeedType.Share, SupportFragmentManager);
                MainRecyclerView.SetXAdapter(PostFeedAdapter, null);
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
                    TxtSharePost.Click += TxtSharePostOnClick;
                }
                else
                {
                    TxtSharePost.Click -= TxtSharePostOnClick;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Event

        //Share Post 
        private async void TxtSharePostOnClick(object sender, EventArgs e)
        {
            try
            {
                if (TypePost == "Group")
                {
                    (int apiStatus, dynamic respond) = await RequestsAsync.Posts.SharePostToAsync(PostData.PostId, GroupData.GroupId, "share_post_on_group", TxtContentPost.Text);
                    ResultApi(apiStatus, respond);
                }
                else if (TypePost == "Page")
                { 
                    (int apiStatus, dynamic respond) = await RequestsAsync.Posts.SharePostToAsync(PostData.PostId, PageData.PageId, "share_post_on_page",TxtContentPost.Text);
                    ResultApi(apiStatus, respond);
                }
                else if (TypePost == "MyTimeline")
                {
                    (int apiStatus, dynamic respond) = await RequestsAsync.Posts.SharePostToAsync(PostData.PostId, UserDetails.UserId, "share_post_on_timeline");
                    ResultApi(apiStatus, respond);
                } 
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void ResultApi(int apiStatus, dynamic respond)
        {
            try
            {
                if (apiStatus == 200)
                {
                    if (respond is SharePostToObject result)
                    {
                        var combine = new FeedCombiner(result.Data, TabbedMainActivity.GetInstance()?.NewsFeedTab?.PostFeedAdapter.ListDiffer, this);
                        combine.CombineDefaultPostSections("Top");
                          
                        Toast.MakeText(this, GetText(Resource.String.Lbl_PostSuccessfullyShared), ToastLength.Short).Show();

                        if (UserDetails.SoundControl)
                            Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("PopNotificationPost.mp3");

                        Finish();
                    }
                }
                else Methods.DisplayReportResult(this, respond);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
         
        #endregion
         
        private void GetDataPost()
        {
            try
            {
                if (TypePost == "Group")
                {
                    GroupData = JsonConvert.DeserializeObject<GroupClass>(Intent.GetStringExtra("ShareToGroup"));
                    if (GroupData != null)
                    {
                        GlideImageLoader.LoadImage(this, GroupData.Avatar, PostSectionImage, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                        TxtUserName.Text = GroupData.GroupName;
                    }
                }
                else if (TypePost == "Page")
                {
                    PageData = JsonConvert.DeserializeObject<PageClass>(Intent.GetStringExtra("ShareToPage"));
                    if (PageData != null)
                    {
                        GlideImageLoader.LoadImage(this, PageData.Avatar, PostSectionImage, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                        TxtUserName.Text = PageData.PageName;
                    } 
                }
                else if (TypePost == "MyTimeline")
                {
                    GlideImageLoader.LoadImage(this, UserDetails.Avatar, PostSectionImage, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                    TxtUserName.Text = UserDetails.FullName;
                }

                PostData = JsonConvert.DeserializeObject<PostDataObject>(Intent.GetStringExtra("PostObject"));
                if (PostData != null)
                {
                    switch (TypePost)
                    {
                        case "Group" when PostData.GroupRecipient == null:
                        {
                            if (GroupData != null)
                            {
                                PostData.GroupId = GroupData.GroupId;
                                PostData.GroupRecipient = GroupData;
                            } 
                            break;
                        }
                        case "Page" when PostData.Publisher == null:
                        {
                            if (PageData != null)
                            {
                                PostData.PageId = PageData.PageId;
                                PostData.Publisher = new PublisherPost()
                                {
                                    Avatar = PageData.Avatar,
                                    About = PageData.About,
                                    Active = PageData.Active,
                                    Address = PageData.Address,
                                    BackgroundImage = PageData.BackgroundImage,
                                    Boosted = Convert.ToInt32(PageData.Boosted),
                                    CallActionType = Convert.ToInt32(PageData.CallActionType),
                                    Category = PageData.Category,
                                    Company = PageData.Company,
                                    Cover = PageData.Cover,
                                    Google = PageData.Google,
                                    Instgram = PageData.Instgram,
                                    IsPageOnwer = PageData.IsPageOnwer,
                                    Linkedin = PageData.Linkedin,
                                    Name = PageData.Name,
                                    PageCategory = Convert.ToInt32(PageData.PageCategory),
                                    PageDescription = PageData.PageDescription,
                                    PageId = Convert.ToInt32(PageData.PageId),
                                    PageName = PageData.PageName,
                                    PageTitle = PageData.PageTitle,
                                    Phone = PageData.Phone,
                                    Rating = Convert.ToInt32(PageData.Rating),
                                    Registered = PageData.Registered,
                                    Twitter = PageData.Twitter,
                                    Url = PageData.Url,
                                };
                            } 
                            break;
                        }
                    }
                     
                    var combine = new FeedCombiner(PostData, PostFeedAdapter.ListDiffer, this);
                    combine.CombineDefaultPostSections();

                    PostFeedAdapter.NotifyDataSetChanged();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        } 
    }
}