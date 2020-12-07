// -----------------------------------------------------------------------
// <copyright file="TmdbMoviesController.cs" company="Weloveloli">
//     Copyright (c) Weloveloli.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Weloveloli.AVGui.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Chromely;
    using Chromely.Core.Configuration;
    using Chromely.Core.Network;

    /// <summary>
    /// Defines the <see cref="TmdbMoviesController" />.
    /// </summary>
    [ControllerProperty(Name = "TmdbMoviesController")]
    public class TmdbMoviesController : ChromelyController
    {
        /// <summary>
        /// Defines the TmdbBaseUrl.
        /// </summary>
        private const string TmdbBaseUrl = "https://api.themoviedb.org/3/";

        /// <summary>
        /// Defines the ChromelyTmdbApiKey.
        /// </summary>
        private const string ChromelyTmdbApiKey = "4f457e870e91b76e02292d52a46fc445";

        /// <summary>
        /// The TmdbLatestUrl.
        /// </summary>
        /// <param name="apiKey">The apiKey<see cref="string"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private static string TmdbLatestUrl(string apiKey = ChromelyTmdbApiKey) => $"movie/latest?api_key={apiKey}&language=en-US";

        /// <summary>
        /// The TmdbPopularUrl.
        /// </summary>
        /// <param name="apiKey">The apiKey<see cref="string"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private static string TmdbPopularUrl(string apiKey = ChromelyTmdbApiKey) => $"movie/popular?api_key={apiKey}&language=en-US&page=1";

        /// <summary>
        /// The TmdbTopRatedUrl.
        /// </summary>
        /// <param name="apiKey">The apiKey<see cref="string"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private static string TmdbTopRatedUrl(string apiKey = ChromelyTmdbApiKey) => $"movie/top_rated?api_key={apiKey}&language=en-US&page=1";

        /// <summary>
        /// The TmdbNowPlayingUrl.
        /// </summary>
        /// <param name="apiKey">The apiKey<see cref="string"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private static string TmdbNowPlayingUrl(string apiKey = ChromelyTmdbApiKey) => $"movie/now_playing?api_key={apiKey}&language=en-US&page=1";

        /// <summary>
        /// The TmdbUpcomingUrl.
        /// </summary>
        /// <param name="apiKey">The apiKey<see cref="string"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private static string TmdbUpcomingUrl(string apiKey = ChromelyTmdbApiKey) => $"movie/upcoming?api_key={apiKey}&language=en-US&page=1";

        /// <summary>
        /// The TmdbSearchUrl.
        /// </summary>
        /// <param name="queryValue">The queryValue<see cref="string"/>.</param>
        /// <param name="apiKey">The apiKey<see cref="string"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private static string TmdbSearchUrl(string queryValue, string apiKey = ChromelyTmdbApiKey) => $"search/movie?api_key={apiKey}&query={queryValue}&language=en-US&page=1&include_adult=false";

        /// <summary>
        /// The TmdbGetMovieUrl.
        /// </summary>
        /// <param name="movieId">The movieId<see cref="string"/>.</param>
        /// <param name="apiKey">The apiKey<see cref="string"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private static string TmdbGetMovieUrl(string movieId, string apiKey = ChromelyTmdbApiKey) => $"movie/{movieId}?api_key={apiKey}";

        /// <summary>
        /// Defines the _config.
        /// </summary>
        private readonly IChromelyConfiguration _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="TmdbMoviesController"/> class.
        /// </summary>
        /// <param name="config">The config<see cref="IChromelyConfiguration"/>.</param>
        public TmdbMoviesController(IChromelyConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// The GetMovies.
        /// </summary>
        /// <param name="request">The request<see cref="IChromelyRequest"/>.</param>
        /// <returns>The <see cref="IChromelyResponse"/>.</returns>
        [RequestAction(RouteKey = "/tmdbmoviescontroller/movies")]
        public IChromelyResponse GetMovies(IChromelyRequest request)
        {
            var parameters = request.Parameters as IDictionary<string, string>;
            var name = string.Empty;
            var query = string.Empty;

            if (parameters != null && parameters.Any())
            {
                if (parameters.ContainsKey("name"))
                {
                    name = parameters["name"] ?? string.Empty;
                }

                if (parameters.ContainsKey("query"))
                {
                    query = parameters["query"] ?? string.Empty;
                }
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                return new ChromelyResponse() { RequestId = request.Id, Data = new List<Result>() };
            }

            if (name.Equals("search") && string.IsNullOrWhiteSpace(query))
            {
                return new ChromelyResponse() { RequestId = request.Id, Data = new List<Result>() };
            }

            var paramUrl = string.Empty;
            switch (name.ToLower())
            {
                case "latest":
                    paramUrl = TmdbLatestUrl();
                    break;
                case "popular":
                    paramUrl = TmdbPopularUrl();
                    break;
                case "toprated":
                    paramUrl = TmdbTopRatedUrl();
                    break;
                case "nowplaying":
                    paramUrl = TmdbNowPlayingUrl();
                    break;
                case "upcoming":
                    paramUrl = TmdbUpcomingUrl();
                    break;
                case "search":
                    paramUrl = TmdbSearchUrl(query);
                    break;
            }

            var tmdbMoviesTask = Task.Run(() =>
            {
                return GetTmdbMovieListAsync(paramUrl);
            });

            tmdbMoviesTask.Wait();

            List<Result> movies = new List<Result>();
            var tmdMovieInfo = tmdbMoviesTask.Result;

            if (tmdbMoviesTask.Result != null)
            {
                movies = tmdbMoviesTask.Result.results;
            }

            return new ChromelyResponse() { RequestId = request.Id, Data = movies };
        }

        /// <summary>
        /// The HomePage.
        /// </summary>
        /// <param name="queryParameters">The queryParameters<see cref="IDictionary{string, string}"/>.</param>
        [CommandAction(RouteKey = "/tmdbmoviescontroller/homepage")]
        public void HomePage(IDictionary<string, string> queryParameters)
        {
            if (queryParameters == null || !queryParameters.Any())
            {
                return;
            }

            var movieId = string.Empty;
            if (queryParameters.ContainsKey("movieid"))
            {
                movieId = queryParameters["movieid"] ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(movieId))
            {
                return;
            }

            var tmdbMovieTask = Task.Run(() =>
            {
                return GetTmdbMovieAsync(movieId);
            });

            tmdbMovieTask.Wait();

            var movie = tmdbMovieTask.Result;
            if (movie != null && !string.IsNullOrWhiteSpace(movie.homepage))
            {
                BrowserLauncher.Open(_config.Platform, movie.homepage);
            }
        }

        /// <summary>
        /// The GetTmdbMovieListAsync.
        /// </summary>
        /// <param name="paramUrl">The paramUrl<see cref="string"/>.</param>
        /// <returns>The <see cref="Task{TmdMoviesInfo}"/>.</returns>
        private async Task<TmdMoviesInfo> GetTmdbMovieListAsync(string paramUrl)
        {
            var baseAddress = new Uri(TmdbBaseUrl);
            using (var httpClient = new HttpClient { BaseAddress = baseAddress })
            {
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json");

                using (var response = await httpClient.GetAsync(paramUrl))
                {
                    string responseData = await response.Content.ReadAsStringAsync();

                    var options = new JsonSerializerOptions();
                    options.ReadCommentHandling = JsonCommentHandling.Skip;
                    options.AllowTrailingCommas = true;

                    return JsonSerializer.Deserialize<TmdMoviesInfo>(responseData, options);
                }
            }
        }

        /// <summary>
        /// The GetTmdbMovieAsync.
        /// </summary>
        /// <param name="movieId">The movieId<see cref="string"/>.</param>
        /// <returns>The <see cref="Task{TmdMovie}"/>.</returns>
        private async Task<TmdMovie> GetTmdbMovieAsync(string movieId)
        {
            var baseAddress = new Uri(TmdbBaseUrl);
            using (var httpClient = new HttpClient { BaseAddress = baseAddress })
            {
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("accept", "application/json");

                using (var response = await httpClient.GetAsync(TmdbGetMovieUrl(movieId)))
                {
                    string responseData = await response.Content.ReadAsStringAsync();

                    var options = new JsonSerializerOptions();
                    options.ReadCommentHandling = JsonCommentHandling.Skip;
                    options.AllowTrailingCommas = true;

                    return JsonSerializer.Deserialize<TmdMovie>(responseData, options);
                }
            }
        }
    }

    /// <summary>
    /// Defines the <see cref="Result" />.
    /// </summary>
    public class Result
    {
        /// <summary>
        /// Gets or sets the popularity.
        /// </summary>
        public double popularity { get; set; }

        /// <summary>
        /// Gets or sets the vote_count.
        /// </summary>
        public int vote_count { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether video.
        /// </summary>
        public bool video { get; set; }

        /// <summary>
        /// Gets or sets the poster_path.
        /// </summary>
        public string poster_path { get; set; }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether adult.
        /// </summary>
        public bool adult { get; set; }

        /// <summary>
        /// Gets or sets the backdrop_path.
        /// </summary>
        public string backdrop_path { get; set; }

        /// <summary>
        /// Gets or sets the original_language.
        /// </summary>
        public string original_language { get; set; }

        /// <summary>
        /// Gets or sets the original_title.
        /// </summary>
        public string original_title { get; set; }

        /// <summary>
        /// Gets or sets the genre_ids.
        /// </summary>
        public List<int> genre_ids { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string title { get; set; }

        /// <summary>
        /// Gets or sets the vote_average.
        /// </summary>
        public double vote_average { get; set; }

        /// <summary>
        /// Gets or sets the overview.
        /// </summary>
        public string overview { get; set; }

        /// <summary>
        /// Defines the _releaseDate.
        /// </summary>
        private string _releaseDate;

        /// <summary>
        /// Gets or sets the release_date.
        /// </summary>
        public string release_date
        {
            get
            {
                DateTime dateTime;
                if (DateTime.TryParse(_releaseDate, out dateTime))
                {
                    return string.Format("{0:dddd, MMMM d, yyyy}", dateTime);
                }

                return _releaseDate;
            }
            set
            {
                _releaseDate = value;
            }
        }
    }

    /// <summary>
    /// Defines the <see cref="TmdMoviesInfo" />.
    /// </summary>
    public class TmdMoviesInfo
    {
        /// <summary>
        /// Gets or sets the page.
        /// </summary>
        public int page { get; set; }

        /// <summary>
        /// Gets or sets the total_results.
        /// </summary>
        public int total_results { get; set; }

        /// <summary>
        /// Gets or sets the total_pages.
        /// </summary>
        public int total_pages { get; set; }

        /// <summary>
        /// Gets or sets the results.
        /// </summary>
        public List<Result> results { get; set; }
    }

    /// <summary>
    /// Defines the <see cref="TmdMovie" />.
    /// </summary>
    public class TmdMovie
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// Gets or sets the homepage.
        /// </summary>
        public string homepage { get; set; }
    }
}
