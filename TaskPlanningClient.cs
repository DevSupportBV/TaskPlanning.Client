using AutoPlanning.Models;
using RestEase;
using System;
using System.Net.Http.Headers;
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

        private RequestModifier AddAuthHeader()
        {
            return async (request, cancellationToken) =>
            {
                var auth = request.Headers.Authorization;

                if (auth == null)
                    return;

                var response = await api.GetAuthToken(new TokenRequest { AccessKey = accessKey });

                request.Headers.Authorization = new AuthenticationHeaderValue(auth.Scheme, response.AccessToken);
            };
        }

        public async Task<PlanningTask> Plan(PlanningRequest request)
        {
            PlanningTask task = await api.PostPlanning(request);

            do
            {
                await Task.Delay(5000);
                task = await api.GetPlanning(task.Id);
            }

            while (IsInProgress(task));

            if (task.Status == PlanningTaskStatus.Success)
                return task;
            else
                throw new Exception($"Failed planning: {task.Status}");
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
