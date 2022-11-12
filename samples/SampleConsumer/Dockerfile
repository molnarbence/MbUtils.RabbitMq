#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["samples/SampleConsumer/SampleConsumer.csproj", "samples/SampleConsumer/"]
COPY ["src/Consumer/MbUtils.RabbitMq.Consumer.csproj", "src/Consumer/"]
RUN dotnet restore "samples/SampleConsumer/SampleConsumer.csproj"
COPY . .
WORKDIR "/src/samples/SampleConsumer"
RUN dotnet build "SampleConsumer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SampleConsumer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SampleConsumer.dll"]