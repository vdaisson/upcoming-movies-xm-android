using System.Collections.Generic;

namespace UpcomingMoviesAndroid.Models
{
    public class MovieDetailsModel : CacheableModel
    {
        public bool Adult { get; set; }
        public string BackdropPath { get; set; }
        public int Budget { get; set; }
        public List<GenreModel> Genres { get; set; }
        public string HomePage { get; set; }
        public int Id { get; set; }
        public string ImdbId { get; set; }
        public string OriginalLanguage { get; set; }
        public string OriginalTitle { get; set; }
        public string Overview { get; set; }
        public double Popularity { get; set; }
        public string PosterPath { get; set; }
        public string ReleaseDate { get; set; }
        public int Revenue { get; set; }
        public int Runtime { get; set; }
        public string Status { get; set; }
        public string Tagline { get; set; }
        public string Title { get; set; }
        [RestSharp.Deserializers.DeserializeAs(Name = "video")]
        public bool HasVideo { get; set; }
        public double VoteAverage { get; set; }
        public int VoteCount { get; set; }
        public byte[] MovieImage { get; set; }
        public override bool IsCached { get; set; }

        //TODO: ProducionCompanies, ProductionCountries, BelongsToCollection, SpokenLanguages properties
    }
}