using System;
using Android.Content;
using WoWonderClient.Classes.Posts;

namespace WoWonder.Activities.NativePost.Post
{
    public static class PostFunctions
    {
        public static PostModelType GetAdapterType(PostDataObject item)
        {
            try
            {
                if (item == null)
                    return PostModelType.NormalPost;

                if (!string.IsNullOrEmpty(item.PostType) && item.PostType == "ad")
                    return PostModelType.AdsPost;
                if (item.SharedInfo.SharedInfoClass != null)
                    return PostModelType.SharedPost;

                if (!string.IsNullOrEmpty(item.PostType) && item.PostType == "profile_cover_picture" || item.PostType == "profile_picture")
                    return PostModelType.ImagePost;

                if (!string.IsNullOrEmpty(item.PostType) && item.PostType == "live" && !string.IsNullOrEmpty(item.StreamName))
                    return PostModelType.LivePost;

                if (item.PostFileFull != null && (GetImagesExtensions(item.PostFileFull) || item.PhotoMulti?.Count > 0 || item.PhotoAlbum?.Count > 0 || !string.IsNullOrEmpty(item.AlbumName)))
                {
                    if (item.PhotoMulti?.Count > 0)
                    {
                        switch (item.PhotoMulti?.Count)
                        {
                            case 2:
                                return PostModelType.MultiImage2;
                            case 3:
                                return PostModelType.MultiImage3;
                            case 4:
                                return PostModelType.MultiImage4;
                            default:
                                {
                                    if (item.PhotoMulti?.Count >= 5)
                                        return PostModelType.MultiImages;
                                    break;
                                }
                        }
                    }

                    if (item.PhotoAlbum?.Count > 0)
                    {
                        switch (item.PhotoAlbum?.Count)
                        {
                            case 1:
                                return PostModelType.ImagePost;
                            case 2:
                                return PostModelType.MultiImage2;
                            case 3:
                                return PostModelType.MultiImage3;
                            case 4:
                                return PostModelType.MultiImage4;
                            default:
                                {
                                    if (item.PhotoAlbum?.Count >= 5)
                                        return PostModelType.MultiImages;
                                    break;
                                }
                        }
                    }

                    return PostModelType.ImagePost;
                }
                if (!string.IsNullOrEmpty(item.PostFileFull) && (item.PostFileFull.Contains(".MP3") || item.PostFileFull.Contains(".mp3") || item.PostFileFull.Contains(".wav")))
                    return PostModelType.VoicePost;
                if (!string.IsNullOrEmpty(item.PostRecord))
                    return PostModelType.VoicePost;
                if (!string.IsNullOrEmpty(item.PostFileFull) && GetVideosExtensions(item.PostFileFull ))
                    return PostModelType.VideoPost;
                if (!string.IsNullOrEmpty(item.PostSticker))
                    return PostModelType.StickerPost;
                if (!string.IsNullOrEmpty(item.PostYoutube))
                    return PostModelType.YoutubePost;
                if (!string.IsNullOrEmpty(item.PostDeepsound))
                    return PostModelType.DeepSoundPost;
                if (!string.IsNullOrEmpty(item.PostPlaytube))
                    return PostModelType.PlayTubePost;
                if (!string.IsNullOrEmpty(item.PostLink))
                    return PostModelType.LinkPost;
                if (item.Product?.ProductClass != null)
                    return PostModelType.ProductPost;
                if (item.Job != null && (item.PostType == "job" || item.Job.Value.JobInfoClass != null))
                    return PostModelType.JobPost;
                if (item.Offer?.OfferClass != null && (item.PostType == "offer"))
                    return PostModelType.OfferPost;
                if (item.Blog != null)
                    return PostModelType.BlogPost;
                if (item.Event?.EventClass != null)
                    return PostModelType.EventPost;
                if (item.PostFileFull != null && (item.PostFileFull.Contains(".rar") || item.PostFileFull.Contains(".zip") || item.PostFileFull.Contains(".pdf")))
                    return PostModelType.FilePost;
                if (item.ColorId != "0")
                    return PostModelType.ColorPost;
                if (item.PollId != "0")
                    return PostModelType.PollPost;
                if (!string.IsNullOrEmpty(item.PostFacebook))
                    return PostModelType.FacebookPost;
                if (!string.IsNullOrEmpty(item.PostVimeo))
                    return PostModelType.VimeoPost;
                if (item.FundData?.FundDataClass != null)
                    return PostModelType.FundingPost;
                if (item.Fund?.PurpleFund != null)
                    return PostModelType.PurpleFundPost;
                if (!string.IsNullOrEmpty(item.PostMap))
                    return PostModelType.MapPost;

                return PostModelType.NormalPost;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);  
                return PostModelType.NormalPost;
            }
        }
        public static string GetFeelingTypeIcon(string feeling)
        {
            if (feeling == "sad")
                return "☹️";
            if (feeling == "happy")
                return "😄";
            if (feeling == "angry")
                return "😠";
            if (feeling == "funny")
                return "😂";
            if (feeling == "loved")
                return "😍";
            if (feeling == "cool")
                return "🕶️";
            if (feeling == "tired")
                return "😩";
            if (feeling == "sleepy")
                return "😴";
            if (feeling == "expressionless")
                return "😑";
            if (feeling == "confused")
                return "😕";
            if (feeling == "shocked")
                return "😱";
            if (feeling == "so_sad")
                return "😭";
            if (feeling == "blessed")
                return "😇";
            if (feeling == "bored")
                return "😒";
            if (feeling == "broken")
                return "💔";
            if (feeling == "lovely")
                return "❤️";
            if (feeling == "hot")
                return "😏"; 
            else
                return string.Empty;
        }

