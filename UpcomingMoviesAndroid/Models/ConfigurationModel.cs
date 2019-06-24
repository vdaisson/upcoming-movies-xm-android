using System.Collections.Generic;


namespace UpcomingMoviesAndroid.Models
{
    public class ConfigurationModel : CacheableModel
    {
        [RestSharp.Deserializers.DeserializeAs(Name = "images")]
        public ImageConfig ImageConfig { get; set; }
        public List<string> ChangeKeys { get; set; }
        public override bool IsCached { get; set; }
    }

    public class ImageConfig
    {
        public string BaseUrl { get; set; }
        public string SecureBaseUrl { get; set; }
        public List<string> BackdropSizes { get; set; }
        public List<string> LogoSizes { get; set; }
        public List<string> PosterSizes { get; set; }
        public List<string> ProfileSizes { get; set; }
        public List<string> StillSizes { get; set; }
    }
}