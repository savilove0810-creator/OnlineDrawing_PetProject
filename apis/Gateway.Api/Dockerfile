FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY apis/Gateway.Api/Gateway.Api.csproj apis/Gateway.Api/
COPY apis/BuildingBlocks/Drawing.Shared.Web/Drawing.Shared.Web.csproj apis/BuildingBlocks/Drawing.Shared.Web/
RUN dotnet restore apis/Gateway.Api/Gateway.Api.csproj

COPY apis/Gateway.Api/ apis/Gateway.Api/
COPY apis/BuildingBlocks/Drawing.Shared.Web/ apis/BuildingBlocks/Drawing.Shared.Web/

WORKDIR /src/apis/Gateway.Api
RUN dotnet publish Gateway.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Gateway.Api.dll"]
