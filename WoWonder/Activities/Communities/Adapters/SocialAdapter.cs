using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Load.Resource.Bitmap;
using Bumptech.Glide.Request;
using Java.Util;
using Newtonsoft.Json;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Activities.UsersPages;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;
using WoWonderClient.Requests;
using IList = System.Collections.IList;

namespace WoWonder.Activities.Communities.Adapters
{
    public class SocialAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<GroupsAdapterClickEventArgs> GroupItemClick;
        public event EventHandler<GroupsAdapterClickEventArgs> GroupItemLongClick;

        public event EventHandler<PageAdapterClickEventArgs> PageItemClick;
        public event EventHandler<PageAdapterClickEventArgs> PageItemLongClick;
         
        private Activity ActivityContext { get;  set; }
        public ObservableCollection<SocialModelsClass> SocialList { get; private set; }
        private SocialModelType SocialPageType { get; set; }
         
        public SocialAdapter(Activity context, SocialModelType socialModelType)
        {
            try
            {
                HasStableIds = true;
                ActivityContext = context;
                SocialPageType = socialModelType;
                
                SocialList = new ObservableCollection<SocialModelsClass>(); 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override int ItemCount => SocialList?.Count ?? 0;
         
        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            { 
                View itemView; 
                var item = SocialList[viewType]; 
                switch (item.TypeView)
                {
                    case SocialModelType.MangedGroups:
                    {
                        itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.ViewModel_HRecyclerView, parent, false);
                        var vh = new AdapterHolders.GroupsSocialViewHolder(ActivityContext, itemView);
                        return vh;
                    }
                    case SocialModelType.JoinedGroups:
                    {
                        itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Style_GroupCircle_view, parent, false);
                        var vh = new GroupsAdapterViewHolder(itemView, GroupsOnClick);
                        return vh;
                    }
                    case SocialModelType.Section:
                    {
                        itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.ViewModel_Section, parent, false);
                        var vh = new AdapterHolders.SectionViewHolder(itemView);
                        return vh;
                    }
                    case SocialModelType.MangedPages:
                    {
                        itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.ViewModel_HRecyclerView, parent, false);
                        var vh = new AdapterHolders.PagesSocialViewHolder(ActivityContext, itemView);
                        return vh;
                    }
                    case SocialModelType.LikedPages:
                    {
                        itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Style_HPage_view, parent, false);
                        var vh = new PageAdapterViewHolder(itemView, PageOnClick);
                        return vh;
                    }
                    default:
                        return null;
                } 
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
                var item = SocialList[viewHolder.AdapterPosition];
                if (item != null)
                {
                    switch (item.TypeView)
                    {
                        case SocialModelType.MangedGroups:
                        {
                            if (viewHolder is AdapterHolders.GroupsSocialViewHolder holder)
                            {
                                if (holder.GroupsAdapter.GroupList.Count == 0)
                                {
                                    holder.GroupsAdapter.GroupList = new ObservableCollection<GroupClass>(item.MangedGroupsModel.GroupsList);
                                    holder.GroupsAdapter.NotifyDataSetChanged();
                                }
                                else if (item.MangedGroupsModel?.GroupsList != null && holder.GroupsAdapter.GroupList.Count != item.MangedGroupsModel.GroupsList.Count)
                                {
                                    holder.GroupsAdapter.GroupList = new ObservableCollection<GroupClass>(item.MangedGroupsModel.GroupsList);
                                    holder.GroupsAdapter.NotifyDataSetChanged();
                                }

                                if (!string.IsNullOrEmpty(item.MangedGroupsModel?.TitleHead))
                                    holder.AboutHead.Text = item.MangedGroupsModel?.TitleHead;

                                holder.AboutMore.Text = item.MangedGroupsModel?.More;

                                if (holder.GroupsAdapter?.GroupList?.Count >= 5)
                                {
                                    holder.AboutMore.Visibility = ViewStates.Visible;
                                    holder.AboutMoreIcon.Visibility = ViewStates.Visible;
                                }
                                else
                                {
                                    holder.AboutMore.Visibility = ViewStates.Invisible;
                                    holder.AboutMoreIcon.Visibility = ViewStates.Invisible;
                                }

                                if (!holder.AboutMore.HasOnClickListeners)
                                {
                                    holder.AboutMore.Click += (sender, args) => { OpenAllViewer("MangedGroupsModel", item); };
                                    holder.AboutMoreIcon.Click += (sender, args) => { OpenAllViewer("MangedGroupsModel", item); };
                                } 
                            }

                            break;
                        }
                        case SocialModelType.JoinedGroups:
                        {
                            if (viewHolder is GroupsAdapterViewHolder holder)
                            {
                                var options = new RequestOptions();
                                options.Transform(new MultiTransformation(new CenterCrop(), new RoundedCorners(110)));
                                options.Error(Resource.Drawable.ImagePlacholder).Placeholder(Resource.Drawable.ImagePlacholder);

                                GlideImageLoader.LoadImage(ActivityContext, item.GroupData.Avatar, holder.Image, ImageStyle.CenterCrop, ImagePlaceholders.Drawable, false, options);

                                if (!string.IsNullOrEmpty(item.GroupData.GroupTitle) || !string.IsNullOrWhiteSpace(item.GroupData.GroupTitle))
                                    holder.Name.Text = Methods.FunString.DecodeString(item.GroupData.GroupName);
                                else
                                    holder.Name.Text = Methods.FunString.DecodeString(item.GroupData.GroupName);
                             
                                if (item.GroupData.IsJoined == "true" || item.GroupData.IsJoined == "yes")
                                {
                                    holder.JoinButton.Text = ActivityContext.GetString(Resource.String.Btn_Joined);
                                    holder.JoinButton.Tag = "true";
                                }
                                else
                                {
                                    holder.JoinButton.Text = ActivityContext.GetString(Resource.String.Btn_Join_Group);
                                    holder.JoinButton.Tag = "false";
                                }

                                if (!holder.JoinButton.HasOnClickListeners)
                                {
                                    holder.JoinButton.Click += (sender, args) => 
                                    {
                                        try
                                        {
                                            if (!Methods.CheckConnectivity())
                                            {
                                                Toast.MakeText(ActivityContext, ActivityContext.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                                                return;
                                            }

                                            if (holder.JoinButton.Tag.ToString() == "false")
                                            {
                                                holder.JoinButton.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                                                holder.JoinButton.Text = ActivityContext.GetString(Resource.String.Btn_Joined);
                                                holder.JoinButton.Tag = "true";
                                            }
                                            else
                                            {
                                                holder.JoinButton.SetTextColor(Color.Black);
                                                holder.JoinButton.Text = ActivityContext.GetString(Resource.String.Btn_Join_Group);
                                                holder.JoinButton.Tag = "false";
                                            }

                                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Group.Join_Group(item.GroupData.GroupId) });
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine(e);
                                        }
                                    };
                                } 
                            }

                            break;
                        }
                        case SocialModelType.Section:
                        {
                            if (!(viewHolder is AdapterHolders.SectionViewHolder holder))
                                return;

                            holder.AboutHead.Text = item.TitleHead;
                            if(string.IsNullOrEmpty(item.MoreIcon))
                                holder.AboutMoreIcon.Text = item.MoreIcon;
                            break;
                        }
                        case SocialModelType.MangedPages:
                        {
                            if (viewHolder is AdapterHolders.PagesSocialViewHolder holder)
                            {
                                if (holder.PagesAdapter.UserPagesList.Count == 0)
                                {
                                    holder.PagesAdapter.UserPagesList = new ObservableCollection<PageClass>(item.PagesModelClass.PagesList);
                                    holder.PagesAdapter.NotifyDataSetChanged();
                                }
                                else if (item.PagesModelClass?.PagesList != null && holder.PagesAdapter.UserPagesList.Count != item.PagesModelClass.PagesList.Count)
                                {
                                    holder.PagesAdapter.UserPagesList = new ObservableCollection<PageClass>(item.PagesModelClass.PagesList);
                                    holder.PagesAdapter.NotifyDataSetChanged();
                                }

                                if (!string.IsNullOrEmpty(item.PagesModelClass?.TitleHead))
                                    holder.AboutHead.Text = item.PagesModelClass?.TitleHead;

                                holder.AboutMore.Text = item.PagesModelClass?.More;

                                if (holder.PagesAdapter?.UserPagesList?.Count >= 5)
                                {
                                    holder.AboutMore.Visibility = ViewStates.Visible;
                                    holder.AboutMoreIcon.Visibility = ViewStates.Visible;
                                }
                                else
                                {
                                    holder.AboutMore.Visibility = ViewStates.Invisible;
                                    holder.AboutMoreIcon.Visibility = ViewStates.Invisible;
                                }
 
                                if (!holder.AboutMore.HasOnClickListeners)
                                {
                                    holder.AboutMore.Click += (sender, args) => { OpenAllViewer("MangedPagesModel", item); };
                                    holder.AboutMoreIcon.Click += (sender, args) => { OpenAllViewer("MangedPagesModel", item); };
                                }

                            }

                            break;
                        }
                        case SocialModelType.LikedPages:
                        {
                            if (viewHolder is PageAdapterViewHolder holder)
                            {
                                GlideImageLoader.LoadImage(ActivityContext, item.PageData.Avatar, holder.Image, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                             
                                holder.About.Text = item.PageData.Category;

                                if (!string.IsNullOrEmpty(item.PageData.PageTitle) || !string.IsNullOrWhiteSpace(item.PageData.PageTitle))
                                    holder.Name.Text = Methods.FunString.SubStringCutOf(Methods.FunString.DecodeString(item.PageData.PageTitle), 14);
                                else
                                    holder.Name.Text = Methods.FunString.SubStringCutOf(Methods.FunString.DecodeString(item.PageData.PageName), 14);

                                //Set style Btn Like page 
                                if (item.PageData.IsLiked == "true" || item.PageData.IsLiked == "yes")
                                { 
                                    holder.Button.SetBackgroundResource(Resource.Drawable.follow_button_profile_friends_pressed);
                                    holder.Button.SetTextColor(Color.ParseColor("#ffffff"));
                                    holder.Button.Text = ActivityContext.GetText(Resource.String.Btn_Unlike);
                                    holder.Button.Tag = "true";
                                }
                                else
                                {
                                    holder.Button.SetBackgroundResource(Resource.Drawable.follow_button_profile_friends);
                                    holder.Button.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                                    holder.Button.Text = ActivityContext.GetText(Resource.String.Btn_Like);
                                    holder.Button.Tag = "false";
                                }
                             
                                if (!holder.Button.HasOnClickListeners)
                                {
                                    holder.Button.Click += (sender, args) =>
                                    {
                                        try
                                        {
                                            if (!Methods.CheckConnectivity())
                                            {
                                                Toast.MakeText(ActivityContext, ActivityContext.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                                                return;
                                            }

                                            if (holder.Button.Tag.ToString() == "false")
                                            {
                                                holder.Button.SetBackgroundResource(Resource.Drawable.follow_button_profile_friends_pressed);
                                                holder.Button.SetTextColor(Color.ParseColor("#ffffff"));
                                                holder.Button.Text = ActivityContext.GetText(Resource.String.Btn_Unlike);
                                                holder.Button.Tag = "true";
                                            }
                                            else
                                            {
                                                holder.Button.SetBackgroundResource(Resource.Drawable.follow_button_profile_friends);
                                                holder.Button.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                                                holder.Button.Text = ActivityContext.GetText(Resource.String.Btn_Like);
                                                holder.Button.Tag = "false";
                                            }
                                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Page.Like_Page(item.PageData.PageId) });
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine(e);
                                        }
                                    };
                                }
                            }

                            break;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
         
        private void OpenAllViewer(string type, SocialModelsClass item)
        {
            try
            {
                var intent = new Intent(ActivityContext, typeof(AllViewerActivity));
                intent.PutExtra("Type", type); //MangedGroupsModel , MangedPagesModel 
                intent.PutExtra("PassedId", UserDetails.UserId);
                switch (type)
                {
                    case "MangedGroupsModel":
                        intent.PutExtra("itemObject", JsonConvert.SerializeObject(item.MangedGroupsModel));
                        break;
                    case "MangedPagesModel":
                        intent.PutExtra("itemObject", JsonConvert.SerializeObject(item.PagesModelClass));
                        break; 
                } 
                ActivityContext.StartActivity(intent);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        public override void OnViewRecycled(Java.Lang.Object holder)
        {
            try
            {
                if (holder != null)
                {
                    if (holder is GroupsAdapterViewHolder viewHolder)
                    {
                        Glide.With(ActivityContext).Clear(viewHolder.Image);
                    } 
                    
                    if (holder is PageAdapterViewHolder viewHolder2)
                    {
                        Glide.With(ActivityContext).Clear(viewHolder2.Image);
                    }
                }
                base.OnViewRecycled(holder);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        public SocialModelsClass GetItem(int position)
        {
            return SocialList[position];
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

        private void GroupsOnClick(GroupsAdapterClickEventArgs args)
        {
            GroupItemClick?.Invoke(this, args);
        }

        private void GroupsOnLongClick(GroupsAdapterClickEventArgs args)
        {
            GroupItemLongClick?.Invoke(this, args);
        }

        private void PageOnClick(PageAdapterClickEventArgs args)
        {
            PageItemClick?.Invoke(this, args);
        }

        private void PageOnLongClick(PageAdapterClickEventArgs args)
        {
            PageItemLongClick?.Invoke(this, args);
        }
         
        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = SocialList[p0];

                if (SocialPageType == SocialModelType.Groups)
                {
                    if (item.GroupData == null)
                        return d;
                    else
                    {
                        if (!string.IsNullOrEmpty(item.GroupData.Avatar))
                            d.Add(item.GroupData.Avatar); 
                        return d;
                    }
                }
                else if (SocialPageType == SocialModelType.Pages)
                {
                    if (item.PageData == null)
                        return d;
                    else
                    {
                        if (!string.IsNullOrEmpty(item.PageData.Avatar))
                            d.Add(item.PageData.Avatar);
                        return d;
                    }
                }
             
                return Collections.SingletonList(p0);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Collections.SingletonList(p0);
            }
        }

        public RequestBuilder GetPreloadRequestBuilder(Java.Lang.Object p0)
        {
            return Glide.With(ActivityContext).Load(p0.ToString())
                .Apply(new RequestOptions().CenterCrop().SetDiskCacheStrategy(DiskCacheStrategy.All)); 
        } 
    }

    public class GroupsAdapterViewHolder : RecyclerView.ViewHolder
    {
        public GroupsAdapterViewHolder(View itemView, Action<GroupsAdapterClickEventArgs> clickListener ) : base(itemView)
        {
            try
            {
                MainView = itemView;
                
                Image = MainView.FindViewById<ImageView>(Resource.Id.Image);
                Name = MainView.FindViewById<TextView>(Resource.Id.groupName);
                CountJoinedUsers = MainView.FindViewById<TextView>(Resource.Id.groupUsers);
                JoinButton = MainView.FindViewById<TextView>(Resource.Id.groupButtonJoin);
                //Event
                itemView.Click += (sender, e) => clickListener(new GroupsAdapterClickEventArgs {View = itemView, Position = AdapterPosition});
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public View MainView { get; }
        public ImageView Image { get; private set; }
        public TextView Name { get; private set; }
        public TextView CountJoinedUsers { get; private set; }
        public TextView JoinButton { get; private set; }
 
    }

    public class GroupsAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }


    public class PageAdapterViewHolder : RecyclerView.ViewHolder
    {
        public PageAdapterViewHolder(View itemView, Action<PageAdapterClickEventArgs> clickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Image = MainView.FindViewById<ImageView>(Resource.Id.Image);
                Name = MainView.FindViewById<TextView>(Resource.Id.card_name);
                About = MainView.FindViewById<TextView>(Resource.Id.card_dist);
                Button = MainView.FindViewById<Button>(Resource.Id.cont);

                //Event
                itemView.Click += (sender, e) => clickListener(new PageAdapterClickEventArgs{ View = itemView, Position = AdapterPosition });
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #region Variables Basic

        public View MainView { get; }

        public ImageView Image { get; private set; }

        public TextView Name { get; private set; }
        public TextView About { get; private set; }
        public Button Button { get; private set; }

        #endregion
    }

    public class PageAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }

}