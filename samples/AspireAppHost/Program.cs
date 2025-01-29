var builder = DistributedApplication.CreateBuilder(args);

var messaging = builder.AddRabbitMQ("messaging").WithLifetime(ContainerLifetime.Persistent).WithManagementPlugin();

builder.AddProject<Projects.SampleWebConsumer>("SampleWebConsumer").WithReference(messaging);
builder.AddProject<Projects.SampleWebProducer>("SampleWebProducer").WithReference(messaging);

builder.Build().Run();