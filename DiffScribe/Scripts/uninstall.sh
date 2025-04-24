#!/bin/sh

DEST="/usr/local/DiffScribe"
PROFILE="$HOME/.profile" # update the profile file name depending on your shell

sudo rm -rf "$DEST"

if grep -qs "$DEST" "$PROFILE"; then
    sed -i.bak "/$DEST/d" "$PROFILE"
    echo "$DEST removed from PATH. Restart your shell session."
fi