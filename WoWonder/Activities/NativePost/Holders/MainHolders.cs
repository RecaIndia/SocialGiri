using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;

namespace WoWonder.Activities.NativePost.Holders
{
    public class MainHolders
    {
        public class EmptyStateAdapterViewHolder : RecyclerView.ViewHolder
        {
            public View MainView { get; private set; }
            public TextView EmptyText { get; private set; }
            public ImageView EmptyImage { get; private set; }

            public EmptyStateAdapterViewHolder(View itemView) : base(itemView)
            {
                MainView = itemView;
                EmptyText = MainView.FindViewById<TextView>(Resource.Id.textEmpty);
                EmptyImage = MainView.FindViewById<ImageView>(Resource.Id.imageEmpty);
            }
        }
    }
}