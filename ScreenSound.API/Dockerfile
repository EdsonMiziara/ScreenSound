# Imagem da API (ASP.NET Core). Dentro do container a API escuta só HTTP na porta 8080;
# HTTPS fica a cargo de quem estiver na frente (nginx do front, Azure, proxy reverso...).

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
# Restaura primeiro só com os .csproj para aproveitar o cache de camadas
COPY ["ScreenSound.API/ScreenSound.API.csproj", "ScreenSound.API/"]
COPY ["ScreenSound.Shared.Data/ScreenSound.Shared.Data.csproj", "ScreenSound.Shared.Data/"]
COPY ["ScreenSound.Shared.Modelos/ScreenSound.Shared.Modelos.csproj", "ScreenSound.Shared.Modelos/"]
RUN dotnet restore "ScreenSound.API/ScreenSound.API.csproj"
COPY ScreenSound.API/ ScreenSound.API/
COPY ScreenSound.Shared.Data/ ScreenSound.Shared.Data/
COPY ScreenSound.Shared.Modelos/ ScreenSound.Shared.Modelos/
RUN dotnet publish "ScreenSound.API/ScreenSound.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish --no-restore /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
# Pasta das fotos enviadas (artistas e perfis) com permissão para o usuário sem privilégios da imagem
RUN mkdir -p /app/wwwroot/FotosPerfil /home/app/.aspnet/DataProtection-Keys && chown -R $APP_UID /app/wwwroot /home/app/.aspnet
USER $APP_UID
ENTRYPOINT ["dotnet", "ScreenSound.API.dll"]
