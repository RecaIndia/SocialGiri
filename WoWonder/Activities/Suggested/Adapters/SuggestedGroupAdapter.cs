using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Android.App;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Bumptech.Glide;
using Java.Util;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;
using WoWonderClient.Requests;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;

namespace WoWonder.Activities.Suggested.Adapters
{
    public class SuggestedGroupAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<SuggestedGroupAdapterClickEventArgs> ItemClick;
        public event EventHandler<SuggestedGroupAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<GroupClass> GroupList = new ObservableCollection<GroupClass>();

        public SuggestedGroupAdapter(Activity context)
        {
            try
            {
                HasStableIds = true;
                ActivityContext = context;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
         
        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_SuggestedGroupView
                View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Style_SuggestedGroupView, parent, false);
                var vh = new SuggestedGroupAdapterViewHolder(itemView, Click, LongClick);
                return vh;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return null;
            }
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                if (viewHolder is SuggestedGroupAdapterViewHolder holder)
                {
                    var item = GroupList[position];
                    if (item != null)
                    {
                        GlideImageLoader.LoadImage(ActivityContext, item.Cover, holder.Image, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

                        holder.Name.Text = Methods.FunString.DecodeString(item.GroupName);
                        holder.CountMembers.Text = Methods.FunString.FormatPriceValue(item.Members) +  " " +ActivityContext.GetString(Resource.String.Lbl_Members);

                        if (item.IsJoined == "true" || item.IsJoined == "yes")
                        {
                            holder.JoinButton.SetBackgroundResource(Resource.Drawable.buttonFlatGray);
                            holder.JoinButton.SetTextColor(Color.White);
                            holder.JoinButton.Text = ActivityContext.GetString(Resource.String.Btn_Joined);
                            holder.JoinButton.Tag = "true";
                        }
                        else
                        {
                            holder.JoinButton.SetBackgroundResource(Resource.Drawable.buttonFlat);
                            holder.JoinButton.SetTextColor(Color.White);
                            holder.JoinButton.Text = ActivityContext.GetString(Resource.String.Btn_Join_Group);
                            holder.JoinButton.Tag = "false";
                        }

                        if (!holder.JoinButton.HasOnClickListeners)
                        {
                            holder.JoinButton.Click += delegate
                            {
                                if (!Methods.CheckConnectivity())
                                {
                                    Toast.MakeText(ActivityContext, ActivityContext.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                                    return;
                                }

                                if (holder.JoinButton.Tag.ToString() == "false")
                                {
                                    holder.JoinButton.SetBackgroundResource(Resource.Drawable.buttonFlatGray);
                                    holder.JoinButton.SetTextColor(Color.White);
                                    holder.JoinButton.Text = ActivityContext.GetString(Resource.String.Btn_Joined);
                                    holder.JoinButton.Tag = "true";
                                }
                                else
                                {
                                    holder.JoinButton.SetBackgroundResource(Resource.Drawable.buttonFlat);
                                    holder.JoinButton.SetTextColor(Color.White);
                                    holder.JoinButton.Text = ActivityContext.GetString(Resource.String.Btn_Join_Group);
                                    holder.JoinButton.Tag = "false";
                                }

                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Group.Join_Group(item.GroupId) });
                            };
                        }
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
                    if (holder is SuggestedGroupAdapterViewHolder viewHolder)
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
        public override int ItemCount => GroupList?.Count ?? 0;

        public GroupClass GetItem(int position)
        {
            return GroupList[position];
        }

        public override long GetItemId(int position)
        {
            try
            {
                return int.Parse(GroupList[position].UserId);
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

        void Click(SuggestedGroupAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void LongClick(SuggestedGroupAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);


        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = GroupList[p0];
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

        public RequestBuilder GetPreloadRequestBuilder(Object p0)
        {
            return GlideImageLoader.GetPreLoadRequestBuilder(ActivityContext, p0.ToString(), ImageStyle.CircleCrop);
        }

    }

    public class SuggestedGroupAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic
         
        public View MainView { get; private set; }
         
        public ImageView Image { get; private set; }

        public TextView Name { get; private set; }
        public TextView CountMembers { get; private set; }
        public Button JoinButton { get; private set; }

        #endregion

        public SuggestedGroupAdapterViewHolder(View itemView, Action<SuggestedGroupAdapterClickEventArgs> clickListener, Action<SuggestedGroupAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Image = MainView.FindViewById<ImageView>(Resource.Id.coverGroup);
                Name = MainView.FindViewById<TextView>(Resource.Id.name);
                CountMembers = MainView.FindViewById<TextView>(Resource.Id.countMembers);
                JoinButton = MainView.FindViewById<Button>(Resource.Id.JoinButton);

                //Event
                itemView.Click += (sender, e) => clickListener(new SuggestedGroupAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                Console.WriteLine(longClickListener);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    public class SuggestedGroupAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }

    public class JoneSuggestionsClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
        public dynamic UserClass { get; set; }
        public Button ButtonFollow { get; set; }

    }
}