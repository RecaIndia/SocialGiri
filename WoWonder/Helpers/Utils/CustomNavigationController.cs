using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Graphics;
using Android.Support.V4.Content;
using Android.Views;
using MeoNavLib.Com;
using WoWonder.Activities.Tabbes;
using WoWonder.Helpers.Ads;
using WoWonder.Helpers.Controller;

namespace WoWonder.Helpers.Utils
{
    public class CustomNavigationController : Java.Lang.Object , MeowBottomNavigation.IClickListener, MeowBottomNavigation.IReselectListener
    {
        private readonly Activity MainContext;
        public int PageNumber;
        private static int OpenNewsFeedTab = 1;

        private readonly TabbedMainActivity Context;
        private readonly MeowBottomNavigation NavigationTabBar;
        private List<MeowBottomNavigation.Model> Models;

        public CustomNavigationController(Activity activity , MeowBottomNavigation bottomNavigation)
        {
            MainContext = activity;
            NavigationTabBar = bottomNavigation;

            if (activity is TabbedMainActivity cont)
                Context = cont;
            
            Initialize();
        }

        private void Initialize()
        {
            try
            {
                Models = new List<MeowBottomNavigation.Model>
                {
                    new MeowBottomNavigation.Model(0, ContextCompat.GetDrawable(MainContext, Resource.Drawable.icon_home_vector)),
                    new MeowBottomNavigation.Model(1, ContextCompat.GetDrawable(MainContext, Resource.Drawable.icon_notification_vector)),
                };

                if (AppSettings.ShowTrendingPage)
                    Models.Add(new MeowBottomNavigation.Model(2, ContextCompat.GetDrawable(MainContext, Resource.Drawable.icon_fire_vector)));

                Models.Add(new MeowBottomNavigation.Model(3, ContextCompat.GetDrawable(MainContext, Resource.Drawable.ic_menu)));
                 
                NavigationTabBar.AddModel(Models);

                NavigationTabBar.SetDefaultIconColor(Color.ParseColor("#bfbfbf"));
                NavigationTabBar.SetSelectedIconColor(Color.ParseColor(AppSettings.MainColor));

                NavigationTabBar.SetBackgroundBottomColor(AppSettings.SetTabDarkTheme ? Color.Black : Color.White);
                NavigationTabBar.SetCircleColor(AppSettings.SetTabDarkTheme ? Color.Black : Color.White);

                NavigationTabBar.SetCountTextColor(Color.White);
                NavigationTabBar.SetCountBackgroundColor(Color.ParseColor(AppSettings.MainColor));

                NavigationTabBar.SetOnClickMenuListener(this);
                NavigationTabBar.SetOnReselectListener(this); 
            } 
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OnClickItem(MeowBottomNavigation.Model item)
        {
            try
            {
                if (!item.GetCount().Equals("0") || !item.GetCount().Equals("empty"))
                {
                    NavigationTabBar.SetCount(item.GetId(), "empty"); 
                }

                PageNumber = item.GetId();
                
                if (PageNumber >= 0)
                {
                    if (PageNumber == 0) // News_Feed_Tab
                    {
                        if (AppSettings.ShowAddPostOnNewsFeed && Context.FloatingActionButton.Visibility == ViewStates.Invisible)
                            Context.FloatingActionButton.Visibility = ViewStates.Visible;

                        AdsGoogle.Ad_Interstitial(MainContext);
                    }
                    else if (PageNumber == 1) // Notifications_Tab
                    {
                        if (Context.FloatingActionButton.Visibility == ViewStates.Visible)
                            Context.FloatingActionButton.Visibility = ViewStates.Gone;

                        Context.RewardedVideoAd = AdsGoogle.Ad_RewardedVideo(MainContext); 
                    }
                    else if (PageNumber == 2 && AppSettings.ShowTrendingPage) // Trending_Tab
                    {
                        if (Context.FloatingActionButton.Visibility == ViewStates.Visible)
                            Context.FloatingActionButton.Visibility = ViewStates.Gone;

                        AdsGoogle.Ad_Interstitial(MainContext);

                        if (AppSettings.ShowLastActivities)
                            Task.Factory.StartNew(() => { Context.TrendingTab.StartApiService(); });
                    }
                    else if (PageNumber == 3) // More_Tab
                    {
                        if (Context.FloatingActionButton.Visibility == ViewStates.Visible)
                            Context.FloatingActionButton.Visibility = ViewStates.Gone;

                        Context.RewardedVideoAd = AdsGoogle.Ad_RewardedVideo(MainContext);
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.Get_MyProfileData_Api(MainContext) });
                    }
                }

                if (Context.ViewPager.CurrentItem != PageNumber)
                    Context.ViewPager.SetCurrentItem(PageNumber, true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OnReselectItem(MeowBottomNavigation.Model item)
        {
            try
            {
                var p = item.GetId();

                if (p < 0) return;

                if (p == 0) // News_Feed_Tab
                {
                    if (OpenNewsFeedTab == 2)
                    {
                        OpenNewsFeedTab = 1;
                        Context.NewsFeedTab.MainRecyclerView.ScrollToPosition(0);
                    }
                    else
                        OpenNewsFeedTab++;
                }
                else if (p == 1) // Notifications_Tab
                {
                    Context.NewsFeedTab?.MainRecyclerView?.ReleasePlayer();
                    OpenNewsFeedTab = 1;
                }
                else if (p == 2 && AppSettings.ShowTrendingPage) // Trending_Tab
                {
                    Context.NewsFeedTab?.MainRecyclerView?.ReleasePlayer();
                    OpenNewsFeedTab = 1;
                }
                else if (p == 3) // More_Tab
                {
                    Context.NewsFeedTab?.MainRecyclerView?.ReleasePlayer();
                    OpenNewsFeedTab = 1;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public void ShowBadge(int id , string count, bool showBadge)
        {
            try
            {
                if (id < 0) return;

                if (showBadge)
                { 
                    if (id == 0) // News_Feed_Tab
                    {
                        NavigationTabBar.SetCount(0, count);
                    }
                    else if (id == 1) // Notifications_Tab
                    {
                        NavigationTabBar.SetCount(1, count);
                    }
                    else if (id == 2) // Trending_Tab
                    {
                        NavigationTabBar.SetCount(2, count);
                    }
                    else if (id == 3) // More_Tab
                    {
                        NavigationTabBar.SetCount(3, count);
                    }
                }
                else
                {
                    if (id == 0) // News_Feed_Tab
                    {
                        NavigationTabBar.SetCount(0, "empty");
                    }
                    else if (id == 1) // Notifications_Tab
                    {
                        NavigationTabBar.SetCount(1, "empty");
                    }
                    else if (id == 2) // Trending_Tab
                    {
                        NavigationTabBar.SetCount(2, "empty");
                    }
                    else if (id == 3) // More_Tab
                    {
                        NavigationTabBar.SetCount(3, "empty");
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