using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Newtonsoft.Json;
using Sephiroth.ImageZoom;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Activities.PostData;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Requests;
using ClipboardManager = Android.Content.ClipboardManager;
using Exception = System.Exception;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace WoWonder.Activities.UsersPages
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class ImagePostViewerActivity : AppCompatActivity 
    {
        #region Variables Basic

        private ImageViewTouch PageImage;
        private ImageView ImgLike, ImgWoWonder, ImgWonder; 
        private TextView TxtDescription, TxtCountLike, TxtCountWoWonder, TxtWonder, ShareText; 
        private LinearLayout MainSectionButton,BtnCountLike, BtnCountWoWonder, BtnLike, BtnComment, BtnShare, BtnWonder, InfoImageLiner;
        private RelativeLayout MainLayout;
        private PostDataObject AlbumData;
        private ReactButton LikeButton;
        private string ImageUrl;
        private PostClickListener ClickListener;
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
                SetContentView(Resource.Layout.ImagePostViewer_Layout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();

                Get_DataImage();

                ClickListener = new PostClickListener(this);

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

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.ImagePost, menu);

            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;

                case Resource.Id.download:
                    Download_OnClick();
                    break;

                case Resource.Id.ic_action_comment:
                    Copy_OnClick();
                    break;

                case Resource.Id.action_More:
                    More_OnClick();
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        //Event Download Image  
        private void Download_OnClick()
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                }
                else
                {
                    Methods.MultiMedia.DownloadMediaTo_GalleryAsync(Methods.Path.FolderDcimImage, ImageUrl);
                    Toast.MakeText(this, GetText(Resource.String.Lbl_ImageSaved), ToastLength.Short).Show();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Event Copy link image 
        private void Copy_OnClick()
        {
            try
            {
                var clipboardManager = (ClipboardManager)GetSystemService(ClipboardService);

                var clipData = ClipData.NewPlainText("text", AlbumData.Url);
                clipboardManager.PrimaryClip = clipData;

                Toast.MakeText(this, GetText(Resource.String.Lbl_Copied), ToastLength.Short).Show();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Event More 
        private void More_OnClick()
        {
            try
            {
                ClickListener.MorePostIconClick(new GlobalClickEventArgs { NewsFeedClass = AlbumData });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                PageImage = FindViewById<ImageViewTouch>(Resource.Id.imageview); 
                TxtDescription = FindViewById<TextView>(Resource.Id.tv_description); 
                ImgLike = FindViewById<ImageView>(Resource.Id.image_like1);
                ImgWoWonder = FindViewById<ImageView>(Resource.Id.image_wowonder);
                 
                TxtCountLike = FindViewById<TextView>(Resource.Id.LikeText1);
                TxtCountWoWonder = FindViewById<TextView>(Resource.Id.WoWonderTextCount);

                MainLayout = FindViewById<RelativeLayout>(Resource.Id.main);
                InfoImageLiner = FindViewById<LinearLayout>(Resource.Id.infoImageLiner);
                InfoImageLiner.Visibility = ViewStates.Visible;
                 
                BtnCountLike = FindViewById<LinearLayout>(Resource.Id.linerlikeCount);
                BtnCountWoWonder = FindViewById<LinearLayout>(Resource.Id.linerwowonderCount);

                BtnLike = FindViewById<LinearLayout>(Resource.Id.linerlike);
                BtnComment = FindViewById<LinearLayout>(Resource.Id.linercomment);
                BtnShare = FindViewById<LinearLayout>(Resource.Id.linershare);

                MainSectionButton = FindViewById<LinearLayout>(Resource.Id.mainsection);
                BtnWonder = FindViewById<LinearLayout>(Resource.Id.linerSecondReaction);
                ImgWonder = FindViewById<ImageView>(Resource.Id.image_SecondReaction);
                TxtWonder = FindViewById<TextView>(Resource.Id.SecondReactionText);

                // set the default image display type
               // PageImage.SetDisplayType(ImageViewTouchBase.DisplayType.FitIfBigger);

                LikeButton = FindViewById<ReactButton>(Resource.Id.beactButton);
                LikeButton.SetTextColor(Color.White);

                ShareText = FindViewById<TextView>(Resource.Id.ShareText); 
                if (!AppSettings.ShowTextShareButton && ShareText != null)
                    ShareText.Visibility = ViewStates.Gone;

                if (AppSettings.PostButton == PostButtonSystem.ReactionDefault || AppSettings.PostButton == PostButtonSystem.ReactionSubShine || AppSettings.PostButton == PostButtonSystem.Like)
                {
                    MainSectionButton.WeightSum = 3;
                    BtnWonder.Visibility = ViewStates.Gone;

                    TxtCountWoWonder.Visibility = ViewStates.Gone;
                    BtnCountWoWonder.Visibility = ViewStates.Gone;
                    ImgWoWonder.Visibility = ViewStates.Gone;

                }
                else if (AppSettings.PostButton == PostButtonSystem.Wonder)
                {
                    MainSectionButton.WeightSum = 4;
                    BtnWonder.Visibility = ViewStates.Visible;

                    TxtCountWoWonder.Visibility = ViewStates.Visible;
                    BtnCountWoWonder.Visibility = ViewStates.Visible;
                    ImgWoWonder.Visibility = ViewStates.Visible;

                    ImgWoWonder.SetImageResource(Resource.Drawable.ic_action_wowonder);
                    ImgWonder.SetImageResource(Resource.Drawable.ic_action_wowonder);
                    TxtWonder.Text = Application.Context.GetText(Resource.String.Btn_Wonder);
                }
                else if (AppSettings.PostButton == PostButtonSystem.DisLike)
                {
                    MainSectionButton.WeightSum = 4;
                    BtnWonder.Visibility = ViewStates.Visible;

                    TxtCountWoWonder.Visibility = ViewStates.Visible;
                    BtnCountWoWonder.Visibility = ViewStates.Visible;
                    ImgWoWonder.Visibility = ViewStates.Visible;

                    ImgWoWonder.SetImageResource(Resource.Drawable.ic_action_dislike);
                    ImgWonder.SetImageResource(Resource.Drawable.ic_action_dislike);
                    TxtWonder.Text = Application.Context.GetText(Resource.String.Btn_Dislike);
                }

                if (!AppSettings.ShowShareButton && BtnShare != null)
                    BtnShare.Visibility = ViewStates.Gone;
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
                    BtnComment.Click += BtnCommentOnClick;
                    BtnShare.Click += BtnShareOnClick;
                    BtnCountLike.Click += BtnCountLikeOnClick;
                    BtnCountWoWonder.Click += BtnCountWoWonderOnClick;
                    InfoImageLiner.Click += MainLayoutOnClick;
                    MainLayout.Click += MainLayoutOnClick;
                     
                    if (AppSettings.PostButton == PostButtonSystem.Wonder || AppSettings.PostButton == PostButtonSystem.DisLike)
                        BtnWonder.Click += BtnWonderOnClick;

                    LikeButton.Click += (sender, args) => LikeButton.ClickLikeAndDisLike(new GlobalClickEventArgs()
                    {
                        NewsFeedClass = AlbumData,
                        View = TxtCountLike,
                    },null, "ImagePostViewerActivity");

                    if (AppSettings.PostButton == PostButtonSystem.ReactionDefault || AppSettings.PostButton == PostButtonSystem.ReactionSubShine)
                        LikeButton.LongClick += (sender, args) => LikeButton.LongClickDialog(new GlobalClickEventArgs()
                        {
                            NewsFeedClass = AlbumData,
                            View = TxtCountLike,
                        }, null, "ImagePostViewerActivity"); 
                }
                else
                {
                    BtnComment.Click -= BtnCommentOnClick;
                    BtnShare.Click -= BtnShareOnClick;
                    BtnCountLike.Click -= BtnCountLikeOnClick;
                    BtnCountWoWonder.Click -= BtnCountWoWonderOnClick;
                    InfoImageLiner.Click -= MainLayoutOnClick;
                    MainLayout.Click -= MainLayoutOnClick;

                    if (AppSettings.PostButton == PostButtonSystem.Wonder || AppSettings.PostButton == PostButtonSystem.DisLike)
                        BtnWonder.Click -= BtnWonderOnClick;

                    LikeButton.Click += null;
                    if (AppSettings.PostButton == PostButtonSystem.ReactionDefault || AppSettings.PostButton == PostButtonSystem.ReactionSubShine)
                        LikeButton.LongClick -= null; 
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

                PageImage = null;
                TxtDescription = null;
                ImgLike = null;
                ImgWoWonder = null;
                TxtCountLike = null;
                TxtCountWoWonder = null;
                MainLayout = null;
                InfoImageLiner = null;
                BtnCountLike = null;
                BtnCountWoWonder = null;
                BtnLike = null;
                BtnComment = null;
                BtnShare = null;
                MainSectionButton = null;
                BtnWonder = null;
                ImgWonder = null;
                TxtWonder = null;
                LikeButton = null;
                ShareText = null;
                ClickListener = null;
                RewardedVideoAd = null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        #endregion

        #region Events

        private void MainLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                InfoImageLiner.Visibility = InfoImageLiner.Visibility != ViewStates.Visible ? ViewStates.Visible : ViewStates.Invisible;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Event Add Wonder
        private void BtnWonderOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(Application.Context, Application.Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                    return;
                }

                if (AlbumData.IsWondered != null && AlbumData.IsWondered.Value)
                {
                    var x = Convert.ToInt32(AlbumData.PostWonders);
                    if (x > 0)
                        x--;
                    else
                        x = 0;
                             
                    ImgWonder.SetColorFilter(Color.White);
                    ImgWoWonder.SetColorFilter(Color.White);
                    
                    AlbumData.IsWondered = false;
                    AlbumData.PostWonders = Convert.ToString(x, CultureInfo.InvariantCulture);

                    TxtCountWoWonder.Text = Methods.FunString.FormatPriceValue(x);
                    
                    switch (AppSettings.PostButton)
                    {
                        case PostButtonSystem.Wonder:
                            TxtWonder.Text = GetText(Resource.String.Btn_Wonder);
                            break;
                        case PostButtonSystem.DisLike:
                            TxtWonder.Text = GetText(Resource.String.Btn_Dislike);
                            break;
                    }

                    BtnWonder.Tag = "false";
                }
                else
                {
                    var x = Convert.ToInt32(AlbumData.PostWonders);
                    x++;

                    AlbumData.IsWondered = true;
                    AlbumData.PostWonders = Convert.ToString(x, CultureInfo.InvariantCulture);

                    ImgWonder.SetColorFilter(Color.ParseColor("#f89823"));
                    ImgWoWonder.SetColorFilter(Color.ParseColor("#f89823"));

                    TxtCountWoWonder.Text = Methods.FunString.FormatPriceValue(x);

                    if (AppSettings.PostButton == PostButtonSystem.Wonder)
                        TxtWonder.Text = GetText(Resource.String.Lbl_wondered);
                    else if (AppSettings.PostButton == PostButtonSystem.DisLike)
                        TxtWonder.Text = GetText(Resource.String.Lbl_disliked);

                    BtnWonder.Tag = "true";
                }
                 
                TxtCountWoWonder.Text = Methods.FunString.FormatPriceValue(int.Parse(AlbumData.PostWonders));
                       
                switch (AppSettings.PostButton)
                {
                    case PostButtonSystem.Wonder:
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.Post_Actions(AlbumData.PostId, "wonder") });
                        break;
                    case PostButtonSystem.DisLike:
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.Post_Actions(AlbumData.PostId, "dislike") });
                        break;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
         
        //Event Show all users Wowonder >> Open Post PostData_Activity
        private void BtnCountWoWonderOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(PostDataActivity));
                intent.PutExtra("PostId", AlbumData.PostId);
                intent.PutExtra("PostType", "post_wonders");
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Event Show all users liked >> Open Post PostData_Activity
        private void BtnCountLikeOnClick(object sender, EventArgs e)
        {
            try
            {
                if (AppSettings.PostButton == PostButtonSystem.ReactionDefault || AppSettings.PostButton == PostButtonSystem.ReactionSubShine)
                {
                    if (AlbumData.Reaction.Count > 0)
                    {
                        var intent = new Intent(this, typeof(ReactionPostTabbedActivity));
                        intent.PutExtra("PostObject", JsonConvert.SerializeObject(AlbumData));
                        StartActivity(intent);
                    }
                }
                else
                {
                    var intent = new Intent(this, typeof(PostDataActivity));
                    intent.PutExtra("PostId", AlbumData.PostId);
                    intent.PutExtra("PostType", "post_likes");
                    StartActivity(intent);
                } 
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Event Share
        private void BtnShareOnClick(object sender, EventArgs e)
        {
            try
            {
                ClickListener.SharePostClick(new GlobalClickEventArgs() {NewsFeedClass = AlbumData,}, PostModelType.ImagePost);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Event Add Comment
        private void BtnCommentOnClick(object sender, EventArgs e)
        {
            try
            {
                ClickListener.CommentPostClick(new GlobalClickEventArgs()
                {
                    NewsFeedClass = AlbumData, 
                });
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
         
        #endregion
         
        //Get Data 
        private void Get_DataImage()
        {
            try
            {
                AlbumData = JsonConvert.DeserializeObject<PostDataObject>(Intent.GetStringExtra("AlbumObject"));
                if (AlbumData != null)
                {
                    string index = Intent.GetStringExtra("itemIndex") ?? "0";
                    if (index == "00")
                    {
                        ImageUrl = !string.IsNullOrEmpty(AlbumData.PostSticker) ? AlbumData.PostSticker : AlbumData.PostFileFull;
                        //var bit = drawable_from_url(new Uri(ImageUrl));
                        //PageImage.SetImageBitmap(bit);
                        if (ImageUrl.Contains(".gif"))
                            Glide.With(this).Load(ImageUrl).Apply(new RequestOptions().Placeholder(Resource.Drawable.ImagePlacholder).FitCenter()).Into(PageImage);
                        else
                            Glide.With(this).Load(ImageUrl).Apply(new RequestOptions()).Into(PageImage); 
                    } 
                    else
                    {
                        var imageData = AlbumData.PhotoAlbum[int.Parse(index)];
                        if (imageData != null)
                        {
                            ImageUrl = imageData.Image;
                            //var bit = drawable_from_url(new Uri(imageData.Image));
                            //PageImage.SetImageBitmap(bit);

                            if (ImageUrl.Contains(".gif"))
                                Glide.With(this).Load(ImageUrl).Apply(new RequestOptions().Placeholder(Resource.Drawable.ImagePlacholder).FitCenter()).Into(PageImage);
                            else
                                Glide.With(this).Load(ImageUrl).Apply(new RequestOptions()).Into(PageImage);
                        }
                    }

                    if (string.IsNullOrEmpty(AlbumData.Orginaltext) || string.IsNullOrWhiteSpace(AlbumData.Orginaltext))
                    {
                        TxtDescription.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        TxtDescription.Text = Methods.FunString.DecodeString(AlbumData.Orginaltext);
                    }

                    if (AlbumData.IsLiked != null && AlbumData.IsLiked.Value)
                    {
                        BtnLike.Tag = "true";
                        ImgLike.SetColorFilter(Color.ParseColor(AppSettings.MainColor));
                    }
                    else
                    {
                        BtnLike.Tag = "false";
                        ImgLike.SetColorFilter(Color.White);
                    }
                      
                    if (AlbumData.IsWondered != null && AlbumData.IsWondered.Value)
                    {
                        BtnWonder.Tag = "true";
                        ImgWonder.SetColorFilter(Color.ParseColor("#f89823"));
                        ImgWoWonder.SetColorFilter(Color.ParseColor("#f89823"));

                        TxtWonder.Text = GetText(Resource.String.Lbl_wondered);
                    }
                    else
                    {
                        BtnWonder.Tag = "false";
                        ImgWonder.SetColorFilter(Color.White);
                        ImgWoWonder.SetColorFilter(Color.White); 
                        TxtWonder.Text = GetText(Resource.String.Btn_Wonder);

                    }
                      
                    TxtCountWoWonder.Text = Methods.FunString.FormatPriceValue(int.Parse(AlbumData.PostWonders));

                    if (AppSettings.PostButton == PostButtonSystem.ReactionDefault || AppSettings.PostButton == PostButtonSystem.ReactionSubShine)
                    {
                        AlbumData.Reaction ??= new WoWonderClient.Classes.Posts.Reaction();

                        TxtCountLike.Text = Methods.FunString.FormatPriceValue(AlbumData.Reaction.Count);

                        if ((bool)(AlbumData.Reaction != null & AlbumData.Reaction?.IsReacted))
                        { 
                            if (!string.IsNullOrEmpty(AlbumData.Reaction.Type))
                            {
                                switch (AlbumData.Reaction.Type)
                                {
                                    case "1":
                                    case "Like":
                                        LikeButton.SetReactionPack(ReactConstants.Like);
                                        break;
                                    case "2":
                                    case "Love":
                                        LikeButton.SetReactionPack(ReactConstants.Love);
                                        break;
                                    case "3":
                                    case "HaHa":
                                        LikeButton.SetReactionPack(ReactConstants.HaHa);
                                        break;
                                    case "4":
                                    case "Wow":
                                        LikeButton.SetReactionPack(ReactConstants.Wow);
                                        break;
                                    case "5":
                                    case "Sad":
                                        LikeButton.SetReactionPack(ReactConstants.Sad);
                                        break;
                                    case "6":
                                    case "Angry":
                                        LikeButton.SetReactionPack(ReactConstants.Angry);
                                        break;
                                    default:
                                        LikeButton.SetReactionPack(ReactConstants.Default);
                                        break;
                                } 
                            }
                        }
                        else
                        {
                            LikeButton.SetDefaultReaction(XReactions.GetDefaultReact());
                            LikeButton.SetTextColor(Color.White);
                        }
                    }
                    else 
                    {
                        if (AlbumData.IsLiked != null && AlbumData.IsLiked.Value)
                            LikeButton.SetReactionPack(ReactConstants.Like);

                        TxtCountLike.Text = Methods.FunString.FormatPriceValue(int.Parse(AlbumData.PostLikes));
                         
                        switch (AppSettings.PostButton)
                        {
                            case PostButtonSystem.Wonder when AlbumData.IsWondered != null && AlbumData.IsWondered.Value:
                                ImgWonder.SetImageResource(Resource.Drawable.ic_action_wowonder);
                                ImgWonder.SetColorFilter(Color.ParseColor(AppSettings.MainColor));

                                TxtWonder.Text = GetString(Resource.String.Lbl_wondered);
                                TxtWonder.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                                break;
                            case PostButtonSystem.Wonder:
                                ImgWonder.SetImageResource(Resource.Drawable.ic_action_wowonder);
                                ImgWonder.SetColorFilter(Color.White);

                                TxtWonder.Text = GetString(Resource.String.Btn_Wonder);
                                TxtWonder.SetTextColor(Color.ParseColor("#444444"));
                                break;
                            case PostButtonSystem.DisLike when AlbumData.IsWondered != null && AlbumData.IsWondered.Value:
                                ImgWonder.SetImageResource(Resource.Drawable.ic_action_dislike);
                                ImgWonder.SetColorFilter(Color.ParseColor(AppSettings.MainColor));

                                TxtWonder.Text = GetString(Resource.String.Lbl_disliked);
                                TxtWonder.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                                break;
                            case PostButtonSystem.DisLike:
                                ImgWonder.SetImageResource(Resource.Drawable.ic_action_dislike);
                                ImgWonder.SetColorFilter(Color.White);

                                TxtWonder.Text = GetString(Resource.String.Btn_Dislike);
                                TxtWonder.SetTextColor(Color.ParseColor("#444444"));
                                break;
                        } 
                    }
                } 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
 
    }
}