FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY src/Shared/BankMore.Shared/ Shared/BankMore.Shared/
COPY src/Transferencia/BankMore.Transferencia.Domain/ Transferencia/BankMore.Transferencia.Domain/
COPY src/Transferencia/BankMore.Transferencia.Application/ Transferencia/BankMore.Transferencia.Application/
COPY src/Transferencia/BankMore.Transferencia.Infrastructure/ Transferencia/BankMore.Transferencia.Infrastructure/
COPY src/Transferencia/BankMore.Transferencia.Api/ Transferencia/BankMore.Transferencia.Api/
COPY nuget.config .

RUN dotnet restore Transferencia/BankMore.Transferencia.Api/BankMore.Transferencia.Api.csproj

RUN dotnet publish Transferencia/BankMore.Transferencia.Api/BankMore.Transferencia.Api.csproj \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/* \
    && mkdir -p /data && chown $APP_UID /data
USER $APP_UID

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

HEALTHCHECK --interval=15s --timeout=5s --retries=3 \
    CMD curl -sf http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "BankMore.Transferencia.Api.dll"]
