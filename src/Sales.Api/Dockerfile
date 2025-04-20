FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8090

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Sales.Api/Sales.Api.csproj", "src/Sales.Api/"]
COPY ["src/Sales.Application/Sales.Application.csproj", "src/Sales.Application/"]
COPY ["src/Sales.Domain/Sales.Domain.csproj", "src/Sales.Domain/"]
COPY ["src/Sales.Infrastructure/Sales.Infrastructure.csproj", "src/Sales.Infrastructure/"]
RUN dotnet restore "src/Sales.Api/Sales.Api.csproj"
COPY . .
WORKDIR "/src/src/Sales.Api"
RUN dotnet build "./Sales.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Sales.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Sales.Api.dll"]
