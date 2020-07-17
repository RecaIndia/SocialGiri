using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AFollestad.MaterialDialogs;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Text;
using Android.Views;
using Android.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Com.Theartofdev.Edmodo.Cropper;
using Java.Lang;
using Newtonsoft.Json;
using Plugin.Share;
using Plugin.Share.Abstractions;
using WoWonder.Activities.AddPost;
using WoWonder.Activities.Communities.Adapters;
using WoWonder.Activities.Communities.Pages.Settings;
using WoWonder.Activities.General;
using WoWonder.Activities.NativePost.Extra;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo.SuperTextLibrary;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Page;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Classes.Product;
using WoWonderClient.Requests;
using ClipboardManager = Android.Content.ClipboardManager;
using Exception = System.Exception;
using File = Java.IO.File;
using String = Java.Lang.String;
using Uri = Android.Net.Uri;

namespace WoWonder.Activities.Communities.Pages
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class PageProfileActivity : AppCompatActivity, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback, MaterialDialog.IInputCallback
    { 
        #region Variables Basic

        private ImageView ProfileImage, CoverImage, IconBack;
        private TextView TxtPageName, TxtPageUsername, CategoryText, IconCategory, IconEdit, IconLike, LikeCountText, IconMembers;
        private SuperTextView AboutDesc;
        private Button BtnLike, ActionButton;
        private ImageButton BtnMore, MessageButton;
        private FloatingActionButton FloatingActionButtonView;
        private LinearLayout EditAvatarImagePage, RatingLiner, MembersLiner;
        private TextView TxtEditPageInfo;
        private WRecyclerView MainRecyclerView;
        private NativePostAdapter PostFeedAdapter;
        private string PageId = "", UserId = "", ImageType = "";
        public static PageClass PageData;
        public RatingBar RatingBarView;
        private FeedCombiner Combiner;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                View mContentView = Window.DecorView;
                var uiOptions = (int)mContentView.SystemUiVisibility;
                var newUiOptions = uiOptions;

                // newUiOptions |= (int)SystemUiFlags.Fullscreen;
                newUiOptions |= (int)SystemUiFlags.LayoutStable;
                mContentView.SystemUiVisibility = (StatusBarVisibility)newUiOptions;

                base.OnCreate(savedInstanceState);
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);

                Methods.App.FullScreenApp(this);

                // Create your application here
                SetContentView(Resource.Layout.Page_Profile_Layout);

                PageId = Intent.GetStringExtra("PageId") ?? string.Empty;

                //Get Value And Set Toolbar
                InitComponent(); 
                SetRecyclerViewAdapters();

                GetDataPage();
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
            catch (Exception exception)
            {
                Console.WriteLine(exception);
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
                ProfileImage = (ImageView)FindViewById(Resource.Id.image_profile);
                CoverImage = (ImageView)FindViewById(Resource.Id.iv1);
                IconBack = (ImageView)FindViewById(Resource.Id.image_back);
                RatingLiner = (LinearLayout)FindViewById(Resource.Id.ratingLiner);
                MembersLiner = (LinearLayout)FindViewById(Resource.Id.liner5);
                EditAvatarImagePage = (LinearLayout)FindViewById(Resource.Id.LinearEdit);
                TxtEditPageInfo = (TextView)FindViewById(Resource.Id.tv_EditPageinfo);
                TxtPageName = (TextView)FindViewById(Resource.Id.Page_name);
                TxtPageUsername = (TextView)FindViewById(Resource.Id.Page_Username);
                BtnLike = (Button)FindViewById(Resource.Id.likeButton);
                BtnMore = (ImageButton)FindViewById(Resource.Id.morebutton);
                IconLike = (TextView)FindViewById(Resource.Id.IconLike);
                IconMembers = (TextView)FindViewById(Resource.Id.IconMembers);
                LikeCountText = (TextView)FindViewById(Resource.Id.LikeCountText);
                IconCategory = (TextView)FindViewById(Resource.Id.IconCategory);
                CategoryText = (TextView)FindViewById(Resource.Id.CategoryText);
                IconEdit = (TextView)FindViewById(Resource.Id.IconEdit);
                AboutDesc = (SuperTextView)FindViewById(Resource.Id.aboutdesc);
                RatingBarView = (RatingBar)FindViewById(Resource.Id.ratingBar);
                FloatingActionButtonView = FindViewById<FloatingActionButton>(Resource.Id.floatingActionButtonView);
                MessageButton = (ImageButton)FindViewById(Resource.Id.messagebutton);
                ActionButton = (Button)FindViewById(Resource.Id.actionButton);
                 
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconLike, IonIconsFonts.Thumbsup);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconCategory, IonIconsFonts.Pricetag);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconEdit, IonIconsFonts.Edit); 
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconMembers, IonIconsFonts.AndroidPersonAdd);
                 
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
                PostFeedAdapter = new NativePostAdapter(this, PageId, MainRecyclerView, NativeFeedType.Page, SupportFragmentManager);
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
                    BtnLike.Click += BtnLikeOnClick;
                    BtnMore.Click += BtnMoreOnClick;
                    TxtEditPageInfo.Click += TxtEditPageInfoOnClick;
                    IconBack.Click += IconBackOnClick;
                    EditAvatarImagePage.Click += EditAvatarImagePageOnClick;
                    FloatingActionButtonView.Click += AddPostOnClick;
                    MessageButton.Click += MessageButtonOnClick;
                    RatingLiner.Click += RatingLinerOnClick;
                    MembersLiner.Click += MembersLinerOnClick;
                }
                else
                {
                    BtnLike.Click -= BtnLikeOnClick;
                    BtnMore.Click -= BtnMoreOnClick;
                    TxtEditPageInfo.Click -= TxtEditPageInfoOnClick;
                    IconBack.Click -= IconBackOnClick;
                    EditAvatarImagePage.Click -= EditAvatarImagePageOnClick;
                    FloatingActionButtonView.Click -= AddPostOnClick;
                    MessageButton.Click -= MessageButtonOnClick;
                    RatingLiner.Click -= RatingLinerOnClick;
                    MembersLiner.Click -= MembersLinerOnClick;
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

                ProfileImage = null;
                CoverImage = null;
                IconBack = null;
                TxtPageName = null;
                TxtPageUsername = null;
                CategoryText = null;
                IconCategory = null;
                IconEdit = null;
                IconLike = null;
                LikeCountText = null;
                IconMembers = null;
                AboutDesc = null;
                BtnLike = null;
                ActionButton = null;
                BtnMore = null;
                MessageButton = null;
                FloatingActionButtonView = null;
                TxtEditPageInfo = null;
                MainRecyclerView = null;
                PostFeedAdapter = null;
                PageId = null;
                UserId = null;
                ImageType = null;
                PageData = null;
                RatingBarView = null;
                EditAvatarImagePage = null;
                RatingLiner = null;
                MembersLiner = null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        #endregion

        #region Events

        // Rating Page 
        private void RatingLinerOnClick(object sender, EventArgs e)
        {
            try
            {
                if (PageData.IsRating == "true")
                {
                    // You have already rated this page!
                    Toast.MakeText(this, GetString(Resource.String.Lbl_YouHaveAlReadyRatedThisPage), ToastLength.Short).Show(); 
                }
                else
                {

                    DialogRatingBarFragment dialog = new DialogRatingBarFragment(this,PageId,PageData);
                    dialog.Show(SupportFragmentManager, dialog.Tag);
                    dialog.OnUpComplete += DialogOnOnUpComplete;
                } 
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void DialogOnOnUpComplete(object sender, DialogRatingBarFragment.RatingBarUpEventArgs e)
        {
            try
            {
                var th = new Thread(ActLikeARequest);
                th.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void ActLikeARequest()
        {
            var x = Resource.Animation.slide_right;
            Console.WriteLine(x);
        }

        //Event Add New post  
        private void AddPostOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(AddPostActivity));
                intent.PutExtra("Type", "SocialPage");
                intent.PutExtra("PostId", PageId);
                intent.PutExtra("itemObject", JsonConvert.SerializeObject(PageData));
                StartActivityForResult(intent, 2500);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Event Update Image Cover Page
        private void EditAvatarImagePageOnClick(object sender, EventArgs e)
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

        private void IconBackOnClick(object sender, EventArgs e)
        {
            Finish();
        }

        private void TxtEditPageInfoOnClick(object sender, EventArgs e)
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

        //Event Show More : Copy Link , Share , Edit (If user isOwner_Pages)
        private void BtnMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                arrayAdapter.Add(GetString(Resource.String.Lbl_CopeLink));
                arrayAdapter.Add(GetString(Resource.String.Lbl_Share));
                arrayAdapter.Add(GetString(Resource.String.Lbl_Reviews));
                if (PageData.IsPageOnwer != null && PageData.IsPageOnwer.Value)
                {
                    switch (PageData?.Boosted)
                    {
                        case "0":
                            arrayAdapter.Add(GetString(Resource.String.Lbl_BoostPage));
                            break;
                        case "1":
                            arrayAdapter.Add(GetString(Resource.String.Lbl_UnBoostPage));
                            break;
                    }
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

        //Event Like => like , dislike 
        private async void BtnLikeOnClick(object sender, EventArgs e)
        {
            try
            {
                if (BtnLike.Tag.ToString() == "MyPage")
                {
                    SettingsPage_OnClick();
                }
                else
                {
                    if (!Methods.CheckConnectivity())
                    {
                        Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                    }
                    else
                    {
                        string isLiked = BtnLike.Text == GetText(Resource.String.Btn_Liked) ? "false" : "true";
                        BtnLike.BackgroundTintList = isLiked == "yes" || isLiked == "true" ? ColorStateList.ValueOf(Color.ParseColor("#efefef")) : ColorStateList.ValueOf(Color.ParseColor(AppSettings.MainColor));
                        BtnLike.Text = GetText(isLiked == "yes" || isLiked == "true" ? Resource.String.Btn_Liked : Resource.String.Btn_Like);
                        BtnLike.SetTextColor(isLiked == "yes" || isLiked == "true" ? Color.Black : Color.White);

                        var (apiStatus, respond) = await RequestsAsync.Page.Like_Page(PageId);
                        if (apiStatus == 200)
                        {
                            if (respond is LikePageObject result)
                            {
                                isLiked = result.LikeStatus == "unliked" ? "false" : "true";
                                BtnLike.BackgroundTintList = isLiked == "yes" || isLiked == "true" ? ColorStateList.ValueOf(Color.ParseColor("#efefef")) : ColorStateList.ValueOf(Color.ParseColor(AppSettings.MainColor));
                                BtnLike.Text = GetText(isLiked == "yes" || isLiked == "true" ? Resource.String.Btn_Liked : Resource.String.Btn_Like);
                                BtnLike.SetTextColor(isLiked == "yes" || isLiked == "true" ? Color.Black : Color.White);
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
         
        //Event Send Message to page  
        private void MessageButtonOnClick(object sender, EventArgs e)
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
                            Methods.App.OpenAppByPackageName(this, AppSettings.MessengerPackageName , "OpenChatPage" , new ChatObject(){UserId = UserId , PageId = PageId , PageName = PageData.PageName , Avatar = PageData.Avatar});
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
                else
                {
                    if (!Methods.CheckConnectivity())
                    {
                        Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                        return;
                    }

                    var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                    dialog.Title(GetString(Resource.String.Lbl_SendMessageTo) + " " + Methods.FunString.DecodeString(PageData.Name));
                    dialog.Input(Resource.String.Lbl_WriteMessage, 0, false, this);
                    dialog.InputType(InputTypes.TextFlagImeMultiLine);
                    dialog.PositiveText(GetText(Resource.String.Btn_Send)).OnPositive(this);
                    dialog.NegativeText(GetText(Resource.String.Lbl_Cancel)).OnNegative(this);
                    dialog.Build().Show();
                    dialog.AlwaysCallSingleChoiceCallback();
                }
                
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Event Invite friends to like this Page
        private void MembersLinerOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(InviteMembersPageActivity));
                intent.PutExtra("PageId", PageId);
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
                                    UpdateImagePage_Api(ImageType, pathImg);
                                }
                                else if (ImageType == "Avatar")
                                {
                                    pathImg = resultUri.Path;
                                    UpdateImagePage_Api(ImageType, pathImg);
                                }
                            }
                            else
                            {
                                Toast.MakeText(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long).Show();
                            }
                        }
                        else
                        {
                            Toast.MakeText(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long).Show();
                        }
                    } 
                } 
                else if (requestCode == 2500 && resultCode == Result.Ok) //Edit post
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
                    string result = data.GetStringExtra("pageItem"); 
                    var item = JsonConvert.DeserializeObject<PageClass>(result);
                    if (item != null)
                        LoadPassedData(item); 
                }
                else if (requestCode == 2019 && resultCode == Result.Ok)
                {
                    var manged = PagesActivity.GetInstance().MAdapter.SocialList.FirstOrDefault(a => a.TypeView == SocialModelType.MangedPages);
                    var dataListGroup = manged?.PagesModelClass.PagesList?.FirstOrDefault(a => a.PageId == PageId);
                    if (dataListGroup != null)
                    {
                        manged.PagesModelClass.PagesList.Remove(dataListGroup);
                        PagesActivity.GetInstance().MAdapter.NotifyDataSetChanged();

                        ListUtils.MyPageList.Remove(dataListGroup);

                        Finish();
                    } 
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

        // Function Update Image Page : Avatar && Cover
        private async void UpdateImagePage_Api(string type, string path)
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
                        var (apiStatus, respond) = await RequestsAsync.Page.Update_Page_Avatar(PageId, path);
                        if (apiStatus == 200)
                        {
                            if (respond is MessageObject result)
                            {
                                Toast.MakeText(this, result.Message, ToastLength.Short).Show();

                                //Set image
                                File file2 = new File(path);
                                var photoUri = FileProvider.GetUriForFile(this, PackageName + ".fileprovider", file2);
                                Glide.With(this).Load(photoUri).Apply(new RequestOptions()).Into(ProfileImage);


                                //GlideImageLoader.LoadImage(this, file.Path, ProfileImage, ImageStyle.RoundedCrop, ImagePlaceholders.Color);
                            }
                        }
                        else Methods.DisplayReportResult(this, respond);
                    }
                    else if (type == "Cover")
                    {
                        var (apiStatus, respond) = await RequestsAsync.Page.Update_Page_Cover(PageId, path);
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

        #region Get Data Page
         
        private void GetDataPage()
        {
            try
            { 
                PageData = JsonConvert.DeserializeObject<PageClass>(Intent.GetStringExtra("PageObject"));
                if (PageData != null)
                {
                    LoadPassedData(PageData); 
                } 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            StartApiService();
        }

        private void LoadPassedData(PageClass pageData)
        {
            try
            {
                UserId = PageData.IsPageOnwer != null && PageData.IsPageOnwer.Value ? UserDetails.UserId : PageData.UserId;

                //Extra  
                LikeCountText.Text = pageData.LikesCount;
               
                GlideImageLoader.LoadImage(this, pageData.Avatar, ProfileImage, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                Glide.With(this).Load(pageData.Cover.Replace(" ", "")).Apply(new RequestOptions().Placeholder(Resource.Drawable.Cover_image).Error(Resource.Drawable.Cover_image)).Into(CoverImage);

                if (PageData.IsPageOnwer != null && PageData.IsPageOnwer.Value)
                {
                    BtnLike.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(AppSettings.MainColor));
                    BtnLike.Text = GetText(Resource.String.Lbl_Edit);
                    BtnLike.SetTextColor(Color.White);
                    BtnLike.Tag = "MyPage";
                    BtnMore.BackgroundTintList = ColorStateList.ValueOf(Color.ParseColor(AppSettings.MainColor));
                    BtnMore.ImageTintList = ColorStateList.ValueOf(Color.White); 
                }
                else
                {
                    BtnLike.BackgroundTintList = pageData.IsLiked == "yes" || pageData.IsLiked == "true" ? ColorStateList.ValueOf(Color.ParseColor("#efefef")) : ColorStateList.ValueOf(Color.ParseColor(AppSettings.MainColor));
                    BtnLike.Text = GetText(pageData.IsLiked == "yes" || pageData.IsLiked == "true" ? Resource.String.Btn_Liked : Resource.String.Btn_Like);
                    BtnLike.SetTextColor(pageData.IsLiked == "yes" || pageData.IsLiked == "true" ? Color.Black : Color.White);
                    BtnMore.BackgroundTintList = pageData.IsLiked == "yes" || pageData.IsLiked == "true" ? ColorStateList.ValueOf(Color.ParseColor("#efefef")) : ColorStateList.ValueOf(Color.ParseColor(AppSettings.MainColor));
                    BtnMore.ImageTintList = pageData.IsLiked == "yes" || pageData.IsLiked == "true" ? ColorStateList.ValueOf(Color.Black) : ColorStateList.ValueOf(Color.White);
                    BtnLike.Tag = "UserPage"; 
                }

                if (PageData.IsPageOnwer != null && PageData.IsPageOnwer.Value)
                {
                    EditAvatarImagePage.Visibility = ViewStates.Visible;
                    TxtEditPageInfo.Visibility = ViewStates.Visible;
                    FloatingActionButtonView.Visibility = ViewStates.Visible;
                    MessageButton.Visibility = ViewStates.Gone;
                    RatingLiner.Visibility = ViewStates.Gone;
                }
                else
                {
                    EditAvatarImagePage.Visibility = ViewStates.Gone;
                    TxtEditPageInfo.Visibility = ViewStates.Gone;
                    FloatingActionButtonView.Visibility = ViewStates.Gone;
                    MessageButton.Visibility = ViewStates.Visible;
                    RatingLiner.Visibility = ViewStates.Visible;
                }

                if (PageData.IsPageOnwer != null && PageData.IsPageOnwer.Value || PageData.UsersPost == "1")
                {
                    var checkSection = PostFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.AddPostBox);
                    if (checkSection == null)
                    { 
                        Combiner.AddPostBoxPostView("Page", -1, new PostDataObject() { PageId = PageData.PageId, Publisher = new PublisherPost() { PageName = PageData.PageName, Avatar = pageData.Avatar } });

                        if (AppSettings.ShowSearchForPosts)
                            Combiner.SearchForPostsView("Page", new PostDataObject() { PageId = PageData.PageId, Publisher = new PublisherPost() { PageName = PageData.PageName, Avatar = pageData.Avatar } });

                        PostFeedAdapter.NotifyItemInserted(PostFeedAdapter.ListDiffer.Count -1 );
                    }
                }
                 
                TxtPageUsername.Text = "@" + pageData.Username;
                TxtPageName.Text = Methods.FunString.DecodeString(pageData.Name);

                CategoriesController cat = new CategoriesController();
                CategoryText.Text = cat.Get_Translate_Categories_Communities(pageData.PageCategory, pageData.Category);

                var readMoreOption = new StReadMoreOption.Builder()
                    .TextLength(200, StReadMoreOption.TypeCharacter)
                    .MoreLabel(GetText(Resource.String.Lbl_ReadMore))
                    .LessLabel(GetText(Resource.String.Lbl_ReadLess))
                    .MoreLabelColor(Color.ParseColor(AppSettings.MainColor))
                    .LessLabelColor(Color.ParseColor(AppSettings.MainColor))
                    .LabelUnderLine(true)
                    .Build();

                if (Methods.FunString.StringNullRemover(pageData.About) != "Empty")
                {
                    var about = Methods.FunString.DecodeString(pageData.About);
                    readMoreOption.AddReadMoreTo(AboutDesc, new String(about));
                }
                else
                {
                    AboutDesc.Text = GetText(Resource.String.Lbl_NoAnyDescription);
                }
                 
                RatingBarView.Rating = Float.ParseFloat(pageData.Rating);
                SetCallActionsButtons(pageData);
                SetAdminInfo(pageData); 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void SetCallActionsButtons(PageClass pageData)
        {
            try
            {

                if (pageData.CallActionType != "0" && !string.IsNullOrEmpty(pageData.CallActionTypeUrl))
                {
                    var name = "Lbl_call_action_" + pageData.CallActionType;
                    var resourceId = Resources.GetIdentifier(name, "string", ApplicationInfo.PackageName);
                    ActionButton.Visibility = ViewStates.Visible;
                    ActionButton.Text = Resources.GetString(resourceId);
                    if (!ActionButton.HasOnClickListeners)
                    {
                        ActionButton.Click += (sender, args) =>
                        {
                            try
                            {
                                if(!string.IsNullOrEmpty(pageData.CallActionTypeUrl))
                                    Methods.App.OpenbrowserUrl(this, pageData.CallActionTypeUrl);
                                else
                                    Toast.MakeText(this,GetString(Resource.String.Lbl_call_action_sorry),ToastLength.Short).Show();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }
                        };
                    } 
                }
                else
                {
                    ReplaceView(ActionButton, BtnLike);
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        
        private void SetAdminInfo(PageClass pageData)
        {
            try
            {  
                if (pageData.AdminInfo?.AdminInfoClass != null && pageData.AdminInfo?.AdminInfoClass?.UserId == UserDetails.UserId)
                { 
                    if (pageData.AdminInfo?.AdminInfoClass.Avatar == "0")
                    {
                        TxtEditPageInfo.Visibility = ViewStates.Gone;
                        EditAvatarImagePage.Visibility = ViewStates.Gone;
                    } 
                } 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void ReplaceView(View currentView, View newView)
        {
            ViewGroup parent = GetParent(currentView);
            if (parent == null)
            {
                return;
            }
            int index = parent.IndexOfChild(currentView);
            RemoveView(currentView);
            RemoveView(newView);
            parent.AddView(newView, index);
        }

        private ViewGroup GetParent(View view)
        {
            return (ViewGroup)view.Parent;
        }

        private void RemoveView(View view)
        {
            ViewGroup parent = GetParent(view);
            parent?.RemoveView(view);
        }

        private void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { GetPageDataApi, () => MainRecyclerView.FetchNewsFeedApiPosts() });
        }

        private async Task GetPageDataApi()
        {
            var (apiStatus, respond) = await RequestsAsync.Page.Get_Page_Data(PageId);

            if (apiStatus != 200 || !(respond is GetPageDataObject result) || result.PageData == null)
                Methods.DisplayReportResult(this, respond);
            else
            {
                PageData = result.PageData;
                LoadPassedData(PageData);
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
                    SettingsPage_OnClick();
                }
                else if (text == GetString(Resource.String.Lbl_BoostPage) || text == GetString(Resource.String.Lbl_UnBoostPage))
                {
                    BoostPageEvent();
                }
                else if (text == GetString(Resource.String.Lbl_Reviews))
                {
                    ReviewsEvent();
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
         
        public void OnInput(MaterialDialog p0, ICharSequence p1)
        {
            //Send Message to page 
            try
            {
                if (p1.Length() > 0)
                {
                    var strName = p1.ToString();

                    if (!Methods.CheckConnectivity())
                    {
                        Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection),ToastLength.Short).Show();
                        return;
                    }

                    if (!string.IsNullOrEmpty(strName) || !string.IsNullOrWhiteSpace(strName))
                    {
                        var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                        var time = unixTimestamp.ToString();

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.PageChat.Send_MessageToPageChat(PageId, UserId, time, strName) });
                        Toast.MakeText(this, GetString(Resource.String.Lbl_MessageSentSuccessfully), ToastLength.Short).Show();
                    }
                    else
                    {
                        Toast.MakeText(this, GetString(Resource.String.Lbl_something_went_wrong), ToastLength.Short).Show();
                    }
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

                var clipData = ClipData.NewPlainText("text", PageData.Url);
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
                    Title = PageData.PageTitle,
                    Text = PageData.About,
                    Url = PageData.Url
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Event Menu >> Settings
        private void SettingsPage_OnClick()
        {
            try
            {
                var intent = new Intent(this, typeof(SettingsPageActivity));
                intent.PutExtra("PageData", JsonConvert.SerializeObject(PageData));
                intent.PutExtra("PagesId", PageId);
                StartActivity(intent);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
         
        private async void BoostPageEvent()
        {
            try
            {  
                var dataUser = ListUtils.MyProfileList.FirstOrDefault();
                if (dataUser?.IsPro != "1" && ListUtils.SettingsSiteList?.Pro == "1" && AppSettings.ShowGoPro)
                {
                    var intent = new Intent(this, typeof(GoProActivity));
                    StartActivity(intent);
                    return;
                }

                if (Methods.CheckConnectivity())
                {
                    PageData.Boosted = "1";
                    //Sent Api 
                    var (apiStatus, respond) = await RequestsAsync.Page.BoostPage(PageId);
                    if (apiStatus == 200)
                    {
                        if (respond is MessageObject actionsObject)
                        {
                            if (actionsObject.Message == "boosted")
                            {
                                PageData.Boosted = "1";
                                Toast.MakeText(this, GetText(Resource.String.Lbl_PageSuccessfullyBoosted), ToastLength.Short).Show();
                            }
                            else
                            {
                                PageData.Boosted = "0";
                            } 
                        }
                    }
                    else Methods.DisplayReportResult(this, respond); 
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        
        private void ReviewsEvent()
        {
            try
            {
                var intent = new Intent(this, typeof(ReviewsPageActivity));
                intent.PutExtra("PageId", PageId);
                StartActivity(intent); 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion
         
    }
}