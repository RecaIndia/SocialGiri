using Android.App;
using Android.Support.V7.Widget;
using Android.Views;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Request;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Text;
using WoWonderClient.Classes.Posts;
using System.Collections.ObjectModel;
using Android.Content;
using Android.Gms.Ads;
using Android.Gms.Ads.Formats;
using Android.Support.V4.Graphics.Drawable;
using Android.Support.V7.Content.Res;
using WoWonderClient.Classes.Story;
using Android.Util;
using AutoMapper;
using WoWonderClient.Classes.Global;
using Bumptech.Glide.Util;
using Bumptech.Glide.Integration.RecyclerView;
using Com.Tuyenmonkey.Textdecorator;
using Java.Lang;
using WoWonder.Activities.Comment;
using WoWonder.Activities.Comment.Adapters;
using WoWonder.Activities.MyProfile;
using WoWonder.Activities.NativePost.Pages;
using WoWonder.Activities.UserProfile;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo;
using WoWonder.Library.Anjo.SuperTextLibrary;
using WoWonder.SQLite;
using Xamarin.Facebook.Ads;
using Console = System.Console;
using Exception = System.Exception;
using NativeAd = Xamarin.Facebook.Ads.NativeAd;
using Object = Java.Lang.Object;
using String = Java.Lang.String;
using Android.OS;
using WoWonder.Activities.Comment.Fragment;

namespace WoWonder.Activities.NativePost.Post
{
    public enum NativeFeedType
    {
        Global, Event, Group, Page, Popular, User, Saved, HashTag, SearchForPosts, Memories, Share
    }

    public class AdapterModelsClass
    {
        public int Id { get; set; }

        public string TypePost { get; set; }

        public PostDataObject PostData { get; set; }
        public bool IsDefaultFeedPost { get; set; }
        public bool IsSharingPost { get; set; }
        public PostModelType TypeView { get; set; }
        public SpannableString PostDataDecoratedContent { get; set; }
        public AboutModelClass AboutModel { get; set; }
        public SocialLinksModelClass SocialLinksModel { get; set; }
        public FollowersModelClass FollowersModel { get; set; }
        public GroupsModelClass GroupsModel { get; set; }
        public PagesModelClass PagesModel { get; set; }
        public ImagesModelClass ImagesModel { get; set; }
        public AlertModelClass AlertModel { get; set; }
        public ObservableCollection<GetUserStoriesObject.StoryObject> StoryList { get; set; }

        public PollsOptionObject PollsOption { get; set; }

        public string PollId { get; set; }
        public string PollOwnerUserId { get; set; }
        public bool Progress { get; set; }

    }

    public class NativePostAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider, StTools.IXAutoLinkOnClickListener, UnifiedNativeAd.IOnUnifiedNativeAdLoadedListener
    {
        public readonly Activity ActivityContext;

        private RecyclerView MainRecyclerView { get; }
        public NativeFeedType NativePostType { get; set; }
        public string IdParameter { get; private set; }

        public readonly RequestBuilder FullGlideRequestBuilder;
        private readonly RequestBuilder CircleGlideRequestBuilder;

        public List<AdapterModelsClass> ListDiffer { get; set; }
        private PreCachingLayoutManager PreCachingLayout { get; set; }

        private RecyclerView.RecycledViewPool RecycledViewPool { get; set; }

        private readonly PostClickListener PostClickListener;
        public AdapterHolders.StoryViewHolder HolderStory { get; private set; }
        private StReadMoreOption ReadMoreOption { get; }
        public List<NativeAd> MAdItems;
        public NativeAdsManager MNativeAdsManager;
        private IOnLoadMoreListener OnLoadMoreListener;
        private bool Loading;

