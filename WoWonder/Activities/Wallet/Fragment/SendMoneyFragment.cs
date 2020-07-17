using System;
using System.Collections.Generic;
using System.Linq;
using AFollestad.MaterialDialogs;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using Java.Lang;
using WoWonder.Activities.Contacts;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Utils;
using WoWonder.Payment;
using Exception = System.Exception;

namespace WoWonder.Activities.Wallet.Fragment
{
    public class SendMoneyFragment : Android.Support.V4.App.Fragment, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region  Variables Basic

        private TextView IconAmount, IconEmail;
        public EditText TxtAmount, TxtEmail;
        private TextView TxtMyBalance;
        private Button BtnContinue;
        public InitPayPalPayment InitPayPalPayment;
        public string Price, UserId;
        private TabbedWalletActivity GlobalContext;

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
            GlobalContext = (TabbedWalletActivity) Activity;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                View view = inflater.Inflate(Resource.Layout.SendMoneyLayout, container, false);

                InitComponent(view);
                AddOrRemoveEvent(true);

                return view;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return null;
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
                TxtMyBalance = view.FindViewById<TextView>(Resource.Id.myBalance);

                IconAmount = view.FindViewById<TextView>(Resource.Id.IconAmount);
                TxtAmount = view.FindViewById<EditText>(Resource.Id.AmountEditText);
                IconEmail = view.FindViewById<TextView>(Resource.Id.IconEmail);
                TxtEmail = view.FindViewById<EditText>(Resource.Id.EmailEditText);
                BtnContinue = view.FindViewById<Button>(Resource.Id.ContinueButton);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeRegular, IconAmount, FontAwesomeIcon.MoneyBillWave);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeRegular, IconEmail, FontAwesomeIcon.At);

                Methods.SetColorEditText(TxtEmail, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtAmount, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                Methods.SetFocusable(TxtEmail);

                InitPayPalPayment = new InitPayPalPayment(Activity);
                 
                var userData = ListUtils.MyProfileList.FirstOrDefault();
                if (userData != null)
                {
                    TxtMyBalance.Text = userData.Wallet;
                }

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
                    BtnContinue.Click += BtnContinueOnClick;
                    TxtEmail.Touch += TxtEmailOnTouch;
                }
                else
                {
                    BtnContinue.Click -= BtnContinueOnClick;
                    TxtEmail.Touch -= TxtEmailOnTouch;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Events
         
        //select user >> Get User id
        private void TxtEmailOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e.Event.Action != MotionEventActions.Down) return;

                Intent intent = new Intent(Activity, typeof(SelectContactsActivity));
                Activity.StartActivityForResult(intent, 1202);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Send Amount
        private void BtnContinueOnClick(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(TxtAmount.Text) || string.IsNullOrWhiteSpace(TxtAmount.Text) || int.Parse(TxtAmount.Text) == 0)
                {
                    Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_Please_enter_amount), ToastLength.Short).Show();
                    return;
                }

                if (string.IsNullOrEmpty(TxtEmail.Text) || string.IsNullOrWhiteSpace(TxtEmail.Text))
                {
                    Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_Please_enter_name_email), ToastLength.Short).Show();
                    return;
                }

                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long).Show();
                    return;
                }

                GlobalContext.TypeOpenPayment = "SendMoneyFragment";
                Price = TxtAmount.Text;

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(Context).Theme(AppSettings.SetTabDarkTheme ? Theme.Dark : Theme.Light);
                
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
            catch (Exception exception)
            {
                Console.WriteLine(exception);
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
                    InitPayPalPayment.BtnPaypalOnClick(Price);
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
                Intent intent = new Intent(Context, typeof(PaymentCardDetailsActivity));
                intent.PutExtra("Id", "");
                intent.PutExtra("Price", Price);
                intent.PutExtra("payType", "SendMoney");
                Context.StartActivity(intent);
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
                Intent intent = new Intent(Context, typeof(PaymentLocalActivity));
                intent.PutExtra("Id", "");
                intent.PutExtra("Price", Price);
                intent.PutExtra("payType", "SendMoney");
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