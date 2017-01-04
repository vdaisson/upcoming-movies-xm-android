using System;
using System.Collections.Generic;

using RestSharp;
using UpcomingMoviesAndroid.Models;
using UpcomingMoviesAndroid.MovieAPI;
using System.Threading.Tasks;
using System.Net.Http;

namespace UpcomingMoviesAndroid.Controllers
{
    /// <summary>
    /// The App core, handles calls to the API and requests from the UI
    /// </summary>
    public sealed class MovieController
    {
        //TODO: Change resource url strings from code to a config file or enum
        //TODO: Get parameter creation out of call methods
        //TODO: Handle repeated movies on result

        private static MovieListModel _movieList = new MovieListModel();
        private static MovieDetailsModel _movieDetails = new MovieDetailsModel();
        private static ConfigurationModel _config = new ConfigurationModel();
        private static GenreListModel _genreList = new GenreListModel();
        private static readonly MovieController _controller = new MovieController();
        private static readonly MovieAPIHandler _apiHandler = new MovieAPIHandler();
        private static readonly HttpClient _httpClient = new HttpClient();

        private static bool _isUpcomingListActive = false;
        private static string _storedQuery;

        // List of movies should have a limit, the user wouldn't like to scroll through thousands of them
        private const int MOVIE_COUNT_LIMIT = 150;

        private const string NOT_VALID_RESPONSE = "Response is not valid";
        
        // Minimum number of movie votes for it to be on the search result (0 returns LOTS of movies)
        private const int VOTE_COUNT_MINIMUM = 3;

        /// <summary>
        /// Whether the App's active listing is from a search or a default upcoming movie list
        /// </summary>
        public bool IsUpcomingListActive
        {
            get { return _isUpcomingListActive; }
        }

        public static MovieController GetMovieController()
        {
            return _controller;
        }