        public NativePostAdapter(Activity context, string apiIdParameter, RecyclerView recyclerView, NativeFeedType nativePostType, Android.Support.V4.App.FragmentManager fragmentManager)
        {
            try
            {
                HasStableIds = true;
                ActivityContext = context;
                NativePostType = nativePostType;
                MainRecyclerView = recyclerView;
                IdParameter = apiIdParameter;
                PostClickListener = new PostClickListener(ActivityContext);

                RecycledViewPool = new RecyclerView.RecycledViewPool();

                ReadMoreOption = new StReadMoreOption.Builder()
                     .TextLength(200, StReadMoreOption.TypeCharacter)
                     .MoreLabel(context.GetText(Resource.String.Lbl_ReadMore))
                     .LessLabel(context.GetText(Resource.String.Lbl_ReadLess))
                     .MoreLabelColor(Color.ParseColor(AppSettings.MainColor))
                     .LessLabelColor(Color.ParseColor(AppSettings.MainColor))
                     .LabelUnderLine(true)
                     .Build();

                MAdItems = new List<NativeAd>();
                BindAdFb();

                Glide.Get(context).SetMemoryCategory(MemoryCategory.Low);

                var glideRequestOptions = new RequestOptions().SetDiskCacheStrategy(DiskCacheStrategy.All).Placeholder(new ColorDrawable(Color.ParseColor("#efefef"))).Error(Resource.Drawable.ImagePlacholder).Format(Bumptech.Glide.Load.DecodeFormat.PreferRgb565);
                FullGlideRequestBuilder = Glide.With(context).AsBitmap().Apply(glideRequestOptions).Thumbnail(0.5f).SetUseAnimationPool(false);

                var glideRequestOptions2 = new RequestOptions().SetDiskCacheStrategy(DiskCacheStrategy.All).Placeholder(Resource.Drawable.no_profile_image_circle).Error(Resource.Drawable.no_profile_image_circle).Format(Bumptech.Glide.Load.DecodeFormat.PreferRgb565).CircleCrop();
                CircleGlideRequestBuilder = Glide.With(context).AsBitmap().Apply(glideRequestOptions2);

                ListDiffer = new List<AdapterModelsClass>();
                PreCachingLayout = new PreCachingLayoutManager(ActivityContext)
                {
                    Orientation = LinearLayoutManager.Vertical
                };

                PreCachingLayout.SetPreloadItemCount(35);
                PreCachingLayout.AutoMeasureEnabled = false;
                PreCachingLayout.SetExtraLayoutSpace(2000);
                MainRecyclerView.SetLayoutManager(PreCachingLayout);
                MainRecyclerView.GetLayoutManager().ItemPrefetchEnabled = true;

                // var sizeProvider = new FixedPreloadSizeProvider(600, 350);
                var sizeProvider = new ViewPreloadSizeProvider();
                var preLoader = new RecyclerViewPreloader<AdapterModelsClass>(context, this, sizeProvider, 10);
                MainRecyclerView.AddOnScrollListener(preLoader);
                MainRecyclerView.SetAdapter(this);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                View itemView;
                switch (viewType)
                {
                    case (int)PostModelType.PromotePost:
                        {
                            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Post_PromoteSection_Layout, parent, false);
                            var vh = new AdapterHolders.PromoteHolder(itemView);

                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = PromotePost " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.HeaderPost:
                        {
                            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Post_TopSection_Layout, parent, false);
                            var vh = new AdapterHolders.PostTopSectionViewHolder(itemView, this, PostClickListener);

                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = HeaderPost " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.TextSectionPostPart:
                        {
                            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Post_TextSection_Layout, parent, false);
                            var vh = new AdapterHolders.PostTextSectionViewHolder(itemView);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = TextSectionPostPart " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.BottomPostPart:
                        {
                            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Post_ButtomSection_Layout, parent, false);
                            var vh = new AdapterHolders.PostBottomSectionViewHolder(itemView, this, PostClickListener);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType =  BottomPostPart " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.PrevBottomPostPart:
                        {
                            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Post_PreButtomSection_Layout, parent, false);
                            var vh = new AdapterHolders.PostPrevBottomSectionViewHolder(itemView, this, PostClickListener);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = PrevBottomPostPart " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.AddCommentSection:
                        {
                            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Post_Content_AddComment_Section, parent, false);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType =  AddCommentSection " + viewType);
                            var vh = new AdapterHolders.PostAddCommentSectionViewHolder(itemView, this, PostClickListener);
                            return vh;
                        }
                    case (int)PostModelType.CommentSection:
                        {
                            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Post_Content_Comment_Section, parent, false);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType =  AddCommentSection " + viewType);
                            var vh = new CommentAdapterViewHolder(itemView , new CommentAdapter(ActivityContext), new CommentClickListener(ActivityContext , "Comment") , "Post" );
                            return vh;
                        }
                    case (int)PostModelType.Divider:
                        {
                            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Post_Devider, parent, false);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType =  Divider " + viewType);
                            var vh = new AdapterHolders.PostDividerSectionViewHolder(itemView);
                            return vh;
                        }
                    case (int)PostModelType.ImagePost:
                    case (int)PostModelType.StickerPost:
                    case (int)PostModelType.MapPost:
                        {
                            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Post_Content_Image_Layout, parent, false);
                            var vh = new AdapterHolders.PostImageSectionViewHolder(itemView, this, PostClickListener, viewType);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = ImagePost" + viewType);
                            return vh;
                        }
                    case (int)PostModelType.MultiImage2:
                        {
                            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Post_Content_2Images_Layout, parent, false);
                            var vh = new AdapterHolders.Post2ImageSectionViewHolder(itemView, this, PostClickListener);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType =  MultiImage2 " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.MultiImage3:
                        {
                            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Post_Content_3Images_Layout, parent, false);
                            var vh = new AdapterHolders.Post3ImageSectionViewHolder(itemView, this, PostClickListener);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = MultiImage3 " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.MultiImage4:
                        {
                            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Post_Content_4Images_Layout, parent, false);
                            var vh = new AdapterHolders.Post4ImageSectionViewHolder(itemView, this, PostClickListener);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType =  MultiImage4 " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.VideoPost:
                        {
                            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Post_Content_video_layout, parent, false);
                            var vh = new AdapterHolders.PostVideoSectionViewHolder(itemView, this);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = VideoPost " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.BlogPost:
                        {
                            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Post_Content_Blog_Layout, parent, false);
                            var vh = new AdapterHolders.PostBlogSectionViewHolder(itemView, this, PostClickListener);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = BlogPost " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.ColorPost:
                        {
                            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Post_Content_ColorBox_Layout, parent, false);
                            var vh = new AdapterHolders.PostColorBoxSectionViewHolder(itemView);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = ColorPost " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.EventPost:
                        {
                            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Post_Content_Event_Section, parent, false);
                            var vh = new AdapterHolders.EventPostViewHolder(itemView, this, PostClickListener);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = EventPost " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.LinkPost:
                        {
                            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Post_Content_Link_Layout, parent, false);
                            var vh = new AdapterHolders.LinkPostViewHolder(itemView, this, PostClickListener);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = LinkPost " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.MultiImages:
                        {
                            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Post_Content_MultiImages_Layout, parent, false);
                            var vh = new AdapterHolders.PostMultiImagesViewHolder(itemView, this, PostClickListener);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = MultiImages" + viewType);
                            return vh;
                        }
                    case (int)PostModelType.FilePost:
                        {
                            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Post_Content_File_Layout, parent, false);
                            var vh = new AdapterHolders.FilePostViewHolder(itemView, this, PostClickListener);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = FilePost " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.FundingPost:
                        {
                            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Post_Content_Funding_Layout, parent, false);
                            var vh = new AdapterHolders.FundingPostViewHolder(itemView, this, PostClickListener);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = FundingPost " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.ProductPost:
                        {
                            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Post_Content_Product_Layout, parent, false);
                            var vh = new AdapterHolders.ProductPostViewHolder(itemView, this, PostClickListener);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = ProductPost " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.VoicePost:
                        {
                            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Post_Content_Voice_Layout, parent, false);
                            var vh = new AdapterHolders.SoundPostViewHolder(itemView, this, PostClickListener);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = VoicePost " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.YoutubePost:
                        {
                            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Post_Content_Youtube_Section, parent, false);
                            var vh = new AdapterHolders.YoutubePostViewHolder(itemView, this, PostClickListener);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = YoutubePost " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.OfferPost:
                        {
                            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Post_Content_Offer_Section, parent, false);
                            var vh = new AdapterHolders.OfferPostViewHolder(itemView, this, PostClickListener);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = OfferPost " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.JobPostSection1:
                        {
                            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Post_Content_Job_Layout1, parent, false);
                            var vh = new AdapterHolders.JobPostViewHolder1(itemView, this, PostClickListener);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = JobPostSection1 " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.JobPostSection2:
                        {
                            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Post_Content_Job_Layout2, parent, false);
                            var vh = new AdapterHolders.JobPostViewHolder2(itemView, this, PostClickListener);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType =  JobPostSection2 " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.PollPost:
                        {
                            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Post_Content_Poll_Section, parent, false);
                            var vh = new AdapterHolders.PollsPostViewHolder(itemView, this);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = PollPost " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.SharedHeaderPost:
                        {
                            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Post_TopSectionShare_Layout, parent, false);
                            var vh = new AdapterHolders.PostTopSharedSectionViewHolder(itemView, this, PostClickListener);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = SharedHeaderPost " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.LivePost:
                    case (int)PostModelType.DeepSoundPost:
                    case (int)PostModelType.VimeoPost:
                    case (int)PostModelType.FacebookPost:
                    case (int)PostModelType.PlayTubePost:
                        {
                            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Post_Content_WebView_Layout, parent, false);
                            var vh = new AdapterHolders.PostPlayTubeContentViewHolder(itemView);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = WebView " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.AlertBox:
                        {
                            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.ViewModel_Alert, parent, false);
                            var vh = new AdapterHolders.AlertAdapterViewHolder(itemView, this, PostModelType.AlertBox);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType =  AlertBox " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.AlertBoxAnnouncement:
                        {
                            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.ViewModel_Announcement, parent, false);
                            var vh = new AdapterHolders.AlertAdapterViewHolder(itemView, this, PostModelType.AlertBoxAnnouncement);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType =  AlertBoxAnnouncement " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.AlertJoinBox:
                        {
                            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.ViewModel_AlertJoin, parent, false);
                            var vh = new AdapterHolders.AlertJoinAdapterViewHolder(itemView, this);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = AlertJoinBox " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.Section:
                        {
                            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.ViewModel_Section, parent, false);
                            var vh = new AdapterHolders.SectionViewHolder(itemView);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = Section " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.AddPostBox:
                        {
                            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.ViewModel_AddPost, parent, false);
                            var vh = new AdapterHolders.AddPostViewHolder(itemView, this);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType =  AddPostBox " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.SearchForPosts:
                        {
                            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.ViewModel_SearchForPost, parent, false);
                            var vh = new AdapterHolders.SearchForPostsViewHolder(itemView, this);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = SearchForPosts" + viewType);
                            return vh;
                        }
                    case (int)PostModelType.SocialLinks:
                        {
                            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.ViewModel_SociaLink, parent, false);
                            var vh = new AdapterHolders.SocialLinksViewHolder(itemView, this);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = SocialLinks " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.AboutBox:
                        {
                            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.ViewModel_About, parent, false);
                            var vh = new AdapterHolders.AboutBoxViewHolder(itemView);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = AboutBox " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.Story:
                        {
                            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.ViewModel_HRecyclerView, parent, false);
                            var vh = new AdapterHolders.StoryViewHolder(itemView, this, PostClickListener);
                            vh.StoryRecyclerView.SetRecycledViewPool(RecycledViewPool);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType =  Story " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.FollowersBox:
                        {
                            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.ViewModel_HRecyclerView, parent, false);
                            var vh = new AdapterHolders.FollowersViewHolder(itemView, this, PostClickListener);
                            vh.FollowersRecyclerView.SetRecycledViewPool(RecycledViewPool);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.GroupsBox:
                        {
                            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.ViewModel_HRecyclerView, parent, false);
                            var vh = new AdapterHolders.GroupsViewHolder(itemView, this, PostClickListener);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = GroupsBox " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.SuggestedGroupsBox:
                        {
                            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.ViewModel_HRecyclerView, parent, false);
                            var vh = new AdapterHolders.SuggestedGroupsViewHolder(itemView, this);
                            vh.GroupsRecyclerView.SetRecycledViewPool(RecycledViewPool);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.SuggestedUsersBox:
                        {
                            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.ViewModel_HRecyclerView, parent, false);
                            var vh = new AdapterHolders.SuggestedUsersViewHolder(itemView, this);
                            vh.UsersRecyclerView.SetRecycledViewPool(RecycledViewPool);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = SuggestedUsersBox " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.ImagesBox:
                        {
                            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.ViewModel_HRecyclerView, parent, false);
                            var vh = new AdapterHolders.ImagesViewHolder(itemView, this, PostClickListener);
                            vh.ImagesRecyclerView.SetRecycledViewPool(RecycledViewPool);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = ImagesBox " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.PagesBox:
                        {
                            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.ViewModel_Pages, parent, false);
                            var vh = new AdapterHolders.PagesViewHolder(itemView, this, PostClickListener);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType =  PagesBox" + viewType);
                            return vh;
                        }
                    case (int)PostModelType.AdsPost:
                        {
                            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.PostType_Ads, parent, false);
                            var vh = new AdapterHolders.AdsPostViewHolder(itemView, this, PostClickListener);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = AdsPost " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.EmptyState:
                        {
                            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Style_EmptyState, parent, false);
                            var vh = new AdapterHolders.EmptyStateAdapterViewHolder(itemView);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType =  EmptyState " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.AdMob1:
                        {
                            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.PostType_AdMob, parent, false);
                            var vh = new AdapterHolders.AdMobAdapterViewHolder(itemView, this);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType =  AdMob " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.AdMob2:
                        { 
                            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.PostType_AdMob2, parent, false);
                            var vh = new AdapterHolders.AdMobAdapterViewHolder(itemView, this);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType =  AdMob " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.FbAdNative:
                        {
                            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.PostType_FbNativeAd, parent, false);
                            var vh = new AdapterHolders.FbAdNativeAdapterViewHolder(ActivityContext, itemView, this);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = FbAdNative " + viewType);
                            return vh;
                        }
                    case (int)PostModelType.ViewProgress:
                        {
                            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.ItemProgressView, parent, false);
                            var vh = new AdapterHolders.ProgressViewHolder(itemView);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = ViewProgress " + viewType);
                            return vh;
                        }
                    default:
                        {
                            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Post_Content_Null_Layout, parent, false);
                            var vh = new AdapterHolders.PostDefaultSectionViewHolder(itemView);
                            Console.WriteLine("WoLog: NativePostAdapter / OnCreateViewHolder  >>  PostModelType = default " + viewType);
                            return vh;
                        }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("EX:ALLEN PostAdapter >> " + exception);
                return null;
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position, IList<Object> payloads)
        {
            try
            {
                if (payloads.Count > 0)
                {
                    if (payloads[0].ToString() == "StoryRefresh")
                    {
                        if (viewHolder is AdapterHolders.StoryViewHolder holder)
                            holder.RefreshData();
                    }
                    else if (payloads[0].ToString() == "reaction")
                    {
                        switch (viewHolder)
                        {
                            case AdapterHolders.PostPrevBottomSectionViewHolder holder:
                                NotifyItemChanged(position);
                                break;
                            case AdapterHolders.PostBottomSectionViewHolder holder2:
                                NotifyItemChanged(position);
                                break;
                        }
                    }
                    else if (payloads[0].ToString() == "commentReplies")
                    {
                        switch (viewHolder)
                        {
                            case AdapterHolders.PostPrevBottomSectionViewHolder holder:
                                NotifyItemChanged(position);
                                break;
                            case CommentAdapterViewHolder holder:
                                NotifyItemChanged(position);
                                break;
                        }
                    }
                    else if (payloads[0].ToString() == "BoostedPost")
                    {
                        switch (viewHolder)
                        {
                            case AdapterHolders.PromoteHolder holder:
                                NotifyItemChanged(position);
                                break; 
                        }
                    }
                    else
                    {
                        base.OnBindViewHolder(viewHolder, position, payloads);
                    }
                }
                else
                {
                    base.OnBindViewHolder(viewHolder, position, payloads);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                base.OnBindViewHolder(viewHolder, position, payloads);
            }
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                var item = ListDiffer[position];
                var itemViewType = viewHolder.ItemViewType;
                switch (itemViewType)
                {
                    case (int)PostModelType.PromotePost:
                        {
                            if (!(viewHolder is AdapterHolders.PromoteHolder holder))
                                return;

                            bool isPromoted = item.PostData.IsPostBoosted == "1" || item.PostData.SharedInfo.SharedInfoClass != null && item.PostData.SharedInfo.SharedInfoClass?.IsPostBoosted == "1";
                            if (!isPromoted)
                            {
                                holder.PromoteLayout.Visibility = ViewStates.Gone;
                            }

                            break;
                        }
                    case (int)PostModelType.HeaderPost:
                        {
                            if (!(viewHolder is AdapterHolders.PostTopSectionViewHolder holder))
                                return;

                            UserDataObject publisher = item.PostData.Publisher ?? item.PostData.UserData;

                            if (item.PostData.PostPrivacy == "4")
                                GlideImageLoader.LoadImage(ActivityContext, "user_anonymous", holder.UserAvatar, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                            else
                                CircleGlideRequestBuilder.Load(publisher.Avatar).Into(holder.UserAvatar);

                            if (item.PostData.PostPrivacy == "4") //Anonymous Post
                                holder.Username.Text = ActivityContext.GetText(Resource.String.Lbl_Anonymous);
                            else
                                holder.Username.SetText(item.PostDataDecoratedContent);

                            holder.TimeText.Text = item.PostData.Time;

                            if (holder.PrivacyPostIcon != null && !string.IsNullOrEmpty(item.PostData.PostPrivacy) && (publisher.UserId == UserDetails.UserId || AppSettings.ShowPostPrivacyForAllUser))
                            {
                                if (item.PostData.PostPrivacy == "0") //Everyone
                                {
                                    holder.PrivacyPostIcon.SetImageResource(Resource.Drawable.icon_privacy_globe);
                                }
                                else if (item.PostData.PostPrivacy.Contains("ifollow") || item.PostData.PostPrivacy == "2") //People_i_Follow
                                {
                                    holder.PrivacyPostIcon.SetImageResource(Resource.Drawable.icon_privacy_user);
                                }
                                else if (item.PostData.PostPrivacy.Contains("me") || item.PostData.PostPrivacy == "1") //People_Follow_Me
                                {
                                    holder.PrivacyPostIcon.SetImageResource(Resource.Drawable.icon_privacy_user_friends);
                                }
                                else if (item.PostData.PostPrivacy == "4") //Anonymous
                                {
                                    holder.PrivacyPostIcon.SetImageResource(Resource.Drawable.icon_privacy_user_secret);
                                }
                                else //No_body) 
                                {
                                    holder.PrivacyPostIcon.SetImageResource(Resource.Drawable.icon_privacy_lock);
                                }

                                holder.PrivacyPostIcon.Visibility = ViewStates.Visible;
                            }
                            Console.WriteLine("WoLog: NativePostAdapter / OnBindViewHolder  >>  PostModelType = HeaderPost " + position);
                            break;
                        }
                    case (int)PostModelType.SharedHeaderPost:
                        {
                            if (!(viewHolder is AdapterHolders.PostTopSharedSectionViewHolder holder))
                                return;

                            var itemPost = item.PostData;

                            UserDataObject publisher = itemPost.Publisher ?? itemPost.UserData;

                            if (itemPost.PostPrivacy == "4")
                                GlideImageLoader.LoadImage(ActivityContext, "user_anonymous", holder.UserAvatar, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                            else
                                CircleGlideRequestBuilder.Load(publisher.Avatar).Into(holder.UserAvatar);

                            if (itemPost.PostPrivacy == "4") //Anonymous Post
                                holder.Username.Text = ActivityContext.GetText(Resource.String.Lbl_Anonymous);
                            else
                                holder.Username.Text = publisher.Name;

                            holder.TimeText.Text = itemPost.Time;

                            if (holder.PrivacyPostIcon != null && !string.IsNullOrEmpty(itemPost.PostPrivacy) && (publisher.UserId == UserDetails.UserId || AppSettings.ShowPostPrivacyForAllUser))
                            {
                                if (itemPost.PostPrivacy == "0") //Everyone
                                {
                                    holder.PrivacyPostIcon.SetImageResource(Resource.Drawable.icon_privacy_globe);
                                }
                                else if (itemPost.PostPrivacy.Contains("ifollow") || itemPost.PostPrivacy == "2") //People_i_Follow
                                {
                                    holder.PrivacyPostIcon.SetImageResource(Resource.Drawable.icon_privacy_user);
                                }
                                else if (itemPost.PostPrivacy.Contains("me") || itemPost.PostPrivacy == "1") //People_Follow_Me
                                {
                                    holder.PrivacyPostIcon.SetImageResource(Resource.Drawable.icon_privacy_user_friends);
                                }
                                else if (itemPost.PostPrivacy == "4") //Anonymous
                                {
                                    holder.PrivacyPostIcon.SetImageResource(Resource.Drawable.icon_privacy_user_secret);
                                }
                                else //No_body) 
                                {
                                    holder.PrivacyPostIcon.SetImageResource(Resource.Drawable.icon_privacy_lock);
                                }

                                holder.PrivacyPostIcon.Visibility = ViewStates.Visible;
                            }

                            break;
                        }
                    case (int)PostModelType.PrevBottomPostPart:
                        {
                            if (!(viewHolder is AdapterHolders.PostPrevBottomSectionViewHolder holder))
                                return;

                            if (holder.CommentCount != null)
                                holder.CommentCount.Text = item.PostData.PostComments;

                            if (holder.ShareCount != null)
                                holder.ShareCount.Text = item.PostData.PostShare;

                            if (holder.LikeCount != null)
                            {
                                if (AppSettings.PostButton == PostButtonSystem.ReactionDefault || AppSettings.PostButton == PostButtonSystem.ReactionSubShine)
                                    holder.LikeCount.Text = item.PostData.PostLikes;
                                else
                                    holder.LikeCount.Text = item.PostData.PostLikes + " " + ActivityContext.GetString(Resource.String.Btn_Likes);
                            }

                            holder.ViewCount.Text = item.PostData.PrevButtonViewText;
                            Console.WriteLine("WoLog: NativePostAdapter / OnBindViewHolder  >>  PostModelType = PrevBottomPostPart " + position);
                            break;
                        }
                    case (int)PostModelType.BottomPostPart:
                        {
                            if (!(viewHolder is AdapterHolders.PostBottomSectionViewHolder holder))
                                return;

                            if (AppSettings.PostButton == PostButtonSystem.ReactionDefault || AppSettings.PostButton == PostButtonSystem.ReactionSubShine)
                            {
                                item.PostData.Reaction ??= new WoWonderClient.Classes.Posts.Reaction();

                                if ((bool)(item.PostData.Reaction != null & item.PostData.Reaction?.IsReacted))
                                {
                                    if (!string.IsNullOrEmpty(item.PostData.Reaction.Type))
                                    {
                                        switch (item.PostData.Reaction.Type)
                                        {
                                            case "1":
                                            case "Like":
                                                holder.LikeButton.SetReactionPack(ReactConstants.Like);
                                                break;
                                            case "2":
                                            case "Love":
                                                holder.LikeButton.SetReactionPack(ReactConstants.Love);
                                                break;
                                            case "3":
                                            case "HaHa":
                                                holder.LikeButton.SetReactionPack(ReactConstants.HaHa);
                                                break;
                                            case "4":
                                            case "Wow":
                                                holder.LikeButton.SetReactionPack(ReactConstants.Wow);
                                                break;
                                            case "5":
                                            case "Sad":
                                                holder.LikeButton.SetReactionPack(ReactConstants.Sad);
                                                break;
                                            case "6":
                                            case "Angry":
                                                holder.LikeButton.SetReactionPack(ReactConstants.Angry);
                                                break;
                                            default:
                                                holder.LikeButton.SetReactionPack(ReactConstants.Default);
                                                break;

                                        }
                                    }
                                }
                                else
                                    holder.LikeButton.SetReactionPack(ReactConstants.Default);
                            }
                            else
                            {
                                if ((bool)(item.PostData.Reaction != null & !item.PostData.Reaction?.IsReacted))
                                    holder.LikeButton.SetReactionPack(ReactConstants.Default);

                                if (item.PostData.IsLiked != null && item.PostData.IsLiked.Value)
                                    holder.LikeButton.SetReactionPack(ReactConstants.Like);
                                 
                                if (holder.SecondReactionButton != null)
                                {
                                    switch (AppSettings.PostButton)
                                    {
                                        case PostButtonSystem.Wonder when item.PostData.IsWondered != null && item.PostData.IsWondered.Value:
                                        {
                                            Drawable unwrappedDrawable = AppCompatResources.GetDrawable(ActivityContext, Resource.Drawable.ic_action_wowonder);
                                            Drawable wrappedDrawable = DrawableCompat.Wrap(unwrappedDrawable);
                                            if (Build.VERSION.SdkInt <= BuildVersionCodes.Lollipop)
                                            {
                                                DrawableCompat.SetTint(wrappedDrawable, Color.ParseColor("#f89823"));
                                            }
                                            else
                                            {
                                                wrappedDrawable = wrappedDrawable.Mutate();
                                                wrappedDrawable.SetColorFilter(new PorterDuffColorFilter(Color.ParseColor("#f89823"), PorterDuff.Mode.SrcAtop));
                                            }

                                            holder.SecondReactionButton.SetCompoundDrawablesWithIntrinsicBounds(wrappedDrawable, null, null, null);

                                            holder.SecondReactionButton.Text = ActivityContext.GetString(Resource.String.Lbl_wondered);
                                            holder.SecondReactionButton.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                                            break;
                                        }
                                        case PostButtonSystem.Wonder:
                                        {
                                            Drawable unwrappedDrawable = AppCompatResources.GetDrawable(ActivityContext, Resource.Drawable.ic_action_wowonder);
                                            Drawable wrappedDrawable = DrawableCompat.Wrap(unwrappedDrawable);
                                            if (Build.VERSION.SdkInt <= BuildVersionCodes.Lollipop)
                                            {
                                                DrawableCompat.SetTint(wrappedDrawable, Color.ParseColor("#666666"));
                                            }
                                            else
                                            {
                                                wrappedDrawable = wrappedDrawable.Mutate();
                                                wrappedDrawable.SetColorFilter(new PorterDuffColorFilter(Color.ParseColor("#666666"), PorterDuff.Mode.SrcAtop));
                                            }
                                            holder.SecondReactionButton.SetCompoundDrawablesWithIntrinsicBounds(wrappedDrawable, null, null, null);

                                            holder.SecondReactionButton.Text = ActivityContext.GetString(Resource.String.Btn_Wonder);
                                            holder.SecondReactionButton.SetTextColor(Color.ParseColor("#444444"));
                                            break;
                                        }
                                        case PostButtonSystem.DisLike when item.PostData.IsWondered != null && item.PostData.IsWondered.Value:
                                        {
                                            Drawable unwrappedDrawable = AppCompatResources.GetDrawable(ActivityContext, Resource.Drawable.ic_action_dislike);
                                            Drawable wrappedDrawable = DrawableCompat.Wrap(unwrappedDrawable);

                                            if (Build.VERSION.SdkInt <= BuildVersionCodes.Lollipop)
                                            {
                                                DrawableCompat.SetTint(wrappedDrawable, Color.ParseColor("#f89823"));
                                            }
                                            else
                                            {
                                                wrappedDrawable = wrappedDrawable.Mutate();
                                                wrappedDrawable.SetColorFilter(new PorterDuffColorFilter(Color.ParseColor("#f89823"), PorterDuff.Mode.SrcAtop));
                                            }

                                            holder.SecondReactionButton.SetCompoundDrawablesWithIntrinsicBounds(wrappedDrawable, null, null, null);

                                            holder.SecondReactionButton.Text = ActivityContext.GetString(Resource.String.Lbl_disliked);
                                            holder.SecondReactionButton.SetTextColor(Color.ParseColor("#f89823"));
                                            break;
                                        }
                                        case PostButtonSystem.DisLike:
                                        {
                                            Drawable unwrappedDrawable = AppCompatResources.GetDrawable(ActivityContext, Resource.Drawable.ic_action_dislike);
                                            Drawable wrappedDrawable = DrawableCompat.Wrap(unwrappedDrawable);
                                            if (Build.VERSION.SdkInt <= BuildVersionCodes.Lollipop)
                                            {
                                                DrawableCompat.SetTint(wrappedDrawable, Color.ParseColor("#666666"));
                                            }
                                            else
                                            {
                                                wrappedDrawable = wrappedDrawable.Mutate();
                                                wrappedDrawable.SetColorFilter(new PorterDuffColorFilter(Color.ParseColor("#666666"), PorterDuff.Mode.SrcAtop));
                                            }

                                            holder.SecondReactionButton.SetCompoundDrawablesWithIntrinsicBounds(wrappedDrawable, null, null, null);

                                            holder.SecondReactionButton.Text = ActivityContext.GetString(Resource.String.Btn_Dislike);
                                            holder.SecondReactionButton.SetTextColor(Color.ParseColor("#444444"));
                                            break;
                                        }
                                    }
                                }
                            }

                            if (item.IsSharingPost)
                            {
                                holder.ShareLinearLayout.Visibility = ViewStates.Gone;
                            }
                            else
                            {
                                holder.ShareLinearLayout.Visibility = AppSettings.ShowShareButton ? ViewStates.Visible : ViewStates.Gone;
                            }
                            Console.WriteLine("WoLog: NativePostAdapter / OnBindViewHolder  >>  PostModelType =  BottomPostPart " + position);
                            Console.WriteLine(holder);
                            break;
                        }
                    case (int)PostModelType.TextSectionPostPart:
                        {
                            if (!(viewHolder is AdapterHolders.PostTextSectionViewHolder holder))
                                return;

                            if (string.IsNullOrEmpty(item.PostData.Orginaltext) || string.IsNullOrWhiteSpace(item.PostData.Orginaltext))
                            {
                                if (holder.Description.Visibility != ViewStates.Gone)
                                    holder.Description.Visibility = ViewStates.Gone;
                            }
                            else
                            {
                                if (holder.Description.Visibility != ViewStates.Visible)
                                    holder.Description.Visibility = ViewStates.Visible;

                                if (!holder.Description.Text.Contains(ActivityContext.GetText(Resource.String.Lbl_ReadMore)) && !holder.Description.Text.Contains(ActivityContext.GetText(Resource.String.Lbl_ReadLess)))
                                {
                                    if (item.PostData.RegexFilterList != null & item.PostData.RegexFilterList?.Count > 0)
                                        holder.Description.SetAutoLinkOnClickListener(this, item.PostData.RegexFilterList);
                                    else
                                        holder.Description.SetAutoLinkOnClickListener(this, new Dictionary<string, string>());

                                    ReadMoreOption.AddReadMoreTo(holder.Description, new String(item.PostData.Orginaltext));
                                }
                                else if (holder.Description.Text.Contains(ActivityContext.GetText(Resource.String.Lbl_ReadLess)))
                                {
                                    ReadMoreOption.AddReadLess(holder.Description, new String(item.PostData.Orginaltext));
                                }
                                else
                                {
                                    holder.Description.Text = item.PostData.Orginaltext;
                                }
                            }
                            Console.WriteLine("WoLog: NativePostAdapter / OnBindViewHolder  >>  PostModelType =  TextSectionPostPart " + position);
                            break;
                        }
                    case (int)PostModelType.CommentSection:
                        {
                            if (!(viewHolder is CommentAdapterViewHolder holder))
                                return;

                            var comment = item.PostData.GetPostComments.FirstOrDefault(danjo => string.IsNullOrEmpty(danjo.CFile) && string.IsNullOrEmpty(danjo.Record));
                            if (comment == null)
                                return;
                             
                            var db = Mapper.Map<CommentObjectExtra>(comment);
                            LoadCommentData(db, holder);

                            holder.CommentAdapter.CommentList = new ObservableCollection<CommentObjectExtra>(Mapper.Map<ObservableCollection<CommentObjectExtra>>(item.PostData.GetPostComments) );

                            break;
                        }
                    case (int)PostModelType.AddCommentSection:
                        {
                            if (!(viewHolder is AdapterHolders.PostAddCommentSectionViewHolder holder))
                                return;

                            GlideImageLoader.LoadImage(ActivityContext, UserDetails.Avatar, holder.ProfileImageView, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                            break;
                        }
                    case (int)PostModelType.StickerPost:
                    case (int)PostModelType.ImagePost:
                        {
                            if (!(viewHolder is AdapterHolders.PostImageSectionViewHolder holder))
                                return;

                            string imageUrl;
                            if (item.PostData.PhotoAlbum?.Count > 0)
                            {
                                var imagesList = item.PostData.PhotoAlbum;
                                imageUrl = imagesList[0].Image;
                            }
                            else
                            {
                                imageUrl = !string.IsNullOrEmpty(item.PostData.PostSticker) ? item.PostData.PostSticker : item.PostData.PostFileFull;
                            }
                            holder.Image.Layout(0, 0, 0, 0);
                            FullGlideRequestBuilder.Load(imageUrl).Into(holder.Image);
                            Console.WriteLine("WoLog: NativePostAdapter / OnBindViewHolder  >>  PostModelType =  " + position);
                            break;
                        }
                    case (int)PostModelType.MapPost:
                        {
                            if (!(viewHolder is AdapterHolders.PostImageSectionViewHolder holder))
                                return;

                            FullGlideRequestBuilder.Load(item.PostData.ImageUrlMap).Into(holder.Image);

                            break;
                        }
                    case (int)PostModelType.MultiImage2:
                        {
                            if (!(viewHolder is AdapterHolders.Post2ImageSectionViewHolder holder))
                                return;

                            if (item.PostData.PhotoMulti?.Count == 2 || item.PostData.PhotoAlbum?.Count == 2)
                            {
                                var imagesList = item.PostData.PhotoMulti ?? item.PostData.PhotoAlbum;

                                FullGlideRequestBuilder.Load(imagesList[0].Image).Into(holder.Image);
                                FullGlideRequestBuilder.Load(imagesList[1].Image).Into(holder.Image2);
                            }

                            break;
                        }
                    case (int)PostModelType.MultiImage3:
                        {
                            if (!(viewHolder is AdapterHolders.Post3ImageSectionViewHolder holder))
                                return;

                            if (item.PostData.PhotoMulti?.Count == 3 || item.PostData.PhotoAlbum?.Count == 3)
                            {
                                var imagesList = item.PostData.PhotoMulti ?? item.PostData.PhotoAlbum;

                                FullGlideRequestBuilder.Load(imagesList[0].Image).Into(holder.Image);
                                FullGlideRequestBuilder.Load(imagesList[1].Image).Into(holder.Image2);
                                FullGlideRequestBuilder.Load(imagesList[2].Image).Into(holder.Image3);
                            }

                            break;
                        }
                    case (int)PostModelType.MultiImage4:
                        {
                            if (!(viewHolder is AdapterHolders.Post4ImageSectionViewHolder holder))
                                return;

                            if (item.PostData.PhotoMulti?.Count == 4 || item.PostData.PhotoAlbum?.Count == 4)
                            {
                                var imagesList = item.PostData.PhotoMulti ?? item.PostData.PhotoAlbum;

                                FullGlideRequestBuilder.Load(imagesList[0].Image).Into(holder.Image);
                                FullGlideRequestBuilder.Load(imagesList[1].Image).Into(holder.Image2);
                                FullGlideRequestBuilder.Load(imagesList[2].Image).Into(holder.Image3);
                                FullGlideRequestBuilder.Load(imagesList[3].Image).Into(holder.Image4);
                            }

                            break;
                        }
                    case (int)PostModelType.MultiImages:
                        {
                            if (!(viewHolder is AdapterHolders.PostMultiImagesViewHolder holder))
                                return;

                            if (item.PostData.PhotoMulti?.Count > 4 || item.PostData.PhotoAlbum?.Count > 4)
                            {
                                var imagesList = item.PostData.PhotoMulti ?? item.PostData.PhotoAlbum;

                                FullGlideRequestBuilder.Load(imagesList[0].Image).Into(holder.Image);
                                FullGlideRequestBuilder.Load(imagesList[1].Image).Into(holder.Image2);
                                FullGlideRequestBuilder.Load(imagesList[2].Image).Into(holder.Image3);

                                holder.CountImageLabel.Text = "+" + (imagesList?.Count - 2);
                            }

                            break;
                        }
                    case (int)PostModelType.VideoPost:
                        {
                            if (!(viewHolder is AdapterHolders.PostVideoSectionViewHolder holder))
                                return;

                            if (!string.IsNullOrEmpty(item.PostData.PostFileThumb))
                                FullGlideRequestBuilder.Load(item.PostData.PostFileThumb).Into(holder.VideoImage);
                            else
                                FullGlideRequestBuilder.Load(item.PostData.PostFileFull).Into(holder.VideoImage);

                            //Glide.With(ActivityContext)
                            //        .AsBitmap()
                            //        .Placeholder(Resource.Drawable.blackdefault)
                            //        .Error(Resource.Drawable.blackdefault)
                            //        .Load(item.PostData.PostFileFull) // or URI/path
                            //        .Into(holder.VideoImage); //image view to set thumbnail to 

                            break;
                        }
                    case (int)PostModelType.BlogPost:
                        {
                            if (!(viewHolder is AdapterHolders.PostBlogSectionViewHolder holder))
                                return;

                            if (!string.IsNullOrEmpty(item.PostData?.Blog.Thumbnail))
                                FullGlideRequestBuilder.Load(item.PostData?.Blog.Thumbnail).Into(holder.ImageBlog);

                            holder.PostBlogText.Text = item.PostData?.Blog.Title;
                            holder.PostBlogContent.Text = item.PostData?.Blog.Description;
                            holder.CatText.Text = item.PostData?.Blog.CategoryName;
                            break;
                        }
                    case (int)PostModelType.ColorPost:
                        {
                            if (!(viewHolder is AdapterHolders.PostColorBoxSectionViewHolder holder))
                                return;

                            if (!string.IsNullOrEmpty(item.PostData.ColorBoxImageUrl))
                                FullGlideRequestBuilder.Load(item.PostData.ColorBoxImageUrl).Into(holder.ColorBoxImage);

                            if (item.PostData.ColorBoxGradientDrawable != null)
                                holder.ColorBoxImage.Background = item.PostData.ColorBoxGradientDrawable;

                            if (item.PostData != null)
                                holder.DesTextView.SetTextColor(item.PostData.ColorBoxTextColor);

                            holder.DesTextView.Text = item.PostData.Orginaltext;
                            break;
                        }
                    case (int)PostModelType.EventPost:
                        {
                            if (!(viewHolder is AdapterHolders.EventPostViewHolder holder))
                                return;
                             
                            if (item.PostData.Event?.EventClass != null)
                            { 
                                FullGlideRequestBuilder.Load(item.PostData?.Event?.EventClass.Cover).Into(holder.Image);
                                holder.TxtEventTitle.Text = item.PostData?.Event?.EventClass?.Name;
                                holder.TxtEventDescription.Text = item.PostData?.Event?.EventClass?.Description;
                                holder.TxtEventLocation.Text = item.PostData?.Event?.EventClass?.Location;
                            }
                            break;
                        }
                    case (int)PostModelType.LinkPost:
                        {
                            if (!(viewHolder is AdapterHolders.LinkPostViewHolder holder))
                                return;

                            holder.LinkUrl.Text = item.PostData?.PostLink;
                            holder.PostLinkTitle.Text = item.PostData?.PostLinkTitle;
                            holder.PostLinkContent.Text = item.PostData?.PostLinkContent;

                            if (string.IsNullOrEmpty(item.PostData?.PostLinkImage) || item.PostData.PostLinkTitle.Contains("Page Not Found") || item.PostData.PostLinkContent.Contains("See posts, photos and more on Facebook."))
                                holder.Image.Visibility = ViewStates.Gone;
                            else
                            {
                                if (item.PostData.PostLink.Contains("www.facebook.com"))
                                    FullGlideRequestBuilder.Clone().Load(item.PostData.PostLinkImage).Error(Resource.Drawable.facebook).Placeholder(Resource.Drawable.facebook).Into(holder.Image);
                                else
                                    FullGlideRequestBuilder.Clone().Load(item.PostData.PostLinkImage).Placeholder(new ColorDrawable(Color.ParseColor("#efefef"))).Into(holder.Image);
                            }

                            break;
                        }
                    case (int)PostModelType.FilePost:
                        {
                            if (!(viewHolder is AdapterHolders.FilePostViewHolder holder))
                                return;

                            holder.PostFileText.Text = item.PostData.PostFileName;
                            break;
                        }
                    case (int)PostModelType.FundingPost:
                        {
                            if (!(viewHolder is AdapterHolders.FundingPostViewHolder holder))
                                return;

                            if (item.PostData.FundData != null)
                            {
                                FullGlideRequestBuilder.Load(item.PostData.FundData.Value.FundDataClass.Image).Into(holder.Image);

                                holder.Title.Text = item.PostData.FundData.Value.FundDataClass.Title;
                                holder.DonationTime.Text = item.PostData.FundData.Value.FundDataClass.Time;
                                holder.Description.Text = item.PostData.FundData.Value.FundDataClass.Description;
                                holder.Raised.Text = item.PostData.FundData.Value.FundDataClass.Raised;
                                holder.TottalAmount.Text = item.PostData.FundData.Value.FundDataClass.Amount;
                                holder.Progress.Progress = Convert.ToInt32(item.PostData.FundData.Value.FundDataClass.Bar);

                                item.PostData.FundData.Value.FundDataClass.UserData ??= item.PostData.Publisher;
                            }

                            break;
                        }
                    case (int)PostModelType.ProductPost:
                        {
                            if (!(viewHolder is AdapterHolders.ProductPostViewHolder holder))
                                return;

                            if (item.PostData.Product != null)
                            {
                                if (item.PostData.Product.Value.ProductClass?.Images.Count > 0)
                                    FullGlideRequestBuilder.Load(item.PostData.Product.Value.ProductClass?.Images[0].Image)
                                        .Into(holder.Image);

                                if (item.PostData.Product.Value.ProductClass?.Seller == null)
                                    if (item.PostData.Product.Value.ProductClass != null)
                                        item.PostData.Product.Value.ProductClass.Seller = item.PostData.Publisher;

                                if (!string.IsNullOrEmpty(item.PostData.Product.Value.ProductClass?.LocationDecodedText))
                                    holder.PostProductLocationText.Text = item.PostData.Product.Value.ProductClass?.LocationDecodedText;
                                else
                                {
                                    holder.PostProductLocationText.Visibility = ViewStates.Gone;
                                    holder.LocationIcon.Visibility = ViewStates.Gone;
                                }

                                holder.PostLinkTitle.Text = item.PostData.Product.Value.ProductClass?.Name;
                                holder.PostProductContent.Text = item.PostData.Product.Value.ProductClass?.Description;
                                holder.PriceText.Text = item.PostData.Product.Value.ProductClass?.CurrencyText;
                                holder.TypeText.Text = item.PostData.Product.Value.ProductClass?.TypeDecodedText;
                                holder.StatusText.Text = item.PostData.Product.Value.ProductClass?.StatusDecodedText;
                            }

                            break;
                        }
                    case (int)PostModelType.VoicePost:
                        {
                            if (!(viewHolder is AdapterHolders.SoundPostViewHolder holder))
                                return;

                            Console.WriteLine(holder);
                            break;
                        }
                    case (int)PostModelType.YoutubePost:
                        {
                            if (!(viewHolder is AdapterHolders.YoutubePostViewHolder holder))
                                return;

                            FullGlideRequestBuilder.Load("https://img.youtube.com/vi/" + item.PostData.PostYoutube + "/0.jpg").Into(holder.Image);
                            break;
                        }
                    case (int)PostModelType.PlayTubePost:
                        {
                            if (!(viewHolder is AdapterHolders.PostPlayTubeContentViewHolder holder))
                                return;

                            var playTubeUrl = ListUtils.SettingsSiteList?.PlaytubeUrl;

                            var fullEmbedUrl = playTubeUrl + "/embed/" + item.PostData.PostPlaytube;

                            if (AppSettings.EmbedPlayTubePostType)
                            {
                                var vc = holder.WebView.LayoutParameters;
                                vc.Height = 600;
                                holder.WebView.LayoutParameters = vc;

                                holder.WebView.LoadUrl(fullEmbedUrl);
                            }
                            else
                            {
                                item.PostData.PostLink = fullEmbedUrl;
                                holder.WebView.Visibility = ViewStates.Gone;
                            }

                            break;
                        }
                    case (int)PostModelType.LivePost:
                        {
                            if (!(viewHolder is AdapterHolders.PostPlayTubeContentViewHolder holder))
                                return;

                            var liveUrl = "https://viewer.millicast.com/v2?streamId=";
                            var id = ListUtils.SettingsSiteList?.LiveAccountId;
                            string fullEmbedUrl = liveUrl + id + "/" + item.PostData.StreamName;

                            if (AppSettings.EmbedLivePostType)
                            {
                                var vc = holder.WebView.LayoutParameters;
                                vc.Height = 600;
                                holder.WebView.LayoutParameters = vc;

                                holder.WebView.LoadUrl(fullEmbedUrl);
                            }
                            else
                            {
                                item.PostData.PostLink = fullEmbedUrl;
                                holder.WebView.Visibility = ViewStates.Gone;
                            }
                            break;
                        }
                    case (int)PostModelType.DeepSoundPost:
                        {
                            if (!(viewHolder is AdapterHolders.PostPlayTubeContentViewHolder holder))
                                return;

                            var deepSoundUrl = ListUtils.SettingsSiteList?.DeepsoundUrl;

                            var fullEmbedUrl = deepSoundUrl + "/embed/" + item.PostData.PostDeepsound;

                            if (AppSettings.EmbedDeepSoundPostType)
                            {
                                var vc = holder.WebView.LayoutParameters;
                                vc.Height = 480;
                                holder.WebView.LayoutParameters = vc;

                                holder.WebView.LoadUrl(fullEmbedUrl);
                            }
                            else
                            {
                                item.PostData.PostLink = fullEmbedUrl;
                                holder.WebView.Visibility = ViewStates.Gone;
                            }
                            break;
                        }
                    case (int)PostModelType.VimeoPost:
                        {
                            if (!(viewHolder is AdapterHolders.PostPlayTubeContentViewHolder holder))
                                return;

                            var fullEmbedUrl = "https://player.vimeo.com/video/" + item.PostData.PostVimeo;

                            if (AppSettings.EmbedVimeoVideoPostType)
                            {
                                var vc = holder.WebView.LayoutParameters;
                                vc.Height = 700;
                                holder.WebView.LayoutParameters = vc;

                                holder.WebView.LoadUrl(fullEmbedUrl);
                            }
                            else
                            {
                                item.PostData.PostLink = fullEmbedUrl;
                                holder.WebView.Visibility = ViewStates.Gone;
                            }
                            break;
                        }
                    case (int)PostModelType.FacebookPost:
                        {
                            if (!(viewHolder is AdapterHolders.PostPlayTubeContentViewHolder holder))
                                return;

                            var fullEmbedUrl = "https://www.facebook.com/video/embed?video_id=" + item.PostData.PostFacebook.Split("/videos/").Last();

                            if (AppSettings.EmbedFacebookVideoPostType)
                            {
                                var vc = holder.WebView.LayoutParameters;
                                vc.Height = 700;
                                holder.WebView.LayoutParameters = vc;

                                //Load url to be rendered on WebView 
                                holder.WebView.LoadUrl(fullEmbedUrl);
                                holder.WebView.Visibility = ViewStates.Visible;
                            }
                            else
                            {
                                item.PostData.PostLink = fullEmbedUrl;
                                holder.WebView.Visibility = ViewStates.Gone;
                            }
                            break;
                        }
                    case (int)PostModelType.OfferPost:
                        {
                            if (!(viewHolder is AdapterHolders.OfferPostViewHolder holder))
                                return;

                            if (!string.IsNullOrEmpty(item.PostData.Offer?.OfferClass?.Image))
                                FullGlideRequestBuilder.Load(item.PostData.Offer?.OfferClass?.Image).Into(holder.ImageBlog);

                            holder.PostBlogText.Text = item.PostData.Offer?.OfferClass?.OfferText;
                            holder.PostBlogContent.Text = item.PostData.Offer?.OfferClass?.Description;
                            break;
                        }
                    case (int)PostModelType.JobPostSection1:
                        {
                            if (!(viewHolder is AdapterHolders.JobPostViewHolder1 holder))
                                return;

                            if (item.PostData.Job != null)
                            {

                                if (item.PostData.Job.Value.JobInfoClass.Page != null)
                                    FullGlideRequestBuilder.Load(item.PostData.Job.Value.JobInfoClass.Page.Avatar).Into(holder.JobAvatar);

                                FullGlideRequestBuilder.Load(item.PostData.Job.Value.JobInfoClass.Image).Into(holder.JobCoverImage);

                                holder.JobTitle.Text = item.PostData.Job.Value.JobInfoClass.Title;
                                holder.PageName.Text = item.PostData.Job?.JobInfoClass.Page.PageName;

                            }
                            break;
                        }
                    case (int)PostModelType.JobPostSection2:
                        {
                            if (!(viewHolder is AdapterHolders.JobPostViewHolder2 holder))
                                return;

                            if (item.PostData.Job != null)
                            {
                                //Set Button if its applied
                                if (item.PostData.Job.Value.JobInfoClass.Apply == "true")
                                    holder.JobButton.Enabled = false;

                                holder.JobButton.Text = item.PostData.Job?.JobInfoClass.ButtonText;
                                holder.Description.Text = item.PostData.Job?.JobInfoClass.Description;
                                holder.MinimumNumber.Text = item.PostData.Job?.JobInfoClass.Minimum + " " + item.PostData.Job?.JobInfoClass.SalaryDate;
                                holder.MaximumNumber.Text = item.PostData.Job?.JobInfoClass.Maximum + " " + item.PostData.Job?.JobInfoClass.SalaryDate;
                            }

                            break;
                        }
                    case (int)PostModelType.PollPost:
                        {
                            if (!(viewHolder is AdapterHolders.PollsPostViewHolder holder))
                                return;

                            holder.VoteText.Text = item.PollsOption.Text;
                            holder.ProgressBarView.Progress = int.Parse(item.PollsOption.PercentageNum);
                            holder.ProgressText.Text = item.PollsOption.Percentage;

                            if (!string.IsNullOrEmpty(item.PostData.VotedId) && item.PostData.VotedId != "0")
                            {
                                if (item.PollsOption.Id == item.PostData.VotedId)
                                {
                                    holder.CheckIcon.SetImageResource(Resource.Drawable.icon_checkmark_filled_vector);
                                    holder.CheckIcon.ClearColorFilter();
                                }
                                else
                                {
                                    holder.CheckIcon.SetImageResource(Resource.Drawable.icon_check_circle_vector);
                                    holder.CheckIcon.SetColorFilter(new PorterDuffColorFilter(Color.ParseColor("#999999"), PorterDuff.Mode.SrcAtop));
                                }
                            }
                            else
                            {
                                holder.CheckIcon.SetImageResource(Resource.Drawable.icon_check_circle_vector);
                                holder.CheckIcon.SetColorFilter(new PorterDuffColorFilter(Color.ParseColor("#999999"), PorterDuff.Mode.SrcAtop));
                            }

                            break;
                        }
                    case (int)PostModelType.AlertBox:
                        {
                            if (!(viewHolder is AdapterHolders.AlertAdapterViewHolder holder))
                                return;

                            if (!string.IsNullOrEmpty(item.AlertModel?.TitleHead))
                                holder.HeadText.Text = item.AlertModel?.TitleHead;

                            if (!string.IsNullOrEmpty(item.AlertModel?.SubText))
                                holder.SubText.Text = item.AlertModel?.SubText;

                            if (item.AlertModel?.ImageDrawable != null)
                                holder.Image.SetImageResource(item.AlertModel.ImageDrawable);

                            if (!string.IsNullOrEmpty(item.AlertModel?.LinerColor))
                                holder.LineView.SetBackgroundColor(Color.ParseColor(item.AlertModel?.LinerColor));

                            break;
                        }
                    case (int)PostModelType.AlertBoxAnnouncement:
                        {
                            if (!(viewHolder is AdapterHolders.AlertAdapterViewHolder holder))
                                return;

                            if (!string.IsNullOrEmpty(item.AlertModel?.TitleHead))
                                holder.HeadText.Text = Methods.FunString.DecodeString(item.AlertModel?.TitleHead);

                            if (!string.IsNullOrEmpty(item.AlertModel?.SubText))
                                holder.SubText.Text = Methods.FunString.DecodeString(item.AlertModel?.SubText);

                            break;
                        }
                    case (int)PostModelType.AlertJoinBox:
                        {
                            if (!(viewHolder is AdapterHolders.AlertJoinAdapterViewHolder holder))
                                return;

                            if (!string.IsNullOrEmpty(item.AlertModel?.TitleHead))
                                holder.HeadText.Text = item.AlertModel?.TitleHead;

                            if (!string.IsNullOrEmpty(item.AlertModel?.SubText))
                                holder.SubText.Text = item.AlertModel?.SubText;

                            if (item.AlertModel?.ImageDrawable != null)
                                holder.NormalImageView.SetImageResource(item.AlertModel.ImageDrawable);
                            else
                                holder.NormalImageView.Visibility = ViewStates.Gone;

                            if (item.AlertModel?.IconImage != null)
                                holder.IconImageView.SetImageResource(item.AlertModel.IconImage);

                            switch (item.AlertModel?.TypeAlert)
                            {
                                case "Groups":
                                    holder.MainRelativeLayout.SetBackgroundResource(Resource.Drawable.Shape_Gradient_Linear);
                                    holder.ButtonView.Text = ActivityContext.GetString(Resource.String.Lbl_FindYourGroups);
                                    break;
                                case "Pages":
                                    holder.MainRelativeLayout.SetBackgroundResource(Resource.Drawable.Shape_Gradient_Linear1);
                                    holder.ButtonView.Text = ActivityContext.GetString(Resource.String.Lbl_FindPopularPages);
                                    break;
                            }
                            break;
                        }
                    case (int)PostModelType.Section:
                        {
                            if (!(viewHolder is AdapterHolders.SectionViewHolder holder))
                                return;

                            holder.AboutHead.Text = item.AboutModel.TitleHead;
                            holder.AboutMoreIcon.Visibility = ViewStates.Gone;

                            break;
                        }
                    case (int)PostModelType.AddPostBox:
                        {
                            if (!(viewHolder is AdapterHolders.AddPostViewHolder holder))
                                return;

                            GlideImageLoader.LoadImage(ActivityContext, UserDetails.Avatar, holder.ProfileImageView, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                            break;
                        }
                    case (int)PostModelType.SearchForPosts:
                        {
                            if (!(viewHolder is AdapterHolders.SearchForPostsViewHolder holder))
                                return;
                            Console.WriteLine(holder);
                            break;
                        }
                    case (int)PostModelType.SocialLinks:
                        {
                            if (!(viewHolder is AdapterHolders.SocialLinksViewHolder holder))
                                return;

                            if (Methods.FunString.StringNullRemover(item.SocialLinksModel.Facebook) == "Empty")
                            {
                                holder.BtnFacebook.Enabled = false;
                                holder.BtnFacebook.SetColor(Color.ParseColor("#8c8a8a"));
                            }
                            else
                                holder.BtnFacebook.Enabled = true;

                            if (Methods.FunString.StringNullRemover(item.SocialLinksModel.Google) == "Empty")
                            {
                                holder.BtnGoogle.Enabled = false;
                                holder.BtnGoogle.SetColor(Color.ParseColor("#8c8a8a"));
                            }
                            else
                                holder.BtnGoogle.Enabled = true;

                            if (Methods.FunString.StringNullRemover(item.SocialLinksModel.Twitter) == "Empty")
                            {
                                holder.BtnTwitter.Enabled = false;
                                holder.BtnTwitter.SetColor(Color.ParseColor("#8c8a8a"));
                            }
                            else
                                holder.BtnTwitter.Enabled = true;

                            if (Methods.FunString.StringNullRemover(item.SocialLinksModel.Youtube) == "Empty")
                            {
                                holder.BtnYoutube.Enabled = false;
                                holder.BtnYoutube.SetColor(Color.ParseColor("#8c8a8a"));
                            }
                            else
                                holder.BtnYoutube.Enabled = true;

                            if (Methods.FunString.StringNullRemover(item.SocialLinksModel.Vk) == "Empty")
                            {
                                holder.BtnVk.Enabled = false;
                                holder.BtnVk.SetColor(Color.ParseColor("#8c8a8a"));
                            }
                            else
                                holder.BtnVk.Enabled = true;

                            if (Methods.FunString.StringNullRemover(item.SocialLinksModel.Instegram) == "Empty")
                            {
                                holder.BtnInstegram.Enabled = false;
                                holder.BtnInstegram.SetColor(Color.ParseColor("#8c8a8a"));
                            }
                            else
                                holder.BtnInstegram.Enabled = true;

                            break;
                        }
                    case (int)PostModelType.AboutBox:
                        {
                            if (!(viewHolder is AdapterHolders.AboutBoxViewHolder holder))
                                return;

                            if (!string.IsNullOrEmpty(item.AboutModel.TitleHead))
                                holder.AboutHead.Text = item.AboutModel.TitleHead;

                            holder.AboutDescription.SetAutoLinkOnClickListener(this, new Dictionary<string, string>());
                            holder.AboutDescription.Text = Methods.FunString.DecodeString(item.AboutModel.Description);
                            ReadMoreOption.AddReadMoreTo(holder.AboutDescription, new String(holder.AboutDescription.Text));


                            break;
                        }
                    case (int)PostModelType.Story:
                        {
                            if (!(viewHolder is AdapterHolders.StoryViewHolder holder))
                                return;

                            HolderStory = holder;

                            if (item.StoryList.Count > 0)
                            {
                                holder.StoryAdapter.StoryList = new ObservableCollection<GetUserStoriesObject.StoryObject>(item.StoryList);
                            }

                            var dataOwner = holder.StoryAdapter.StoryList.FirstOrDefault(a => a.Type == "Your");
                            if (dataOwner == null)
                            {
                                holder.StoryAdapter.StoryList.Insert(0, new GetUserStoriesObject.StoryObject()
                                {
                                    Avatar = UserDetails.Avatar,
                                    Type = "Your",
                                    Username = ActivityContext.GetText(Resource.String.Lbl_YourStory),
                                    Stories = new List<GetUserStoriesObject.StoryObject.Story>()
                                        {
                                            new GetUserStoriesObject.StoryObject.Story()
                                            {
                                                Thumbnail = UserDetails.Avatar,
                                            }
                                        }
                                });
                            }

                            holder.StoryAdapter.NotifyDataSetChanged();


                            if (holder.StoryAdapter?.StoryList?.Count > 4)
                            {
                                holder.AboutMore.Visibility = ViewStates.Visible;
                                holder.AboutMoreIcon.Visibility = ViewStates.Visible;
                            }
                            else
                            {
                                holder.AboutMore.Visibility = ViewStates.Invisible;
                                holder.AboutMoreIcon.Visibility = ViewStates.Invisible;
                            }

                            break;
                        }
                    case (int)PostModelType.FollowersBox:
                        {
                            if (!(viewHolder is AdapterHolders.FollowersViewHolder holder))
                                return;

                            if (holder.FollowersAdapter?.MUserFriendsList?.Count == 0)
                            {
                                holder.FollowersAdapter.MUserFriendsList = new ObservableCollection<UserDataObject>(item.FollowersModel.FollowersList);
                                holder.FollowersAdapter.NotifyDataSetChanged();
                            }

                            if (!string.IsNullOrEmpty(item.FollowersModel.TitleHead))
                                holder.AboutHead.Text = item.FollowersModel.TitleHead;

                            holder.AboutMore.Text = item.FollowersModel.More;

                            if (holder.FollowersAdapter?.MUserFriendsList?.Count > 4)
                            {
                                holder.AboutMore.Visibility = ViewStates.Visible;
                                holder.AboutMoreIcon.Visibility = ViewStates.Visible;

                            }
                            else
                            {
                                holder.AboutMore.Visibility = ViewStates.Invisible;
                                holder.AboutMoreIcon.Visibility = ViewStates.Invisible;
                            }

                            break;
                        }
                    case (int)PostModelType.GroupsBox:
                        {
                            if (!(viewHolder is AdapterHolders.GroupsViewHolder holder))
                                return;

                            if (holder.GroupsAdapter?.GroupList?.Count == 0)
                            {
                                holder.GroupsAdapter.GroupList = new ObservableCollection<GroupClass>(item.GroupsModel.GroupsList);
                                holder.GroupsAdapter.NotifyDataSetChanged();
                            }

                            if (!string.IsNullOrEmpty(item.GroupsModel?.TitleHead))
                                holder.AboutHead.Text = item.GroupsModel?.TitleHead;

                            holder.AboutMore.Text = item.GroupsModel?.More;

                            if (holder.GroupsAdapter != null)
                            {
                                if (holder.GroupsAdapter?.GroupList?.Count > 4)
                                {
                                    holder.AboutMore.Visibility = ViewStates.Visible;
                                    holder.AboutMoreIcon.Visibility = ViewStates.Visible;
                                }
                                else
                                {
                                    holder.AboutMore.Visibility = ViewStates.Invisible;
                                    holder.AboutMoreIcon.Visibility = ViewStates.Invisible;
                                }
                            }

                            break;
                        }
                    case (int)PostModelType.SuggestedGroupsBox:
                        {
                            if (!(viewHolder is AdapterHolders.SuggestedGroupsViewHolder holder))
                                return;

                            if (holder.GroupsAdapter?.GroupList?.Count == 0)
                            {
                                holder.GroupsAdapter.GroupList = new ObservableCollection<GroupClass>(ListUtils.SuggestedGroupList.Take(12));
                                holder.GroupsAdapter.NotifyDataSetChanged();
                                holder.AboutMore.Text = ListUtils.SuggestedGroupList.Count.ToString();

                                if (holder.GroupsAdapter?.GroupList?.Count > 4)
                                {
                                    holder.AboutMore.Visibility = ViewStates.Visible;
                                    holder.AboutMoreIcon.Visibility = ViewStates.Visible;
                                }
                                else
                                {
                                    holder.AboutMore.Visibility = ViewStates.Invisible;
                                    holder.AboutMoreIcon.Visibility = ViewStates.Invisible;
                                }
                            }

                            break;
                        }
                    case (int)PostModelType.SuggestedUsersBox:
                        {
                            if (!(viewHolder is AdapterHolders.SuggestedUsersViewHolder holder))
                                return;

                            if (holder.UsersAdapter?.UserList?.Count == 0)
                            {
                                holder.UsersAdapter.UserList = new ObservableCollection<UserDataObject>(ListUtils.SuggestedUserList.Take(12));
                                holder.UsersAdapter.NotifyDataSetChanged();
                                holder.AboutMore.Text = ListUtils.SuggestedUserList.Count.ToString();

                                if (holder.UsersAdapter?.UserList?.Count > 4)
                                {
                                    holder.AboutMore.Visibility = ViewStates.Visible;
                                    holder.AboutMoreIcon.Visibility = ViewStates.Visible;
                                }
                                else
                                {
                                    holder.AboutMore.Visibility = ViewStates.Invisible;
                                    holder.AboutMoreIcon.Visibility = ViewStates.Invisible;
                                }
                            }

                            break;
                        }
                    case (int)PostModelType.ImagesBox:
                        {
                            if (!(viewHolder is AdapterHolders.ImagesViewHolder holder))
                                return;

                            if (!string.IsNullOrEmpty(item.ImagesModel.TitleHead))
                                holder.AboutHead.Text = item.ImagesModel.TitleHead;

                            holder.AboutMore.Text = item.ImagesModel.More;

                            if (item.ImagesModel?.ImagesList == null)
                            {
                                holder.MainView.Visibility = ViewStates.Gone;
                                return;
                            }

                            if (holder.MainView.Visibility != ViewStates.Visible)
                                holder.MainView.Visibility = ViewStates.Visible;

                            if (holder.ImagesAdapter?.UserPhotosList?.Count == 0)
                            {
                                holder.ImagesAdapter.UserPhotosList = new ObservableCollection<PostDataObject>(item.ImagesModel.ImagesList);
                                holder.ImagesAdapter.NotifyDataSetChanged();
                            }

                            if (holder.ImagesAdapter?.UserPhotosList?.Count > 3)
                            {
                                holder.AboutMore.Visibility = ViewStates.Visible;
                                holder.AboutMoreIcon.Visibility = ViewStates.Visible;
                            }
                            else
                            {
                                holder.AboutMore.Visibility = ViewStates.Invisible;
                                holder.AboutMoreIcon.Visibility = ViewStates.Invisible;
                            }
                            break;
                        }
                    case (int)PostModelType.PagesBox:
                        {
                            if (!(viewHolder is AdapterHolders.PagesViewHolder holder))
                                return;

                            if (!string.IsNullOrEmpty(item.PagesModel?.TitleHead))
                                holder.AboutHead.Text = item.PagesModel?.TitleHead;

                            if (!string.IsNullOrEmpty(item.PagesModel?.More))
                                holder.AboutMore.Text = item.PagesModel?.More;

                            var count = item.PagesModel?.PagesList.Count;
                            Console.WriteLine(count);

                            try
                            {
                                if (!string.IsNullOrEmpty(item.PagesModel?.PagesList[0]?.Avatar))
                                    GlideImageLoader.LoadImage(ActivityContext, item.PagesModel?.PagesList[0]?.Avatar, holder.PageImage1, ImageStyle.CircleCrop, ImagePlaceholders.Color);

                                if (!string.IsNullOrEmpty(item.PagesModel?.PagesList[1]?.Avatar))
                                    GlideImageLoader.LoadImage(ActivityContext, item.PagesModel?.PagesList[1]?.Avatar, holder.PageImage2, ImageStyle.CircleCrop, ImagePlaceholders.Color);

                                if (!string.IsNullOrEmpty(item.PagesModel?.PagesList[2]?.Avatar))
                                    GlideImageLoader.LoadImage(ActivityContext, item.PagesModel?.PagesList[2]?.Avatar, holder.PageImage1, ImageStyle.CircleCrop, ImagePlaceholders.Color);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }
                            break;
                        }
                    case (int)PostModelType.AdsPost:
                        {
                            if (!(viewHolder is AdapterHolders.AdsPostViewHolder holder))
                                return;

                            FullGlideRequestBuilder.Load(item.PostData.UserData.Avatar).Into(holder.UserAvatar);

                            if (string.IsNullOrEmpty(item.PostData.AdMedia))
                                holder.Image.Visibility = ViewStates.Gone;
                            else
                                FullGlideRequestBuilder.Load(item.PostData.AdMedia).Into(holder.Image);

                            holder.Username.Text = item.PostData.Name;
                            holder.TimeText.Text = item.PostData.Posted;
                            holder.TextLocation.Text = item.PostData.Location;

                            if (string.IsNullOrEmpty(item.PostData.Orginaltext))
                            {
                                if (holder.Description.Visibility != ViewStates.Gone)
                                    holder.Description.Visibility = ViewStates.Gone;
                            }
                            else
                            {
                                if (holder.Description.Visibility != ViewStates.Visible)
                                    holder.Description.Visibility = ViewStates.Visible;

                                if (!holder.Description.Text.Contains(ActivityContext.GetText(Resource.String.Lbl_ReadMore)) && !holder.Description.Text.Contains(ActivityContext.GetText(Resource.String.Lbl_ReadLess)))
                                {
                                    if (item.PostData.RegexFilterList != null & item.PostData.RegexFilterList?.Count > 0)
                                        holder.Description.SetAutoLinkOnClickListener(this, item.PostData.RegexFilterList);
                                    else
                                        holder.Description.SetAutoLinkOnClickListener(this, new Dictionary<string, string>());

                                    ReadMoreOption.AddReadMoreTo(holder.Description, new String(item.PostData.Orginaltext));
                                }
                                else if (holder.Description.Text.Contains(ActivityContext.GetText(Resource.String.Lbl_ReadLess)))
                                {
                                    ReadMoreOption.AddReadLess(holder.Description, new String(item.PostData.Orginaltext));
                                }
                                else
                                {
                                    holder.Description.Text = item.PostData.Orginaltext;
                                }
                            }

                            TextSanitizer headlineSanitizer = new TextSanitizer(holder.Headline, ActivityContext);
                            headlineSanitizer.Load(item.PostData.Headline);

                            holder.LinkUrl.Text = item.PostData.Url;

                            break;
                        }
                    case (int)PostModelType.EmptyState:
                        {
                            if (!(viewHolder is AdapterHolders.EmptyStateAdapterViewHolder holder))
                                return;

                            BindEmptyState(holder);

                            break;
                        }
                    case (int)PostModelType.AdMob1:
                    case (int)PostModelType.AdMob2:
                    case (int)PostModelType.FbAdNative:
                    case (int)PostModelType.Divider:
                        break;
                    case (int)PostModelType.ViewProgress:
                        {
                            if (!(viewHolder is AdapterHolders.ProgressViewHolder holder))
                                return;
                            Console.WriteLine(holder);
                            break;
                        }
                    default:
                        {
                            if (!(viewHolder is AdapterHolders.PostDefaultSectionViewHolder holder))
                                return;
                            Console.WriteLine(holder);
                            break;
                        }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception); Log.Debug("wael >> NativePostAdapter", exception.Message + "\n" + exception.StackTrace + "\n" + exception.HelpLink);
            }
        }

        public void LoadCommentData(CommentObjectExtra item, RecyclerView.ViewHolder viewHolder)
        {
            try
            {
                if (!(viewHolder is CommentAdapterViewHolder holder))
                    return;
                 
                if (!string.IsNullOrEmpty(item.Text) || !string.IsNullOrWhiteSpace(item.Text))
                {
                    var changer = new TextSanitizer(holder.CommentText, ActivityContext);
                    changer.Load(Methods.FunString.DecodeString(item.Text));
                }
                else
                {
                    holder.CommentText.Visibility = ViewStates.Gone;
                }

                holder.TimeTextView.Text = Methods.Time.TimeAgo(int.Parse(item.Time));
                holder.UserName.Text = item.Publisher.Name;

                GlideImageLoader.LoadImage(ActivityContext, item.Publisher.Avatar, holder.Image, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                var textHighLighter = item.Publisher.Name;
                var textIsPro = string.Empty;

                if (item.Publisher.Verified == "1")
                    textHighLighter += " " + IonIconsFonts.CheckmarkCircled;

                if (item.Publisher.IsPro == "1")
                {
                    textIsPro = " " + IonIconsFonts.Flash;
                    textHighLighter += textIsPro;
                }

                var decorator = TextDecorator.Decorate(holder.UserName, textHighLighter).SetTextStyle((int)TypefaceStyle.Bold, 0, item.Publisher.Name.Length);

                if (item.Publisher.Verified == "1")
                    decorator.SetTextColor(Resource.Color.Post_IsVerified, IonIconsFonts.CheckmarkCircled);

                if (item.Publisher.IsPro == "1")
                    decorator.SetTextColor(Resource.Color.text_color_in_between, textIsPro);

                decorator.Build();

                //Image
                if (holder.ItemViewType == 1 || holder.CommentImage != null)
                {
                    //if (!string.IsNullOrEmpty(item.CFile) && (item.CFile.Contains("file://") || item.CFile.Contains("content://") || item.CFile.Contains("storage") || item.CFile.Contains("/data/user/0/")))
                    //{
                    //    File file2 = new File(item.CFile);
                    //    var photoUri = FileProvider.GetUriForFile(ActivityContext, ActivityContext.PackageName + ".fileprovider", file2);
                    //    Glide.With(ActivityContext).Load(photoUri).Apply(new RequestOptions()).Into(holder.CommentImage);

                    //    //GlideImageLoader.LoadImage(ActivityContext,item.CFile, holder.CommentImage, ImageStyle.CenterCrop, ImagePlaceholders.Color);
                    //}
                    //else
                    //{
                    //    if (!item.CFile.Contains(Client.WebsiteUrl))
                    //        item.CFile = WoWonderTools.GetTheFinalLink(item.CFile);

                    //    GlideImageLoader.LoadImage(ActivityContext, item.CFile, holder.CommentImage, ImageStyle.CenterCrop, ImagePlaceholders.Color);
                    //}
                }

                //Voice
                if (holder.VoiceLayout != null && !string.IsNullOrEmpty(item.Record))
                {
                    //LoadAudioItem(holder, position, item);
                }

                if (item.Replies != "0" && item.Replies != null)
                    holder.ReplyTextView.Text = ActivityContext.GetText(Resource.String.Lbl_Reply) + " " + "(" + item.Replies + ")";

                if (AppSettings.PostButton == PostButtonSystem.ReactionDefault || AppSettings.PostButton == PostButtonSystem.ReactionSubShine)
                {
                    item.Reaction ??= new WoWonderClient.Classes.Posts.Reaction();

                    if ((bool)(item.Reaction != null & item.Reaction?.IsReacted))
                    {
                        if (!string.IsNullOrEmpty(item.Reaction.Type))
                        {
                            switch (item.Reaction.Type)
                            {
                                case "1":
                                case "Like":
                                    ReactionComment.SetReactionPack(holder, ReactConstants.Like);
                                    holder.LikeTextView.Tag = "Liked";
                                    break;
                                case "2":
                                case "Love":
                                    ReactionComment.SetReactionPack(holder, ReactConstants.Love);
                                    holder.LikeTextView.Tag = "Liked";
                                    break;
                                case "3":
                                case "HaHa":
                                    ReactionComment.SetReactionPack(holder, ReactConstants.HaHa);
                                    holder.LikeTextView.Tag = "Liked";
                                    break;
                                case "4":
                                case "Wow":
                                    ReactionComment.SetReactionPack(holder, ReactConstants.Wow);
                                    holder.LikeTextView.Tag = "Liked";
                                    break;
                                case "5":
                                case "Sad":
                                    ReactionComment.SetReactionPack(holder, ReactConstants.Sad);
                                    holder.LikeTextView.Tag = "Liked";
                                    break;
                                case "6":
                                case "Angry":
                                    ReactionComment.SetReactionPack(holder, ReactConstants.Angry);
                                    holder.LikeTextView.Tag = "Liked";
                                    break;
                                default:
                                    holder.LikeTextView.Text = ActivityContext.GetText(Resource.String.Btn_Like);
                                    if (AppSettings.SetTabDarkTheme)
                                        holder.LikeTextView.SetTextColor(Color.White);
                                    else
                                        holder.LikeTextView.SetTextColor(Color.Black);
                                    holder.LikeTextView.Tag = "Like";
                                    break;

                            }
                        }
                    }
                    else
                    {
                        holder.LikeTextView.Text = ActivityContext.GetText(Resource.String.Btn_Like);
                        if (AppSettings.SetTabDarkTheme)
                            holder.LikeTextView.SetTextColor(Color.White);
                        else
                            holder.LikeTextView.SetTextColor(Color.Black);
                        holder.LikeTextView.Tag = "Like";
                    }
                }
                else
                {
                    if (item.IsCommentLiked)
                    {
                        holder.LikeTextView.Text = ActivityContext.GetText(Resource.String.Btn_Liked);
                        holder.LikeTextView.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                        holder.LikeTextView.Tag = "Liked";
                    }
                    else
                    {
                        holder.LikeTextView.Text = ActivityContext.GetText(Resource.String.Btn_Like);

                        if (AppSettings.SetTabDarkTheme)
                        {
                            holder.ReplyTextView.SetTextColor(Color.White);
                            holder.LikeTextView.SetTextColor(Color.White);
                        }
                        else
                        {
                            holder.ReplyTextView.SetTextColor(Color.Black);
                            holder.LikeTextView.SetTextColor(Color.Black);
                        }

                        holder.LikeTextView.Tag = "Like";
                    }
                }

                holder.TimeTextView.Tag = "true"; 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        #region Progress On Scroll

        public void SetOnLoadMoreListener(IOnLoadMoreListener onLoadMoreListener)
        {
            OnLoadMoreListener = onLoadMoreListener;
        }

        public override void OnAttachedToRecyclerView(RecyclerView recyclerView)
        {
            LastItemViewDetector(recyclerView);
            base.OnAttachedToRecyclerView(recyclerView);
        }

        private void LastItemViewDetector(RecyclerView recyclerView)
        {
            try
            {
                if (recyclerView.GetLayoutManager() is LinearLayoutManager layoutManager)
                {
                    recyclerView.AddOnScrollListener(new MyScroll(this, layoutManager));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private class MyScroll : RecyclerView.OnScrollListener
        {
            private readonly LinearLayoutManager LayoutManager;
            private readonly NativePostAdapter PostAdapter;
            public MyScroll(NativePostAdapter postAdapter, LinearLayoutManager layoutManager)
            {
                PostAdapter = postAdapter;
                LayoutManager = layoutManager;
            }
            public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
            {
                try
                {
                    base.OnScrolled(recyclerView, dx, dy);

                    if (!PostAdapter.Loading && PostAdapter.ItemCount > 10)
                    {
                        if (LayoutManager != null && LayoutManager.FindLastCompletelyVisibleItemPosition() == PostAdapter.ItemCount - 2)
                        {
                            //bottom of list!
                            int currentPage = PostAdapter.ItemCount / 5;
                            PostAdapter.OnLoadMoreListener.OnLoadMore(currentPage);
                            PostAdapter.Loading = true;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public void SetLoading()
        {
            try
            {
                if (ItemCount > 0)
                {
                    var list = ListDiffer.FirstOrDefault(anjo => anjo.TypeView == PostModelType.ViewProgress);
                    if (list == null)
                    {
                        var data = new AdapterModelsClass()
                        {
                            TypeView = PostModelType.ViewProgress,
                            Progress = true,
                        };
                        ListDiffer.Add(data);
                        NotifyItemInserted(ListDiffer.IndexOf(data));
                        Loading = true;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void SetLoaded()
        {
            try
            {
                Loading = false;
                //var list = ListDiffer.FirstOrDefault(anjo => anjo.TypeView == PostModelType.ViewProgress);
                //if (list != null)
                //{
                //    ListDiffer.Remove(list);
                //    NotifyItemRemoved(ListDiffer.IndexOf(list));
                //}
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        public override bool OnFailedToRecycleView(Object holder)
        {
            //Console.WriteLine("Russian Failed" + holder);
            return true;
        }

        public void BindAdFb()
        {
            try
            {
                if (AppSettings.ShowFbNativeAds && MAdItems?.Count == 0)
                {
                    MNativeAdsManager = new NativeAdsManager(ActivityContext, AppSettings.AdsFbNativeKey, 5);
                    MNativeAdsManager.LoadAds();
                    MNativeAdsManager.SetListener(new MyNativeAdsManagerListener(this));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void BindEmptyState(AdapterHolders.EmptyStateAdapterViewHolder holder)
        {
            try
            {
                holder.EmptyText.Text = NativePostType switch
                {
                    NativeFeedType.HashTag => ActivityContext.GetText(Resource.String.Lbl_NoPost_TitleText_hashtag),
                    NativeFeedType.Saved => ActivityContext.GetText(Resource.String.Lbl_NoPost_TitleText_saved),
                    NativeFeedType.Group => ActivityContext.GetText(Resource.String.Lbl_NoPost_TitleText_Group),
                    _ => ActivityContext.GetText(Resource.String.Lbl_NoPost_TitleText)
                };
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public AdapterModelsClass GetItem(int position)
        {
            var item = ListDiffer[position];
            return item;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override int ItemCount => ListDiffer?.Count ?? 0;

        public override int GetItemViewType(int position)
        {
            try
            {
                var item = ListDiffer[position];

                if (item == null)
                    return (int)PostModelType.NormalPost;

                return item.TypeView switch
                {
                    PostModelType.SharedHeaderPost => (int)PostModelType.SharedHeaderPost,
                    PostModelType.HeaderPost => (int)PostModelType.HeaderPost,
                    PostModelType.TextSectionPostPart => (int)PostModelType.TextSectionPostPart,
                    PostModelType.PrevBottomPostPart => (int)PostModelType.PrevBottomPostPart,
                    PostModelType.BottomPostPart => (int)PostModelType.BottomPostPart,
                    PostModelType.Divider => (int)PostModelType.Divider,
                    PostModelType.AddCommentSection => (int)PostModelType.AddCommentSection,
                    PostModelType.CommentSection => (int)PostModelType.CommentSection,
                    PostModelType.AdsPost => (int)PostModelType.AdsPost,
                    PostModelType.AlertBoxAnnouncement => (int)PostModelType.AlertBoxAnnouncement,
                    PostModelType.AlertBox => (int)PostModelType.AlertBox,
                    PostModelType.AddPostBox => (int)PostModelType.AddPostBox,
                    PostModelType.SearchForPosts => (int)PostModelType.SearchForPosts,
                    PostModelType.SocialLinks => (int)PostModelType.SocialLinks,
                    PostModelType.VideoPost => (int)PostModelType.VideoPost,
                    PostModelType.AboutBox => (int)PostModelType.AboutBox,
                    PostModelType.BlogPost => (int)PostModelType.BlogPost,
                    PostModelType.LivePost => (int)PostModelType.LivePost,
                    PostModelType.DeepSoundPost => (int)PostModelType.DeepSoundPost,
                    PostModelType.EmptyState => (int)PostModelType.EmptyState,
                    PostModelType.FilePost => (int)PostModelType.FilePost,
                    PostModelType.MapPost => (int)PostModelType.MapPost,
                    PostModelType.FollowersBox => (int)PostModelType.FollowersBox,
                    PostModelType.GroupsBox => (int)PostModelType.GroupsBox,
                    PostModelType.SuggestedGroupsBox => (int)PostModelType.SuggestedGroupsBox,
                    PostModelType.SuggestedUsersBox => (int)PostModelType.SuggestedUsersBox,
                    PostModelType.ImagePost => (int)PostModelType.ImagePost,
                    PostModelType.ImagesBox => (int)PostModelType.ImagesBox,
                    PostModelType.LinkPost => (int)PostModelType.LinkPost,
                    PostModelType.PagesBox => (int)PostModelType.PagesBox,
                    PostModelType.PlayTubePost => (int)PostModelType.PlayTubePost,
                    PostModelType.ProductPost => (int)PostModelType.ProductPost,
                    PostModelType.StickerPost => (int)PostModelType.StickerPost,
                    PostModelType.Story => (int)PostModelType.Story,
                    PostModelType.VoicePost => (int)PostModelType.VoicePost,
                    PostModelType.YoutubePost => (int)PostModelType.YoutubePost,
                    PostModelType.Section => (int)PostModelType.Section,
                    PostModelType.AlertJoinBox => (int)PostModelType.AlertJoinBox,
                    PostModelType.SharedPost => (int)PostModelType.SharedPost,
                    PostModelType.EventPost => (int)PostModelType.EventPost,
                    PostModelType.ColorPost => (int)PostModelType.ColorPost,
                    PostModelType.FacebookPost => (int)PostModelType.FacebookPost,
                    PostModelType.VimeoPost => (int)PostModelType.VimeoPost,
                    PostModelType.MultiImage2 => (int)PostModelType.MultiImage2,
                    PostModelType.MultiImage3 => (int)PostModelType.MultiImage3,
                    PostModelType.MultiImage4 => (int)PostModelType.MultiImage4,
                    PostModelType.MultiImages => (int)PostModelType.MultiImages,
                    PostModelType.JobPostSection1 => (int)PostModelType.JobPostSection1,
                    PostModelType.JobPostSection2 => (int)PostModelType.JobPostSection2,
                    PostModelType.FundingPost => (int)PostModelType.FundingPost,
                    PostModelType.PurpleFundPost => (int)PostModelType.PurpleFundPost,
                    PostModelType.PollPost => (int)PostModelType.PollPost,
                    PostModelType.AdMob1 => (int)PostModelType.AdMob1,
                    PostModelType.AdMob2 => (int)PostModelType.AdMob2,
                    PostModelType.FbAdNative => (int)PostModelType.FbAdNative,
                    PostModelType.OfferPost => (int)PostModelType.OfferPost,
                    PostModelType.ViewProgress => (int)PostModelType.ViewProgress,
                    PostModelType.PromotePost => (int)PostModelType.PromotePost,
                    PostModelType.NormalPost => (int)PostModelType.NormalPost,
                    _ => (int)PostModelType.NormalPost
                };
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return (int)PostModelType.NormalPost;
            }
        }

        public override void OnViewRecycled(Object holder)
        {
            try
            {
                if (holder != null)
                {
                    if (holder is AdapterHolders.PostImageSectionViewHolder viewHolder)
                    {
                        Glide.With(ActivityContext).Clear(viewHolder.Image);
                        viewHolder.Image.SetImageDrawable(null);
                    }

                    else if (holder is AdapterHolders.PostTopSectionViewHolder viewHolder2)
                        Glide.With(ActivityContext).Clear(viewHolder2.UserAvatar);
                    else if (holder is AdapterHolders.EventPostViewHolder viewHolder3)
                        Glide.With(ActivityContext).Clear(viewHolder3.Image);
                    else if (holder is AdapterHolders.ProductPostViewHolder viewHolder4)
                        Glide.With(ActivityContext).Clear(viewHolder4.Image);
                    else if (holder is AdapterHolders.OfferPostViewHolder viewHolder5)
                        Glide.With(ActivityContext).Clear(viewHolder5.ImageBlog);
                    else if (holder is AdapterHolders.PostBlogSectionViewHolder viewHolder6)
                        Glide.With(ActivityContext).Clear(viewHolder6.ImageBlog);
                    else if (holder is AdapterHolders.YoutubePostViewHolder viewHolder7)
                        Glide.With(ActivityContext).Clear(viewHolder7.Image);
                    else if (holder is AdapterHolders.PostVideoSectionViewHolder viewHolder8)
                        Glide.With(ActivityContext).Clear(viewHolder8.VideoImage);
                    else if (holder is AdapterHolders.FundingPostViewHolder viewHolder9)
                        Glide.With(ActivityContext).Clear(viewHolder9.Image);
                    else if (holder is AdapterHolders.LinkPostViewHolder viewHolder10)
                        Glide.With(ActivityContext).Clear(viewHolder10.Image);
                    else if (holder is AdapterHolders.PostColorBoxSectionViewHolder viewHolder11)
                        Glide.With(ActivityContext).Clear(viewHolder11.ColorBoxImage);

                    else if (holder is AdapterHolders.PostMultiImagesViewHolder viewHolder12)
                    {
                        Glide.With(ActivityContext).Clear(viewHolder12.Image);
                        Glide.With(ActivityContext).Clear(viewHolder12.Image2);
                        Glide.With(ActivityContext).Clear(viewHolder12.Image3);
                    }
                    else if (holder is AdapterHolders.Post2ImageSectionViewHolder viewHolder13)
                    {
                        Glide.With(ActivityContext).Clear(viewHolder13.Image);
                        Glide.With(ActivityContext).Clear(viewHolder13.Image2);
                    }
                    else if (holder is AdapterHolders.Post3ImageSectionViewHolder viewHolder14)
                    {
                        Glide.With(ActivityContext).Clear(viewHolder14.Image);
                        Glide.With(ActivityContext).Clear(viewHolder14.Image2);
                        Glide.With(ActivityContext).Clear(viewHolder14.Image3);
                    }
                    else if (holder is AdapterHolders.Post4ImageSectionViewHolder viewHolder15)
                    {
                        Glide.With(ActivityContext).Clear(viewHolder15.Image);
                        Glide.With(ActivityContext).Clear(viewHolder15.Image2);
                        Glide.With(ActivityContext).Clear(viewHolder15.Image3);
                        Glide.With(ActivityContext).Clear(viewHolder15.Image4);
                    }
                }
                base.OnViewRecycled(holder);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();

                var item = ListDiffer[p0];
                if (item == null)
                    return d;

                if (item.PostData.PhotoMulti?.Count > 0)
                    d.AddRange(from photo in item.PostData.PhotoMulti where !string.IsNullOrEmpty(photo.Image) select photo.Image);

                if (item.PostData.PhotoAlbum?.Count > 0)
                {
                    d.AddRange(from photo in item.PostData.PhotoAlbum where !string.IsNullOrEmpty(photo.Image) select photo.Image);
                }

                if (item.PostData.ColorId != "0")
                {
                    if (ListUtils.SettingsSiteList?.PostColors != null && ListUtils.SettingsSiteList?.PostColors.Value.PostColorsList != null)
                    {
                        var getColorObject = ListUtils.SettingsSiteList.PostColors.Value.PostColorsList.FirstOrDefault(a => a.Key == item.PostData.ColorId);
                        if (getColorObject.Value != null)
                        {
                            if (!string.IsNullOrEmpty(getColorObject.Value.Image))
                                d.Add(getColorObject.Value.Image);
                        }
                    }
                }

                if (item.PostData.PostSticker != null && !string.IsNullOrEmpty(item.PostData.PostSticker))
                    d.Add(item.PostData.PostSticker);

                if (!string.IsNullOrEmpty(item.PostData.PostLinkImage))
                    d.Add(item.PostData.PostLinkImage); //+ "===" + p0);

                if (PostFunctions.GetImagesExtensions(item.PostData.PostFileFull))
                    d.Add(item.PostData.PostFileFull);// + "===" + p0);

                if (!string.IsNullOrEmpty(item.PostData.PostFileThumb))
                    d.Add(item.PostData.PostFileThumb);
                else if (PostFunctions.GetVideosExtensions(item.PostData.PostFileFull))
                    d.Add(item.PostData.PostFileFull);

                if (!string.IsNullOrEmpty(item.PostData.Publisher.Avatar))
                    d.Add(item.PostData.Publisher.Avatar);

                if (!string.IsNullOrEmpty(item.PostData.PostYoutube))
                    d.Add("https://img.youtube.com/vi/" + item.PostData.PostYoutube + "/0.jpg");

                if (item.PostData.Product?.ProductClass?.Images != null)
                    d.AddRange(from productImage in item.PostData.Product.Value.ProductClass?.Images select productImage.Image);

                if (!string.IsNullOrEmpty(item.PostData.Blog?.Thumbnail))
                    d.Add(item.PostData.Blog?.Thumbnail);

                if (!string.IsNullOrEmpty(item.PostData.Event?.EventClass?.Cover))
                    d.Add(item.PostData.Event.Value.EventClass?.Cover);

                if (!string.IsNullOrEmpty(item.PostData?.PostMap))
                {
                    if (!item.PostData.PostMap.Contains("https://maps.googleapis.com/maps/api/staticmap?"))
                    {
                        string imageUrlMap = "https://maps.googleapis.com/maps/api/staticmap?";
                        //imageUrlMap += "center=" + item.CurrentLatitude + "," + item.CurrentLongitude;
                        imageUrlMap += "center=" + item.PostData.PostMap.Replace("/", "");
                        imageUrlMap += "&zoom=10";
                        imageUrlMap += "&scale=1";
                        imageUrlMap += "&size=300x300";
                        imageUrlMap += "&maptype=roadmap";
                        imageUrlMap += "&key=" + ActivityContext.GetText(Resource.String.google_maps_key);
                        imageUrlMap += "&format=png";
                        imageUrlMap += "&visual_refresh=true";
                        imageUrlMap += "&markers=size:small|color:0xff0000|label:1|" + item.PostData.PostMap.Replace("/", "");

                        item.PostData.ImageUrlMap = imageUrlMap;
                    }
                    else
                    {
                        item.PostData.ImageUrlMap = item.PostData.PostMap;
                    }

                    d.Add(item.PostData.ImageUrlMap);
                }

                return d;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                var d = new List<string>();
                return d;
            }
        }

        private readonly List<string> ImageCachedList = new List<string>();
        private readonly List<string> ImageCircleCachedList = new List<string>();
        //private int Count;
        public RequestBuilder GetPreloadRequestBuilder(Object p0)
        {
            try
            {
                var url = p0.ToString();
                if (url.Contains("avatar") /*&& !ImageCircleCachedList.Contains(url)*/)
                {
                    // ImageCircleCachedList.Add(url);
                    return CircleGlideRequestBuilder.Load(url);
                }
                else
                {
                    //if (!ImageCachedList.Contains(url))
                    //{
                    //    ImageCachedList.Add(url);
                    return FullGlideRequestBuilder.Load(url);
                    //}
                }

                //var f = Count++;
                //Console.WriteLine("Preloaded ++ " + f + " ++++ " + p0);
                //return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }

        }

        private TemplateView Template;
        public void BindAdMob(AdapterHolders.AdMobAdapterViewHolder holder)
        {
            try
            {
                Template = holder.MianAlert;
               
                AdLoader.Builder builder = new AdLoader.Builder(holder.MainView.Context, AppSettings.AdAdMobNativeKey);
                builder.ForUnifiedNativeAd(this);

                VideoOptions videoOptions = new VideoOptions.Builder()
                    .SetStartMuted(true)
                    .Build();

                NativeAdOptions adOptions = new NativeAdOptions.Builder()
                    .SetVideoOptions(videoOptions)
                    .Build();

                builder.WithNativeAdOptions(adOptions);

                AdLoader adLoader = builder.WithAdListener(new AdListener()).Build();
                adLoader.LoadAd(new AdRequest.Builder().Build());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OnUnifiedNativeAdLoaded(UnifiedNativeAd ad)
        {
            try
            {
                if (Template.GetTemplateTypeName() == TemplateView.BigTemplate)
                {
                    Template.PopulateUnifiedNativeAdView(ad);
                }
                else if (Template.GetTemplateTypeName() == TemplateView.NativeContentAd)
                {
                    Template.NativeContentAdView(ad);
                }
                else if (Template.GetTemplateTypeName() == TemplateView.NativeAppInstallAd)
                {
                    Template.NativeAppInstallAdView(ad);
                }
                else
                {
                    NativeTemplateStyle styles = new NativeTemplateStyle.Builder().Build();

                    Template.SetStyles(styles);
                    Template.SetNativeAd(ad);
                }
                 
                ActivityContext.RunOnUiThread(() =>
                {
                    Template.Visibility = ViewStates.Visible;
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void AutoLinkTextClick(StTools.XAutoLinkMode p0, string p1, Dictionary<string, string> userData)
        {
            try
            {
                var typeText = Methods.FunString.Check_Regex(p1.Replace(" ", ""));
                if (typeText == "Email")
                {
                    Methods.App.SendEmail(ActivityContext, p1.Replace(" ", ""));
                }
                else if (typeText == "Website")
                {
                    string url = p1.Replace(" ", "");
                    if (!p1.Contains("http"))
                    {
                        url = "http://" + p1.Replace(" ", "");
                    }

                    //var intent = new Intent(MainContext, typeof(LocalWebViewActivity));
                    //intent.PutExtra("URL", url.Replace(" ", ""));
                    //intent.PutExtra("Type", url.Replace(" ", ""));
                    //MainContext.StartActivity(intent);
                    new IntentController(ActivityContext).OpenBrowserFromApp(url);
                }
                else if (typeText == "Hashtag")
                {
                    var intent = new Intent(ActivityContext, typeof(HashTagPostsActivity));
                    intent.PutExtra("Id", p1.Replace(" ", ""));
                    intent.PutExtra("Tag", p1.Replace(" ", ""));
                    ActivityContext.StartActivity(intent);
                }
                else if (typeText == "Mention")
                {
                    var dataUSer = ListUtils.MyProfileList.FirstOrDefault();
                    string name = p1.Replace("@", "").Replace(" ", "");

                    var sqlEntity = new SqLiteDatabase();
                    var user = sqlEntity.Get_DataOneUser(name);
                    sqlEntity.Dispose();

                    if (user != null)
                    {
                        WoWonderTools.OpenProfile(ActivityContext, user.UserId, user);
                    }
                    else if (userData?.Count > 0)
                    {
                        var data = userData.FirstOrDefault(a => a.Value == name);
                        if (data.Key != null && data.Key == UserDetails.UserId)
                        {
                            if (PostClickListener.OpenMyProfile) return;
                            var intent = new Intent(ActivityContext, typeof(MyProfileActivity));
                            ActivityContext.StartActivity(intent);
                        }
                        else if (data.Key != null)
                        {
                            var intent = new Intent(ActivityContext, typeof(UserProfileActivity));
                            //intent.PutExtra("UserObject", JsonConvert.SerializeObject(item));
                            intent.PutExtra("UserId", data.Key);
                            ActivityContext.StartActivity(intent);
                        }
                    }
                    else
                    {
                        if (name == dataUSer?.Name || name == dataUSer?.Username)
                        {
                            if (PostClickListener.OpenMyProfile) return;
                            var intent = new Intent(ActivityContext, typeof(MyProfileActivity));
                            ActivityContext.StartActivity(intent);
                        }
                        else
                        {
                            var intent = new Intent(ActivityContext, typeof(UserProfileActivity));
                            //intent.PutExtra("UserObject", JsonConvert.SerializeObject(item));
                            intent.PutExtra("name", name);
                            ActivityContext.StartActivity(intent);
                        }
                    }
                }
                else if (typeText == "Number")
                {
                    Methods.App.SaveContacts(ActivityContext, p1.Replace(" ", ""), "", "2");
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
    }

    public sealed class PreCachingLayoutManager : LinearLayoutManager
    {
        public class SmoothScrolledControl : LinearSmoothScroller
        {
            public SmoothScrolledControl(Context context) : base(context)
            {

            }

            public void SetSmoothScrolledControl(PreCachingLayoutManager preCachingLayoutManager)
            {
                PreCachingLayoutManager = preCachingLayoutManager;
            }

            private PreCachingLayoutManager PreCachingLayoutManager;

            public override PointF ComputeScrollVectorForPosition(int targetPosition)
            {
                return PreCachingLayoutManager.ComputeScrollVectorForPosition(targetPosition);
            }
        }

        private readonly Context Context;
        private int ExtraLayoutSpace = -1;
        private int DefaultExtraLayoutSpace = 600;
        private OrientationHelper MOrientationHelper;
        private int MAdditionalAdjacentPrefetchItemCount;

        public PreCachingLayoutManager(Activity context) : base(context)
        {
            Context = context;
            Init();
        }

        public void Init()
        {
            MOrientationHelper = OrientationHelper.CreateOrientationHelper(this, Orientation);
            ItemPrefetchEnabled = true;
            InitialPrefetchItemCount = 20;

        }

        public void SetExtraLayoutSpace(int space)
        {
            ExtraLayoutSpace = space;
        }

        protected override int GetExtraLayoutSpace(RecyclerView.State state)
        {
            if (ExtraLayoutSpace > 0)
            {
                return ExtraLayoutSpace;
            }
            else
            {
                return DefaultExtraLayoutSpace;
            }
        }

        public void SetPreloadItemCount(int preloadItemCount)
        {
            if (preloadItemCount < 1)
            {
                throw new IllegalArgumentException("adjacentPrefetchItemCount must not smaller than 1!");
            }
            MAdditionalAdjacentPrefetchItemCount = preloadItemCount - 1;
        }

        public override void CollectAdjacentPrefetchPositions(int dx, int dy, RecyclerView.State state, ILayoutPrefetchRegistry layoutPrefetchRegistry)
        {
            try
            {
                base.CollectAdjacentPrefetchPositions(dx, dy, state, layoutPrefetchRegistry);

                var delta = Orientation == Horizontal ? dx : dy;
                if (ChildCount == 0 || delta == 0)
                    return;

                var layoutDirection = delta > 0 ? 1 : -1;
                var child = GetChildClosest(layoutDirection);
                var currentPosition = GetPosition(child) + layoutDirection;

                if (layoutDirection != 1)
                    return;

                var scrollingOffset = MOrientationHelper.GetDecoratedEnd(child) - MOrientationHelper.EndAfterPadding;
                for (var i = currentPosition + 1; i < currentPosition + MAdditionalAdjacentPrefetchItemCount + 1; i++)
                {
                    if (i >= 0 && i < state.ItemCount)
                    {
                        layoutPrefetchRegistry.AddPosition(i, Java.Lang.Math.Max(0, scrollingOffset));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private View GetChildClosest(int layoutDirection)
        {
            return GetChildAt(layoutDirection == -1 ? 0 : ChildCount - 1);
        }
    }
}