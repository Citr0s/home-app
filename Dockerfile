FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /web-api/app
EXPOSE 82

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /web-api/src
COPY ["/api/home-box-landing/HomeBoxLanding.Api/HomeBoxLanding.Api.csproj", "api/"]
COPY ["/api/home-box-landing/HomeBoxLanding.Api.Tests/HomeBoxLanding.Api.Tests.csproj", "api/"]
RUN dotnet restore "api/HomeBoxLanding.Api.csproj"
RUN dotnet restore "api/HomeBoxLanding.Api.Tests.csproj"
WORKDIR "/web-api/src/api"
COPY . .
RUN dotnet build "HomeBoxLanding.Api.csproj" -c Release -o /web-api/app/build

FROM build AS publish
RUN dotnet publish "HomeBoxLanding.Api.csproj" -c Release -o /web-api/app/publish

FROM base AS final
WORKDIR /web-api/app
COPY --from=publish /web-api/app/publish .

COPY /dist/home-box-landing /web-api/app/wwwroot

CMD ["dotnet", "/web-api/app/HomeBoxLanding.Api.dll"]