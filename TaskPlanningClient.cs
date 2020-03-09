using AutoPlanning.Models;
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

        public static TaskPlanningClient Create(string accessKey)
        {
            return new TaskPlanningClient("https://api.taskplanningapi.com/TaskplanningAPI", accessKey);
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

        public Task<PlanningTask> Plan(PlanningRequest request)
        {
            return Plan(request, CancellationToken.None);
        }
        public async Task<PlanningTask> Plan(PlanningRequest request, CancellationToken cancellationToken)
        {
            ClearAccessToken();

            PlanningTask planningTask = null;
            try
            {
                planningTask = await api.PostPlanning(request, cancellationToken);

                do
                {
                    await Task.Delay(5000, cancellationToken);
                    planningTask = await api.GetPlanning(planningTask.Id, cancellationToken);
                }

                while (IsInProgress(planningTask) && !cancellationToken.IsCancellationRequested);

                if (cancellationToken.IsCancellationRequested)
                    planningTask.Status = PlanningTaskStatus.Canceled;

                return planningTask;
            }
            catch (TaskCanceledException e)
            {
                if (planningTask == null)
                    planningTask = new PlanningTask();

                planningTask.Status = PlanningTaskStatus.Canceled;
                planningTask.Exception = e;

                return planningTask;
            }
        }

        private void ClearAccessToken()
        {
            accessToken = null;
        }

        private bool IsInProgress(PlanningTask task) => task.Status switch
        {
            PlanningTaskStatus.NotRunning => true,
            PlanningTaskStatus.Running    => true,

            PlanningTaskStatus.Success  => false,
            PlanningTaskStatus.Canceled => false,
            PlanningTaskStatus.Error    => false,

            _ => throw new NotImplementedException($"Please implement {nameof(PlanningTaskStatus)} value {task.Status}")
        };
    }
}
