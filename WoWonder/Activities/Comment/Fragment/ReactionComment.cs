using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using WoWonder.Activities.Comment.Adapters;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.Library.Anjo;
using WoWonderClient.Requests;

namespace WoWonder.Activities.Comment.Fragment
{
    public class ReactionComment 
    {
        private readonly Activity MainContext;
        private readonly string TypeClass;
        private Android.Support.V7.App.AlertDialog MReactAlertDialog;

        //ImagesButton one for every Reaction
        private ImageView MImgButtonOne;
        private ImageView MImgButtonTwo;
        private ImageView MImgButtonThree;
        private ImageView MImgButtonFour;
        private ImageView MImgButtonFive;
        private ImageView MImgButtonSix;

        //Integer variable to change react dialog shape Default value is react_dialog_shape
        private int MReactDialogShape = Resource.Xml.react_dialog_shape;

        //Array of six Reaction one for every ImageButton Icon
        private List<Reaction> MReactionPack = XReactions.GetReactions();

        private CommentReplyClickEventArgs PostData;

        public ReactionComment(Activity context , string type)
        {
            MainContext = context;
            TypeClass = type;
        }
         
        /// <summary>
        /// Show Reaction dialog when user long click on react button
        /// </summary>
        public void ClickDialog(CommentReplyClickEventArgs postData)
        {
            try
            {
                PostData = postData;

                //Show Dialog With 6 React
                Android.Support.V7.App.AlertDialog.Builder dialogBuilder = new Android.Support.V7.App.AlertDialog.Builder(MainContext);

                //Irrelevant code for customizing the buttons and title
                LayoutInflater inflater = (LayoutInflater)MainContext.GetSystemService(Context.LayoutInflaterService);
                View dialogView = inflater.Inflate(Resource.Layout.XReactDialogLayout, null);

                InitializingReactImages(dialogView);
                ClickImageButtons();

                dialogBuilder.SetView(dialogView);
                MReactAlertDialog = dialogBuilder.Create();
                MReactAlertDialog.Window.SetBackgroundDrawableResource(MReactDialogShape);

                Window window = MReactAlertDialog.Window;
                window.SetLayout(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);

                MReactAlertDialog.Show();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="view">View Object to initialize all ImagesButton</param>
        private void InitializingReactImages(View view)
        {
            try
            {
                MImgButtonOne = view.FindViewById<ImageView>(Resource.Id.imgButtonOne);
                MImgButtonTwo = view.FindViewById<ImageView>(Resource.Id.imgButtonTwo);
                MImgButtonThree = view.FindViewById<ImageView>(Resource.Id.imgButtonThree);
                MImgButtonFour = view.FindViewById<ImageView>(Resource.Id.imgButtonFour);
                MImgButtonFive = view.FindViewById<ImageView>(Resource.Id.imgButtonFive);
                MImgButtonSix = view.FindViewById<ImageView>(Resource.Id.imgButtonSix);

                if (AppSettings.PostButton == PostButtonSystem.ReactionDefault)
                {
                    Glide.With(MainContext).Load(Resource.Drawable.emoji_like).Apply(new RequestOptions().FitCenter()).Into(MImgButtonOne);
                    Glide.With(MainContext).Load(Resource.Drawable.emoji_love).Apply(new RequestOptions().FitCenter()).Into(MImgButtonTwo);
                    Glide.With(MainContext).Load(Resource.Drawable.emoji_haha).Apply(new RequestOptions().FitCenter()).Into(MImgButtonThree);
                    Glide.With(MainContext).Load(Resource.Drawable.emoji_wow).Apply(new RequestOptions().FitCenter()).Into(MImgButtonFour);
                    Glide.With(MainContext).Load(Resource.Drawable.emoji_sad).Apply(new RequestOptions().FitCenter()).Into(MImgButtonFive);
                    Glide.With(MainContext).Load(Resource.Drawable.emoji_angry_face).Apply(new RequestOptions().FitCenter()).Into(MImgButtonSix);
                }
                else if (AppSettings.PostButton == PostButtonSystem.ReactionSubShine)
                {
                    Glide.With(MainContext).Load(Resource.Drawable.like).Apply(new RequestOptions().FitCenter()).Into(MImgButtonOne);
                    Glide.With(MainContext).Load(Resource.Drawable.love).Apply(new RequestOptions().FitCenter()).Into(MImgButtonTwo);
                    Glide.With(MainContext).Load(Resource.Drawable.haha).Apply(new RequestOptions().FitCenter()).Into(MImgButtonThree);
                    Glide.With(MainContext).Load(Resource.Drawable.wow).Apply(new RequestOptions().FitCenter()).Into(MImgButtonFour);
                    Glide.With(MainContext).Load(Resource.Drawable.sad).Apply(new RequestOptions().FitCenter()).Into(MImgButtonFive);
                    Glide.With(MainContext).Load(Resource.Drawable.angry).Apply(new RequestOptions().FitCenter()).Into(MImgButtonSix);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        } 

        /// <summary>
        /// Set onClickListener For every Image Buttons on Reaction Dialog
        /// </summary>
        private void ClickImageButtons()
        {
            ImgButtonSetListener(MImgButtonOne, 0, ReactConstants.Like);
            ImgButtonSetListener(MImgButtonTwo, 1, ReactConstants.Love);
            ImgButtonSetListener(MImgButtonThree, 2, ReactConstants.HaHa);
            ImgButtonSetListener(MImgButtonFour, 3, ReactConstants.Wow);
            ImgButtonSetListener(MImgButtonFive, 4, ReactConstants.Sad);
            ImgButtonSetListener(MImgButtonSix, 5, ReactConstants.Angry);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="imgButton">ImageButton view to set onClickListener</param>
        /// <param name="reactIndex">Index of Reaction to take it from ReactionPack</param>
        private void ImgButtonSetListener(ImageView imgButton, int reactIndex , string reactName)
        {
            if (!imgButton.HasOnClickListeners)
                imgButton.Click += (sender, e) => ImgButtonOnClick(new ReactionsClickEventArgs { ImgButton = imgButton, Position = reactIndex , React = reactName });
        }

        private void ImgButtonOnClick(ReactionsClickEventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(Application.Context, Application.Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                    return;
                }

                if (UserDetails.SoundControl)
                    Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("reaction.mp3");
                 
                PostData.CommentObject.Reaction ??= new WoWonderClient.Classes.Posts.Reaction();

                PostData.CommentObject.Reaction.Count++;

                if (!PostData.CommentObject.Reaction.IsReacted)
                    PostData.CommentObject.Reaction.IsReacted = true;
                 
                var data = MReactionPack[e.Position];
                if (data != null)
                {
                    SetReactionPack(PostData.Holder, data.GetReactText());
                    PostData.Holder.LikeTextView.Tag = "Liked";
                }

                string reactionType = "reaction_comment";
                if (TypeClass == "Reply")
                    reactionType = "reaction_reply";
                 
                if (e.React == ReactConstants.Like)
                {
                    PostData.CommentObject.Reaction.Type = "Like";
                    PostData.CommentObject.Reaction.Like++;
                    PostData.CommentObject.Reaction.Like1++;

                    string react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Name == "Like").Value?.Id ?? "1";
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Comment.ReactionCommentAsync(PostData.CommentObject.Id, react, reactionType) });
                }
                else if (e.React == ReactConstants.Love)
                {
                    PostData.CommentObject.Reaction.Type = "Love";
                    PostData.CommentObject.Reaction.Love++;
                    PostData.CommentObject.Reaction.Love2++;
                    string react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Name == "Love").Value?.Id ?? "2";
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Comment.ReactionCommentAsync(PostData.CommentObject.Id, react, reactionType) });
                }
                else if (e.React == ReactConstants.HaHa)
                {
                    PostData.CommentObject.Reaction.Type = "HaHa";
                    PostData.CommentObject.Reaction.HaHa++;
                    PostData.CommentObject.Reaction.HaHa3++;
                    string react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Name == "HaHa").Value?.Id ?? "3";
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Comment.ReactionCommentAsync(PostData.CommentObject.Id, react, reactionType) });
                }
                else if (e.React == ReactConstants.Wow)
                {
                    PostData.CommentObject.Reaction.Type = "Wow";
                    PostData.CommentObject.Reaction.Wow++;
                    PostData.CommentObject.Reaction.Wow4++;
                    string react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Name == "Wow").Value?.Id ?? "4";
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Comment.ReactionCommentAsync(PostData.CommentObject.Id, react, reactionType) });
                }
                else if (e.React == ReactConstants.Sad)
                {
                    PostData.CommentObject.Reaction.Type = "Sad";
                    PostData.CommentObject.Reaction.Sad++;
                    PostData.CommentObject.Reaction.Sad5++;
                    string react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Name == "Sad").Value?.Id ?? "5";
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Comment.ReactionCommentAsync(PostData.CommentObject.Id, react, reactionType) });
                }
                else if (e.React == ReactConstants.Angry)
                {
                    PostData.CommentObject.Reaction.Type = "Angry";
                    PostData.CommentObject.Reaction.Angry++;
                    PostData.CommentObject.Reaction.Angry6++;
                    string react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Name == "Angry").Value?.Id ?? "6";
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.Comment.ReactionCommentAsync(PostData.CommentObject.Id, react, reactionType) });
                }
                 
                MReactAlertDialog.Dismiss(); 
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        } 

        public static void SetReactionPack(CommentAdapterViewHolder holder, string React)
        {
            try
            {
               var MReactionPack = XReactions.GetReactions();

                var data = MReactionPack?.FirstOrDefault(a => a.GetReactText() == React);
                if (data != null)
                {
                    holder.LikeTextView.Text = data.GetReactText();
                    holder.LikeTextView.SetTextColor(Color.ParseColor(data.GetReactTextColor()));
                } 
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        } 
    }
}