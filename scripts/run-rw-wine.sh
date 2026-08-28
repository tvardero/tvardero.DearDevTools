#!/usr/bin/env bash
# Launches Rain World directly through Proton 9.0's own wine64 binary,
# bypassing Steam's Proton launcher script (which swallows the game's
# stdout/stderr via reaper/pressure-vessel). This keeps the process's
# stdout/stderr attached to the current terminal so BepInEx/Unity log
# lines can be watched live, without needing to enable BepInEx's
# in-game console window (which under wine opens as a separate,
# non-attached window anyway).
#
# Requires the existing Steam Proton 9.0 (Beta) install and a Rain World
# compatdata prefix that Steam has already created (i.e. the game has
# been launched via Steam at least once).
#
# Usage:
#   ./scripts/run-rw-wine.sh [rainWorldPath] [-- game args...]
#
# rainWorldPath resolution order (mirrors build.cake's --rainWorldPath):
#   1. First positional argument
#   2. RAINWORLD_PATH environment variable
#   3. RAINWORLD_PATH key in .env.local (repo root)
#   4. RAINWORLD_PATH key in .env (repo root)

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

read_env_file() {
    local key="$1"
    local file="$2"
    [[ -f "$file" ]] || return 0
    while IFS='=' read -r k v || [[ -n "$k" ]]; do
        k="${k#$'\xef\xbb\xbf'}"  # strip UTF-8 BOM if present on the first line
        [[ -z "$k" || "$k" == \#* ]] && continue
        k="$(echo -n "$k" | xargs)"
        if [[ "$k" == "$key" ]]; then
            v="${v%%$'\r'}"
            v="$(echo -n "$v" | xargs)"
            v="${v%\"}"; v="${v#\"}"
            v="${v%\'}"; v="${v#\'}"
            echo -n "$v"
            return 0
        fi
    done < "$file"
}

RAIN_WORLD_PATH=""
if [[ $# -gt 0 && "$1" != -* ]]; then
    RAIN_WORLD_PATH="$1"
    shift
fi

if [[ -z "$RAIN_WORLD_PATH" ]]; then
    RAIN_WORLD_PATH="${RAINWORLD_PATH:-}"
fi

if [[ -z "$RAIN_WORLD_PATH" ]]; then
    RAIN_WORLD_PATH="$(read_env_file RAINWORLD_PATH "$REPO_ROOT/.env.local")"
fi

if [[ -z "$RAIN_WORLD_PATH" ]]; then
    RAIN_WORLD_PATH="$(read_env_file RAINWORLD_PATH "$REPO_ROOT/.env")"
fi

if [[ -z "$RAIN_WORLD_PATH" ]]; then
    echo "error: Rain World installation path is required. Specify it as the first argument, " \
         "the RAINWORLD_PATH environment variable, or via .env/.env.local in the repo root." >&2
    exit 1
fi

if [[ ! -f "$RAIN_WORLD_PATH/RainWorld.exe" ]]; then
    echo "error: RainWorld.exe not found under: $RAIN_WORLD_PATH" >&2
    exit 1
fi

STEAM_ROOT="$HOME/.local/share/Steam"
PROTON_DIR="$STEAM_ROOT/steamapps/common/Proton 9.0 (Beta)"
PREFIX_DIR="$STEAM_ROOT/steamapps/compatdata/312520/pfx"
WINE64="$PROTON_DIR/files/bin/wine64"

if [[ ! -x "$WINE64" ]]; then
    echo "error: Proton 9.0 (Beta) wine64 binary not found at: $WINE64" >&2
    echo "       (expected Steam library at: $STEAM_ROOT)" >&2
    exit 1
fi

if [[ ! -d "$PREFIX_DIR" ]]; then
    echo "error: Rain World's Proton prefix not found at: $PREFIX_DIR" >&2
    echo "       Launch Rain World via Steam/Proton 9.0 at least once first, so the prefix exists." >&2
    exit 1
fi

export WINEPREFIX="$PREFIX_DIR"
export LD_LIBRARY_PATH="$PROTON_DIR/files/lib:$PROTON_DIR/files/lib64${LD_LIBRARY_PATH:+:$LD_LIBRARY_PATH}"
export WINEDLLPATH="$PROTON_DIR/files/lib64/wine:$PROTON_DIR/files/lib/wine"
export WINEESYNC=1
export WINEDEBUG="${WINEDEBUG:--all}"
export STEAM_COMPAT_CLIENT_INSTALL_PATH="$STEAM_ROOT"

cd "$RAIN_WORLD_PATH"

echo "Launching RainWorld.exe via Proton 9.0's wine64 (stdout attached)..." >&2
echo "  RainWorld:    $RAIN_WORLD_PATH" >&2
echo "  WINEPREFIX:   $WINEPREFIX" >&2

exec "$WINE64" RainWorld.exe "$@"
