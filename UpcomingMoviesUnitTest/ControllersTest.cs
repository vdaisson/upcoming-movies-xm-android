using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RestSharp;
using UpcomingMoviesAndroid.Controllers;
using UpcomingMoviesAndroid.Models;
using Android.App;
using System.Threading.Tasks;
using System.Globalization;
using System.Collections.Generic;

namespace UpcomingMoviesUnitTest
{
    [TestClass]
    public class ControllersTest
    {
        [TestMethod, Timeout(10000)]
        async public Task TestMovieControllerUpcomingListNotNullResponseAsync()
        {
            var response = await MovieController.GetMovieController().GetUpcomingMovieListAsync();

            Assert.IsNotNull(response);
        }

        [TestMethod, Timeout(10000)]
        async public Task TestMovieControllerUpcomingListNotNullMoviesListAsync()
        {
            var response = await MovieController.GetMovieController().GetUpcomingMovieListAsync();

            Assert.IsNotNull(response.Movies);
        }

        [TestMethod, Timeout(10000)]
        async public Task TestMovieControllerUpcomingListListHasContentAsync()
        {
            var response = await MovieController.GetMovieController().GetUpcomingMovieListAsync();

            Assert.IsTrue(response.Movies.Count > 0 && !string.IsNullOrEmpty(response.Movies[0].Title));
        }

        [TestMethod, Timeout(10000)]
        async public Task TestMovieControllerMovieDetailsNotNullResponseAsync()
        {
            var response = await MovieController.GetMovieController().GetMovieDetailsAsync(121856);

            Assert.IsNotNull(response);
        }

        [TestMethod, Timeout(10000)]
        async public Task TestMovieControllerMovieDetailsCorrectResultAsync()
        {
            var response = await MovieController.GetMovieController().GetMovieDetailsAsync(121856);

            Assert.IsTrue(string.Equals(response.OriginalTitle, "Assassin's Creed"));
        }

        [TestMethod, Timeout(10000)]
        async public Task TestMovieControllerMovieDetailsImageNotNullAsync()
        {
            var response = await MovieController.GetMovieController().GetMovieDetailsAsync(121856);

            Assert.IsTrue(response.MovieImage != null);
        }

        [TestMethod, Timeout(10000)]
        async public Task TestMovieControllerUpcomingListManyCallsOKResponseAsync()
        {

            var response = await MovieController.GetMovieController().GetUpcomingMovieListAsync().ConfigureAwait(false);
            response = await MovieController.GetMovieController().GetUpcomingMovieListAsync(true).ConfigureAwait(false);
            response = await MovieController.GetMovieController().GetUpcomingMovieListAsync(true).ConfigureAwait(false);

            Assert.IsTrue(response.Page == 3 && response.Movies.Count == 60);

        }

        [TestMethod]
        async public Task TestMovieControllerUpcomingListToLastPageAsync()
        {

            var response = await MovieController.GetMovieController().GetUpcomingMovieListAsync().ConfigureAwait(false);
            int lastPage = response.TotalPages;

            while (response.Page < lastPage)
                response = await MovieController.GetMovieController().GetUpcomingMovieListAsync(true).ConfigureAwait(false);

            Assert.IsTrue(response.Page == lastPage);

        }

        [TestMethod]
        async public Task TestMovieControllerMovieSearchGetsResults()
        {

            var response = await MovieController.GetMovieController().GetSearchedMovieListAsync("underworld");
            bool failed = false;

            foreach (var movie in response.Movies)
                failed = !movie.Title.Contains("underworld");

            Assert.IsTrue(response.Movies.Count == 20);

        }
    }
}
