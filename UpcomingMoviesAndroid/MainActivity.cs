using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content;
using UpcomingMoviesAndroid.Models;
using Android.Views;
using System.Threading.Tasks;
using UpcomingMoviesAndroid.Controllers;
using UpcomingMoviesAndroid.Adapters;
using UpcomingMoviesAndroid.Listeners;
using System;
using Android.Support.V7.Widget;
using Android.Views.InputMethods;

namespace UpcomingMoviesAndroid
{
    [Activity(Label = "@string/app_name", MainLauncher = true, Icon = "@drawable/icon", 
        LaunchMode = Android.Content.PM.LaunchMode.SingleTop)]
    public class MainActivity : Activity, RecyclerView.IOnScrollChangeListener, SearchView.IOnQueryTextListener, SearchView.IOnCloseListener
    {
        //MovieListModel upcomingMovieList;
        RecyclerView _recyclerView;
        RecyclerView.LayoutManager _layoutManager;
        MovieListRecyclerAdapter _adapter;
        MovieListRecyclerOnScrollListener _scrollListener;
        //bool _isSearchList = false;
        string _storedQuery;

        bool isLoading = false;

        async protected override void OnCreate(Bundle bundle)
        {

            base.OnCreate(bundle);

            SetContentView(Resource.Layout.MainLayoutv7);

            // Forces keyboard to stay hidden hide when details are clicked
            Window.SetSoftInputMode(SoftInput.StateAlwaysHidden);

            // Sets the searchview and its event handlers
            var searchView = FindViewById<SearchView>(Resource.Id.searchViewMovies);
            searchView.SetOnQueryTextListener(this);
            searchView.SetOnCloseListener(this);

            LoadingScreen(true);

            // Loads data
            await FillMovieListAsync();
        }

        protected override void OnResume()
        {
            base.OnResume();

            // Tries to get focus when the activity resumes
            if (_recyclerView != null)
                _recyclerView.RequestFocus(FocusSearchDirection.Up);
        }

        /// <summary>
        /// Shows or hides loading screen
        /// </summary>
        /// <param name="show">Whether to show or hide the loading screen</param>
        private void LoadingScreen(bool show)
        {
            if (show)
            {
                FindViewById<LinearLayout>(Resource.Id.innerLinearLayout).Visibility = ViewStates.Gone;
                FindViewById<RelativeLayout>(Resource.Id.loadingPanelMain1).Visibility = ViewStates.Visible;
            }
            else
            {
                FindViewById<LinearLayout>(Resource.Id.innerLinearLayout).Visibility = ViewStates.Visible;
                FindViewById<RelativeLayout>(Resource.Id.loadingPanelMain1).Visibility = ViewStates.Gone;
            }
        }

        /// <summary>
        /// Calls the controller to get the upcoming movie list and set RecyclerView adapter
        /// </summary>
        /// <param name="reload">Whether it's a list reload or just more pages</param>
        /// <returns></returns>
        async private Task FillMovieListAsync(bool reload = false)
        {
            var controller = MovieController.GetMovieController();

            var upcomingMovieList = await controller.GetUpcomingMovieListAsync();

            if (upcomingMovieList != null)
            {
                if (!reload)
                {
                    // First time call

                    _recyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerViewMovies);
                    _recyclerView.HasFixedSize = true;
                    _layoutManager = new LinearLayoutManager(this);
                    _recyclerView.SetLayoutManager(_layoutManager);

                    _adapter = new MovieListRecyclerAdapter(upcomingMovieList);
                    _adapter.ItemClick -= OnItemClick;
                    _adapter.ItemClick += OnItemClick;
                    _recyclerView.SetAdapter(_adapter);


                    _scrollListener = new MovieListRecyclerOnScrollListener(new LinearLayoutManager(this));
                    _scrollListener.LoadMovieEvent -= OnScrollListener_LoadMovieEvent;
                    _scrollListener.LoadMovieEvent += OnScrollListener_LoadMovieEvent;

                    _recyclerView.AddOnScrollListener(_scrollListener);

                    LoadingScreen(false);
                }
                else
                {
                    // Since this method to load data fiddles with UI elements, it has to be run on a UI thread
                    RunOnUiThread(() => LoadRecyclerListData(upcomingMovieList, true));
                }
            }
            else
            {
                ShowDataLoadError();
            }
        }

