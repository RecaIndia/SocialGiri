using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Com.Theartofdev.Edmodo.Cropper;
using Newtonsoft.Json;
using Plugin.Share;
using Plugin.Share.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AFollestad.MaterialDialogs;
using Android.Content.Res;
using Android.Graphics;
using Android.Support.V4.Content;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Java.Lang;
using WoWonder.Activities.AddPost;
using WoWonder.Activities.Communities.Adapters;
using WoWonder.Activities.Communities.Groups.Settings;
using WoWonder.Activities.NativePost.Extra;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Group;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Classes.Product;
using WoWonderClient.Requests;
using Exception = System.Exception;
using File = Java.IO.File;
using Uri = Android.Net.Uri;

namespace WoWonder.Activities.Communities.Groups
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class GroupProfileActivity : AppCompatActivity, MaterialDialog.IListCallback,MaterialDialog.ISingleButtonCallback
    {
        #region Variables Basic

        private Button BtnJoin;
        private ImageButton BtnMore;
        private ImageView UserProfileImage, CoverImage, IconBack, IconPrivacy;
        private TextView CategoryText, IconEdit, TxtGroupName, TxtGroupUsername, PrivacyText, TxtEditGroupInfo, TxtMembers, InviteText;
        private FloatingActionButton FloatingActionButtonView;
        private LinearLayout EditAvatarImageGroup;
        private WRecyclerView MainRecyclerView;
        private NativePostAdapter PostFeedAdapter;
        private string GroupId, ImageType;
        public static GroupClass GroupDataClass;
        private ImageView JoinRequestImage1, JoinRequestImage2, JoinRequestImage3;
        private RelativeLayout LayoutJoinRequest;
        private FeedCombiner Combiner;
        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);                 
                View mContentView = Window.DecorView;
                var uiOptions = (int)mContentView.SystemUiVisibility;
                var newUiOptions = uiOptions;

                // newUiOptions |= (int)SystemUiFlags.Fullscreen;
                newUiOptions |= (int)SystemUiFlags.LayoutStable;
                mContentView.SystemUiVisibility = (StatusBarVisibility)newUiOptions;


                base.OnCreate(savedInstanceState);

                Methods.App.FullScreenApp(this);

                // Create your application here
                SetContentView(Resource.Layout.Group_Profile_Layout);
                
                 
                GroupId = Intent.GetStringExtra("GroupId") ?? string.Empty;

                //Get Value And Set Toolbar
                InitComponent();
                SetRecyclerViewAdapters();

                GetDataGroup();
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
                UserProfileImage = (ImageView)FindViewById(Resource.Id.image_profile);
                CoverImage = (ImageView)FindViewById(Resource.Id.iv1);

                IconBack = (ImageView)FindViewById(Resource.Id.image_back);
                EditAvatarImageGroup = (LinearLayout)FindViewById(Resource.Id.LinearEdit);
                TxtEditGroupInfo = (TextView)FindViewById(Resource.Id.tv_EditGroupinfo);
                TxtGroupName = (TextView)FindViewById(Resource.Id.Group_name);
                TxtGroupUsername = (TextView)FindViewById(Resource.Id.Group_Username);
                BtnJoin = (Button)FindViewById(Resource.Id.joinButton);
                BtnMore = (ImageButton)FindViewById(Resource.Id.morebutton);
                IconPrivacy = (ImageView)FindViewById(Resource.Id.IconPrivacy);
                PrivacyText = (TextView)FindViewById(Resource.Id.PrivacyText);
                CategoryText = (TextView)FindViewById(Resource.Id.CategoryText);
                IconEdit = (TextView)FindViewById(Resource.Id.IconEdit);
                TxtMembers = (TextView)FindViewById(Resource.Id.membersText);
                InviteText = (TextView)FindViewById(Resource.Id.InviteText); 
                FloatingActionButtonView = FindViewById<FloatingActionButton>(Resource.Id.floatingActionButtonView);

                JoinRequestImage1 = (ImageView)FindViewById(Resource.Id.image_page_1);
                JoinRequestImage2 = (ImageView)FindViewById(Resource.Id.image_page_2);
                JoinRequestImage3 = (ImageView)FindViewById(Resource.Id.image_page_3);

                LayoutJoinRequest = (RelativeLayout)FindViewById(Resource.Id.layout_join_Request);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconEdit, IonIconsFonts.Edit);

                if (AppSettings.FlowDirectionRightToLeft)
                    IconBack.SetImageResource(Resource.Drawable.ic_action_ic_back_rtl);
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
                MainRecyclerView = FindViewById<WRecyclerView>(Resource.Id.newsfeedRecyler);
                PostFeedAdapter = new NativePostAdapter(this, GroupId, MainRecyclerView, NativeFeedType.Group, SupportFragmentManager);
                MainRecyclerView.SetXAdapter(PostFeedAdapter, null);
                Combiner = new FeedCombiner(null, PostFeedAdapter.ListDiffer, this);
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
                    IconBack.Click += IconBackOnClick;
                    EditAvatarImageGroup.Click += EditAvatarImageGroupOnClick;
                    TxtEditGroupInfo.Click += TxtEditGroupInfoOnClick;
                    BtnJoin.Click += BtnJoinOnClick;
                    BtnMore.Click += BtnMoreOnClick;
                    FloatingActionButtonView.Click += AddPostOnClick;
                    LayoutJoinRequest.Click += LayoutJoinRequestOnClick;
                    TxtMembers.Click += TxtMembersOnClick;
                    InviteText.Click += InviteTextOnClick;
                }
                else
                {
                    IconBack.Click -= IconBackOnClick;
                    EditAvatarImageGroup.Click -= EditAvatarImageGroupOnClick;
                    TxtEditGroupInfo.Click -= TxtEditGroupInfoOnClick;
                    BtnJoin.Click -= BtnJoinOnClick;
                    BtnMore.Click -= BtnMoreOnClick;
                    FloatingActionButtonView.Click -= AddPostOnClick;
                    LayoutJoinRequest.Click -= LayoutJoinRequestOnClick;
                    TxtMembers.Click -= TxtMembersOnClick;
                    InviteText.Click -= InviteTextOnClick;
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
                BtnJoin = null;
                BtnMore = null;
                UserProfileImage = null;
                CoverImage = null;
                IconBack = null;
                IconPrivacy = null;
                CategoryText = null;
                IconEdit = null;
                TxtGroupName = null;
                TxtGroupUsername = null;
                PrivacyText = null;
                TxtEditGroupInfo = null;
                TxtMembers = null;
                InviteText = null;
                FloatingActionButtonView = null;
                EditAvatarImageGroup = null;
                MainRecyclerView = null;
                PostFeedAdapter = null;
                GroupId = null;
                ImageType = null;
                GroupDataClass = null;
                JoinRequestImage1 = null;
                JoinRequestImage2 = null;
                JoinRequestImage3 = null;
                LayoutJoinRequest = null;
                Combiner = null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        #endregion

        #region Events

        //Event Show More : Copy Link , Share , Edit (If user isOwner_Groups)
        private void BtnMoreOnClick(object sender, EventArgs e)
        {
            try
            { 
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                arrayAdapter.Add(GetString(Resource.String.Lbl_CopeLink));
                arrayAdapter.Add(GetString(Resource.String.Lbl_Share));
                if (GroupDataClass.IsOwner)
                {
                    arrayAdapter.Add(GetString(Resource.String.Lbl_Settings));
                }

                dialogList.Title(GetString(Resource.String.Lbl_More));
                dialogList.Items(arrayAdapter);
                dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(this);
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show(); 
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Event Add New post
        private void AddPostOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(AddPostActivity));
                intent.PutExtra("Type", "SocialGroup");
                intent.PutExtra("PostId", GroupId);
                intent.PutExtra("itemObject", JsonConvert.SerializeObject(GroupDataClass));
                StartActivityForResult(intent, 2500);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Event Join_Group => Joined , Join Group
        private async void BtnJoinOnClick(object sender, EventArgs e)
        {
            try
            {
                if (BtnJoin.Tag.ToString() == "MyGroup")
                {
                    SettingGroup_OnClick();
                }
                else
                {
                    if (!Methods.CheckConnectivity())
                    {
                        Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                    }
                    else
                    {
                        string isJoined = BtnJoin.Text == GetText(Resource.String.Btn_Joined) ? "false" : "true";
                        BtnJoin.BackgroundTintList = isJoined == "yes" || isJoined == "true" ? ColorStateList.ValueOf(Color.ParseColor("#efefef")) : ColorStateList.ValueOf(Color.ParseColor(AppSettings.MainColor));
                        BtnJoin.Text = GetText(isJoined == "yes" || isJoined == "true" ? Resource.String.Btn_Joined : Resource.String.Btn_Join_Group);
                        BtnJoin.SetTextColor(isJoined == "yes" || isJoined == "true" ? Color.Black : Color.White);

                        var (apiStatus, respond) = await RequestsAsync.Group.Join_Group(GroupId);
                        if (apiStatus == 200)
                        {
                            if (respond is JoinGroupObject result)
                            { 
                                if (result.JoinStatus == "requested")
                                { 
                                    BtnJoin.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(AppSettings.MainColor));
                                    BtnJoin.Text = GetText(Resource.String.Lbl_Request);
                                    BtnJoin.SetTextColor(Color.White);
                                    BtnMore.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(AppSettings.MainColor));
                                    BtnMore.ImageTintList = ColorStateList.ValueOf(Color.White);
                                } 
                                else
                                {
                                    isJoined = result.JoinStatus == "left" ? "false" : "true";
                                    BtnJoin.BackgroundTintList = isJoined == "yes" || isJoined == "true" ? ColorStateList.ValueOf(Color.ParseColor("#efefef")) : ColorStateList.ValueOf(Color.ParseColor(AppSettings.MainColor));
                                    BtnJoin.Text = GetText(isJoined == "yes" || isJoined == "true" ? Resource.String.Btn_Joined : Resource.String.Btn_Join_Group);
                                    BtnJoin.SetTextColor(isJoined == "yes" || isJoined == "true" ? Color.Black : Color.White);
                                }   
                            }
                        }
                        else Methods.DisplayReportResult(this, respond);
                    }
                } 
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Event Update Image Cover Group
        private void TxtEditGroupInfoOnClick(object sender, EventArgs e)
        {
            try
            {
                OpenDialogGallery("Cover");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Event Update Image avatar Group
        private void EditAvatarImageGroupOnClick(object sender, EventArgs e)
        {
            try
            {
                OpenDialogGallery("Avatar");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Back
        private void IconBackOnClick(object sender, EventArgs e)
        {
           Finish();
        }

        //Join Request
        private void LayoutJoinRequestOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(JoinRequestActivity));
                intent.PutExtra("GroupId", GroupId);
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Show Group Members
        private void TxtMembersOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(GroupMembersActivity));
                intent.PutExtra("itemObject", JsonConvert.SerializeObject(GroupDataClass));
                intent.PutExtra("GroupId", GroupId);
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Invite Members 
        private void InviteTextOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(InviteMembersGroupActivity));
                intent.PutExtra("GroupId", GroupId);
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }


        #endregion

        #region Permissions && Result

        //Result
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                //If its from Camera or Gallery
                if (requestCode == CropImage.CropImageActivityRequestCode)
                {
                    var result = CropImage.GetActivityResult(data);

                    if (resultCode == Result.Ok)
                    {
                        if (result.IsSuccessful)
                        {
                            var resultUri = result.Uri;

                            if (!string.IsNullOrEmpty(resultUri.Path))
                            {
                                string pathImg;
                                if (ImageType == "Cover")
                                {
                                    pathImg = resultUri.Path;
                                    UpdateImageGroup_Api(ImageType, pathImg);
                                }
                                else if (ImageType == "Avatar")
                                {
                                    pathImg = resultUri.Path;
                                    UpdateImageGroup_Api(ImageType, pathImg);
                                }
                            }
                            else
                            {
                                Toast.MakeText(this, GetText(Resource.String.Lbl_something_went_wrong),ToastLength.Long).Show();
                            }
                        } 
                    } 
                }
                else if (requestCode == 2500 && resultCode == Result.Ok)//add post
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
                else if (requestCode == 2005 && resultCode == Result.Ok)
                {
                    string result = data.GetStringExtra("groupItem");
                    var item = JsonConvert.DeserializeObject<GroupClass>(result);
                    if (item != null)
                        LoadPassedData(item);
                }
                else if (requestCode == 2019 && resultCode == Result.Ok)
                { 
                    var manged = GroupsActivity.GetInstance().MAdapter.SocialList.FirstOrDefault(a => a.TypeView == SocialModelType.MangedGroups); 
                    var dataListGroup = manged?.MangedGroupsModel.GroupsList?.FirstOrDefault(a => a.GroupId == GroupId);
                    if (dataListGroup != null)
                    { 
                        manged.MangedGroupsModel.GroupsList.Remove(dataListGroup); 
                        GroupsActivity.GetInstance().MAdapter.NotifyDataSetChanged();

                        ListUtils.MyGroupList.Remove(dataListGroup);
                    }
                    Finish();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults); 
                if (requestCode == 108)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        OpenDialogGallery(ImageType);
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long).Show();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
         
        #endregion
         
        #region MaterialDialog

        public void OnSelection(MaterialDialog p0, View p1, int itemId, ICharSequence itemString)
        {
            try
            {
                string text = itemString.ToString();
                if (text == GetString(Resource.String.Lbl_CopeLink))
                {
                    CopyLinkEvent();
                }
                else if (text == GetString(Resource.String.Lbl_Share))
                {
                    ShareEvent();
                }
                else if (text == GetString(Resource.String.Lbl_Settings))
                {
                    SettingGroup_OnClick();
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

        //Event Menu >> Copy Link
        private void CopyLinkEvent()
        {
            try
            {
                var clipboardManager = (ClipboardManager)GetSystemService(ClipboardService);

                var clipData = ClipData.NewPlainText("text", GroupDataClass.Url);
                clipboardManager.PrimaryClip = clipData;

                Toast.MakeText(this, GetText(Resource.String.Lbl_Copied), ToastLength.Short).Show();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Event Menu >> Share
        private async void ShareEvent()
        {
            try
            {
                //Share Plugin same as video
                if (!CrossShare.IsSupported) return;

                await CrossShare.Current.Share(new ShareMessage
                {
                    Title = GroupDataClass.GroupName,
                    Text = GroupDataClass.About,
                    Url = GroupDataClass.Url
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Event Menu >> Setting
        private void SettingGroup_OnClick()
        {
            try
            {
                var intent = new Intent(this, typeof(SettingsGroupActivity));
                intent.PutExtra("itemObject", JsonConvert.SerializeObject(GroupDataClass));
                intent.PutExtra("GroupId", GroupId);
                StartActivity(intent);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        } 
         
        #endregion

        #region Get Data Group

        private void GetDataGroup()
        {
            try
            {
                GroupDataClass = JsonConvert.DeserializeObject<GroupClass>(Intent.GetStringExtra("GroupObject"));
                if (GroupDataClass != null)
                {
                    LoadPassedData(GroupDataClass); 
                } 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            StartApiService();
        }

        private void LoadPassedData(GroupClass result)
        {
            try
            {
                GlideImageLoader.LoadImage(this, result.Avatar, UserProfileImage, ImageStyle.CenterCrop, ImagePlaceholders.Color);
                Glide.With(this).Load(result.Cover.Replace(" ", "")).Apply(new RequestOptions().Placeholder(Resource.Drawable.Cover_image).Error(Resource.Drawable.Cover_image)).Into(CoverImage);

                TxtGroupUsername.Text = "@" + result.Username;
                TxtGroupName.Text = Methods.FunString.DecodeString(result.Name);
                CategoryText.Text = Methods.FunString.DecodeString(result.Category); 

                if (result.UserId == UserDetails.UserId)
                    result.IsOwner = true;

                if (result.IsOwner)
                {
                    BtnJoin.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(AppSettings.MainColor));
                    BtnJoin.Text = GetText(Resource.String.Lbl_Edit);
                    BtnJoin.SetTextColor( Color.White);
                    BtnJoin.Tag = "MyGroup";
                    BtnMore.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(AppSettings.MainColor));
                    BtnMore.ImageTintList = ColorStateList.ValueOf(Color.White); 
                }
                else
                {
                    BtnJoin.BackgroundTintList = result.IsJoined == "yes" || result.IsJoined == "true" ? ColorStateList.ValueOf(Color.ParseColor("#efefef")) : ColorStateList.ValueOf(Color.ParseColor(AppSettings.MainColor));
                    BtnJoin.Text = GetText(result.IsJoined == "yes" || result.IsJoined == "true" ? Resource.String.Btn_Joined : Resource.String.Btn_Join_Group);
                    BtnJoin.SetTextColor(result.IsJoined == "yes" || result.IsJoined == "true" ? Color.Black : Color.White);
                    BtnMore.BackgroundTintList = result.IsJoined == "yes" || result.IsJoined == "true" ? ColorStateList.ValueOf(Color.ParseColor("#efefef")) : ColorStateList.ValueOf(Color.ParseColor(AppSettings.MainColor));
                    BtnMore.ImageTintList = result.IsJoined == "yes" || result.IsJoined == "true" ? ColorStateList.ValueOf(Color.Black) : ColorStateList.ValueOf(Color.White);
                    BtnJoin.Tag = "UserGroup";
                }

                if (result.IsOwner || result.IsJoined == "yes" || result.IsJoined == "true")
                {
                    var checkSection = PostFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.AddPostBox);
                    if (checkSection == null)
                    {
                        Combiner.AddPostDivider();

                        Combiner.AddPostBoxPostView("Group", -1, new PostDataObject() { GroupRecipient = result });

                        if (AppSettings.ShowSearchForPosts)
                            Combiner.SearchForPostsView("Group", new PostDataObject() { GroupRecipient = result }); 

                        PostFeedAdapter.NotifyItemInserted(PostFeedAdapter.ListDiffer.Count -1 );
                    } 
                }
                 
                PrivacyText.Text = GetText(result.Privacy == "1" ? Resource.String.Radio_Public : Resource.String.Radio_Private);

                if (result.Privacy != "1")
                    IconPrivacy.SetImageResource(Resource.Drawable.ic_private);

                if (result.IsOwner)
                {
                    EditAvatarImageGroup.Visibility = ViewStates.Visible;
                    TxtEditGroupInfo.Visibility = ViewStates.Visible;
                }
                else
                {
                    EditAvatarImageGroup.Visibility = ViewStates.Gone;
                    TxtEditGroupInfo.Visibility = ViewStates.Gone;
                }
                 
                if (result.Privacy == "1" || result.IsOwner)
                {
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => MainRecyclerView.FetchNewsFeedApiPosts() });
                }
                else
                {
                    if (PostFeedAdapter.ListDiffer.FirstOrDefault(a => a.IsDefaultFeedPost) != null)
                    {
                        var emptyStateChecker = PostFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.EmptyState);
                        if (emptyStateChecker != null && PostFeedAdapter.ListDiffer.Count > 1)
                            PostFeedAdapter.ListDiffer.Remove(emptyStateChecker);
                    }
                    else
                    {
                        var emptyStateChecker = PostFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.EmptyState);
                        if (emptyStateChecker == null)
                            PostFeedAdapter.ListDiffer.Add(new AdapterModelsClass { TypeView = PostModelType.EmptyState, Id = 744747447 });
                    }
                    PostFeedAdapter.NotifyDataSetChanged();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { GetGroupDataApi, GetJoin });
        }

        private async Task GetGroupDataApi()
        {
            var (apiStatus, respond) = await RequestsAsync.Group.Get_Group_Data(GroupId);

            if (apiStatus != 200 || !(respond is GetGroupDataObject result) || result.GroupData == null)
                Methods.DisplayReportResult(this, respond);
            else
            {
                GroupDataClass = result.GroupData;
                LoadPassedData(GroupDataClass);
            } 
        }
          
        #endregion

        #region Update Image Avatar && Cover
         
        private void OpenDialogGallery(string typeImage)
        {
            try
            {
                ImageType = typeImage;
                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    Methods.Path.Chack_MyFolder();

                    //Open Image 
var myUri = Uri.FromFile(new File(Methods.Path.FolderDiskImage, Methods.GetTimestamp(DateTime.Now) + ".jpeg"));
                    CropImage.Builder()
                        .SetInitialCropWindowPaddingRatio(0)
                        .SetAutoZoomEnabled(true)
                        .SetMaxZoom(4)
                        .SetGuidelines(CropImageView.Guidelines.On)
                        .SetCropMenuCropButtonTitle(GetText(Resource.String.Lbl_Crop))
                        .SetOutputUri(myUri).Start(this);
                }
                else
                {
                    if (!CropImage.IsExplicitCameraPermissionRequired(this) && CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted &&
                        CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted && CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted)
                    {
                        Methods.Path.Chack_MyFolder();

                        //Open Image 
    var myUri = Uri.FromFile(new File(Methods.Path.FolderDiskImage, Methods.GetTimestamp(DateTime.Now) + ".jpeg"));
                        CropImage.Builder()
                            .SetInitialCropWindowPaddingRatio(0)
                            .SetAutoZoomEnabled(true)
                            .SetMaxZoom(4)
                            .SetGuidelines(CropImageView.Guidelines.On)
                            .SetCropMenuCropButtonTitle(GetText(Resource.String.Lbl_Crop))
                            .SetOutputUri(myUri).Start(this);
                    }
                    else
                    {
                        new PermissionsController(this).RequestPermission(108);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        // Function Update Image Group : Avatar && Cover
        private async void UpdateImageGroup_Api(string type, string path)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                }
                else
                {
                    if (type == "Avatar")
                    {
                        var (apiStatus, respond) = await RequestsAsync.Group.Update_Group_Avatar(GroupId, path);
                        if (apiStatus == 200)
                        {
                            if (respond is MessageObject result)
                            {
                                Toast.MakeText(this, result.Message, ToastLength.Short).Show();

                                //Set image
                                File file2 = new File(path);
                                var photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);
                                Glide.With(this).Load(photoUri).Apply(new RequestOptions()).Into(UserProfileImage);

                                //GlideImageLoader.LoadImage(this, file.Path, UserProfileImage, ImageStyle.RoundedCrop, ImagePlaceholders.Color);
                            }
                        }
                        else Methods.DisplayReportResult(this, respond);
                    }
                    else if (type == "Cover")
                    {
                        var (apiStatus, respond) = await RequestsAsync.Group.Update_Group_Cover(GroupId, path);
                        if (apiStatus == 200)
                        {
                            if (!(respond is MessageObject result))
                                return;

                            Toast.MakeText(this, result.Message, ToastLength.Short).Show();

                            //Set image
                            File file2 = new File(path);
                            var photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);
                            Glide.With(this).Load(photoUri).Apply(new RequestOptions()).Into(CoverImage);


                            //GlideImageLoader.LoadImage(this, file.Path, CoverImage, ImageStyle.CenterCrop, ImagePlaceholders.Color);
                        }
                        else Methods.DisplayReportResult(this, respond);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        private async Task GetJoin()
        {
            if (GroupDataClass.UserId == UserDetails.UserId)
            {
                (int apiStatus, var respond) = await RequestsAsync.Group.GetGroupJoinRequests(GroupId, "5");
                if (apiStatus == 200)
                {
                    if (respond is GetGroupJoinRequestsObject result)
                    {
                        RunOnUiThread(() =>
                        {
                            var respondList = result.Data.Count;
                            if (respondList > 0)
                            {
                                LayoutJoinRequest.Visibility = ViewStates.Visible;
                                try
                                {
                                    for (var i = 0; i < 4; i++)
                                        switch (i)
                                        {
                                            case 0:
                                                GlideImageLoader.LoadImage(this, result.Data[i]?.UserData?.Avatar, JoinRequestImage1, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                                                break;
                                            case 1:
                                                GlideImageLoader.LoadImage(this, result.Data[i]?.UserData?.Avatar, JoinRequestImage2, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                                                break;
                                            case 2:
                                                GlideImageLoader.LoadImage(this, result.Data[i]?.UserData?.Avatar, JoinRequestImage3, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                                                break;
                                        }
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                }
                            }
                            else
                            {
                                LayoutJoinRequest.Visibility = ViewStates.Gone;
                            }
                        });
                    }
                }
                else Methods.DisplayReportResult(this, respond);
            }
            else
            {
                RunOnUiThread(() => { LayoutJoinRequest.Visibility = ViewStates.Gone; });
            } 
        }
    }
}