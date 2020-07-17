using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Android.App;
using Android.Graphics;
using Android.Media;
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
using WoWonderClient.Classes.Comments;
using Console = System.Console;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;
using Timer = System.Timers.Timer;
using Uri = Android.Net.Uri;

namespace WoWonder.Activities.Comment.Adapters
{
    public class CommentObjectExtra : GetCommentObject 
    {
        public new Android.Media.MediaPlayer MediaPlayer { get; set; }
        public new Timer MediaTimer { get; set; }
        public new CommentAdapterViewHolder SoundViewHolder { get; set; }
    }

    public class CommentAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public string EmptyState = "Wo_Empty_State";
        private readonly Activity ActivityContext;

        public ObservableCollection<CommentObjectExtra> CommentList = new ObservableCollection<CommentObjectExtra>();
        private readonly CommentClickListener PostEventListener;

        public CommentAdapter(Activity context)
        {
            try
            {
                HasStableIds = true;
                ActivityContext = context;
                PostEventListener = new CommentClickListener(ActivityContext, "Comment");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override int ItemCount => CommentList?.Count ?? 0;
         
        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            { 
                switch (viewType)
                {
                    case 0:
                        return new CommentAdapterViewHolder(LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Style_Comment, parent, false),this, PostEventListener);
                    case 1:
                        return new CommentAdapterViewHolder(LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Style_Comment_Image, parent, false), this, PostEventListener);
                    case 2:
                        return new CommentAdapterViewHolder(LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Style_Comment_Voice, parent, false), this, PostEventListener);
                    case 666:
                        return new MainHolders.EmptyStateAdapterViewHolder(LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Style_EmptyState, parent, false));
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

