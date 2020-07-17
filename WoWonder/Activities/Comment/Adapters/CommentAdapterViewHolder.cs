using System;
using System.Linq;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using AT.Markushi.UI;
using Com.Luseen.Autolinklibrary;
using Refractored.Controls;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Helpers.Model;

namespace WoWonder.Activities.Comment.Adapters
{
    public class CommentAdapterViewHolder : RecyclerView.ViewHolder, View.IOnClickListener, View.IOnLongClickListener
    {
        #region Variables Basic

        public View MainView { get; private set; }
        public CommentAdapter CommentAdapter;
        private readonly ReplyCommentAdapter ReplyCommentAdapter;
        private readonly CommentClickListener PostClickListener;
        private readonly string TypeClass;
     
        public LinearLayout BubbleLayout { get; private set; }
        public CircleImageView Image { get; private set; }
        public AutoLinkTextView CommentText { get; private set; }
        public TextView TimeTextView { get; private set; }
        public TextView UserName { get; private set; }
        public TextView ReplyTextView { get; private set; }
        public TextView LikeTextView { get; private set; }
        public TextView DislikeTextView { get; private set; }

        public ImageView CommentImage { get; private set; }

        public LinearLayout VoiceLayout { get; private set; }
        public CircleButton PlayButton { get; private set; }
        public TextView DurationVoice { get; private set; }
        public TextView TimeVoice { get; private set; }

        #endregion

