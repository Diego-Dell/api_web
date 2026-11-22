# 1. Compila la API con el SDK de .NET 10.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ApiWeb.csproj ./
RUN dotnet restore ApiWeb.csproj
COPY . .
RUN dotnet publish ApiWeb.csproj -c Release -o /app /p:UseAppHost=false

# 2. Imagen liviana que escucha en el puerto que asigna Render (PORT).
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["sh", "-c", "dotnet ApiWeb.dll --urls http://0.0.0.0:${PORT:-8080}"]
