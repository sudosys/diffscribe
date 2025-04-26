#!/bin/sh

if [ -f "$(pwd)/dsc" ]; then
    SRC="$(pwd)"
elif [ -f "$(dirname "$(pwd)")/dsc" ]; then
    SRC="$(dirname "$(pwd)")"
else
    echo "Error: dsc not found in the current or parent directory."
    exit 1
fi

DEST="/usr/local/DiffScribe"

sudo mkdir -p "$DEST"
sudo cp -r "$SRC"/* "$DEST/"

PROFILE="$HOME/.profile" # update the profile file name depending on your shell
if ! grep -qs "$DEST" "$PROFILE"; then
    printf "\nexport PATH=\"\$PATH:$DEST\"" >> "$PROFILE"
    echo "Added $DEST to PATH. Restart your session."
fi