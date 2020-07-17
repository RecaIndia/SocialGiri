using System;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Text.Style;
using Android.Util;
using Java.Lang;
using Java.Util.Regex;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Posts;
using Exception = System.Exception;

namespace WoWonder.Helpers.Fonts
{
    public class TextViewWithImages : AppCompatTextView
    {
        
        protected TextViewWithImages(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public TextViewWithImages(Context context) : base(context)
        {
        }

        public TextViewWithImages(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        public TextViewWithImages(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
        }

        public override void SetText(ICharSequence text, BufferType type)
        {
            try
            {
                //SpannableString s = GetTextWithImages(Context, new Java.Lang.String(text.ToArray(), 0, text.Count()));
                base.SetText(text, BufferType.Spannable);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static UserDataObject Publisher;
        public virtual void SetText(ICharSequence text)
        {
            try
            {
                SetText(text, BufferType.Spannable);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static SpannableString GetTextWithImages(PostDataObject item, Context context, ICharSequence text)
        {
            try
            {
                SpannableString spendable = new SpannableString(text);
                AddImages(item, context, spendable);
                return spendable;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
         
        private static void AddImages(PostDataObject item, Context context, SpannableString spendable)
        { 
            try
            {   //Regex pattern that looks for embedded images of the format: [img src=imageName/]
                //exp. This [img src=imageName/] is an icon.

                Pattern refImg = Pattern.Compile("\\Q[img src=\\E([a-zA-Z0-9_]+?)\\Q/]\\E");

                //bool hasChanges = false;

                Matcher matcher = refImg.Matcher(spendable);

                while (matcher.Find())
                {
                    bool set = true;
                    foreach (var span in spendable.GetSpans(matcher.Start(), matcher.End(), Class.FromType(typeof(ImageSpan))))
                    {
                        if (spendable.GetSpanStart(span) >= matcher.Start() && spendable.GetSpanEnd(span) <= matcher.End())
                        {
                            spendable.RemoveSpan(span);
                        }
                        else
                        {
                            set = false;
                            break;
                        }
                    }

                    if (set)
                    { 
                        string resName = spendable.SubSequence(matcher.Start(1), matcher.End(1)).Trim();
                        int id = context.Resources.GetIdentifier(resName, "drawable", context.PackageName);

                        var d = ContextCompat.GetDrawable(context, id);
                        if (d != null)
                        {
                            d.SetBounds(0, 0, d.IntrinsicWidth, d.IntrinsicHeight);
                            spendable.SetSpan(new ImageSpan(d, SpanAlign.Baseline), matcher.Start(), matcher.End(), SpanTypes.ExclusiveExclusive);
                        }
                        else
                            spendable.SetSpan(new ImageSpan(context, id, SpanAlign.Baseline), matcher.Start(), matcher.End(), SpanTypes.ExclusiveExclusive);
                         
                        //hasChanges = true;

                    }
                }

                var username = WoWonderTools.GetNameFinal(Publisher);
                SetTextStyle(spendable, username, TypefaceStyle.Bold);
                 
                if (spendable.ToString().Contains(context.GetText(Resource.String.Lbl_ChangedProfileCover)))
                {
                    SetTextColor(spendable ,context.GetText(Resource.String.Lbl_ChangedProfileCover), "#888888");
                } 
                else if (spendable.ToString().Contains(context.GetText(Resource.String.Lbl_ChangedProfilePicture)))
                {
                    SetTextColor(spendable, context.GetText(Resource.String.Lbl_ChangedProfilePicture), "#888888"); 
                }
                else if (spendable.ToString().Contains(context.GetText(Resource.String.Lbl_WasLive)))
                {
                    SetTextColor(spendable, context.GetText(Resource.String.Lbl_WasLive), "#888888");
                }
                else if (spendable.ToString().Contains(context.GetText(Resource.String.Lbl_IsListeningTo)))
                {
                    SetTextColor(spendable, context.GetText(Resource.String.Lbl_IsListeningTo), "#888888");
                }
                else if (spendable.ToString().Contains(context.GetText(Resource.String.Lbl_IsPlaying)))
                {
                    SetTextColor(spendable, context.GetText(Resource.String.Lbl_IsPlaying), "#888888");
                }
                else if (spendable.ToString().Contains(context.GetText(Resource.String.Lbl_IsTravelingTo)))
                {
                    SetTextColor(spendable, context.GetText(Resource.String.Lbl_IsTravelingTo), "#888888");
                }
                else if (spendable.ToString().Contains(context.GetText(Resource.String.Lbl_IsWatching)))
                {
                    SetTextColor(spendable ,context.GetText(Resource.String.Lbl_IsWatching), "#888888");
                }
                else if (spendable.ToString().Contains(context.GetText(Resource.String.Lbl_AddedNewProductForSell)))
                {
                    SetTextColor(spendable ,context.GetText(Resource.String.Lbl_AddedNewProductForSell), "#888888");
                }
                else if (spendable.ToString().Contains(context.GetText(Resource.String.Lbl_CreatedNewArticle)))
                {
                    SetTextColor(spendable ,context.GetText(Resource.String.Lbl_CreatedNewArticle), "#888888");
                }
                else if (spendable.ToString().Contains(context.GetText(Resource.String.Lbl_CreatedNewEvent)) || (item.Event?.EventClass != null && item.SharedInfo.SharedInfoClass == null))
                {
                    SetTextColor(spendable ,context.GetText(Resource.String.Lbl_CreatedNewEvent), "#888888");
                    SetTextColor(spendable , Methods.FunString.DecodeString(item.Event?.EventClass.Name), "#888888");
                }
                else if (spendable.ToString().Contains(context.GetText(Resource.String.Lbl_addedNewPhotosTo)))
                {
                    SetTextColor(spendable ,context.GetText(Resource.String.Lbl_addedNewPhotosTo), "#888888");
                }
                else if (spendable.ToString().Contains(context.GetText(Resource.String.Lbl_CreatedNewFund)))
                {
                    SetTextColor(spendable ,context.GetText(Resource.String.Lbl_CreatedNewFund), "#888888");
                }
                else if (spendable.ToString().Contains(context.GetText(Resource.String.Lbl_OfferPostAdded)))
                {
                    SetTextColor(spendable ,context.GetText(Resource.String.Lbl_OfferPostAdded), "#888888");
                }
                else if (spendable.ToString().Contains(context.GetText(Resource.String.Lbl_SharedPost)))
                {
                    SetTextColor(spendable ,context.GetText(Resource.String.Lbl_SharedPost), "#888888");
                }
                   
                //return hasChanges;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void SetTextColor(SpannableString spendable , string texts, string color, float proportion = 0.9f)
        {
            try
            {
                string content = spendable.ToString();
                spendable.SetSpan(new ForegroundColorSpan(Color.ParseColor(color)), content.IndexOf(texts, StringComparison.Ordinal),/* content.IndexOf(texts, StringComparison.Ordinal) +*/ content.Length, SpanTypes.ExclusiveExclusive);
                spendable.SetSpan(new RelativeSizeSpan(proportion), content.IndexOf(texts, StringComparison.Ordinal), /*content.IndexOf(texts, StringComparison.Ordinal) +*/ content.Length, SpanTypes.ExclusiveExclusive);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            } 
        }

        private static void SetTextStyle(SpannableString spendable, string texts, TypefaceStyle style)
        {
            try
            {
                string content = spendable.ToString();
                spendable.SetSpan(new StyleSpan(style), content.IndexOf(texts, StringComparison.Ordinal), content.IndexOf(texts, StringComparison.Ordinal) + texts.Length, SpanTypes.ExclusiveExclusive);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

    }
}