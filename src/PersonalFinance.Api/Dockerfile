FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

# Trust any corporate CA certs dropped in docker-certs/ (e.g. for TLS-inspecting proxies). No-op if empty.
COPY docker-certs/ /usr/local/share/ca-certificates/
RUN update-ca-certificates

COPY Directory.Build.props NuGet.config ./
COPY src/PersonalFinance.Domain/PersonalFinance.Domain.csproj src/PersonalFinance.Domain/
COPY src/PersonalFinance.Application/PersonalFinance.Application.csproj src/PersonalFinance.Application/
COPY src/PersonalFinance.Infrastructure/PersonalFinance.Infrastructure.csproj src/PersonalFinance.Infrastructure/
COPY src/PersonalFinance.Api/PersonalFinance.Api.csproj src/PersonalFinance.Api/
RUN dotnet restore src/PersonalFinance.Api/PersonalFinance.Api.csproj

COPY src/ src/
RUN dotnet publish src/PersonalFinance.Api/PersonalFinance.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app ./

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "PersonalFinance.Api.dll"]