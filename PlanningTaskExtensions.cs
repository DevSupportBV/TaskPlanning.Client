using System;
using System.Collections.Generic;
using System.Text;
using TaskPlanning.Models;

namespace TaskPlanning.Client
{
    public static class PlanningTaskExtensions
    {
        public static bool IsInProgress(this PlanningTask task) => task.Status switch
        {
            PlanningTaskStatus.Queued => true,
            PlanningTaskStatus.Running => true,

            PlanningTaskStatus.Success => false,
            PlanningTaskStatus.Canceled => false,
            PlanningTaskStatus.Error => false,

            _ => throw new NotImplementedException($"Please implement {nameof(PlanningTaskStatus)} value {task.Status}")
        };
    }
}
