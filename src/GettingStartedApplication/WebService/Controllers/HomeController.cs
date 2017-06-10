﻿// ------------------------------------------------------------
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

        public IActionResult GuestExe()
        {
            return this.View();
        }

        public IActionResult Stateless()
        {
            return this.View();
        }

        public IActionResult Stateful()
        {
            return this.View();
        }

        public IActionResult Actor()
        {
            return this.View();
        }

        public IActionResult Tenant()
        {
            return this.View();
        }
    }
}