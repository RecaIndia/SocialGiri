using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Android.Gms.Maps.Model;
using Android.Icu.Util;
using Android.Locations;
using WoWonder.Activities.NativePost.Post;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Fonts;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonderClient;
using WoWonderClient.Classes.Posts;
using Calendar = Android.Icu.Util.Calendar;

namespace WoWonder.Activities.NativePost.Extra
{
    public class PostModelResolver
    {
        private readonly Context MainContext;
        private readonly PostModelType PostFeedType;

        public PostModelResolver(Context mainContext, PostModelType postFeedType)
        {
            MainContext = mainContext;
            PostFeedType = postFeedType;
        }

        public void PrepareHeader(PostDataObject item)
        {
            var time = item.Time;
            bool success = int.TryParse(time, out var number);
            item.Time = success ? Methods.Time.TimeAgo(number) : time;

            item.Name = Methods.FunString.DecodeString(item.Name);
        }

        public void PrepareTextSection(PostDataObject item)
        {
            item.Orginaltext = Methods.FunString.DecodeString(item.Orginaltext);
        }

        public void PreparePostPrevBottom(PostDataObject item)
        {
            if (AppSettings.PostButton == PostButtonSystem.ReactionDefault || AppSettings.PostButton == PostButtonSystem.ReactionSubShine)
                item.PostLikes = item.Reaction?.Count == null ? "0" + " " + MainContext.GetString(Resource.String.Btn_Likes) : item.Reaction?.Count + " " + MainContext.GetString(Resource.String.Btn_Likes);
            else
                item.PostLikes = Methods.FunString.FormatPriceValue(int.Parse(item.PostLikes));

            item.PrevButtonViewText = PostFeedType switch
            {
                PostModelType.VideoPost => item.VideoViews + " " + MainContext.GetString(Resource.String.Lbl_Views),
                PostModelType.PollPost => item.Options[0]?.All + " " + MainContext.GetString(Resource.String.Lbl_votesPost),
                _ => item.PrevButtonViewText
            };
        }

        public void PrepareBlog(PostDataObject item)
        {

            if (!string.IsNullOrEmpty(item.Blog?.Title))
            {
                var prepareTitle = Methods.FunString.DecodeString(Methods.FunString.SubStringCutOf(item.Blog?.Title, 60));
                item.Blog.Title = prepareTitle;

            }

            if (!string.IsNullOrEmpty(item.Blog?.Description))
            {
                var prepareDescription = Methods.FunString.DecodeString(Methods.FunString.SubStringCutOf(item.Blog?.Title, 120));
                item.Blog.Description = prepareDescription;
            }

            if (!string.IsNullOrEmpty(item.Blog?.CategoryName))
            {
                item.Blog.CategoryName = item.Blog?.CategoryName;
            }

        }

        public void PrepareColorBox(PostDataObject item)
        {
            if (ListUtils.SettingsSiteList?.PostColors != null && ListUtils.SettingsSiteList?.PostColors.Value.PostColorsList != null)
            {

                var getColorObject = ListUtils.SettingsSiteList.PostColors.Value.PostColorsList.FirstOrDefault(a => a.Key == item.ColorId);

                if (getColorObject.Value != null)
                {
                    if (!string.IsNullOrEmpty(getColorObject.Value.Image))
                    {
                        item.ColorBoxImageUrl = getColorObject.Value.Image;
                    }
                    else
                    {
                        var colorsList = new List<int>();

                        if (!string.IsNullOrEmpty(getColorObject.Value.Color1))
                            colorsList.Add(Color.ParseColor(getColorObject.Value.Color1));

                        if (!string.IsNullOrEmpty(getColorObject.Value.Color2))
                            colorsList.Add(Color.ParseColor(getColorObject.Value.Color2));

                        item.ColorBoxGradientDrawable = new GradientDrawable(GradientDrawable.Orientation.TopBottom, colorsList.ToArray());
                        item.ColorBoxGradientDrawable.SetCornerRadius(0f);
                    }

                    item.ColorBoxTextColor = Color.ParseColor(getColorObject.Value.TextColor);
                }
            }
        }

