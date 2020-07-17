using System;
using System.IO;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Database;
using Android.Widget;
using WoWonder.Activities.Videos;
using WoWonder.Helpers.Utils;
using WoWonder.SQLite;
using WoWonderClient.Classes.Movies;

namespace WoWonder.Helpers.Controller
{ 
    public class VideoDownloadAsyncControler
    {
        private readonly DownloadManager Downloadmanager;
        private readonly DownloadManager.Request Request;

        private readonly string FilePath = Methods.Path.FolderDcimMovie;
        private readonly string Filename;
        private long DownloadId;
        private static GetMoviesObject.Movie Video;
        private static Context ContextActivty;

        public VideoDownloadAsyncControler(string url, string filename, Context contextActivty)
        {
            try
            {

                if (!Directory.Exists(FilePath))
                    Directory.CreateDirectory(FilePath);

                if (!filename.Contains(".mp4") || !filename.Contains(".Mp4") || !filename.Contains(".MP4"))
                {
                    Filename = filename + ".mp4";
                }

                ContextActivty = contextActivty;

                Downloadmanager = (DownloadManager)Application.Context.GetSystemService(Context.DownloadService);
                Request = new DownloadManager.Request(Android.Net.Uri.Parse(url));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public void StartDownloadManager(string title, GetMoviesObject.Movie video)
        {
            try
            {
                if (video != null && !string.IsNullOrEmpty(title))
                {
                    Video = video;
                    SqLiteDatabase dbDatabase = new SqLiteDatabase();
                    dbDatabase.Insert_WatchOfflineVideos(Video);
                    dbDatabase.Dispose();

                    Request.SetTitle(title);
                    Request.SetAllowedNetworkTypes(DownloadNetwork.Mobile | DownloadNetwork.Wifi);
                    Request.SetDestinationInExternalPublicDir(FilePath, Filename);
                    Request.SetNotificationVisibility(DownloadVisibility.Visible);
                    Request.SetAllowedOverRoaming(true);
                    //Request.SetVisibleInDownloadsUi(true);
                    DownloadId = Downloadmanager.Enqueue(Request);

                    DownloadComplete onDownloadComplete = new DownloadComplete();
                    Application.Context.ApplicationContext.RegisterReceiver(onDownloadComplete, new IntentFilter(DownloadManager.ActionDownloadComplete));
                }
                else
                {
                    Toast.MakeText(ContextActivty, Application.Context.GetText(Resource.String.Lbl_Download_faileds), ToastLength.Short).Show();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public void StopDownloadManager()
        {
            try
            {
                Downloadmanager.Remove(DownloadId);
                RemoveDiskVideoFile(Filename);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public static bool RemoveDiskVideoFile(string filename)
        {
            try
            {
                string path = Methods.Path.FolderDcimMovie +  filename;
                if (File.Exists(path))
                {
                    SqLiteDatabase dbDatabase = new SqLiteDatabase();
                    dbDatabase.Remove_WatchOfflineVideos(Video.Id);
                    dbDatabase.Dispose();

                    File.Delete(path);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return false;
            }
        }

        public bool CheckDownloadLinkIfExits()
        {
            try
            {
                if (File.Exists(FilePath + Filename))
                    return true;

                return false;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return false;
            }
        }

        public static string GetDownloadedDiskVideoUri(string url)
        {
            try
            {
                string filename = url.Split('/').Last();

                var fullpath = "file://" + Android.Net.Uri.Parse(Methods.Path.FolderDcimMovie +  filename + ".mp4");
                if (File.Exists(fullpath))
                    return fullpath;

                var fullpath2 = Methods.Path.FolderDcimMovie +  filename + ".mp4";
                if (File.Exists(fullpath2))
                    return fullpath2;
                  
                return null;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return null;
            }
        }

        [BroadcastReceiver()]
        [IntentFilter(new[] { DownloadManager.ActionDownloadComplete })]
        public class DownloadComplete : BroadcastReceiver
        {
            public override void OnReceive(Context context, Intent intent)
            {
                try
                {
                    if (intent.Action == DownloadManager.ActionDownloadComplete)
                    {
                        DownloadManager downloadManagerExcuter = (DownloadManager)Application.Context.GetSystemService(Context.DownloadService);
                        long downloadId = intent.GetLongExtra(DownloadManager.ExtraDownloadId, -1);
                        DownloadManager.Query query = new DownloadManager.Query();
                        query.SetFilterById(downloadId);
                        ICursor c = downloadManagerExcuter.InvokeQuery(query);

                        if (c.MoveToFirst())
                        {
                            SqLiteDatabase dbDatabase = new SqLiteDatabase();

                            int columnIndex = c.GetColumnIndex(DownloadManager.ColumnStatus);
                            if (c.GetInt(columnIndex) == (int)DownloadStatus.Successful)
                            {
                                string downloadedPath = c.GetString(c.GetColumnIndex(DownloadManager.ColumnLocalUri));

                                ActivityManager.RunningAppProcessInfo appProcessInfo = new ActivityManager.RunningAppProcessInfo();
                                ActivityManager.GetMyMemoryState(appProcessInfo);
                                if (appProcessInfo.Importance == Importance.Foreground || appProcessInfo.Importance == Importance.Background)
                                {

                                    dbDatabase.Update_WatchOfflineVideos(Video.Id, downloadedPath);
                                    var videoViewer = (VideoViewerActivity) ContextActivty;
                                    if (ContextActivty != null)
                                    {
                                        var fullScreen = ContextActivty as FullScreenVideoActivity;

                                        if (videoViewer != null)
                                        {
                                            if (videoViewer.VideoActionsController.VideoData.Id == Video.Id)
                                            {
                                                //videoViewer.VideoActionsController.DownloadIcon.SetImageDrawable(Application.Context.GetDrawable(Resource.Drawable.ic_checked_red));
                                                //videoViewer.VideoActionsController.DownloadIcon.Tag = "Downloaded";
                                            }
                                        }
                                        else if (fullScreen != null)
                                        {
                                            if (fullScreen.VideoActionsController.VideoData.Id == Video.Id)
                                            {
                                                //fullScreen.VideoActionsController.DownloadIcon.SetImageDrawable(Application.Context.GetDrawable(Resource.Drawable.ic_checked_red));
                                                //fullScreen.VideoActionsController.DownloadIcon.Tag = "Downloaded";
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    dbDatabase.Update_WatchOfflineVideos(Video.Id, downloadedPath);
                                }
                            }
                            dbDatabase.Dispose();
                        }
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
            }
        }
    }
}