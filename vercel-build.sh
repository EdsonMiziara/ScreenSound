#!/usr/bin/env bash
# Build do front (Blazor WebAssembly) no Vercel.
# A imagem de build do Vercel não tem o .NET, então o SDK 9 é instalado aqui.
# O resultado fica em out/wwwroot (outputDirectory no vercel.json).
set -euo pipefail

export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=1
# A imagem de build pode não ter a biblioteca ICU; o build não depende de cultura
export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

DOTNET_DIR="$HOME/.dotnet"
if [ ! -x "$DOTNET_DIR/dotnet" ]; then
    curl -sSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
    bash /tmp/dotnet-install.sh --channel 9.0 --install-dir "$DOTNET_DIR"
fi

"$DOTNET_DIR/dotnet" publish ScreenSound.Web/ScreenSound.Web.csproj -c Release -o out
