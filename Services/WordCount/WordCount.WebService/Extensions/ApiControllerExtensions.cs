// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace System.Web.Http
{
    using System.IO;
    using System.Net.Http;
    using System.Reflection;
    using System.Text;

    /// <summary>
    /// Extension methods for ApiController to help render HTML and JS files.
    /// This is here because we are using Web API but want to also render an HTML page.
    /// Normally we use ASP.NET MVC for this, but that still requires IIS (System.Web) at this time and we need to self-host,
    /// for which we use OWIN. 
    /// ASP.NET vNext will allow self-host with MVC, at which point we won't need these extensions any more.
    /// </summary>
    public static class ApiControllerExtensions
    {
        /// <summary>
        /// Creates an HttpResponseMessage from the specified view with the specified media type.
        /// The view must a fully-qualified assembly name of an embedded resources.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="view"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static HttpResponseMessage View(this ApiController instance, string view, string mediaType)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            using (Stream stream = assembly.GetManifestResourceStream(view))
            using (StreamReader reader = new StreamReader(stream))
            {
                HttpResponseMessage message = new HttpResponseMessage();
                message.Content = new StringContent(reader.ReadToEnd(), Encoding.UTF8, mediaType);
                return message;
            }
        }
    }
}