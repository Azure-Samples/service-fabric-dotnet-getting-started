// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.Azure.Service.Fabric.Samples.VoicemailBoxWebService
{
    using System.Web.Http;

    public static class RouteConfig
    {
        /// <summary>
        /// Routing registration.
        /// </summary>
        /// <param name="routes"></param>
        public static void RegisterRoutes(HttpRouteCollection routes)
        {
            routes.MapHttpRoute(
                name: "Default",
                routeTemplate: "{action}",
                defaults: new {controller = "Default", action = "Index"},
                constraints: new {}
                );

            routes.MapHttpRoute(
                name: "GetActorID",
                routeTemplate: "{action}",
                defaults: new {controller = "Default", action = "GetActorID"},
                constraints: new {}
                );

            routes.MapHttpRoute(
                name: "GetGreeting",
                routeTemplate: "{action}",
                defaults: new {controller = "Default", action = "GetGreeting"},
                constraints: new {}
                );

            routes.MapHttpRoute(
                name: "SetGreeting",
                routeTemplate: "SetGreeting/{greeting}",
                defaults: new {controller = "Default", action = "SetGreeting"},
                constraints: new {}
                );

            routes.MapHttpRoute(
                name: "GetMessages",
                routeTemplate: "{action}",
                defaults: new {controller = "Default", action = "GetMessages"},
                constraints: new {}
                );

            routes.MapHttpRoute(
                name: "LeaveMessage",
                routeTemplate: "LeaveMessage/{message}",
                defaults: new {controller = "Default", action = "LeaveMessage"},
                constraints: new {}
                );

            routes.MapHttpRoute(
                name: "DeleteMessage",
                routeTemplate: "{action}",
                defaults: new {controller = "Default", action = "DeleteMessage"},
                constraints: new {}
                );

            routes.MapHttpRoute(
                name: "DeleteAllMessages",
                routeTemplate: "{action}",
                defaults: new {controller = "Default", action = "DeleteAllMessages"},
                constraints: new {}
                );

            routes.MapHttpRoute(
                name: "Files",
                routeTemplate: "Files/{name}",
                defaults: new {controller = "File", action = "Get"},
                constraints: new {}
                );
        }
    }
}