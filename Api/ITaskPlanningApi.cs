﻿using AutoPlanning.Models;
using Newtonsoft.Json;
using RestEase;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TaskPlanning.Client.Api
{
    [Header("User-Agent", "TaskPlanning.Client.Api")]
    interface ITaskPlanningApi
    {
        [Post("api/Authentication/token")]
        Task<TokenResponse> GetAuthToken([Body]TokenRequest tokenRequest);

        [Post("api/Planning")]
        [Header("Authorization", "Bearer")]
        Task<PlanningTask> PostPlanning([Body]PlanningRequest request);

        [Get("api/Planning/{planningTaskId}")]
        [Header("Authorization", "Bearer")]
        Task<PlanningTask> GetPlanning([Path] Guid planningTaskId);
    }

    public class TokenRequest //TODO: Move to Models library
    {
        public string AccessKey { get; set; }
    }

    public class TokenResponse //TODO: Move to Models library
    {
        [JsonProperty("access_Token")]
        public string AccessToken { get; set; }
    }
}