        /// <summary>
        /// Calls the controller to get the movie results based on the search query
        /// </summary>
        /// <param name="query">The keyword to query the API</param>
        /// <returns></returns>
        async private Task FillMovieSearchListAsync(string query)
        {
            var oldQuery = _storedQuery;
            _storedQuery = query;

            var controller = MovieController.GetMovieController();

            var searchedMovieList = await controller.GetSearchedMovieListAsync(query);

            if (searchedMovieList != null)
                // Compares new and old search keyword to see if it's a list reload
                RunOnUiThread(() => LoadRecyclerListData(searchedMovieList, !string.Equals(oldQuery, _storedQuery)));
            else
            {
                ShowDataLoadError();
            }

        }

        private void OnScrollListener_LoadMovieEvent(object sender, EventArgs e)
        {
            // Srolling has hit the bottom, query the API for more pages
            if (!isLoading)
            {
                _scrollListener.LoadMovieEvent -= OnScrollListener_LoadMovieEvent;
                ShowLoading(true);
                Task.Run(() => HandleLoadMovieAsync(!MovieController.GetMovieController().IsUpcomingListActive));
            }
        }

        /// <summary>
        /// Handles requests for more movie pages
        /// </summary>
        /// <param name="search">Whether it's a search based query or a upcoming movie list one</param>
        /// <returns></returns>
        async private Task HandleLoadMovieAsync(bool search = false)
        {
            MovieListModel updatedMovieList;

            if (search)
                updatedMovieList = await MovieController.GetMovieController().GetSearchedMovieListAsync(_storedQuery, true);
            else
                updatedMovieList = await MovieController.GetMovieController().GetUpcomingMovieListAsync(true);

            if (updatedMovieList != null)
                RunOnUiThread(() => LoadRecyclerListData(updatedMovieList, false));
            else
                RunOnUiThread(() => ShowDataLoadError());
        }

        /// <summary>
        /// Calls the adapter to change its data and scroll to top if requested
        /// </summary>
        /// <param name="movieList">Movie list to fill the adapter</param>
        /// <param name="scrollTop">Whether to scroll all the way to the top after data is loaded</param>
        private void LoadRecyclerListData(MovieListModel movieList, bool scrollTop)
        {
            _adapter.ChangeData(movieList);

            ShowLoading(false);

            if (scrollTop)
                _recyclerView.ScrollToPosition(0);

            if (movieList.Page < movieList.TotalPages)
            {
                _scrollListener.LoadMovieEvent -= OnScrollListener_LoadMovieEvent;
                _scrollListener.LoadMovieEvent += OnScrollListener_LoadMovieEvent;
            }
            else
                _scrollListener.LoadMovieEvent -= OnScrollListener_LoadMovieEvent;
        }
    
        private void OnItemClick (Object sender, int position)
        {
            HideKeyboard();

            // Calls the movie details Activity
            var movie = _adapter.GetItemId(position);
            var detailsIntent = new Intent(this, typeof(DetailsActivity));
            detailsIntent.PutExtra("movie_id", movie);
            StartActivity(detailsIntent);
        }

        /// <summary>
        /// Shows or hides the little loading circle at the bottom of the screen
        /// </summary>
        /// <param name="show"></param>
        private void ShowLoading(bool show)
        {
            isLoading = show;

            var progressBar = FindViewById<ProgressBar>(Resource.Id.progressBarMain);

            if (show)
                progressBar.Visibility = ViewStates.Visible;
            else
                progressBar.Visibility = ViewStates.Gone;

        }

        /// <summary>
        /// Small toast to alert about data loading errors
        /// </summary>
        private void ShowDataLoadError()
        {
            Toast.MakeText(this, Resource.String.main_data_load_error, ToastLength.Short);

            ShowLoading(false);
        }

        public void OnScrollChange(View v, int scrollX, int scrollY, int oldScrollX, int oldScrollY)
        {
            
        }

        public bool OnQueryTextChange(string newText)
        {
            return true;
        }

        public bool OnQueryTextSubmit(string query)
        {
            // When users submits text, query the API for movies based on the keyword

            HideKeyboard();

            ShowLoading(true);

            Task.Run(() => FillMovieSearchListAsync(query));

            return true;
        }

        /// <summary>
        /// Simple method to force keyboard to hide
        /// </summary>
        private void HideKeyboard()
        {
            View view = this.CurrentFocus;
            if (view != null)
            {
                InputMethodManager imm = (InputMethodManager)GetSystemService(Context.InputMethodService);
                imm.HideSoftInputFromWindow(view.WindowToken, 0);
            }
        }

        /// <summary>
        /// When the search view is closed, load upcoming movies list
        /// </summary>
        /// <returns></returns>
        bool SearchView.IOnCloseListener.OnClose()
        {

            if (!MovieController.GetMovieController().IsUpcomingListActive)
            {
                ShowLoading(true);
                Task.Run(() => FillMovieListAsync(true));
            }

            return false;
        }
    }
}

