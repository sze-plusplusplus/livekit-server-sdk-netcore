FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build

WORKDIR /src
COPY *.sln .
COPY Livekit.Client/*.csproj ./Livekit.Client/
COPY Livekit.Client.Example/*.csproj ./Livekit.Client.Example/

RUN dotnet restore

COPY Livekit.Client/. ./Livekit.Client/
COPY Livekit.Client.Example/. ./Livekit.Client.Example/

WORKDIR "/src/Livekit.Client.Example"

RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine

WORKDIR /app
EXPOSE 5000

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Livekit.Client.Example.dll"]