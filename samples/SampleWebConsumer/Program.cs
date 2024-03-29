using MbUtils.RabbitMq.Consumer;
using SampleWebConsumer;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// Add services to the container.
builder.Services
   .AddRabbitMqConsumer<TestConsumer>(builder.Configuration.GetSection(nameof(TestConsumer)))
   .AddRabbitMqConsumer<TestConsumer2>(builder.Configuration.GetSection(nameof(TestConsumer2)));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.MapGet("/operations/healthcheck", (IConsumerStatusManager consumerStatusManager) => {
   var currentStatus = consumerStatusManager.CurrentStatusInfo;
   return Results.Ok(currentStatus);
});

app.Run();
