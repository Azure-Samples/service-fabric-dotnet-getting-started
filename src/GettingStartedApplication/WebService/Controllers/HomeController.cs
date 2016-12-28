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
            return View();
        }

        public IActionResult Stateless()
        {
            return View();
        }

        public IActionResult Stateful()
        {
            return View();
        }

        public IActionResult Actor()
        {
            return View();
        }
    }
}