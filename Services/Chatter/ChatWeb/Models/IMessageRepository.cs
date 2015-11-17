// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace ChatWeb.Models
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ChatWeb.Domain;

    public interface IMessageRepository
    {
        Task AddMessageAsync(Message message);
        Task<IEnumerable<KeyValuePair<DateTime, Message>>> GetMessages();
        Task ClearMessagesAsync();
    }
}