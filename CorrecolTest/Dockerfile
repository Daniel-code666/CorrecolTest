FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
USER $APP_UID
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Directory.Build.props ./
COPY CorrecolTest/CorrecolTest.csproj CorrecolTest/
COPY CorrecolTest.Domain/CorrecolTest.Domain.csproj CorrecolTest.Domain/
COPY CorrecolTest.Application/CorrecolTest.Application.csproj CorrecolTest.Application/
COPY CorrecolTest.Infrastructure/CorrecolTest.Infrastructure.csproj CorrecolTest.Infrastructure/
RUN dotnet restore CorrecolTest/CorrecolTest.csproj
COPY CorrecolTest/ CorrecolTest/
COPY CorrecolTest.Domain/ CorrecolTest.Domain/
COPY CorrecolTest.Application/ CorrecolTest.Application/
COPY CorrecolTest.Infrastructure/ CorrecolTest.Infrastructure/
RUN dotnet publish CorrecolTest/CorrecolTest.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM base AS final
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CorrecolTest.dll"]
