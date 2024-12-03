FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Apache.NMS.RestAPI/Apache.NMS.RestAPI.csproj", "Apache.NMS.RestAPI/"]
COPY ["Apache.NMS.RestAPI.Interfaces/Apache.NMS.RestAPI.Interfaces.csproj", "Apache.NMS.RestAPI.Interfaces/"]
COPY ["Apache.NMS.RestAPI.Logic/Apache.NMS.RestAPI.Logic.csproj", "Apache.NMS.RestAPI.Logic/"]
RUN dotnet restore "Apache.NMS.RestAPI/Apache.NMS.RestAPI.csproj"
COPY . .
WORKDIR "/src/Apache.NMS.RestAPI"
RUN dotnet build "Apache.NMS.RestAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Apache.NMS.RestAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Apache.NMS.RestAPI.dll"]
