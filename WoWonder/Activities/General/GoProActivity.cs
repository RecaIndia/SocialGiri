using System;
using System.Collections.Generic;
using System.Linq;
using AFollestad.MaterialDialogs;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Java.Lang;
using WoWonder.Activities.General.Adapters;
using WoWonder.Activities.Suggested.User;
using WoWonder.Activities.Tabbes;
using WoWonder.Helpers.Utils;
using WoWonder.Payment;
using WoWonder.PaymentGoogle;
using WoWonder.SQLite;
using WoWonderClient;
using WoWonderClient.Requests;
using Xamarin.PayPal.Android;
using Exception = System.Exception;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace WoWonder.Activities.General
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Keyboard | ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenLayout | ConfigChanges.ScreenSize | ConfigChanges.SmallestScreenSize | ConfigChanges.UiMode | ConfigChanges.Locale)]
    public class GoProActivity : AppCompatActivity, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region Variables Basic

        private RecyclerView MainRecyclerView, MainPlansRecyclerView;
        private GridLayoutManager LayoutManagerView;
        private LinearLayoutManager PlansLayoutManagerView;
        private GoProFeaturesAdapter FeaturesAdapter;
        private UpgradeGoProAdapter PlansAdapter;
        private InitPayPalPayment InitPayPalPayment;
        private InitInAppBillingPayment BillingPayment;
        private ImageView IconClose;
        private string Caller, PayId, Price, PayType;
        private UpgradeGoProClass ItemUpgrade;

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
                SetContentView(Resource.Layout.Go_Pro_Layout);

                Caller = Intent.GetStringExtra("class") ?? "";

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();
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
                if (AppSettings.ShowInAppBilling && Client.IsExtended)
                    BillingPayment?.DisconnectInAppBilling();
                else
                    InitPayPalPayment?.StopPayPalService();

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
                    FinishPage();
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
                MainRecyclerView = FindViewById<RecyclerView>(Resource.Id.recycler);
                MainPlansRecyclerView = FindViewById<RecyclerView>(Resource.Id.recycler2);
                IconClose = FindViewById<ImageView>(Resource.Id.iv1);

                if (AppSettings.ShowInAppBilling && Client.IsExtended)
                    BillingPayment = new InitInAppBillingPayment(this);
                else
                    InitPayPalPayment = new InitPayPalPayment(this);
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
                    toolbar.Title = GetText(Resource.String.Lbl_Go_Pro);
                    toolbar.SetTitleTextColor(Color.White);
                    SetSupportActionBar(toolbar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(false);
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
                FeaturesAdapter = new GoProFeaturesAdapter(this);
                LayoutManagerView = new GridLayoutManager(this, 3);
                MainRecyclerView.SetLayoutManager(LayoutManagerView);
                MainRecyclerView.HasFixedSize = true;
                MainRecyclerView.SetAdapter(FeaturesAdapter);

                PlansAdapter = new UpgradeGoProAdapter(this);
                PlansLayoutManagerView = new LinearLayoutManager(this, LinearLayoutManager.Horizontal, false);
                MainPlansRecyclerView.SetLayoutManager(PlansLayoutManagerView);
                MainPlansRecyclerView.HasFixedSize = true;
                MainPlansRecyclerView.SetAdapter(PlansAdapter);
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
                    PlansAdapter.UpgradeButtonItemClick += PlansAdapterOnItemClick;
                    IconClose.Click += IconCloseOnClick;
                }
                else
                {
                    PlansAdapter.UpgradeButtonItemClick -= PlansAdapterOnItemClick;
                    IconClose.Click -= IconCloseOnClick;
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
                MainRecyclerView = null;
                MainPlansRecyclerView = null;
                LayoutManagerView = null;
                PlansLayoutManagerView = null;
                FeaturesAdapter = null;
                PlansAdapter = null;
                InitPayPalPayment = null;
                BillingPayment = null;
                IconClose = null;
                PayId = null;
                Price = null;
                PayType = null;
                ItemUpgrade = null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        #endregion

        #region Events

        private void PlansAdapterOnItemClick(object sender, UpgradeGoProAdapterClickEventArgs e)
        {
            try
            {
                if (e.Position > -1)
                {
                    ItemUpgrade = PlansAdapter.GetItem(e.Position);
                    if (ItemUpgrade != null)
                    {
                        var arrayAdapter = new List<string>();
                        var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                        if (AppSettings.ShowInAppBilling && Client.IsExtended)
                            arrayAdapter.Add(GetString(Resource.String.Btn_GooglePlay));

                        if (AppSettings.ShowPaypal)
                            arrayAdapter.Add(GetString(Resource.String.Btn_Paypal));

                        if (AppSettings.ShowCreditCard)
                            arrayAdapter.Add(GetString(Resource.String.Lbl_CreditCard));

                        if (AppSettings.ShowBankTransfer)
                            arrayAdapter.Add(GetString(Resource.String.Lbl_BankTransfer));

                        dialogList.Items(arrayAdapter);
                        dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(this);
                        dialogList.AlwaysCallSingleChoiceCallback();
                        dialogList.ItemsCallback(this).Build().Show(); 
                    } 
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Close
        private void IconCloseOnClick(object sender, EventArgs e)
        {
            try
            {
                FinishPage();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        #region Result

        //Result
        protected override async void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);

                if (AppSettings.ShowInAppBilling && Client.IsExtended)
                    BillingPayment?.Handler?.HandleActivityResult(requestCode, resultCode, data);
                 
                if (requestCode == InitPayPalPayment?.PayPalDataRequestCode)
                {
                    switch (resultCode)
                    {
                        case Result.Ok:
                            var confirmObj = data.GetParcelableExtra(PaymentActivity.ExtraResultConfirmation);
                            PaymentConfirmation configuration = Android.Runtime.Extensions.JavaCast<PaymentConfirmation>(confirmObj);
                            if (configuration != null)
                            {
                                //string createTime = configuration.ProofOfPayment.CreateTime;
                                //string intent = configuration.ProofOfPayment.Intent;
                                //string paymentId = configuration.ProofOfPayment.PaymentId;
                                //string state = configuration.ProofOfPayment.State;
                                //string transactionId = configuration.ProofOfPayment.TransactionId;

                                if (PayType == "membership")
                                {
                                    if (Methods.CheckConnectivity())
                                    {
                                        (int apiStatus, var respond) = await RequestsAsync.Global.SetProAsync(PayId).ConfigureAwait(false);
                                        if (apiStatus == 200)
                                        {
                                            RunOnUiThread(() =>
                                            {
                                                var dataUser = ListUtils.MyProfileList.FirstOrDefault();
                                                if (dataUser != null)
                                                {
                                                    dataUser.IsPro = "1";

                                                    var sqlEntity = new SqLiteDatabase();
                                                    sqlEntity.Insert_Or_Update_To_MyProfileTable(dataUser);
                                                    sqlEntity.Dispose();
                                                }

                                                Toast.MakeText(this, GetText(Resource.String.Lbl_Done), ToastLength.Long).Show();
                                                FinishPage();
                                            });
                                        }
                                        else Methods.DisplayReportResult(this, respond);
                                    }
                                    else
                                    {
                                        Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long).Show();
                                    }
                                }
                            }

                            break;
                        case Result.Canceled:
                            Toast.MakeText(this, GetText(Resource.String.Lbl_Canceled), ToastLength.Long).Show();
                            break;
                    }
                }
                else if (requestCode == PaymentActivity.ResultExtrasInvalid)
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_Invalid), ToastLength.Long).Show();
                }
                else if (requestCode == 1001 && resultCode == Result.Ok && AppSettings.ShowInAppBilling && Client.IsExtended)
                {
                    if (Methods.CheckConnectivity())
                    {
                        (int apiStatus, var respond) = await RequestsAsync.Global.SetProAsync(PayId).ConfigureAwait(false);
                        if (apiStatus == 200)
                        {
                            RunOnUiThread(() =>
                            {
                                var dataUser = ListUtils.MyProfileList.FirstOrDefault();
                                if (dataUser != null)
                                {
                                    dataUser.IsPro = "1";

                                    var sqlEntity = new SqLiteDatabase();
                                    sqlEntity.Insert_Or_Update_To_MyProfileTable(dataUser);
                                    sqlEntity.Dispose();
                                }

                                Toast.MakeText(this, GetText(Resource.String.Lbl_Done), ToastLength.Long).Show();
                                FinishPage();
                            });
                        }
                        else Methods.DisplayReportResult(this, respond);
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long).Show();
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
                if (text == GetString(Resource.String.Btn_Paypal))
                {
                    Price = ItemUpgrade.PlanPrice;
                    PayType = "membership";
                    PayId = ItemUpgrade.Id.ToString();
                    InitPayPalPayment.BtnPaypalOnClick(Price);
                }
                else if (text == GetString(Resource.String.Btn_GooglePlay))
                {
                    Price = ItemUpgrade.PlanPrice;
                    PayId = ItemUpgrade.Id.ToString();

                    BillingPayment.SetConnInAppBilling();
                    BillingPayment.InitInAppBilling(Price, "membership", PayId);
                }
                else if (text == GetString(Resource.String.Lbl_CreditCard))
                {
                    OpenIntentCreditCard();
                }
                else if (text == GetString(Resource.String.Lbl_BankTransfer))
                {
                    OpenIntentBankTransfer();
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
        private void OpenIntentCreditCard()
        {
            try
            {
                Intent intent = new Intent(this, typeof(PaymentCardDetailsActivity));
                intent.PutExtra("Id", ItemUpgrade.Id.ToString());
                intent.PutExtra("Price", ItemUpgrade.PlanPrice);
                intent.PutExtra("payType", "membership");
                StartActivity(intent);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void OpenIntentBankTransfer()
        {
            try
            {
                Intent intent = new Intent(this, typeof(PaymentLocalActivity));
                intent.PutExtra("Id", ItemUpgrade.Id.ToString());
                intent.PutExtra("Price", ItemUpgrade.PlanPrice);
                intent.PutExtra("payType", "membership");
                StartActivity(intent);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        private void FinishPage()
        {
            try
            {
                if (Caller == "register")
                {
                    if (AppSettings.ShowSuggestedUsersOnRegister)
                    {
                        Intent newIntent = new Intent(this, typeof(SuggestionsUsersActivity));
                        newIntent.PutExtra("class", "register");
                        StartActivity(newIntent);
                    }
                    else
                    {
                        StartActivity(new Intent(this, typeof(TabbedMainActivity)));
                    }
                }

                Finish();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

    }
}