using Android.App;
using Android.Views;
using Android.Widget;
using UpcomingMoviesAndroid.Models;
using Android.Graphics;

namespace UpcomingMoviesAndroid.Adapters
{
    /// <summary>
    /// Adapter class used for a ListView widget
    /// </summary>
    public class MovieListAdapter : BaseAdapter<MovieModel>
    {
        private MovieListModel movieList;
        private Activity context;

        // Implements the ViewHolder pattern
        private class ViewHolder : Java.Lang.Object
        {
            public TextView Title { get; set; }
            public TextView Genre { get; set; }
            public TextView ReleaseDate { get; set; }
            public ImageView Image { get; set; }
        }

        public MovieListAdapter (Activity context, MovieListModel data) : base()
        {
            this.context = context;
            movieList = data;
        }

        public override MovieModel this[int position]
        {
            get
            {
                return movieList.Movies[position];
            }
        }

        public override int Count
        {
            get
            {
                return movieList.Movies.Count;
            }
        }

        public override long GetItemId(int position)
        {
            return movieList.Movies[position].Id;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            ViewHolder viewHolder = null;
            var movieListItem = movieList.Movies[position];

            View view = convertView;

            if (view != null)
                viewHolder = view.Tag as ViewHolder;

            if (viewHolder == null)
            {
                viewHolder = new ViewHolder();
                view = context.LayoutInflater.Inflate(Resource.Layout.MovieListItemLayout, null);
                viewHolder.Title = view.FindViewById<TextView>(Resource.Id.txtViewMovieListItemTitle);
                //viewHolder.Genre = view.FindViewById<TextView>(Resource.Id.txtViewMovieListItemGenres);
                viewHolder.ReleaseDate = view.FindViewById<TextView>(Resource.Id.txtViewMovieListItemReleaseDate);
                viewHolder.Image = view.FindViewById<ImageView>(Resource.Id.imgViewMovieListItem);

                view.Tag = viewHolder;
            }

            viewHolder.Title.Text = movieListItem.Title;
            viewHolder.ReleaseDate.Text = $"{view.Context.Resources.GetString(Resource.String.details_release_date)} {movieListItem.ReleaseDate}";

            var bitmapOptions = new BitmapFactory.Options();
            bitmapOptions.InSampleSize = 2;
            if (movieListItem.MovieImage.Length > 0)
                viewHolder.Image.SetImageBitmap(BitmapFactory.DecodeByteArray(movieListItem.MovieImage, 0, movieListItem.MovieImage.Length, bitmapOptions));
            else
                viewHolder.Image.SetImageBitmap(BitmapFactory.DecodeResource(view.Context.Resources, Resource.Drawable.noimage_w500, bitmapOptions));

            return view;
        }

        public void SetData(MovieListModel data)
        {
            movieList = data;

            NotifyDataSetChanged();
        }

        public void ClearRows()
        {
            movieList.Movies.Clear();
            movieList = null;

            //NotifyDataSetChanged();
        }

    }
}