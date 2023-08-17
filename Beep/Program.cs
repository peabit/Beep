using Quartz;
using Beep;

const string triggerName = "NotifierTrigger";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddQuartz(cfg =>
{
    cfg.UseMicrosoftDependencyInjectionJobFactory();
    
    var jobKey = new JobKey("Notifier");

    cfg.AddJob<NotifyJob>(opts => opts.WithIdentity(jobKey));

    cfg.AddTrigger(opts =>
    {
        opts
            .ForJob(jobKey)
            .WithIdentity(triggerName)
            .StartNow()
            .WithSimpleSchedule(triggerCfg =>
            {
                triggerCfg
                    .WithInterval(TimeSpan.FromSeconds(5))
                    .RepeatForever();
            });
    });
});

builder.Services.AddQuartzHostedService(opts => opts.WaitForJobsToComplete = true);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost(
    "/change-interval&{seconds:int:min(1)}",
    async (int seconds, ISchedulerFactory schedulerFactory) =>
    {
        var scheduler = await schedulerFactory.GetScheduler();
        
        var trigger = TriggerBuilder
            .Create()
            .WithIdentity(triggerName)
            .StartNow()
            .WithSimpleSchedule(opt => opt
                .WithIntervalInSeconds(seconds)
                .RepeatForever()
            )
            .Build();

        await scheduler.RescheduleJob(new TriggerKey(triggerName), trigger);
    }
);

app.Run();