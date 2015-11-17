// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace ChatWeb.Models
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using ChatWeb.Domain;

    public class MessageRepository : IMessageRepository
    {
        //This is a local message repository that holds the data in a volatile, ConcurrentDictionary
        private static ConcurrentDictionary<DateTime, Message> Messages = new ConcurrentDictionary<DateTime, Message>();

        public Task AddMessageAsync(Message message)
        {
            Messages[DateTime.Now] = message;
            return Task.FromResult(true);
        }

        public Task ClearMessagesAsync()
        {
            Messages.Clear();
            return Task.FromResult(true);
        }

        Task<IEnumerable<KeyValuePair<DateTime, Message>>> IMessageRepository.GetMessages()
        {
            return Task.FromResult(Messages.Select(x => x));
        }
    }
}