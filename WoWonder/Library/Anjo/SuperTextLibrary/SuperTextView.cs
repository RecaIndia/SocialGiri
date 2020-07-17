using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Text.Style;
using Android.Util;
using Java.Lang;
using Java.Lang.Reflect;
using Java.Util.Regex;
using System;
using System.Collections.Generic;
using Exception = System.Exception;
using Pattern = Java.Util.Regex.Pattern;


namespace WoWonder.Library.Anjo.SuperTextLibrary
{
    public class SuperTextView : AppCompatTextView
    {
        private static readonly int MinPhoneNumberLength = 8;
        private static readonly Color DefaultColor = Color.Red;
        public StTools.IXAutoLinkOnClickListener AutoLinkOnClickListener;
        private Dictionary<string, string> UserId;
        private StTools.XAutoLinkMode[] AutoLinkModes;
        private List<StTools.XAutoLinkMode> MBoldAutoLinkModes;
        private string CustomRegex;
        private bool IsUnderLineEnabled;
        private Color MentionModeColor = DefaultColor;
        private Color HashtagModeColor = DefaultColor;
        private Color UrlModeColor = DefaultColor;
        private Color PhoneModeColor = DefaultColor;
        private Color EmailModeColor = DefaultColor;
        private Color CustomModeColor = DefaultColor;
        private Color DefaultSelectedColor = Color.LightGray;

        protected SuperTextView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            Init();
        }

        public SuperTextView(Context context) : base(context)
        {
            Init();
        }

