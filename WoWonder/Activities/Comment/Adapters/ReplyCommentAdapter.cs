using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.Graphics;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using Com.Tuyenmonkey.Textdecorator;
using Java.IO;
using Java.Util;
using WoWonder.Activities.Comment.Fragment;
using WoWonder.Activities.NativePost.Holders;
using WoWonder.Helpers.CacheLoaders;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo;
using WoWonderClient;
using Console = System.Console;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;

namespace WoWonder.Activities.Comment.Adapters
{ 
    public class ReplyCommentAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    { 
        public string EmptyState = "Wo_Empty_State"; 
        private readonly ReplyCommentActivity ActivityContext; 
        public ObservableCollection<CommentObjectExtra> ReplyCommentList = new ObservableCollection<CommentObjectExtra>();
        private readonly CommentClickListener PostEventListener;

        public ReplyCommentAdapter(ReplyCommentActivity context)
        {
            try
            {
                HasStableIds = true;
                ActivityContext = context;
                PostEventListener = new CommentClickListener(ActivityContext, "Reply");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override int ItemCount => ReplyCommentList?.Count ?? 0;

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                switch (viewType)
                {
                    case 0: return new CommentAdapterViewHolder(LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Style_Comment, parent, false), this, PostEventListener);
                    case 1: return new CommentAdapterViewHolder(LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Style_Comment_Image, parent, false), this, PostEventListener);
                    case 666: return new MainHolders.EmptyStateAdapterViewHolder(LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Style_EmptyState, parent, false));
                    default:
                        return new CommentAdapterViewHolder(LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Style_Comment, parent, false), this, PostEventListener);
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
                if (viewHolder.ItemViewType == 666)
                {
                    if (!(viewHolder is MainHolders.EmptyStateAdapterViewHolder emptyHolder))
                        return;

                    emptyHolder.EmptyText.Text = "No Replies to be displayed";
                    return;
                }

                if (!(viewHolder is CommentAdapterViewHolder holder))
                    return;

                var item = ReplyCommentList[position];
                if (item == null)
                    return;

                if (AppSettings.FlowDirectionRightToLeft)
                    holder.BubbleLayout.LayoutDirection = LayoutDirection.Rtl;

                if (!string.IsNullOrEmpty(item.Text) || !string.IsNullOrWhiteSpace(item.Text))
                {
                    var changer = new TextSanitizer(holder.CommentText, ActivityContext);
                    changer.Load(Methods.FunString.DecodeString(item.Text));
                }
                else
                {
                    holder.CommentText.Visibility = ViewStates.Gone;
                }
                 
                if (holder.TimeTextView.Tag?.ToString() == "true")
                    return;

                holder.TimeTextView.Text = Methods.Time.TimeAgo(int.Parse(item.Time));
                holder.UserName.Text = item.Publisher.Name;
                GlideImageLoader.LoadImage(ActivityContext, item.Publisher.Avatar, holder.Image, ImageStyle.CircleCrop, ImagePlaceholders.Color);
                 
                var textHighLighter = item.Publisher.Name;
                var textIsPro = string.Empty;

                if (item.Publisher.Verified == "1")
                    textHighLighter += " " + IonIconsFonts.CheckmarkCircled;

                if (item.Publisher.IsPro == "1")
                {
                    textIsPro = " " + IonIconsFonts.Flash;
                    textHighLighter += textIsPro;
                }

                var decorator = TextDecorator.Decorate(holder.UserName, textHighLighter)
                    .SetTextStyle((int)TypefaceStyle.Bold, 0, item.Publisher.Name.Length);

                if (item.Publisher.Verified == "1")
                    decorator.SetTextColor(Resource.Color.Post_IsVerified, IonIconsFonts.CheckmarkCircled);

                if (item.Publisher.IsPro == "1")
                    decorator.SetTextColor(Resource.Color.text_color_in_between, textIsPro);

                decorator.Build();

                if (holder.ItemViewType == 1)
                    if (!string.IsNullOrEmpty(item.CFile) && (item.CFile.Contains("file://") || item.CFile.Contains("content://") || item.CFile.Contains("storage") || item.CFile.Contains("/data/user/0/")))
                    {
                        File file2 = new File(item.CFile);
                        var photoUri = FileProvider.GetUriForFile(ActivityContext, ActivityContext.PackageName + ".fileprovider", file2);
                        Glide.With(ActivityContext).Load(photoUri).Apply(new RequestOptions()).Into(holder.CommentImage);
                         
                        //GlideImageLoader.LoadImage(ActivityContext, item.CFile, holder.CommentImage, ImageStyle.CenterCrop, ImagePlaceholders.Color);
                    }
                    else
                    {
                        GlideImageLoader.LoadImage(ActivityContext, Client.WebsiteUrl + "/" + item.CFile, holder.CommentImage, ImageStyle.CenterCrop, ImagePlaceholders.Color);
                    }

