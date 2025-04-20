FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8090

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/SalesApi/SalesApi.csproj", "src/SalesApi/"]
COPY ["src/SalesApi.Application/SalesApi.Application.csproj", "src/SalesApi.Application/"]
COPY ["src/SalesApi.Domain/SalesApi.Domain.csproj", "src/SalesApi.Domain/"]
COPY ["src/SalesApi.Infrastructure/SalesApi.Infrastructure.csproj", "src/SalesApi.Infrastructure/"]
RUN dotnet restore "src/SalesApi/SalesApi.csproj"
COPY . .
WORKDIR "/src/src/SalesApi"
RUN dotnet build "./SalesApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SalesApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SalesApi.dll"]
