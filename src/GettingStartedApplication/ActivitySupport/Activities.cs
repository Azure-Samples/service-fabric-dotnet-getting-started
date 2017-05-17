using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace Microsoft.Diagnostics.Activities
{
    public class Activities
    {
        private static TelemetryClient telemetryClient = new TelemetryClient();

        public static async Task<T> ServiceRemotingDependencyCallAsync<T>(
            Func<Task<T>> dependencyCallAction,
            string dependencyType,
            string dependencyName, 
            string target,
            string additionalData = "",
            string activityName = "ServiceRemotingOut")
        {
            DependencyTelemetry dt = new DependencyTelemetry();
            dt.Type = dependencyType;
            dt.Name = dependencyName;
            dt.Target = target;
            dt.Data = additionalData;

            var operation = telemetryClient.StartOperation<DependencyTelemetry>(dt);
            bool success = true;
            string responseCode = string.Empty;
            try
            {
                T retval = await dependencyCallAction().ConfigureAwait(false);
                return retval;
            }
            catch (Exception e)
            {
                success = false;
                responseCode = e.ToString();
                telemetryClient.TrackException(e);
                throw;
            }
            finally
            {
                dt.Success = success;
                dt.ResultCode = responseCode;
                telemetryClient.StopOperation(operation);
            }
        }

        public static async Task<T> HandleServiceRemotingRequestAsync<T>(
            Func<Task<T>> requestHandler,
            string requestId,
            string requestName,
            IEnumerable<KeyValuePair<string, string>> correlationContext,
            string activityName = "ServiceRemotingIn")
        {
            RequestTelemetry rt = SetUpRequestActivity(requestId, requestName, correlationContext, activityName);

            bool success = true;
            string responseCode = string.Empty;
            try
            {
                T retval = await requestHandler().ConfigureAwait(false);
                return retval;
            }
            catch (Exception e)
            {
                success = false;
                responseCode = e.ToString();
                telemetryClient.TrackException(e);
                throw;
            }
            finally
            {
                Activity.Current.Stop();

                rt.Stop(Stopwatch.GetTimestamp());
                rt.Success = success;
                telemetryClient.TrackRequest(rt);
            }
        }

        public static async Task HandleActorRequestAsync(
            Func<Task> requestHandler,
            string requestId,
            string requestName,
            IEnumerable<KeyValuePair<string, string>> correlationContext,
            string activityName = "ActorCall")
        {
            RequestTelemetry rt = SetUpRequestActivity(requestId, requestName, correlationContext, activityName);

            bool success = true;
            string responseCode = string.Empty;
            try
            {
                await requestHandler().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                success = false;
                responseCode = e.ToString();
                telemetryClient.TrackException(e);
                throw;
            }
            finally
            {
                Activity.Current.Stop();

                rt.Stop(Stopwatch.GetTimestamp());
                rt.Success = success;
                telemetryClient.TrackRequest(rt);
            }
        }

        private static RequestTelemetry SetUpRequestActivity(
            string requestId, 
            string requestName, 
            IEnumerable<KeyValuePair<string, string>> correlationContext, 
            string activityName)
        {
            var activity = new Activity(activityName);
            activity.SetParentId(requestId);
            RequestTelemetry rt = new RequestTelemetry();
            rt.Context.Operation.ParentId = requestId;

            foreach (KeyValuePair<string, string> pair in correlationContext)
            {
                activity.AddBaggage(pair.Key, pair.Value);
            }

            activity.Start();

            rt.Id = activity.Id;
            rt.Context.Operation.Id = activity.RootId;
            rt.Name = requestName;

            telemetryClient.Initialize(rt);
            rt.Start(Stopwatch.GetTimestamp());
            return rt;
        }
    }
}
