FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore
RUN dotnet build -c release -o /bin/ --no-restore

RUN dotnet publish -c release -o /app/

FROM base as final

WORKDIR /app

COPY --from=build /app .

ENTRYPOINT [ "dotnet", "ImageProcessor.dll" ]