        public void PrepareEvent(PostDataObject item)
        {
            if (!string.IsNullOrEmpty(item.Event?.EventClass?.Name))
            {
                var prepareTitle = Methods.FunString.DecodeString(Methods.FunString.SubStringCutOf(item.Event?.EventClass?.Name, 100));
                item.Event.Value.EventClass.Name = prepareTitle;

            }
            if (!string.IsNullOrEmpty(item.Event?.EventClass?.Description))
            {
                var prepareDescription = Methods.FunString.DecodeString(Methods.FunString.SubStringCutOf(item.Event?.EventClass?.Description, 100));
                item.Event.Value.EventClass.Description = prepareDescription;
            }

            if (!string.IsNullOrEmpty(item.Event?.EventClass?.Location))
            {
                var prepareLocation = Methods.FunString.DecodeString(Methods.FunString.SubStringCutOf(item.Event?.EventClass?.Location, 60));
                item.Event.Value.EventClass.Location = prepareLocation;
            }
        }

        public void PrepareLink(PostDataObject item)
        {
            if (!string.IsNullOrEmpty(item.PostLink))
            {
                var prepareUrl = item.PostLink.Replace("https://", "").Replace("http://", "").Split('/').FirstOrDefault();
                item.PostLink = prepareUrl;

            }
            if (!string.IsNullOrEmpty(item.PostLinkContent))
            {
                var prepareDescription = Methods.FunString.DecodeString(Methods.FunString.SubStringCutOf(item.PostLinkContent, 100));
                item.PostLinkContent = prepareDescription;
            }
            if (!string.IsNullOrEmpty(item.PostLinkTitle))
            {
                var prepareTitle = Methods.FunString.DecodeString(Methods.FunString.SubStringCutOf(item.PostLinkTitle, 100));
                item.PostLinkTitle = prepareTitle;
            }
        }

        public void PrepareFunding(PostDataObject item)
        {
            if (item.FundData != null)
            {
                if (!string.IsNullOrEmpty(item.FundData.Value.FundDataClass?.Title))
                {
                    var prepareTitle = Methods.FunString.DecodeString(Methods.FunString.SubStringCutOf(item.FundData.Value.FundDataClass?.Title, 100));
                    item.FundData.Value.FundDataClass.Title = prepareTitle;
                }

                if (!string.IsNullOrEmpty(item.FundData.Value.FundDataClass?.Time))
                {
                    var prepareTime = Methods.Time.TimeAgo(int.Parse(item.FundData.Value.FundDataClass.Time));
                    item.FundData.Value.FundDataClass.Time = prepareTime;
                }
                if (!string.IsNullOrEmpty(item.FundData.Value.FundDataClass?.Description))
                {
                    var prepareDescription = Methods.FunString.DecodeString(Methods.FunString.SubStringCutOf(item.FundData.Value.FundDataClass?.Description, 100));
                    item.FundData.Value.FundDataClass.Description = prepareDescription;
                }
                else
                {
                    if (item.FundData.Value.FundDataClass != null)
                        item.FundData.Value.FundDataClass.Description = MainContext.GetText(Resource.String.Lbl_NoAnyDescription);
                }

                try
                {
                    if (!string.IsNullOrEmpty(item.FundData.Value.FundDataClass?.Raised))
                    {
                        decimal d = decimal.Parse(item.FundData.Value.FundDataClass?.Raised, CultureInfo.InvariantCulture);
                        item.FundData.Value.FundDataClass.Raised = AppSettings.CurrencyFundingPriceStatic + d.ToString("0.00");
                    }

                    if (!string.IsNullOrEmpty(item.FundData.Value.FundDataClass?.Amount))
                    {
                        decimal d = decimal.Parse(item.FundData.Value.FundDataClass?.Amount, CultureInfo.InvariantCulture);
                        item.FundData.Value.FundDataClass.Amount = AppSettings.CurrencyFundingPriceStatic + d.ToString("0.00");
                    }
                    if (!string.IsNullOrEmpty(item.FundData.Value.FundDataClass?.Bar))
                    {
                        decimal dBar = decimal.Parse(item.FundData.Value.FundDataClass.Bar, CultureInfo.InvariantCulture);
                        item.FundData.Value.FundDataClass.Bar = dBar.ToString("0");

                    }
                }
                catch (Exception)
                {
                    if (!string.IsNullOrEmpty(item.FundData.Value.FundDataClass?.Raised))
                        item.FundData.Value.FundDataClass.Raised = AppSettings.CurrencyFundingPriceStatic + item.FundData.Value.FundDataClass.Raised;

                    if (!string.IsNullOrEmpty(item.FundData.Value.FundDataClass?.Amount))
                        item.FundData.Value.FundDataClass.Amount = AppSettings.CurrencyFundingPriceStatic + item.FundData.Value.FundDataClass.Amount;

                    if (!string.IsNullOrEmpty(item.FundData.Value.FundDataClass?.Bar))
                        item.FundData.Value.FundDataClass.Bar = item.FundData.Value.FundDataClass.Bar;
                }
            }
        }

