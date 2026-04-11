FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["core/TaskManager.Api/TaskManager.Api.csproj", "core/TaskManager.Api/"]
COPY ["core/TaskManager.Application/TaskManager.Application.csproj", "core/TaskManager.Application/"]
COPY ["core/TaskManager.Core/TaskManager.Core.csproj", "core/TaskManager.Core/"]
COPY ["core/TaskManager.Infrastructure/TaskManager.Infrastructure.csproj", "core/TaskManager.Infrastructure/"]
RUN dotnet restore "core/TaskManager.Api/TaskManager.Api.csproj"

COPY . .
WORKDIR "/src/core/TaskManager.Api"
RUN dotnet build "TaskManager.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "TaskManager.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskManager.Api.dll"]
