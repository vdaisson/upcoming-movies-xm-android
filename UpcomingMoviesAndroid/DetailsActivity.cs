using System;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

using UpcomingMoviesAndroid.Controllers;
using UpcomingMoviesAndroid.Models;
using System.Threading.Tasks;
using Android.Graphics;

namespace UpcomingMoviesAndroid
{
    [Activity(Label = "@string/details_activity_name")]
    public class DetailsActivity : Activity
    {
        async protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Details);

            // Show loading screen while waiting for movie data to load
            ShowLoadingScreen(true);

            await FillMovieDetailsAsync(savedInstanceState);
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            // Saves scroll position through the activity lifecycle
            outState.PutInt("SCROLL_VALUE", FindViewById<ScrollView>(Resource.Id.scrollViewDetails).ScrollY);
        }

        /// <summary>
        /// Calls controller to query for movie details based on id passed by main activity
        /// </summary>
        /// <param name="savedInstanceState">Bundle passed by Android framework</param>
        /// <returns></returns>
        async private Task FillMovieDetailsAsync(Bundle savedInstanceState)
        {
            int movieId = GetMovieId(savedInstanceState);
            var controller = MovieController.GetMovieController();

            //Wait for the movie details from controller
            var movie = await controller.GetMovieDetailsAsync(movieId);

            await FillViewsAsync(movie);

            ShowLoadingScreen(false);

            RestoreScrollState(savedInstanceState);
            
        }

        /// <summary>
        /// Tries to restore scroll original position
        /// </summary>
        /// <param name="savedInstanceState"></param>
        private void RestoreScrollState(Bundle savedInstanceState)
        {
            if (savedInstanceState != null)
            {
                var scrollView = FindViewById<ScrollView>(Resource.Id.scrollViewDetails);
                int yPosition = savedInstanceState.GetInt("SCROLL_VALUE");
                if (yPosition != 0)
                    scrollView.Post(() => scrollView.ScrollTo(0, yPosition));
            }
        }

        /// <summary>
        /// Show or hides the Activity's loading screen
        /// </summary>
        /// <param name="show"></param>
        private void ShowLoadingScreen(bool show)
        {
            if (show)
            {
                FindViewById<ScrollView>(Resource.Id.scrollViewDetails).Visibility = ViewStates.Gone;
                FindViewById<RelativeLayout>(Resource.Id.loadingPanel).Visibility = ViewStates.Visible;
            }
            else
            {
                FindViewById<RelativeLayout>(Resource.Id.loadingPanel).Visibility = ViewStates.Gone;
                FindViewById<ScrollView>(Resource.Id.scrollViewDetails).Visibility = ViewStates.Visible;
            }
        }

        /// <summary>
        /// Adapter-like method to fill the Views with movie data
        /// </summary>
        /// <param name="movie">Movie model to get details from</param>
        /// <returns></returns>
        async private Task FillViewsAsync(MovieDetailsModel movie)
        {
            // Movie Title
            TextView titleView = FindViewById<TextView>(Resource.Id.textViewTitle);
            titleView.Text = movie.Title;

            // List of genres
            TextView genresView = FindViewById<TextView>(Resource.Id.textViewGenres);
            string genres;
            if (movie.Genres.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var genre in movie.Genres)
                    sb.Append($"{genre.Name} - ");

                genres = sb.ToString(0, sb.Length - 3);
            }
            else
                genres = Application.GetString(Resource.String.not_defined_genre);
            genresView.Text = genres;

            // Movie Overview
            TextView overviewView = FindViewById<TextView>(Resource.Id.textViewOverview);
            overviewView.Text = movie.Overview;

            // Release Date
            TextView releaseDateView = FindViewById<TextView>(Resource.Id.textViewReleaseDate);
            //TODO: Localize DateTime convertion and view text
            DateTime releaseDate = Convert.ToDateTime(movie.ReleaseDate);
            releaseDateView.Text = $"{releaseDateView.Text} {releaseDate.Month}/{releaseDate.Day}/{releaseDate.Year}";

            // Poster image
            ImageView iv = FindViewById<ImageView>(Resource.Id.imgViewMovie);
            if (movie.MovieImage.Length > 0)
                iv.SetImageBitmap(await BitmapFactory.DecodeByteArrayAsync(movie.MovieImage, 0, movie.MovieImage.Length).ConfigureAwait(false));
            else
                iv.SetImageBitmap(await BitmapFactory.DecodeResourceAsync(Resources, Resource.Drawable.noimage_w500).ConfigureAwait(false));
        }

        /// <summary>
        /// Gets movie id passed by main Activity
        /// </summary>
        /// <param name="savedInstanceState"></param>
        /// <returns></returns>
        private int GetMovieId(Bundle savedInstanceState)
        {
            int movieId = (int)Intent.GetLongExtra("movie_id", 0);

            return movieId;
        }

        // Simple click handler to finish the Activity when back button is pressed
        [Java.Interop.Export("BackButton_Click")]
        public void BackButton_Click(View view)
        {
            Finish();
        }
    }
}