using System.Collections.Generic;

namespace UpcomingMoviesAndroid.Models
{
    public class MovieModel
    {
        public string PosterPath { get; set; }
        public bool Adult { get; set; }
        public string Overview { get; set; }
        public string ReleaseDate { get; set; }
        public List<int> GenreIds { get; set; }
        public int Id { get; set; }
        public string OriginalTitle { get; set; }
        public string OriginalLanguage { get; set; }
        public string Title { get; set; }
        public string BackdropPath { get; set; }
        public double Popularity { get; set; }
        public int VoteCount { get; set; }
        [RestSharp.Deserializers.DeserializeAs(Name = "video")]
        public bool HasVideo { get; set; }
        public double VoteAverage { get; set; }
        public byte[] MovieImage { get; set; }
        public List<GenreModel> Genres { get; set; }
        //TODO: Dates property
    }
}