        public void LoadCommentData(CommentObjectExtra item, CommentAdapterViewHolder holder, int position = 0, bool hasClickEvents = true)
        {
            try
            { 
                if (!string.IsNullOrEmpty(item.Text) || !string.IsNullOrWhiteSpace(item.Text))
                {
                    var changer = new TextSanitizer(holder.CommentText, ActivityContext);
                    changer.Load(Methods.FunString.DecodeString(item.Text));
                }
                else
                {
                    holder.CommentText.Visibility = ViewStates.Gone;
                }
                 
                holder.TimeTextView.Text = Methods.Time.TimeAgo(int.Parse(item.Time));
                holder.UserName.Text = item.Publisher.Name;

                GlideImageLoader.LoadImage(ActivityContext, item.Publisher.Avatar, holder.Image, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
             
                var textHighLighter = item.Publisher.Name;
                var textIsPro = string.Empty;

                if (item.Publisher.Verified == "1")
                    textHighLighter += " " + IonIconsFonts.CheckmarkCircled;

                if (item.Publisher.IsPro == "1")
                {
                    textIsPro = " " + IonIconsFonts.Flash;
                    textHighLighter += textIsPro;
                }

                var decorator = TextDecorator.Decorate(holder.UserName, textHighLighter).SetTextStyle((int) TypefaceStyle.Bold, 0, item.Publisher.Name.Length);

                if (item.Publisher.Verified == "1")
                    decorator.SetTextColor(Resource.Color.Post_IsVerified, IonIconsFonts.CheckmarkCircled);

                if (item.Publisher.IsPro == "1")
                    decorator.SetTextColor(Resource.Color.text_color_in_between, textIsPro);

                decorator.Build();
              
                //Image
                if (holder.ItemViewType == 1 || holder.CommentImage != null)
                {
                    if(!string.IsNullOrEmpty(item.CFile) && (item.CFile.Contains("file://") || item.CFile.Contains("content://") || item.CFile.Contains("storage") || item.CFile.Contains("/data/user/0/")))
                    {
                        File file2 = new File(item.CFile);
                        var photoUri = FileProvider.GetUriForFile(ActivityContext, ActivityContext.PackageName + ".fileprovider", file2);
                        Glide.With(ActivityContext).Load(photoUri).Apply(new RequestOptions()).Into(holder.CommentImage);
                         
                        //GlideImageLoader.LoadImage(ActivityContext,item.CFile, holder.CommentImage, ImageStyle.CenterCrop, ImagePlaceholders.Color);
                    }
                    else
                    {
                        if (!item.CFile.Contains(Client.WebsiteUrl))
                            item.CFile = WoWonderTools.GetTheFinalLink(item.CFile);

                        GlideImageLoader.LoadImage(ActivityContext, item.CFile, holder.CommentImage, ImageStyle.CenterCrop, ImagePlaceholders.Color);
                    } 
                }

                //Voice
                if (holder.VoiceLayout != null && !string.IsNullOrEmpty(item.Record))
                {
                    LoadAudioItem(holder, position, item);
                }
                 
                if (item.Replies != "0" && item.Replies != null)
                    holder.ReplyTextView.Text = ActivityContext.GetText(Resource.String.Lbl_Reply) + " " + "(" + item.Replies + ")";

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
            catch (Exception e)
            {
                Console.WriteLine(e);
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

                    var itemEmpty = CommentList.FirstOrDefault(a => a.Id == EmptyState);
                    if (itemEmpty != null && !string.IsNullOrEmpty(itemEmpty.Orginaltext)) 
                    {
                        emptyHolder.EmptyText.Text = itemEmpty.Orginaltext;
                    }
                    else
                    { 
                        emptyHolder.EmptyText.Text = ActivityContext.GetText(Resource.String.Lbl_NoComments);
                    }
                    return;
                }

                if (!(viewHolder is CommentAdapterViewHolder holder))
                    return;

                var item = CommentList[position];
                if (item == null)
                    return;

                LoadCommentData(item, holder, position);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private int PositionSound;

        private void LoadAudioItem(CommentAdapterViewHolder soundViewHolder, int position, CommentObjectExtra item)
        {
            try
            {
                item.SoundViewHolder ??= soundViewHolder;

                soundViewHolder.VoiceLayout.Visibility = ViewStates.Visible;

                var fileName = item.Record.Split('/').Last();

                var mediaFile = WoWonderTools.GetFile(item.PostId, Methods.Path.FolderDcimSound, fileName, item.Record);
                soundViewHolder.DurationVoice.Text = string.IsNullOrEmpty(item.MediaDuration)
                    ? Methods.AudioRecorderAndPlayer.GetTimeString(Methods.AudioRecorderAndPlayer.Get_MediaFileDuration(mediaFile))
                    : item.MediaDuration;

                soundViewHolder.PlayButton.Visibility = ViewStates.Visible;

                if (!soundViewHolder.PlayButton.HasOnClickListeners)
                {
                    soundViewHolder.PlayButton.Click += (o, args) =>
                    {
                        try
                        {
                            if (PositionSound != position)
                            {
                                var list = CommentList.Where(a => a.MediaPlayer != null).ToList();
                                if (list.Count > 0)
                                {
                                    foreach (var extra in list)
                                    {
                                        if (extra.MediaPlayer != null)
                                        {
                                            extra.MediaPlayer.Stop();
                                            extra.MediaPlayer.Reset();
                                        }
                                        extra.MediaPlayer = null;
                                        extra.MediaTimer = null;

                                        extra.MediaPlayer?.Release();
                                        extra.MediaPlayer = null;
                                    }
                                }
                            }

                            if (mediaFile.Contains("http"))
                                mediaFile = WoWonderTools.GetFile(item.PostId, Methods.Path.FolderDcimSound, fileName, item.Record);

                            if (item.MediaPlayer == null)
                            {
                                PositionSound = position;
                                item.MediaPlayer = new Android.Media.MediaPlayer();
                                item.MediaPlayer.SetAudioAttributes(new AudioAttributes.Builder().SetUsage(AudioUsageKind.Media).SetContentType(AudioContentType.Music).Build());

                                item.MediaPlayer.Completion += (sender, e) =>
                                {
                                    try
                                    {
                                        soundViewHolder.PlayButton.Tag = "Play";
                                        //soundViewHolder.PlayButton.SetImageResource(item.ModelType == MessageModelType.LeftAudio ? Resource.Drawable.ic_play_dark_arrow : Resource.Drawable.ic_play_arrow);
                                        soundViewHolder.PlayButton.SetImageResource(Resource.Drawable.ic_play_dark_arrow);

                                        item.MediaIsPlaying = false;

                                        item.MediaPlayer.Stop();
                                        item.MediaPlayer.Reset();
                                        item.MediaPlayer = null;

                                        item.MediaTimer.Enabled = false;
                                        item.MediaTimer.Stop();
                                        item.MediaTimer = null;
                                    }
                                    catch (Exception exception)
                                    {
                                        Console.WriteLine(exception);
                                    }
                                };

                                item.MediaPlayer.Prepared += (s, ee) =>
                                {
                                    try
                                    {
                                        item.MediaIsPlaying = true;
                                        soundViewHolder.PlayButton.Tag = "Pause";
                                        soundViewHolder.PlayButton.SetImageResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.ic_media_pause_light : Resource.Drawable.ic_media_pause_dark);

                                        if (item.MediaTimer == null)
                                            item.MediaTimer = new Timer { Interval = 1000 };

                                        item.MediaPlayer.Start();

                                        //var durationOfSound = item.MediaPlayer.Duration;

                                        item.MediaTimer.Elapsed += (sender, eventArgs) =>
                                        {
                                            ActivityContext.RunOnUiThread(() =>
                                            {
                                                try
                                                {
                                                    if (item.MediaTimer.Enabled)
                                                    {
                                                        if (item.MediaPlayer.CurrentPosition <= item.MediaPlayer.Duration)
                                                        {
                                                            soundViewHolder.DurationVoice.Text = Methods.AudioRecorderAndPlayer.GetTimeString(item.MediaPlayer.CurrentPosition);
                                                        }
                                                        else
                                                        {
                                                            soundViewHolder.DurationVoice.Text = Methods.AudioRecorderAndPlayer.GetTimeString(item.MediaPlayer.Duration);

                                                            soundViewHolder.PlayButton.Tag = "Play";
                                                            //soundViewHolder.PlayButton.SetImageResource(item.ModelType == MessageModelType.LeftAudio ? Resource.Drawable.ic_play_dark_arrow : Resource.Drawable.ic_play_arrow);
                                                            soundViewHolder.PlayButton.SetImageResource(Resource.Drawable.ic_play_dark_arrow);
                                                        }
                                                    }
                                                }
                                                catch (Exception e)
                                                {
                                                    Console.WriteLine(e);
                                                    soundViewHolder.PlayButton.Tag = "Play";
                                                }
                                            });
                                        };
                                        item.MediaTimer.Start();
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine(e);
                                    }
                                };

                                if (mediaFile.Contains("http"))
                                {
                                    item.MediaPlayer.SetDataSource(ActivityContext, Uri.Parse(mediaFile));
                                    item.MediaPlayer.PrepareAsync();
                                }
                                else
                                {
                                    File file2 = new File(mediaFile);
                                    var photoUri = FileProvider.GetUriForFile(ActivityContext, ActivityContext.PackageName + ".fileprovider", file2);

                                    item.MediaPlayer.SetDataSource(ActivityContext, photoUri);
                                    item.MediaPlayer.Prepare();
                                }

                                item.SoundViewHolder = soundViewHolder;
                            }
                            else
                            {
                                if (soundViewHolder.PlayButton.Tag.ToString() == "Play")
                                {
                                    soundViewHolder.PlayButton.Tag = "Pause";
                                    soundViewHolder.PlayButton.SetImageResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.ic_media_pause_light : Resource.Drawable.ic_media_pause_dark);

                                    item.MediaIsPlaying = true;
                                    item.MediaPlayer?.Start();

                                    if (item.MediaTimer != null)
                                    {
                                        item.MediaTimer.Enabled = true;
                                        item.MediaTimer.Start();
                                    }
                                }
                                else if (soundViewHolder.PlayButton.Tag.ToString() == "Pause")
                                {
                                    soundViewHolder.PlayButton.Tag = "Play";
                                    //soundViewHolder.PlayButton.SetImageResource(item.ModelType == MessageModelType.LeftAudio ? Resource.Drawable.ic_play_dark_arrow : Resource.Drawable.ic_play_arrow);
                                    soundViewHolder.PlayButton.SetImageResource(Resource.Drawable.ic_play_dark_arrow);

                                    item.MediaIsPlaying = false;
                                    item.MediaPlayer?.Pause();

                                    if (item.MediaTimer != null)
                                    {
                                        item.MediaTimer.Enabled = false;
                                        item.MediaTimer.Stop();
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    };
                }

                item.SoundViewHolder ??= soundViewHolder;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
         
        public CommentObjectExtra GetItem(int position)
        {
            return CommentList[position];
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
                var item = CommentList[position];

                if (string.IsNullOrEmpty(item.CFile) && string.IsNullOrEmpty(item.Record) && item.Text != EmptyState)
                    return 0;

                if ((!string.IsNullOrEmpty(item.CFile) && !string.IsNullOrEmpty(item.Record)) || !string.IsNullOrEmpty(item.CFile))
                    return 1;

                if (!string.IsNullOrEmpty(item.Record))
                    return 2;

                if (item.Text == EmptyState)
                    return 666;

                return 0;
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
                var item = CommentList[p0];
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