        public SuperTextView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Init();
        }

        public SuperTextView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            Init();
        }

        private void Init()
        {
            try
            {
                AddAutoLinkMode(new[] { StTools.XAutoLinkMode.ModePhone, StTools.XAutoLinkMode.ModeEmail, StTools.XAutoLinkMode.ModeHashTag, StTools.XAutoLinkMode.ModeUrl, StTools.XAutoLinkMode.ModeMention, StTools.XAutoLinkMode.ModeCustom });
                SetPhoneModeColor(Color.ParseColor("#008000"));
                SetEmailModeColor(Color.ParseColor("#3e0e4c"));
                SetHashtagModeColor(Color.ParseColor("#0000ff"));
                SetUrlModeColor(Color.ParseColor("#df541e"));
                SetMentionModeColor(Color.ParseColor(AppSettings.MainColor));
                //EnableUnderLine();
                //SetAutoLinkOnClickListener(this); 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void SetTextInfo(SuperTextView showMore)
        {
            try
            {
                if (showMore != null)
                {
                    showMore.AddAutoLinkMode(new[] { StTools.XAutoLinkMode.ModePhone, StTools.XAutoLinkMode.ModeEmail, StTools.XAutoLinkMode.ModeHashTag, StTools.XAutoLinkMode.ModeUrl, StTools.XAutoLinkMode.ModeMention, StTools.XAutoLinkMode.ModeCustom });
                    showMore.SetPhoneModeColor(Color.ParseColor("#008000"));
                    showMore.SetEmailModeColor(Color.ParseColor("#3e0e4c"));
                    showMore.SetHashtagModeColor(Color.ParseColor("#0000ff"));
                    showMore.SetUrlModeColor(Color.ParseColor("#df541e"));
                    showMore.SetMentionModeColor(Color.ParseColor(AppSettings.MainColor));
                    //showMore.EnableUnderLine();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override void SetText(ICharSequence text, BufferType type)
        {
            try
            {
                if (TextUtils.IsEmpty(text))
                {
                    base.SetText(text, type);
                    return;
                }

                SpannableString spannableString = MakeSpannAbleString(text);
                MovementMethod = (new XLinkTouchMovementMethod());

                base.SetText(spannableString, type);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                base.SetText(text, type);
            }
        }

        private SpannableString MakeSpannAbleString(ICharSequence text)
        {
            try
            {
                SpannableString spendableString = new SpannableString(text);

                List<StTools.XAutoLinkItem> autoLinkItems = MatchedRanges(text);

                foreach (StTools.XAutoLinkItem autoLinkItem in autoLinkItems)
                {
                    Color currentColor = GetColorByMode(autoLinkItem.GetAutoLinkMode());
                    XTouchableSpan clickAbleSpan = new XTouchableSpan(this, autoLinkItem, currentColor, DefaultSelectedColor, IsUnderLineEnabled);

                    spendableString.SetSpan(clickAbleSpan, autoLinkItem.GetStartPoint(), autoLinkItem.GetEndPoint(), SpanTypes.ExclusiveExclusive);

                    // check if we should make this auto link item bold
                    if (MBoldAutoLinkModes != null && MBoldAutoLinkModes.Contains(autoLinkItem.GetAutoLinkMode()))
                    {
                        // make the auto link item bold
                        spendableString.SetSpan(new StyleSpan(TypefaceStyle.Bold), autoLinkItem.GetStartPoint(), autoLinkItem.GetEndPoint(), SpanTypes.ExclusiveExclusive);
                    }
                }
                return spendableString;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                SpannableString spendableString = new SpannableString(text);
                return spendableString;
            }
        }

        private List<StTools.XAutoLinkItem> MatchedRanges(ICharSequence text)
        {
            try
            {
                List<StTools.XAutoLinkItem> autoLinkItems = new List<StTools.XAutoLinkItem>();

                if (AutoLinkModes == null)
                {
                    Init();
                    //throw new NullPointerException("Please add at least one mode");
                }

                foreach (StTools.XAutoLinkMode anAutoLinkMode in AutoLinkModes)
                {
                    string regex = StTools.XUtils.GetRegexByAutoLinkMode(anAutoLinkMode, CustomRegex);
                   
                    if (regex.Length <= 0)
                        continue;
                    
                    Pattern pattern = Pattern.Compile(regex);
                    Matcher matcher = pattern.Matcher(text);
                   
                    if (anAutoLinkMode == StTools.XAutoLinkMode.ModePhone)
                    {
                        while (matcher.Find())
                        {
                            StTools.XAutoLinkItem ss = new StTools.XAutoLinkItem(matcher.Start(), matcher.End(), matcher.Group(), anAutoLinkMode, UserId);

                            if (matcher.Group().Length > MinPhoneNumberLength)
                            {
                                autoLinkItems.Add(ss);
                            }
                        }
                    }
                    else
                    {
                        while (matcher.Find())
                        {
                            autoLinkItems.Add(new StTools.XAutoLinkItem(matcher.Start(), matcher.End(), matcher.Group(), anAutoLinkMode, UserId));
                        }
                    }
                }

                return autoLinkItems;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new List<StTools.XAutoLinkItem>();
            }
        }

        private Color GetColorByMode(StTools.XAutoLinkMode autoLinkMode)
        {
            try
            {
                if (autoLinkMode == StTools.XAutoLinkMode.ModeHashTag)
                {
                    return HashtagModeColor;
                }

                if (autoLinkMode == StTools.XAutoLinkMode.ModeMention)
                {
                    return MentionModeColor;
                }

                if (autoLinkMode == StTools.XAutoLinkMode.ModePhone)
                {
                    return PhoneModeColor;
                }

                if (autoLinkMode == StTools.XAutoLinkMode.ModeEmail)
                {
                    return EmailModeColor;
                }

                if (autoLinkMode == StTools.XAutoLinkMode.ModeUrl)
                {
                    return UrlModeColor;
                }

                return CustomModeColor;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return CustomModeColor;
            }
        }

        public void SetMentionModeColor(Color mentionModeColor)
        {
            try
            {
                MentionModeColor = mentionModeColor;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void SetHashtagModeColor(Color hashtagModeColor)
        {
            try
            {
                HashtagModeColor = hashtagModeColor;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void SetUrlModeColor(Color urlModeColor)
        {
            try
            {
                UrlModeColor = urlModeColor;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void SetPhoneModeColor(Color phoneModeColor)
        {
            try
            {
                PhoneModeColor = phoneModeColor;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void SetEmailModeColor(Color emailModeColor)
        {
            try
            {
                EmailModeColor = emailModeColor;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void SetCustomModeColor(Color customModeColor)
        {
            try
            {
                CustomModeColor = customModeColor;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void SetSelectedStateColor(Color defaultSelectedColor)
        {
            try
            {
                DefaultSelectedColor = defaultSelectedColor;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void AddAutoLinkMode(StTools.XAutoLinkMode[] autoLinkModes)
        {
            try
            {
                AutoLinkModes = autoLinkModes;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void SetBoldAutoLinkModes(List<StTools.XAutoLinkMode> autoLinkModes)
        {
            try
            {
                MBoldAutoLinkModes = new List<StTools.XAutoLinkMode>();

                foreach (StTools.XAutoLinkMode autoLinkMode in autoLinkModes)
                {
                    MBoldAutoLinkModes.Add(autoLinkMode);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void SetCustomRegex(string regex)
        {
            try
            {
                CustomRegex = regex;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void SetAutoLinkOnClickListener(StTools.IXAutoLinkOnClickListener autoLinkOnClickListener, Dictionary<string, string> userId)
        {
            try
            {
                AutoLinkOnClickListener = autoLinkOnClickListener;
                UserId = userId;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void EnableUnderLine()
        {
            IsUnderLineEnabled = true;
        }


        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            try
            {
                if (Build.VERSION.SdkInt >= (BuildVersionCodes)16)
                {
                    StaticLayout layout = null;
                    Field field = null;

                    Class klass = Class.FromType(typeof(DynamicLayout));

                    try
                    {
                        Field insetsDirtyField = klass.GetDeclaredField("sStaticLayout");

                        insetsDirtyField.Accessible = (true);
                        layout = (StaticLayout)insetsDirtyField.Get(klass);

                    }
                    catch (NoSuchFieldException ex)
                    {
                        Console.WriteLine(ex);
                    }
                    catch (IllegalAccessException ex)
                    {
                        Console.WriteLine(ex);
                    }

                    if (layout != null)
                    {
                        try
                        {
                            Field insetsDirtyField = klass.GetDeclaredField("sStaticLayout");

                            field = layout.Class.GetDeclaredField("mMaximumVisibleLineCount");
                            field.Accessible = (true);
                            field.SetInt(layout, MaxLines);
                        }
                        catch (NoSuchFieldException e)
                        {
                            Console.WriteLine(e);
                        }
                        catch (IllegalAccessException e)
                        {
                            Console.WriteLine(e);
                        }
                    }

                    base.OnMeasure(widthMeasureSpec, heightMeasureSpec);

                    if (layout != null && field != null)
                    {
                        try
                        {
                            field.SetInt(layout, Integer.MaxValue);
                        }
                        catch (IllegalAccessException e)
                        {
                            Console.WriteLine(e);
                        }
                    }
                }
                else
                {
                    base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}