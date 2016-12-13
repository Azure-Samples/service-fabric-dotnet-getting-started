// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace WebService.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    public class HomeController : Controller
    {

        public IActionResult Index()
        {
            return this.View();
        }

        //public IActionResult About()
        //{
        //    this.ViewData["Message"] = "Your application description page.";

        //    return this.View();
        //}

        //public IActionResult Contact()
        //{
        //    this.ViewData["Message"] = "Your contact page.";

        //}

        public IActionResult Error()
        {
            return this.View();
        }
    }
}