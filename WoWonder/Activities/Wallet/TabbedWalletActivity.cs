using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using IAmMert.ReadableBottomBarLib;
using Newtonsoft.Json;
using WoWonder.Activities.Wallet.Fragment;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;
using WoWonderClient.Requests;
using Xamarin.PayPal.Android;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace WoWonder.Activities.Wallet
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class TabbedWalletActivity : AppCompatActivity, ReadableBottomBar.IItemSelectListener
    { 
        #region Variables Basic

        private ReadableBottomBar BottomBar;
        private FragmentBottomNavigationView FragmentBottomNavigator;
        public SendMoneyFragment SendMoneyFragment;
        public AddFundsFragment AddFundsFragment;
        public string TypeOpenPayment = "";
        private static TabbedWalletActivity Instance;

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
                SetContentView(Resource.Layout.TabbedWalletLayout);

                Instance = this;

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();

                SetupBottomNavigationView();
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
                BottomBar = FindViewById<ReadableBottomBar>(Resource.Id.readableBottomBar);
                BottomBar.SetOnItemSelectListener(this); 
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
                    toolbar.Title = GetString(Resource.String.Lbl_WalletCredits);
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

        public static TabbedWalletActivity GetInstance()
        {
            try
            {
                return Instance;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
        private void DestroyBasic()
        {
            try
            {
                BottomBar = null;
                FragmentBottomNavigator = null;
                SendMoneyFragment = null;
                AddFundsFragment = null;
                TypeOpenPayment = null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        #endregion

        #region Set Navigation And Show Fragment

        private void SetupBottomNavigationView()
        {
            try
            {
                FragmentBottomNavigator = new FragmentBottomNavigationView(this);

                SendMoneyFragment = new SendMoneyFragment();
                AddFundsFragment = new AddFundsFragment();

                FragmentBottomNavigator.FragmentListTab0.Add(SendMoneyFragment);
                FragmentBottomNavigator.FragmentListTab1.Add(AddFundsFragment);

                FragmentBottomNavigator.NavigationTabBarOnStartTabSelected(0);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion
         
        #region Permissions && Result
       
        //Result 
        protected override async void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                if (TypeOpenPayment == "SendMoneyFragment")
                {
                    if (requestCode == SendMoneyFragment?.InitPayPalPayment?.PayPalDataRequestCode)
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
                                    if (Methods.CheckConnectivity())
                                    {
                                        (int apiStatus, var respond) = await RequestsAsync.Global.SendMoneyWalletAsync(SendMoneyFragment?.UserId, SendMoneyFragment?.TxtAmount.Text).ConfigureAwait(false);
                                        if (apiStatus == 200)
                                        {
                                            RunOnUiThread(() =>
                                            {
                                                try
                                                {
                                                    SendMoneyFragment.TxtAmount.Text = string.Empty;
                                                    SendMoneyFragment.TxtEmail.Text = string.Empty;

                                                    Toast.MakeText(this, GetText(Resource.String.Lbl_Done), ToastLength.Long).Show();
                                                }
                                                catch (Exception e)
                                                {
                                                    Console.WriteLine(e);
                                                }
                                            });
                                        }
                                        else Methods.DisplayReportResult(this, respond);
                                    }
                                    else
                                    {
                                        Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long).Show();
                                    }
                                }
                                break;
                            case (int)Result.Canceled:
                                Toast.MakeText(this, GetText(Resource.String.Lbl_Canceled), ToastLength.Long).Show();
                                break;
                        }
                    }
                    else if (requestCode == PaymentActivity.ResultExtrasInvalid)
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Invalid), ToastLength.Long).Show();
                    }
                }
                else if (TypeOpenPayment == "AddFundsFragment")
                {
                    if (requestCode == AddFundsFragment?.InitPayPalPayment?.PayPalDataRequestCode)
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
                                    if (Methods.CheckConnectivity())
                                    {
                                        (int apiStatus, var respond) = await RequestsAsync.Global.TopUpWalletAsync(UserDetails.UserId, AddFundsFragment?.TxtAmount.Text).ConfigureAwait(false);
                                        if (apiStatus == 200)
                                        {
                                            RunOnUiThread(() =>
                                            {
                                                try
                                                {
                                                    AddFundsFragment.TxtAmount.Text = string.Empty;

                                                    Toast.MakeText(this, GetText(Resource.String.Lbl_Done), ToastLength.Long).Show();
                                                }
                                                catch (Exception e)
                                                {
                                                    Console.WriteLine(e);
                                                }
                                            });
                                        }
                                        else Methods.DisplayReportResult(this, respond);
                                    }
                                    else
                                    {
                                        Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long).Show();
                                    }
                                }

                                break;
                            case (int)Result.Canceled:
                                Toast.MakeText(this, GetText(Resource.String.Lbl_Canceled), ToastLength.Long).Show();
                                break;
                        }
                    }
                    else if (requestCode == PaymentActivity.ResultExtrasInvalid)
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Invalid), ToastLength.Long).Show();
                    }
                }
                else if (requestCode == 1202 && resultCode ==  Result.Ok)
                {
                    var userObject = data.GetStringExtra("DataUser");
                    if (!string.IsNullOrEmpty(userObject))
                    {
                        var userData = JsonConvert.DeserializeObject<UserDataObject>(userObject);
                        if (userData != null)
                        {
                           SendMoneyFragment.TxtEmail.Text = WoWonderTools.GetNameFinal(userData);
                           SendMoneyFragment.UserId = userData.UserId;
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

        public void OnItemSelected(int index)
        {
            try
            {
                FragmentBottomNavigator.NavigationTabBarOnStartTabSelected(index);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}