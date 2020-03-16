using System;
using System.Collections.Generic;
using System.Text;
using TaskPlanning.Models;

namespace TaskPlanning.Client
{
    public class TaskPlanningUpdateEventArgs : EventArgs
    {
        public TaskPlanningUpdateEventArgs(PlanningTask planningTask)
        {
            this.PlanningTask = planningTask;
        }

        public bool IsInProgress => PlanningTask?.IsInProgress() ?? false;

        public PlanningTask PlanningTask { get; private set; }
    }
}
