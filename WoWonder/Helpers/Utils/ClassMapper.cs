using System;
using AutoMapper;
using AutoMapper.Configuration;
using WoWonder.Activities.Comment.Adapters;
using WoWonder.SQLite;
using WoWonderClient.Classes.Comments;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Movies;

namespace WoWonder.Helpers.Utils
{
    public static class ClassMapper
    {
        public static void SetMappers()
        {
            try
            {
                var cfg = new MapperConfigurationExpression
                {
                    AllowNullCollections = true
                };

                cfg.CreateMap<CommentObjectExtra, GetCommentObject>();
                cfg.CreateMap<GetCommentObject, CommentObjectExtra>();
                 
                cfg.CreateMap<GetSiteSettingsObject.ConfigObject, DataTables.SettingsTb>().ForMember(x => x.AutoIdSettings, opt => opt.Ignore());
                cfg.CreateMap<UserDataObject, DataTables.MyContactsTb>().ForMember(x => x.AutoIdMyFollowing, opt => opt.Ignore());
                cfg.CreateMap<UserDataObject, DataTables.MyFollowersTb>().ForMember(x => x.AutoIdMyFollowers, opt => opt.Ignore());
                cfg.CreateMap<UserDataObject, DataTables.MyProfileTb>().ForMember(x => x.AutoIdMyProfile, opt => opt.Ignore());
                cfg.CreateMap<GetMoviesObject.Movie, DataTables.WatchOfflineVideosTb>().ForMember(x => x.AutoIdWatchOfflineVideos, opt => opt.Ignore());
                cfg.CreateMap<GiftObject.DataGiftObject, DataTables.GiftsTb>().ForMember(x => x.AutoIdGift, opt => opt.Ignore());

                Mapper.Initialize(cfg);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}