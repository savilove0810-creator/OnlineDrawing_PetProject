FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY apis/Rooms.Api/Rooms.Api.csproj apis/Rooms.Api/
COPY apis/BuildingBlocks/Drawing.Shared.Web/Drawing.Shared.Web.csproj apis/BuildingBlocks/Drawing.Shared.Web/
RUN dotnet restore apis/Rooms.Api/Rooms.Api.csproj

COPY apis/Rooms.Api/ apis/Rooms.Api/
COPY apis/BuildingBlocks/Drawing.Shared.Web/ apis/BuildingBlocks/Drawing.Shared.Web/

WORKDIR /src/apis/Rooms.Api
RUN dotnet publish Rooms.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Rooms.Api.dll"]