                if (AppSettings.PostButton == PostButtonSystem.ReactionDefault || AppSettings.PostButton == PostButtonSystem.ReactionSubShine)
                {
                    item.Reaction ??= new WoWonderClient.Classes.Posts.Reaction();

                    if ((bool)(item.Reaction != null & item.Reaction?.IsReacted))
                    {
                        if (!string.IsNullOrEmpty(item.Reaction.Type))
                        {
                            switch (item.Reaction.Type)
                            {
                                case "1":
                                case "Like":
                                    ReactionComment.SetReactionPack(holder, ReactConstants.Like);
                                    holder.LikeTextView.Tag = "Liked";
                                    break;
                                case "2":
                                case "Love":
                                    ReactionComment.SetReactionPack(holder, ReactConstants.Love);
                                    holder.LikeTextView.Tag = "Liked";
                                    break;
                                case "3":
                                case "HaHa":
                                    ReactionComment.SetReactionPack(holder, ReactConstants.HaHa);
                                    holder.LikeTextView.Tag = "Liked";
                                    break;
                                case "4":
                                case "Wow":
                                    ReactionComment.SetReactionPack(holder, ReactConstants.Wow);
                                    holder.LikeTextView.Tag = "Liked";
                                    break;
                                case "5":
                                case "Sad":
                                    ReactionComment.SetReactionPack(holder, ReactConstants.Sad);
                                    holder.LikeTextView.Tag = "Liked";
                                    break;
                                case "6":
                                case "Angry":
                                    ReactionComment.SetReactionPack(holder, ReactConstants.Angry);
                                    holder.LikeTextView.Tag = "Liked";
                                    break;
                                default:
                                    holder.LikeTextView.Text = ActivityContext.GetText(Resource.String.Btn_Like);
                                    holder.LikeTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                                    holder.LikeTextView.Tag = "Like";
                                    break;

                            }
                        }
                    }
                    else
                    {
                        holder.LikeTextView.Text = ActivityContext.GetText(Resource.String.Btn_Like);
                        holder.LikeTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                        holder.LikeTextView.Tag = "Like";
                    }
                }
                else if (AppSettings.PostButton == PostButtonSystem.Wonder || AppSettings.PostButton == PostButtonSystem.DisLike)
                {
                    if ((bool)(item.Reaction != null & !item.Reaction?.IsReacted))
                        ReactionComment.SetReactionPack(holder, ReactConstants.Default);

                    if (item.IsCommentLiked)
                    {
                        holder.LikeTextView.Text = ActivityContext.GetText(Resource.String.Btn_Liked);
                        holder.LikeTextView.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                        holder.LikeTextView.Tag = "Liked";
                    }

                    switch (AppSettings.PostButton)
                    {
                        case PostButtonSystem.Wonder when item.IsCommentWondered:
                            {
                                holder.DislikeTextView.Text = ActivityContext.GetString(Resource.String.Lbl_wondered);
                                holder.DislikeTextView.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                                holder.DislikeTextView.Tag = "Disliked";
                                break;
                            }
                        case PostButtonSystem.Wonder:
                            {
                                holder.DislikeTextView.Text = ActivityContext.GetString(Resource.String.Btn_Wonder);
                                holder.DislikeTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                                holder.DislikeTextView.Tag = "Dislike";
                                break;
                            }
                        case PostButtonSystem.DisLike when item.IsCommentWondered:
                            {
                                holder.DislikeTextView.Text = ActivityContext.GetString(Resource.String.Lbl_disliked);
                                holder.DislikeTextView.SetTextColor(Color.ParseColor("#f89823"));
                                holder.DislikeTextView.Tag = "Disliked";
                                break;
                            }
                        case PostButtonSystem.DisLike:
                            {
                                holder.DislikeTextView.Text = ActivityContext.GetString(Resource.String.Btn_Dislike);
                                holder.DislikeTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                                holder.DislikeTextView.Tag = "Dislike";
                                break;
                            }
                    }
                }
                else
                {
                    if (item.IsCommentLiked)
                    {
                        holder.LikeTextView.Text = ActivityContext.GetText(Resource.String.Btn_Liked);
                        holder.LikeTextView.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                        holder.LikeTextView.Tag = "Liked";
                    }
                    else
                    {
                        holder.LikeTextView.Text = ActivityContext.GetText(Resource.String.Btn_Like);
                        holder.LikeTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                        holder.LikeTextView.Tag = "Like";
                    }
                }


                holder.TimeTextView.Tag = "true"; 
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
         
        public CommentObjectExtra GetItem(int position)
        {
            return ReplyCommentList[position];
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
                if (string.IsNullOrEmpty(ReplyCommentList[position].CFile) && ReplyCommentList[position].Text != EmptyState)
                    return 0;
                else if (ReplyCommentList[position].Text == EmptyState)
                    return 666;
                else
                    return 1;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return 0;
            }
        }
         
        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = ReplyCommentList[p0];
                if (item == null)
                    return d;
                else
                {
                    if (item.Text != EmptyState)
                    {
                        if (!string.IsNullOrEmpty(item.CFile))
                            d.Add(item.CFile);

                        if (!string.IsNullOrEmpty(item.Publisher.Avatar))
                            d.Add(item.Publisher.Avatar);

                        return d;
                    }

                    return Collections.SingletonList(p0);
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
            return GlideImageLoader.GetPreLoadRequestBuilder(ActivityContext, p0.ToString(), ImageStyle.CenterCrop);
        } 
    }  
}