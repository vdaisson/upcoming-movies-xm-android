using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;

namespace UpcomingMoviesAndroid.ViewHolders
{
    /// <summary>
    /// ViewHolder pattern class required by RecyclerView
    /// </summary>
    class MovieListViewHolder : RecyclerView.ViewHolder
    {
        public TextView Title { get; private set; }
        public TextView Genre { get; private set; }
        public TextView ReleaseDate { get; private set; }
        public ImageView Image { get; private set; }

        public MovieListViewHolder (View view, Action<int> itemClickListener) : base (view)
        {
            Title = view.FindViewById<TextView>(Resource.Id.txtViewMovieListItemTitle);
            Genre = view.FindViewById<TextView>(Resource.Id.txtViewMovieListItemGenres);
            ReleaseDate = view.FindViewById<TextView>(Resource.Id.txtViewMovieListItemReleaseDate);
            Image = view.FindViewById<ImageView>(Resource.Id.imgViewMovieListItem);

            view.Click += (sender, e) => itemClickListener(AdapterPosition);
        }
    }
}