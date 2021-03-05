#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

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
COPY ["ZTR.M7Config.Web/ZTR.M7Config.Web.csproj", "ZTR.M7Config.Web/"]
COPY ["Framework/Framework.Service/Framework.Service.csproj", "Framework/Framework.Service/"]
COPY ["Framework/Framework.Security/Framework.Security.csproj", "Framework/Framework.Security/"]
COPY ["Framework/Framework.Business/Framework.Business.csproj", "Framework/Framework.Business/"]
COPY ["Framework/Framework.Configuration/Framework.Configuration.csproj", "Framework/Framework.Configuration/"]
COPY ["ZTR.M7Config.Business/ZTR.M7Config.Business.csproj", "ZTR.M7Config.Business/"]
COPY ["ZTR.M7Config.Business.Parsers/ZTR.M7Config.Business.Parsers.csproj", "ZTR.M7Config.Business.Parsers/"]
COPY ["ZTR.M7Config.Business.GitRepository/ZTR.M7Config.Business.GitRepository.csproj", "ZTR.M7Config.Business.GitRepository/"]
COPY ["ZTR.M7Config.Business.Common/ZTR.M7Config.Business.Common.csproj", "ZTR.M7Config.Business.Common/"]
RUN dotnet restore "ZTR.M7Config.Web/ZTR.M7Config.Web.csproj"
COPY . .
WORKDIR "/src/ZTR.M7Config.Web"
RUN dotnet build "ZTR.M7Config.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ZTR.M7Config.Web.csproj" -c Release -o /app/publish

FROM base AS final
ENV ASPNETCORE_ENVIRONMENT "Production"

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Service.dll"]