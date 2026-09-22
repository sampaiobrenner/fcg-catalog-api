FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY global.json nuget.config Directory.Build.props Directory.Build.targets Directory.Packages.props ./
COPY src/Fcg.Catalog.Domain/Fcg.Catalog.Domain.csproj src/Fcg.Catalog.Domain/
COPY src/Fcg.Catalog.Application/Fcg.Catalog.Application.csproj src/Fcg.Catalog.Application/
COPY src/Fcg.Catalog.Infrastructure/Fcg.Catalog.Infrastructure.csproj src/Fcg.Catalog.Infrastructure/
COPY src/Fcg.Catalog.WebApi/Fcg.Catalog.WebApi.csproj src/Fcg.Catalog.WebApi/
RUN dotnet restore src/Fcg.Catalog.WebApi/Fcg.Catalog.WebApi.csproj

COPY src/ src/
RUN dotnet publish src/Fcg.Catalog.WebApi/Fcg.Catalog.WebApi.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "Fcg.Catalog.WebApi.dll"]
