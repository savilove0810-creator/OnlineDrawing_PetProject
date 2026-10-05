FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY apis/Auth.Api/Auth.Api.csproj apis/Auth.Api/
COPY apis/BuildingBlocks/Drawing.Shared.Web/Drawing.Shared.Web.csproj apis/BuildingBlocks/Drawing.Shared.Web/
RUN dotnet restore apis/Auth.Api/Auth.Api.csproj

COPY apis/Auth.Api/ apis/Auth.Api/
COPY apis/BuildingBlocks/Drawing.Shared.Web/ apis/BuildingBlocks/Drawing.Shared.Web/

WORKDIR /src/apis/Auth.Api
RUN dotnet publish Auth.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Auth.Api.dll"]
