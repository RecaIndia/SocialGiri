using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Webkit;

namespace WoWonder.Library.UI
{
    public class WoWebView : WebView
    {
        protected WoWebView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public WoWebView(Context context) : base(context)
        {
            Init();
        }

        public WoWebView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Init();
        }

        public WoWebView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            Init();
        }

        public WoWebView(Context context, IAttributeSet attrs, int defStyleAttr, bool privateBrowsing) : base(context, attrs, defStyleAttr, privateBrowsing)
        {
            Init();
        }

        public WoWebView(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            Init();
        }

        private void Init()
        {
            try
            {
                Settings.SetLayoutAlgorithm(WebSettings.LayoutAlgorithm.NarrowColumns);
                Settings.SetLayoutAlgorithm(WebSettings.LayoutAlgorithm.NarrowColumns);
                Settings.JavaScriptEnabled = true;
                Settings.LoadsImagesAutomatically = true;
                Settings.SetAppCacheEnabled(true);
                Settings.JavaScriptCanOpenWindowsAutomatically = true;
                Settings.DomStorageEnabled = true;
                Settings.AllowFileAccess = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}