        public void PrepareProduct(PostDataObject item)
        {

            if (item.Product != null && item.Product.Value.ProductClass?.Seller == null)
                if (item.Product.Value.ProductClass != null)
                    item.Product.Value.ProductClass.Seller = item.Publisher;

            if (item.Product?.ProductClass != null)
            {
                if (!string.IsNullOrEmpty(item.Product.Value.ProductClass?.Location))
                {
                    item.Product.Value.ProductClass.LocationDecodedText = item.Product.Value.ProductClass?.Location;
                    var prepareLocation = Methods.FunString.DecodeString(Methods.FunString.SubStringCutOf(item.Product.Value.ProductClass?.Location, 100));
                    item.Product.Value.ProductClass.LocationDecodedText = prepareLocation;
                }

                if (!string.IsNullOrEmpty(item.Product.Value.ProductClass?.Name))
                {
                    var prepareTitle = Methods.FunString.DecodeString(Methods.FunString.SubStringCutOf(item.Product.Value.ProductClass?.Name, 100));
                    item.Product.Value.ProductClass.Name = prepareTitle;
                }

                if (!string.IsNullOrEmpty(item.Product.Value.ProductClass?.Description))
                {
                    var prepareDescription = Methods.FunString.DecodeString(Methods.FunString.SubStringCutOf(item.Product.Value.ProductClass?.Description, 100));
                    item.Product.Value.ProductClass.Description = prepareDescription;
                }

                var (currency, currencyIcon) = WoWonderTools.GetCurrency(item.Product.Value.ProductClass?.Currency);
                Console.WriteLine(currency);
                if (item.Product.Value.ProductClass != null)
                {
                    item.Product.Value.ProductClass.CurrencyText = currencyIcon + " " + item.Product.Value.ProductClass?.Price;
                    item.Product.Value.ProductClass.TypeDecodedText = MainContext.GetString(item.Product.Value.ProductClass?.Type == "0" ? Resource.String.Radio_New : Resource.String.Radio_Used); //TODO Should be array from API
                    item.Product.Value.ProductClass.StatusDecodedText = item.Product.Value.ProductClass?.Status == "0" ? MainContext.GetString(Resource.String.Lbl_In_Stock) : " "; //TODO Should be array from API
                }
            }
        }

        public void PrepareOffer(PostDataObject item)
        {
            if (!string.IsNullOrEmpty(item.Offer?.OfferClass?.OfferText))
            {
                var prepareOfferText = Methods.FunString.DecodeString(Methods.FunString.SubStringCutOf(item.Offer?.OfferClass?.OfferText, 100));
                item.Offer.Value.OfferClass.OfferText = prepareOfferText;
            }
            if (!string.IsNullOrEmpty(item.Offer?.OfferClass?.Description))
            {
                var prepareDescription = Methods.FunString.DecodeString(Methods.FunString.SubStringCutOf(item.Offer?.OfferClass?.Description, 100));
                item.Offer.Value.OfferClass.Description = prepareDescription;
            }
        }

