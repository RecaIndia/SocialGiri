﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Bumptech.Glide;
using Java.Util;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;
using IList = System.Collections.IList;

namespace WoWonder.Activities.UserProfile.Adapters
{
    public class UserPagesAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<UserPagesAdapterClickEventArgs> ItemClick;
        public event EventHandler<UserPagesAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext; 
        public ObservableCollection<PageClass> UserPagesList = new ObservableCollection<PageClass>();

        public UserPagesAdapter(Activity context)
        {
            try
            {
                ActivityContext = context;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override int ItemCount => UserPagesList?.Count ?? 0;
 
        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_PageCircle_view
                var itemView = LayoutInflater.From(parent.Context)
                    .Inflate(Resource.Layout.Style_PageCircle_view, parent, false);
                var vh = new UserPagesAdapterViewHolder(itemView, Click, LongClick);
                return vh;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            { 
                if (viewHolder is UserPagesAdapterViewHolder holder)
                {
                    var item = UserPagesList[position];
                    if (item != null)
                    {
                        GlideImageLoader.LoadImage(ActivityContext, item.Avatar, holder.Image, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                        string name = Methods.FunString.DecodeString(item.PageName);
                        holder.Name.Text = name;
                         
                        if (item.UserId == UserDetails.UserId)
                            item.IsPageOnwer = true;
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
        public override void OnViewRecycled(Java.Lang.Object holder)
        {
            try
            {
                if (holder != null)
                {
                    if (holder is UserPagesAdapterViewHolder viewHolder)
                    {
                        Glide.With(ActivityContext).Clear(viewHolder.Image);
                    }
                }
                base.OnViewRecycled(holder);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        public PageClass GetItem(int position)
        {
            return UserPagesList[position];
        }

        public override long GetItemId(int position)
        {
            try
            {
                return int.Parse(UserPagesList[position].UserId);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return 0;
            }
        }

        public override int GetItemViewType(int position)
        {
            try
            {
                return position;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return 0;
            }
        }

        private void Click(UserPagesAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(UserPagesAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }
         
        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = UserPagesList[p0];
                if (item == null)
                    return d;
                else
                {
                    if (!string.IsNullOrEmpty(item.Avatar))
                        d.Add(item.Avatar);

                    return d;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
               return Collections.SingletonList(p0);
            }
        }

        public RequestBuilder GetPreloadRequestBuilder(Java.Lang.Object p0)
        {
            return GlideImageLoader.GetPreLoadRequestBuilder(ActivityContext, p0.ToString(), ImageStyle.CircleCrop);
        }


    }

    public class UserPagesAdapterViewHolder : RecyclerView.ViewHolder
    {
        public UserPagesAdapterViewHolder(View itemView, Action<UserPagesAdapterClickEventArgs> clickListener,
            Action<UserPagesAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Image = MainView.FindViewById<ImageView>(Resource.Id.Image);
                Name = MainView.FindViewById<TextView>(Resource.Id.Name);
                IconPage = MainView.FindViewById<TextView>(Resource.Id.Icon);

                //Event
                itemView.Click += (sender, e) => clickListener(new UserPagesAdapterClickEventArgs{View = itemView, Position = AdapterPosition});
                itemView.LongClick += (sender, e) => longClickListener(new UserPagesAdapterClickEventArgs{View = itemView, Position = AdapterPosition});

             
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconPage, IonIconsFonts.IosFlag);
                //#####

            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #region Variables Basic

        public View MainView { get; }

        
        public ImageView Image { get; set; }
        public TextView Name { get; set; }
        public TextView IconPage { get; set; }

        #endregion
    }

    public class UserPagesAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}