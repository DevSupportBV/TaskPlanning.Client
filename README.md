# TaskPlanning.Client
![NuGet](https://img.shields.io/nuget/v/TaskPlanning.Client?label=TaskPlanning.Client "NuGet")

You can use this library to access taskplanningapi.com's scheduling API from .Net environments.

## Related
- [TaskPlanning.Client.Sample](https://github.com/DevSupportBV/TaskPlanning.Client.Sample): Example console application highlighting key features.
- [TaskPlanning.Models](https://www.nuget.org/packages/TaskPlanning.Models/): Contains only the DTO models with Newtonsoft Json.NET attributes for serialization. ![NuGet](https://img.shields.io/nuget/v/TaskPlanning.Models?label=TaskPlanning.Models "NuGet")

## Basic usage

### TaskPlanningClient
```C#
//Use your private access key to create instance of the TaskPlanningClient
var accessKey = "YourAccessKey";
var client = TaskPlanningClient.Create(accessKey);

//Create a planning request
var request = new PlanningRequest(); //More on this later

//Pass request to the Plan method and await the PlanningTask result.
PlanningTask planning = await client.Plan(request);
```

### PlanningRequest
This data structure contain all your available data that is needed to perform a planning request. 
#### PlanningRequest.PlanningWindow
This must contain the planning window you would like to plan. You can specify the Start en End date time
#### PlanningRequest.Resources
Here you can specify all your resources that can be planned. Resources can be anything e.g. a person. Every resource can also have different time windows. 
#### PlanningRequest.Mode
With this you can specify which planning mode you want to use. 
```C#
    public enum PlanningMode
    {
        Regular = 0,
        Options = 1
    }
```
Planning mode Regular will try to plan all items. With planning mode Options you can get back all the options for the provided plan items, this can be useful if you would like to reschedule a plan item.

#### PlanningRequest.PendingPlanItems
These are the items that should be planned.


### PlanningTask
The PlanningTask data structure will contain information about the execution of your planning request.
When the request is fully completed the status becomes Success.


```C#
public class PlanningTask
{
    public virtual Guid Id { get; set; }
    public virtual TimeSpan? Duration { get; set; }
    public virtual Planning Planning { get; set; }
    public virtual int Progress { get; set; }
    public virtual Exception Exception { get; set; }
    public virtual PlanningTaskStatus Status { get; set; }
}
```
### PlanningTaskStatus enum
The PlanningTaskStatus enum contains the various stages a planning request job can have.
Some of them are for internal use because this library handles status polling for you.
You should only encounter **Success**, **Error** and if you make use of cancellation tokens you can also expect **Canceled**.

```C#
public enum PlanningTaskStatus
{
    Running = 0,
    Success = 1,
    Canceled = 2,
    Queued = 3,
    Error = 99
}
```
