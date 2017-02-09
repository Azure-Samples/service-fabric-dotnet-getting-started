// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Common
{
    using System;
    using System.Runtime.Remoting.Messaging;
    using System.Text;
    using Microsoft.ServiceFabric.Services.Remoting;

    /// <summary>
    /// Activity id's can be used in causailty tracking when it flows through messages that are
    /// passed in the system. This class uses the 'CallContext' to store the current activity id.
    /// </summary>
    public class ActivityId
    {
        private const string ActivityIdKeyName = "__ActivityId__";

        public static void SetActivityIdHeader(ServiceRemotingMessageHeaders headers)
        {
            string activityId = GetOrCreateActivityId();
            headers.AddHeader(ActivityIdKeyName, Encoding.UTF8.GetBytes(activityId));
        }

        public static void UpdateCurrentActivityId(ServiceRemotingMessageHeaders headers)
        {
            byte[] headerValue;
            if (!headers.TryGetHeaderValue(ActivityIdKeyName, out headerValue))
            {
                return;
            }

            UpdateCurrentActivityId(Encoding.UTF8.GetString(headerValue));
        }

        public static void UpdateCurrentActivityId(string activityId)
        {
            CallContext.LogicalSetData(ActivityIdKeyName, activityId);
        }

        public static bool TryGetCurrentActivityId(out string activityId)
        {
            activityId = (string) CallContext.LogicalGetData(ActivityIdKeyName);
            return (activityId != null);
        }

        public static string GetOrCreateActivityId()
        {
            string activityId;
            if (!TryGetCurrentActivityId(out activityId))
            {
                activityId = Guid.NewGuid().ToString();
                UpdateCurrentActivityId(activityId);
            }

            return activityId;
        }
    }
}