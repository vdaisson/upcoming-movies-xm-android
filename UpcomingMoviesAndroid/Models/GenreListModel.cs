using System.Collections.Generic;

namespace UpcomingMoviesAndroid.Models
{
    public class GenreListModel : CacheableModel
    {
        public List<GenreModel> Genres { get; set; }
        public override bool IsCached { get; set; }
    }
}