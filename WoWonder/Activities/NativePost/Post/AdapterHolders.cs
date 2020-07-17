using Android.App;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using AT.Markushi.UI;
using Com.Luseen.Autolinklibrary;
using Refractored.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Android.Content;
using Android.Util;
using Newtonsoft.Json;
using WoWonder.Activities.AddPost;
using WoWonder.Activities.Communities.Groups;
using WoWonder.Activities.Communities.Pages;
using WoWonder.Activities.NativePost.Extra;
using WoWonder.Activities.Search;
using WoWonder.Activities.SearchForPosts;
using WoWonder.Activities.Story.Adapters;
using WoWonder.Activities.Suggested.Adapters;
using WoWonder.Activities.Suggested.Groups;
using WoWonder.Activities.Suggested.User;
using WoWonder.Activities.Tabbes;
using WoWonder.Activities.UserProfile.Adapters;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo;
using WoWonder.Library.Anjo.SuperTextLibrary;
using WoWonder.Library.UI;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Requests;
using Xamarin.Facebook.Ads;
using Exception = System.Exception;

namespace WoWonder.Activities.NativePost.Post
{
    public class AdapterHolders
    {
        public class PostTopSharedSectionViewHolder : RecyclerView.ViewHolder, View.IOnClickListener
        {
            public View MainView { get; private set; }
            private readonly NativePostAdapter PostAdapter;
            private readonly PostClickListener PostClickListener;
             
            public AppCompatTextView Username { get; private set; }

