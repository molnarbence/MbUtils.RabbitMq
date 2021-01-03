using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;

namespace MbUtils.RabbitMq.Consumer
{
   public class ConsumerHostBuilder : IHostBuilder
   {
      private readonly IHostBuilder _hostBuilder;
      public IDictionary<object, object> Properties => _hostBuilder.Properties;

      public ConsumerHostBuilder(string[] args)
      {
         if (args is null)
         {
            throw new ArgumentNullException(nameof(args));
         }

         _hostBuilder = Host.CreateDefaultBuilder(args);
      }

      public IHost Build() => _hostBuilder.Build();

      public IHostBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate) => _hostBuilder.ConfigureAppConfiguration(configureDelegate);

      public IHostBuilder ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate) => _hostBuilder.ConfigureContainer(configureDelegate);

      public IHostBuilder ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate) => _hostBuilder.ConfigureHostConfiguration(configureDelegate);

      public IHostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate) => _hostBuilder.ConfigureServices(configureDelegate);

      public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory) => _hostBuilder.UseServiceProviderFactory(factory);

      public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory) => _hostBuilder.UseServiceProviderFactory(factory);
   }
}
