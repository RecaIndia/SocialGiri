using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Android.Content;
using Android.Util;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Fonts;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Classes.Story;
using Uri = Android.Net.Uri;

namespace WoWonder.Activities.NativePost.Extra
{
    public class FeedCombiner
    {
        private readonly Context MainContext;
        private readonly PostDataObject PostCollection;
        private readonly List<AdapterModelsClass> PostList;
        private readonly PostModelType PostFeedType;
        private readonly PostModelResolver PostModelResolver;
        private readonly WoTextDecorator TextDecorator;

        public FeedCombiner(PostDataObject post, List<AdapterModelsClass> diffList, Context context)
        {
            try
            {
                MainContext = context;
                PostFeedType = PostFunctions.GetAdapterType(post);
                PostModelResolver = new PostModelResolver(context, PostFeedType);
                PostCollection = post;
                PostList = diffList;
                TextDecorator = new WoTextDecorator();
            }
            catch (Exception e)
            {
                Console.WriteLine(e); Log.Debug("wael >> FeedCombiner", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
            }
        }

        // Add Post
        //=====================================

        public void AddAutoSection(bool isSharing = false)
        {
            try
            {
                if (isSharing)
                {
                    var getSharedPostType = PostFunctions.GetAdapterType(PostCollection.SharedInfo.SharedInfoClass);
                    var collection = PostCollection.SharedInfo.SharedInfoClass;

                    switch (getSharedPostType)
                    {
                        case PostModelType.BlogPost:
                            PostModelResolver.PrepareBlog(collection);
                            break;
                        case PostModelType.EventPost:
                            PostModelResolver.PrepareEvent(collection);
                            if (Convert.ToInt32(collection.EventId) > 0)
                                return;
                            break;
                        case PostModelType.ColorPost:
                            PostModelResolver.PrepareColorBox(collection);
                            break;
                        case PostModelType.LinkPost:
                            PostModelResolver.PrepareLink(collection);
                            break;
                        case PostModelType.ProductPost:
                            PostModelResolver.PrepareProduct(collection);
                            break;
                        case PostModelType.FundingPost:
                            PostModelResolver.PrepareFunding(collection);
                            break;
                        case PostModelType.OfferPost:
                            PostModelResolver.PrepareOffer(collection);
                            break;
                        case PostModelType.MapPost:
                            PostModelResolver.PrepareMapPost(collection);
                            break;
                        case PostModelType.PollPost:
                        case PostModelType.AdsPost:
                        case PostModelType.AdMob1:
                        case PostModelType.AdMob2:
                        case PostModelType.FbAdNative:
                            return;
                        case PostModelType.VideoPost:
                            WRecyclerView.GetInstance()?.CacheVideosFiles(Uri.Parse(collection.PostFileFull));
                            break;
                        case PostModelType.JobPost:
                            AddJobPost();
                            return;
                    }

                    var item = new AdapterModelsClass
                    {
                        TypeView = getSharedPostType,
                        Id = int.Parse((int)PostFeedType + collection.Id),
                        PostData = collection,
                        IsDefaultFeedPost = true,
                    };

                    PostList.Add(item);
                }
                else
                {
                    switch (PostFeedType)
                    {
                        case PostModelType.BlogPost:
                            PostModelResolver.PrepareBlog(PostCollection);
                            break;
                        case PostModelType.EventPost:
                            PostModelResolver.PrepareEvent(PostCollection);
                            if (Convert.ToInt32(PostCollection.EventId) > 0)
                                return;
                            break;
                        case PostModelType.ColorPost:
                            PostModelResolver.PrepareColorBox(PostCollection);
                            break;
                        case PostModelType.LinkPost:
                            PostModelResolver.PrepareLink(PostCollection);
                            break;
                        case PostModelType.ProductPost:
                            PostModelResolver.PrepareProduct(PostCollection);
                            break;
                        case PostModelType.FundingPost:
                            PostModelResolver.PrepareFunding(PostCollection);
                            break;
                        case PostModelType.OfferPost:
                            PostModelResolver.PrepareOffer(PostCollection);
                            break;
                        case PostModelType.MapPost:
                            PostModelResolver.PrepareMapPost(PostCollection);
                            break;
                        case PostModelType.PollPost:
                        case PostModelType.AdsPost:
                        case PostModelType.AdMob1:
                        case PostModelType.AdMob2:
                        case PostModelType.FbAdNative:
                            return;
                        case PostModelType.VideoPost:
                            WRecyclerView.GetInstance()?.CacheVideosFiles(Uri.Parse(PostCollection.PostFileFull));
                            break;
                        case PostModelType.JobPost:
                            AddJobPost();
                            return;
                        case PostModelType.SharedPost:
                            AddSharedPost();
                            return;
                    }

                    var item = new AdapterModelsClass
                    {
                        TypeView = PostFeedType,
                        Id = int.Parse((int)PostFeedType + PostCollection.Id),
                        PostData = PostCollection,
                        IsDefaultFeedPost = true,
                        PostDataDecoratedContent = TextDecorator.SetupStrings(PostCollection, MainContext),
                    };

                    PostList.Add(item);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e); Log.Debug("wael >> FeedCombiner", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
            }
        }

        #region Add Post

        public void AddPostPromote(bool isSharing = false)
        {
            try
            {
                bool isPromoted = false;
                if (isSharing)
                {
                    var collection = PostCollection.SharedInfo.SharedInfoClass;

                    if (collection.IsPostBoosted == "1" || collection.SharedInfo.SharedInfoClass != null && collection.SharedInfo.SharedInfoClass?.IsPostBoosted == "1")
                        isPromoted = true;

                    if (isPromoted)
                    {
                        var item = new AdapterModelsClass
                        {
                            TypeView = PostModelType.PromotePost,
                            Id = int.Parse((int)PostModelType.PromotePost + collection.Id),
                            PostData = collection,
                            IsDefaultFeedPost = true,
                        };
                        PostList.Add(item);
                    }
                }
                else
                {
                    if (PostCollection.IsPostBoosted == "1" || PostCollection.SharedInfo.SharedInfoClass != null && PostCollection.SharedInfo.SharedInfoClass?.IsPostBoosted == "1")
                        isPromoted = true;

                    if (isPromoted)
                    {
                        var item = new AdapterModelsClass
                        {
                            TypeView = PostModelType.PromotePost,

                            Id = int.Parse((int)PostModelType.PromotePost + PostCollection.Id),
                            PostData = PostCollection,
                            IsDefaultFeedPost = true,
                        };
                        PostList.Add(item);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e); Log.Debug("wael >> FeedCombiner", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
            }
        }

        public void AddPostHeader(bool isSharing = false)
        {
            try
            {
                if (isSharing)
                {
                    var collection = PostCollection.SharedInfo.SharedInfoClass;
                    PostModelResolver.PrepareHeader(collection);
                    var item = new AdapterModelsClass
                    {
                        TypeView = PostModelType.SharedHeaderPost,
                        Id = int.Parse((int)PostModelType.SharedHeaderPost + collection.Id),
                        PostData = collection,
                        IsDefaultFeedPost = true,
                    };
                    PostList.Add(item);
                }
                else
                {
                    PostModelResolver.PrepareHeader(PostCollection);
                    var item = new AdapterModelsClass
                    {
                        TypeView = PostModelType.HeaderPost,

                        Id = int.Parse((int)PostModelType.HeaderPost + PostCollection.Id),
                        PostData = PostCollection,
                        IsDefaultFeedPost = true,
                        PostDataDecoratedContent = TextDecorator.SetupStrings(PostCollection, MainContext),
                    };
                    PostList.Add(item);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e); Log.Debug("wael >> FeedCombiner", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
            }
        }

        public void AddPostTextSection(bool isSharing = false)
        {
            try
            {
                if (isSharing)
                {
                    var collection = PostCollection.SharedInfo.SharedInfoClass;

                    var getSharedPostType = PostFunctions.GetAdapterType(collection);
                    if (getSharedPostType == PostModelType.ColorPost)
                        return;

                    if (string.IsNullOrEmpty(collection.Orginaltext))
                        return;

                    PostModelResolver.PrepareTextSection(collection);

                    var item = new AdapterModelsClass
                    {
                        TypeView = PostModelType.TextSectionPostPart,
                        Id = int.Parse((int)PostModelType.TextSectionPostPart + collection.Id),
                        PostData = collection,
                        IsDefaultFeedPost = true
                    };

                    PostList.Add(item);
                }
                else
                {
                    if (PostFeedType == PostModelType.ColorPost)
                        return;

                    if (string.IsNullOrEmpty(PostCollection.Orginaltext))
                        return;

                    PostModelResolver.PrepareTextSection(PostCollection);

                    var item = new AdapterModelsClass
                    {
                        TypeView = PostModelType.TextSectionPostPart,
                        Id = int.Parse((int)PostModelType.TextSectionPostPart + PostCollection.Id),
                        PostData = PostCollection,
                        IsDefaultFeedPost = true
                    };

                    PostList.Add(item);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e); Log.Debug("wael >> FeedCombiner", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
            }
        }

        public void AddPostPrevBottom()
        {
            try
            {
                PostModelResolver.PreparePostPrevBottom(PostCollection);

                var item = new AdapterModelsClass
                {
                    TypeView = PostModelType.PrevBottomPostPart,
                    Id = int.Parse((int)PostModelType.PrevBottomPostPart + PostCollection.Id),

                    PostData = PostCollection,
                    IsDefaultFeedPost = true
                };

                PostList.Add(item);
            }
            catch (Exception e)
            {
                Console.WriteLine(e); Log.Debug("wael >> FeedCombiner", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
            }
        }

        public void AddPostFooter()
        {
            try
            {
                bool isSharing = false;
                var collection = PostCollection.SharedInfo.SharedInfoClass;
                if (collection != null)
                {
                    isSharing = true;
                }

                var item = new AdapterModelsClass
                {
                    TypeView = PostModelType.BottomPostPart,
                    Id = int.Parse((int)PostModelType.BottomPostPart + PostCollection.Id),
                    IsSharingPost = isSharing,
                    PostData = PostCollection,
                    IsDefaultFeedPost = true
                };

                PostList.Add(item);
            }
            catch (Exception e)
            {
                Console.WriteLine(e); Log.Debug("wael >> FeedCombiner", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
            }
        }

        public void AddPostDivider(int index = -1)
        {

            try
            {
                var item = new AdapterModelsClass
                {
                    TypeView = PostModelType.Divider,
                    Id = int.Parse((int)PostModelType.Divider + PostCollection?.Id),

                    PostData = PostCollection,
                    IsDefaultFeedPost = true
                };

                if (index == -1)
                {
                    PostList.Add(item);
                }
                else
                {
                    PostList.Insert(index, item);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e); Log.Debug("wael >> FeedCombiner", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
            }
        }

        public void AddPostCommentAbility()
        {
            try
            {
                if (PostCollection == null || PostCollection.PostComments == "0" || PostCollection.GetPostComments?.Count == 0)
                    return;

                var check = PostCollection.GetPostComments.FirstOrDefault(banjo => string.IsNullOrEmpty(banjo.CFile) && string.IsNullOrEmpty(banjo.Record));
                if (check == null)
                    return;

                var item1 = new AdapterModelsClass
                {
                    TypeView = PostModelType.CommentSection,
                    Id = int.Parse((int)PostModelType.CommentSection + PostCollection?.Id),
                    PostData = PostCollection,
                    IsDefaultFeedPost = true
                };
                PostList.Add(item1);

                var item = new AdapterModelsClass
                {
                    TypeView = PostModelType.AddCommentSection,
                    Id = int.Parse((int)PostModelType.AddCommentSection + PostCollection?.Id),
                    PostData = PostCollection,
                    IsDefaultFeedPost = true
                };
                PostList.Add(item);
            }
            catch (Exception e)
            {
                Console.WriteLine(e); Log.Debug("wael >> FeedCombiner", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
            }
        }
        public void AddSharedPost()
        {
            try
            {
                if (PostCollection.SharedInfo.SharedInfoClass != null)
                {
                    AddPostPromote(true);
                    AddPostHeader(true);
                    AddPostTextSection(true);
                    AddAutoSection(true);
                    AddPollsPostView();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e); Log.Debug("wael >> FeedCombiner", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
            }
        }

        public void AddPollsPostView()
        {
            try
            {
                if (PostCollection.Options != null)
                {
                    var count = PostCollection.Options.Count;
                    if (count > 0)
                    {
                        foreach (var poll in PostCollection.Options)
                        {
                            PostModelResolver.PreparePoll(poll);

                            poll.PostId = PostCollection.Id;
                            poll.RelatedToPollsCount = count;

                            var i = new AdapterModelsClass
                            {
                                TypeView = PostModelType.PollPost,
                                Id = int.Parse((int)PostModelType.PollPost + PostCollection.Id),

                                PostData = PostCollection,
                                IsDefaultFeedPost = true,
                                PollId = poll.Id,
                                PollsOption = poll,
                                PollOwnerUserId = PostCollection.Publisher?.UserId
                            };
                            PostList.Add(i);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e); Log.Debug("wael >> FeedCombiner", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
            }
        }

        #endregion

        #region Extra Post

        public void AddAdsPost(string type = "Add")
        {
            try
            {
                if (PostCollection.UserData != null)
                {
                    PostModelResolver.PrepareAds(PostCollection);

                    var item = new AdapterModelsClass
                    {
                        TypeView = PostModelType.AdsPost,
                        Id = int.Parse((int)PostModelType.AdsPost + PostCollection.Id),
                        PostData = PostCollection,
                        PostDataDecoratedContent = TextDecorator.SetupStrings(PostCollection, MainContext),
                        IsDefaultFeedPost = true
                    };

                    if (type == "Add")
                    {
                        PostList.Add(item);
                        AddPostDivider();
                    }
                    else
                    {
                        CountIndex = 0;
                        var model1 = PostList.FirstOrDefault(a => a.TypeView == PostModelType.Story);
                        var model2 = PostList.FirstOrDefault(a => a.TypeView == PostModelType.AddPostBox);
                        var model3 = PostList.FirstOrDefault(a => a.TypeView == PostModelType.AlertBox);
                        var model4 = PostList.FirstOrDefault(a => a.TypeView == PostModelType.SearchForPosts);

                        if (model4 != null)
                            CountIndex += PostList.IndexOf(model4) + 1;
                        else if (model3 != null)
                            CountIndex += PostList.IndexOf(model3) + 1;
                        else if (model2 != null)
                            CountIndex += PostList.IndexOf(model2) + 1;
                        else if (model1 != null)
                            CountIndex += PostList.IndexOf(model1) + 1;
                        else
                            CountIndex = 0;

                        CountIndex++;
                        PostList.Insert(CountIndex, item);

                        InsertOnTopPostDivider();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e); Log.Debug("wael >> FeedCombiner", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
            }
        }

        public void AddJobPost()
        {
            try
            {
                if (PostCollection.Job != null)
                {
                    PostModelResolver.PrepareJob(PostCollection);

                    var i = new AdapterModelsClass
                    {
                        TypeView = PostModelType.JobPostSection1,
                        Id = int.Parse((int)PostModelType.JobPostSection1 + PostCollection.Id),

                        PostData = PostCollection,
                        IsDefaultFeedPost = true
                    };
                    var i2 = new AdapterModelsClass
                    {
                        TypeView = PostModelType.JobPostSection2,
                        Id = int.Parse((int)PostModelType.JobPostSection2 + PostCollection.Id),

                        PostData = PostCollection,
                        IsDefaultFeedPost = true
                    };

                    PostList.Add(i);
                    PostList.Add(i2);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e); Log.Debug("wael >> FeedCombiner", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
            }
        }

        public void AddStoryPostView()
        {
            try
            {
                if (AppSettings.ShowStory)
                {
                    var story = new AdapterModelsClass
                    {
                        TypeView = PostModelType.Story,
                        StoryList = new ObservableCollection<GetUserStoriesObject.StoryObject>(),
                        Id = 545454545
                    };

                    PostList.Add(story);
                    AddPostDivider();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e); Log.Debug("wael >> FeedCombiner", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
            }
        }

        public void GroupsBoxPostView(GroupsModelClass modelClass, int index)
        {
            try
            {
                var item = new AdapterModelsClass
                {
                    TypeView = PostModelType.GroupsBox,
                    Id = 222222222,
                    GroupsModel = modelClass
                };

                if (index == -1)
                {
                    PostList.Add(item);
                    AddPostDivider();
                }
                else
                {
                    PostList.Insert(index, item);
                    AddPostDivider(PostList.IndexOf(item) + 1);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e); Log.Debug("wael >> FeedCombiner", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
            }
        }

        public void PagesBoxPostView(PagesModelClass modelClass, int index)
        {
            try
            {
                var item = new AdapterModelsClass
                {
                    TypeView = PostModelType.PagesBox,
                    Id = 333333333,
                    PagesModel = modelClass
                };

                if (index == -1)
                {
                    PostList.Add(item);
                    AddPostDivider();
                }
                else
                {
                    PostList.Insert(index, item);
                    AddPostDivider(PostList.IndexOf(item) + 1);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e); Log.Debug("wael >> FeedCombiner", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
            }
        }

        public void ImagesBoxPostView(ImagesModelClass modelClass, int index)
        {
            try
            {
                var item = new AdapterModelsClass
                {
                    TypeView = PostModelType.ImagesBox,
                    Id = 444444444,
                    ImagesModel = modelClass
                };

                if (index == -1)
                {
                    PostList.Add(item);
                    AddPostDivider();
                }
                else
                {
                    PostList.Insert(index, item);
                    AddPostDivider(PostList.IndexOf(item) + 1);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e); Log.Debug("wael >> FeedCombiner", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
            }
        }

        public void FollowersBoxPostView(FollowersModelClass modelClass, int index)
        {
            try
            {
                var item = new AdapterModelsClass
                {
                    TypeView = PostModelType.FollowersBox,
                    Id = 1111111111,
                    FollowersModel = modelClass
                };

                if (index == -1)
                {
                    PostList.Add(item);
                    AddPostDivider();
                }
                else
                {
                    PostList.Insert(index, item);
                    AddPostDivider(PostList.IndexOf(item) + 1);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e); Log.Debug("wael >> FeedCombiner", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
            }
        }

        public void SocialLinksBoxPostView(SocialLinksModelClass modelClass, int index)
        {
            try
            {
                var item = new AdapterModelsClass
                {
                    TypeView = PostModelType.SocialLinks,
                    Id = 0023,
                    SocialLinksModel = modelClass,
                };

                if (index == -1)
                {
                    PostList.Add(item);
                    AddPostDivider();
                }
                else
                {
                    PostList.Insert(index, item);
                    AddPostDivider(PostList.IndexOf(item) + 1);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e); Log.Debug("wael >> FeedCombiner", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
            }
        }

        public void AboutBoxPostView(string description, int index)
        {
            try
            {
                var item = new AdapterModelsClass
                {
                    TypeView = PostModelType.AboutBox,
                    Id = 0000,
                    AboutModel = new AboutModelClass { Description = description },
                };

                if (index == -1)
                {
                    PostList.Add(item);
                    AddPostDivider();
                }
                else
                {
                    PostList.Insert(index, item);
                    AddPostDivider(PostList.IndexOf(item) + 1);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e); Log.Debug("wael >> FeedCombiner", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
            }
        }

        public void AddPostBoxPostView(string typePost, int index, PostDataObject postData = null)
        {
            try
            {
                var item = new AdapterModelsClass
                {
                    TypeView = PostModelType.AddPostBox,
                    TypePost = typePost,
                    Id = 959595959,
                    PostData = postData
                };

                if (index == -1)
                {
                    PostList.Add(item);
                    AddPostDivider();
                }
                else
                {
                    PostList.Insert(index, item);
                    AddPostDivider(index);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e); Log.Debug("wael >> FeedCombiner", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
            }
        }

        public void SearchForPostsView(string type, PostDataObject postData = null)
        {
            try
            {
                var check = PostList.FirstOrDefault(a => a.TypeView == PostModelType.SearchForPosts);
                if (check == null)
                {
                    var item = new AdapterModelsClass
                    {
                        TypeView = PostModelType.SearchForPosts,
                        TypePost = type,
                        Id = 2321,
                        PostData = postData
                    };

                    PostList.Add(item);
                    AddPostDivider();
                } 
            }
            catch (Exception e)
            {
                Console.WriteLine(e); Log.Debug("wael >> FeedCombiner", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
            }
        }

        public void SetAnnouncementAlertView(string subText, string color, int? resource = null)
        {
            try
            {
                var item = new AdapterModelsClass
                {
                    TypeView = PostModelType.AlertBoxAnnouncement,
                    Id = 66655666,
                    AlertModel = new AlertModelClass
                    {
                        TitleHead = MainContext.GetText(Resource.String.Lbl_Announcement),
                        SubText = subText,
                        LinerColor = color
                    }
                };

                if (resource.HasValue)
                    item.AlertModel.ImageDrawable = resource.Value;

                PostList.Insert(0, item);
                AddPostDivider(PostList.IndexOf(item) + 1);
            }
            catch (Exception e)
            {
                Console.WriteLine(e); Log.Debug("wael >> FeedCombiner", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
            }
        }

        public void AddGreetingAlertPostView()
        {
            try
            {
                var item = PostModelResolver.PrepareGreetingAlert();
                if (item == null)
                    return;

                PostList.Add(item);
                AddPostDivider();
            }
            catch (Exception e)
            {
                Console.WriteLine(e); Log.Debug("wael >> FeedCombiner", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
            }
        }

        public void AddFindMoreAlertPostView(string type)
        {
            try
            {
                var item = PostModelResolver.SetFindMoreAlert(type);
                if (item == null)
                    return;

                PostList.Add(item);
                AddPostDivider();
            }
            catch (Exception e)
            {
                Console.WriteLine(e); Log.Debug("wael >> FeedCombiner", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
            }
        }

        public static string TypePost = TemplateView.MediumTemplate;
        public void AddAdsPostView(PostModelType modelType)
        {
            try
            {
                if (modelType == PostModelType.AdMob1)
                {
                    if (TypePost == TemplateView.MediumTemplate)
                    {
                        TypePost = TemplateView.NativeAppInstallAd;
                    }
                    else if (TypePost == TemplateView.NativeAppInstallAd)
                    {
                        TypePost = TemplateView.MediumTemplate;
                    }

                    if (TypePost == TemplateView.MediumTemplate)
                    {
                        var adMobBox = new AdapterModelsClass
                        {
                            TypeView = PostModelType.AdMob1,
                            Id = 2222019,
                        };
                        PostList.Add(adMobBox); 
                    }
                    else
                    {
                        var adMobBox = new AdapterModelsClass
                        {
                            TypeView = PostModelType.AdMob2,
                            Id = 2222019,
                        };
                        PostList.Add(adMobBox); 
                    } 
                }
                else
                {
                    var adMobBox = new AdapterModelsClass
                    {
                        TypeView = PostModelType.FbAdNative,
                        Id = 2222220
                    };
                    PostList.Add(adMobBox);
                }

                AddPostDivider();
            }
            catch (Exception e)
            {
                Console.WriteLine(e); Log.Debug("wael >> FeedCombiner", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
            }
        }

        public void AddSuggestedBoxPostView(PostModelType modelType)
        {
            try
            {
                if (modelType == PostModelType.SuggestedGroupsBox)
                {
                    PostList.Add(new AdapterModelsClass
                    {
                        TypeView = PostModelType.SuggestedGroupsBox,
                        Id = 3216545,
                    });
                }
                else
                {
                    PostList.Add(new AdapterModelsClass
                    {
                        TypeView = PostModelType.SuggestedUsersBox,
                        Id = 3228546,
                    });
                }

                AddPostDivider();
            }
            catch (Exception e)
            {
                Console.WriteLine(e); Log.Debug("wael >> FeedCombiner", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
            }
        }

        #endregion

        // Insert Post
        //=====================================
        private void InsertOnTopAutoSection(bool isSharing = false)
        {
            try
            {
                if (isSharing)
                {

                    var getSharedPostType = PostFunctions.GetAdapterType(PostCollection.SharedInfo.SharedInfoClass);

                    var collection = PostCollection.SharedInfo.SharedInfoClass;

                    switch (getSharedPostType)
                    {
                        case PostModelType.BlogPost:
                            PostModelResolver.PrepareBlog(collection);
                            break;
                        case PostModelType.EventPost:
                            PostModelResolver.PrepareEvent(collection);
                            if (Convert.ToInt32(collection.EventId) > 0)
                                return;
                            break;
                        case PostModelType.ColorPost:
                            PostModelResolver.PrepareColorBox(collection);
                            break;
                        case PostModelType.LinkPost:
                            PostModelResolver.PrepareLink(collection);
                            break;
                        case PostModelType.ProductPost:
                            PostModelResolver.PrepareProduct(collection);
                            break;
                        case PostModelType.FundingPost:
                            PostModelResolver.PrepareFunding(collection);
                            break;
                        case PostModelType.OfferPost:
                            PostModelResolver.PrepareOffer(collection);
                            break;
                        case PostModelType.MapPost:
                            PostModelResolver.PrepareMapPost(collection);
                            break;
                        case PostModelType.PollPost:
                        case PostModelType.AdsPost:
                        case PostModelType.AdMob1:
                        case PostModelType.AdMob2:
                        case PostModelType.FbAdNative:
                            return;
                        case PostModelType.VideoPost:
                            WRecyclerView.GetInstance()?.CacheVideosFiles(Uri.Parse(collection.PostFileFull));
                            break;
                        case PostModelType.JobPost:
                            AddJobPost();
                            return;
                    }

                    var item = new AdapterModelsClass
                    {
                        TypeView = getSharedPostType,
                        Id = int.Parse((int)getSharedPostType + collection.Id),
                        PostData = collection,
                        IsDefaultFeedPost = true
                    };

                    CountIndex++;
                    PostList.Insert(CountIndex, item);
                }
                else
                {
                    switch (PostFeedType)
                    {
                        case PostModelType.BlogPost:
                            PostModelResolver.PrepareBlog(PostCollection);
                            break;
                        case PostModelType.EventPost:
                            PostModelResolver.PrepareEvent(PostCollection);
                            if (Convert.ToInt32(PostCollection.EventId) > 0)
                                return;
                            break;
                        case PostModelType.ColorPost:
                            PostModelResolver.PrepareColorBox(PostCollection);
                            break;
                        case PostModelType.LinkPost:
                            PostModelResolver.PrepareLink(PostCollection);
                            break;
                        case PostModelType.ProductPost:
                            PostModelResolver.PrepareProduct(PostCollection);
                            break;
                        case PostModelType.FundingPost:
                            PostModelResolver.PrepareFunding(PostCollection);
                            break;
                        case PostModelType.OfferPost:
                            PostModelResolver.PrepareOffer(PostCollection);
                            break;
                        case PostModelType.MapPost:
                            PostModelResolver.PrepareMapPost(PostCollection);
                            break;
                        case PostModelType.PollPost:
                        case PostModelType.AdsPost:
                        case PostModelType.AdMob1:
                        case PostModelType.AdMob2:
                        case PostModelType.FbAdNative:
                            return;
                        case PostModelType.VideoPost:
                            WRecyclerView.GetInstance()?.CacheVideosFiles(Uri.Parse(PostCollection.PostFileFull));
                            break;
                        case PostModelType.JobPost:
                            AddJobPost();
                            return;
                        case PostModelType.SharedPost:
                            InsertOnTopSharedPost();
                            return;
                    }

                    var item = new AdapterModelsClass
                    {
                        TypeView = PostFeedType,
                        Id = int.Parse((int)PostFeedType + PostCollection.Id),
                        PostData = PostCollection,
                        IsDefaultFeedPost = true,
                        PostDataDecoratedContent = TextDecorator.SetupStrings(PostCollection, MainContext),
                    };

                    CountIndex++;
                    PostList.Insert(CountIndex, item);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e); Log.Debug("wael >> FeedCombiner", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
            }
        }

        #region Insert Post

        public void InsertOnTopPostPromote(bool isSharing = false)
        {
            try
            {
                bool isPromoted = false;
                if (isSharing)
                {
                    var collection = PostCollection.SharedInfo.SharedInfoClass;

                    if (collection.IsPostBoosted == "1" || collection.SharedInfo.SharedInfoClass != null && collection.SharedInfo.SharedInfoClass?.IsPostBoosted == "1")
                        isPromoted = true;

                    if (isPromoted)
                    {
                        var item = new AdapterModelsClass
                        {
                            TypeView = PostModelType.PromotePost,
                            Id = int.Parse((int)PostModelType.PromotePost + collection.Id),
                            PostData = collection,
                            IsDefaultFeedPost = true
                        };

                        CountIndex++;
                        PostList.Insert(CountIndex, item);
                    }
                }
                else
                {
                    if (PostCollection.IsPostBoosted == "1" || PostCollection.SharedInfo.SharedInfoClass != null && PostCollection.SharedInfo.SharedInfoClass?.IsPostBoosted == "1")
                        isPromoted = true;

                    if (isPromoted)
                    {
                        var item = new AdapterModelsClass
                        {
                            TypeView = PostModelType.PromotePost,
                            Id = int.Parse((int)PostModelType.PromotePost + PostCollection.Id),
                            PostData = PostCollection,
                            IsDefaultFeedPost = true,
                            PostDataDecoratedContent = TextDecorator.SetupStrings(PostCollection, MainContext),
                        };
                        CountIndex++;
                        PostList.Insert(CountIndex, item);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e); Log.Debug("wael >> FeedCombiner", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
            }
        }

        public void InsertOnTopPostHeader(bool isSharing = false)
        {
            try
            {
                if (isSharing)
                {
                    var collection = PostCollection.SharedInfo.SharedInfoClass;
                    PostModelResolver.PrepareHeader(collection);
                    var item = new AdapterModelsClass
                    {
                        TypeView = PostModelType.SharedHeaderPost,
                        Id = int.Parse((int)PostModelType.SharedHeaderPost + collection.Id),
                        PostData = collection,
                        IsDefaultFeedPost = true
                    };

                    CountIndex++;
                    PostList.Insert(CountIndex, item);
                }
                else
                {
                    PostModelResolver.PrepareHeader(PostCollection);
                    var item = new AdapterModelsClass
                    {
                        TypeView = PostModelType.HeaderPost,
                        Id = int.Parse((int)PostModelType.HeaderPost + PostCollection.Id),
                        PostData = PostCollection,
                        IsDefaultFeedPost = true,
                        PostDataDecoratedContent = TextDecorator.SetupStrings(PostCollection, MainContext),
                    };
                    CountIndex++;
                    PostList.Insert(CountIndex, item);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e); Log.Debug("wael >> FeedCombiner", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
            }
        }

        private void InsertOnTopPostTextSection(bool isSharing = false)
        {
            try
            {
                if (isSharing)
                {
                    var collection = PostCollection.SharedInfo.SharedInfoClass;

                    var getSharedPostType = PostFunctions.GetAdapterType(collection);
                    if (getSharedPostType == PostModelType.ColorPost)
                        return;


                    if (string.IsNullOrEmpty(collection.Orginaltext))
                        return;

                    PostModelResolver.PrepareTextSection(collection);

                    var item = new AdapterModelsClass
                    {
                        TypeView = PostModelType.TextSectionPostPart,
                        Id = int.Parse((int)PostModelType.TextSectionPostPart + collection.Id),
                        PostData = collection,
                        IsDefaultFeedPost = true
                    };

                    CountIndex++;
                    PostList.Insert(CountIndex, item);
                }
                else
                {
                    if (PostFeedType == PostModelType.ColorPost)
                        return;

                    if (string.IsNullOrEmpty(PostCollection.Orginaltext))
                        return;

                    PostModelResolver.PrepareTextSection(PostCollection);

                    var item = new AdapterModelsClass
                    {
                        TypeView = PostModelType.TextSectionPostPart,
                        Id = int.Parse((int)PostModelType.TextSectionPostPart + PostCollection.Id),
                        PostData = PostCollection,
                        IsDefaultFeedPost = true
                    };
                    CountIndex++;
                    PostList.Insert(CountIndex, item);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e); Log.Debug("wael >> FeedCombiner", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
            }
        }

        private void InsertOnTopPollsPostView()
        {
            try
            {
                if (PostCollection.Options != null)
                {
                    var count = PostCollection.Options.Count;
                    if (count > 0)
                    {
                        foreach (var poll in PostCollection.Options)
                        {
                            PostModelResolver.PreparePoll(poll);

                            poll.PostId = PostCollection.Id;
                            poll.RelatedToPollsCount = count;

                            var i = new AdapterModelsClass
                            {
                                TypeView = PostModelType.PollPost,
                                Id = int.Parse((int)PostModelType.PollPost + PostCollection.Id),
                                PostData = PostCollection,
                                IsDefaultFeedPost = true,
                                PollId = poll.Id,
                                PollsOption = poll,
                                PollOwnerUserId = PostCollection.Publisher?.UserId
                            };
                            CountIndex++;
                            PostList.Insert(CountIndex, i);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e); Log.Debug("wael >> FeedCombiner", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
            }
        }

        private void InsertOnTopPostPrevBottom()
        {
            try
            {
                PostModelResolver.PreparePostPrevBottom(PostCollection);

                var item = new AdapterModelsClass
                {
                    TypeView = PostModelType.PrevBottomPostPart,
                    Id = int.Parse((int)PostModelType.PrevBottomPostPart + PostCollection.Id),
                    PostData = PostCollection,
                    IsDefaultFeedPost = true
                };

                CountIndex++;
                PostList.Insert(CountIndex, item);
            }
            catch (Exception e)
            {
                Console.WriteLine(e); Log.Debug("wael >> FeedCombiner", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
            }
        }

        private void InsertOnTopPostFooter()
        {
            try
            {
                var item = new AdapterModelsClass
                {
                    TypeView = PostModelType.BottomPostPart,
                    Id = int.Parse((int)PostModelType.BottomPostPart + PostCollection.Id),
                    PostData = PostCollection,
                    IsDefaultFeedPost = true
                };

                CountIndex++;
                PostList.Insert(CountIndex, item);
            }
            catch (Exception e)
            {
                Console.WriteLine(e); Log.Debug("wael >> FeedCombiner", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
            }
        }

        private void InsertOnTopPostDivider()
        {
            try
            {
                var item = new AdapterModelsClass
                {
                    TypeView = PostModelType.Divider,
                    Id = int.Parse((int)PostModelType.Divider + PostCollection.Id),

                    PostData = PostCollection,
                    IsDefaultFeedPost = true
                };

                CountIndex++;
                PostList.Insert(CountIndex, item);
            }
            catch (Exception e)
            {
                Console.WriteLine(e); Log.Debug("wael >> FeedCombiner", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
            }
        }

        private void InsertOnTopSharedPost()
        {
            try
            {
                if (PostCollection.SharedInfo.SharedInfoClass != null)
                {
                    InsertOnTopPostPromote(true);
                    InsertOnTopPostHeader(true);
                    InsertOnTopPostTextSection(true);
                    InsertOnTopAutoSection(true);
                    InsertOnTopPollsPostView();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e); Log.Debug("wael >> FeedCombiner", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
            }
        }

        #endregion

        private int CountIndex = 1;
        public void CombineDefaultPostSections(string type = "Add")
        {
            try
            {
                if (type == "Add")
                {
                    AddPostPromote();
                    AddPostHeader();
                    AddPostTextSection();
                    AddAutoSection();
                    AddPollsPostView();
                    AddPostPrevBottom();
                    AddPostFooter();
                    AddPostCommentAbility();
                    AddPostDivider();
                }
                else
                {
                    CountIndex = 0;
                    var model1 = PostList.FirstOrDefault(a => a.TypeView == PostModelType.Story);
                    var model2 = PostList.FirstOrDefault(a => a.TypeView == PostModelType.AddPostBox);
                    var model3 = PostList.FirstOrDefault(a => a.TypeView == PostModelType.AlertBox);
                    var model4 = PostList.FirstOrDefault(a => a.TypeView == PostModelType.SearchForPosts);

                    if (model4 != null)
                        CountIndex += PostList.IndexOf(model4) + 1;
                    else if (model3 != null)
                        CountIndex += PostList.IndexOf(model3) + 1;
                    else if (model2 != null)
                        CountIndex += PostList.IndexOf(model2) + 1;
                    else if (model1 != null)
                        CountIndex += PostList.IndexOf(model1) + 1;
                    else
                        CountIndex = 0;

                    InsertOnTopPostPromote();
                    InsertOnTopPostHeader();
                    InsertOnTopPostTextSection();
                    InsertOnTopAutoSection();
                    InsertOnTopPollsPostView();
                    InsertOnTopPostPrevBottom();
                    InsertOnTopPostFooter();
                    InsertOnTopPostDivider();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e); Log.Debug("wael >> FeedCombiner", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
            }
        }

    }
}