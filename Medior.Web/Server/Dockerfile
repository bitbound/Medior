#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Medior.Web/Server/Medior.Web.Server.csproj", "Medior.Web/Server/"]
COPY ["Medior.Web/Client/Medior.Web.Client.csproj", "Medior.Web/Client/"]
COPY ["Medior.Web/Shared/Medior.Web.Shared.csproj", "Medior.Web/Shared/"]
RUN dotnet restore "Medior.Web/Server/Medior.Web.Server.csproj"
COPY . .
WORKDIR "/src/Medior.Web/Server"
RUN dotnet build "Medior.Web.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Medior.Web.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Medior.Web.Server.dll"]