            public AppCompatTextView TimeText { get; private set; }
            public ImageView PrivacyPostIcon { get; private set; }
            public ImageView UserAvatar { get; private set; }
            public PostTopSharedSectionViewHolder(View itemView, NativePostAdapter postAdapter, PostClickListener postClickListener) : base(itemView)
            {
                try
                {
                    MainView = itemView;

                    Username = itemView.FindViewById<AppCompatTextView>(Resource.Id.username);
                    TimeText = itemView.FindViewById<AppCompatTextView>(Resource.Id.time_text);
                    PrivacyPostIcon = itemView.FindViewById<ImageView>(Resource.Id.privacyPost);
                    UserAvatar = itemView.FindViewById<ImageView>(Resource.Id.userAvatar);

                    PostAdapter = postAdapter;
                    PostClickListener = postClickListener;

                    Username.SetOnClickListener(this);
                    UserAvatar.SetOnClickListener(this); 
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }

            public void OnClick(View v)
            {
                try
                {
                    if (AdapterPosition != RecyclerView.NoPosition)
                    {
                        var item = PostAdapter.ListDiffer[AdapterPosition]?.PostData;

                        if (v.Id == Username.Id)
                            PostClickListener.ProfilePostClick(new ProfileClickEventArgs { NewsFeedClass = item, Position = AdapterPosition, View = MainView }, "NewsFeedClass", "Username");
                        else if (v.Id == UserAvatar.Id) 
                            PostClickListener.ProfilePostClick(new ProfileClickEventArgs { NewsFeedClass = item, Position = AdapterPosition, View = MainView }, "NewsFeedClass", "UserAvatar");
                    } 
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }
        }

        public class PostTopSectionViewHolder : RecyclerView.ViewHolder, View.IOnClickListener
        {
            public View MainView { get; private set; }
            private readonly NativePostAdapter PostAdapter;
            private readonly PostClickListener PostClickListener;

            public TextViewWithImages Username { get;  set; }

            public AppCompatTextView TimeText { get; set; }
            public ImageView PrivacyPostIcon { get;  set; }
            public ImageView MoreIcon { get; private set; }
            public ImageView UserAvatar { get; set; }
          
            public PostTopSectionViewHolder(View itemView, NativePostAdapter postAdapter ,  PostClickListener postClickListener) : base(itemView)
            {
                try
                {
                    MainView = itemView;

                    Username = itemView.FindViewById<TextViewWithImages>(Resource.Id.username);
                    TimeText = itemView.FindViewById<AppCompatTextView>(Resource.Id.time_text);
                    PrivacyPostIcon = itemView.FindViewById<ImageView>(Resource.Id.privacyPost);
                    UserAvatar = itemView.FindViewById<ImageView>(Resource.Id.userAvatar);
                    MoreIcon = itemView.FindViewById<ImageView>(Resource.Id.moreicon);

                    PostAdapter = postAdapter;
                    PostClickListener = postClickListener;

                    Username.SetOnClickListener(this);
                    UserAvatar.SetOnClickListener(this);
                    MoreIcon.SetOnClickListener(this);

                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }

            public void OnClick(View v)
            {
                try
                {
                    if (AdapterPosition != RecyclerView.NoPosition)
                    {
                        var item = PostAdapter.ListDiffer[AdapterPosition]?.PostData;

                        if (v.Id == Username.Id)
                            PostClickListener.ProfilePostClick(new ProfileClickEventArgs { NewsFeedClass = item, Position = AdapterPosition, View = MainView }, "NewsFeedClass", "Username");
                        else if (v.Id == UserAvatar.Id)
                            PostClickListener.ProfilePostClick(new ProfileClickEventArgs { NewsFeedClass = item, Position = AdapterPosition, View = MainView }, "NewsFeedClass", "UserAvatar");
                        else if (v.Id == MoreIcon.Id)
                            PostClickListener.MorePostIconClick(new GlobalClickEventArgs { NewsFeedClass = item, Position = AdapterPosition, View = MainView });
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            } 
        }

        public class PostTextSectionViewHolder : RecyclerView.ViewHolder
        {
            public SuperTextView Description { get; private set; }

            public PostTextSectionViewHolder(View itemView) : base(itemView)
            {
                Description = itemView.FindViewById<SuperTextView>(Resource.Id.description);
                Description?.SetTextInfo(Description);
            }
        }

        public class PostPrevBottomSectionViewHolder : RecyclerView.ViewHolder, View.IOnClickListener
        {
            public View MainView { get; private set; }
            private readonly NativePostAdapter PostAdapter;
            private readonly PostClickListener PostClickListener;

            public TextView ShareCount { get; private set; }
            public TextView CommentCount { get; private set; }
            public TextView LikeCount { get; private set; }
            public TextView ViewCount { get; private set; }

            public PostPrevBottomSectionViewHolder(View itemView, NativePostAdapter postAdapter, PostClickListener postClickListener) : base(itemView)
            {
                try
                {
                    MainView = itemView;

                    ShareCount = itemView.FindViewById<TextView>(Resource.Id.Sharecount);
                    CommentCount = itemView.FindViewById<TextView>(Resource.Id.Commentcount);
                    LikeCount = itemView.FindViewById<TextView>(Resource.Id.Likecount);
                    ViewCount = itemView.FindViewById<TextView>(Resource.Id.viewcount);

                    if (!AppSettings.ShowCountSharePost)
                        ShareCount.Visibility = ViewStates.Gone;
                     
                    PostAdapter = postAdapter;
                    PostClickListener = postClickListener;

                    LikeCount.SetOnClickListener(this);
                    CommentCount.SetOnClickListener(this);
                    ShareCount.SetOnClickListener(this);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }

            public void OnClick(View v)
            {
                try
                {
                    if (AdapterPosition != RecyclerView.NoPosition)
                    {
                        var item = PostAdapter.ListDiffer[AdapterPosition]?.PostData;
                        var postType = PostFunctions.GetAdapterType(item);

                        if (v.Id == LikeCount.Id)
                            PostClickListener.DataItemPostClick(new GlobalClickEventArgs { NewsFeedClass = item, Position = AdapterPosition, View = MainView });
                        else if (v.Id == CommentCount.Id)
                            PostClickListener.CommentPostClick(new GlobalClickEventArgs { NewsFeedClass = item, Position = AdapterPosition, View = MainView });
                        else if (v.Id == ShareCount.Id)
                            PostClickListener.SharePostClick(new GlobalClickEventArgs { NewsFeedClass = item, Position = AdapterPosition, View = MainView }, postType);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }

        }
         
        public class PostBottomSectionViewHolder : RecyclerView.ViewHolder, View.IOnClickListener, View.IOnLongClickListener
        {
            public View MainView { get; private set; }
            private readonly NativePostAdapter PostAdapter;
            private readonly PostClickListener PostClickListener;
            public LinearLayout MainSectionButton { get; private set; }
            public LinearLayout ShareLinearLayout { get; private set; }
            public LinearLayout CommentLinearLayout { get; private set; }
            public LinearLayout SecondReactionLinearLayout { get; set; }
            public ReactButton LikeButton { get; private set; }

            public TextView SecondReactionButton { get; private set; }

            public PostBottomSectionViewHolder(View itemView, NativePostAdapter postAdapter, PostClickListener postClickListener) : base(itemView)
            {
                try
                {
                    MainView = itemView;

                    ShareLinearLayout = itemView.FindViewById<LinearLayout>(Resource.Id.ShareLinearLayout);
                    CommentLinearLayout = itemView.FindViewById<LinearLayout>(Resource.Id.CommentLinearLayout);
                    SecondReactionLinearLayout = itemView.FindViewById<LinearLayout>(Resource.Id.SecondReactionLinearLayout);
                    LikeButton = itemView.FindViewById<ReactButton>(Resource.Id.beactButton);

                    SecondReactionButton = itemView.FindViewById<TextView>(Resource.Id.SecondReactionText);

                    if (!AppSettings.ShowShareButton)
                        ShareLinearLayout.Visibility = ViewStates.Gone;

                    MainSectionButton = itemView.FindViewById<LinearLayout>(Resource.Id.linerSecondReaction);
                    if (AppSettings.PostButton == PostButtonSystem.ReactionDefault || AppSettings.PostButton == PostButtonSystem.ReactionSubShine || AppSettings.PostButton == PostButtonSystem.Like)
                    {
                        MainSectionButton.WeightSum = AppSettings.ShowShareButton ? 3 : 2;

                        SecondReactionLinearLayout.Visibility = ViewStates.Gone;
                    }
                    else if (AppSettings.PostButton == PostButtonSystem.Wonder)
                    {
                        MainSectionButton.WeightSum = AppSettings.ShowShareButton ? 4 : 3;

                        SecondReactionLinearLayout.Visibility = ViewStates.Visible;

                        SecondReactionButton.SetCompoundDrawablesWithIntrinsicBounds(Resource.Drawable.icon_post_wonder_vector, 0, 0, 0);
                        SecondReactionButton.Text = Application.Context.GetText(Resource.String.Btn_Wonder);
                    }
                    else if (AppSettings.PostButton == PostButtonSystem.DisLike)
                    {
                        MainSectionButton.WeightSum = AppSettings.ShowShareButton ? 4 : 3;

                        SecondReactionLinearLayout.Visibility = ViewStates.Visible;
                        SecondReactionButton.SetCompoundDrawablesWithIntrinsicBounds(Resource.Drawable.ic_action_dislike, 0, 0, 0);
                        SecondReactionButton.Text = Application.Context.GetText(Resource.String.Btn_Dislike);
                    }

                    PostAdapter = postAdapter;
                    PostClickListener = postClickListener;

                    LikeButton.SetOnClickListener(this);
                    LikeButton.SetOnLongClickListener(this);
                    CommentLinearLayout.SetOnClickListener(this);
                    ShareLinearLayout.SetOnClickListener(this);
                    SecondReactionButton.SetOnClickListener(this); 
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }

            public void OnClick(View v)
            {
                try
                {
                    if (AdapterPosition != RecyclerView.NoPosition)
                    {
                        var item = PostAdapter.ListDiffer[AdapterPosition]?.PostData;

                        var postType = PostFunctions.GetAdapterType(item);

                        if (v.Id == LikeButton.Id)
                            LikeButton.ClickLikeAndDisLike(new GlobalClickEventArgs { NewsFeedClass = item, Position = AdapterPosition, View = MainView }, PostAdapter);
                        else if (v.Id == CommentLinearLayout.Id)
                            PostClickListener.CommentPostClick(new GlobalClickEventArgs { NewsFeedClass = item, Position = AdapterPosition, View = MainView });
                        else if (v.Id == ShareLinearLayout.Id)
                            PostClickListener.SharePostClick(new GlobalClickEventArgs { NewsFeedClass = item, Position = AdapterPosition, View = MainView }, postType); 
                        else if (v.Id == SecondReactionButton.Id)
                            PostClickListener.SecondReactionButtonClick(new GlobalClickEventArgs { NewsFeedClass = item, Position = AdapterPosition, View = MainView });
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }

            public bool OnLongClick(View v)
            {
                //add event if System = ReactButton 
                if (AppSettings.PostButton == PostButtonSystem.ReactionDefault || AppSettings.PostButton == PostButtonSystem.ReactionSubShine)
                {
                    var item = PostAdapter.ListDiffer[AdapterPosition]?.PostData;

                    if (LikeButton.Id == v.Id)
                        LikeButton.LongClickDialog(new GlobalClickEventArgs() { NewsFeedClass = item, Position = AdapterPosition, View = MainView },PostAdapter);
                }

                return true;
            } 
        }

        public class PostImageSectionViewHolder : RecyclerView.ViewHolder, View.IOnClickListener
        {
            public View MainView { get; private set; }
            private readonly NativePostAdapter PostAdapter;
            private readonly PostClickListener PostClickListener;
            private readonly int ClickType;

            public ImageView Image { get; set; }

            public PostImageSectionViewHolder(View itemView, NativePostAdapter postAdapter, PostClickListener postClickListener , int clickType) : base(itemView)
            {
                try
                {
                    MainView = itemView;
                    ClickType = clickType;

                    itemView.SetLayerType(LayerType.Hardware, null);
                    Image = itemView.FindViewById<ImageView>(Resource.Id.Image);

                    PostAdapter = postAdapter;
                    PostClickListener = postClickListener;

                    Image.SetOnClickListener(this);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }

            public void OnClick(View v)
            {
                try
                {
                    if (AdapterPosition != RecyclerView.NoPosition)
                    {
                        var item = PostAdapter.ListDiffer[AdapterPosition]?.PostData;

                        if (v.Id == Image.Id)
                        {
                            switch (ClickType)
                            {
                                case (int)PostModelType.MapPost:
                                    PostClickListener.MapPostClick(new GlobalClickEventArgs { NewsFeedClass = item, Position = AdapterPosition, View = MainView });
                                    break;
                                case (int)PostModelType.ImagePost:
                                case (int)PostModelType.StickerPost:
                                     PostClickListener.SingleImagePostClick(new GlobalClickEventArgs { NewsFeedClass = item, Position = AdapterPosition, View = MainView });
                                    break;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            } 
        }

        public class Post2ImageSectionViewHolder : RecyclerView.ViewHolder, View.IOnClickListener
        {
            public View MainView { get; private set; }
            private readonly NativePostAdapter PostAdapter;
            private readonly PostClickListener PostClickListener;

            public ImageView Image { get; private set; }
            public ImageView Image2 { get; private set; }
          
            public Post2ImageSectionViewHolder(View itemView, NativePostAdapter postAdapter, PostClickListener postClickListener) : base(itemView)
            {
                try
                {
                    MainView = itemView;

                    itemView.SetLayerType(LayerType.Hardware, null);
                    Image = itemView.FindViewById<ImageView>(Resource.Id.image);
                    Image2 = itemView.FindViewById<ImageView>(Resource.Id.image2);

                    PostAdapter = postAdapter;
                    PostClickListener = postClickListener;

                    Image.SetOnClickListener(this);
                    Image2.SetOnClickListener(this);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }

            public void OnClick(View v)
            {
                try
                {
                    if (AdapterPosition != RecyclerView.NoPosition)
                    {
                        var item = PostAdapter.ListDiffer[AdapterPosition]?.PostData;

                        if (v.Id == Image.Id)
                            PostClickListener.ImagePostClick(new GlobalClickEventArgs { NewsFeedClass = item, Position = 0, View = MainView });
                        if (v.Id == Image2.Id)
                            PostClickListener.ImagePostClick(new GlobalClickEventArgs { NewsFeedClass = item, Position = 1, View = MainView });
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            } 
        }

        public class Post3ImageSectionViewHolder : RecyclerView.ViewHolder, View.IOnClickListener
        {
            public View MainView { get; private set; }
            private readonly NativePostAdapter PostAdapter;
            private readonly PostClickListener PostClickListener;

            public ImageView Image { get; private set; }
            public ImageView Image2 { get; private set; }
            public ImageView Image3 { get; private set; }
          
            public Post3ImageSectionViewHolder(View itemView, NativePostAdapter postAdapter, PostClickListener postClickListener) : base(itemView)
            {
                try
                {
                    MainView = itemView;

                    itemView.SetLayerType(LayerType.Hardware, null);
                    Image = itemView.FindViewById<ImageView>(Resource.Id.image);
                    Image2 = itemView.FindViewById<ImageView>(Resource.Id.image2);
                    Image3 = itemView.FindViewById<ImageView>(Resource.Id.image3);

                    PostAdapter = postAdapter;
                    PostClickListener = postClickListener;

                    Image.SetOnClickListener(this);
                    Image2.SetOnClickListener(this);
                    Image3.SetOnClickListener(this);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }
            public void OnClick(View v)
            {
                try
                {
                    if (AdapterPosition != RecyclerView.NoPosition)
                    {
                        var item = PostAdapter.ListDiffer[AdapterPosition]?.PostData;

                        if (v.Id == Image.Id)
                            PostClickListener.ImagePostClick(new GlobalClickEventArgs { NewsFeedClass = item, Position = 0, View = MainView });
                        if (v.Id == Image2.Id)
                            PostClickListener.ImagePostClick(new GlobalClickEventArgs { NewsFeedClass = item, Position = 1, View = MainView });
                        if (v.Id == Image3.Id)
                            PostClickListener.ImagePostClick(new GlobalClickEventArgs { NewsFeedClass = item, Position = 2, View = MainView });
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }
        }

        public class Post4ImageSectionViewHolder : RecyclerView.ViewHolder, View.IOnClickListener
        {
            public View MainView { get; private set; }
            private readonly NativePostAdapter PostAdapter;
            private readonly PostClickListener PostClickListener;

            public ImageView Image { get; private set; }
            public ImageView Image2 { get; private set; }
            public ImageView Image3 { get; private set; }
            public ImageView Image4 { get; private set; }
         
            public Post4ImageSectionViewHolder(View itemView, NativePostAdapter postAdapter, PostClickListener postClickListener) : base(itemView)
            {
                try
                {
                    MainView = itemView;

                    itemView.SetLayerType(LayerType.Hardware, null);
                    Image = itemView.FindViewById<ImageView>(Resource.Id.image);
                    Image2 = itemView.FindViewById<ImageView>(Resource.Id.image2);
                    Image3 = itemView.FindViewById<ImageView>(Resource.Id.image3);
                    Image4 = itemView.FindViewById<ImageView>(Resource.Id.image4);

                    PostAdapter = postAdapter;
                    PostClickListener = postClickListener;

                    Image.SetOnClickListener(this);
                    Image2.SetOnClickListener(this);
                    Image3.SetOnClickListener(this);
                    Image4.SetOnClickListener(this);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }

            public void OnClick(View v)
            {
                try
                {
                    if (AdapterPosition != RecyclerView.NoPosition)
                    {
                        var item = PostAdapter.ListDiffer[AdapterPosition]?.PostData;

                        if (v.Id == Image.Id)
                            PostClickListener.ImagePostClick(new GlobalClickEventArgs { NewsFeedClass = item, Position = 0, View = MainView });
                        if (v.Id == Image2.Id)
                            PostClickListener.ImagePostClick(new GlobalClickEventArgs { NewsFeedClass = item, Position = 1, View = MainView });
                        if (v.Id == Image3.Id)
                            PostClickListener.ImagePostClick(new GlobalClickEventArgs { NewsFeedClass = item, Position = 2, View = MainView });
                        if (v.Id == Image4.Id)
                            PostClickListener.ImagePostClick(new GlobalClickEventArgs { NewsFeedClass = item, Position = 3, View = MainView });
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            } 
        }

        public class PostMultiImagesViewHolder : RecyclerView.ViewHolder, View.IOnClickListener
        {
            public View MainView { get; private set; }
            private readonly NativePostAdapter PostAdapter;
            private readonly PostClickListener PostClickListener;

            public ImageView Image { get; private set; }
            public ImageView Image2 { get; private set; }
            public ImageView Image3 { get; private set; }
            public TextView CountImageLabel { get; private set; }

            public PostMultiImagesViewHolder(View itemView, NativePostAdapter postAdapter, PostClickListener postClickListener) : base(itemView)
            {
                try
                {
                    MainView = itemView;
                    Image = itemView.FindViewById<ImageView>(Resource.Id.image);
                    Image2 = itemView.FindViewById<ImageView>(Resource.Id.image2);
                    Image3 = itemView.FindViewById<ImageView>(Resource.Id.image3);
                    CountImageLabel = itemView.FindViewById<TextView>(Resource.Id.counttext);

                    PostAdapter = postAdapter;
                    PostClickListener = postClickListener;

                    Image.SetOnClickListener(this);
                    Image2.SetOnClickListener(this);
                    Image3.SetOnClickListener(this);
                    CountImageLabel.SetOnClickListener(this);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }

            public void OnClick(View v)
            {
                try
                {
                    if (AdapterPosition != RecyclerView.NoPosition)
                    {
                        var item = PostAdapter.ListDiffer[AdapterPosition]?.PostData;

                        if (v.Id == Image.Id)
                            PostClickListener.ImagePostClick(new GlobalClickEventArgs { NewsFeedClass = item, Position = 0, View = MainView });
                        if (v.Id == Image2.Id)
                            PostClickListener.ImagePostClick(new GlobalClickEventArgs { NewsFeedClass = item, Position = 1, View = MainView });
                        if (v.Id == Image3.Id)
                            PostClickListener.ImagePostClick(new GlobalClickEventArgs { NewsFeedClass = item, Position = 2, View = MainView });
                        if (v.Id == CountImageLabel.Id)
                            PostClickListener.ImagePostClick(new GlobalClickEventArgs { NewsFeedClass = item, Position = 4, View = MainView });
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            } 
        }
        
        public class PostVideoSectionViewHolder : RecyclerView.ViewHolder, View.IOnClickListener
        {
            public View MainView { get; private set; }
            private readonly NativePostAdapter PostAdapter;

            public ImageView PlayControl { get; private set; }
            public ImageView VideoImage { get; private set; }
            public ProgressBar VideoProgressBar { get; private set; }
            public FrameLayout MediaContainer { get; private set; }
            public string VideoUrl { get; set; }
            public PostVideoSectionViewHolder(View itemView , NativePostAdapter postAdapter) : base(itemView)
            {
                try
                {
                    MainView = itemView;

                    itemView.Tag = this;

                    //itemView.SetLayerType(LayerType.Hardware, null);
                    MediaContainer = itemView.FindViewById<FrameLayout>(Resource.Id.media_container);
                    VideoImage = itemView.FindViewById<ImageView>(Resource.Id.image);
                    PlayControl = itemView.FindViewById<ImageView>(Resource.Id.Play_control);
                    VideoProgressBar = itemView.FindViewById<ProgressBar>(Resource.Id.progressBar);

                    PostAdapter = postAdapter;

                    PlayControl.SetOnClickListener(this);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }
            public void OnClick(View v)
            {
                try
                {
                    if (AdapterPosition != RecyclerView.NoPosition)
                    {
                        var item = PostAdapter.ListDiffer[AdapterPosition]?.PostData;

                        if (v.Id == PlayControl.Id)
                            WRecyclerView.GetInstance()?.PlayVideo(!WRecyclerView.GetInstance().CanScrollVertically(1), this, item);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }

        }

        public class PostBlogSectionViewHolder : RecyclerView.ViewHolder, View.IOnClickListener
        {
            public View MainView { get; private set; }
            private readonly NativePostAdapter PostAdapter;
            private readonly PostClickListener PostClickListener;

            public ImageView ImageBlog { get; private set; }
            public ImageView BlogIcon { get; private set; }
            public TextView PostBlogText { get; private set; }
            public TextView PostBlogContent { get; private set; }
            public TextView CatText { get; private set; }
            public RelativeLayout PostLinkLinearLayout { get; private set; }

            public PostBlogSectionViewHolder(View itemView, NativePostAdapter postAdapter, PostClickListener postClickListener) : base(itemView)
            {
                try
                {
                    MainView = itemView;

                    ImageBlog = itemView.FindViewById<ImageView>(Resource.Id.imageblog);
                    BlogIcon = itemView.FindViewById<ImageView>(Resource.Id.blogIcon);
                    PostBlogText = itemView.FindViewById<TextView>(Resource.Id.postblogText);
                    PostBlogContent = itemView.FindViewById<TextView>(Resource.Id.postBlogContent);
                    CatText = itemView.FindViewById<TextView>(Resource.Id.catText);
                    PostLinkLinearLayout = itemView.FindViewById<RelativeLayout>(Resource.Id.linklinearLayout);

                    PostAdapter = postAdapter;
                    PostClickListener = postClickListener;

                    PostLinkLinearLayout.SetOnClickListener(this);
                    PostBlogText.SetOnClickListener(this);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }
            public void OnClick(View v)
            {
                try
                {
                    if (AdapterPosition != RecyclerView.NoPosition)
                    {
                        var item = PostAdapter.ListDiffer[AdapterPosition]?.PostData;

                        if (v.Id == PostLinkLinearLayout.Id || v.Id == PostBlogText.Id)
                            PostClickListener.ArticleItemPostClick(item?.Blog);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }

        }

        public class PostColorBoxSectionViewHolder : RecyclerView.ViewHolder
        {
            public SuperTextView DesTextView { get; private set; }
            public ImageView ColorBoxImage { get; private set; }
            public PostColorBoxSectionViewHolder(View itemView) : base(itemView)
            {
                try
                {
                    DesTextView = itemView.FindViewById<SuperTextView>(Resource.Id.description);
                    ColorBoxImage = itemView.FindViewById<ImageView>(Resource.Id.Image);
                    DesTextView?.SetTextInfo(DesTextView);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }
        }

        public class EventPostViewHolder : RecyclerView.ViewHolder, View.IOnClickListener
        {
            public View MainView { get; private set; }
            private readonly NativePostAdapter PostAdapter;
            private readonly PostClickListener PostClickListener;

            public ImageView Image { get; private set; }
            public AppCompatTextView TxtEventTitle { get; private set; }
            public AppCompatTextView TxtEventDescription { get; private set; }
            public TextView TxtEventTime { get; private set; }
            public AppCompatTextView TxtEventLocation { get; private set; }
            public RelativeLayout PostLinkLinearLayout { get; private set; }
             
            public EventPostViewHolder(View itemView, NativePostAdapter postAdapter, PostClickListener postClickListener) : base(itemView)
            {
                try
                {
                    MainView = itemView;

                    Image = itemView.FindViewById<ImageView>(Resource.Id.Image);
                    TxtEventTitle = itemView.FindViewById<AppCompatTextView>(Resource.Id.event_titile);
                    TxtEventDescription = itemView.FindViewById<AppCompatTextView>(Resource.Id.event_description);
                    TxtEventTime = itemView.FindViewById<TextView>(Resource.Id.event_time);
                    TxtEventLocation = itemView.FindViewById<AppCompatTextView>(Resource.Id.event_location);
                    PostLinkLinearLayout = itemView.FindViewById<RelativeLayout>(Resource.Id.linklinearLayout);

                    PostAdapter = postAdapter;
                    PostClickListener = postClickListener;

                    PostLinkLinearLayout.SetOnClickListener(this);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }
            public void OnClick(View v)
            {
                try
                {
                    if (AdapterPosition != RecyclerView.NoPosition)
                    {
                        var item = PostAdapter.ListDiffer[AdapterPosition]?.PostData;

                        if (v.Id == PostLinkLinearLayout.Id)
                            PostClickListener.EventItemPostClick(new GlobalClickEventArgs() { NewsFeedClass = item, Position = AdapterPosition, View = MainView });
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }

        }

        public class LinkPostViewHolder : RecyclerView.ViewHolder, View.IOnClickListener
        {
            public View MainView { get; private set; }
            private readonly NativePostAdapter PostAdapter;
            private readonly PostClickListener PostClickListener;

            public ImageView Image { get; private set; }
            public TextView LinkUrl { get; private set; }
            public TextView PostLinkTitle { get; private set; }
            public TextView PostLinkContent { get; private set; }
            public LinearLayout PostLinkLinearLayout { get; private set; }

            public LinkPostViewHolder(View itemView, NativePostAdapter postAdapter, PostClickListener postClickListener) : base(itemView)
            {
                try
                {
                    MainView = itemView;

                    Image = itemView.FindViewById<ImageView>(Resource.Id.image);
                    LinkUrl = itemView.FindViewById<TextView>(Resource.Id.linkUrl);
                    PostLinkTitle = itemView.FindViewById<TextView>(Resource.Id.postLinkTitle);
                    PostLinkContent = itemView.FindViewById<TextView>(Resource.Id.postLinkContent);
                    PostLinkLinearLayout = itemView.FindViewById<LinearLayout>(Resource.Id.linklinearLayout);

                    PostAdapter = postAdapter;
                    PostClickListener = postClickListener;

                    PostLinkLinearLayout.SetOnClickListener(this);

                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }
            public void OnClick(View v)
            {
                try
                {
                    if (AdapterPosition != RecyclerView.NoPosition)
                    {
                        var item = PostAdapter.ListDiffer[AdapterPosition]?.PostData;

                        if (v.Id == PostLinkLinearLayout.Id)
                            PostClickListener.LinkPostClick(new GlobalClickEventArgs() { NewsFeedClass = item, Position = AdapterPosition, View = MainView }, "LinkPost");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }

        }

        public class FilePostViewHolder : RecyclerView.ViewHolder, View.IOnClickListener
        {
            public View MainView { get; private set; }
            private readonly NativePostAdapter PostAdapter;
            private readonly PostClickListener PostClickListener;

            public CircleButton DownlandButton { get; private set; }
            public TextView PostFileText { get; private set; }
            public ImageView FileIcon { get; private set; }

            public FilePostViewHolder(View itemView, NativePostAdapter postAdapter, PostClickListener postClickListener) : base(itemView)
            {
                try
                {
                    MainView = itemView;
                    DownlandButton = itemView.FindViewById<CircleButton>(Resource.Id.downlaodButton);
                    PostFileText = itemView.FindViewById<TextView>(Resource.Id.postfileText);
                    FileIcon = itemView.FindViewById<ImageView>(Resource.Id.fileIcon);

                    PostAdapter = postAdapter;
                    PostClickListener = postClickListener;

                    DownlandButton.SetOnClickListener(this);
                    PostFileText.SetOnClickListener(this);
                    FileIcon.SetOnClickListener(this);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }

            public void OnClick(View v)
            {
                try
                {
                    if (AdapterPosition != RecyclerView.NoPosition)
                    {
                        var item = PostAdapter.ListDiffer[AdapterPosition]?.PostData;

                        if (v.Id == DownlandButton.Id)
                            PostClickListener.FileDownloadPostClick(new GlobalClickEventArgs() { NewsFeedClass = item, Position = AdapterPosition, View = MainView });
                        else if (v.Id == PostFileText.Id || v.Id == FileIcon.Id)
                            PostClickListener.OpenFilePostClick(new GlobalClickEventArgs() { NewsFeedClass = item, Position = AdapterPosition, View = MainView });
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }

        }

        public class FundingPostViewHolder : RecyclerView.ViewHolder, View.IOnClickListener
        {
            public View MainView { get; private set; }
            private readonly NativePostAdapter PostAdapter;
            private readonly PostClickListener PostClickListener;
             
            public ImageView Image { get; private set; }
            public TextView Title { get; private set; }
            public TextView Description { get; private set; }
            public ProgressBar Progress { get; private set; }
            public TextView TottalAmount { get; private set; }
            public TextView Raised { get; private set; }
            public TextView DonationTime { get; private set; }
            public RelativeLayout MainCardView { get; private set; }

            public FundingPostViewHolder(View itemView, NativePostAdapter postAdapter, PostClickListener postClickListener) : base(itemView)
            {
                try
                {
                    MainView = itemView;

                    MainCardView = itemView.FindViewById<RelativeLayout>(Resource.Id.info_container);
                    Image = itemView.FindViewById<ImageView>(Resource.Id.fundImage);
                    Title = itemView.FindViewById<TextView>(Resource.Id.fundTitle);
                    Description = itemView.FindViewById<TextView>(Resource.Id.fundDescription);
                    Progress = itemView.FindViewById<ProgressBar>(Resource.Id.progressBar);
                    TottalAmount = itemView.FindViewById<TextView>(Resource.Id.TottalAmount);
                    Raised = itemView.FindViewById<TextView>(Resource.Id.raised);
                    DonationTime = itemView.FindViewById<TextView>(Resource.Id.time);

                    PostAdapter = postAdapter;
                    PostClickListener = postClickListener;

                    MainCardView.SetOnClickListener(this);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }
            public void OnClick(View v)
            {
                try
                {
                    if (AdapterPosition != RecyclerView.NoPosition)
                    {
                        var item = PostAdapter.ListDiffer[AdapterPosition]?.PostData;

                        if (v.Id == MainCardView.Id)
                            PostClickListener.OpenFundingPostClick(new GlobalClickEventArgs() { NewsFeedClass = item, Position = AdapterPosition, View = MainView });
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }

        }

        public class SoundPostViewHolder : RecyclerView.ViewHolder, View.IOnClickListener
        {
            public View MainView { get; private set; }
            private readonly NativePostAdapter PostAdapter;
            private readonly PostClickListener PostClickListener;

            public SeekBar SeekBar { get; private set; }
            public ImageView MoreIcon { get; private set; }
            public TextView Time { get; private set; }
            public TextView Duration { get; private set; }
            public ProgressBar LoadingProgressView { get; private set; }
            public CircleButton PlayButton { get; private set; }
            public Android.Media.MediaPlayer VoicePlayer;
            public SoundPostViewHolder(View itemView, NativePostAdapter postAdapter, PostClickListener postClickListener) : base(itemView)
            {
                try
                {
                    MainView = itemView;
                    SeekBar = itemView.FindViewById<SeekBar>(Resource.Id.seekBar);
                    MoreIcon = itemView.FindViewById<ImageView>(Resource.Id.moreicon2);
                    Time = itemView.FindViewById<TextView>(Resource.Id.time);
                    Duration = itemView.FindViewById<TextView>(Resource.Id.Duration);
                    LoadingProgressView = itemView.FindViewById<ProgressBar>(Resource.Id.loadingProgressview);
                    PlayButton = itemView.FindViewById<CircleButton>(Resource.Id.playButton);
                    PlayButton.Tag = "Play";

                    PostAdapter = postAdapter;
                    PostClickListener = postClickListener;

                    PlayButton.SetOnClickListener(this);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }
            public void OnClick(View v)
            {
                try
                {
                    if (AdapterPosition != RecyclerView.NoPosition)
                    {
                        var item = PostAdapter.ListDiffer[AdapterPosition]?.PostData;

                        if (v.Id == PlayButton.Id)
                            PostClickListener.VoicePostClick(new GlobalClickEventArgs() { NewsFeedClass = item, Position = AdapterPosition, View = MainView, HolderSound = this });
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }

        }

        public class YoutubePostViewHolder : RecyclerView.ViewHolder, View.IOnClickListener
        {
            public View MainView { get; private set; }
            private readonly NativePostAdapter PostAdapter;
            private readonly PostClickListener PostClickListener;

            public ImageView Image { get; private set; }
            public ImageView VideoIcon { get; private set; }
             
            public YoutubePostViewHolder(View itemView, NativePostAdapter postAdapter, PostClickListener postClickListener) : base(itemView)
            {
                try
                {
                    MainView = itemView;
                    Image = itemView.FindViewById<ImageView>(Resource.Id.image);
                    VideoIcon = itemView.FindViewById<ImageView>(Resource.Id.videoicon);

                    PostAdapter = postAdapter;
                    PostClickListener = postClickListener;

                    VideoIcon.SetOnClickListener(this);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }
            public void OnClick(View v)
            {
                try
                {
                    if (AdapterPosition != RecyclerView.NoPosition)
                    {
                        var item = PostAdapter.ListDiffer[AdapterPosition]?.PostData;

                        if (v.Id == VideoIcon.Id)
                            PostClickListener.YoutubePostClick(new GlobalClickEventArgs() { NewsFeedClass = item, Position = AdapterPosition, View = MainView });
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            } 
        }

        public class OfferPostViewHolder : RecyclerView.ViewHolder, View.IOnClickListener
        {
            public View MainView { get; private set; }
            private readonly NativePostAdapter PostAdapter;
            private readonly PostClickListener PostClickListener;

            public ImageView ImageBlog { get; private set; }
            public ImageView BlogIcon { get; private set; }
            public TextView PostBlogText { get; private set; }
            public TextView PostBlogContent { get; private set; }
            public TextView CatText { get; private set; }
            public RelativeLayout MainLayout { get; private set; }

            public OfferPostViewHolder(View itemView, NativePostAdapter postAdapter, PostClickListener postClickListener) : base(itemView)
            {
                try
                {
                    MainView = itemView;

                    MainLayout = itemView.FindViewById<RelativeLayout>(Resource.Id.mainLayout);
                    ImageBlog = itemView.FindViewById<ImageView>(Resource.Id.imageblog);
                    BlogIcon = itemView.FindViewById<ImageView>(Resource.Id.blogIcon);
                    PostBlogText = itemView.FindViewById<TextView>(Resource.Id.postblogText);
                    PostBlogContent = itemView.FindViewById<TextView>(Resource.Id.postBlogContent);
                    CatText = itemView.FindViewById<TextView>(Resource.Id.catText);

                    PostAdapter = postAdapter;
                    PostClickListener = postClickListener;

                    MainLayout.SetOnClickListener(this);

                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }
            public void OnClick(View v)
            {
                try
                {
                    if (AdapterPosition != RecyclerView.NoPosition)
                    {
                        var item = PostAdapter.ListDiffer[AdapterPosition]?.PostData;

                        if (v.Id == MainLayout.Id)
                            PostClickListener.OffersPostClick(new GlobalClickEventArgs() { NewsFeedClass = item, Position = AdapterPosition, View = MainView });
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            } 
        }

        public class PostPlayTubeContentViewHolder : RecyclerView.ViewHolder
        {
            public WoWebView WebView { get; private set; }

            public PostPlayTubeContentViewHolder(View itemView) : base(itemView)
            {
                WebView = itemView.FindViewById<WoWebView>(Resource.Id.webview);
                itemView.SetLayerType(LayerType.Hardware,null);
            } 
        }
      
        public class AdsPostViewHolder : RecyclerView.ViewHolder, View.IOnClickListener
        {
            public View MainView { get; private set; }
            private readonly NativePostAdapter PostAdapter;
            private readonly PostClickListener PostClickListener;
             
            public CircleImageView UserAvatar { get; private set; }
            public AppCompatTextView Username { get; private set; }
            public AppCompatTextView TimeText { get; private set; }
            public ImageView Image { get; private set; }
            public ImageView Moreicon { get; private set; }
            public SuperTextView Description { get; private set; }
            public TextView IconLocation { get; private set; }
            public TextView TextLocation { get; private set; }
            public TextView IconLink { get; private set; }
            public AutoLinkTextView Headline { get; private set; }
            public TextView LinkUrl { get; private set; }
            public TextView PostLinkTitle { get; private set; }
            public TextView PostLinkContent { get; private set; }
            public LinearLayout PostLinkLinearLayout { get; private set; }

            public AdsPostViewHolder(View itemView, NativePostAdapter postAdapter, PostClickListener postClickListener) : base(itemView)
            {
                try
                {
                    MainView = itemView;

                    UserAvatar = itemView.FindViewById<CircleImageView>(Resource.Id.userAvatar);
                    Username = itemView.FindViewById<AppCompatTextView>(Resource.Id.username);
                    TimeText = itemView.FindViewById<AppCompatTextView>(Resource.Id.time_text);
                    Moreicon = itemView.FindViewById<ImageView>(Resource.Id.moreicon);
                    Description = itemView.FindViewById<SuperTextView>(Resource.Id.description);
                    Description?.SetTextInfo(Description);

                    IconLocation = itemView.FindViewById<TextView>(Resource.Id.iconloca);
                    TextLocation = itemView.FindViewById<TextView>(Resource.Id.textloc);
                    IconLink = itemView.FindViewById<TextView>(Resource.Id.IconLink);
                    Headline = itemView.FindViewById<AutoLinkTextView>(Resource.Id.headline);
                    Image = itemView.FindViewById<ImageView>(Resource.Id.image);
                    LinkUrl = itemView.FindViewById<TextView>(Resource.Id.linkUrl);
                    PostLinkTitle = itemView.FindViewById<TextView>(Resource.Id.postLinkTitle);
                    PostLinkContent = itemView.FindViewById<TextView>(Resource.Id.postLinkContent);
                    PostLinkLinearLayout = itemView.FindViewById<LinearLayout>(Resource.Id.linklinearLayout);

                    PostLinkTitle.Visibility = ViewStates.Gone;
                    PostLinkContent.Visibility = ViewStates.Gone;

                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconLocation, IonIconsFonts.AndroidPin);
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconLink, IonIconsFonts.Link);

                    PostAdapter = postAdapter;
                    PostClickListener = postClickListener;

                    PostLinkLinearLayout.SetOnClickListener(this);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }
            public void OnClick(View v)
            {
                try
                {
                    if (AdapterPosition != RecyclerView.NoPosition)
                    {
                        var item = PostAdapter.ListDiffer[AdapterPosition]?.PostData;

                        if (v.Id == PostLinkLinearLayout.Id)
                            PostClickListener.LinkPostClick(new GlobalClickEventArgs() { NewsFeedClass = item, Position = AdapterPosition, View = MainView }, "AdsPost");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }

        }

        public class ProductPostViewHolder : RecyclerView.ViewHolder, View.IOnClickListener
        {
            public View MainView { get; private set; }
            private readonly NativePostAdapter PostAdapter;
            private readonly PostClickListener PostClickListener;

            public ImageView Image { get; private set; }
            public ImageView LocationIcon { get; private set; }
            public TextView PostProductLocationText { get; private set; }
            public TextView PostLinkTitle { get; private set; }
            public TextView PostProductContent { get; private set; }
            public TextView TypeText { get; private set; }
            public TextView PriceText { get; private set; }
            public TextView StatusText { get; private set; }
            public RelativeLayout PostLinkLinearLayout { get; private set; }

            public ProductPostViewHolder(View itemView, NativePostAdapter postAdapter, PostClickListener postClickListener) : base(itemView)
            {
                try
                {
                    MainView = itemView;

                    Image = itemView.FindViewById<ImageView>(Resource.Id.image);
                    LocationIcon = itemView.FindViewById<ImageView>(Resource.Id.locationIcon);
                    PostProductLocationText = itemView.FindViewById<TextView>(Resource.Id.postProductLocationText);
                    PostLinkTitle = itemView.FindViewById<TextView>(Resource.Id.postProductTitle);
                    PostProductContent = itemView.FindViewById<TextView>(Resource.Id.postProductContent);
                    TypeText = itemView.FindViewById<TextView>(Resource.Id.typeText);
                    PriceText = itemView.FindViewById<TextView>(Resource.Id.priceText);
                    StatusText = itemView.FindViewById<TextView>(Resource.Id.statusText);
                    PostLinkLinearLayout = itemView.FindViewById<RelativeLayout>(Resource.Id.contentPost);

                    PostAdapter = postAdapter;
                    PostClickListener = postClickListener;

                    PostLinkLinearLayout.SetOnClickListener(this);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }
            public void OnClick(View v)
            {
                try
                {
                    if (AdapterPosition != RecyclerView.NoPosition)
                    {
                        var item = PostAdapter.ListDiffer[AdapterPosition]?.PostData;

                        if (v.Id == PostLinkLinearLayout.Id)
                            PostClickListener.ProductPostClick(new GlobalClickEventArgs() { NewsFeedClass = item, Position = AdapterPosition, View = MainView });
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            } 
        }

        public class PollsPostViewHolder : RecyclerView.ViewHolder, View.IOnClickListener
        {
            public View MainView { get; private set; }
            private readonly NativePostAdapter PostAdapter;

            public TextView VoteText { get; private set; }
            public RelativeLayout LinkLinearLayout { get; private set; }
            public TextView ProgressText { get; private set; }
            public ImageView CheckIcon { get; private set; }
            public ProgressBar ProgressBarView { get; private set; }
             
            public PollsPostViewHolder(View itemView, NativePostAdapter postAdapter) : base(itemView)
            {
                try
                {
                    MainView = itemView;
                    LinkLinearLayout = itemView.FindViewById<RelativeLayout>(Resource.Id.linklinearLayout);
                    VoteText = itemView.FindViewById<TextView>(Resource.Id.voteText);
                    ProgressBarView = itemView.FindViewById<ProgressBar>(Resource.Id.progress);
                    ProgressText = itemView.FindViewById<TextView>(Resource.Id.progressTextview);
                    CheckIcon = itemView.FindViewById<ImageView>(Resource.Id.checkIcon);

                    PostAdapter = postAdapter;

                    LinkLinearLayout.SetOnClickListener(this);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }

            public async void OnClick(View v)
            {
                try
                {
                    if (AdapterPosition != RecyclerView.NoPosition)
                    {
                        if (!Methods.CheckConnectivity())
                        {
                            Toast.MakeText(Application.Context, Application.Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                            return;
                        }
                          
                        var itemObject = PostAdapter.ListDiffer[AdapterPosition];
                        if (v.Id == LinkLinearLayout.Id && itemObject != null)
                        {
                            if (!string.IsNullOrEmpty(itemObject.PostData.VotedId) && itemObject.PostData.VotedId != "0")
                            {
                                //You have already voted this poll.
                                Toast.MakeText(PostAdapter.ActivityContext, PostAdapter.ActivityContext.GetText(Resource.String.Lbl_ErrorVotedPoll), ToastLength.Short).Show();
                                return;
                            }
                             
                            //send api
                            (int apiStatus, var respond) = await RequestsAsync.Posts.AddPollPostAsync(itemObject.PollId);
                            if (apiStatus == 200)
                            {
                                if (respond is AddPollPostObject result)
                                {
                                    itemObject.PostData.VotedId = itemObject.PollId;

                                    //Set The correct value after for polls after new vote
                                    var data = result.Votes.FirstOrDefault(a => a.Id == itemObject.PollId);
                                    if (data != null)
                                    {
                                        ProgressBarView.Progress = int.Parse(data.PercentageNum);
                                        ProgressText.Text = data.Percentage;

                                        if (!string.IsNullOrEmpty(itemObject.PostData.VotedId) && itemObject.PostData.VotedId != "0")
                                        {
                                            if (itemObject.PollsOption.Id == itemObject.PostData.VotedId)
                                            {
                                                CheckIcon.SetImageResource(Resource.Drawable.icon_checkmark_filled_vector);
                                                CheckIcon.ClearColorFilter();
                                            }
                                            else
                                            {
                                                CheckIcon.SetImageResource(Resource.Drawable.icon_check_circle_vector);
                                                CheckIcon.SetColorFilter(new PorterDuffColorFilter(Color.ParseColor("#999999"), PorterDuff.Mode.SrcAtop));
                                            }
                                        }
                                        else
                                        {
                                            CheckIcon.SetImageResource(Resource.Drawable.icon_check_circle_vector);
                                            CheckIcon.SetColorFilter(new PorterDuffColorFilter(Color.ParseColor("#999999"), PorterDuff.Mode.SrcAtop));
                                        }
                                    } 
                                }
                            }
                            else
                            {
                                string errorText = respond is ErrorObject errorMessage ? errorMessage.Error.ErrorText : (string)respond.ToString() ?? "";
                                if (!string.IsNullOrEmpty(errorText))
                                    Toast.MakeText(PostAdapter.ActivityContext, errorText, ToastLength.Short).Show();

                                Methods.DisplayReportResult(TabbedMainActivity.GetInstance(), respond);
                            }
                        } 
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }
        }
      
        public class JobPostViewHolder1 : RecyclerView.ViewHolder, View.IOnClickListener
        {
            public View MainView { get; private set; }
            private readonly NativePostAdapter PostAdapter;
            private readonly PostClickListener PostClickListener;

            public ImageView JobCoverImage { get; private set; }
            public CircleImageView JobAvatar { get; private set; }
            public TextView JobTitle { get; private set; }
            public TextView PageName { get; private set; }
            public TextView JobInfo { get; private set; }
            public RelativeLayout MainLayout { get; private set; }


            public JobPostViewHolder1(View itemView, NativePostAdapter postAdapter, PostClickListener postClickListener) : base(itemView)
            {
                try
                {
                    MainView = itemView;
                    MainLayout = itemView.FindViewById<RelativeLayout>(Resource.Id.mainLayout);
                    JobCoverImage = itemView.FindViewById<ImageView>(Resource.Id.JobCoverImage);
                    JobAvatar = itemView.FindViewById<CircleImageView>(Resource.Id.JobAvatar);
                    JobTitle = itemView.FindViewById<TextView>(Resource.Id.Jobtitle);
                    PageName = itemView.FindViewById<TextView>(Resource.Id.pageName);
                    JobInfo = itemView.FindViewById<TextView>(Resource.Id.JobInfo);

                    PostAdapter = postAdapter;
                    PostClickListener = postClickListener;

                    MainLayout.SetOnClickListener(this);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }
            public void OnClick(View v)
            {
                try
                {
                    if (AdapterPosition != RecyclerView.NoPosition)
                    {
                        var item = PostAdapter.ListDiffer[AdapterPosition]?.PostData;

                        if (v.Id == MainLayout.Id)
                            PostClickListener.JobPostClick(new GlobalClickEventArgs() { NewsFeedClass = item, Position = AdapterPosition, View = MainView });
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }


        }

        public class JobPostViewHolder2 : RecyclerView.ViewHolder, View.IOnClickListener
        {
            public View MainView { get; private set; }
            private readonly NativePostAdapter PostAdapter;
            private readonly PostClickListener PostClickListener;

            public Button JobButton { get; private set; }
            public TextView MinimumTextView { get; private set; }
            public TextView MaximumTextView { get; private set; }
            public TextView MaximumNumber { get; private set; }
            public TextView MinimumNumber { get; private set; }
            public SuperTextView Description { get; private set; }
            public RelativeLayout MainLayout { get; private set; }

            public JobPostViewHolder2(View itemView, NativePostAdapter postAdapter, PostClickListener postClickListener) : base(itemView)
            {
                try
                {
                    MainView = itemView;
                    MainLayout = itemView.FindViewById<RelativeLayout>(Resource.Id.mainLayout);
                    JobButton = itemView.FindViewById<Button>(Resource.Id.JobButton);
                    MinimumTextView = itemView.FindViewById<TextView>(Resource.Id.minimum);
                    MaximumTextView = itemView.FindViewById<TextView>(Resource.Id.maximum);
                    MaximumNumber = itemView.FindViewById<TextView>(Resource.Id.maximumNumber);
                    MinimumNumber = itemView.FindViewById<TextView>(Resource.Id.minimumNumber);
                    Description = itemView.FindViewById<SuperTextView>(Resource.Id.description);
                    Description?.SetTextInfo(Description);

                    PostAdapter = postAdapter;
                    PostClickListener = postClickListener;

                    MainLayout.SetOnClickListener(this);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }
            public void OnClick(View v)
            {
                try
                {
                    if (AdapterPosition != RecyclerView.NoPosition)
                    {
                        var item = PostAdapter.ListDiffer[AdapterPosition]?.PostData;

                        if (v.Id == MainLayout.Id)
                            PostClickListener.JobPostClick(new GlobalClickEventArgs() { NewsFeedClass = item, Position = AdapterPosition, View = MainView });
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }

        }

        public class StoryViewHolder : RecyclerView.ViewHolder, View.IOnClickListener
        {
            public View MainView { get; private set; }
            private readonly NativePostAdapter PostAdapter;
            private readonly PostClickListener PostClickListener;

            public RecyclerView StoryRecyclerView { get; private set; }
            public StoryAdapter StoryAdapter { get; private set; }
            public TextView AboutHead { get; private set; }
            public TextView AboutMore { get; private set; }
            public TextView AboutMoreIcon { get; private set; }

            public StoryViewHolder(View itemView, NativePostAdapter postAdapter, PostClickListener postClickListener) : base(itemView)
            {
                try
                {
                    MainView = itemView;
                    StoryRecyclerView = MainView.FindViewById<RecyclerView>(Resource.Id.Recyler);
                    AboutHead = MainView.FindViewById<TextView>(Resource.Id.headText);
                    AboutMore = MainView.FindViewById<TextView>(Resource.Id.moreText);
                    AboutMoreIcon = MainView.FindViewById<TextView>(Resource.Id.icon);

                    AboutMore.Visibility = ViewStates.Invisible;
                    AboutMoreIcon.Visibility = ViewStates.Invisible;

                    PostAdapter = postAdapter;
                    PostClickListener = postClickListener;

                    AboutMore.SetOnClickListener(this);
                    AboutMoreIcon.SetOnClickListener(this);
                     
                    if (StoryAdapter != null)
                        return;

                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, AboutMoreIcon, AppSettings.FlowDirectionRightToLeft ? IonIconsFonts.ChevronLeft : IonIconsFonts.ChevronRight);

                    StoryRecyclerView.SetLayoutManager(new LinearLayoutManager(itemView.Context, LinearLayoutManager.Horizontal, false));
                    StoryAdapter = new StoryAdapter(postAdapter.ActivityContext);
                    StoryRecyclerView.SetAdapter(StoryAdapter);
                    StoryAdapter.ItemClick += TabbedMainActivity.GetInstance().StoryAdapterOnItemClick;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }

            public void RefreshData()
            {
                try
                {
                    StoryAdapter.NotifyDataSetChanged();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }

            public void OnClick(View v)
            {
                try
                {
                    if (AdapterPosition != RecyclerView.NoPosition)
                    {
                        var item = PostAdapter.ListDiffer[AdapterPosition];

                        if (v.Id == AboutMore.Id || v.Id == AboutMoreIcon.Id)
                            PostClickListener.OpenAllViewer("StoryModel", PostAdapter.IdParameter, item);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }

        }

        public class AddPostViewHolder : RecyclerView.ViewHolder, View.IOnClickListener
        {
            public View MainView { get; private set; }
            private readonly NativePostAdapter PostAdapter;

            public CircleImageView ProfileImageView { get; private set; }
            public TextView PostText { get; private set; }
            public LinearLayout ImageGallery { get; private set; }
            public LinearLayout IconMore { get; private set; }

            public AddPostViewHolder(View itemView, NativePostAdapter postAdapter) : base(itemView)
            {
                try
                {
                    MainView = itemView;
                    ProfileImageView = MainView.FindViewById<CircleImageView>(Resource.Id.image);
                    PostText = MainView.FindViewById<TextView>(Resource.Id.postText);
                    ImageGallery = MainView.FindViewById<LinearLayout>(Resource.Id.photoLinear);
                    IconMore = MainView.FindViewById<LinearLayout>(Resource.Id.moreLinear);

                    PostAdapter = postAdapter;

                    IconMore.SetOnClickListener(this);
                    ImageGallery.SetOnClickListener(this);
                    PostText.SetOnClickListener(this); 

                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }

            public void OnClick(View v)
            {
                try
                {
                    if (AdapterPosition != RecyclerView.NoPosition)
                    {
                        var item = PostAdapter.ListDiffer[AdapterPosition];

                        if (v.Id == IconMore.Id)
                        {
                            try
                            {
                                var intent = new Intent(PostAdapter.ActivityContext, typeof(AddPostActivity));

                                switch (item.TypePost)
                                {
                                    case "feed":
                                    case "user":
                                        intent.PutExtra("Type", "Normal_More");
                                        intent.PutExtra("PostId", PostAdapter.IdParameter);
                                        break;
                                    case "Group":
                                        intent.PutExtra("Type", "SocialGroup");
                                        intent.PutExtra("PostId", item.PostData.GroupRecipient.GroupId);
                                        intent.PutExtra("itemObject", JsonConvert.SerializeObject(item.PostData.GroupRecipient));
                                        break;
                                    case "Event":
                                        intent.PutExtra("Type", "SocialEvent");
                                        if (item.PostData.Event != null)
                                        {
                                            intent.PutExtra("PostId", item.PostData.Event.Value.EventClass.Id);
                                            intent.PutExtra("itemObject", JsonConvert.SerializeObject(item.PostData.Event.Value.EventClass));
                                        }
                                        break;
                                    case "Page":
                                        intent.PutExtra("Type", "SocialPage");
                                        intent.PutExtra("PostId", item.PostData.PageId);
                                        var page = new PageClass()
                                        {
                                            PageId = item.PostData.PageId,
                                            PageName = item.PostData.Publisher.PageName,
                                            Avatar = item.PostData.Publisher.Avatar,
                                        };
                                        intent.PutExtra("itemObject", JsonConvert.SerializeObject(page));
                                        break;
                                    default:
                                        intent.PutExtra("Type", "Normal");
                                        intent.PutExtra("PostId", PostAdapter.IdParameter);
                                        break;
                                }

                                PostAdapter.ActivityContext.StartActivityForResult(intent, 2500);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink); Log.Debug("wael >> AdapterBinders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                            }
                        }
                        else if (v.Id == ImageGallery.Id)
                        {
                            try
                            {
                                var intent = new Intent(PostAdapter.ActivityContext, typeof(AddPostActivity));

                                switch (item.TypePost)
                                {
                                    case "feed":
                                    case "user":
                                        intent.PutExtra("Type", "Normal_Gallery");
                                        intent.PutExtra("PostId", PostAdapter.IdParameter);
                                        break;
                                    case "Group":
                                        intent.PutExtra("Type", "SocialGroup");
                                        intent.PutExtra("PostId", item.PostData.GroupRecipient.GroupId);
                                        intent.PutExtra("itemObject", JsonConvert.SerializeObject(item.PostData.GroupRecipient));
                                        break;
                                    case "Event":
                                        intent.PutExtra("Type", "SocialEvent");
                                        if (item.PostData.Event != null)
                                        {
                                            intent.PutExtra("PostId", item.PostData.Event.Value.EventClass.Id);
                                            intent.PutExtra("itemObject", JsonConvert.SerializeObject(item.PostData.Event.Value.EventClass));
                                        }
                                        break;
                                    case "Page":
                                        intent.PutExtra("Type", "SocialPage");
                                        intent.PutExtra("PostId", item.PostData.PageId);
                                        var page = new PageClass()
                                        {
                                            PageId = item.PostData.PageId,
                                            PageName = item.PostData.Publisher.PageName,
                                            Avatar = item.PostData.Publisher.Avatar,
                                        };
                                        intent.PutExtra("itemObject", JsonConvert.SerializeObject(page));
                                        break;
                                    default:
                                        intent.PutExtra("Type", "Normal");
                                        intent.PutExtra("PostId", PostAdapter.IdParameter);
                                        break;
                                }

                                //intent.PutExtra("itemObject", JsonConvert.SerializeObject(EventData));
                                PostAdapter.ActivityContext.StartActivityForResult(intent, 2500);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink); Log.Debug("wael >> AdapterBinders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                            }
                        }
                        else if (v.Id == PostText.Id)
                        {
                            try
                            {
                                var intent = new Intent(PostAdapter.ActivityContext, typeof(AddPostActivity));
                                switch (item.TypePost)
                                {
                                    case "feed":
                                    case "user":
                                        intent.PutExtra("Type", "Normal");
                                        intent.PutExtra("PostId", PostAdapter.IdParameter);
                                        break;
                                    case "Group":
                                        intent.PutExtra("Type", "SocialGroup");
                                        intent.PutExtra("PostId", item.PostData.GroupRecipient.GroupId);
                                        intent.PutExtra("itemObject", JsonConvert.SerializeObject(item.PostData.GroupRecipient));
                                        break;
                                    case "Event":
                                        intent.PutExtra("Type", "SocialEvent");
                                        if (item.PostData.Event != null)
                                        {
                                            intent.PutExtra("PostId", item.PostData.Event.Value.EventClass.Id);
                                            intent.PutExtra("itemObject", JsonConvert.SerializeObject(item.PostData.Event.Value.EventClass));
                                        }
                                        break;
                                    case "Page":
                                        intent.PutExtra("Type", "SocialPage");
                                        intent.PutExtra("PostId", item.PostData.PageId);
                                        var page = new PageClass()
                                        {
                                            PageId = item.PostData.PageId,
                                            PageName = item.PostData.Publisher.PageName,
                                            Avatar = item.PostData.Publisher.Avatar,
                                        };
                                        intent.PutExtra("itemObject", JsonConvert.SerializeObject(page));
                                        break;
                                    default:
                                        intent.PutExtra("Type", "Normal");
                                        intent.PutExtra("PostId", PostAdapter.IdParameter);
                                        break;
                                }
                                PostAdapter.ActivityContext.StartActivityForResult(intent, 2500);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink); Log.Debug("wael >> AdapterBinders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }

        }

        public class SearchForPostsViewHolder : RecyclerView.ViewHolder, View.IOnClickListener
        {
            public View MainView { get; private set; }
            private readonly NativePostAdapter PostAdapter;

            public LinearLayout SearchForPostsLayout { get; private set; }

            public SearchForPostsViewHolder(View itemView , NativePostAdapter postAdapter) : base(itemView)
            {
                try
                {
                    MainView = itemView;
                    SearchForPostsLayout = MainView.FindViewById<LinearLayout>(Resource.Id.searchForPostsLayout);

                    PostAdapter = postAdapter;

                    SearchForPostsLayout.SetOnClickListener(this); 
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }

            public void OnClick(View v)
            {
                try
                {
                    if (AdapterPosition != RecyclerView.NoPosition)
                    {
                        var item = PostAdapter.ListDiffer[AdapterPosition];

                        if (v.Id == SearchForPostsLayout.Id)
                        {
                            var intent = new Intent(PostAdapter.ActivityContext, typeof(SearchForPostsActivity));

                            switch (item.TypePost)
                            {
                                case "feed":
                                case "user":
                                    intent.PutExtra("TypeSearch", "user");
                                    intent.PutExtra("IdSearch", PostAdapter.IdParameter);
                                    break;
                                case "Group":
                                    intent.PutExtra("TypeSearch", "group");
                                    if (item.PostData.GroupRecipient != null)
                                        intent.PutExtra("IdSearch", item.PostData.GroupRecipient.GroupId);
                                    break;
                                case "Page":
                                    intent.PutExtra("TypeSearch", "page");
                                    if (item.PostData != null)
                                        intent.PutExtra("IdSearch", item.PostData.PageId);
                                    break;
                                default:
                                    intent.PutExtra("TypeSearch", "user");
                                    intent.PutExtra("IdSearch", PostAdapter.IdParameter);
                                    break;
                            }

                            PostAdapter.ActivityContext.StartActivity(intent);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }

        }

        public class SocialLinksViewHolder : RecyclerView.ViewHolder, View.IOnClickListener
        {
            public View MainView { get; private set; }
            private readonly NativePostAdapter PostAdapter;

            public CircleButton BtnFacebook { get; private set; }
            public CircleButton BtnInstegram { get; private set; }
            public CircleButton BtnTwitter { get; private set; }
            public CircleButton BtnGoogle { get; private set; }
            public CircleButton BtnVk { get; private set; }
            public CircleButton BtnYoutube { get; private set; }

            public SocialLinksViewHolder(View itemView , NativePostAdapter postAdapter) : base(itemView)
            {
                try
                {
                    MainView = itemView;

                    BtnFacebook = MainView.FindViewById<CircleButton>(Resource.Id.facebook_button);
                    BtnInstegram = MainView.FindViewById<CircleButton>(Resource.Id.instegram_button);
                    BtnTwitter = MainView.FindViewById<CircleButton>(Resource.Id.twitter_button);
                    BtnGoogle = MainView.FindViewById<CircleButton>(Resource.Id.google_button);
                    BtnVk = MainView.FindViewById<CircleButton>(Resource.Id.vk_button);
                    BtnYoutube = MainView.FindViewById<CircleButton>(Resource.Id.youtube_button);

                    PostAdapter = postAdapter;

                    BtnFacebook.SetOnClickListener(this);
                    BtnInstegram.SetOnClickListener(this);
                    BtnTwitter.SetOnClickListener(this);
                    BtnGoogle.SetOnClickListener(this);
                    BtnVk.SetOnClickListener(this);
                    BtnYoutube.SetOnClickListener(this); 
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }

            public void OnClick(View v)
            {
                try
                {
                    if (AdapterPosition != RecyclerView.NoPosition)
                    {
                        var item = PostAdapter.ListDiffer[AdapterPosition];

                        if (v.Id == BtnFacebook.Id)
                        {
                            if (Methods.CheckConnectivity())
                                new IntentController(PostAdapter.ActivityContext).OpenFacebookIntent(PostAdapter.ActivityContext, item.SocialLinksModel.Facebook);
                            else
                                Toast.MakeText(PostAdapter.ActivityContext, PostAdapter.ActivityContext.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                        }
                        else if (v.Id == BtnInstegram.Id)
                        {
                            if (Methods.CheckConnectivity())
                                new IntentController(PostAdapter.ActivityContext).OpenInstagramIntent(item.SocialLinksModel.Instegram);
                            else
                                Toast.MakeText(PostAdapter.ActivityContext, PostAdapter.ActivityContext.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                        }
                        else if (v.Id == BtnTwitter.Id)
                        {
                            if (Methods.CheckConnectivity())
                                new IntentController(PostAdapter.ActivityContext).OpenTwitterIntent(item.SocialLinksModel.Twitter);
                            else
                                Toast.MakeText(PostAdapter.ActivityContext, PostAdapter.ActivityContext.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                        }
                        else if (v.Id == BtnGoogle.Id)
                        {
                            if (Methods.CheckConnectivity())
                                Methods.App.OpenbrowserUrl(PostAdapter.ActivityContext, "https://plus.google.com/u/0/" + item.SocialLinksModel.Google);
                            else
                                Toast.MakeText(PostAdapter.ActivityContext, PostAdapter.ActivityContext.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                        }
                        else if (v.Id == BtnVk.Id)
                        {
                            if (Methods.CheckConnectivity())
                                new IntentController(PostAdapter.ActivityContext).OpenVkontakteIntent(item.SocialLinksModel.Vk);
                            else
                                Toast.MakeText(PostAdapter.ActivityContext, PostAdapter.ActivityContext.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                        }
                        else if (v.Id == BtnYoutube.Id)
                        {
                            if (Methods.CheckConnectivity())
                                new IntentController(PostAdapter.ActivityContext).OpenYoutubeIntent(item.SocialLinksModel.Youtube);
                            else
                                Toast.MakeText(PostAdapter.ActivityContext, PostAdapter.ActivityContext.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }

        }

        public class AboutBoxViewHolder : RecyclerView.ViewHolder
        {
            public View MainView { get; private set; }
            public TextView AboutHead { get; private set; }
            public SuperTextView AboutDescription { get; private set; }

            public AboutBoxViewHolder(View itemView) : base(itemView)
            {
                try
                {
                    MainView = itemView;
                    AboutHead = MainView.FindViewById<TextView>(Resource.Id.tv_about);
                    AboutDescription = MainView.FindViewById<SuperTextView>(Resource.Id.tv_aboutdescUser);
                    AboutDescription?.SetTextInfo(AboutDescription);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }
        }

        public class FollowersViewHolder : RecyclerView.ViewHolder, View.IOnClickListener
        {
            public View MainView { get; private set; }
            private readonly NativePostAdapter PostAdapter;
            private readonly PostClickListener PostClickListener;

            public RecyclerView FollowersRecyclerView { get; private set; }
            public UserFriendsAdapter FollowersAdapter { get; private set; }
            public TextView AboutHead { get; private set; }
            public TextView AboutMore { get; private set; }
            public TextView AboutMoreIcon { get; private set; }

            public FollowersViewHolder(View itemView, NativePostAdapter postAdapter, PostClickListener postClickListener) : base(itemView)
            {
                try
                {
                    MainView = itemView;
                    FollowersRecyclerView = MainView.FindViewById<RecyclerView>(Resource.Id.Recyler);
                    AboutHead = MainView.FindViewById<TextView>(Resource.Id.headText);
                    AboutMore = MainView.FindViewById<TextView>(Resource.Id.moreText);
                    AboutMoreIcon = MainView.FindViewById<TextView>(Resource.Id.icon);

                    PostAdapter = postAdapter;
                    PostClickListener = postClickListener;

                    AboutMore.SetOnClickListener(this);
                    AboutMoreIcon.SetOnClickListener(this);
                    
                    if (FollowersAdapter != null)
                        return;

                    FollowersRecyclerView.SetLayoutManager(new LinearLayoutManager(itemView.Context, LinearLayoutManager.Horizontal, false));
                    FollowersAdapter = new UserFriendsAdapter(postAdapter.ActivityContext);
                    FollowersRecyclerView.SetAdapter(FollowersAdapter);

                    FollowersAdapter.ItemClick += (sender, e) =>
                    {
                        var position = e.Position;
                        if (position < 0) return;

                        var user = FollowersAdapter.GetItem(position);
                        if (user == null)
                            return;

                        WoWonderTools.OpenProfile(postAdapter.ActivityContext, user.UserId, user);
                    };
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }

            public void OnClick(View v)
            {
                try
                {
                    if (AdapterPosition != RecyclerView.NoPosition)
                    {
                        var item = PostAdapter.ListDiffer[AdapterPosition];

                        if (v.Id == AboutMore.Id || v.Id == AboutMoreIcon.Id)
                            PostClickListener.OpenAllViewer("FollowersModel", PostAdapter.IdParameter, item);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }
        }

        public class ImagesViewHolder : RecyclerView.ViewHolder, View.IOnClickListener
        {
            public View MainView { get; private set; }
            private readonly NativePostAdapter PostAdapter;
            private readonly PostClickListener PostClickListener;

            public RecyclerView ImagesRecyclerView { get; private set; }
            public UserPhotosAdapter ImagesAdapter { get; private set; }
            public TextView AboutHead { get; private set; }
            public TextView AboutMore { get; private set; }
            public TextView AboutMoreIcon { get; private set; }

            public ImagesViewHolder(View itemView, NativePostAdapter postAdapter, PostClickListener postClickListener) : base(itemView)
            {
                try
                {
                    MainView = itemView;
                    ImagesRecyclerView = MainView.FindViewById<RecyclerView>(Resource.Id.Recyler);
                    AboutHead = MainView.FindViewById<TextView>(Resource.Id.headText);
                    AboutMore = MainView.FindViewById<TextView>(Resource.Id.moreText);
                    AboutMoreIcon = MainView.FindViewById<TextView>(Resource.Id.icon);

                    PostAdapter = postAdapter;
                    PostClickListener = postClickListener;

                    AboutMore.SetOnClickListener(this);
                    AboutMoreIcon.SetOnClickListener(this);

                    if (ImagesAdapter != null)
                        return;
                     
                    ImagesRecyclerView.SetLayoutManager(new LinearLayoutManager(itemView.Context, LinearLayoutManager.Horizontal, false));
                    ImagesAdapter = new UserPhotosAdapter(postAdapter.ActivityContext);
                    ImagesRecyclerView.SetAdapter(ImagesAdapter);

                    ImagesAdapter.ItemClick += (sender, e) =>
                    {
                        var position = e.Position;
                        if (position < 0) return;

                        var photo = ImagesAdapter.GetItem(position);
                        if (photo == null)
                            return;

                        postClickListener.OpenImageLightBox(photo);
                    };
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }

            public void OnClick(View v)
            {
                try
                {
                    if (AdapterPosition != RecyclerView.NoPosition)
                    {
                        var item = PostAdapter.ListDiffer[AdapterPosition];

                        if (v.Id == AboutMore.Id || v.Id == AboutMoreIcon.Id)
                            PostClickListener.OpenAllViewer("ImagesModel", PostAdapter.IdParameter, item);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }

        }

        public class PagesSocialViewHolder : RecyclerView.ViewHolder
        {
            public View MainView { get; private set; }
            public RecyclerView PagesRecyclerView { get; private set; }
            public UserPagesAdapter PagesAdapter { get; private set; }
            public TextView AboutHead { get; private set; }
            public TextView AboutMore { get; private set; }
            public TextView AboutMoreIcon { get; private set; }

            public PagesSocialViewHolder(Activity activity, View itemView) : base(itemView)
            {
                try
                {
                    MainView = itemView;
                    PagesRecyclerView = MainView.FindViewById<RecyclerView>(Resource.Id.Recyler);
                    AboutHead = MainView.FindViewById<TextView>(Resource.Id.headText);
                    AboutMore = MainView.FindViewById<TextView>(Resource.Id.moreText);
                    AboutMoreIcon = MainView.FindViewById<TextView>(Resource.Id.icon);

                    if (PagesAdapter != null)
                        return;

                    //FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, AboutMoreIcon, AppSettings.FlowDirectionRightToLeft ? IonIconsFonts.ChevronLeft : IonIconsFonts.ChevronRight);

                    PagesRecyclerView.SetLayoutManager(new LinearLayoutManager(itemView.Context, LinearLayoutManager.Horizontal, false));
                    PagesAdapter = new UserPagesAdapter(activity);
                    PagesRecyclerView.SetAdapter(PagesAdapter);
                    PagesAdapter.ItemClick += (sender, e) =>
                    {
                        try
                        {
                            var position = e.Position;
                            if (position < 0)
                                return;

                            var item = PagesAdapter.GetItem(position);
                            if (item == null)
                                return;

                            MainApplication.GetInstance()?.NavigateTo(activity, typeof(PageProfileActivity), item);
                        }
                        catch (Exception x)
                        {
                            Console.WriteLine(x);
                        }
                    };
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }
        }

        public class GroupsSocialViewHolder : RecyclerView.ViewHolder
        {
            public View MainView { get; private set; }
            public RecyclerView GroupsRecyclerView { get; private set; }
            public UserGroupsAdapter GroupsAdapter { get; private set; }

            public TextView AboutHead { get; private set; }
            public TextView AboutMore { get; private set; }
            public TextView AboutMoreIcon { get; private set; }

            public GroupsSocialViewHolder(Activity activity, View itemView) : base(itemView)
            {
                try
                {
                    MainView = itemView;
                    GroupsRecyclerView = MainView.FindViewById<RecyclerView>(Resource.Id.Recyler);
                    AboutHead = MainView.FindViewById<TextView>(Resource.Id.headText);
                    AboutMore = MainView.FindViewById<TextView>(Resource.Id.moreText);
                    AboutMoreIcon = MainView.FindViewById<TextView>(Resource.Id.icon);

                    if (GroupsAdapter != null)
                        return;

                    //FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, AboutMoreIcon, AppSettings.FlowDirectionRightToLeft ? IonIconsFonts.ChevronLeft : IonIconsFonts.ChevronRight);

                    GroupsRecyclerView.SetLayoutManager(new LinearLayoutManager(itemView.Context, LinearLayoutManager.Horizontal, false));
                    GroupsAdapter = new UserGroupsAdapter(activity);
                    GroupsRecyclerView.SetAdapter(GroupsAdapter);
                    GroupsAdapter.ItemClick += (sender, e) =>
                    {
                        try
                        {
                            var position = e.Position;
                            if (position < 0)
                                return;

                            var item = GroupsAdapter.GetItem(position);
                            if (item == null)
                                return;

                            if (UserDetails.UserId == item.UserId)
                                item.IsOwner = true;

                            //if (!string.IsNullOrEmpty(item.GroupsModel.UserProfileId) && UserDetails.UserId == item.GroupsModel.UserProfileId)
                            //    group.IsJoined = "true";

                            MainApplication.GetInstance()?.NavigateTo(activity, typeof(GroupProfileActivity), item);
                        }
                        catch (Exception x)
                        {
                            Console.WriteLine(x);
                        }
                    };
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }
        }


        public class GroupsViewHolder : RecyclerView.ViewHolder, View.IOnClickListener
        {
            public View MainView { get; private set; }
            private readonly NativePostAdapter PostAdapter;
            private readonly PostClickListener PostClickListener;

            public RecyclerView GroupsRecyclerView { get; private set; }
            public UserGroupsAdapter GroupsAdapter { get; private set; }

            public TextView AboutHead { get; private set; }
            public TextView AboutMore { get; private set; }
            public TextView AboutMoreIcon { get; private set; }

            public GroupsViewHolder(View itemView, NativePostAdapter postAdapter, PostClickListener postClickListener) : base(itemView)
            {
                try
                {
                    MainView = itemView;
                    GroupsRecyclerView = MainView.FindViewById<RecyclerView>(Resource.Id.Recyler);
                    AboutHead = MainView.FindViewById<TextView>(Resource.Id.headText);
                    AboutMore = MainView.FindViewById<TextView>(Resource.Id.moreText);
                    AboutMoreIcon = MainView.FindViewById<TextView>(Resource.Id.icon);

                    PostAdapter = postAdapter;
                    PostClickListener = postClickListener;

                    AboutMore.SetOnClickListener(this);
                    AboutMoreIcon.SetOnClickListener(this);

                    if (GroupsAdapter != null)
                        return;

                    GroupsRecyclerView.SetLayoutManager(new LinearLayoutManager(itemView.Context, LinearLayoutManager.Horizontal, false));
                    GroupsAdapter = new UserGroupsAdapter(postAdapter.ActivityContext);
                    GroupsRecyclerView.SetAdapter(GroupsAdapter);
                    GroupsAdapter.ItemClick += (sender, e) =>
                    {
                        try
                        {
                            var position = e.Position;
                            if (position < 0)
                                return;

                            var item = GroupsAdapter.GetItem(position);
                            if (item == null)
                                return;

                            if (UserDetails.UserId == item.UserId)
                                item.IsOwner = true;

                            MainApplication.GetInstance()?.NavigateTo(postAdapter.ActivityContext, typeof(GroupProfileActivity), item);
                        }
                        catch (Exception x)
                        {
                            Console.WriteLine(x);
                        }
                    };
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }
            public void OnClick(View v)
            {
                try
                {
                    if (AdapterPosition != RecyclerView.NoPosition)
                    {
                        var item = PostAdapter.ListDiffer[AdapterPosition];

                        if (v.Id == AboutMore.Id || v.Id == AboutMoreIcon.Id)
                            PostClickListener.OpenAllViewer("GroupsModel", PostAdapter.IdParameter, item);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }

        }

        public class SuggestedUsersViewHolder : RecyclerView.ViewHolder, View.IOnClickListener
        {
            public View MainView { get; private set; }
            private readonly NativePostAdapter PostAdapter;

            public RecyclerView UsersRecyclerView { get; private set; }
            public SuggestionsUserAdapter UsersAdapter { get; private set; }

            public TextView AboutHead { get; private set; }
            public TextView AboutMore { get; private set; }
            public TextView AboutMoreIcon { get; private set; }

            public SuggestedUsersViewHolder(View itemView, NativePostAdapter postAdapter) : base(itemView)
            {
                try
                {
                    MainView = itemView;
                    UsersRecyclerView = MainView.FindViewById<RecyclerView>(Resource.Id.Recyler);
                    AboutHead = MainView.FindViewById<TextView>(Resource.Id.headText);
                    AboutMore = MainView.FindViewById<TextView>(Resource.Id.moreText);
                    AboutMoreIcon = MainView.FindViewById<TextView>(Resource.Id.icon);

                    AboutHead.Text = postAdapter.ActivityContext.GetString(Resource.String.Lbl3_SuggestionsUsers);
                    if (ListUtils.SuggestedUserList.Count  > 0)
                        AboutMore.Text = ListUtils.SuggestedUserList.Count.ToString();

                    PostAdapter = postAdapter;

                    AboutMore.SetOnClickListener(this);
                    AboutMoreIcon.SetOnClickListener(this);

                    if (UsersAdapter != null)
                        return;

                    // FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, AboutMoreIcon, AppSettings.FlowDirectionRightToLeft ? IonIconsFonts.ChevronLeft : IonIconsFonts.ChevronRight);

                    UsersRecyclerView.SetLayoutManager(new LinearLayoutManager(itemView.Context, LinearLayoutManager.Horizontal, false));
                    UsersAdapter = new SuggestionsUserAdapter(postAdapter.ActivityContext)
                    {
                        UserList = new ObservableCollection<UserDataObject>(ListUtils.SuggestedUserList.Take(12))
                    };
                    UsersRecyclerView.SetAdapter(UsersAdapter);
                    UsersAdapter.ItemClick += (sender, e) =>
                    {
                        try
                        {
                            var position = e.Position;
                            if (position < 0)
                                return;

                            var item = UsersAdapter.GetItem(position);
                            if (item == null)
                                return;

                            WoWonderTools.OpenProfile(postAdapter.ActivityContext, item.UserId, item);
                        }
                        catch (Exception x)
                        {
                            Console.WriteLine(x);
                        }
                    };

                    if (UsersAdapter?.UserList?.Count > 4)
                    {
                        AboutMore.Visibility = ViewStates.Visible;
                        AboutMoreIcon.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        AboutMore.Visibility = ViewStates.Invisible;
                        AboutMoreIcon.Visibility = ViewStates.Invisible;
                    } 
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }
            public void OnClick(View v)
            {
                try
                {
                    if (AdapterPosition != RecyclerView.NoPosition)
                    {
                        if (v.Id == AboutMore.Id || v.Id == AboutMoreIcon.Id)
                        {
                            var intent = new Intent(PostAdapter.ActivityContext, typeof(SuggestionsUsersActivity));
                            intent.PutExtra("class", "newsFeed");
                            PostAdapter.ActivityContext.StartActivity(intent);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            } 
        }

        public class SuggestedGroupsViewHolder : RecyclerView.ViewHolder, View.IOnClickListener
        {
            public View MainView { get; private set; }
            private readonly NativePostAdapter PostAdapter;

            public RecyclerView GroupsRecyclerView { get; private set; }
            public SuggestedGroupAdapter GroupsAdapter { get; private set; }

            public TextView AboutHead { get; private set; }
            public TextView AboutMore { get; private set; }
            public TextView AboutMoreIcon { get; private set; }

            public SuggestedGroupsViewHolder(View itemView, NativePostAdapter postAdapter) : base(itemView)
            {
                try
                {
                    MainView = itemView;
                    GroupsRecyclerView = MainView.FindViewById<RecyclerView>(Resource.Id.Recyler);
                    AboutHead = MainView.FindViewById<TextView>(Resource.Id.headText);
                    AboutMore = MainView.FindViewById<TextView>(Resource.Id.moreText);
                    AboutMoreIcon = MainView.FindViewById<TextView>(Resource.Id.icon);

                    AboutHead.Text = postAdapter.ActivityContext.GetString(Resource.String.Lbl_SuggestedGroups);
                    if (ListUtils.SuggestedGroupList.Count > 0)
                        AboutMore.Text = ListUtils.SuggestedGroupList.Count.ToString();

                    PostAdapter = postAdapter;

                    AboutMore.SetOnClickListener(this);
                    AboutMoreIcon.SetOnClickListener(this);
                      
                    if (GroupsAdapter != null)
                        return;

                    // FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, AboutMoreIcon, AppSettings.FlowDirectionRightToLeft ? IonIconsFonts.ChevronLeft : IonIconsFonts.ChevronRightChevronRight);

                    GroupsRecyclerView.SetLayoutManager(new LinearLayoutManager(itemView.Context, LinearLayoutManager.Horizontal, false));
                    GroupsAdapter = new SuggestedGroupAdapter(postAdapter.ActivityContext)
                    {
                        GroupList = new ObservableCollection<GroupClass>(ListUtils.SuggestedGroupList.Take(12))
                    };
                    GroupsRecyclerView.SetAdapter(GroupsAdapter);
                    GroupsAdapter.ItemClick += (sender, e) =>
                    {
                        try
                        {
                            var position = e.Position;
                            if (position < 0)
                                return;

                            var item = GroupsAdapter.GetItem(position);
                            if (item == null)
                                return;

                            if (UserDetails.UserId == item.UserId)
                                item.IsOwner = true;

                            //if (!string.IsNullOrEmpty(item.GroupsModel.UserProfileId) && UserDetails.UserId == item.GroupsModel.UserProfileId)
                            //    group.IsJoined = "true";

                            MainApplication.GetInstance()?.NavigateTo(postAdapter.ActivityContext, typeof(GroupProfileActivity), item);
                        }
                        catch (Exception x)
                        {
                            Console.WriteLine(x);
                        }
                    };

                    if (GroupsAdapter?.GroupList?.Count > 4)
                    {
                        AboutMore.Visibility = ViewStates.Visible;
                        AboutMoreIcon.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        AboutMore.Visibility = ViewStates.Invisible;
                        AboutMoreIcon.Visibility = ViewStates.Invisible;
                    } 
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }
            public void OnClick(View v)
            {
                try
                {
                    if (AdapterPosition != RecyclerView.NoPosition)
                    {
                        if (v.Id == AboutMore.Id || v.Id == AboutMoreIcon.Id)
                        {
                            var intent = new Intent(PostAdapter.ActivityContext, typeof(SuggestedGroupActivity));
                            PostAdapter.ActivityContext.StartActivity(intent);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }

        }

        public class PagesViewHolder : RecyclerView.ViewHolder, View.IOnClickListener
        {
            public View MainView { get; private set; }
            private readonly NativePostAdapter PostAdapter;
            private readonly PostClickListener PostClickListener;

            public RelativeLayout layoutSuggestionPages { get; private set; }
            public ImageView PageImage1 { get; private set; }
            public ImageView PageImage2 { get; private set; }
            public ImageView PageImage3 { get; private set; }
            public TextView AboutHead { get; private set; }
            public TextView AboutMore { get; private set; }
            public TextView AboutMoreIcon { get; private set; }

            public PagesViewHolder(View itemView, NativePostAdapter postAdapter, PostClickListener postClickListener) : base(itemView)
            {
                try
                {
                    MainView = itemView;

                    layoutSuggestionPages = MainView.FindViewById<RelativeLayout>(Resource.Id.layout_suggestion_Pages);
                    PageImage1 = MainView.FindViewById<ImageView>(Resource.Id.image_page_1);
                    PageImage2 = MainView.FindViewById<ImageView>(Resource.Id.image_page_2);
                    PageImage3 = MainView.FindViewById<ImageView>(Resource.Id.image_page_3);

                    AboutHead = MainView.FindViewById<TextView>(Resource.Id.headText);
                    AboutMore = MainView.FindViewById<TextView>(Resource.Id.moreText);
                    AboutMoreIcon = MainView.FindViewById<TextView>(Resource.Id.icon);

                    PostAdapter = postAdapter;
                    PostClickListener = postClickListener;
                     
                    layoutSuggestionPages.SetOnClickListener(this);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }

            public void OnClick(View v)
            {
                try
                {
                    if (AdapterPosition != RecyclerView.NoPosition)
                    {
                        var item = PostAdapter.ListDiffer[AdapterPosition];

                        if (v.Id == layoutSuggestionPages.Id)
                            PostClickListener.OpenAllViewer("PagesModel", PostAdapter.IdParameter, item);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            } 
        }

        public class EmptyStateAdapterViewHolder : RecyclerView.ViewHolder
        {
            public View MainView { get; private set; }
            public TextView EmptyText { get; private set; }
            public ImageView EmptyImage { get; private set; }

            public EmptyStateAdapterViewHolder(View itemView) : base(itemView)
            {
                MainView = itemView;
                EmptyText = MainView.FindViewById<TextView>(Resource.Id.textEmpty);
                EmptyImage = MainView.FindViewById<ImageView>(Resource.Id.imageEmpty);
            }
        }

        public class AlertAdapterViewHolder : RecyclerView.ViewHolder
        {
            public View MainView { get; private set; }
            public RelativeLayout MianAlert { get; private set; }
            public TextView HeadText { get; private set; }
            public TextView SubText { get; private set; }
            public View LineView { get; private set; }
            public ImageView Image { get; private set; }
            public NativePostAdapter Adapter { get; private set; }
            public AlertAdapterViewHolder(View itemView, NativePostAdapter adapter , PostModelType viewType) : base(itemView)
            {
                try
                {
                    MainView = itemView;
                    Adapter = adapter;

                    MianAlert = MainView.FindViewById<RelativeLayout>(Resource.Id.main);

                    LineView = MainView.FindViewById<View>(Resource.Id.lineview);
                    HeadText = MainView.FindViewById<TextView>(Resource.Id.HeadText);
                    SubText = MainView.FindViewById<TextView>(Resource.Id.subText);
                    Image = MainView.FindViewById<ImageView>(Resource.Id.Image);

                    if (!MianAlert.HasOnClickListeners)
                        MianAlert.Click += (sender, args) =>
                        {
                            try
                            {
                                if (viewType == PostModelType.AlertBox)
                                {
                                    var data = Adapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.AlertBox);
                                    if (data != null)
                                    {
                                        TabbedMainActivity.GetInstance()?.NewsFeedTab.MainRecyclerView.RemoveByRowIndex(data);
                                    }
                                }
                                else
                                {
                                    var data = Adapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.AlertBoxAnnouncement);
                                    if (data != null)
                                    {
                                        TabbedMainActivity.GetInstance()?.NewsFeedTab.MainRecyclerView.RemoveByRowIndex(data);
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                            }
                        };

                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }
        }

        public class AdMobAdapterViewHolder : RecyclerView.ViewHolder
        {
            public View MainView { get; private set; }
            public TemplateView MianAlert { get; private set; }

            public AdMobAdapterViewHolder(View itemView, NativePostAdapter postAdapter) : base(itemView)
            {
                try
                {
                    MainView = itemView;

                    MianAlert = MainView.FindViewById<TemplateView>(Resource.Id.my_template);
                    MianAlert.Visibility = ViewStates.Gone;

                   postAdapter.BindAdMob(this);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }
        }
        
        public class FbAdNativeAdapterViewHolder : RecyclerView.ViewHolder
        {
            public View MainView { get; private set; }
            public NativeAdLayout NativeAdLayout { get; private set; }

            public FbAdNativeAdapterViewHolder(Activity activity ,View itemView , NativePostAdapter postAdapter) : base(itemView)
            {
                try
                {
                    MainView = itemView;

                    NativeAdLayout = itemView.FindViewById<NativeAdLayout>(Resource.Id.native_ad_container);
                    NativeAdLayout.Visibility = ViewStates.Gone;

                    if (postAdapter.MAdItems.Count > 0)
                    {
                        var ad = postAdapter.MAdItems.FirstOrDefault();
                        AdsFacebook.InitNative(activity, NativeAdLayout, ad);
                        postAdapter.MAdItems.Remove(ad);
                    }
                    else
                        AdsFacebook.InitNative(activity, NativeAdLayout, null);
                    postAdapter.BindAdFb();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }
        }

        public class AlertJoinAdapterViewHolder : RecyclerView.ViewHolder, View.IOnClickListener
        {
            public View MainView { get; private set; }
            public NativePostAdapter PostAdapter { get; private set; }
            public RelativeLayout MainRelativeLayout { get; private set; }
            public TextView HeadText { get; private set; }
            public TextView SubText { get; private set; }
            public Button ButtonView { get; private set; }
            public ImageView IconImageView { get; private set; }
            public ImageView NormalImageView { get; private set; }

            public AlertJoinAdapterViewHolder(View itemView ,  NativePostAdapter postAdapter) : base(itemView)
            {
                try
                {
                    MainView = itemView;
                    PostAdapter = postAdapter;

                    MainRelativeLayout = MainView.FindViewById<RelativeLayout>(Resource.Id.mainview);
                    ButtonView = MainView.FindViewById<Button>(Resource.Id.buttonview);
                    HeadText = MainView.FindViewById<TextView>(Resource.Id.HeadText);
                    SubText = MainView.FindViewById<TextView>(Resource.Id.subText);
                    IconImageView = MainView.FindViewById<ImageView>(Resource.Id.IconImageview);
                    NormalImageView = MainView.FindViewById<ImageView>(Resource.Id.Imageview);

                    ButtonView.SetOnClickListener(this);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }

            public void OnClick(View v)
            {
                try
                {
                    if (AdapterPosition != RecyclerView.NoPosition)
                    {
                        if (v.Id == ButtonView.Id)
                        {
                            var item = PostAdapter.ListDiffer[AdapterPosition];

                            var intent = new Intent(PostAdapter.ActivityContext, typeof(SearchTabbedActivity));

                            if (item.AlertModel?.TypeAlert == "Pages")
                                intent.PutExtra("Key", "Random_Pages");
                            else if (item.AlertModel?.TypeAlert == "Groups")
                                intent.PutExtra("Key", "Random_Groups");

                            PostAdapter.ActivityContext.StartActivity(intent);
                        } 
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }
        }

        public class SectionViewHolder : RecyclerView.ViewHolder
        {
            public View MainView { get; private set; }
            public TextView AboutHead { get; private set; }
            public TextView AboutMore { get; private set; }
            public TextView AboutMoreIcon { get; private set; }

            public SectionViewHolder(View itemView) : base(itemView)
            {
                try
                {
                    MainView = itemView;

                    AboutHead = MainView.FindViewById<TextView>(Resource.Id.headText);
                    AboutMore = MainView.FindViewById<TextView>(Resource.Id.moreText);
                    AboutMoreIcon = MainView.FindViewById<TextView>(Resource.Id.icon);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }
        }

        public class PostDividerSectionViewHolder : RecyclerView.ViewHolder
        {
          
            public PostDividerSectionViewHolder(View itemView) : base(itemView)
            {
                 
            }
        }
          
        public class PostAddCommentSectionViewHolder : RecyclerView.ViewHolder, View.IOnClickListener
        {
            public View MainView { get; private set; }
            private readonly NativePostAdapter PostAdapter;
            private readonly PostClickListener PostClickListener;
            public CircleImageView ProfileImageView { get; private set; }
            public LinearLayout AddCommentLayout { get; private set; }
            public LinearLayout LayoutEditText { get; private set; }
            public ImageView EmojiIcon { get; private set; }
            public ImageView ImageIcon { get; private set; }
            public TextView AddCommentTextView { get; private set; }

            public PostAddCommentSectionViewHolder(View itemView, NativePostAdapter postAdapter, PostClickListener postClickListener) : base(itemView)
            {
                try
                {
                    MainView = itemView;
                    ProfileImageView = MainView.FindViewById<CircleImageView>(Resource.Id.image);

                    AddCommentLayout = MainView.FindViewById<LinearLayout>(Resource.Id.addCommentLayout);
                    LayoutEditText = MainView.FindViewById<LinearLayout>(Resource.Id.LayoutEditText);
                    AddCommentTextView = MainView.FindViewById<TextView>(Resource.Id.postText);
                    EmojiIcon = MainView.FindViewById<ImageView>(Resource.Id.Emojiicon);
                    ImageIcon = MainView.FindViewById<ImageView>(Resource.Id.Imageicon);

                    PostAdapter = postAdapter;
                    PostClickListener = postClickListener;

                    AddCommentLayout.SetOnClickListener(this);
                    LayoutEditText.SetOnClickListener(this);
                    ImageIcon.SetOnClickListener(this); 
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            public void OnClick(View v)
            {
                try
                {
                    if (AdapterPosition != RecyclerView.NoPosition)
                    {
                        var item = PostAdapter.ListDiffer[AdapterPosition]?.PostData;

                        if (v.Id == ImageIcon.Id)
                            PostClickListener.CommentPostClick(new GlobalClickEventArgs { NewsFeedClass = item, Position = AdapterPosition, View = MainView } , "Normal_Gallery");
                        else if (v.Id == AddCommentLayout.Id || v.Id == LayoutEditText.Id)
                            PostClickListener.CommentPostClick(new GlobalClickEventArgs { NewsFeedClass = item, Position = AdapterPosition, View = MainView });
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e); Log.Debug("wael >> AdapterHolders", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
                }
            }
        }

        public class PostDefaultSectionViewHolder : RecyclerView.ViewHolder
        {
          
            public PostDefaultSectionViewHolder(View itemView) : base(itemView)
            {
                 
            }
        } 

        public class PostViewHolder : RecyclerView.ViewHolder
        {
            public View LineView { get; private set; }
            public PostViewHolder(View itemView) : base(itemView)
            {
                try
                {
                    LineView = itemView.FindViewById<TextView>(Resource.Id.simpleViewAnimator);
                    LineView.SetBackgroundColor(Color.Transparent);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        } 

        public class ProgressViewHolder : RecyclerView.ViewHolder
        {
            public ProgressBar ProgressBar { get; private set; }

            public ProgressViewHolder(View itemView) : base(itemView)
            {
                try
                {
                    ProgressBar = (ProgressBar)itemView.FindViewById(Resource.Id.progress_bar);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
        
        public class PromoteHolder : RecyclerView.ViewHolder
        {
            public RelativeLayout PromoteLayout { get; private set; }
            public PromoteHolder(View itemView) : base(itemView)
            {
                try
                {
                    PromoteLayout = (RelativeLayout)itemView.FindViewById(Resource.Id.promoteLayout);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

    }
}