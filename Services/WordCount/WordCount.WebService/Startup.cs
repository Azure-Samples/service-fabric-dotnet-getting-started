// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace WordCount.WebService
{
    using System.Web.Http;
    using Owin;
    using WordCount.Common;
    using Microsoft.Owin.StaticFiles;
    using Microsoft.Owin;
    public class Startup : IOwinAppBuilder
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            appBuilder.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
            System.Net.ServicePointManager.DefaultConnectionLimit = 256;

            HttpConfiguration config = new HttpConfiguration();

            FormatterConfig.ConfigureFormatters(config.Formatters);
            RouteConfig.RegisterRoutes(config.Routes);

            SetUpStaticFileHosting(appBuilder);
            appBuilder.UseWebApi(config);
            
        }

        private void SetUpStaticFileHosting(IAppBuilder appBuilder)
        {
            var options = new FileServerOptions
            {
                RequestPath = new PathString("/wwwroot"),
                EnableDirectoryBrowsing = true
            };
            appBuilder.UseFileServer(options);
            appBuilder.UseStaticFiles();
        }
    }
}