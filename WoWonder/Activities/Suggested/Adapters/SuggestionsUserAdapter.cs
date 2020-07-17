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
using Refractored.Controls;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Utils;
using WoWonder.SQLite;
using WoWonderClient.Classes.Global;
using WoWonderClient.Requests;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;

namespace WoWonder.Activities.Suggested.Adapters
{ 
    public class SuggestionsUserAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<SuggestionsUserAdapterClickEventArgs> ItemClick;
        public event EventHandler<SuggestionsUserAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<UserDataObject> UserList = new ObservableCollection<UserDataObject>();
        
        public SuggestionsUserAdapter(Activity context)
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

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position, IList<Object> payloads)
        {
            try
            {
                if (payloads.Count > 0)
                {
                    if (viewHolder is SuggestionsUserAdapterViewHolder holder)
                    {
                        var users = UserList[position];

                        var data = (string)payloads[0];
                         
                        if (data == "true")
                        {
                            holder.Button.SetBackgroundResource(Resource.Drawable.buttonFlatGray);
                            holder.Button.SetTextColor(Color.White);
                            if (AppSettings.ConnectivitySystem == 1) // Following 
                            {
                                holder.Button.Text = ActivityContext.GetString(Resource.String.Lbl_Following);
                                holder.Button.Tag = "true";
                                users.IsFollowing = "1";
                            }
                            else
                            {
                                holder.Button.Text = ActivityContext.GetText(Resource.String.Lbl_Request);
                                holder.Button.Tag = "Request";
                                users.IsFollowing = "2";
                            } 
                        }
                        else
                        {
                            holder.Button.SetBackgroundResource(Resource.Drawable.buttonFlat);
                            holder.Button.SetTextColor(Color.White);
                            holder.Button.Text = ActivityContext.GetText(AppSettings.ConnectivitySystem == 1 ? Resource.String.Lbl_Follow : Resource.String.Lbl_AddFriends);
                            holder.Button.Tag = "false";

                            var dbDatabase = new SqLiteDatabase();
                            dbDatabase.Delete_UsersContact(users.UserId);
                            dbDatabase.Dispose();
                        } 
                    }
                }
                else
                {
                    base.OnBindViewHolder(viewHolder, position, payloads);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                base.OnBindViewHolder(viewHolder, position, payloads);
            }
        }


        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_PageCircle_view
                View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Style_Suggestions_view, parent, false);
                var vh = new SuggestionsUserAdapterViewHolder(itemView, Click, LongClick);
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
                if (viewHolder is SuggestionsUserAdapterViewHolder holder)
                {
                    var item = UserList[position];
                    if (item != null)
                    {
                        holder.Username.Text = Methods.FunString.SubStringCutOf("@" + item.Username, 15) ;
                        holder.Name.Text = Methods.FunString.SubStringCutOf(WoWonderTools.GetNameFinal(item),15) ;

                        GlideImageLoader.LoadImage(ActivityContext, item.Avatar, holder.Image, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                         
                        switch (item.IsFollowing)
                        {
                            // My Friend
                            case "1":
                                holder.Button.SetBackgroundResource(Resource.Drawable.buttonFlatGray);
                                holder.Button.SetTextColor(Color.White);
                                holder.Button.Text = ActivityContext.GetText(AppSettings.ConnectivitySystem == 1 ? Resource.String.Lbl_Following : Resource.String.Lbl_Friends);
                                holder.Button.Tag = "true";
                                break;
                            // Request
                            case "2":
                                holder.Button.SetBackgroundResource(Resource.Drawable.buttonFlatGray);
                                holder.Button.SetTextColor(Color.White);
                                holder.Button.Text = ActivityContext.GetText(Resource.String.Lbl_Request);
                                holder.Button.Tag = "Request";
                                break;
                            //Not Friend
                            case "0":
                                holder.Button.SetBackgroundResource(Resource.Drawable.buttonFlat);
                                holder.Button.SetTextColor(Color.White);
                                holder.Button.Text = ActivityContext.GetText(AppSettings.ConnectivitySystem == 1 ? Resource.String.Lbl_Follow : Resource.String.Lbl_AddFriends);
                                holder.Button.Tag = "false";
                                break;
                            default:
                                holder.Button.SetBackgroundResource(Resource.Drawable.buttonFlat);
                                holder.Button.SetTextColor(Color.White);
                                holder.Button.Text = ActivityContext.GetText(AppSettings.ConnectivitySystem == 1 ? Resource.String.Lbl_Follow : Resource.String.Lbl_AddFriends);
                                holder.Button.Tag = "false";
                                break;
                        }
                         
                        if (!holder.Button.HasOnClickListeners)
                            holder.Button.Click += (sender, e) => FollowButtonClick(new FollowSuggestionsClickEventArgs { View = viewHolder.ItemView, UserClass = item, Position = position, ButtonFollow = holder.Button });
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void FollowButtonClick(FollowSuggestionsClickEventArgs e)
        {
            try
            {
                if (e.UserClass != null)
                {
                    if (!Methods.CheckConnectivity())
                    {
                        Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                        return;
                    }
                     
                    NotifyItemChanged(e.Position, e.ButtonFollow.Tag.ToString() == "false" ? "true" : "false");

                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Global.Follow_User(e.UserClass.UserId) });
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
                    if (holder is SuggestionsUserAdapterViewHolder viewHolder)
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

        public override int ItemCount => UserList?.Count ?? 0;
 
        public UserDataObject GetItem(int position)
        {
            return UserList[position];
        }

        public override long GetItemId(int position)
        {
            try
            {
                return int.Parse(UserList[position].UserId);
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

        void Click(SuggestionsUserAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void LongClick(SuggestionsUserAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);


        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = UserList[p0];
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

    public class SuggestionsUserAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic


        public View MainView { get;  set; }

        
        public ImageView Image { get; set; }
        public CircleImageView ImageOnline { get; set; }

        public TextView Name { get; set; }
        public TextView Username { get; set; }
        public Button Button { get; set; }

        #endregion

        public SuggestionsUserAdapterViewHolder(View itemView, Action<SuggestionsUserAdapterClickEventArgs> clickListener,Action<SuggestionsUserAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Image = MainView.FindViewById<ImageView>(Resource.Id.people_profile_sos);
                Name = MainView.FindViewById<TextView>(Resource.Id.name);
                Username = MainView.FindViewById<TextView>(Resource.Id.username);
                Button = MainView.FindViewById<Button>(Resource.Id.FollowButton);
               
                //Event
                itemView.Click += (sender, e) => clickListener(new SuggestionsUserAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new SuggestionsUserAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    public class SuggestionsUserAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }

    public class FollowSuggestionsClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
        public dynamic UserClass { get; set; }
        public Button ButtonFollow { get; set; }

    }
}