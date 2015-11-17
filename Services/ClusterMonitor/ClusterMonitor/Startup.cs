// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace ClusterMonitor
{
    using System;
    using System.Diagnostics;
    using System.Fabric.Description;
    using System.Web.Http;
    using Microsoft.Owin;
    using Microsoft.Owin.FileSystems;
    using Microsoft.Owin.StaticFiles;
    using Owin;

    public class Startup : IOwinAppBuilder
    {
        private readonly ConfigurationSettings configSettings;

        public Startup(ConfigurationSettings configSettings)
        {
            this.configSettings = configSettings;
        }

        public void Configuration(IAppBuilder appBuilder)
        {
            HttpConfiguration config = new HttpConfiguration();

            PhysicalFileSystem physicalFileSystem = new PhysicalFileSystem(@".\wwwroot");
            FileServerOptions fileOptions = new FileServerOptions();

            fileOptions.EnableDefaultFiles = true;
            fileOptions.RequestPath = PathString.Empty;
            fileOptions.FileSystem = physicalFileSystem;
            fileOptions.DefaultFilesOptions.DefaultFileNames = new[] {"index.html"};
            fileOptions.StaticFileOptions.FileSystem = fileOptions.FileSystem = physicalFileSystem;
            fileOptions.StaticFileOptions.ServeUnknownFileTypes = true;

            try
            {
                config.MessageHandlers.Add(new ProxyHandler(this.configSettings));

                appBuilder.UseWebApi(config);
                appBuilder.UseFileServer(fileOptions);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
            }
        }
    }
}