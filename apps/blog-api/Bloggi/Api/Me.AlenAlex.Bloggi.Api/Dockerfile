FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Api/Me.AlenAlex.Bloggi.Api/Me.AlenAlex.Bloggi.Api.csproj", "Api/Me.AlenAlex.Bloggi.Api/"]
RUN dotnet restore "Api/Me.AlenAlex.Bloggi.Api/Me.AlenAlex.Bloggi.Api.csproj"
COPY . .
WORKDIR "/src/Api/Me.AlenAlex.Bloggi.Api"
RUN dotnet build "./Me.AlenAlex.Bloggi.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Me.AlenAlex.Bloggi.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Me.AlenAlex.Bloggi.Api.dll"]
