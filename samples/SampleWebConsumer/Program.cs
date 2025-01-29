using SampleWebConsumer;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// Add services to the container.
builder.AddServiceDefaults();

builder
   .AddRabbitMqConsumer<TestConsumer>(config.GetSection(nameof(TestConsumer)), "messaging")
   .AddRabbitMqConsumer<TestConsumer2>(config.GetSection(nameof(TestConsumer2)), "messaging");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.Run();
