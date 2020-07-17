using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Support.V4.Content;
using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Java.IO;
using Java.Util;
using Newtonsoft.Json;
using WoWonder.Activities.Jobs;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;
using Console = System.Console;
using Exception = System.Exception;

namespace WoWonder.Activities.NearbyBusiness.Adapters
{
    public class NearbyBusinessAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider 
    {
        public event EventHandler<NearbyBusinessAdapterClickEventArgs> ItemClick;
        public event EventHandler<NearbyBusinessAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<NearbyBusinessesDataObject> NearbyBusinessList = new ObservableCollection<NearbyBusinessesDataObject>();

        public NearbyBusinessAdapter(Activity context)
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

        public override int ItemCount => NearbyBusinessList?.Count ?? 0;


        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_JobView
                var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Style_JobView, parent, false);
                var vh = new NearbyBusinessAdapterViewHolder(itemView, Click, LongClick);
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
                if (viewHolder is NearbyBusinessAdapterViewHolder holder)
                {
                    var item = NearbyBusinessList[position];
                    if (item.Job?.JobInfoClass != null)
                    {
                        if (item.Job.Value.JobInfoClass.Image.Contains("http"))
                        {
                            GlideImageLoader.LoadImage(ActivityContext, item.Job.Value.JobInfoClass.Image, holder.Image, ImageStyle.FitCenter, ImagePlaceholders.Drawable);
                        }
                        else
                        {
                            File file2 = new File(item.Job.Value.JobInfoClass.Image);
                            var photoUri = FileProvider.GetUriForFile(ActivityContext, ActivityContext.PackageName + ".fileprovider", file2);
                            Glide.With(ActivityContext).Load(photoUri).Apply(new RequestOptions()).Into(holder.Image);
                        }

                        holder.Title.Text = Methods.FunString.DecodeString(item.Job.Value.JobInfoClass.Title);

                        var (currency, currencyIcon) = WoWonderTools.GetCurrency(item.Job.Value.JobInfoClass.Currency);
                        var categoryName = CategoriesController.ListCategoriesJob.FirstOrDefault(categories => categories.CategoriesId == item.Job.Value.JobInfoClass.Category)?.CategoriesName;
                        Console.WriteLine(currency);
                        if (string.IsNullOrEmpty(categoryName))
                            categoryName = Application.Context.GetText(Resource.String.Lbl_Unknown);

                        holder.Salary.Text = currencyIcon + " " + item.Job.Value.JobInfoClass.Minimum + " - " + currencyIcon + " " + item.Job.Value.JobInfoClass.Maximum + " . " + categoryName;

                        holder.Description.Text = Methods.FunString.SubStringCutOf(Methods.FunString.DecodeString(item.Job.Value.JobInfoClass.Description), 100);

                        if (item.Job.Value.JobInfoClass.IsOwner)
                        {
                            holder.IconMore.Visibility = ViewStates.Visible;
                            holder.Button.Text = ActivityContext.GetString(Resource.String.Lbl_show_applies) + " (" + item.Job.Value.JobInfoClass.ApplyCount + ")";
                            holder.Button.Tag = "ShowApply";
                        }
                        else
                        {
                            holder.IconMore.Visibility = ViewStates.Gone;
                        }

                        //Set Button if its applied
                        if (item.Job.Value.JobInfoClass.Apply == "true")
                        {
                            holder.Button.Text = ActivityContext.GetString(Resource.String.Lbl_already_applied);
                            holder.Button.Enabled = false;
                        }

                        if (!holder.Button.HasOnClickListeners)
                        {
                            holder.Button.Click += (sender, args) =>
                            {
                                try
                                {
                                    switch (holder.Button.Tag.ToString())
                                    {
                                        // Open Apply Job Activity 
                                        case "ShowApply":
                                            {
                                                if (item.Job.Value.JobInfoClass.ApplyCount == "0")
                                                {
                                                    Toast.MakeText(ActivityContext, ActivityContext.GetString(Resource.String.Lbl_ThereAreNoRequests), ToastLength.Short).Show();
                                                    return;
                                                }

                                                var intent = new Intent(ActivityContext, typeof(ShowApplyJobActivity));
                                                intent.PutExtra("JobsObject", JsonConvert.SerializeObject(item.Job.Value.JobInfoClass));
                                                ActivityContext.StartActivity(intent);
                                                break;
                                            }
                                        case "Apply":
                                            {
                                                var intent = new Intent(ActivityContext, typeof(ApplyJobActivity));
                                                intent.PutExtra("JobsObject", JsonConvert.SerializeObject(item.Job.Value.JobInfoClass));
                                                ActivityContext.StartActivity(intent);
                                                break;
                                            }
                                    }
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                }
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
                    if (holder is NearbyBusinessAdapterViewHolder viewHolder)
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
        public NearbyBusinessesDataObject GetItem(int position)
        {
            return NearbyBusinessList[position];
        }

        public override long GetItemId(int position)
        {
            try
            {
                return int.Parse(NearbyBusinessList[position].JobId);
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

        private void Click(NearbyBusinessAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void LongClick(NearbyBusinessAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }


        public RequestBuilder GetPreloadRequestBuilder(Java.Lang.Object p0)
        {
            return GlideImageLoader.GetPreLoadRequestBuilder(ActivityContext, p0.ToString(), ImageStyle.CircleCrop);
        }

        System.Collections.IList ListPreloader.IPreloadModelProvider.GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = NearbyBusinessList[p0];
                if (item == null)
                    return d;
                else
                {
                    if (item.Job != null && !string.IsNullOrEmpty(item.Job.Value.JobInfoClass.Image))
                        d.Add(item.Job.Value.JobInfoClass.Image);

                    return d;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Collections.SingletonList(p0);
            }
        } 
    }

    public class NearbyBusinessAdapterViewHolder : RecyclerView.ViewHolder
    {
        public NearbyBusinessAdapterViewHolder(View itemView, Action<NearbyBusinessAdapterClickEventArgs> clickListener, Action<NearbyBusinessAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Image = MainView.FindViewById<ImageView>(Resource.Id.JobCoverImage);
                Title = MainView.FindViewById<TextView>(Resource.Id.title);
                Salary = MainView.FindViewById<TextView>(Resource.Id.salary);
                Description = MainView.FindViewById<TextView>(Resource.Id.description);
                IconMore = MainView.FindViewById<TextView>(Resource.Id.iconMore);
                Button = MainView.FindViewById<Button>(Resource.Id.applyButton);
                Button.Tag = "Apply";

                IconMore.Visibility = ViewStates.Gone; 

                //Event  
                itemView.Click += (sender, e) => clickListener(new NearbyBusinessAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new NearbyBusinessAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #region Variables Basic

        public View MainView { get; }

        public ImageView Image { get; private set; }
        public TextView Title { get; private set; }
        public TextView Salary { get; private set; }
        public TextView IconMore { get; private set; }
        public Button Button { get; private set; }
        public TextView Description { get; private set; }

        #endregion
    }


    public class NearbyBusinessAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}