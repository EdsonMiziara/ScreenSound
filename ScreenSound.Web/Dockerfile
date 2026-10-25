# Imagem do front (Blazor WebAssembly): compila com o SDK e serve os arquivos estáticos com nginx,
# que também faz proxy de /api/ para o container da API (mesma origem, sem CORS).

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ScreenSound.Web/ScreenSound.Web.csproj", "ScreenSound.Web/"]
COPY ["ScreenSound.Shared.Modelos/ScreenSound.Shared.Modelos.csproj", "ScreenSound.Shared.Modelos/"]
RUN dotnet restore "ScreenSound.Web/ScreenSound.Web.csproj"
COPY ScreenSound.Web/ ScreenSound.Web/
COPY ScreenSound.Shared.Modelos/ ScreenSound.Shared.Modelos/
RUN dotnet publish "ScreenSound.Web/ScreenSound.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish --no-restore

FROM nginx:1.27-alpine AS final
COPY ScreenSound.Web/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
EXPOSE 80
