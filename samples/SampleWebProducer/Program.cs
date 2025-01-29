using System.Text;
using MbUtils.RabbitMq.Producer;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client.Events;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddRabbitMqMessageProducer(connectionName: "messaging");

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
   app.MapOpenApi();
}

app.MapGet("/produce/{queueName}/{textMessage}", (string textMessage, string queueName, [FromServices] IMessageProducerFactory messageProducerFactory) =>
{
   var producer = messageProducerFactory.Create(queueName);
   producer.Produce(Encoding.UTF8.GetBytes(textMessage));

   return $"Message {textMessage} sent to {queueName}";
});

app.Run();