        //Comment
        public CommentAdapterViewHolder(View itemView, CommentAdapter commentAdapter, CommentClickListener postClickListener , string typeClass = "Comment") : base(itemView)
        {
            try
            {
                MainView = itemView;

                CommentAdapter = commentAdapter;
                PostClickListener = postClickListener;
                TypeClass = typeClass;

                BubbleLayout = MainView.FindViewById<LinearLayout>(Resource.Id.bubble_layout);
                Image = MainView.FindViewById<CircleImageView>(Resource.Id.card_pro_pic);
                CommentText = MainView.FindViewById<AutoLinkTextView>(Resource.Id.active);
                UserName = MainView.FindViewById<TextView>(Resource.Id.username);
                TimeTextView = MainView.FindViewById<TextView>(Resource.Id.time);
                ReplyTextView = MainView.FindViewById<TextView>(Resource.Id.reply);
                LikeTextView = MainView.FindViewById<TextView>(Resource.Id.Like);
                DislikeTextView = MainView.FindViewById<TextView>(Resource.Id.dislike);
                CommentImage = MainView.FindViewById<ImageView>(Resource.Id.image);

                try
                {
                    VoiceLayout = MainView.FindViewById<LinearLayout>(Resource.Id.voiceLayout);
                    PlayButton = MainView.FindViewById<CircleButton>(Resource.Id.playButton);
                    DurationVoice = MainView.FindViewById<TextView>(Resource.Id.Duration);
                    TimeVoice = MainView.FindViewById<TextView>(Resource.Id.timeVoice);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                  
                var font = Typeface.CreateFromAsset(MainView.Context.Resources.Assets, "ionicons.ttf");
                UserName.SetTypeface(font, TypefaceStyle.Normal);

                if (AppSettings.FlowDirectionRightToLeft)
                    BubbleLayout.SetBackgroundResource(Resource.Drawable.comment_rounded_right_layout);

                if (AppSettings.PostButton == PostButtonSystem.DisLike || AppSettings.PostButton == PostButtonSystem.Wonder)
                    DislikeTextView.Visibility = ViewStates.Visible;

                ReplyTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                LikeTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                DislikeTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                 
                MainView.SetOnLongClickListener(this);
                Image.SetOnClickListener(this);
                LikeTextView.SetOnClickListener(this);
                DislikeTextView.SetOnClickListener(this);
                ReplyTextView.SetOnClickListener(this);
                CommentImage?.SetOnClickListener(this); 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Reply
        public CommentAdapterViewHolder(View itemView, ReplyCommentAdapter commentAdapter, CommentClickListener postClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                ReplyCommentAdapter = commentAdapter;
                PostClickListener = postClickListener;
                TypeClass = "Reply";

                BubbleLayout = MainView.FindViewById<LinearLayout>(Resource.Id.bubble_layout);
                Image = MainView.FindViewById<CircleImageView>(Resource.Id.card_pro_pic);
                CommentText = MainView.FindViewById<AutoLinkTextView>(Resource.Id.active);
                UserName = MainView.FindViewById<TextView>(Resource.Id.username);
                TimeTextView = MainView.FindViewById<TextView>(Resource.Id.time);
                ReplyTextView = MainView.FindViewById<TextView>(Resource.Id.reply);
                LikeTextView = MainView.FindViewById<TextView>(Resource.Id.Like);
                DislikeTextView = MainView.FindViewById<TextView>(Resource.Id.dislike);
                CommentImage = MainView.FindViewById<ImageView>(Resource.Id.image);

                try
                {
                    VoiceLayout = MainView.FindViewById<LinearLayout>(Resource.Id.voiceLayout);
                    PlayButton = MainView.FindViewById<CircleButton>(Resource.Id.playButton);
                    DurationVoice = MainView.FindViewById<TextView>(Resource.Id.Duration);
                    TimeVoice = MainView.FindViewById<TextView>(Resource.Id.timeVoice);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                var font = Typeface.CreateFromAsset(MainView.Context.Resources.Assets, "ionicons.ttf");
                UserName.SetTypeface(font, TypefaceStyle.Normal);

                if (AppSettings.FlowDirectionRightToLeft)
                    BubbleLayout.SetBackgroundResource(Resource.Drawable.comment_rounded_right_layout);

                if (AppSettings.PostButton == PostButtonSystem.DisLike || AppSettings.PostButton == PostButtonSystem.Wonder || AppSettings.PostButton == PostButtonSystem.Like)
                {
                    LikeTextView.Visibility = ViewStates.Gone;
                    DislikeTextView.Visibility = ViewStates.Gone;
                }
                 
                ReplyTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                LikeTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                DislikeTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                MainView.SetOnLongClickListener(this);
                Image.SetOnClickListener(this);
                LikeTextView.SetOnClickListener(this);
                DislikeTextView.SetOnClickListener(this);
                ReplyTextView.SetOnClickListener(this);
                CommentImage?.SetOnClickListener(this);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OnClick(View v)
        {
            try
            {
                if (AdapterPosition != RecyclerView.NoPosition)
                {
                    CommentObjectExtra item = null;
                    switch (TypeClass)
                    {
                        case "Comment":
                            item = CommentAdapter.CommentList[AdapterPosition];
                            break;
                        case "Post":
                            item = CommentAdapter.CommentList.FirstOrDefault(danjo => string.IsNullOrEmpty(danjo.CFile) && string.IsNullOrEmpty(danjo.Record));
                            break;
                        case "Reply":
                            item = ReplyCommentAdapter.ReplyCommentList[AdapterPosition];
                            break;
                    }
                     
                    if (v.Id == Image.Id)
                        PostClickListener.ProfilePostClick(new ProfileClickEventArgs { Holder = this, CommentClass = item, Position = AdapterPosition, View = MainView });
                    else if (v.Id == LikeTextView.Id)
                        PostClickListener.LikeCommentReplyPostClick(new CommentReplyClickEventArgs { Holder = this, CommentObject = item, Position = AdapterPosition, View = MainView });
                    else if (v.Id == DislikeTextView.Id)
                        PostClickListener.DislikeCommentReplyPostClick(new CommentReplyClickEventArgs { Holder = this, CommentObject = item, Position = AdapterPosition, View = MainView });
                    else if (v.Id == ReplyTextView.Id)
                        PostClickListener.CommentReplyPostClick(new CommentReplyClickEventArgs { Holder = this, CommentObject = item, Position = AdapterPosition, View = MainView });
                    else if (v.Id == CommentImage?.Id)
                        PostClickListener.OpenImageLightBox(item);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e); Log.Debug("wael >> CommentAdapterViewHolder", e.Message + "\n" + e.StackTrace + "\n" + e.HelpLink);
            }
        }

        public bool OnLongClick(View v)
        {
            //add event if System = ReactButton 
            if (AdapterPosition != RecyclerView.NoPosition)
            {
                CommentObjectExtra item = null;
                switch (TypeClass)
                {
                    case "Comment":
                        item = CommentAdapter.CommentList[AdapterPosition];
                        break;
                    case "Post":
                        item = CommentAdapter.CommentList.FirstOrDefault(danjo => string.IsNullOrEmpty(danjo.CFile) && string.IsNullOrEmpty(danjo.Record));
                        break;
                    case "Reply":
                        item = ReplyCommentAdapter.ReplyCommentList[AdapterPosition];
                        break;
                }

                if (v.Id == MainView.Id)
                    PostClickListener.MoreCommentReplyPostClick(new CommentReplyClickEventArgs { Holder = this, CommentObject = item, Position = AdapterPosition, View = MainView });
            }

            return true;
        }

    }
}