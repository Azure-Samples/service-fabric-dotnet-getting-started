// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace ChatWeb.Controllers
{
    using Microsoft.AspNet.Mvc;

    // This controller loads the single index.html page
    public class ChatterController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index()
        {
            return this.View();
        }
    }
}