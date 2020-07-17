using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Views;
using Android.Widget;
using Bumptech.Glide;
using Java.Lang;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.Support.Design.Widget;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Request;
using WoWonder.Activities.NativePost.Extra;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Classes.Product;
using WoWonderClient.Classes.Story;
using WoWonderClient.Requests;
using Exception = System.Exception;

namespace WoWonder.Activities.Tabbes.Fragment
{
    public class NewsFeedNative : Android.Support.V4.App.Fragment
    {
        #region Variables Basic

        private FloatingActionButton PopupBubbleView;
        public WRecyclerView MainRecyclerView;
        public NativePostAdapter PostFeedAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private readonly Handler MainHandler = new Handler(); 
        private TabbedMainActivity GlobalContext;

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
            GlobalContext = (TabbedMainActivity)Activity;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                View view = inflater.Inflate(Resource.Layout.TestNewsFeed, container, false);
                return view;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return null;
            }
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            try
            {
                base.OnViewCreated(view, savedInstanceState);

                InitComponent(view);
                SetRecyclerViewAdapters(view);

                LoadPost();
                 
                GlobalContext.GetOneSignalNotification();
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

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                PopupBubbleView = (FloatingActionButton)view.FindViewById(Resource.Id.popup_bubble);

                SwipeRefreshLayout = view.FindViewById<SwipeRefreshLayout>(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
                SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void SetRecyclerViewAdapters(View view)
        {
            try
            {
                MainRecyclerView = (WRecyclerView)view.FindViewById(Resource.Id.newsfeedRecyler);
                PostFeedAdapter = new NativePostAdapter(Activity, UserDetails.UserId, MainRecyclerView, NativeFeedType.Global, Activity.SupportFragmentManager);
                MainRecyclerView.SetXAdapter(PostFeedAdapter, SwipeRefreshLayout);
                if (AppSettings.ShowNewPostOnNewsFeed)
                    MainRecyclerView.SetXPopupBubble(PopupBubbleView);
                else
                    PopupBubbleView.Visibility = ViewStates.Gone;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Refresh

        //Refresh 
        private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                PostFeedAdapter.ListDiffer.Clear();

                MainRecyclerView?.StopVideo();

                var combiner = new FeedCombiner(null, PostFeedAdapter.ListDiffer, Activity);
                 
                if (AppSettings.ShowStory)
                {
                    combiner.AddStoryPostView();
                }

                combiner.AddPostBoxPostView("feed", -1);

                var checkSectionAlertBox = PostFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.AlertBox);
                {
                    PostFeedAdapter.ListDiffer.Remove(checkSectionAlertBox);
                }

                var checkSectionAlertJoinBox = PostFeedAdapter.ListDiffer.Where(a => a.TypeView == PostModelType.AlertJoinBox).ToList();
                {
                    foreach (var adapterModelsClass in checkSectionAlertJoinBox)
                    {
                        PostFeedAdapter.ListDiffer.Remove(adapterModelsClass);
                    }
                }
                 
                PostFeedAdapter.NotifyDataSetChanged();

                StartApiService();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        #region Get Post Feed

        private void LoadPost()
        {
            try
            {

                var combiner = new FeedCombiner(null, PostFeedAdapter.ListDiffer, Activity);

                combiner.AddStoryPostView();
                combiner.AddPostBoxPostView("feed", -1);
                combiner.AddGreetingAlertPostView();
               
                if (PostFeedAdapter.ListDiffer.Count <= 5)
                {
                    StartApiService();
                }
                else
                {
                    var item = PostFeedAdapter.ListDiffer.LastOrDefault();

                    var lastItem = PostFeedAdapter.ListDiffer.IndexOf(item);

                    item = PostFeedAdapter.ListDiffer[lastItem];

                    string offset;
                    if (item.TypeView == PostModelType.Divider || item.TypeView == PostModelType.ViewProgress || item.TypeView == PostModelType.AdMob1 || item.TypeView == PostModelType.AdMob2 || item.TypeView == PostModelType.FbAdNative || item.TypeView == PostModelType.AdsPost || item.TypeView == PostModelType.SuggestedGroupsBox || item.TypeView == PostModelType.SuggestedUsersBox || item.TypeView == PostModelType.CommentSection || item.TypeView == PostModelType.AddCommentSection)
                    {
                        item = PostFeedAdapter.ListDiffer.LastOrDefault(a => a.TypeView != PostModelType.Divider && a.TypeView != PostModelType.ViewProgress && a.TypeView != PostModelType.AdMob1 && a.TypeView != PostModelType.AdMob2 && a.TypeView != PostModelType.FbAdNative && a.TypeView != PostModelType.AdsPost && a.TypeView != PostModelType.SuggestedGroupsBox && a.TypeView != PostModelType.SuggestedUsersBox && a.TypeView != PostModelType.CommentSection && a.TypeView != PostModelType.AddCommentSection);
                        offset = item?.PostData?.Id ?? "0";
                        Console.WriteLine(offset);
                    }
                    else
                    {
                        offset = item.PostData?.Id ?? "0";
                    }

                    StartApiService(offset);
                }
                 
                //Start Updating the news feed every few minus 
                MainHandler.PostDelayed(new ApiPostUpdaterHelper(Activity, MainRecyclerView, MainHandler), 30000);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void StartApiService(string offset = "0")
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(Activity, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadStory, () => MainRecyclerView.FetchNewsFeedApiPosts(offset) }); 
        }

        public override void OnDestroy()
        {
            try
            {
                MainRecyclerView = null;
                PostFeedAdapter = null;
                base.OnDestroy();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
         
        private class ApiPostUpdaterHelper : Java.Lang.Object, IRunnable
        {
            private readonly WRecyclerView MainRecyclerView;
            private readonly Handler MainHandler;
            private readonly Activity Activity;

            public ApiPostUpdaterHelper(Activity activity, WRecyclerView mainRecyclerView, Handler mainHandler)
            {
                MainRecyclerView = mainRecyclerView;
                MainHandler = mainHandler;
                Activity = activity;
            }

            public async void Run()
            {
                try
                {
                    await MainRecyclerView.FetchNewsFeedApiPosts("0", "Insert");
                    TabbedMainActivity.GetInstance()?.Get_Notifications();
                    await ApiRequest.Get_MyProfileData_Api(Activity).ConfigureAwait(false);
                    MainHandler.PostDelayed(new ApiPostUpdaterHelper(Activity, MainRecyclerView, MainHandler), 30000);
                    Console.WriteLine("Allen Post + started" + DateTime.Now);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Console.WriteLine("Allen Post + failed");
                }
            }
        }

        #endregion

        #region Get Story

        private async Task LoadStory()
        {
            if (Methods.CheckConnectivity())
            {
                var checkSection = PostFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.Story);
                if (checkSection != null)
                {
                    if (checkSection.StoryList == null)
                        checkSection.StoryList = new ObservableCollection<GetUserStoriesObject.StoryObject>();

                    (int apiStatus, var respond) = await RequestsAsync.Story.Get_UserStories();
                    if (apiStatus == 200)
                    {
                        if (respond is GetUserStoriesObject result)
                        {
                            foreach (var item in result.Stories)
                            {
                                var check = checkSection.StoryList.FirstOrDefault(a => a.UserId == item.UserId);
                                if (check != null)
                                {
                                    foreach (var item2 in item.Stories)
                                    {
                                        item.DurationsList ??= new List<long>();

                                        //image and video
                                        var mediaFile = !item2.Thumbnail.Contains("avatar") && item2.Videos.Count == 0 ? item2.Thumbnail : item2.Videos[0].Filename;

                                        var type = Methods.AttachmentFiles.Check_FileExtension(mediaFile);
                                        if (type != "Video")
                                        {
                                            Glide.With(Context).Load(mediaFile).Apply(new RequestOptions().SetDiskCacheStrategy(DiskCacheStrategy.All).CenterCrop()).Preload();
                                            item.DurationsList.Add(10000L);
                                        }
                                        else
                                        {
                                            var fileName = mediaFile.Split('/').Last();
                                            mediaFile = WoWonderTools.GetFile(DateTime.Now.Day.ToString(), Methods.Path.FolderDiskStory, fileName, mediaFile);

                                            var duration = WoWonderTools.GetDuration(mediaFile);
                                            item.DurationsList.Add(Long.ParseLong(duration));
                                        }
                                    }

                                    check.Stories = item.Stories;
                                }
                                else
                                {
                                    foreach (var item1 in item.Stories)
                                    {
                                        item.DurationsList ??= new List<long>();

                                        //image and video
                                        var mediaFile = !item1.Thumbnail.Contains("avatar") && item1.Videos.Count == 0 ? item1.Thumbnail : item1.Videos[0].Filename;

                                        var type1 = Methods.AttachmentFiles.Check_FileExtension(mediaFile);
                                        if (type1 != "Video")
                                        {
                                            Glide.With(Context).Load(mediaFile).Apply(new RequestOptions().SetDiskCacheStrategy(DiskCacheStrategy.All).CenterCrop()).Preload();
                                            item.DurationsList.Add(10000L);
                                        }
                                        else
                                        {
                                            var fileName = mediaFile.Split('/').Last();
                                            WoWonderTools.GetFile(DateTime.Now.Day.ToString(), Methods.Path.FolderDiskStory, fileName, mediaFile);

                                            var duration = WoWonderTools.GetDuration(mediaFile);
                                            item.DurationsList.Add(Long.ParseLong(duration));
                                        }
                                    }

                                    checkSection.StoryList.Add(item);
                                }
                            }
                        }
                    }
                    else Methods.DisplayReportResult(Activity, respond);

                    if (checkSection.StoryList.Count > 4)
                    {
                        PostFeedAdapter.HolderStory.AboutMore.Visibility = ViewStates.Visible;
                        PostFeedAdapter.HolderStory.AboutMoreIcon.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        PostFeedAdapter.HolderStory.AboutMore.Visibility = ViewStates.Invisible;
                        PostFeedAdapter.HolderStory.AboutMoreIcon.Visibility = ViewStates.Invisible;
                    }

                    var d = new Runnable(() => { PostFeedAdapter.NotifyItemChanged(PostFeedAdapter.ListDiffer.IndexOf(checkSection)); }); d.Run();
                }
            }
            else
            {
                Toast.MakeText(Context, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
            }
        }

        #endregion

        #region Permissions && Result

        //Result

        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);

                if (requestCode == 2500 && resultCode == (int)Result.Ok) //add post
                {
                    if (!string.IsNullOrEmpty(data.GetStringExtra("itemObject")))
                    {
                        var postData = JsonConvert.DeserializeObject<PostDataObject>(data.GetStringExtra("itemObject"));
                        if (postData != null)
                        {
                            var countList = PostFeedAdapter.ItemCount;

                            var combine = new FeedCombiner(postData, PostFeedAdapter.ListDiffer, Context);
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
                else if (requestCode == 3950 && resultCode == (int)Result.Ok) //Edit post
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
                else if (requestCode == 3500 && resultCode == (int)Result.Ok) //Edit post product 
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

    }
}