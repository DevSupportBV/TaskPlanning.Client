using TaskPlanning.Models;
using RestEase;
using System;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using TaskPlanning.Client.Api;

namespace TaskPlanning.Client
{
    public class TaskPlanningClient
    {
        private ITaskPlanningApi api;

        /// <summary>
        /// This method will create a new instance of this client class. It will also try to validate the access key, when this happens 
        /// a <see cref="InvalidAccessKeyException"/> will be thrown.
        /// </summary>
        /// <param name="accessKey"></param>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        public static async Task<TaskPlanningClient> Create(string accessKey, string endpoint = null)
        {
            var client = new TaskPlanningClient(endpoint ?? "https://api.taskplanningapi.com/TaskplanningAPI", accessKey);
            try
            {
                await client.GetAccessToken(CancellationToken.None);
            }
            catch (RestEase.ApiException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    throw new InvalidAccessKeyException();
                else
                    throw ex;
            }
            return client;
        }

        private string accessKey;
        private TaskPlanningClient(string endpoint, string accessKey)
        {
            this.accessKey = accessKey;
            api = RestClient.For<ITaskPlanningApi>(endpoint, AddAuthHeader());
        }

        private string accessToken;
        private RequestModifier AddAuthHeader()
        {
            return async (request, cancellationToken) =>
            {
                var auth = request.Headers.Authorization;

                if (auth == null)
                    return;

                var token = accessToken ?? await GetAccessToken(cancellationToken);

                request.Headers.Authorization = new AuthenticationHeaderValue(auth.Scheme, token);
            };
        }

        private async Task<string> GetAccessToken(CancellationToken cancellationToken)
        {
            var response = await api.GetAuthToken(new TokenRequest { AccessKey = accessKey }, cancellationToken);
            return response.AccessToken;
        }

        /// <summary>
        /// This method will start the planning process and handle's the polling to the backend. When the planning is done
        /// the async task will stop and return the <see cref="PlanningTask"/> object.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="pollingInterval"></param>
        /// <returns></returns>
        public Task<PlanningTask> Plan(PlanningRequest request, TimeSpan? pollingInterval = null)
        {
            return Plan(request, CancellationToken.None, pollingInterval);
        }

        /// <summary>
        /// This method will start the planning process and handle's the polling to the backend. When the planning is done or stopped/cancelled
        /// the async task will stop and return the <see cref="PlanningTask"/> object.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="pollingInterval"></param>
        /// <returns></returns>
        public async Task<PlanningTask> Plan(PlanningRequest request, CancellationToken cancellationToken, TimeSpan? pollingInterval = null)
        {
            var pollingTimeSpan = pollingInterval ?? TimeSpan.FromSeconds(5);
            ClearAccessToken();

            PlanningTask planningTask = null;
            try
            {
                planningTask = await api.PostPlanning(request, cancellationToken);

                PlanningTaskUpdated?.Invoke(this, new TaskPlanningUpdateEventArgs(planningTask));
                do
                {
                    await Task.Delay(pollingTimeSpan, cancellationToken);
                    planningTask = await api.GetPlanning(planningTask.Id, cancellationToken);
                    PlanningTaskUpdated?.Invoke(this, new TaskPlanningUpdateEventArgs(planningTask));
                }

                while (planningTask.IsInProgress() && !cancellationToken.IsCancellationRequested);

                if (cancellationToken.IsCancellationRequested)
                {
                    planningTask.Status = PlanningTaskStatus.Canceled;
                    PlanningTaskUpdated?.Invoke(this, new TaskPlanningUpdateEventArgs(planningTask));
                }

                return planningTask;
            }
            catch (ApiException e)
            {
                throw new Exception(e.Content);
            }
            catch (TaskCanceledException e)
            {
                if (planningTask == null)
                    planningTask = new PlanningTask();

                planningTask.Status = PlanningTaskStatus.Canceled;
                planningTask.Exception = e;

                PlanningTaskUpdated?.Invoke(this, new TaskPlanningUpdateEventArgs(planningTask));

                return planningTask;
            }
        }

        public delegate void PlanningTaskUpdatedEventHandler(object sender, TaskPlanningUpdateEventArgs e);
        /// <summary>
        /// This event will be invoked during the planning process. All status update on the <see cref="PlanningTask"/> will be send through this event.
        /// </summary>
        public event PlanningTaskUpdatedEventHandler PlanningTaskUpdated;

        private void ClearAccessToken()
        {
            accessToken = null;
        }

    }
}
