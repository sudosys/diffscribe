#!/bin/sh

SRC="$(pwd)"
DEST="/usr/local/DiffScribe"

sudo mkdir -p "$DEST"
sudo cp -r "$SRC"/* "$DEST/"

PROFILE="$HOME/.profile" # update the profile file name depending on your shell
if ! grep -qs "$DEST" "$PROFILE"; then
    echo "export PATH=\"\$PATH:$DEST\"" >> "$PROFILE"
    echo "Added $DEST to PATH. Restart your session."
fi