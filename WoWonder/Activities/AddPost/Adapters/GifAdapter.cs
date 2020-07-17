﻿using System;
using System.Collections.ObjectModel;
using Android.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using WoWonder.Helpers.Model;

namespace WoWonder.Activities.AddPost.Adapters
{
    public class GifAdapter : RecyclerView.Adapter
    {
        
        private readonly Activity ActivityContext;
        public ObservableCollection<GifGiphyClass.Datum> GifList = new ObservableCollection<GifGiphyClass.Datum>();

        public GifAdapter(Activity context)
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

        public override int ItemCount => GifList?.Count ?? 0;
 
        public event EventHandler<GifAdapterClickEventArgs> ItemClick;
        public event EventHandler<GifAdapterClickEventArgs> ItemLongClick;

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_Gif_View
                var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Style_Gif_View, parent, false);
                var vh = new GifAdapterViewHolder(itemView, Click, LongClick);
                return vh;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return null;
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            { 
                if (viewHolder is GifAdapterViewHolder holder)
                {
                    var item = GifList[position];
                    if (!string.IsNullOrEmpty(item?.Images?.PreviewGif.Url))
                    {
                        Glide.With(ActivityContext).Load(item.Images.FixedHeightDownsampled.Url).Apply(new RequestOptions().Placeholder(Resource.Drawable.ImagePlacholder).Override(AppSettings.ImagePostSize)).Into(holder.Image); 
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
                    if (holder is GifAdapterViewHolder viewHolder)
                        Glide.With(ActivityContext).Clear(viewHolder.Image);
                }
                base.OnViewRecycled(holder);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        public GifGiphyClass.Datum GetItem(int position)
        {
            return GifList[position];
        }

        public override long GetItemId(int position)
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

        private void Click(GifAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(GifAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }
    }

    public class GifAdapterViewHolder : RecyclerView.ViewHolder
    {
        public GifAdapterViewHolder(View itemView, Action<GifAdapterClickEventArgs> clickListener, Action<GifAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;
                Image = (ImageView) MainView.FindViewById(Resource.Id.Image);

                //Create an Event
                itemView.Click += (sender, e) => clickListener(new GifAdapterClickEventArgs {View = itemView, Position = AdapterPosition});
                itemView.LongClick += (sender, e) => longClickListener(new GifAdapterClickEventArgs {View = itemView, Position = AdapterPosition});
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #region Variables Basic

        public View MainView { get; } 
        public ImageView Image { get; }

        #endregion
    }

    public class GifAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}