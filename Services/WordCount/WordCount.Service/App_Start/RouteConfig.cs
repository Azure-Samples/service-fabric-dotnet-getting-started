// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace WordCount.Service
{
    using System.Web.Http;

    public static class RouteConfig
    {
        /// <summary>
        /// Routing registration.
        /// </summary>
        /// <param name="routes">The Http routes</param>
        public static void RegisterRoutes(HttpRouteCollection routes)
        {
            routes.MapHttpRoute(
                name: "AddWord",
                routeTemplate: "AddWord/{word}",
                defaults: new {controller = "Default", action = "AddWord"},
                constraints: new {}
                );

            routes.MapHttpRoute(
                name: "Count",
                routeTemplate: "Count",
                defaults: new {controller = "Default", action = "Count"},
                constraints: new {}
                );
        }
    }
}