using Newtonsoft.Json;
using WoWonderClient.Classes.Posts;

namespace WoWonder.Helpers.Model
{
    public class AddPostObject
    {
        [JsonProperty("api_status")]
        public string ApiStatus { get; set; }

        [JsonProperty("api_text")]
        public string ApiText { get; set; }

        [JsonProperty("api_version")]
        public string ApiVersion { get; set; }

        [JsonProperty("post_data")]
        public PostDataObject PostData { get; set; }
    }
}