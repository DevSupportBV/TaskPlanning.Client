using TaskPlanning.Models;
using Newtonsoft.Json;
using RestEase;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TaskPlanning.Client.Api
{
    [Header("User-Agent", "TaskPlanning.Client.Api")]
    interface ITaskPlanningApi
    {
        [Post("api/Authentication/token")]
        Task<TokenResponse> GetAuthToken([Body]TokenRequest tokenRequest, CancellationToken cancellationToken);

        [Post("api/Planning")]
        [Header("Authorization", "Bearer")]
        Task<PlanningTask> PostPlanning([Body]PlanningRequest request, CancellationToken cancellationToken);

        [Get("api/Planning/{planningTaskId}")]
        [Header("Authorization", "Bearer")]
        Task<PlanningTask> GetPlanning([Path] Guid planningTaskId, CancellationToken cancellationToken);
    }
}
