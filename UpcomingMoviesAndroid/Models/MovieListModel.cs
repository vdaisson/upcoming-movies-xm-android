using System.Collections.Generic;

namespace UpcomingMoviesAndroid.Models
{
    public class MovieListModel : CacheableModel
    {
        public int Page { get; set; }
        [RestSharp.Deserializers.DeserializeAs(Name = "results")]
        public List<MovieModel> Movies { get; set; }
        public int TotalPages { get; set; }
        public int TotalResults { get; set; }
        public override bool IsCached { get; set; }
    }
}