#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/aspnet:8.0.18 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0.412 AS build
ARG TARGETARCH
ARG APP_VERSION="6.13.0"
WORKDIR /src
COPY ["Trident.Web/Trident.Web.csproj", "Trident.Web/"]
RUN dotnet restore "Trident.Web/Trident.Web.csproj"
COPY . .
WORKDIR "/src/Trident.Web"
RUN dotnet build "Trident.Web.csproj" -c Release -o /app/build /p:Version=$APP_VERSION -a $TARGETARCH

FROM build AS publish
RUN dotnet publish "Trident.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false /p:Version=$APP_VERSION -a $TARGETARCH

FROM base AS final
WORKDIR /app
ENV TRIDENT_INSTANCE_URL=blah
ENV TRIDENT_INSTANCE_API_KEY=blah
ENV TRIDENT_SUMOLOGIC_URL=blah
ENV TRIDENT_ENVIRONMENT="Production"
ENV TRIDENT_OPEN_FEATURE_CLIENT_ID="blah"
ENV TRIDENT_CONNECTION_STRING=blah
ENV ASPNETCORE_URLS="http://+:8080"
ENV ASPNETCORE_ENVIRONMENT="Production"
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Trident.Web.dll"]