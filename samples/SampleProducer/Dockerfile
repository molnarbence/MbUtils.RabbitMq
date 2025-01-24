#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["*.props", "./"]
COPY ["samples/SampleProducer/SampleProducer.csproj", "samples/SampleProducer/"]
COPY ["src/Producer/MbUtils.RabbitMq.Producer.csproj", "src/Producer/"]
COPY ["src/Producer.Abstractions/MbUtils.RabbitMq.Producer.Abstractions.csproj", "src/Producer.Abstractions/"]
RUN dotnet restore "samples/SampleProducer/SampleProducer.csproj"
COPY . .
WORKDIR "/src/samples/SampleProducer"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SampleProducer.dll", "produce"]