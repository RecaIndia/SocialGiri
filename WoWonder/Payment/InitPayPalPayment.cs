using System;
using Android.App;
using Android.Content;
using Java.Math;
using WoWonder.Helpers.Utils;
using WoWonderClient;
using Xamarin.PayPal.Android;

namespace WoWonder.Payment
{
    public class InitPayPalPayment
    {
        private readonly Activity ActivityContext;
        private static PayPalConfiguration PayPalConfig;
        private PayPalPayment PayPalPayment;
        private Intent IntentService;
        public readonly int PayPalDataRequestCode = 7171;

        public InitPayPalPayment(Activity activity)
        {
            ActivityContext = activity;
        }

        //Paypal
        public void BtnPaypalOnClick(string price)
        {
            try
            {
                var init = InitPayPal(price);
                if (!init)
                    return;

                Intent intent = new Intent(ActivityContext, typeof(PaymentActivity));
                intent.PutExtra(PayPalService.ExtraPaypalConfiguration, PayPalConfig);
                intent.PutExtra(PaymentActivity.ExtraPayment, PayPalPayment);
                ActivityContext.StartActivityForResult(intent, PayPalDataRequestCode);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private bool InitPayPal(string price)
        {
            try
            {
                //PayerID
                string currency = "USD";
                string paypalClintId = "";
                var option = ListUtils.SettingsSiteList;
                if (option != null)
                {
                    currency = option.PaypalCurrency ?? "USD";
                    paypalClintId = option.PaypalId;
                }

                if (string.IsNullOrEmpty(paypalClintId))
                    return false;

                PayPalConfig = new PayPalConfiguration()
                    .ClientId(paypalClintId)
                    .LanguageOrLocale(AppSettings.Lang)
                    .MerchantName(AppSettings.ApplicationName)
                    .MerchantPrivacyPolicyUri(Android.Net.Uri.Parse(Client.WebsiteUrl + "/terms/privacy-policy"));

                switch (ListUtils.SettingsSiteList?.PaypalMode)
                {
                    case "sandbox":
                        PayPalConfig.Environment(PayPalConfiguration.EnvironmentSandbox);
                        break;
                    case "live":
                        PayPalConfig.Environment(PayPalConfiguration.EnvironmentProduction);
                        break;
                    default:
                        PayPalConfig.Environment(PayPalConfiguration.EnvironmentProduction);
                        break;
                }

                PayPalPayment = new PayPalPayment(new BigDecimal(price), currency, "Pay the card", PayPalPayment.PaymentIntentSale);

                IntentService = new Intent(ActivityContext, typeof(PayPalService));
                IntentService.PutExtra(PayPalService.ExtraPaypalConfiguration, PayPalConfig);
                ActivityContext.StartService(IntentService);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public void StopPayPalService()
        {
            try
            {
                if (PayPalConfig != null)
                {
                    ActivityContext.StopService(new Intent(ActivityContext, typeof(PayPalService)));
                    PayPalConfig = null;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}