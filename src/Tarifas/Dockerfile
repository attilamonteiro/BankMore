FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY src/Shared/BankMore.Shared/ Shared/BankMore.Shared/
COPY src/Tarifas/BankMore.Tarifas.Domain/ Tarifas/BankMore.Tarifas.Domain/
COPY src/Tarifas/BankMore.Tarifas.Infrastructure/ Tarifas/BankMore.Tarifas.Infrastructure/
COPY src/Tarifas/BankMore.Tarifas.Worker/ Tarifas/BankMore.Tarifas.Worker/
COPY nuget.config .

RUN dotnet restore Tarifas/BankMore.Tarifas.Worker/BankMore.Tarifas.Worker.csproj

RUN dotnet publish Tarifas/BankMore.Tarifas.Worker/BankMore.Tarifas.Worker.csproj \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app

RUN mkdir -p /data && chown $APP_UID /data
USER $APP_UID

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "BankMore.Tarifas.Worker.dll"]
