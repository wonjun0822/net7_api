FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /source

COPY api/*.csproj .
RUN dotnet restore -r linux-musl-x64 /p:PublishReadyToRun=true

COPY api/. .
RUN dotnet publish -c Release -o /app -r linux-musl-x64 --self-contained true --no-restore /p:PublishTrimmed=true /p:PublishReadyToRun=true /p:PublishSingleFile=true

FROM mcr.microsoft.com/dotnet/runtime-deps:7.0-alpine-amd64
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["./net7_api"]