        /// <summary>
        /// Gets configuration information from the API
        /// </summary>
        /// <returns>Model containing API configuration information</returns>
        async public Task<ConfigurationModel> GetConfigAsync()
        {
            try
            {
                // If there's no configuration model cached, queries the API for information
                if (!_config.IsCached)
                {
                    string resourceUrl = "/configuration";

                    IRestResponse<ConfigurationModel> response = await _apiHandler.SendAPIRequestAsync<ConfigurationModel>(resourceUrl, Method.GET);

                    if (IsValidResponse(response))
                    {
                        _config = response.Data;
                        _config.IsCached = true;
                    }
                    else
                        throw new Exception(NOT_VALID_RESPONSE);
                }

                return _config;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a list model containing all(?) the movie genres from the API
        /// </summary>
        /// <returns>List model containg all movie genres used by TMDB</returns>
        async public Task<GenreListModel> GetGenreListAsync()
        {
            try
            {
                if (!_genreList.IsCached)
                {
                    string resourceUrl = "/genre/movie/list";

                    IRestResponse<GenreListModel> response = await _apiHandler.SendAPIRequestAsync<GenreListModel>(resourceUrl, Method.GET);

                    if (IsValidResponse(response))
                    {
                        _genreList = response.Data;
                        _genreList.IsCached = true;
                    }
                    else
                        throw new Exception(NOT_VALID_RESPONSE);
                }

                return _genreList;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Get a list of movies based on a query keyword from the user
        /// </summary>
        /// <param name="query">A simple word or keywords (Ex. 'underworld)'</param>
        /// <param name="nextPage">Whether a new list or a request to get the next page from the searched movie list</param>
        /// <returns>Movie list model containing movie information from the API</returns>
        async public Task<MovieListModel> GetSearchedMovieListAsync(string query, bool nextPage = false)
        {
            try
            {
                // Do a series of checks to make it clear which kind of request it is
                if (!_movieList.IsCached || nextPage || _isUpcomingListActive || !string.Equals(_storedQuery, query))
                {
                    string resourceUrl = "search/movie";

                    IRestResponse<MovieListModel> response;

                    var queryParameter = new RequestParameter { Name = "query", Value = query };

                    // Search queries based on current year
                    // var yearParameter = new RequestParameter { Name = "primary_release_year", Value = $"{DateTime.Now.Year}" };

                    if (nextPage)
                    {
                        int pageToRequest = _movieList.Page + 1;
                        response = await _apiHandler.SendAPIRequestAsync<MovieListModel>(resourceUrl, Method.GET, queryParameter, new RequestParameter { Name = "page", Value = $"{pageToRequest}" }).ConfigureAwait(false);
                    }
                    else
                        response = await _apiHandler.SendAPIRequestAsync<MovieListModel>(resourceUrl, Method.GET, queryParameter).ConfigureAwait(false);

                    if (IsValidResponse(response))
                    {
                        var nextMovieList = response.Data;

                        // If the new search results are too much to handle, sets the current list page to last page so the app won't request any more results from the API
                        if (_movieList.Movies != null && _movieList.Movies.Count + nextMovieList.Movies.Count > MOVIE_COUNT_LIMIT)
                        {
                            _movieList.Page = _movieList.TotalPages;
                            return _movieList;
                        }

                        // Get genre list to fill genre names on movielistmodel (for some reason, the API only returns genre names when you request movie details. Otherwise, it returns genre ids with no names)
                        await FillGenreNamesAsync(nextMovieList).ConfigureAwait(false);

                        // Only image paths are given by the API. Have to get image data via http requests
                        await FillMovieImagesAsync(nextMovieList).ConfigureAwait(false);

                        // Important to check if the search query from the UI is still the same. Otherwise, it's expecting an entire new list
                        if (_movieList.Movies != null && !_isUpcomingListActive && string.Equals(_storedQuery, query))
                        {
                            _movieList.Movies.AddRange(nextMovieList.Movies);
                            _movieList.Page++;
                        }
                        else
                            _movieList = nextMovieList;

                        _movieList.IsCached = true;

                    }
                    else
                    {
                        throw new Exception(NOT_VALID_RESPONSE);
                    }
                }

                return _movieList;
            }
            catch (Exception ex)
            {
                if (_movieList != null)
                    return _movieList;
                else
                    return null;
            }
            finally
            {
                // Caches the last query to check which kind of request is coming next
                _storedQuery = query;
                _isUpcomingListActive = false;
            }
        }

        /// <summary>
        /// Gets the default upcoming movie list based on current day
        /// </summary>
        /// <param name="nextPage">Whether a new list or a request to get the next page from a previous movie list</param>
        /// <returns></returns>
        async public Task<MovieListModel> GetUpcomingMovieListAsync(bool nextPage = false)
        {
            try
            {
                if (!_movieList.IsCached || nextPage || !_isUpcomingListActive)
                {
                    // movie/upcoming is really unreliable, discover is more flexible and has better reult filtering
                    //string resourceUrl = "movie/upcoming";
                    string resourceUrl = "discover/movie";

                    // A lot of filter queries that should be parameterized or something
                    var requestParameters = new List<RequestParameter>();

                    var sortByParameter = new RequestParameter { Name = "sort_by", Value = "popularity.desc" };
                    requestParameters.Add(sortByParameter);

                    // Subtracts one day from current day, to get accurate results from the API
                    var serializer = new RestSharp.Serializers.JsonSerializer();
                    var serializedDate = serializer.Serialize(DateTime.Now.AddDays(-1).Date);
                    var primaryReleaseDateGreaterThanParameter = new RequestParameter { Name = "primary_release_date.gte", Value = $"{serializedDate.Substring(1, serializedDate.Length-2)}" };
                    requestParameters.Add(primaryReleaseDateGreaterThanParameter);

                    var voteCountGreaterThan = new RequestParameter { Name = "vote_count.gte", Value = $"{VOTE_COUNT_MINIMUM}" };
                    requestParameters.Add(voteCountGreaterThan);

                    IRestResponse<MovieListModel> response;

                    // If it's a request for next page, pass the page parameter to the API handler
                    if (nextPage)
                    {
                        int pageToRequest = _movieList.Page + 1;
                        requestParameters.Add(new RequestParameter { Name = "page", Value = $"{pageToRequest}" });

                        response = await _apiHandler.SendAPIRequestAsync<MovieListModel>(resourceUrl, Method.GET, requestParameters.ToArray()).ConfigureAwait(false);
                    }
                    else
                        response = await _apiHandler.SendAPIRequestAsync<MovieListModel>(resourceUrl, Method.GET, requestParameters.ToArray()).ConfigureAwait(false);


                    if (IsValidResponse(response))
                    {
                        var nextMovieList = response.Data;

                        // If the new search results are too much to handle, sets the current list page to last page so the app won't request any more results from the API
                        if (_movieList.Movies != null && _movieList.Movies.Count + nextMovieList.Movies.Count > MOVIE_COUNT_LIMIT)
                        {
                            _movieList.Page = _movieList.TotalPages;
                            return _movieList;
                        }

                        // Get genre list to fill genre names on movielistmodel (for some reason, the API only returns genre names when you request movie details. Otherwise, it returns genre ids with no names)
                        await FillGenreNamesAsync(nextMovieList).ConfigureAwait(false);

                        // Only image paths are given by the API. Have to get image data by http requests
                        await FillMovieImagesAsync(nextMovieList).ConfigureAwait(false);

                        // If static list is not empty, fill in the movies from the next page
                        if (_movieList.Movies != null && _isUpcomingListActive)
                        {
                            _movieList.Movies.AddRange(nextMovieList.Movies);
                            _movieList.Page++;
                        }
                        else
                            _movieList = nextMovieList;

                        _movieList.IsCached = true;

                    }
                    else
                    {
                        throw new Exception(NOT_VALID_RESPONSE);
                    }
                }

                return _movieList;
            }
            catch (Exception ex)
            {
                if (_movieList != null)
                    return _movieList;
                else
                    return null;
            }
            finally
            {
                _isUpcomingListActive = true;
            }

        }

        /// <summary>
        /// God only knows why the API only passes genre ids when querying for movie listing and both ids and names when querying for movie details
        /// </summary>
        /// <param name="movieList">The movie list to fill with genre names to complement ids</param>
        /// <returns></returns>
        async private Task FillGenreNamesAsync(MovieListModel movieList)
        {
            var genreList = await GetGenreListAsync().ConfigureAwait(false);

            if (genreList != null)
            {
                foreach (var movie in movieList.Movies)
                    foreach (var genreId in movie.GenreIds)
                    {
                        if (movie.Genres == null)
                            movie.Genres = new List<GenreModel>();

                        // Fill in genre name based on id provided by upcoming list results
                        var foundGenre = genreList.Genres.Find(i => i.Id == genreId);
                        movie.Genres.Add(new GenreModel() { Id = genreId, Name = foundGenre != null ? foundGenre.Name : null });
                    }
            }
        }

        /// <summary>
        /// Fills the movie list with poster/backdrop image raw data, since the API only returns image paths
        /// </summary>
        /// <param name="movieList">The movie list to fill with image raw data</param>
        /// <returns></returns>
        async private Task FillMovieImagesAsync(MovieListModel movieList)
        {
            string[] imgPathList = new string[movieList.Movies.Count];
            for (int i = 0; i < imgPathList.Length; i++)
                imgPathList[i] = string.IsNullOrEmpty(movieList.Movies[i].PosterPath) ? movieList.Movies[i].BackdropPath : movieList.Movies[i].PosterPath;

            var imgdataList = await GetRawImageAsync(imgPathList).ConfigureAwait(false);

            if (imgdataList != null)
            {
                //Fill movie image property with image bytes
                var nextMovieListEnumerator = movieList.Movies.GetEnumerator();
                nextMovieListEnumerator.MoveNext();

                foreach (var imgData in imgdataList)
                {
                    nextMovieListEnumerator.Current.MovieImage = imgData;
                    nextMovieListEnumerator.MoveNext();
                }
            }
        }

        /// <summary>
        /// Get movie detailed information based on the movie id
        /// </summary>
        /// <param name="id">Movie id to query for details</param>
        /// <returns>Movie details model containing detailed movie information</returns>
        async public Task<MovieDetailsModel> GetMovieDetailsAsync(int id)
        {
            if (!_movieDetails.IsCached || _movieDetails.Id != id)
            {
                string resourceUrl = $"movie/{id}";

                IRestResponse<MovieDetailsModel> response = await _apiHandler.SendAPIRequestAsync<MovieDetailsModel>(resourceUrl, Method.GET);

                if (IsValidResponse(response))
                {
                    _movieDetails = response.Data;
                    _movieDetails.IsCached = true;

                    _movieDetails.MovieImage = await GetRawImageAsync(string.IsNullOrEmpty(_movieDetails.PosterPath) ? _movieDetails.BackdropPath : _movieDetails.PosterPath);
                }
                else
                    return null;

            }

            return _movieDetails;
        }

        /// <summary>
        /// Makes a Http request to the image path and gets raw image data
        /// </summary>
        /// <param name="movieImagePath"></param>
        /// <returns>A byte array containing raw image data</returns>
        async public Task<byte[]> GetRawImageAsync(string movieImagePath)
        {
            try
            {
                string imgSize = string.Empty;
                var config = await GetConfigAsync();

                if (config != null)
                    // Position 4 is 500px width as of 2/1/2017
                    imgSize = config.ImageConfig.PosterSizes[4];
                else
                    //Defaults the image size to 500px width
                    imgSize = "w500";

                byte[] imageBytes = await _httpClient.GetByteArrayAsync(config.ImageConfig.BaseUrl + $"{imgSize}" + movieImagePath).ConfigureAwait(false);

                return imageBytes;
            }
            catch (Exception ex)
            {
                return new byte[0];
            }

        }

        /// <summary>
        /// Makes Http requests to get raw image data from all the passed image paths
        /// </summary>
        /// <param name="movieImagePathList">String array containing image paths like '/HagRRGhTjha3AGeah3.jpg'</param>
        /// <returns>A list of byte arrays containing raw image data from all image paths</returns>
        async public Task<List<byte[]>> GetRawImageAsync(string[] movieImagePathList)
        {
            try
            {
                string imgSize = string.Empty;
                var config = await GetConfigAsync().ConfigureAwait(false);

                if (config != null)
                    // 300px width as of 2/1/2017
                    imgSize = config.ImageConfig.PosterSizes[3];
                else
                    //Defaults the image size to 300px width
                    imgSize = "w300";

                var imgList = new List<byte[]>();

                foreach (var movieImagePath in movieImagePathList)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(movieImagePath))
                        {
                            var byteArray = await _httpClient.GetByteArrayAsync(config.ImageConfig.BaseUrl + $"{imgSize}" + movieImagePath).ConfigureAwait(false);
                            imgList.Add(byteArray);
                        }
                        else
                            imgList.Add(new byte[0]);
                    }
                    catch (HttpRequestException ex)
                    {
                        imgList.Add(new byte[0]);
                    }
                    catch (Exception ex)
                    {
                        imgList.Add(new byte[0]);
                    }
                }

                return imgList;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Checks whether a given response is valid
        /// </summary>
        /// <param name="response">Response to validate</param>
        /// <returns>True if response is clean</returns>
        private bool IsValidResponse(IRestResponse response)
        {
            //TODO: Check for more error cases

            if (response == null || response.StatusCode != System.Net.HttpStatusCode.OK || response.ErrorException != null)
                return false;
            else
                return true;
        }


    }
}