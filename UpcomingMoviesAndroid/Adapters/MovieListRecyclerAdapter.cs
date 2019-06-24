using System;
using System.Text;

using Android.Views;
using Android.Support.V7.Widget;
using UpcomingMoviesAndroid.Models;
using UpcomingMoviesAndroid.ViewHolders;
using Android.Graphics;

namespace UpcomingMoviesAndroid.Adapters
{
    /// <summary>
    /// RecyclerView adapter to display movie information
    /// </summary>
    class MovieListRecyclerAdapter : RecyclerView.Adapter
    {
        public MovieListModel _movieList;
        public event EventHandler<int> ItemClick;

        // Defines a limit of scrolled views until garbage collector is called
        const int LOADED_VIEWS_LIMIT = 10;
        int _loadedViews = 0;

        public MovieListRecyclerAdapter (MovieListModel movieList)
        {
            _movieList = movieList;
        }
        public override int ItemCount
        {
            get
            {
                if (_movieList.Movies != null)
                    return _movieList.Movies.Count;
                else
                    return 0;
            }
        }

        public override long GetItemId(int position)
        {
            return _movieList.Movies[position].Id;
        }

        private void OnClick(int position)
        {
            ItemClick?.Invoke(this, position);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var viewHolder = holder as MovieListViewHolder;

            viewHolder.Title.Text = _movieList.Movies[position].Title;

            // Do some genre text formatting for movies with multiple genres ({genre}-{genre}-{genre}...)
            string genres;
            if (_movieList.Movies[position].Genres != null && _movieList.Movies[position].Genres.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var genre in _movieList.Movies[position].Genres)
                    sb.Append($"{genre.Name} - ");

                genres = sb.ToString(0, sb.Length - 3);
            }
            else
                genres = viewHolder.Genre.Context.GetString(Resource.String.not_defined_genre);
            viewHolder.Genre.Text = genres;

            //TODO: Localize release date format
            viewHolder.ReleaseDate.Text = $"{viewHolder.ReleaseDate.Context.Resources.GetString(Resource.String.details_release_date)} {_movieList.Movies[position].ReleaseDate}";

            // Samplesizing by half to lower memory usage
            var bitmapOptions = new BitmapFactory.Options();
            bitmapOptions.InSampleSize = 2;
            Bitmap bitmap;

            // Checks whether the image is not empty (in that case, gets a default bitmap from resource) and loads the bitmap into the view
            if (_movieList.Movies[position].MovieImage.Length > 0)
                bitmap = BitmapFactory.DecodeByteArray(_movieList.Movies[position].MovieImage, 0, _movieList.Movies[position].MovieImage.Length, bitmapOptions);
            else
                bitmap = BitmapFactory.DecodeResource(viewHolder.Image.Context.Resources, Resource.Drawable.noimage_w500, bitmapOptions);

            // Since it will load a lot of images and Xamarin doesn't handle Bitmap garbage collection well, it HAS TO call the GC after a number of views scrolled or be ready for OutOfMemory exceptions
            viewHolder.Image.SetImageBitmap(bitmap);
            bitmap.Dispose();

            _loadedViews++;

            // When the scrolled view passes the defined limit, garbage collection is called to free Bitmap memory
            if (_loadedViews == LOADED_VIEWS_LIMIT)
            {
                _loadedViews = 0;
                GC.Collect(1);
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.MovieListItemLayout, parent, false);

            var viewHolder = new MovieListViewHolder(view, OnClick);

            return viewHolder;
        }

        /// <summary>
        /// Sets new data for the adapter and notify subscribers
        /// </summary>
        /// <param name="newList"></param>
        public void ChangeData(MovieListModel newList)
        {
            _movieList = newList;

            NotifyDataSetChanged();
        }
    }
}