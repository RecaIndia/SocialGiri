﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using AFollestad.MaterialDialogs;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Ads;
using Android.Gms.Ads.DoubleClick;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using Com.Theartofdev.Edmodo.Cropper;
using Java.Lang;
using Newtonsoft.Json;
using WoWonder.Activities.AddPost.Adapters;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Classes.Product;
using WoWonderClient.Requests;
using Exception = System.Exception;
using File = Java.IO.File;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Uri = Android.Net.Uri;

namespace WoWonder.Activities.Market
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class EditProductActivity : AppCompatActivity, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region Variables Basic

        private TextView TxtAdd, IconTitle, IconPrice, IconLocation, IconCategories, IconAbout, IconType;
        private EditText TxtTitle, TxtPrice, TxtCurrency, TxtLocation, TxtAbout, TxtCategory;
        private RadioButton RbNew, RbUsed;
        private string CategoryId = "", CurrencyId = "", ProductType = "" , PlaceText = "" , TypeDialog = "", DeletedImagesIds = ""; 
        private PublisherAdView PublisherAdView;
        private ProductDataObject ProductData; 
        private AttachmentsAdapter MAdapter;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;

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
                SetContentView(Resource.Layout.CreateProduct_Layout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();
                  
                Get_Data_Product();
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
                PublisherAdView?.Resume();
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
                PublisherAdView?.Pause();
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
                TxtAdd = FindViewById<TextView>(Resource.Id.toolbar_title);
                TxtAdd.Text = GetText(Resource.String.Lbl_Save);

                IconTitle = FindViewById<TextView>(Resource.Id.IconTitle);
                TxtTitle = FindViewById<EditText>(Resource.Id.TitleEditText);
                IconPrice = FindViewById<TextView>(Resource.Id.IconPrice);
                TxtPrice = FindViewById<EditText>(Resource.Id.PriceEditText);
                TxtCurrency = FindViewById<EditText>(Resource.Id.CurrencyEditText);
                IconLocation = FindViewById<TextView>(Resource.Id.IconLocation);
                TxtLocation = FindViewById<EditText>(Resource.Id.LocationEditText);
                IconCategories = FindViewById<TextView>(Resource.Id.IconCategories);
                TxtCategory = FindViewById<EditText>(Resource.Id.CategoriesEditText);
                IconAbout = FindViewById<TextView>(Resource.Id.IconAbout); 
                TxtAbout = FindViewById<EditText>(Resource.Id.AboutEditText);
                IconType = FindViewById<TextView>(Resource.Id.IconType);
                RbNew = FindViewById<RadioButton>(Resource.Id.radioNew);
                RbUsed = FindViewById<RadioButton>(Resource.Id.radioUsed);

                MRecycler = (RecyclerView)FindViewById(Resource.Id.imageRecyler);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconTitle, FontAwesomeIcon.User);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconLocation, FontAwesomeIcon.MapMarkedAlt);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconPrice, FontAwesomeIcon.MoneyBillAlt);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconAbout, FontAwesomeIcon.Paragraph);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeBrands, IconCategories, FontAwesomeIcon.Buromobelexperte);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, IconType, FontAwesomeIcon.LayerPlus);

                Methods.SetColorEditText(TxtTitle, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtPrice, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtCurrency, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtLocation, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtCategory, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtAbout, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                Methods.SetFocusable(TxtCategory);
                Methods.SetFocusable(TxtCurrency);

                PublisherAdView = FindViewById<PublisherAdView>(Resource.Id.multiple_ad_sizes_view); 
                AdsGoogle.InitPublisherAdView(PublisherAdView);
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
                    toolbar.Title = GetText(Resource.String.Lbl_EditProduct);
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
         
        private void SetRecyclerViewAdapters()
        {
            try
            {
                MAdapter = new AttachmentsAdapter(this) { AttachmentList = new ObservableCollection<Attachments>() };
                LayoutManager = new LinearLayoutManager(this, LinearLayoutManager.Horizontal, false);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.SetAdapter(MAdapter);

                MRecycler.Visibility = ViewStates.Visible;

                // Add first image Default 
                var attach = new Attachments
                {
                    Id = MAdapter.AttachmentList.Count + 1,
                    TypeAttachment = "Default",
                    FileSimple = "addImage",
                    FileUrl = "addImage"
                };

                MAdapter.Add(attach);
                MAdapter.NotifyDataSetChanged();
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
                    MAdapter.DeleteItemClick += MAdapterOnDeleteItemClick;
                    MAdapter.ItemClick += MAdapterOnItemClick;
                    RbNew.CheckedChange += RbNewOnCheckedChange;
                    RbUsed.CheckedChange += RbUsedOnCheckedChange;
                    TxtAdd.Click += TxtAddOnClick;
                    TxtLocation.FocusChange += TxtLocationOnFocusChange;
                    TxtCategory.Touch += TxtCategoryOnClick;
                    TxtCurrency.Touch += TxtCurrencyOnTouch;
                }
                else
                {
                    MAdapter.DeleteItemClick -= MAdapterOnDeleteItemClick;
                    MAdapter.ItemClick -= MAdapterOnItemClick;
                    RbNew.CheckedChange -= RbNewOnCheckedChange;
                    RbUsed.CheckedChange -= RbUsedOnCheckedChange;
                    TxtAdd.Click -= TxtAddOnClick;
                    TxtLocation.FocusChange -= TxtLocationOnFocusChange;
                    TxtCategory.Touch -= TxtCategoryOnClick;
                    TxtCurrency.Touch -= TxtCurrencyOnTouch;
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
                PublisherAdView?.Destroy();

                TxtAdd = null;
                IconTitle = null;
                TxtTitle = null;
                IconPrice = null;
                TxtPrice = null;
                TxtCurrency = null;
                IconLocation = null;
                TxtLocation = null;
                IconCategories = null;
                TxtCategory = null;
                IconAbout = null;
                TxtAbout = null;
                IconType = null;
                RbNew = null;
                RbUsed = null;
                MAdapter = null;
                MRecycler = null;
                LayoutManager = null;
                PublisherAdView = null;
                CategoryId = "";
                CurrencyId = "";
                ProductType = "";
                PlaceText = "";
                TypeDialog = "";
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        #endregion

        #region Events

        private void MAdapterOnDeleteItemClick(object sender, AttachmentsAdapterClickEventArgs e)
        {
            try
            {
                var position = e.Position;
                if (position >= 0)
                {
                    var item = MAdapter.GetItem(position);
                    if (item != null)
                    {
                        DeletedImagesIds += item.Id + ",";
                        MAdapter.Remove(item);
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }


        private void MAdapterOnItemClick(object sender, AttachmentsAdapterClickEventArgs e)
        {
            try
            {
                var position = e.Position;
                if (position >= 0)
                {
                    var item = MAdapter.GetItem(position);
                    if (item == null) return;
                    if (item.TypeAttachment != "Default") return;
                    OpenDialogGallery(); //requestCode >> 500 => Image Gallery
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void TxtCurrencyOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e.Event.Action != MotionEventActions.Down) return;

                if (ListUtils.SettingsSiteList?.CurrencySymbolArray.CurrencyList != null)
                {
                    TypeDialog = "Currency";

                    var arrayAdapter = WoWonderTools.GetCurrencySymbolList();
                    if (arrayAdapter?.Count > 0)
                    {
                        var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);
 
                        dialogList.Title(GetText(Resource.String.Lbl_SelectCurrency));
                        dialogList.Items(arrayAdapter);
                        dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(this);
                        dialogList.AlwaysCallSingleChoiceCallback();
                        dialogList.ItemsCallback(this).Build().Show();
                    } 
                }
                else
                {
                    Methods.DisplayReportResult(this, "Not have List Currency Products");
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void TxtCategoryOnClick(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e.Event.Action != MotionEventActions.Down) return;
               
                if (CategoriesController.ListCategoriesProducts.Count > 0)
                {
                    TypeDialog = "Categories";

                    var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                    var arrayAdapter = CategoriesController.ListCategoriesProducts.Select(item => item.CategoriesName).ToList();

                    dialogList.Title(GetText(Resource.String.Lbl_SelectCategories));
                    dialogList.Items(arrayAdapter);
                    dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(this);
                    dialogList.AlwaysCallSingleChoiceCallback();
                    dialogList.ItemsCallback(this).Build().Show();
                }
                else
                {
                    Methods.DisplayReportResult(this, "Not have List Categories Products");
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void TxtLocationOnFocusChange(object sender, View.FocusChangeEventArgs e)
        {
            try
            {
                if (e.HasFocus)
                {
                    // Check if we're running on Android 5.0 or higher
                    if ((int)Build.VERSION.SdkInt < 23)
                    {
                        //Open intent Location when the request code of result is 502
                        new IntentController(this).OpenIntentLocation();
                    }
                    else
                    {
                        if (CheckSelfPermission(Manifest.Permission.AccessFineLocation) == Permission.Granted && CheckSelfPermission(Manifest.Permission.AccessCoarseLocation) == Permission.Granted)
                        {
                            //Open intent Location when the request code of result is 502
                            new IntentController(this).OpenIntentLocation();
                        }
                        else
                        {
                            new PermissionsController(this).RequestPermission(105);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private async void TxtAddOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                }
                else
                {
                    if (string.IsNullOrEmpty(TxtTitle.Text))
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Please_enter_name), ToastLength.Short).Show();
                        return;
                    }

                    if (string.IsNullOrEmpty(TxtPrice.Text))
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Please_enter_price), ToastLength.Short).Show();
                        return;
                    }

                    if (string.IsNullOrEmpty(TxtLocation.Text))
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Please_select_Location), ToastLength.Short).Show();
                        return;
                    }

                    if (string.IsNullOrEmpty(TxtAbout.Text))
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Please_enter_about), ToastLength.Short).Show();
                        return;
                    }

                    if (string.IsNullOrEmpty(TxtCurrency.Text))
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Please_enter_Currency), ToastLength.Short).Show();
                        return;
                    }
                       
                    var list = MAdapter.AttachmentList.Where(a => a.TypeAttachment != "Default").ToList();
                    if (list.Count == 0)
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Please_select_Image), ToastLength.Short).Show();
                    }
                    else
                    {
                        //Show a progress
                        AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading) + "...");

                        if (!string.IsNullOrEmpty(DeletedImagesIds))
                            DeletedImagesIds = DeletedImagesIds.Remove(DeletedImagesIds.Length - 1, 1);
                         
                        var (currency, currencyIcon) = WoWonderTools.GetCurrency(ProductData.Currency);
                        Console.WriteLine(currency);
                        var price = TxtPrice.Text.Replace(currencyIcon, "").Replace(" ", "");
                        var (apiStatus, respond) = await RequestsAsync.Market.Edit_Product(ProductData.Id, TxtTitle.Text, TxtAbout.Text, TxtLocation.Text, price, CurrencyId,
                                                                                           CategoryId, ProductType, list, DeletedImagesIds);
                        if (apiStatus == 200)
                        {
                            if (respond is MessageObject result)
                            {
                                Console.WriteLine(result.Message);
                                var listImage = list.Select(productPathImage => new Images {Id = "", ProductId = ProductData.Id, Image = productPathImage.FileSimple, ImageOrg = productPathImage.FileSimple}).ToList();

                                //Add new item to my Event list
                                var user = ListUtils.MyProfileList?.FirstOrDefault();
                                 
                                if (TabbedMarketActivity.GetInstance()?.MyProductsTab.MAdapter?.MarketList != null)
                                {
                                    var data = TabbedMarketActivity.GetInstance()?.MyProductsTab.MAdapter.MarketList?.FirstOrDefault(a => a.Id == ProductData.Id);
                                    if (data != null)
                                    {
                                        data.Id = ProductData.Id;
                                        data.Name = TxtTitle.Text;
                                        data.UserId = UserDetails.UserId;
                                        data.Location = TxtLocation.Text;
                                        data.Description = TxtAbout.Text;
                                        data.Category = CategoryId;
                                        data.Images = listImage;
                                        data.Price = TxtPrice.Text;
                                        data.Type = ProductType;
                                        data.Seller = user;

                                        TabbedMarketActivity.GetInstance()?.MyProductsTab.MAdapter?.NotifyItemChanged(TabbedMarketActivity.GetInstance().MyProductsTab.MAdapter.MarketList.IndexOf(data));

                                        Intent intent = new Intent();
                                        intent.PutExtra("itemData", JsonConvert.SerializeObject(data));
                                        SetResult(Result.Ok, intent);
                                    }
                                }

                                AndHUD.Shared.ShowSuccess(this, "" , MaskType.Clear, TimeSpan.FromSeconds(2));
                                Toast.MakeText(this, GetString(Resource.String.Lbl_ProductSuccessfullyEdited), ToastLength.Short).Show();

                                Finish(); 
                            }
                        }
                        else
                {
                    Methods.DisplayAndHUDErrorResult(this, respond);
                } 
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                AndHUD.Shared.Dismiss(this);
            }
        }

        private void RbUsedOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                var isChecked = RbUsed.Checked;
                if (isChecked)
                {
                    RbNew.Checked = false;
                    ProductType = "1";
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void RbNewOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                var isChecked = RbNew.Checked;
                if (isChecked)
                {
                    RbUsed.Checked = false;
                    ProductType = "0";
                }
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

                if (requestCode == 502 && resultCode == Result.Ok)
                {
                    GetPlaceFromPicker(data);
                }
                else if (requestCode == CropImage.CropImageActivityRequestCode)
                {
                    var result = CropImage.GetActivityResult(data);

                    if (resultCode == Result.Ok)
                    {
                        if (result.IsSuccessful)
                        {
                            var resultUri = result.Uri;

                            if (!string.IsNullOrEmpty(resultUri.Path))
                            {
                                var productPathImage = resultUri.Path;
                                var attach = new Attachments
                                {
                                    Id = MAdapter.AttachmentList.Count + 1,
                                    TypeAttachment = "postPhotos[]",
                                    FileSimple = productPathImage,
                                    FileUrl = productPathImage
                                };

                                MAdapter.Add(attach);
                            }
                            else
                            {
                                Toast.MakeText(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long).Show();
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
                        OpenDialogGallery();
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denied), ToastLength.Long).Show();
                    }
                }
                else if (requestCode == 105)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        //Open intent Location when the request code of result is 502
                        new IntentController(this).OpenIntentLocation();
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
                if (TypeDialog == "Categories")
                {
                    CategoryId = CategoriesController.ListCategoriesProducts.FirstOrDefault(categories => categories.CategoriesName == itemString.ToString())?.CategoriesId;
                    TxtCategory.Text = itemString.ToString();
                }
                else
                {
                    TxtCurrency.Text = itemString.ToString();
                    CurrencyId = itemId.ToString();
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

        #endregion

        private void GetPlaceFromPicker(Intent data)
        {
            try
            {
                var placeAddress = data.GetStringExtra("Address") ?? "";
                //var placeLatLng = data.GetStringExtra("latLng") ?? "";
                if (!string.IsNullOrEmpty(placeAddress))
                {
                    if (!string.IsNullOrEmpty(PlaceText))
                        PlaceText = string.Empty;

                    PlaceText = placeAddress;
                    TxtLocation.Text = PlaceText;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void OpenDialogGallery()
        {
            try
            {
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

        private void Get_Data_Product()
        {
            try
            {
                ProductData = JsonConvert.DeserializeObject<ProductDataObject>(Intent.GetStringExtra("ProductView"));
                if (ProductData != null)
                {
                    var list = ProductData.Images;
                    foreach (var attach in list.Select(productPathImage => new Attachments
                    {
                        Id = int.Parse(productPathImage.Id),
                        TypeAttachment = "",
                        FileSimple = productPathImage.Image,
                        FileUrl = productPathImage.Image,
                    }))
                    {
                        MAdapter.Add(attach);
                    }
                    TxtTitle.Text = ProductData.Name;

                    var (currency, currencyIcon) = WoWonderTools.GetCurrency(ProductData.Currency);
                    Console.WriteLine(currency);
                    TxtPrice.Text = ProductData.Price;
                    TxtCurrency.Text = currencyIcon;
                    CurrencyId = ProductData.Currency;

                    TxtLocation.Text = ProductData.Location;
                    TxtAbout.Text = ProductData.Description;

                    CategoryId = ProductData.Category; 
                    TxtCategory.Text = CategoriesController.ListCategoriesProducts.FirstOrDefault(a => a.CategoriesId == ProductData.Category)?.CategoriesName; 
                     
                    if (ProductData.Type == "0") // New
                    {
                        RbNew.Checked = true;
                        RbUsed.Checked = false;
                        ProductType = "0";
                    }
                    else // Used
                    {
                        RbNew.Checked = false;
                        RbUsed.Checked = true;
                        ProductType = "1";
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