using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UpcomingMoviesAndroid.MovieAPI;
using RestSharp;
using UpcomingMoviesAndroid.Controllers;
using UpcomingMoviesAndroid.Models;
using System.Threading.Tasks;

namespace UpcomingMoviesUnitTest
{
    [TestClass]
    public class APITest
    {
        [TestMethod, Timeout(5000)]
        public void TestMovieAPIUpcomingMovieListResponseNotNull()
        {
            MovieAPIHandler handler = new MovieAPIHandler();

            IRestResponse<MovieListModel> response = handler.SendAPIRequest<MovieListModel>("movie/upcoming", RestSharp.Method.GET);

            Assert.IsNotNull(response);
        }

        [TestMethod, Timeout(5000)]
        public void TestMovieAPIUpcomingMovieListResponseNoError()
        {
            MovieAPIHandler handler = new MovieAPIHandler();

            IRestResponse<MovieListModel> response = handler.SendAPIRequest<MovieListModel>("movie/upcoming", RestSharp.Method.GET);

            Assert.IsTrue(response.StatusCode == System.Net.HttpStatusCode.OK && response.ErrorException == null);
        }

        [TestMethod, Timeout(5000)]
        public void TestMovieAPIMovieDetailsResponseNotNull()
        {
            MovieAPIHandler handler = new MovieAPIHandler();

            IRestResponse<MovieDetailsModel> response = handler.SendAPIRequest<MovieDetailsModel>("movie/346672", RestSharp.Method.GET);

            Assert.IsNotNull(response);
        }

        [TestMethod, Timeout(5000)]
        public void TestMovieAPIMovieDetailsResponseNoError()
        {
            MovieAPIHandler handler = new MovieAPIHandler();

            IRestResponse<MovieDetailsModel> response = handler.SendAPIRequest<MovieDetailsModel>("movie/346672", RestSharp.Method.GET);

            Assert.IsTrue(response.StatusCode == System.Net.HttpStatusCode.OK && response.ErrorException == null);
        }

        [TestMethod, Timeout(5000)]
        public void TestMovieAPIMovieDetailsResponseCorrectResult()
        {
            MovieAPIHandler handler = new MovieAPIHandler();

            IRestResponse<MovieDetailsModel> response = handler.SendAPIRequest<MovieDetailsModel>("movie/346672", RestSharp.Method.GET);

            Assert.IsTrue(string.Equals(response.Data.Title, "Underworld: Blood Wars"));
        }

        [TestMethod, Timeout(5000)]
        async public Task TestMovieAPIUpcomingMovieListResponseCorrectPage()
        {
            MovieAPIHandler handler = new MovieAPIHandler();

            //IRestResponse<MovieListModel> response = handler.SendAPIRequest<MovieListModel>("movie/upcoming", RestSharp.Method.GET, new RequestParameter { Name = "page", Value = "1" });
            //response = handler.SendAPIRequest<MovieListModel>("movie/upcoming", RestSharp.Method.GET, new RequestParameter { Name = "page", Value = "2" });
            //response = handler.SendAPIRequest<MovieListModel>("movie/upcoming", RestSharp.Method.GET, new RequestParameter { Name = "page", Value = "3" });

            var response = await handler.SendAPIRequestAsync<MovieListModel>("movie/upcoming", RestSharp.Method.GET).ConfigureAwait(false);

            response = await handler.SendAPIRequestAsync<MovieListModel>("movie/upcoming", RestSharp.Method.GET, new RequestParameter() { Name = "page", Value = "2" }).ConfigureAwait(false);

            response = await handler.SendAPIRequestAsync<MovieListModel>("movie/upcoming", RestSharp.Method.GET, new RequestParameter() { Name = "page", Value = "3" }).ConfigureAwait(false);

            Assert.IsTrue(response.Data.Page == 3);
        }
    }
}
