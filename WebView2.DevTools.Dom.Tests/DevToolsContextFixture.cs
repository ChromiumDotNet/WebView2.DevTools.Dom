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
            var userDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WebView2\\UserDataFolder");

            if(Directory.Exists(userDataFolder))
            {
                Directory.Delete(userDataFolder, true);
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

                var folder = TestUtils.FindParentDirectory("WebView2.DevTools.Dom.TestServer");

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
