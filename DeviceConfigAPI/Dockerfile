FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base

# Update and install required dependencies
RUN apt-get update && apt-get install -y git autoconf libtool make automake unzip build-essential

WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["DeviceConfigAPI/Service.csproj", "DeviceConfigAPI/"]
COPY ["Framework/Framework.Service/Framework.Service.csproj", "Framework/Framework.Service/"]
COPY ["Framework/Framework.Security/Framework.Security.csproj", "Framework/Framework.Security/"]
COPY ["Framework/Framework.Business/Framework.Business.csproj", "Framework/Framework.Business/"]
COPY ["Framework/Framework.Configuration/Framework.Configuration.csproj", "Framework/Framework.Configuration/"]
COPY ["Business.GitRepository/Business.GitRepository.csproj", "Business.GitRepository/"]
COPY ["Business.Common/Business.Common.csproj", "Business.Common/"]
COPY ["Business/Business.csproj", "Business/"]
COPY ["Business.Parsers/Business.Parsers.csproj", "Business.Parsers/"]
RUN dotnet restore "DeviceConfigAPI/Service.csproj"
COPY . .
WORKDIR "/src/DeviceConfigAPI"
RUN dotnet build "Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Service.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
RUN mkdir Temp
RUN chmod -R 777 /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Service.dll"]