// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Weloveloli">
//     Copyright (c) Weloveloli.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Weloveloli.AVGui
{
    using System;
    using Chromely;
    using Chromely.Core;
    using Chromely.Core.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Defines the <see cref="Program" />.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// The Main.
        /// </summary>
        /// <param name="args">The args<see cref="string[]"/>.</param>
        [STAThread]
        internal static void Main(string[] args)
        {
            var config = DefaultConfiguration.CreateForRuntimePlatform();
            config.StartUrl = "local://dist/index.html";

            AppBuilder
            .Create()
            .UseConfig<DefaultConfiguration>(config)
            .UseApp<DemoApp>()
            .Build()
            .Run(args);
        }
    }

    /// <summary>
    /// Defines the <see cref="DemoApp" />.
    /// </summary>
    public class DemoApp : ChromelyBasicApp
    {
        /// <summary>
        /// The ConfigureServices.
        /// </summary>
        /// <param name="services">The services<see cref="ServiceCollection"/>.</param>
        public override void ConfigureServices(ServiceCollection services)
        {
            base.ConfigureServices(services);
            services.AddLogging(configure => configure.AddConsole());
            services.AddLogging(configure => configure.AddFile("Logs/serilog-{Date}.txt"));

            /*
            // Optional - adding custom handler
            services.AddSingleton<CefDragHandler, CustomDragHandler>();
            */

            /*
            // Optional- using config section to register IChromelyConfiguration
            // This just shows how it can be used, developers can use custom classes to override this approach
            //
            var builder = new ConfigurationBuilder()
                    .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json");
            var configuration = builder.Build();
            var config = DefaultConfiguration.CreateFromConfigSection(configuration);
            services.AddSingleton<IChromelyConfiguration>(config);
            */

            /* Optional
            var options = new JsonSerializerOptions();
            options.ReadCommentHandling = JsonCommentHandling.Skip;
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.AllowTrailingCommas = true;
            services.AddSingleton<JsonSerializerOptions>(options);
            */

            RegisterControllerAssembly(services, typeof(DemoApp).Assembly);
        }
    }

    // Windows only
    /// <summary>
    /// Defines the <see cref="Demo2App" />.
    /// </summary>
    public class Demo2App : ChromelyFramelessApp
    {
        /// <summary>
        /// The ConfigureServices.
        /// </summary>
        /// <param name="services">The services<see cref="ServiceCollection"/>.</param>
        public override void ConfigureServices(ServiceCollection services)
        {
            base.ConfigureServices(services);
            services.AddLogging(configure => configure.AddConsole());
            services.AddLogging(configure => configure.AddFile("Logs/serilog-{Date}.txt"));

            RegisterControllerAssembly(services, typeof(DemoApp).Assembly);
        }
    }
}
