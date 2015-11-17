// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace ChatWeb.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ChatWeb.Domain;
    using ChatWeb.Models;
    using Microsoft.AspNet.Mvc;

    // This controller accepts WebApi calls from the index.html when the AJAX calls are made
    [Route("api/[controller]")]
    public class ChatController : Controller
    {
        [FromServices]
        public IMessageRepository Messages { get; set; }

        // GET: api/chat
        [HttpGet]
        public Task<IEnumerable<KeyValuePair<DateTime, Message>>> GetMessages()
        {
            return this.Messages.GetMessages();
        }

        // POST api/chat
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] Message message)
        {
            if (message == null)
            {
                ServiceEventSource.Current.WebControllerMessage(this, "Received message with no content");
                return this.HttpBadRequest();
            }
            await this.Messages.AddMessageAsync(message);
            return new NoContentResult();
        }

        //DELETE api/chat
        [HttpDelete]
        public async Task<IActionResult> ClearMessages()
        {
            await this.Messages.ClearMessagesAsync();
            return new NoContentResult();
        }
    }
}