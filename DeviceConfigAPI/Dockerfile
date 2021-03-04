FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
# Update and install required dependencies
RUN apt-get update && apt-get install -y protobuf-compiler

#add user instead of running from root.
RUN adduser \
  --disabled-password \
  --home /app \
  --gecos '' app \
  && chown -R app /app
USER app
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
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
ENV ASPNETCORE_ENVIRONMENT "Production"
    
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Service.dll"]