        public static string GetFeelingTypeTextString(string feeling, Context activityContext)
        {
            if (feeling == "sad")
                return activityContext.GetText(Resource.String.Lbl_Sad);
            if (feeling == "happy")
                return activityContext.GetText(Resource.String.Lbl_Happy);
            if (feeling == "angry")
                return activityContext.GetText(Resource.String.Lbl_Angry);
            if (feeling == "funny")
                return activityContext.GetText(Resource.String.Lbl_Funny);
            if (feeling == "loved")
                return activityContext.GetText(Resource.String.Lbl_Loved);
            if (feeling == "cool")
                return activityContext.GetText(Resource.String.Lbl_Cool);
            if (feeling == "tired")
                return activityContext.GetText(Resource.String.Lbl_Tired);
            if (feeling == "sleepy")
                return activityContext.GetText(Resource.String.Lbl_Sleepy);
            if (feeling == "expressionless")
                return activityContext.GetText(Resource.String.Lbl_Expressionless);
            if (feeling == "confused")
                return activityContext.GetText(Resource.String.Lbl_Confused);
            if (feeling == "shocked")
                return activityContext.GetText(Resource.String.Lbl_Shocked);
            if (feeling == "so_sad")
                return activityContext.GetText(Resource.String.Lbl_VerySad);
            if (feeling == "blessed")
                return activityContext.GetText(Resource.String.Lbl_Blessed);
            if (feeling == "bored")
                return activityContext.GetText(Resource.String.Lbl_Bored);
            if (feeling == "broken")
                return activityContext.GetText(Resource.String.Lbl_Broken);
            if (feeling == "lovely")
                return activityContext.GetText(Resource.String.Lbl_Lovely);
            if (feeling == "hot")
                return activityContext.GetText(Resource.String.Lbl_Hot); 
            else
                return string.Empty;
        }

        public static bool GetVideosExtensions(string extenstion)
        {
            if (extenstion.Contains(".MP4") || extenstion.Contains(".mp4"))
                return true;
            if (extenstion.Contains(".WMV") || extenstion.Contains(".wmv"))
                return true;
            if (extenstion.Contains(".3GP") || extenstion.Contains(".3gp"))
                return true;
            if (extenstion.Contains(".WEBM") || extenstion.Contains(".webm"))
                return true;
            if (extenstion.Contains(".FLV") || extenstion.Contains(".flv"))
                return true;
            if (extenstion.Contains(".AVI") || extenstion.Contains(".avi"))
                return true;
            if (extenstion.Contains(".HDV") || extenstion.Contains(".hdv"))
                return true;
            if (extenstion.Contains(".MPEG") || extenstion.Contains(".mpeg"))
                return true;
            if (extenstion.Contains(".MXF") || extenstion.Contains(".mxf"))
                return true;
            if (extenstion.Contains(".mov") || extenstion.Contains(".MOV"))
                return true; 
            else
                return false;
        }

        public static bool GetImagesExtensions(string extenstion)
        {
            if (extenstion == null)
                return false;

            if ((extenstion.Contains(".PNG") || extenstion.Contains(".png")))
                return true;
            if (extenstion.Contains(".JPG") || extenstion.Contains(".jpg"))
                return true;
            if (extenstion.Contains(".GIF") || extenstion.Contains(".gif"))
                return true;
            if (extenstion.Contains(".JPEG") || extenstion.Contains(".jpeg"))
                return true;
            else
                return false;
        }

       
    }
}