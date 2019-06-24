using System;
using Android.Support.V7.Widget;

namespace UpcomingMoviesAndroid.Listeners
{
    /// <summary>
    /// Scroll listener for the RecyclerView widget
    /// </summary>
    class MovieListRecyclerOnScrollListener : RecyclerView.OnScrollListener
    {
        public delegate void LoadMovieList(object sender, EventArgs e);
        public event EventHandler LoadMovieEvent;

        private LinearLayoutManager _layoutManager;

        public MovieListRecyclerOnScrollListener (LinearLayoutManager layoutManager)
        {
            _layoutManager = layoutManager;
        }

        public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
        {
            base.OnScrolled(recyclerView, dx, dy);

            // If RecyclerView has hit the bottom, fires as event to load more movies
            if (!recyclerView.CanScrollVertically(1))
                LoadMovieEvent?.Invoke(this, null);

        }
    }
}