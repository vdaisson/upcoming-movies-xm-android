# upcoming-movies-xm-android

Simple App developed with Xamarin-Android that queries The Movie Database API to get movies information.

# Basic Build Instructions
There is no special build instructions, just use the libraries below and you're good to go. However I STRONGLY advise anyone to use [NuGet Package Manager](https://www.nuget.org/) to handle library dependencies on code.

Upcoming Movies App uses the following libraries:
- RestSharp v105.2.3 by John Sheehan
  - A great mature .NET library to consume RESTful web services and APIs. It is very easy to use and comes with built-in JSON and XML serializers/deserializers.
 

- System.Net.Http by Microsoft
  - Provides solid async HTTP request methods. And it's Microsoft.
 

- Xamarin.Android.Support.Compat, Xamarin.Android.Support.Core.UI and Xamarin.Android.Support.v7.RecyclerView v24.2.1 by Xamarin Inc.
  - These are not your common Xamarin libraries. But they provide some nice UI widgets like RecyclerCiew, which is a kind of ListView successor. It has built-in best practices enforcement, like ViewHolder pattern for list views.