        public async void PrepareMapPost(PostDataObject item)
        {
            try
            {
                if (!item.PostMap.Contains("https://maps.googleapis.com/maps/api/staticmap?"))
                {
                    string imageUrlMap = "https://maps.googleapis.com/maps/api/staticmap?";
                    //imageUrlMap += "center=" + item.CurrentLatitude + "," + item.CurrentLongitude;
                    imageUrlMap += "center=" + item.PostMap.Replace("/", "");
                    imageUrlMap += "&zoom=10";
                    imageUrlMap += "&scale=1";
                    imageUrlMap += "&size=300x300";
                    imageUrlMap += "&maptype=roadmap";
                    imageUrlMap += "&key=" + MainContext.GetText(Resource.String.google_maps_key);
                    imageUrlMap += "&format=png";
                    imageUrlMap += "&visual_refresh=true";
                    imageUrlMap += "&markers=size:small|color:0xff0000|label:1|" + item.PostMap.Replace("/", "");

                    item.ImageUrlMap = imageUrlMap;
                }
                else
                {
                    item.ImageUrlMap = item.PostMap;
                }

                var latLng = await GetLocationFromAddress(item.PostMap).ConfigureAwait(false);
                if (latLng != null)
                {
                    item.CurrentLatitude = latLng.Latitude;
                    item.CurrentLongitude = latLng.Longitude;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #region Location >> BindMapPost

        private async Task<LatLng> GetLocationFromAddress(string strAddress)
        {
            var locale = MainContext.Resources.Configuration.Locale;
            Geocoder coder = new Geocoder(MainContext, locale);

            try
            {
                var address = await coder.GetFromLocationNameAsync(strAddress, 2);
                if (address == null)
                    return null;

                Address location = address[0];
                var lat = location.Latitude;
                var lng = location.Longitude;

                LatLng p1 = new LatLng(lat, lng);

                return p1;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        #endregion

        public void PrepareAds(PostDataObject item)
        {
            if (item.UserData != null)
            {
                item.Name = Methods.FunString.DecodeString(item.Name);
                item.Posted = Methods.Time.TimeAgo(int.Parse(item.Posted));

                item.Location = Methods.FunString.DecodeString(item.Location);
                item.Headline = Methods.FunString.DecodeString(item.Headline);
                item.Url = item.Url?.Replace("https://", "").Replace("http://", "").Split('/').FirstOrDefault();
            }
        }

        public void PrepareJob(PostDataObject item)
        {
            if (item.Job != null)
            {
                if (!item.Job.Value.JobInfoClass.Image.Contains(Client.WebsiteUrl))
                    item.Job.Value.JobInfoClass.Image = Client.WebsiteUrl + "/" + item.Job.Value.JobInfoClass.Image;

                item.Job.Value.JobInfoClass.IsOwner = item.Job.Value.JobInfoClass.UserId == UserDetails.UserId;


                if (!string.IsNullOrEmpty(item.Job.Value.JobInfoClass.Title))
                {
                    var prepareTitle = Methods.FunString.DecodeString(Methods.FunString.SubStringCutOf(item.Job.Value.JobInfoClass.Title, 100));
                    item.Job.Value.JobInfoClass.Title = prepareTitle;
                }
                if (!string.IsNullOrEmpty(item.Job?.JobInfoClass.Description))
                {
                    var prepareDescription = Methods.FunString.DecodeString(Methods.FunString.SubStringCutOf(item.Job?.JobInfoClass.Description, 200));
                    item.Job.Value.JobInfoClass.Description = prepareDescription;
                }

                if (item.Job.Value.JobInfoClass.Page != null)
                {
                    var prepareName = "@" + Methods.FunString.DecodeString(Methods.FunString.SubStringCutOf(item.Job.Value.JobInfoClass.Page.PageName, 100));
                    item.Job.Value.JobInfoClass.Page.PageName = prepareName;

                    if (item.Job.Value.JobInfoClass.Page.IsPageOnwer != null && item.Job.Value.JobInfoClass.Page.IsPageOnwer.Value)
                        item.Job.Value.JobInfoClass.ButtonText = MainContext.GetString(Resource.String.Lbl_show_applies) + " (" + item.Job.Value.JobInfoClass.ApplyCount + ")";
                }

                if (item.Job.Value.JobInfoClass.Apply == "true")
                {
                    var prepare = MainContext.GetString(Resource.String.Lbl_already_applied);
                    item.Job.Value.JobInfoClass.ButtonText = prepare;
                }

                if (!string.IsNullOrEmpty(item.Job.Value.JobInfoClass.Time))
                {
                    var prepareTime = Methods.Time.TimeAgo(int.Parse(item.Job.Value.JobInfoClass.Time));
                    item.Job.Value.JobInfoClass.Time = prepareTime;
                }

                if (!string.IsNullOrEmpty(item.Job.Value.JobInfoClass.Location))
                {
                    item.Job.Value.JobInfoClass.LocationDecodedText = item.Product?.ProductClass?.Location;
                    var prepareLocation = Methods.FunString.DecodeString(Methods.FunString.SubStringCutOf(item.Product?.ProductClass?.Location, 100));
                    item.Job.Value.JobInfoClass.LocationDecodedText = prepareLocation;
                }

                //Set Salary Date
                if (!string.IsNullOrEmpty(item.Job.Value.JobInfoClass.SalaryDate))
                {
                    var salaryDate = item.Job.Value.JobInfoClass.SalaryDate switch
                    {
                        "per_hour" => MainContext.GetString(Resource.String.Lbl_per_hour),
                        "per_day" => MainContext.GetString(Resource.String.Lbl_per_day),
                        "per_week" => MainContext.GetString(Resource.String.Lbl_per_week),
                        "per_month" => MainContext.GetString(Resource.String.Lbl_per_month),
                        "per_year" => MainContext.GetString(Resource.String.Lbl_per_year),
                        _ => MainContext.GetString(Resource.String.Lbl_Unknown)
                    };

                    item.Job.Value.JobInfoClass.SalaryDate = salaryDate;
                }

                if (!string.IsNullOrEmpty(item.Job.Value.JobInfoClass.JobType))
                {
                    var jobInfo = item.Job.Value.JobInfoClass.JobType switch
                    {
                        //Set job type
                        "full_time" => (IonIconsFonts.IosBriefcase + " " + MainContext.GetString(Resource.String.Lbl_full_time)),
                        "part_time" => (IonIconsFonts.IosBriefcase + " " + MainContext.GetString(Resource.String.Lbl_part_time)),
                        "internship" => (IonIconsFonts.IosBriefcase + " " + MainContext.GetString(Resource.String.Lbl_internship)),
                        "volunteer" => (IonIconsFonts.IosBriefcase + " " + MainContext.GetString(Resource.String.Lbl_volunteer)),
                        "contract" => (IonIconsFonts.IosBriefcase + " " + MainContext.GetString(Resource.String.Lbl_contract)),
                        _ => (IonIconsFonts.IosBriefcase + " " + MainContext.GetString(Resource.String.Lbl_Unknown))
                    };

                    item.Job.Value.JobInfoClass.JobType = jobInfo;
                }


                var categoryName = CategoriesController.ListCategoriesJob.FirstOrDefault(categories => categories.CategoriesId == item.Job.Value.JobInfoClass.Category)?.CategoriesName;
                item.Job.Value.JobInfoClass.Category += " " + " " + IonIconsFonts.Pricetag + " " + categoryName;

            }
        }

        public void PreparePoll(PollsOptionObject poll)
        {
            if (!string.IsNullOrEmpty(poll.Text))
            {
                var prepareText = Methods.FunString.DecodeString(Methods.FunString.SubStringCutOf(poll.Text, 100));
                poll.Text = prepareText;
            }
        }

        public AdapterModelsClass PrepareGreetingAlert()
        {
            try
            {
                //afternoon_system
                var afternoonSystem = ListUtils.SettingsSiteList?.AfternoonSystem;
                if (afternoonSystem == "1")
                {
                    var data = ListUtils.MyProfileList.FirstOrDefault();
                    string name = data != null ? WoWonderTools.GetNameFinal(data) : UserDetails.Username;

                    var alertModel = new AlertModelClass();

                    var c = Calendar.Instance;
                    var timeOfDay = c.Get(CalendarField.HourOfDay);

                    if (timeOfDay >= 0 && timeOfDay < 12)
                    {
                        alertModel = new AlertModelClass
                        {
                            TitleHead = MainContext.GetString(Resource.String.Lbl_GoodMorning) + ", " + name,
                            SubText = MainContext.GetString(Resource.String.Lbl_GoodMorning_Text),
                            LinerColor = "#ffc107",
                            ImageDrawable = Resource.Drawable.ic_post_park
                        };
                    }
                    else if (timeOfDay >= 12 && timeOfDay < 16)
                    {
                        alertModel = new AlertModelClass
                        {
                            TitleHead = MainContext.GetString(Resource.String.Lbl_GoodAfternoon) + ", " + name,
                            SubText = MainContext.GetString(Resource.String.Lbl_GoodAfternoon_Text),
                            LinerColor = "#ffc107",
                            ImageDrawable = Resource.Drawable.ic_post_desert
                        };
                    }
                    else if (timeOfDay >= 16 && timeOfDay < 21)
                    {
                        alertModel = new AlertModelClass
                        {
                            TitleHead = MainContext.GetString(Resource.String.Lbl_GoodEvening) + ", " + name,
                            SubText = MainContext.GetString(Resource.String.Lbl_GoodEvening_Text),
                            LinerColor = "#ffc107",
                            ImageDrawable = Resource.Drawable.ic_post_sea
                        };
                    }
                    else if (timeOfDay >= 21 && timeOfDay < 24)
                    {
                        alertModel = new AlertModelClass
                        {
                            TitleHead = MainContext.GetString(Resource.String.Lbl_GoodEvening) + ", " + name,
                            SubText = MainContext.GetString(Resource.String.Lbl_GoodEvening_Text),
                            LinerColor = "#ffc107",
                            ImageDrawable = Resource.Drawable.ic_post_sea
                        };
                    }

                    var alertBox = new AdapterModelsClass
                    {
                        TypeView = PostModelType.AlertBox,
                        AlertModel = alertModel,
                        Id = 333333333
                    };

                    return alertBox;

                }
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public AdapterModelsClass SetFindMoreAlert(string type)
        {
            try
            {
                var alertModel1 = new AlertModelClass();
                if (type == "Groups")
                {
                    alertModel1 = new AlertModelClass
                    {
                        TitleHead = MainContext.GetString(Resource.String.Lbl_Groups),
                        SubText = MainContext.GetString(Resource.String.Lbl_FindMoreAler_TextGroups),
                        TypeAlert = "Groups",
                        ImageDrawable = Resource.Drawable.image2,
                        IconImage = Resource.Drawable.shareGroup,
                    };
                }
                else if (type == "Pages")
                {
                    alertModel1 = new AlertModelClass
                    {
                        TitleHead = MainContext.GetString(Resource.String.Lbl_Pages),
                        SubText = MainContext.GetString(Resource.String.Lbl_FindMoreAler_TextPages),
                        TypeAlert = "Pages",
                        ImageDrawable = Resource.Drawable.image1,
                        IconImage = Resource.Drawable.sharePage,
                    };
                }

                var addAlertJoinBox = new AdapterModelsClass
                {
                    TypeView = PostModelType.AlertJoinBox,
                    Id = DateTime.Now.Millisecond,
                    AlertModel = alertModel1
                };

                return addAlertJoinBox;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
    }
}