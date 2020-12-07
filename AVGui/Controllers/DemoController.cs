// Copyright © 2017-2020 Chromely Projects. All rights reserved.
// Use of this source code is governed by MIT license that can be found in the LICENSE file.

namespace Weloveloli.AVGui.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text.Json;
    using Chromely;
    using Chromely.Core;
    using Chromely.Core.Configuration;
    using Chromely.Core.Network;

    /// <summary>
    /// The demo controller.
    /// </summary>
    [ControllerProperty(Name = "DemoController")]
    public class DemoController : ChromelyController
    {
        /// <summary>
        /// Defines the _config.
        /// </summary>
        private readonly IChromelyConfiguration _config;

        /// <summary>
        /// Defines the _serializerUtil.
        /// </summary>
        private readonly IChromelySerializerUtil _serializerUtil;

        /// <summary>
        /// Initializes a new instance of the <see cref="DemoController"/> class.
        /// </summary>
        /// <param name="config">The config<see cref="IChromelyConfiguration"/>.</param>
        /// <param name="serializerUtil">The serializerUtil<see cref="IChromelySerializerUtil"/>.</param>
        public DemoController(IChromelyConfiguration config, IChromelySerializerUtil serializerUtil)
        {
            _config = config;
            _serializerUtil = serializerUtil;

            RegisterRequest("/democontroller/movies/get", GetMovies);
            RegisterRequest("/democontroller/movies/post", SaveMovies);
        }

        /// <summary>
        /// The ShowDevTools.
        /// </summary>
        /// <param name="queryParameters">The queryParameters<see cref="IDictionary{string, string}"/>.</param>
        [CommandAction(RouteKey = "/democontroller/showdevtools")]
        public void ShowDevTools(IDictionary<string, string> queryParameters)
        {
            if (_config != null && !string.IsNullOrWhiteSpace(_config.DevToolsUrl))
            {
                BrowserLauncher.Open(_config.Platform, _config.DevToolsUrl);
            }
        }

        /// <summary>
        /// The GetMovies.
        /// </summary>
        /// <param name="request">The request<see cref="IChromelyRequest"/>.</param>
        /// <returns>The <see cref="IChromelyResponse"/>.</returns>
        private IChromelyResponse GetMovies(IChromelyRequest request)
        {
            var movieInfos = new List<MovieInfo>();
            var assemblyName = typeof(MovieInfo).Assembly.GetName().Name;

            movieInfos.Add(new MovieInfo(id: 1, title: "The Shawshank Redemption", year: 1994, votes: 678790, rating: 9.2, assembly: assemblyName));
            movieInfos.Add(new MovieInfo(id: 2, title: "The Godfather", year: 1972, votes: 511495, rating: 9.2, assembly: assemblyName));
            movieInfos.Add(new MovieInfo(id: 3, title: "The Godfather: Part II", year: 1974, votes: 319352, rating: 9.0, assembly: assemblyName));
            movieInfos.Add(new MovieInfo(id: 4, title: "The Good, the Bad and the Ugly", year: 1966, votes: 213030, rating: 8.9, assembly: assemblyName));
            movieInfos.Add(new MovieInfo(id: 5, title: "My Fair Lady", year: 1964, votes: 533848, rating: 8.9, assembly: assemblyName));
            movieInfos.Add(new MovieInfo(id: 6, title: "12 Angry Men", year: 1957, votes: 164558, rating: 8.9, assembly: assemblyName));

            return new ChromelyResponse(request.Id)
            {
                Data = movieInfos
            };
        }

        /// <summary>
        /// The SaveMovies.
        /// </summary>
        /// <param name="request">The request<see cref="IChromelyRequest"/>.</param>
        /// <returns>The <see cref="IChromelyResponse"/>.</returns>
        private IChromelyResponse SaveMovies(IChromelyRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (request.PostData == null)
            {
                throw new Exception("Post data is null or invalid.");
            }

            var response = new ChromelyResponse(request.Id);
            var postDataJson = _serializerUtil.ObjectToJson(request.PostData);

            var options = new JsonSerializerOptions();
            options.ReadCommentHandling = JsonCommentHandling.Skip;
            options.AllowTrailingCommas = true;
            var movies = JsonSerializer.Deserialize<List<MovieInfo>>(postDataJson, options);
            var rowsReceived = movies != null ? movies.Count : 0;
            response.Data = $"{DateTime.Now}: {rowsReceived} rows of data successfully saved.";

            return response;
        }
    }

    /// <summary>
    /// The movie info.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("ReSharper", "StyleCop.SA1600")]
    public class MovieInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MovieInfo"/> class.
        /// </summary>
        public MovieInfo()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MovieInfo"/> class.
        /// </summary>
        /// <param name="id">The id<see cref="int"/>.</param>
        /// <param name="title">The title<see cref="string"/>.</param>
        /// <param name="year">The year<see cref="int"/>.</param>
        /// <param name="votes">The votes<see cref="int"/>.</param>
        /// <param name="rating">The rating<see cref="double"/>.</param>
        /// <param name="assembly">The assembly<see cref="string"/>.</param>
        public MovieInfo(int id, string title, int year, int votes, double rating, string assembly)
        {
            Id = id;
            Title = title;
            Year = year;
            Votes = votes;
            Rating = rating;
            Date = DateTime.Now;
            RestfulAssembly = assembly;
        }

        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the Title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the Year.
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Gets or sets the Votes.
        /// </summary>
        public int Votes { get; set; }

        /// <summary>
        /// Gets or sets the Rating.
        /// </summary>
        public double Rating { get; set; }

        /// <summary>
        /// Gets or sets the Date.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the RestfulAssembly.
        /// </summary>
        public string RestfulAssembly { get; set; }
    }
}
