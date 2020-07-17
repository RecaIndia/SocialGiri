using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using AT.Markushi.UI;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Developer.SEmojis.Actions;
using Developer.SEmojis.Helper;
using Java.Lang;
using JP.ShTs.StoriesProgressView;
using Newtonsoft.Json;
using WoWonder.Activities.AddPost.Service;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using Exception = System.Exception;
using File = Java.IO.File;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Uri = Android.Net.Uri;

namespace WoWonder.Activities.Story
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class AddStoryActivity : AppCompatActivity
    {
        #region Variables Basic

        private ImageView StoryImageView;
        private VideoView StoryVideoView;
        private AppCompatImageView EmojisView;
        private CircleButton PlayIconVideo, AddStoryButton;
        private EmojiconEditText EmojisIconEditText;
        private RelativeLayout RootView;
        private string PathStory = "", Type = "", Thumbnail = UserDetails.Avatar;
        private StoriesProgressView StoriesProgress;
        private long Duration;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                Window.SetSoftInputMode(SoftInput.AdjustResize);
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);

                base.OnCreate(savedInstanceState);

                // Create your application here
                SetContentView(Resource.Layout.AddStory_layout);
                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();

                Thumbnail = Intent.GetStringExtra("Thumbnail") ?? UserDetails.Avatar;

                var dataUri = Intent.GetStringExtra("Uri") ?? "Data not available";
                if (dataUri != "Data not available" && !string.IsNullOrEmpty(dataUri)) PathStory = dataUri; // Uri file 
                var dataType = Intent.GetStringExtra("Type") ?? "Data not available";
                if (dataType != "Data not available" && !string.IsNullOrEmpty(dataType)) Type = dataType; // Type file  

                if (Type == "image")
                    SetImageStory(PathStory);
                else
                    SetVideoStory(PathStory);

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
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        protected override void OnDestroy()
        {
            try
            {
                // Very important !
                StoriesProgress.Destroy();

                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
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
                StoryImageView = FindViewById<ImageView>(Resource.Id.imagstoryDisplay);
                StoryVideoView = FindViewById<VideoView>(Resource.Id.VideoView);
                PlayIconVideo = FindViewById<CircleButton>(Resource.Id.Videoicon_button);
                EmojisView = FindViewById<AppCompatImageView>(Resource.Id.emojiicon);
                EmojisIconEditText = FindViewById<EmojiconEditText>(Resource.Id.EmojiconEditText5);
                AddStoryButton = FindViewById<CircleButton>(Resource.Id.sendButton);
                RootView = FindViewById<RelativeLayout>(Resource.Id.storyDisplay);

                StoriesProgress = FindViewById<StoriesProgressView>(Resource.Id.stories);
                StoriesProgress.Visibility = ViewStates.Gone;

                Methods.SetColorEditText(EmojisIconEditText, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                var emojisIcon = new EmojIconActions(this, RootView, EmojisIconEditText, EmojisView);
                emojisIcon.ShowEmojIcon();

                PlayIconVideo.Visibility = ViewStates.Gone;
                PlayIconVideo.Tag = "Play";
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
                    toolbar.Title = GetString(Resource.String.Lbl_Addnewstory);
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

        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                {
                    AddStoryButton.Click += AddStoryButtonOnClick;
                    StoryVideoView.Completion += StoryVideoViewOnCompletion;
                    PlayIconVideo.Click += PlayIconVideoOnClick;
                }
                else
                {
                    AddStoryButton.Click -= AddStoryButtonOnClick;
                    StoryVideoView.Completion -= StoryVideoViewOnCompletion;
                    PlayIconVideo.Click -= PlayIconVideoOnClick;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void SetImageStory(string url)
        {
            try
            {
                if (StoryImageView.Visibility == ViewStates.Gone)
                    StoryImageView.Visibility = ViewStates.Visible;

                var file = Uri.FromFile(new File(url));

                Glide.With(this).Load(file.Path).Apply(new RequestOptions()).Into(StoryImageView);

                // GlideImageLoader.LoadImage(this, file.Path, StoryImageView, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

                if (StoryVideoView.Visibility == ViewStates.Visible)
                    StoryVideoView.Visibility = ViewStates.Gone;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void SetVideoStory(string url)
        {
            try
            {
                if (StoryImageView.Visibility == ViewStates.Visible)
                    StoryImageView.Visibility = ViewStates.Gone;

                if (StoryVideoView.Visibility == ViewStates.Gone)
                    StoryVideoView.Visibility = ViewStates.Visible;

                PlayIconVideo.Visibility = ViewStates.Visible;
                PlayIconVideo.Tag = "Play";
                PlayIconVideo.SetImageResource(Resource.Drawable.ic_play_arrow);

                if (StoryVideoView.IsPlaying)
                    StoryVideoView.Suspend();

                if (url.Contains("http"))
                {
                    StoryVideoView.SetVideoURI(Uri.Parse(url));
                }
                else
                {
                    var file = Uri.FromFile(new File(url));
                    StoryVideoView.SetVideoPath(file.Path);
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
                StoryImageView = null;
                StoryVideoView = null;
                PlayIconVideo = null;
                EmojisView = null;
                EmojisIconEditText = null;
                AddStoryButton = null;
                RootView = null; 
                StoriesProgress = null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        #endregion

        #region Events

        private void PlayIconVideoOnClick(object sender, EventArgs e)
        {
            try
            {
                if (PlayIconVideo.Tag.ToString() == "Play")
                {
                    MediaMetadataRetriever retriever;
                    if (PathStory.Contains("http"))
                    {
                        StoryVideoView.SetVideoURI(Uri.Parse(PathStory));

                        retriever = new MediaMetadataRetriever();
                        if ((int)Build.VERSION.SdkInt >= 14)
                            retriever.SetDataSource(PathStory, new Dictionary<string, string>());
                        else
                            retriever.SetDataSource(PathStory);
                    }
                    else
                    {
                        var file = Uri.FromFile(new File(PathStory));
                        StoryVideoView.SetVideoPath(file.Path);

                        retriever = new MediaMetadataRetriever();
                        //if ((int)Build.VERSION.SdkInt >= 14)
                        //    retriever.SetDataSource(file.Path, new Dictionary<string, string>());
                        //else
                        //    retriever.SetDataSource(file.Path);
                        retriever.SetDataSource(file.Path);
                    }
                    StoryVideoView.Start();

                    Duration = Long.ParseLong(retriever.ExtractMetadata(MetadataKey.Duration));
                    retriever.Release();

                    StoriesProgress.Visibility = ViewStates.Visible;
                    StoriesProgress.SetStoriesCount(1); // <- set stories
                    StoriesProgress.SetStoryDuration(Duration); // <- set a story duration
                    StoriesProgress.StartStories(); // <- start progress

                    PlayIconVideo.Tag = "Stop";
                    PlayIconVideo.SetImageResource(Resource.Drawable.ic_stop_white_24dp);
                }
                else
                {
                    StoriesProgress.Visibility = ViewStates.Gone;
                    StoriesProgress.Pause();

                    StoryVideoView.Pause();

                    PlayIconVideo.Tag = "Play";
                    PlayIconVideo.SetImageResource(Resource.Drawable.ic_play_arrow);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void StoryVideoViewOnCompletion(object sender, EventArgs e)
        {
            try
            {
                StoriesProgress.Visibility = ViewStates.Gone;
                StoriesProgress.Pause();
                StoryVideoView.Pause();

                PlayIconVideo.Tag = "Play";
                PlayIconVideo.SetImageResource(Resource.Drawable.ic_play_arrow);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //add
        private void AddStoryButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    //Show a progress
                    //AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                    var item = new FileUpload()
                    {
                        StoryFileType = Type,
                        StoryFilePath = PathStory,
                        StoryDescription = EmojisIconEditText.Text,
                        StoryTitle = EmojisIconEditText.Text, 
                        StoryThumbnail = Thumbnail, 
                    };

                    Intent intent = new Intent(this, typeof(PostService));
                    intent.SetAction(PostService.ActionStory);
                    intent.PutExtra("DataPost", JsonConvert.SerializeObject(item));
                    StartService(intent);

                    Finish(); 
                    
                    #region Old

                        //var PostFeedAdapter.ListDiffer = GlobalContext.NewsFeedTab.PostFeedAdapter.ListDiffer;
                        //var checkSection = PostFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.Story);
                        //if (checkSection != null)
                        //{
                        //    var modelStory = GlobalContext.NewsFeedTab.PostFeedAdapter.HolderStory.StoryAdapter;

                        //    string time = Methods.Time.TimeAgo(DateTime.Now);
                        //    int unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                        //    string time2 = unixTimestamp.ToString();

                        //    var userData = ListUtils.MyProfileList.FirstOrDefault();

                        //    //just pass file_path and type video or image
                        //    var (apiStatus, respond) = await RequestsAsync.Story.Create_Story(EmojisIconEditText.Text, EmojisIconEditText.Text, PathStory, Type);
                        //    if (apiStatus == 200)
                        //    {
                        //        if (respond is CreateStoryObject result)
                        //        {
                        //            AndHUD.Shared.ShowSuccess(this, GetText(Resource.String.Lbl_Done), MaskType.Clear, TimeSpan.FromSeconds(3));

                        //            var check = modelStory.StoryList?.FirstOrDefault(a => a.UserId == UserDetails.UserId);
                        //            if (check != null)
                        //            {
                        //                if (Type == "image")
                        //                {
                        //                    var item = new GetUserStoriesObject.StoryObject.Story()
                        //                    {
                        //                        UserId = UserDetails.UserId,
                        //                        Id = result.StoryId,
                        //                        Description = EmojisIconEditText.Text,
                        //                        Title = EmojisIconEditText.Text,
                        //                        TimeText = time,
                        //                        IsOwner = true,
                        //                        Expire = "",
                        //                        Posted = time2,
                        //                        Thumbnail = PathStory,
                        //                        UserData = userData,
                        //                        Images = new List<GetUserStoriesObject.StoryObject.Image>(),
                        //                        Videos = new List<GetUserStoriesObject.StoryObject.Video>()
                        //                    };

                        //                    if (check.DurationsList == null)
                        //                        check.DurationsList = new List<long>() { AppSettings.StoryDuration };
                        //                    else
                        //                        check.DurationsList.Add(AppSettings.StoryDuration);

                        //                    check.Stories.Add(item);
                        //                }
                        //                else
                        //                {
                        //                    //var fileName = PathStory.Split('/').Last();
                        //                    //var fileNameWithoutExtension = fileName.Split('.').First(); 
                        //                    //var path = Methods.Path.FolderDcimImage + "/" + fileNameWithoutExtension +".png"; 
                        //                    //var vidoPlaceHolderImage =Methods.MultiMedia.GetMediaFrom_Gallery(Methods.Path.FolderDcimImage, fileNameWithoutExtension + ".png");
                        //                    //if (vidoPlaceHolderImage == "File Dont Exists")
                        //                    //{
                        //                    //    var bitmapImage = Methods.MultiMedia.Retrieve_VideoFrame_AsBitmap(PathStory);
                        //                    //    Methods.MultiMedia.Export_Bitmap_As_Image(bitmapImage,fileNameWithoutExtension, Methods.Path.FolderDcimImage);
                        //                    //}

                        //                    var item = new GetUserStoriesObject.StoryObject.Story()
                        //                    {
                        //                        UserId = UserDetails.UserId,
                        //                        Id = result.StoryId,
                        //                        Description = EmojisIconEditText.Text,
                        //                        Title = EmojisIconEditText.Text,
                        //                        TimeText = time,
                        //                        IsOwner = true,
                        //                        Expire = "",
                        //                        Posted = time2,
                        //                        Thumbnail = Thumbnail,
                        //                        UserData = userData,
                        //                        Images = new List<GetUserStoriesObject.StoryObject.Image>(),
                        //                        Videos = new List<GetUserStoriesObject.StoryObject.Video>()
                        //                    {
                        //                        new GetUserStoriesObject.StoryObject.Video()
                        //                        {
                        //                            StoryId = result.StoryId,
                        //                            Filename = PathStory,
                        //                            Id = time2,
                        //                            Expire = time2,
                        //                            Type = "video",
                        //                        }
                        //                    }
                        //                    };

                        //                    var duration = WoWonderTools.GetDuration(PathStory);

                        //                    if (check.DurationsList == null)
                        //                        check.DurationsList = new List<long>() { Long.ParseLong(duration) };
                        //                    else
                        //                        check.DurationsList.Add(Long.ParseLong(duration));

                        //                    check.Stories.Add(item);
                        //                }
                        //            }
                        //            else
                        //            {
                        //                if (Type == "image")
                        //                {
                        //                    var item = new GetUserStoriesObject.StoryObject
                        //                    {
                        //                        Type = "image",
                        //                        Stories = new List<GetUserStoriesObject.StoryObject.Story>
                        //                    {
                        //                        new GetUserStoriesObject.StoryObject.Story()
                        //                        {
                        //                            UserId = UserDetails.UserId,
                        //                            Id = result.StoryId,
                        //                            Description = EmojisIconEditText.Text,
                        //                            Title = EmojisIconEditText.Text,
                        //                            TimeText = time,
                        //                            IsOwner = true,
                        //                            Expire = "",
                        //                            Posted = time2,
                        //                            Thumbnail = PathStory,
                        //                            UserData = userData,
                        //                            Images = new List<GetUserStoriesObject.StoryObject.Image>(),
                        //                            Videos = new List<GetUserStoriesObject.StoryObject.Video>(),
                        //                        }

                        //                    },
                        //                        UserId = userData?.UserId,
                        //                        Username = userData?.Username,
                        //                        Email = userData?.Email,
                        //                        FirstName = userData?.FirstName,
                        //                        LastName = userData?.LastName,
                        //                        Avatar = userData?.Avatar,
                        //                        Cover = userData?.Cover,
                        //                        BackgroundImage = userData?.BackgroundImage,
                        //                        RelationshipId = userData?.RelationshipId,
                        //                        Address = userData?.Address,
                        //                        Working = userData?.Working,
                        //                        Gender = userData?.Gender,
                        //                        Facebook = userData?.Facebook,
                        //                        Google = userData?.Google,
                        //                        Twitter = userData?.Twitter,
                        //                        Linkedin = userData?.Linkedin,
                        //                        Website = userData?.Website,
                        //                        Instagram = userData?.Instagram,
                        //                        WebDeviceId = userData?.WebDeviceId,
                        //                        Language = userData?.Language,
                        //                        IpAddress = userData?.IpAddress,
                        //                        PhoneNumber = userData?.PhoneNumber,
                        //                        Timezone = userData?.Timezone,
                        //                        Lat = userData?.Lat,
                        //                        Lng = userData?.Lng,
                        //                        About = userData?.About,
                        //                        Birthday = userData?.Birthday,
                        //                        Registered = userData?.Registered,
                        //                        Lastseen = userData?.Lastseen,
                        //                        LastLocationUpdate = userData?.LastLocationUpdate,
                        //                        Balance = userData?.Balance,
                        //                        Verified = userData?.Verified,
                        //                        Status = userData?.Status,
                        //                        Active = userData?.Active,
                        //                        Admin = userData?.Admin,
                        //                        IsPro = userData?.IsPro,
                        //                        ProType = userData?.ProType,
                        //                        School = userData?.School,
                        //                        Name = userData?.Name,
                        //                        AndroidMDeviceId = userData?.AndroidMDeviceId,
                        //                        ECommented = userData?.ECommented,
                        //                        AndroidNDeviceId = userData?.AndroidMDeviceId,
                        //                        AvatarFull = userData?.AvatarFull,
                        //                        BirthPrivacy = userData?.BirthPrivacy,
                        //                        CanFollow = userData?.CanFollow,
                        //                        ConfirmFollowers = userData?.ConfirmFollowers,
                        //                        CountryId = userData?.CountryId,
                        //                        EAccepted = userData?.EAccepted,
                        //                        EFollowed = userData?.EFollowed,
                        //                        EJoinedGroup = userData?.EJoinedGroup,
                        //                        ELastNotif = userData?.ELastNotif,
                        //                        ELiked = userData?.ELiked,
                        //                        ELikedPage = userData?.ELikedPage,
                        //                        EMentioned = userData?.EMentioned,
                        //                        EProfileWallPost = userData?.EProfileWallPost,
                        //                        ESentmeMsg = userData?.ESentmeMsg,
                        //                        EShared = userData?.EShared,
                        //                        EVisited = userData?.EVisited,
                        //                        EWondered = userData?.EWondered,
                        //                        EmailNotification = userData?.EmailNotification,
                        //                        FollowPrivacy = userData?.FollowPrivacy,
                        //                        FriendPrivacy = userData?.FriendPrivacy,
                        //                        GenderText = userData?.GenderText,
                        //                        InfoFile = userData?.InfoFile,
                        //                        IosMDeviceId = userData?.IosMDeviceId,
                        //                        IosNDeviceId = userData?.IosNDeviceId,
                        //                        IsFollowing = userData?.IsFollowing,
                        //                        IsFollowingMe = userData?.IsFollowingMe,
                        //                        LastAvatarMod = userData?.LastAvatarMod,
                        //                        LastCoverMod = userData?.LastCoverMod,
                        //                        LastDataUpdate = userData?.LastDataUpdate,
                        //                        LastFollowId = userData?.LastFollowId,
                        //                        LastLoginData = userData?.LastLoginData,
                        //                        LastseenStatus = userData?.LastseenStatus,
                        //                        LastseenTimeText = userData?.LastseenTimeText,
                        //                        LastseenUnixTime = userData?.LastseenUnixTime,
                        //                        MessagePrivacy = userData?.MessagePrivacy,
                        //                        NewEmail = userData?.NewEmail,
                        //                        NewPhone = userData?.NewPhone,
                        //                        NotificationSettings = userData?.NotificationSettings,
                        //                        NotificationsSound = userData?.NotificationsSound,
                        //                        OrderPostsBy = userData?.OrderPostsBy,
                        //                        PaypalEmail = userData?.PaypalEmail,
                        //                        PostPrivacy = userData?.PostPrivacy,
                        //                        Referrer = userData?.Referrer,
                        //                        ShareMyData = userData?.ShareMyData,
                        //                        ShareMyLocation = userData?.ShareMyLocation,
                        //                        ShowActivitiesPrivacy = userData?.ShowActivitiesPrivacy,
                        //                        TwoFactor = userData?.TwoFactor,
                        //                        TwoFactorVerified = userData?.TwoFactorVerified,
                        //                        Url = userData?.Url,
                        //                        VisitPrivacy = userData?.VisitPrivacy,
                        //                        Vk = userData?.Vk,
                        //                        Wallet = userData?.Wallet,
                        //                        WorkingLink = userData?.WorkingLink,
                        //                        Youtube = userData?.Youtube,
                        //                        City = userData?.City,
                        //                        Points = userData?.Points,
                        //                        DailyPoints = userData?.DailyPoints,
                        //                        PointDayExpire = userData?.PointDayExpire,
                        //                        State = userData?.State,
                        //                        Zip = userData?.Zip,
                        //                        Details = new DetailsUnion()
                        //                        {
                        //                            DetailsClass = new Details(),
                        //                            AnythingArray = new List<object>()
                        //                        },
                        //                    };

                        //                    if (item.DurationsList == null)
                        //                        item.DurationsList = new List<long>() { AppSettings.StoryDuration };
                        //                    else
                        //                        item.DurationsList.Add(AppSettings.StoryDuration);

                        //                    modelStory.StoryList?.Add(item);
                        //                }
                        //                else
                        //                {
                        //                    var item = new GetUserStoriesObject.StoryObject()
                        //                    {
                        //                        Type = "video",
                        //                        Stories = new List<GetUserStoriesObject.StoryObject.Story>()
                        //                    {
                        //                        new GetUserStoriesObject.StoryObject.Story()
                        //                        {
                        //                            UserId = UserDetails.UserId,
                        //                            Id = result.StoryId,
                        //                            Description = EmojisIconEditText.Text,
                        //                            Title = EmojisIconEditText.Text,
                        //                            TimeText = time,
                        //                            IsOwner = true,
                        //                            Expire = "",
                        //                            Posted = time2,
                        //                            Thumbnail = Thumbnail,
                        //                            UserData = userData,
                        //                            Images = new List<GetUserStoriesObject.StoryObject.Image>(),
                        //                            Videos = new List<GetUserStoriesObject.StoryObject.Video>()
                        //                            {
                        //                                new GetUserStoriesObject.StoryObject.Video()
                        //                                {
                        //                                    StoryId = result.StoryId,
                        //                                    Filename = PathStory,
                        //                                    Id = time2,
                        //                                    Expire = time2,
                        //                                    Type = "video",
                        //                                }
                        //                            }
                        //                        },
                        //                    },
                        //                        UserId = userData?.UserId,
                        //                        Username = userData?.Username,
                        //                        Email = userData?.Email,
                        //                        FirstName = userData?.FirstName,
                        //                        LastName = userData?.LastName,
                        //                        Avatar = userData?.Avatar,
                        //                        Cover = userData?.Cover,
                        //                        BackgroundImage = userData?.BackgroundImage,
                        //                        RelationshipId = userData?.RelationshipId,
                        //                        Address = userData?.Address,
                        //                        Working = userData?.Working,
                        //                        Gender = userData?.Gender,
                        //                        Facebook = userData?.Facebook,
                        //                        Google = userData?.Google,
                        //                        Twitter = userData?.Twitter,
                        //                        Linkedin = userData?.Linkedin,
                        //                        Website = userData?.Website,
                        //                        Instagram = userData?.Instagram,
                        //                        WebDeviceId = userData?.WebDeviceId,
                        //                        Language = userData?.Language,
                        //                        IpAddress = userData?.IpAddress,
                        //                        PhoneNumber = userData?.PhoneNumber,
                        //                        Timezone = userData?.Timezone,
                        //                        Lat = userData?.Lat,
                        //                        Lng = userData?.Lng,
                        //                        About = userData?.About,
                        //                        Birthday = userData?.Birthday,
                        //                        Registered = userData?.Registered,
                        //                        Lastseen = userData?.Lastseen,
                        //                        LastLocationUpdate = userData?.LastLocationUpdate,
                        //                        Balance = userData?.Balance,
                        //                        Verified = userData?.Verified,
                        //                        Status = userData?.Status,
                        //                        Active = userData?.Active,
                        //                        Admin = userData?.Admin,
                        //                        IsPro = userData?.IsPro,
                        //                        ProType = userData?.ProType,
                        //                        School = userData?.School,
                        //                        Name = userData?.Name,
                        //                        AndroidMDeviceId = userData?.AndroidMDeviceId,
                        //                        ECommented = userData?.ECommented,
                        //                        AndroidNDeviceId = userData?.AndroidMDeviceId,
                        //                        AvatarFull = userData?.AvatarFull,
                        //                        BirthPrivacy = userData?.BirthPrivacy,
                        //                        CanFollow = userData?.CanFollow,
                        //                        ConfirmFollowers = userData?.ConfirmFollowers,
                        //                        CountryId = userData?.CountryId,
                        //                        EAccepted = userData?.EAccepted,
                        //                        EFollowed = userData?.EFollowed,
                        //                        EJoinedGroup = userData?.EJoinedGroup,
                        //                        ELastNotif = userData?.ELastNotif,
                        //                        ELiked = userData?.ELiked,
                        //                        ELikedPage = userData?.ELikedPage,
                        //                        EMentioned = userData?.EMentioned,
                        //                        EProfileWallPost = userData?.EProfileWallPost,
                        //                        ESentmeMsg = userData?.ESentmeMsg,
                        //                        EShared = userData?.EShared,
                        //                        EVisited = userData?.EVisited,
                        //                        EWondered = userData?.EWondered,
                        //                        EmailNotification = userData?.EmailNotification,
                        //                        FollowPrivacy = userData?.FollowPrivacy,
                        //                        FriendPrivacy = userData?.FriendPrivacy,
                        //                        GenderText = userData?.GenderText,
                        //                        InfoFile = userData?.InfoFile,
                        //                        IosMDeviceId = userData?.IosMDeviceId,
                        //                        IosNDeviceId = userData?.IosNDeviceId,
                        //                        IsFollowing = userData?.IsFollowing,
                        //                        IsFollowingMe = userData?.IsFollowingMe,
                        //                        LastAvatarMod = userData?.LastAvatarMod,
                        //                        LastCoverMod = userData?.LastCoverMod,
                        //                        LastDataUpdate = userData?.LastDataUpdate,
                        //                        LastFollowId = userData?.LastFollowId,
                        //                        LastLoginData = userData?.LastLoginData,
                        //                        LastseenStatus = userData?.LastseenStatus,
                        //                        LastseenTimeText = userData?.LastseenTimeText,
                        //                        LastseenUnixTime = userData?.LastseenUnixTime,
                        //                        MessagePrivacy = userData?.MessagePrivacy,
                        //                        NewEmail = userData?.NewEmail,
                        //                        NewPhone = userData?.NewPhone,
                        //                        NotificationSettings = userData?.NotificationSettings,
                        //                        NotificationsSound = userData?.NotificationsSound,
                        //                        OrderPostsBy = userData?.OrderPostsBy,
                        //                        PaypalEmail = userData?.PaypalEmail,
                        //                        PostPrivacy = userData?.PostPrivacy,
                        //                        Referrer = userData?.Referrer,
                        //                        ShareMyData = userData?.ShareMyData,
                        //                        ShareMyLocation = userData?.ShareMyLocation,
                        //                        ShowActivitiesPrivacy = userData?.ShowActivitiesPrivacy,
                        //                        TwoFactor = userData?.TwoFactor,
                        //                        TwoFactorVerified = userData?.TwoFactorVerified,
                        //                        Url = userData?.Url,
                        //                        VisitPrivacy = userData?.VisitPrivacy,
                        //                        Vk = userData?.Vk,
                        //                        Wallet = userData?.Wallet,
                        //                        WorkingLink = userData?.WorkingLink,
                        //                        Youtube = userData?.Youtube,
                        //                        City = userData?.City,
                        //                        Points = userData?.Points,
                        //                        DailyPoints = userData?.DailyPoints,
                        //                        State = userData?.State,
                        //                        Zip = userData?.Zip,
                        //                        Details = new DetailsUnion()
                        //                        {
                        //                            DetailsClass = new Details(),
                        //                            AnythingArray = new List<object>()
                        //                        },
                        //                    };

                        //                    var duration = WoWonderTools.GetDuration(PathStory);

                        //                    if (item.DurationsList == null)
                        //                        item.DurationsList = new List<long>() { Long.ParseLong(duration) };
                        //                    else
                        //                        item.DurationsList.Add(Long.ParseLong(duration));

                        //                    modelStory.StoryList?.Add(item);
                        //                }
                        //            }

                        //            modelStory.NotifyDataSetChanged();

                        //            Finish();
                        //        }
                        //    }
                        //    else Methods.DisplayReportResult(this, respond);

                        //    AndHUD.Shared.Dismiss(this);
                        //}
                        #endregion
                }
                else
                {
                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                //AndHUD.Shared.Dismiss(this);
            }
        }

        #endregion
    }
}