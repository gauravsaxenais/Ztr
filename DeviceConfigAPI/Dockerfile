#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Service.csproj", ""]
COPY ["../Business/Business.csproj", "../Business/"]
COPY ["../Business.Parsers/Business.Parsers.csproj", "../Business.Parsers/"]
COPY ["../Framework/Framework.Service/Framework.Service.csproj", "../Framework/Framework.Service/"]
COPY ["../Framework/Framework.Security/Framework.Security.csproj", "../Framework/Framework.Security/"]
COPY ["../Framework/Framework.Business/Framework.Business.csproj", "../Framework/Framework.Business/"]
COPY ["../Framework/Framework.Configuration/Framework.Configuration.csproj", "../Framework/Framework.Configuration/"]
RUN dotnet restore "./Service.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Service.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Service.dll"]