#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["*.props", "./"]
COPY ["samples/SampleWebConsumer/SampleWebConsumer.csproj", "samples/SampleWebConsumer/"]
RUN dotnet restore "samples/SampleWebConsumer/SampleWebConsumer.csproj"
COPY . .
WORKDIR "/src/samples/SampleWebConsumer"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SampleWebConsumer.dll"]