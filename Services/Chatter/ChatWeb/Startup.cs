// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace ChatWeb
{
    using ChatWeb.Models;
    using Microsoft.AspNet.Builder;
    using Microsoft.AspNet.Hosting;    
    using ChatWeb.Domain;
    using Microsoft.Extensions.DependencyInjection;

    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
        }

        // This method gets called by a runtime.
        // Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddScoped<IMessageRepository, ReliableMessageRepository>();
            // Uncomment line below to use local, concurrentDictionary to hold messages
            //services.AddScoped<IMessageRepository, MessageRepository>();
        }

        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // Configure the HTTP request pipeline.
            app.UseStaticFiles();

            // Add MVC to the request pipeline.
            app.UseMvc(
                routes =>
                {
                    routes.MapRoute(
                        name: "default",
                        template: "{controller=Chatter}/{action=Index}/{id?}");
                });
        }
    }
}