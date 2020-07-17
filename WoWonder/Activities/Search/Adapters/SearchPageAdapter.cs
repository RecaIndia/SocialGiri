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

namespace WoWonder.Activities.Search.Adapters
{
    public class SearchPageAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<SearchPageAdapterClickEventArgs> ItemClick;
        public event EventHandler<SearchPageAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;

        public ObservableCollection<PageClass> PageList = new ObservableCollection<PageClass>();

        public SearchPageAdapter(Activity context)
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

        public override int ItemCount => PageList?.Count ?? 0;
 
        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_HPage_view
                var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Style_HPage_view, parent, false);
                var vh = new SearchPageAdapterViewHolder(itemView, Click, LongClick);
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
                if (viewHolder is SearchPageAdapterViewHolder holder)
                {
                    var item = PageList[position];
                    if (item != null)
                    {
                        Initialize(holder, item);
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void Initialize(SearchPageAdapterViewHolder holder, PageClass item)
        {
            try
            {
                GlideImageLoader.LoadImage(ActivityContext, item.Avatar, holder.Image, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                if (!string.IsNullOrEmpty(item.PageTitle) || !string.IsNullOrWhiteSpace(item.PageTitle))
                    holder.Name.Text = Methods.FunString.SubStringCutOf(Methods.FunString.DecodeString(item.PageTitle), 20);
                else
                    holder.Name.Text = Methods.FunString.SubStringCutOf(Methods.FunString.DecodeString(item.PageName), 20);

                CategoriesController cat = new CategoriesController();
                holder.About.Text = cat.Get_Translate_Categories_Communities(item.PageCategory, item.Category);

                //var drawable = TextDrawable.InvokeBuilder().BeginConfig().FontSize(30).EndConfig().BuildRound("", Color.ParseColor("#BF360C"));
                //holder.ImageView.SetImageDrawable(drawable);

                if (item.IsLiked != null)
                {
                    //Set style Btn Like page 
                    if (item.IsLiked == "no" || item.IsLiked == "No" || item.IsLiked == "false")
                    {
                        holder.Button.SetBackgroundResource(Resource.Drawable.follow_button_profile_friends);
                        holder.Button.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                        holder.Button.Text = ActivityContext.GetText(Resource.String.Btn_Like);
                        holder.Button.Tag = "false";
                    }
                    else if (item.IsLiked == "yes" || item.IsLiked == "Yes" || item.IsLiked == "true")
                    {
                        holder.Button.SetBackgroundResource(Resource.Drawable.follow_button_profile_friends_pressed);
                        holder.Button.SetTextColor(Color.ParseColor("#ffffff"));
                        holder.Button.Text = ActivityContext.GetText(Resource.String.Btn_Unlike);
                        holder.Button.Tag = "true";
                    }
                }
                else
                {
                    holder.Button.Visibility = ViewStates.Gone; 
                }
                
                if (!holder.Button.HasOnClickListeners)
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
                                holder.Button.SetBackgroundResource(Resource.Drawable
                                    .follow_button_profile_friends_pressed);
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
                         
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Page.Like_Page(item.PageId) });
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    };
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
                    if (holder is SearchPageAdapterViewHolder viewHolder)
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
            return PageList[position];
        }

        public override long GetItemId(int position)
        {
            try
            {
                return int.Parse(PageList[position].Id);
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

        private void Click(SearchPageAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(SearchPageAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }
         
        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = PageList[p0];
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

    public class SearchPageAdapterViewHolder : RecyclerView.ViewHolder
    {
        public SearchPageAdapterViewHolder(View itemView, Action<SearchPageAdapterClickEventArgs> clickListener,Action<SearchPageAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Image = MainView.FindViewById<ImageView>(Resource.Id.Image);
                Name = MainView.FindViewById<TextView>(Resource.Id.card_name);
                About = MainView.FindViewById<TextView>(Resource.Id.card_dist);
                Button = MainView.FindViewById<Button>(Resource.Id.cont);
                IconGroup = MainView.FindViewById<ImageView>(Resource.Id.Icon);
                CircleView = MainView.FindViewById<View>(Resource.Id.image_view);

               //CircleView.SetBackgroundResource(Resource.Drawable.circlegradient3);
                IconGroup.SetImageResource(Resource.Drawable.icon_social_flag_vector);
                //Event      
                itemView.Click += (sender, e) => clickListener(new SearchPageAdapterClickEventArgs{View = itemView, Position = AdapterPosition});
                itemView.LongClick += (sender, e) => longClickListener(new SearchPageAdapterClickEventArgs{View = itemView, Position = AdapterPosition});

                

               

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #region Variables Basic

        public View MainView { get; }

        
        public ImageView Image { get; set; }

        public TextView Name { get; set; }
        public TextView About { get; set; }
        public Button Button { get; set; }
        public ImageView IconGroup { get; set; }
        public View CircleView { get; set; }

        #endregion
    }

    public class SearchPageAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}