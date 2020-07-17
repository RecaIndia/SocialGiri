﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Page;
using WoWonderClient.Requests;
using Exception = System.Exception;

namespace WoWonder.Activities.Communities.Pages
{
    public class DialogRatingBarFragment : Android.Support.V4.App.DialogFragment
    {
        #region Variables Basic

        private RatingBar RatingBar;
        private EditText TxtReview;
        private Button BtnSave;
        private TextView TxtRate;
        public event EventHandler<RatingBarUpEventArgs> OnUpComplete;
        private readonly string PageId = "";
        private readonly PageClass Item;
        private readonly PageProfileActivity ActivityContext;

        #endregion
         
        #region General
         
        public DialogRatingBarFragment(PageProfileActivity activity, string pageId, PageClass item)
        {
            try
            {
                ActivityContext = activity;
                PageId = pageId;
                Item = item;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                base.OnCreateView(inflater, container, savedInstanceState);

                // Set our view from the "DialogRatePageLayout" layout resource
               // var view = inflater.Inflate(Resource.Layout.DialogRatePageLayout, container, false);

                Context contextThemeWrapper = AppSettings.SetTabDarkTheme ? new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark_Base) : new ContextThemeWrapper(Activity, Resource.Style.MyTheme);
                // clone the inflater using the ContextThemeWrapper
                LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);

                View view = localInflater.Inflate(Resource.Layout.DialogRatePageLayout, container, false);

                // Get values
                InitComponent(view);

                return view;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public override void OnResume()
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

        public override void OnPause()
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

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            try
            {
                Dialog.Window.RequestFeature(WindowFeatures.NoTitle); //Sets the title bar to invisible
                base.OnActivityCreated(savedInstanceState);
                Dialog.Window.Attributes.WindowAnimations = Resource.Style.dialog_animation; //set the animation
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
                RatingBar = view.FindViewById<RatingBar>(Resource.Id.ratingBar);
                TxtReview = view.FindViewById<EditText>(Resource.Id.ReviewEditText);
                TxtRate = view.FindViewById<TextView>(Resource.Id.rate);

                BtnSave = view.FindViewById<Button>(Resource.Id.ApplyButton);
                 
                RatingBar.NumStars = 5;

                TxtRate.Text = GetString(Resource.String.Lbl_Rate) + " : @" + Item.PageName;

                Methods.SetColorEditText(TxtReview, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
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
                    RatingBar.RatingBarChange += RatingBarOnRatingBarChange;
                    BtnSave.Click += BtnSaveOnClick;
                }
                else
                {
                    RatingBar.RatingBarChange -= RatingBarOnRatingBarChange;
                    BtnSave.Click -= BtnSaveOnClick;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
         
        #endregion

        #region Events

        private void RatingBarOnRatingBarChange(object sender, RatingBar.RatingBarChangeEventArgs e)
        {
            try
            {
                RatingBar.Rating = e.Rating;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
         
        private void BtnSaveOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(ActivityContext, ActivityContext.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                }
                else
                {
                    if (RatingBar.Rating <= 0)
                    {
                        Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_Please_select_Rating), ToastLength.Short).Show(); 
                        return;
                    }

                    if (string.IsNullOrEmpty(TxtReview.Text) || string.IsNullOrWhiteSpace(TxtReview.Text))
                    {
                        Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_Please_enter_review), ToastLength.Short).Show();
                        return;
                    }

                    StartApiService(); 
                } 
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(ActivityContext, ActivityContext.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { RatePageApi });
        }

        private async Task RatePageApi()
        {
            (int apiStatus, var respond) = await RequestsAsync.Page.RatePage(PageId, RatingBar.Rating.ToString(CultureInfo.InvariantCulture), TxtReview.Text);
            if (apiStatus == 200)
            {
                if (respond is RatePageObject result)
                { 
                    ActivityContext.RunOnUiThread(() =>
                    {
                        try
                        {
                            ActivityContext.RatingBarView.Rating = Convert.ToInt32(result.Val);
                            PageProfileActivity.PageData.IsRating = "true";
                            PageProfileActivity.PageData.Rating = result.Val;

                            Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_Done), ToastLength.Short).Show();

                            Dismiss();
                            var x = Resource.Animation.slide_right;
                            Console.WriteLine(x);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    });
                }
            }
            else Methods.DisplayReportResult(ActivityContext, respond);
        }
         
        #endregion

        public abstract class RatingBarUpEventArgs : EventArgs
        {
            public View View { get; set; }
            public int Position { get; set; }
        }

        protected virtual void OnRatingBarUpComplete(RatingBarUpEventArgs e)
        {
            OnUpComplete?.Invoke(this, e);
        }

    }
}