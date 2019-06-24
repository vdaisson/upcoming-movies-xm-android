using System;

using RestSharp;
using System.Threading.Tasks;
using RestSharp.Extensions.MonoHttp;

namespace UpcomingMoviesAndroid.MovieAPI
{
    /// <summary>
    /// Makes all the http requests to TMDB API
    /// </summary>
    public class MovieAPIHandler
    {
        //TODO: put strings in config file
        readonly string api_key = "1f54bd990f1cdfb230adb312546d765d";
        readonly string baseURL = "https://api.themoviedb.org/3/";

        /// <summary>
        /// Sync version to send a request to the API and retrieve a deserialized JSON response based on the generic type
        /// </summary>
        /// <typeparam name="T">The desired deserialized type</typeparam>
        /// <param name="resourcePath">Path to the API resource</param>
        /// <param name="method">Which Http method to use</param>
        /// <param name="parameters">Series of parameters to build the API request query string</param>
        /// <returns></returns>
        public IRestResponse<T> SendAPIRequest<T>(string resourcePath, Method method, params RequestParameter[] parameters) where T : new()
        {
            var client = new RestClient(baseURL);
            var request = new RestRequest(resourcePath, method);

            request.AddParameter("api_key", api_key);

            if (parameters != null)
                foreach (var parameter in parameters)
                    request.AddParameter(parameter.Name, parameter.Value);

            IRestResponse<T> response = client.Execute<T>(request);
            
            return response;
        }

        /// <summary>
        /// Async version to send a request to the API and retrieve a deserialized JSON response based on the generic type
        /// </summary>
        /// <typeparam name="T">The desired deserialized type</typeparam>
        /// <param name="resourcePath">Path to the API resource</param>
        /// <param name="method">Which Http method to use</param>
        /// <param name="parameters">Series of parameters to build the API request query string</param>
        /// <returns></returns>
        async public Task<IRestResponse<T>> SendAPIRequestAsync<T>(string resourcePath, Method method, params RequestParameter[] parameters) where T : new()
        {

            var client = new RestClient(baseURL);
            var request = new RestRequest(resourcePath, method);

            request.AddQueryParameter("api_key", api_key);

            if (parameters != null)
                foreach (var parameter in parameters)
                {
                    // When using the search resource, the query has to be UrlEncoded
                    if (parameter.Name == "query")
                        request.AddParameter(parameter.Name, HttpUtility.UrlEncode(parameter.Value));
                    else
                        request.AddParameter(parameter.Name, parameter.Value);
                }

            try
            {
                var response = await client.ExecuteTaskAsync<T>(request).ConfigureAwait(false);

                return response;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Helper class to pass query strings
    /// </summary>
    public struct RequestParameter
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}