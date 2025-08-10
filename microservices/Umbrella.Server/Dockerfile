FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
EXPOSE 8080
COPY ./bin/publish /app
WORKDIR /app 
ENTRYPOINT ["/app/Umbrella.Server"]