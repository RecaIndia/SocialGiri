using System.Collections.Generic;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Posts;

namespace WoWonder.Activities.NativePost.Post
{ 
    public enum PostModelType
    {
        NormalPost = 1, AboutBox = 2, PagesBox = 3, GroupsBox = 4, FollowersBox = 5, ImagesBox = 6, Story = 33333333, AddPostBox = 9999999, EmptyState = 8, AlertBox = 9, VideoPost = 10, ImagePost = 11, VoicePost = 12,
        StickerPost = 13, YoutubePost = 14, DeepSoundPost = 15, PlayTubePost = 16, LinkPost = 17, ProductPost = 18, BlogPost = 19, FilePost = 20, AlertJoinBox = 21, SharedPost = 22, EventPost = 23, ColorPost = 24, FacebookPost = 25, MultiImage2 = 26,
        MultiImage3 = 27, MultiImage4 = 28, MultiImages = 29, PollPost = 30, AdsPost = 31, AdMob1 = 32, AdMob2 = 3200, JobPost =33, AlertBoxAnnouncement = 34, FundingPost = 35 , PurpleFundPost = 36 , SocialLinks = 37 , SuggestedGroupsBox = 38,
        VimeoPost = 39 , MapPost = 40, OfferPost = 41 , SearchForPosts  = 4265465, LivePost = 42 , Section = 38546843 , SuggestedUsersBox = 43 , FbAdNative = 44, JobPostSection2 = 33, JobPostSection1 = 45, SharedHeaderPost = 46,
        HeaderPost = 988, TextSectionPostPart = 998, PrevBottomPostPart = 100, BottomPostPart = 101 , Divider = 102, ViewProgress = 103, PromotePost = 104,AddCommentSection = 999, CommentSection = 981
    }
      
    public class AboutModelClass
    {
        public string TitleHead { get; set; }
        public string Description { get; set; }
    }

    public class SocialLinksModelClass
    {
        public string Facebook { get; set; }
        public string Instegram { get; set; }
        public string Twitter { get; set; }
        public string  Google { get; set; }
        public string Vk { get; set; }
        public string Youtube { get; set; }
    }

    public class FollowersModelClass
    {
        public List<UserDataObject> FollowersList { get; set; }
        public string TitleHead { get; set; }
        public string More { get; set; }
    }

    public class GroupsModelClass
    {
        public List<GroupClass> GroupsList { get; set; }
        public string TitleHead { get; set; }
        public string More { get; set; }
        public string UserProfileId { get; set; }
    }
     
    public class PagesModelClass
    {
        public List<PageClass> PagesList { get; set; }
        public string TitleHead { get; set; }
        public string More { get; set; }
    }
    public class ImagesModelClass
    {
        public List<PostDataObject> ImagesList { get; set; }
        public string TitleHead { get; set; }
        public string More { get; set; }
    }

    public class AlertModelClass
    {
        public int ImageDrawable { get; set; }
        public string TitleHead { get; set; }
        public string SubText { get; set; }
        public string LinerColor { get; set; }


        public string TypeAlert { get; set; }
        public int IconImage { get; set; }
    } 
}