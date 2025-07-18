using System;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Chromium.AspNetCore.Bridge;
using Microsoft.AspNetCore.Hosting.Server;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.IO;

namespace WebView2.DevTools.Dom.Tests
{
    //Shorthand for Owin pipeline func
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class DevToolsContextFixture : IAsyncLifetime
    {
        public static string UserDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WebView2DevToolsDom\\UserDataFolder");

        private IWebHost _host;
        private static AppFunc _appFunc;

        public static AppFunc AppFunc
        {
            get { return _appFunc; }
        }


        Task IAsyncLifetime.InitializeAsync()
        {
            //We create a UserDataFolder for each test as if a single test gets stuck we end up in trouble
            //Here we delete the parent directory before the test run
            if(Directory.Exists(UserDataFolder))
            {
                Directory.Delete(UserDataFolder, true);
            }

            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

            Task.Run(async () =>
            {
                var builder = new WebHostBuilder();

                builder.ConfigureServices(services =>
                {
                    var server = new OwinServer();
                    server.UseOwin(appFunc =>
                    {
                        _appFunc = appFunc;
                    });

                    services.AddSingleton<IServer>(server);
                });

                builder.ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                });

                var folder = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\..\\TestServer"));

                _host = builder
                    .UseStartup<AspNetStartup>()
                    .UseContentRoot(folder)
                    .Build();

                await _host.RunAsync();
            });

            return Task.CompletedTask;
        }

        Task IAsyncLifetime.DisposeAsync()
        {
            return _host.StopAsync();
        }
    }
}
